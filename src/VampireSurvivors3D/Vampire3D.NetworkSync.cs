#pragma warning disable CS8618

using Events;

using System.Text.Json;

using TriggerEncapsulation.Messaging;
using TriggerEncapsulation.Event;

using System.Text.Json.Serialization;

#if CLIENT
using GameUI.TriggerEvent;
#endif

namespace GameEntry.VampireSurvivors3D;

/// <summary>
/// 网络同步的游戏状态数据结构
/// </summary>
public struct NetworkGameState
{
    public int Wave { get; set; }
    public int EnemiesKilled { get; set; }
    public long GameTimeMs { get; set; } // TimeSpan 序列化为毫秒
    public int Level { get; set; }
    public float Experience { get; set; }
    public float ExperienceRequired { get; set; }
    public int PlayerId { get; set; }
}

[JsonSerializable(typeof(NetworkGameState))]
public partial class NetworkGameStateJsonContext : JsonSerializerContext { }

/// <summary>
/// 网络同步的升级数据
/// </summary>
public struct NetworkLevelUpData
{
    public int PlayerId { get; set; }
    public int NewLevel { get; set; }
    public float Experience { get; set; }
}

[JsonSerializable(typeof(NetworkLevelUpData))]
public partial class NetworkLevelUpDataJsonContext : JsonSerializerContext { }

/// <summary>
/// 🆕 网络同步的升级选择数据
/// </summary>
public struct NetworkUpgradeSelectionData
{
    public int PlayerId { get; set; }
    public int UpgradeIndex { get; set; }
    public string UpgradeType { get; set; }
}

[JsonSerializable(typeof(NetworkUpgradeSelectionData))]
public partial class NetworkUpgradeSelectionDataJsonContext : JsonSerializerContext { }

/// <summary>
/// 消息类型标识
/// </summary>
public enum NetworkMessageType : byte
{
    GameStateUpdate = 1,
    PlayerLevelUp = 2,
    UpgradeSelection = 3
}

#if SERVER
/// <summary>
/// 服务器端网络同步器 - 真正通过网络发送游戏状态
/// </summary>
public static class NetworkServerSync
{
    private static GameCore.Timers.Timer? syncTimer;

    public static void Initialize()
    {
        // 创建状态同步定时器 - 每500ms同步一次
        syncTimer = new GameCore.Timers.Timer(TimeSpan.FromMilliseconds(500))
        {
            AutoReset = true
        };
        
        syncTimer.Elapsed += (_, __) => BroadcastGameStateOverNetwork();
        syncTimer.Start();

        Game.Logger.LogInformation("📡 Network Server Sync initialized - Broadcasting every 500ms");
    }

    private static void BroadcastGameStateOverNetwork()
    {
        try
        {
            var stats = GameplaySystem.GetCurrentStats();
            
            // 创建网络状态数据
            var networkState = new NetworkGameState
            {
                Wave = stats.Wave,
                EnemiesKilled = stats.EnemiesKilled,
                GameTimeMs = (long)stats.GameTime.TotalMilliseconds,
                Level = stats.Level,
                Experience = stats.Experience,
                ExperienceRequired = stats.ExperienceRequired,
                PlayerId = 1
            };

            // 序列化并发送
            SendNetworkMessage(NetworkMessageType.GameStateUpdate, networkState);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error broadcasting game state over network");
        }
    }

    public static void BroadcastLevelUpOverNetwork(int playerId, int newLevel, float experience)
    {
        var levelUpData = new NetworkLevelUpData
        {
            PlayerId = playerId,
            NewLevel = newLevel,
            Experience = experience
        };

        SendNetworkMessage(NetworkMessageType.PlayerLevelUp, levelUpData);
        Game.Logger.LogInformation("📡 Network broadcast level up: Player {player} -> Level {level}", playerId, newLevel);
    }

    private static void SendNetworkMessage<T>(NetworkMessageType messageType, T data) where T : struct
    {
        try
        {
            // 创建消息头 (1字节消息类型 + JSON数据)
            var jsonData = JsonSerializer.Serialize(data);
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
            
            // 组合消息：[MessageType(1字节)] + [JSON数据]
            var messageBytes = new byte[1 + jsonBytes.Length];
            messageBytes[0] = (byte)messageType;
            Array.Copy(jsonBytes, 0, messageBytes, 1, jsonBytes.Length);

            // 使用 ProtoCustomMessage 发送
            var customMessage = new ProtoCustomMessage { Message = messageBytes };
            customMessage.Broadcast(); // 广播给所有客户端

            Game.Logger.LogDebug("📤 Sent {type} message ({size} bytes)", messageType, messageBytes.Length);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Failed to send network message {type}", messageType);
        }
    }

    public static void Shutdown()
    {
        syncTimer?.Stop();
        syncTimer?.Dispose();
        syncTimer = null;
        Game.Logger.LogInformation("📡 Network Server Sync shutdown");
    }
}
#endif

#if CLIENT
/// <summary>
/// 客户端网络同步接收器 - 接收并处理来自服务器的状态更新
/// </summary>
public class NetworkClientSync : IGameClass
{
    private static NetworkClientSync? instance;
    private static readonly object lockObject = new();

    // 客户端游戏状态
    private static int currentWave = 1;
    private static int enemiesKilled = 0;
    private static TimeSpan gameTime = TimeSpan.Zero;
    private static int playerLevel = 1;
    private static float playerExperience = 0f;
    private static float experienceRequired = 100f;

