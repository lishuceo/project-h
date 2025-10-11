// #if SERVER
// using Events;
// using EngineInterface.BaseType;
// using GameCore.BaseInterface;
// using GameCore.Event;
// using GameCore.PlayerAndUsers;
// using Microsoft.Extensions.Logging;
// using TriggerEncapsulation.Event;
// using TriggerEncapsulation.Messaging;
// using System.Text;
// using System.Text.Json;
// using GameCore.SceneSystem;
// using GameCore;
// using System.Numerics;

// namespace GameEntry;

// /// <summary>
// /// 服务器游戏逻辑类
// /// 负责处理游戏核心逻辑、玩家管理和状态同步
// /// </summary>
// public class GameServer : IGameClass
// {
//     // ==================== 事件触发器 ====================
//     private static Trigger<EventGameStart>? gameStartTrigger;
//     private static Trigger<EventPlayerUserConnected>? playerConnectedTrigger;
//     private static Trigger<EventPlayerUserDisconnected>? playerDisconnectedTrigger;

//     // ==================== 游戏状态 ====================
//     private static readonly HashSet<int> ConnectedPlayers = new();

//     /// <summary>
//     /// 框架初始化时自动调用此方法注册游戏类
//     /// </summary>
//     public static void OnRegisterGameClass()
//     {
//         Game.OnGameTriggerInitialization += RegisterTriggers;
//     }

//     /// <summary>
//     /// 注册所有服务器事件触发器
//     /// </summary>
//     private static void RegisterTriggers()
//     {
//         // 游戏开始事件
//         gameStartTrigger = new(static async (s, d) =>
//         {
//             Game.Logger.LogInformation("服务器游戏开始！");
//             Game.Logger.LogInformation("数编测试项目服务器！");

//             return true;
//         });
//         gameStartTrigger.Register(Game.Instance);

//         // 玩家连接事件
//         playerConnectedTrigger = new(static async (s, d) =>
//         {
//             var player = d.Player;
//             ConnectedPlayers.Add(player.Id);

//             Game.Logger.LogInformation("玩家 {PlayerId} 已连接，当前在线玩家数: {PlayerCount}",
//                 player.Id, ConnectedPlayers.Count);

//             // 为玩家创建单位
//             await CreatePlayerUnit(player);
//             return true;
//         });
//         playerConnectedTrigger.Register(Game.Instance);

//         // 玩家断线事件
//         playerDisconnectedTrigger = new(static async (s, d) =>
//         {
//             var player = d.Player;
//             ConnectedPlayers.Remove(player.Id);

//             Game.Logger.LogInformation("玩家 {PlayerId} 已断线，当前在线玩家数: {PlayerCount}",
//                 player.Id, ConnectedPlayers.Count);

//             return true;
//         });
//         playerDisconnectedTrigger.Register(Game.Instance);


//     }

//     /// <summary>
//     /// 处理玩家单位创建（由框架自动处理）
//     /// </summary>
//     private static async Task CreatePlayerUnit(Player player)
//     {
//         try
//         {
//             var scene = Scene.GetOrCreate(new GameLink<GameDataScene, GameDataScene>("p_0tja.ScopeData.GameDataScene.TestScene.Root"u8))!;
//             _ = player.SwitchScene(scene, true);
//             Game.Logger.LogInformation("玩家 {PlayerId} 已切换到场景 '{SceneName}'", player.Id, scene.Name);
//             player.MainUnit = new Unit(new GameLink<GameDataUnit, GameDataUnit>("p_0tja.ScopeData.GameDataUnit.TestUnit.Root"u8), player, new ScenePoint(new Vector3(0, 0, 0), scene), Angle.Zero);
//             Game.Logger.LogInformation("玩家 {PlayerId} 创建单位成功", player.MainUnit);
//         }
//         catch (Exception ex)
//         {
//             Game.Logger.LogError(ex, "处理玩家 {PlayerId} 单位创建时发生错误", player.Id);
//         }
//     }
// }

