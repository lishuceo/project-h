#if SERVER
using Events;

using GameCore.Event;

using System.Text.Json;

using TriggerEncapsulation.Event;
using TriggerEncapsulation.Messaging;

using static System.Net.Mime.MediaTypeNames;

namespace GameEntry.TypedMessageTest;

/// <summary>
/// TypedMessage测试游戏模式 - 服务器端
/// </summary>
public static class TypedMessageTestServer
{
    private static readonly Dictionary<string, DateTime> _clientConnections = new();
    private static readonly Dictionary<Guid, LatencyTestMessage> _latencyTests = new();
    private static readonly Random _random = new();
    
    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        // 只在TypedMessageTest游戏模式下运行
        if (Game.GameModeLink != ScopeData.GameMode.TypedMessageTest)
        {
            return;
        }

        Game.Logger.LogInformation("🖥️ Initializing TypedMessage Test Server...");
        
        // 注册服务器端消息处理器
        RegisterServerMessageHandlers();
        
        // 注册玩家连接/断开事件
        RegisterPlayerEvents();
        
        // 启动服务器端定期测试
        _ = StartServerPeriodicTests();
        
        Game.Logger.LogInformation("✅ TypedMessage Test Server initialized");
    }

    /// <summary>
    /// 注册服务器端消息处理器
    /// </summary>
    private static void RegisterServerMessageHandlers()
    {
        // 客户端状态消息处理
        TypedMessageHandler.Register<ClientStateMessage>(OnClientStateReceived, 
            MessagePriority.Normal, "ServerClientStateHandler");
        
        // 互动测试消息处理
        TypedMessageHandler.Register<InteractiveTestMessage>(OnInteractiveTestReceived,
            MessagePriority.High, "ServerInteractiveTestHandler");
        
        // 延迟测试消息处理
        TypedMessageHandler.Register<LatencyTestMessage>(OnLatencyTestReceived,
            MessagePriority.Critical, "ServerLatencyTestHandler");
        
        // 序列化测试消息处理
        TypedMessageHandler.Register<SerializationTestMessage>(OnSerializationTestReceived,
            MessagePriority.Normal, "ServerSerializationTestHandler");
        
        // 网络测试消息处理
        TypedMessageHandler.Register<NetworkTestMessage>(OnNetworkTestReceived,
            MessagePriority.Normal, "ServerNetworkTestHandler");

        Game.Logger.LogDebug("🎯 Server message handlers registered");
    }

    /// <summary>
    /// 注册玩家事件
    /// </summary>
    private static void RegisterPlayerEvents()
    {
        // 监听玩家连接事件
        var playerConnectedTrigger = new Trigger<EventPlayerUserConnected>(OnPlayerConnected, keepReference: true);
        playerConnectedTrigger.Register(Game.Instance);
        
        // 监听玩家断开事件
        var playerDisconnectedTrigger = new Trigger<EventPlayerUserDisconnected>(OnPlayerDisconnected, keepReference: true);
        playerDisconnectedTrigger.Register(Game.Instance);
        
        Game.Logger.LogDebug("👥 Player event handlers registered");
    }

    /// <summary>
    /// 启动服务器端定期测试
    /// </summary>
    private static async Task StartServerPeriodicTests()
    {
        await Game.Delay(TimeSpan.FromSeconds(5)); // 等待客户端连接
        
        Game.Logger.LogInformation("🔄 Starting server periodic tests...");
        
        while (true)
        {
            try
            {
                // 每30秒执行一次定期测试
                await RunPeriodicTests();
                await Game.Delay(TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                Game.Logger.LogError(ex, "❌ Server periodic test failed");
                await Game.Delay(TimeSpan.FromSeconds(10)); // 出错后短暂等待
            }
        }
    }

    /// <summary>
    /// 运行定期测试
    /// </summary>
    private static async Task RunPeriodicTests()
    {
        var onlinePlayers = Player.AllPlayers.ToList(); // 简化实现，获取所有玩家
        if (onlinePlayers.Count == 0)
        {
            Game.Logger.LogDebug("⏳ No players for periodic tests");
            return;
        }

        Game.Logger.LogInformation("🔍 Running server periodic tests with {PlayerCount} players", onlinePlayers.Count);

        // 测试1: 服务器广播消息
        await TestServerBroadcast(onlinePlayers);
        
        // 测试2: 延迟测试
        await TestLatencyMeasurement(onlinePlayers);
        
        // 测试3: 状态同步测试
        await TestStateSynchronization(onlinePlayers);
        
        // 测试4: 批量消息测试
        await TestBatchMessaging(onlinePlayers);
        
        Game.Logger.LogInformation("✅ Server periodic tests completed");
    }

    /// <summary>
    /// 测试服务器广播
    /// </summary>
    private static async Task TestServerBroadcast(List<Player> players)
    {
        var broadcastMessage = new ServerBroadcastMessage
        {
            BroadcastType = "PeriodicTest",
            Message = $"Server broadcast at {DateTime.UtcNow:HH:mm:ss}",
            Data = new Dictionary<string, object>
            {
                { "PlayerCount", players.Count },
                { "ServerUptime", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                { "TestId", Guid.NewGuid().ToString() }
            },
            TargetPlayers = players.Select(p => p.Id.ToString()).ToArray()
        };

        await MessageBuilder.Create(broadcastMessage)
            .WithPriority(MessagePriority.Normal)
            .ToOnlinePlayers()
            .SendAsync();

        Game.Logger.LogDebug("📡 Server broadcast sent to {PlayerCount} players", players.Count);
    }

    /// <summary>
    /// 测试延迟测量
    /// </summary>
    private static async Task TestLatencyMeasurement(List<Player> players)
    {
        foreach (var player in players.Take(3)) // 限制测试玩家数量
        {
            var latencyTest = new LatencyTestMessage
            {
                Direction = "ServerToClient",
                ServerSentTime = DateTime.UtcNow
            };

            _latencyTests[latencyTest.TestId] = latencyTest;

            await MessageBuilder.Create(latencyTest)
                .WithPriority(MessagePriority.Critical)
                .ToPlayer(player)
                .SendAsync();

            // 设置超时清理
            _ = Game.Delay(TimeSpan.FromSeconds(10)).ContinueWith(t =>
            {
                _latencyTests.Remove(latencyTest.TestId);
            });
        }

        Game.Logger.LogDebug("⏱️ Latency tests initiated for {PlayerCount} players", Math.Min(players.Count, 3));
    }

    /// <summary>
    /// 测试状态同步
    /// </summary>
    private static async Task TestStateSynchronization(List<Player> players)
    {
        var stateMessage = new ClientStateMessage
        {
            ClientId = "Server",
            State = "GameRunning",
            StateData = new Dictionary<string, object>
            {
                { "ConnectedPlayers", players.Count },
                { "ServerTime", DateTime.UtcNow },
                { "GameMode", "TypedMessageTest" },
                { "TestsRunning", true }
            }
        };

        _ = await MessageBuilder.Create(stateMessage)
            .WithPriority(MessagePriority.Normal)
            .ToOnlinePlayers()
            .SendAsync();

        Game.Logger.LogDebug("🔄 State synchronization sent");
    }

    /// <summary>
    /// 测试批量消息
    /// </summary>
    private static async Task TestBatchMessaging(List<Player> players)
    {
        const int batchSize = 5;
        var batchId = _random.Next(1000, 9999);
        var batchStartTime = DateTime.UtcNow;

        for (int i = 0; i < batchSize; i++)
        {
            var batchMessage = new BatchTestMessage
            {
                BatchId = batchId,
                MessageIndex = i,
                BatchSize = batchSize,
                BatchType = "ServerPeriodicBatch",
                Data = GenerateTestData(100 + i * 50), // 递增数据大小
                BatchStartTime = batchStartTime
            };

            await MessageBuilder.Create(batchMessage)
                .WithPriority(MessagePriority.Low)
                .ToOnlinePlayers()
                .SendAsync();

            await Game.Delay(TimeSpan.FromMilliseconds(100)); // 间隔发送
        }

        Game.Logger.LogDebug("📦 Batch messaging completed: {BatchId} with {BatchSize} messages", batchId, batchSize);
    }

    /// <summary>
    /// 生成测试数据
    /// </summary>
    private static byte[] GenerateTestData(int size)
    {
        var data = new byte[size];
        _random.NextBytes(data);
        return data;
    }

    // ===== 事件处理器 =====

    private static async Task<bool> OnPlayerConnected(object sender, EventPlayerUserConnected eventArgs)
    {
        var player = eventArgs.Player;
        var user = eventArgs.User;
        var clientId = user.UserId.ToString();
        
        _clientConnections[clientId] = DateTime.UtcNow;
        
        Game.Logger.LogInformation("👤 Player connected: {PlayerId} (User: {UserId})", player.Id, user.UserId);
        
        // 向新连接的玩家发送欢迎消息
        var welcomeMessage = new ServerBroadcastMessage
        {
            BroadcastType = "Welcome",
            Message = $"Welcome to TypedMessage Test Mode, Player {player.Id}!",
            Data = new Dictionary<string, object>
            {
                { "PlayerId", player.Id },
                { "UserId", user.UserId },
                { "ConnectedAt", DateTime.UtcNow },
                { "TestModeVersion", "1.0" }
            }
        };

        await MessageBuilder.Create(welcomeMessage)
            .WithPriority(MessagePriority.High)
            .ToPlayer(player)
            .SendAsync();
        
        return true;
    }

    private static async Task<bool> OnPlayerDisconnected(object sender, EventPlayerUserDisconnected eventArgs)
    {
        var player = eventArgs.Player;
        var user = eventArgs.User;
        var clientId = user.UserId.ToString();
        
        if (_clientConnections.TryGetValue(clientId, out var connectTime))
        {
            var sessionDuration = DateTime.UtcNow - connectTime;
            Game.Logger.LogInformation("👋 Player disconnected: {PlayerId} (User: {UserId}), Session: {Duration:F1}s", 
                player.Id, user.UserId, sessionDuration.TotalSeconds);
            
            _clientConnections.Remove(clientId);
        }
        
        return await Task.FromResult(true);
    }

    // ===== 消息处理器 =====

    private static async Task<bool> OnClientStateReceived(Player? sender, ClientStateMessage message)
    {
        Game.Logger.LogInformation("📊 Client state from {PlayerId}: {State}", 
            sender?.Id.ToString() ?? "Unknown", message.State);
        
        // 回复客户端状态确认
        if (sender != null)
        {
            var ackMessage = new InteractiveTestMessage
            {
                Command = "StateAcknowledged",
                Parameters = new Dictionary<string, object>
                {
                    { "ReceivedState", message.State },
                    { "ServerTime", DateTime.UtcNow },
                    { "PlayerId", sender.Id }
                },
                ResponseRequired = "None"
            };

            await MessageBuilder.Create(ackMessage)
                .WithPriority(MessagePriority.Normal)
                .ToPlayer(sender)
                .SendAsync();
        }
        
        return true;
    }

    private static async Task<bool> OnInteractiveTestReceived(Player? sender, InteractiveTestMessage message)
    {
        Game.Logger.LogInformation("🎮 Interactive command from {PlayerId}: {Command}", 
            sender?.Id.ToString() ?? "Unknown", message.Command);
        
        if (sender == null) return false;

        // 处理不同的交互命令
        switch (message.Command.ToLowerInvariant())
        {
            case "ping":
                await HandlePingCommand(sender, message);
                break;
                
            case "echo":
                await HandleEchoCommand(sender, message);
                break;
                
            case "stress":
                await HandleStressCommand(sender, message);
                break;
                
            default:
                await HandleUnknownCommand(sender, message);
                break;
        }
        
        return true;
    }

    private static async Task HandlePingCommand(Player sender, InteractiveTestMessage message)
    {
        var pongMessage = new InteractiveTestMessage
        {
            Command = "Pong",
            Parameters = new Dictionary<string, object>
            {
                { "OriginalConversationId", message.ConversationId },
                { "ServerTime", DateTime.UtcNow },
                { "PlayerId", sender.Id }
            },
            ConversationId = message.ConversationId
        };

        await MessageBuilder.Create(pongMessage)
            .WithPriority(MessagePriority.High)
            .ToPlayer(sender)
            .SendAsync();
    }

    private static async Task HandleEchoCommand(Player sender, InteractiveTestMessage message)
    {
        var echoMessage = new InteractiveTestMessage
        {
            Command = "EchoResponse",
            Parameters = message.Parameters, // 回显原始参数
            ConversationId = message.ConversationId
        };

        await MessageBuilder.Create(echoMessage)
            .WithPriority(MessagePriority.Normal)
            .ToPlayer(sender)
            .SendAsync();
    }

    private static async Task HandleStressCommand(Player sender, InteractiveTestMessage message)
    {
        // 解析压力测试参数
        int messageCount;
        if (message.Parameters.TryGetValue("MessageCount", out var countObj))
        {
            try
            {
                if (countObj is JsonElement element)
                {
                    _ = element.TryGetInt32(out messageCount);
                }
                else
                {
                    // 尝试转换为整数
                    messageCount = Convert.ToInt32(countObj);
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogWarning("Failed to convert MessageCount: {Value}({type}) ({Exception})", countObj, countObj.GetType(), ex);
                messageCount = 10;
            }
        }
        else
        {
            messageCount = 10;
        }

        int interval;
        if (message.Parameters.TryGetValue("IntervalMs", out var intervalObj))
        {
            try
            {
                if (intervalObj is JsonElement element)
                {
                    _ = element.TryGetInt32(out interval);
                }
                else
                {
                    interval = Convert.ToInt32(intervalObj);
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogWarning("Failed to convert IntervalMs: {Value}({type}) ({Exception})", intervalObj, intervalObj.GetType(), ex);
                interval = 100;
            }
        }
        else
        {
            interval = 100;
        }

        messageCount = Math.Min(messageCount, 100); // 限制最大数量
        interval = Math.Max(interval, 10); // 限制最小间隔

        Game.Logger.LogInformation("💪 Starting stress test for Player {PlayerId}: {Count} messages, {Interval}ms interval",
            sender.Id, messageCount, interval);

        for (int i = 0; i < messageCount; i++)
        {
            var stressMessage = new StressTestMessage
            {
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                MessageNumber = i + 1
            };

            await MessageBuilder.Create(stressMessage)
                .WithPriority(MessagePriority.Low)
                .ToPlayer(sender)
                .SendAsync();

            if (i < messageCount - 1) // 最后一条消息不需要等待
            {
                await Game.Delay(TimeSpan.FromMilliseconds(interval));
            }
        }

        // 发送完成通知
        var completionMessage = new InteractiveTestMessage
        {
            Command = "StressTestCompleted",
            Parameters = new Dictionary<string, object>
            {
                { "MessagesSent", messageCount },
                { "ConversationId", message.ConversationId }
            }
        };

        await MessageBuilder.Create(completionMessage)
            .WithPriority(MessagePriority.High)
            .ToPlayer(sender)
            .SendAsync();
    }

    private static async Task HandleUnknownCommand(Player sender, InteractiveTestMessage message)
    {
        var errorMessage = new InteractiveTestMessage
        {
            Command = "Error",
            Parameters = new Dictionary<string, object>
            {
                { "Error", "UnknownCommand" },
                { "OriginalCommand", message.Command },
                { "AvailableCommands", new[] { "ping", "echo", "stress" } }
            },
            ConversationId = message.ConversationId
        };

        await MessageBuilder.Create(errorMessage)
            .WithPriority(MessagePriority.Normal)
            .ToPlayer(sender)
            .SendAsync();
    }

    private static async Task<bool> OnLatencyTestReceived(Player? sender, LatencyTestMessage message)
    {
        if (sender == null) return false;

        message.ServerReceivedTime = DateTime.UtcNow;

        if (message.Direction == "ClientToServer")
        {
            // 客户端发送的延迟测试，服务器回应
            var responseMessage = new LatencyTestMessage
            {
                TestId = message.TestId,
                Direction = "ServerToClient",
                ClientSentTime = message.ClientSentTime,
                ServerReceivedTime = message.ServerReceivedTime,
                ServerSentTime = DateTime.UtcNow
            };

            await MessageBuilder.Create(responseMessage)
                .WithPriority(MessagePriority.Critical)
                .ToPlayer(sender)
                .SendAsync();
        }
        else
        {
            // 服务器发送的延迟测试的回应
            if (_latencyTests.TryGetValue(message.TestId, out var originalTest))
            {
                var roundTripTime = DateTime.UtcNow - originalTest.ServerSentTime;
                Game.Logger.LogInformation("⏱️ Latency test completed for Player {PlayerId}: RTT={RTT:F2}ms",
                    sender.Id, roundTripTime.TotalMilliseconds);
                
                _latencyTests.Remove(message.TestId);
            }
        }

        return true;
    }

    private static async Task<bool> OnSerializationTestReceived(Player? sender, SerializationTestMessage message)
    {
        Game.Logger.LogInformation("🔧 Serialization test from Player {PlayerId}: {Data}",
            sender?.Id.ToString() ?? "Unknown", message.ToString());
        
        // 验证序列化的各种数据类型
        var isValid = ValidateSerializationData(message);
        
        if (sender != null)
        {
            var responseMessage = new InteractiveTestMessage
            {
                Command = "SerializationTestResult",
                Parameters = new Dictionary<string, object>
                {
                    { "IsValid", isValid },
                    { "ReceivedData", message.ToString() },
                    { "ValidationTime", DateTime.UtcNow }
                }
            };

            await MessageBuilder.Create(responseMessage)
                .WithPriority(MessagePriority.Normal)
                .ToPlayer(sender)
                .SendAsync();
        }
        
        return true;
    }

    private static async Task<bool> OnNetworkTestReceived(Player? sender, NetworkTestMessage message)
    {
        Game.Logger.LogInformation("🌐 Network test from Player {PlayerId}: {Condition}, Size={Size}B",
            sender?.Id.ToString() ?? "Unknown", message.NetworkCondition, message.PacketSize);
        
        if (sender == null) return false;

        // 模拟网络条件
        if (message.SimulateLatency && message.ExpectedDelay > TimeSpan.Zero)
        {
            await Game.Delay(message.ExpectedDelay);
        }
        
        // 响应网络测试
        var responseMessage = new NetworkTestMessage
        {
            NetworkCondition = "ServerResponse",
            PacketSize = message.PacketSize,
            SentTime = DateTime.UtcNow
        };

        await MessageBuilder.Create(responseMessage)
            .WithPriority(MessagePriority.Normal)
            .ToPlayer(sender)
            .SendAsync();
        
        return true;
    }

    /// <summary>
    /// 验证序列化数据的完整性
    /// </summary>
    private static bool ValidateSerializationData(SerializationTestMessage message)
    {
        try
        {
            // 检查各种数据类型是否正确序列化/反序列化
            return message.BoolValue &&
                   message.IntValue > 0 &&
                   message.LongValue > 0 &&
                   message.FloatValue > 0 &&
                   message.DoubleValue > 0 &&
                   !string.IsNullOrEmpty(message.StringValue) &&
                   message.DateTimeValue != default &&
                   message.GuidValue != Guid.Empty &&
                   message.IntArray?.Length > 0 &&
                   message.StringList?.Count > 0 &&
                   message.StringIntDict?.Count > 0 &&
                   message.NestedData != null;
        }
        catch
        {
            return false;
        }
    }
} 
#endif