    public static NetworkClientSync Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance ??= new NetworkClientSync();
                }
            }
            return instance;
        }
    }

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        if (Game.GameModeLink != ScopeData.GameMode.VampireSurvivors3D)
        {
            return;
        }

        // 注册网络消息监听 - 监听来自服务器的自定义消息
        RegisterNetworkMessageListener();
        
        Game.Logger.LogInformation("📡 Network Client Sync initialized");
    }

    private static void RegisterNetworkMessageListener()
    {
        // 监听 EventServerMessage - 这是 ProtoCustomMessage 在客户端触发的事件
        Trigger<EventServerMessage> networkMessageTrigger = new(OnNetworkMessageReceived, keepReference: true);
        networkMessageTrigger.Register(Game.Instance);

        Game.Logger.LogInformation("📡 Network message listener registered");
    }

    private static async Task<bool> OnNetworkMessageReceived(object sender, EventServerMessage eventArgs)
    {
        try
        {
            var messageBytes = eventArgs.Message;
            if (messageBytes.Length < 1)
            {
                Game.Logger.LogWarning("📥 Received empty network message");
                return false;
            }

            // 解析消息类型
            var messageType = (NetworkMessageType)messageBytes[0];
            var jsonBytes = new byte[messageBytes.Length - 1];
            Array.Copy(messageBytes, 1, jsonBytes, 0, jsonBytes.Length);
            var jsonData = System.Text.Encoding.UTF8.GetString(jsonBytes);
            Game.Logger.LogInformation("Received Message: {messageType} - {msg}", messageType, jsonData);
            // 根据消息类型处理
            switch (messageType)
            {
                case NetworkMessageType.GameStateUpdate:
                    var gameState = JsonSerializer.Deserialize(jsonData, NetworkGameStateJsonContext.Default.NetworkGameState);
                    await HandleGameStateUpdate(gameState);
                    break;

                case NetworkMessageType.PlayerLevelUp:
                    var levelUpData = JsonSerializer.Deserialize(jsonData, NetworkLevelUpDataJsonContext.Default.NetworkLevelUpData);
                    await HandlePlayerLevelUp(levelUpData);
                    break;

                default:
                    Game.Logger.LogWarning("📥 Unknown network message type: {type}", messageType);
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error processing network message");
            return false;
        }
    }

    private static async Task HandleGameStateUpdate(NetworkGameState networkState)
    {
        // 更新客户端状态
        currentWave = networkState.Wave;
        enemiesKilled = networkState.EnemiesKilled;
        gameTime = TimeSpan.FromMilliseconds(networkState.GameTimeMs);
        playerLevel = networkState.Level;
        playerExperience = networkState.Experience;
        experienceRequired = networkState.ExperienceRequired;

        Game.Logger.LogDebug("📊 Client state updated via network: Wave {wave}, Kills {kills}, Level {level}", 
            currentWave, enemiesKilled, playerLevel);

        await Task.CompletedTask;
    }

    private static async Task HandlePlayerLevelUp(NetworkLevelUpData levelUpData)
    {
        playerLevel = levelUpData.NewLevel;
        playerExperience = levelUpData.Experience;

        Game.Logger.LogInformation("🎊 Network: Player leveled up to level {level}!", playerLevel);

        // 显示升级界面
        EnhancedUI.ShowUpgradeSelection();

        await Task.CompletedTask;
    }

    /// <summary>
    /// 🆕 发送升级选择到服务器
    /// </summary>
    public static async Task SendUpgradeSelection(int upgradeIndex, string upgradeType)
    {
        try
        {
            var localPlayer = Player.LocalPlayer;
            if (localPlayer == null)
            {
                Game.Logger.LogWarning("❌ Cannot send upgrade selection: No local player");
                return;
            }

            var upgradeData = new NetworkUpgradeSelectionData
            {
                PlayerId = localPlayer.Id,
                UpgradeIndex = upgradeIndex,
                UpgradeType = upgradeType
            };

            // 序列化消息
            var jsonData = JsonSerializer.Serialize(upgradeData, NetworkUpgradeSelectionDataJsonContext.Default.NetworkUpgradeSelectionData);
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
            
            // 创建完整消息（类型 + 数据）
            var messageBytes = new byte[jsonBytes.Length + 1];
            messageBytes[0] = (byte)NetworkMessageType.UpgradeSelection;
            Array.Copy(jsonBytes, 0, messageBytes, 1, jsonBytes.Length);

            // 发送网络消息
            var message = new ProtoCustomMessage { Message = messageBytes };
            message.SendToServer();

            Game.Logger.LogInformation("📤 Upgrade selection sent: Index={index}, Type={type}", upgradeIndex, upgradeType);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Failed to send upgrade selection");
        }
    }

    /// <summary>
    /// 获取当前客户端游戏状态
    /// </summary>
    public static GameStateData GetCurrentState()
    {
        return new GameStateData
        {
            Wave = currentWave,
            EnemiesKilled = enemiesKilled,
            GameTime = gameTime,
            Level = playerLevel,
            Experience = playerExperience,
            ExperienceRequired = experienceRequired
        };
    }

    /// <summary>
    /// 客户端游戏状态数据结构
    /// </summary>
    public struct GameStateData
    {
        public int Wave { get; set; }
        public int EnemiesKilled { get; set; }
        public TimeSpan GameTime { get; set; }
        public int Level { get; set; }
        public float Experience { get; set; }
        public float ExperienceRequired { get; set; }
    }
}
#endif

#pragma warning restore CS8618 