// #endif
#if SERVER
using Events;
using EngineInterface.BaseType;
using GameCore.BaseInterface;
using GameCore.Event;
using GameCore.PlayerAndUsers;
using Microsoft.Extensions.Logging;
using TriggerEncapsulation.Event;
using TriggerEncapsulation.Messaging;
using System.Text;
using System.Text.Json;
using GameCore.SceneSystem;
using GameCore;
using System.Numerics;
using GameCore.SceneSystem.Data;
using GameCore.EntitySystem.Data;
using GameCore.AISystem;
using GameData;
using p_0tja.ScopeData;

namespace GameEntry;

/// <summary>
/// 服务器游戏逻辑类
/// 负责处理游戏核心逻辑、玩家管理和状态同步
/// </summary>
public class GameServer : IGameClass
{
    // ==================== 事件触发器 ====================
    private static Trigger<EventGameStart>? gameStartTrigger;
    private static Trigger<EventPlayerUserConnected>? playerConnectedTrigger;
    private static Trigger<EventPlayerUserDisconnected>? playerDisconnectedTrigger;
    private static Trigger<EventClientMessage>? clientMessageTrigger;

    // ==================== 游戏状态 ====================
    private static readonly HashSet<int> ConnectedPlayers = new();
    
    // 动态创建的怪物列表
    private static readonly List<Unit> dynamicMonsters = new();

