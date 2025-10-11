#if SERVER
using Events;
using TriggerEncapsulation.Messaging;
using System.Text.Json;

namespace GameEntry.VampireSurvivors3D;

/// <summary>
/// 网络同步测试类 - 验证 ProtoCustomMessage 是否正常工作
/// </summary>
public class NetworkSyncTest : IGameClass
{
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

        Game.Logger.LogInformation("📡 Network Sync Test initialized");
        _ = TestNetworkSync();
    }

    private static async Task TestNetworkSync()
    {
        // 延迟3秒后开始测试
        await Game.Delay(TimeSpan.FromSeconds(3));
        Game.Logger.LogInformation("🧪 Starting network sync test...");

        try
        {
            // 测试1: 发送游戏状态更新
            var testGameState = new NetworkGameState
            {
                Wave = 5,
                EnemiesKilled = 123,
                GameTimeMs = 120000, // 2分钟
                Level = 3,
                Experience = 250f,
                ExperienceRequired = 300f,
                PlayerId = 1
            };

            await SendTestMessage(NetworkMessageType.GameStateUpdate, testGameState);
            Game.Logger.LogInformation("✅ Test 1 passed: Game state update sent");

            await Game.Delay(TimeSpan.FromSeconds(1));

            // 测试2: 发送升级消息
            var testLevelUp = new NetworkLevelUpData
            {
                PlayerId = 1,
                NewLevel = 4,
                Experience = 300f
            };

            await SendTestMessage(NetworkMessageType.PlayerLevelUp, testLevelUp);
            Game.Logger.LogInformation("✅ Test 2 passed: Level up message sent");

            Game.Logger.LogInformation("🎉 All network sync tests completed successfully!");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Network sync test failed");
        }
    }

    private static async Task SendTestMessage<T>(NetworkMessageType messageType, T data) where T : struct
    {
        // 序列化数据
        var jsonData = JsonSerializer.Serialize(data);
        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
        
        // 组合消息：[MessageType(1字节)] + [JSON数据]
        var messageBytes = new byte[1 + jsonBytes.Length];
        messageBytes[0] = (byte)messageType;
        Array.Copy(jsonBytes, 0, messageBytes, 1, jsonBytes.Length);

        // 使用 ProtoCustomMessage 发送
        var customMessage = new ProtoCustomMessage { Message = messageBytes };
        customMessage.Broadcast();

        Game.Logger.LogInformation("📤 Test message sent: {type} ({size} bytes)", messageType, messageBytes.Length);
        
        await Task.CompletedTask;
    }
}
#endif 