    /// <summary>
    /// 框架初始化时自动调用此方法注册游戏类
    /// </summary>
    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += RegisterTriggers;
    }

    /// <summary>
    /// 注册所有服务器事件触发器
    /// </summary>
    private static void RegisterTriggers()
    {
        // 只有在JsonScopeDataTest游戏模式下才注册触发器
        if (Game.GameModeLink != ScopeData.GameMode.JsonScopeDataTest)
        {
            Game.Logger.LogInformation("当前游戏模式不是JsonScopeDataTest，跳过服务器逻辑初始化");
            return;
        }

        Game.Logger.LogInformation("✅ 游戏模式匹配JsonScopeDataTest，开始注册服务器触发器");

        // 游戏开始事件
        gameStartTrigger = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("服务器游戏开始！");

            // 初始化服务器状态
            InitializeServerState();

            // 加载默认场景
            await LoadDefaultScene();

            // 创建怪物
            await CreateDynamicMonsters();

            // 向所有玩家广播游戏开始消息
            await BroadcastMessage("Welcome", "欢迎来到游戏世界！怪物已出现！🐺");

            return true;
        });
        gameStartTrigger.Register(Game.Instance);

        // 玩家连接事件
        playerConnectedTrigger = new(static async (s, d) =>
        {
            var player = d.Player;
            ConnectedPlayers.Add(player.Id);

            Game.Logger.LogInformation("玩家 {PlayerId} 已连接，当前在线玩家数: {PlayerCount}",
                player.Id, ConnectedPlayers.Count);

            // 为玩家创建单位
            await CreatePlayerUnit(player);

            // 向新玩家发送欢迎消息
            await SendMessageToPlayer(player, "Welcome", $"欢迎玩家 {player.Id}！");

            // 向其他玩家广播新玩家加入消息
            await BroadcastMessage("Notification", $"玩家 {player.Id} 加入了游戏",
                playerFilter: p => p.Id != player.Id);

            return true;
        });
        playerConnectedTrigger.Register(Game.Instance);

        // 玩家断线事件
        playerDisconnectedTrigger = new(static async (s, d) =>
        {
            var player = d.Player;
            ConnectedPlayers.Remove(player.Id);

            Game.Logger.LogInformation("玩家 {PlayerId} 已断线，当前在线玩家数: {PlayerCount}",
                player.Id, ConnectedPlayers.Count);

            // 向其他玩家广播玩家离开消息
            await BroadcastMessage("Notification", $"玩家 {player.Id} 离开了游戏");

            return true;
        });
        playerDisconnectedTrigger.Register(Game.Instance);

        // 客户端消息处理
        clientMessageTrigger = new(OnClientMessageReceived);
        clientMessageTrigger.Register(Game.Instance);
    }

    /// <summary>
    /// 初始化服务器状态
    /// </summary>
    private static void InitializeServerState()
    {
        ConnectedPlayers.Clear();
        Game.Logger.LogInformation("服务器状态初始化完成");

        // 这里可以添加其他服务器初始化逻辑
        // 例如：设置游戏世界状态、AI系统等
    }

    /// <summary>
    /// 处理来自客户端的消息
    /// </summary>
    private static async Task<bool> OnClientMessageReceived(object sender, EventClientMessage eventArgs)
    {
        var player = eventArgs.Player;

        try
        {
            var json = Encoding.UTF8.GetString(eventArgs.Message);
            var messageData = JsonSerializer.Deserialize<ClientMessage>(json);

            if (messageData?.Type == null) return false;

            Game.Logger.LogInformation("收到玩家 {PlayerId} 的消息: {MessageType}",
                player.Id, messageData.Type);

            // 根据消息类型处理不同的客户端消息
            switch (messageData.Type)
            {
                case "ClientReady":
                    await HandleClientReady(player, messageData);
                    break;

                case "PlayerAction":
                    await HandlePlayerAction(player, messageData);
                    break;

                case "Chat":
                    await HandleChatMessage(player, messageData);
                    break;

                default:
                    Game.Logger.LogWarning("未知的客户端消息类型: {MessageType}", messageData.Type);
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "处理玩家 {PlayerId} 的消息时发生错误", player.Id);
            return false;
        }
    }

    /// <summary>
    /// 处理客户端准备就绪消息
    /// </summary>
    private static async Task HandleClientReady(Player player, ClientMessage message)
    {
        Game.Logger.LogInformation("玩家 {PlayerId} 已准备就绪", player.Id);

        // 向客户端确认收到准备消息
        await SendMessageToPlayer(player, "GameState", "服务器已确认你的准备状态");
    }

    /// <summary>
    /// 处理玩家行动消息
    /// </summary>
    private static async Task HandlePlayerAction(Player player, ClientMessage message)
    {
        Game.Logger.LogInformation("处理玩家 {PlayerId} 的行动", player.Id);

        // 这里可以添加具体的玩家行动处理逻辑
        // 例如：移动、攻击、使用技能等

        // 向其他玩家同步这个行动
        await BroadcastMessage("PlayerAction", $"玩家 {player.Id} 执行了一个行动",
            playerFilter: p => p.Id != player.Id);
    }

    /// <summary>
    /// 处理聊天消息
    /// </summary>
    private static async Task HandleChatMessage(Player player, ClientMessage message)
    {
        var chatContent = message.Data?.ToString() ?? "";
        Game.Logger.LogInformation("玩家 {PlayerId} 发送聊天消息: {Content}", player.Id, chatContent);

        // 广播聊天消息给所有玩家
        await BroadcastMessage("Chat", $"玩家{player.Id}: {chatContent}");
    }

    /// <summary>
    /// 向指定玩家发送消息
    /// </summary>
    private static Task SendMessageToPlayer(Player player, string messageType, string content)
    {
        try
        {
            var serverMessage = new ServerMessage
            {
                Type = messageType,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(serverMessage);
            var message = new ProtoCustomMessage
            {
                Message = Encoding.UTF8.GetBytes(json)
            };

            message.SendTo(player);
            Game.Logger.LogInformation("向玩家 {PlayerId} 发送消息: {MessageType}", player.Id, messageType);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "向玩家 {PlayerId} 发送消息时发生错误", player.Id);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 广播消息给所有或符合条件的玩家
    /// </summary>
    private static Task BroadcastMessage(string messageType, string content,
        Func<Player, bool>? playerFilter = null)
    {
        try
        {
            var serverMessage = new ServerMessage
            {
                Type = messageType,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(serverMessage);
            var message = new ProtoCustomMessage
            {
                Message = Encoding.UTF8.GetBytes(json)
            };

            if (playerFilter != null)
            {
                message.Broadcast(playerFilter);
            }
            else
            {
                message.Broadcast();
            }

            Game.Logger.LogInformation("广播消息: {MessageType} - {Content}", messageType, content);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "广播消息时发生错误");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 创建动态怪物（模仿ARPG模板）
    /// </summary>
    private static async Task CreateDynamicMonsters()
    {
        try
        {
            Game.Logger.LogInformation("🐺 开始创建JsonScopeDataTest模式的怪物...");

            // 获取当前场景 - 使用正确的预定义链接
            var scene = Scene.GetOrCreate(p_0tja.ScopeData.GameDataScene.TestScene.Root);
            if (scene == null)
            {
                Game.Logger.LogError("❌ 无法获取TestScene，跳过怪物创建");
                return;
            }

            // 获取敌对玩家（使用Player 4作为怪物的拥有者）
            var enemyPlayer = Player.GetById(4);
            if (enemyPlayer == null)
            {
                Game.Logger.LogWarning("⚠️ 未找到玩家4，使用默认玩家创建怪物");
                var allPlayers = Player.AllPlayers.ToList();
                enemyPlayer = allPlayers.Count > 1 ? allPlayers[1] : allPlayers.FirstOrDefault();
            }

            if (enemyPlayer == null)
            {
                Game.Logger.LogError("❌ 无法找到合适的玩家来创建怪物");
                return;
            }

            Game.Logger.LogInformation("👥 使用玩家 {PlayerId} 作为怪物拥有者", enemyPlayer.Id);

            // 创建狼人怪物
            await CreateWerewolves(scene, enemyPlayer);

            Game.Logger.LogInformation("✅ 怪物创建完成，总共创建了 {Count} 只怪物", dynamicMonsters.Count);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 创建怪物时发生错误");
        }
    }

    /// <summary>
    /// 创建狼人怪物
    /// </summary>
    private static async Task CreateWerewolves(Scene scene, Player ownerPlayer)
    {
        Game.Logger.LogInformation("🐺 创建狼人怪物...");

        // 狼人位置配置 - 在地图中放置几只狼人
        var werewolfPositions = new Vector3[]
        {
            new(1000, 1000, 0), // 位置1
            new(1500, 1500, 0), // 位置2
            new(500, 1500, 0),  // 位置3
        };

        for (int i = 0; i < werewolfPositions.Length; i++)
        {
            var position = werewolfPositions[i];

            try
            {
                // 创建狼人单位 - 使用JsonScopeDataTest中的WereWolfUnit
                var werewolf = p_0tja.ScopeData.GameDataUnit.WereWolfUnit.Root.Data?.CreateUnit(
                    ownerPlayer,
                    new ScenePoint(position, scene),
                    Angle.Zero
                );

                if (werewolf != null)
                {
                    dynamicMonsters.Add(werewolf);

                    // 🤖 添加AI - 为狼人添加默认AI
                    var aiThinkTree = AIThinkTree.AddDefaultAI(werewolf);
                    if (aiThinkTree != null)
                    {
                        Game.Logger.LogInformation("🧠 狼人 {Index} AI配置成功: {UnitName} at {Position}",
                            i + 1, werewolf.Cache.Name, position);
                    }
                    else
                    {
                        Game.Logger.LogWarning("⚠️ 狼人 {Index} AI配置失败", i + 1);
                    }

                    await Game.Delay(TimeSpan.FromMilliseconds(100)); // 稍微延迟避免同时创建
                }
                else
                {
                    Game.Logger.LogError("❌ 无法创建狼人单位 {Index}，GameLink数据为空", i + 1);
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogError(ex, "❌ 创建狼人单位 {Index} 时发生异常", i + 1);
            }
        }
    }

    /// <summary>
    /// 加载默认场景并设置玩家
    /// </summary>
    private static async Task LoadDefaultScene()
    {
        try
        {
            // 获取或创建默认场景 - 使用正确的预定义链接
            var scene = Scene.GetOrCreate(p_0tja.ScopeData.GameDataScene.TestScene.Root)!;

            if (scene.Loaded)
            {
                Game.Logger.LogInformation("场景 '{SceneName}' 已经加载", scene.Name);
            }
            else
            {
                Game.Logger.LogInformation("正在加载场景: '{SceneName}'", scene.Name);
                _ = scene.Load();
            }

            // 确保玩家1切换到场景（用于测试）
            var player1 = Player.GetById(1);
            if (player1 != null)
            {
                _ = player1.SwitchScene(scene, true);
                Game.Logger.LogInformation("玩家 {PlayerId} 已切换到场景 '{SceneName}'", player1.Id, scene.Name);
                player1.MainUnit = p_0tja.ScopeData.GameDataUnit.TestUnit.Root.Data?.CreateUnit(player1, new ScenePoint(new Vector3(0, 0, 0), scene), Angle.Zero);
            }
            else
            {
                Game.Logger.LogWarning("未找到ID为1的玩家");
            }


            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "加载默认场景时发生错误");
        }
    }

    /// <summary>
    /// 处理玩家单位创建（由框架自动处理）
    /// </summary>
    private static async Task CreatePlayerUnit(Player player)
    {
        try
        {
            Game.Logger.LogInformation("玩家 {PlayerId} 连接，单位将由框架自动创建", player.Id);

            // 注意：单位创建由框架根据ScopeData中的PlacedPlayerObjects自动处理
            // 只有当玩家ID与PlacedPlayerObjects中的OwnerPlayerId匹配时，才会创建单位
            // 目前配置为玩家ID=1会获得英雄单位

            if (player.Id == 1)
            {
                Game.Logger.LogInformation("玩家 {PlayerId} 将获得英雄单位（地图中心位置）", player.Id);
            }
            else
            {
                Game.Logger.LogInformation("玩家 {PlayerId} 在当前配置中没有预设单位", player.Id);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "处理玩家 {PlayerId} 单位创建时发生错误", player.Id);
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public static void Cleanup()
    {
        Game.Logger.LogInformation("🧹 清理JsonScopeDataTest服务端资源...");

        try
        {
            // 清理动态创建的怪物
            CleanupDynamicMonsters();

            // 清理事件触发器
            gameStartTrigger?.Destroy();
            playerConnectedTrigger?.Destroy();
            playerDisconnectedTrigger?.Destroy();
            clientMessageTrigger?.Destroy();

            gameStartTrigger = null;
            playerConnectedTrigger = null;
            playerDisconnectedTrigger = null;
            clientMessageTrigger = null;

            ConnectedPlayers.Clear();

            Game.Logger.LogInformation("✅ JsonScopeDataTest服务端资源清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 清理JsonScopeDataTest服务端资源时出错");
        }
    }

    /// <summary>
    /// 清理动态创建的怪物
    /// </summary>
    private static void CleanupDynamicMonsters()
    {
        try
        {
            Game.Logger.LogInformation("🗑️ 清理动态创建的怪物...");

            foreach (var monster in dynamicMonsters)
            {
                try
                {
                    if (monster.IsValid)
                    {
                        monster.Destroy();
                    }
                }
                catch (Exception ex)
                {
                    Game.Logger.LogWarning("清理怪物时出错: {Error}", ex.Message);
                }
            }

            dynamicMonsters.Clear();
            Game.Logger.LogInformation("✅ 动态怪物清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 清理动态怪物时出错");
        }
    }
}

/// <summary>
/// 客户端消息数据结构（服务器端定义）
/// </summary>
public class ClientMessage
{
    public string? Type { get; set; }
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 服务器消息数据结构（服务器端定义）
/// </summary>
public class ServerMessage
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; }
}

#endif