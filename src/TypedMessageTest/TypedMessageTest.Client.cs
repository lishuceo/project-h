#if CLIENT
using TriggerEncapsulation.Messaging;
using GameCore.Timers;

namespace GameEntry.TypedMessageTest;

/// <summary>
/// TypedMessage测试游戏模式 - 客户端
/// </summary>
public static class TypedMessageTestClient
{
    private static readonly Dictionary<Guid, LatencyTestMessage> _pendingLatencyTests = new();
    private static readonly Dictionary<int, List<BatchTestMessage>> _batchMessages = new();
    private static GameCore.Timers.Timer? _periodicTimer;
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

        Game.Logger.LogInformation("💻 Initializing TypedMessage Test Client...");
        
        // 注册客户端消息处理器
        RegisterClientMessageHandlers();
        
        // 启动客户端定期测试
        StartClientPeriodicTests();
        
        Game.Logger.LogInformation("✅ TypedMessage Test Client initialized");
    }

    /// <summary>
    /// 注册客户端消息处理器
    /// </summary>
    private static void RegisterClientMessageHandlers()
    {
        // 服务器广播消息处理
        TypedMessageHandler.Register<ServerBroadcastMessage>(OnServerBroadcastReceived,
            MessagePriority.Normal, "ClientBroadcastHandler");
        
        // 互动测试消息处理
        TypedMessageHandler.Register<InteractiveTestMessage>(OnInteractiveTestReceived,
            MessagePriority.High, "ClientInteractiveTestHandler");
        
        // 延迟测试消息处理
        TypedMessageHandler.Register<LatencyTestMessage>(OnLatencyTestReceived,
            MessagePriority.Critical, "ClientLatencyTestHandler");
        
        // 批量测试消息处理
        TypedMessageHandler.Register<BatchTestMessage>(OnBatchTestReceived,
            MessagePriority.Low, "ClientBatchTestHandler");
        
        // 压力测试消息处理
        TypedMessageHandler.Register<StressTestMessage>(OnStressTestReceived,
            MessagePriority.Low, "ClientStressTestHandler");
        
        // 网络测试消息处理
        TypedMessageHandler.Register<NetworkTestMessage>(OnNetworkTestReceived,
            MessagePriority.Normal, "ClientNetworkTestHandler");

        Game.Logger.LogDebug("🎯 Client message handlers registered");
    }

    /// <summary>
    /// 启动客户端定期测试
    /// </summary>
    private static void StartClientPeriodicTests()
    {
        // 延迟启动，等待与服务器建立连接
        _ = Game.Delay(TimeSpan.FromSeconds(3)).ContinueWith(async _ =>
        {
            Game.Logger.LogInformation("🚀 Starting client periodic tests...");
            
            // 启动定期测试定时器
            _periodicTimer = new GameCore.Timers.Timer(45000); // 45秒间隔
            _periodicTimer.Elapsed += async (sender, e) => await OnPeriodicTestTick();
            _periodicTimer.AutoReset = true;
            
            // 延迟10秒后开始
            await Game.Delay(TimeSpan.FromSeconds(10));
            _periodicTimer.Start();
            
            // 执行初始测试序列
            await RunInitialClientTests();
        });
    }

    /// <summary>
    /// 执行初始客户端测试
    /// </summary>
    private static async Task RunInitialClientTests()
    {
        try
        {
            Game.Logger.LogInformation("🧪 Running initial client tests...");
            
            // 发送客户端状态
            await SendClientState();
            
            // 发送序列化测试
            await SendSerializationTest();
            
            // 发送延迟测试
            await SendLatencyTest();
            
            // 发送网络测试
            await SendNetworkTest();
            
            Game.Logger.LogInformation("✅ Initial client tests completed");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Initial client tests failed");
        }
    }

    /// <summary>
    /// 定期测试定时器回调
    /// </summary>
    private static async Task OnPeriodicTestTick()
    {
        try
        {
            await RunPeriodicClientTests();
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Periodic client test failed");
        }
    }

    /// <summary>
    /// 执行定期客户端测试
    /// </summary>
    private static async Task RunPeriodicClientTests()
    {
        Game.Logger.LogInformation("🔄 Running periodic client tests...");
        
        // 随机选择要执行的测试
        var testTypes = new[]
        {
            "ping", "echo", "latency", "state", "interactive"
        };
        
        var selectedTest = testTypes[_random.Next(testTypes.Length)];
        
        switch (selectedTest)
        {
            case "ping":
                await SendPingTest();
                break;
                
            case "echo":
                await SendEchoTest();
                break;
                
            case "latency":
                await SendLatencyTest();
                break;
                
            case "state":
                await SendClientState();
                break;
                
            case "interactive":
                await SendRandomInteractiveTest();
                break;
        }
        
        Game.Logger.LogDebug("✅ Periodic client test completed: {TestType}", selectedTest);
    }

    // ===== 测试方法 =====

    /// <summary>
    /// 发送客户端状态
    /// </summary>
    private static async Task SendClientState()
    {
        var stateMessage = new ClientStateMessage
        {
            ClientId = $"Client_{Environment.MachineName}",
            State = "Active",
            StateData = new Dictionary<string, object>
            {
                { "ClientTime", DateTime.UtcNow },
                { "Platform", Environment.OSVersion.ToString() },
                { "TestMode", "TypedMessageTest" },
                { "RandomValue", _random.Next(1000, 9999) }
            }
        };

        await MessageBuilder.Create(stateMessage)
            .WithPriority(MessagePriority.Normal)
            .SendToServerAsync();
        
        Game.Logger.LogDebug("📤 Client state sent");
    }

    /// <summary>
    /// 发送序列化测试
    /// </summary>
    private static async Task SendSerializationTest()
    {
        var serializationTest = new SerializationTestMessage
        {
            BoolValue = true,
            IntValue = _random.Next(1, 1000),
            LongValue = DateTime.UtcNow.Ticks,
            FloatValue = (float)(_random.NextDouble() * 100),
            DoubleValue = _random.NextDouble() * 1000,
            StringValue = $"Client test string {DateTime.UtcNow:HH:mm:ss}",
            DateTimeValue = DateTime.UtcNow,
            GuidValue = Guid.NewGuid(),
            IntArray = Enumerable.Range(1, 5).Select(_ => _random.Next(100)).ToArray(),
            StringList = new List<string> { "Alpha", "Beta", "Gamma", DateTime.UtcNow.ToString("HH:mm:ss") },
            StringIntDict = new Dictionary<string, int>
            {
                { "Random1", _random.Next(100) },
                { "Random2", _random.Next(100) },
                { "Timestamp", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            },
            NestedData = new SerializationTestMessage.NestedTestData
            {
                Name = "ClientNestedData",
                Value = _random.Next(1000),
                CreatedAt = DateTime.UtcNow
            }
        };

        await MessageBuilder.Create(serializationTest)
            .WithPriority(MessagePriority.Normal)
            .SendToServerAsync();
        
        Game.Logger.LogDebug("📤 Serialization test sent");
    }

    /// <summary>
    /// 发送延迟测试
    /// </summary>
    private static async Task SendLatencyTest()
    {
        var latencyTest = new LatencyTestMessage
        {
            Direction = "ClientToServer",
            ClientSentTime = DateTime.UtcNow
        };

        _pendingLatencyTests[latencyTest.TestId] = latencyTest;

        await MessageBuilder.Create(latencyTest)
            .WithPriority(MessagePriority.Critical)
            .SendToServerAsync();
        
        // 设置超时清理
        _ = Game.Delay(TimeSpan.FromSeconds(15)).ContinueWith(t =>
        {
                         if (_pendingLatencyTests.ContainsKey(latencyTest.TestId))
             {
                 _pendingLatencyTests.Remove(latencyTest.TestId);
                 Game.Logger.LogWarning("⏱️ Latency test {TestId} timed out", latencyTest.TestId);
             }
        });
        
        Game.Logger.LogDebug("📤 Latency test sent: {TestId}", latencyTest.TestId);
    }

    /// <summary>
    /// 发送网络测试
    /// </summary>
    private static async Task SendNetworkTest()
    {
        var networkTest = new NetworkTestMessage
        {
            NetworkCondition = "ClientTest",
            PacketSize = _random.Next(100, 1000),
            SimulateLatency = _random.NextDouble() > 0.5,
            ExpectedDelay = TimeSpan.FromMilliseconds(_random.Next(50, 200))
        };

        await MessageBuilder.Create(networkTest)
            .WithPriority(MessagePriority.Normal)
            .SendToServerAsync();
        
        Game.Logger.LogDebug("📤 Network test sent: {Condition}, Size={Size}B", 
            networkTest.NetworkCondition, networkTest.PacketSize);
    }

    /// <summary>
    /// 发送Ping测试
    /// </summary>
    private static async Task SendPingTest()
    {
        var pingMessage = new InteractiveTestMessage
        {
            Command = "Ping",
            Parameters = new Dictionary<string, object>
            {
                { "ClientTime", DateTime.UtcNow },
                { "TestData", $"Ping_{_random.Next(1000, 9999)}" }
            },
            ResponseRequired = "Pong"
        };

        await MessageBuilder.Create(pingMessage)
            .WithPriority(MessagePriority.High)
            .SendToServerAsync();
        
        Game.Logger.LogDebug("📤 Ping test sent: {ConversationId}", pingMessage.ConversationId);
    }

    /// <summary>
    /// 发送Echo测试
    /// </summary>
    private static async Task SendEchoTest()
    {
        var echoMessage = new InteractiveTestMessage
        {
            Command = "Echo",
            Parameters = new Dictionary<string, object>
            {
                { "EchoData", $"Echo test from client at {DateTime.UtcNow:HH:mm:ss}" },
                { "RandomNumber", _random.Next(1000, 9999) },
                { "ClientId", Environment.MachineName }
            },
            ResponseRequired = "EchoResponse"
        };

        await MessageBuilder.Create(echoMessage)
            .WithPriority(MessagePriority.Normal)
            .SendToServerAsync();
        
        Game.Logger.LogDebug("📤 Echo test sent: {ConversationId}", echoMessage.ConversationId);
    }

    /// <summary>
    /// 发送随机交互测试
    /// </summary>
    private static async Task SendRandomInteractiveTest()
    {
        var commands = new[] { "ping", "echo" };
        var command = commands[_random.Next(commands.Length)];
        
        if (command == "ping")
        {
            await SendPingTest();
        }
        else
        {
            await SendEchoTest();
        }
    }

    /// <summary>
    /// 请求压力测试
    /// </summary>
    private static async Task RequestStressTest(int messageCount = 20, int intervalMs = 50)
    {
        var stressRequest = new InteractiveTestMessage
        {
            Command = "Stress",
            Parameters = new Dictionary<string, object>
            {
                { "MessageCount", messageCount },
                { "IntervalMs", intervalMs },
                { "ClientId", Environment.MachineName }
            },
            ResponseRequired = "StressTestCompleted"
        };

        await MessageBuilder.Create(stressRequest)
            .WithPriority(MessagePriority.Normal)
            .SendToServerAsync();
        
        Game.Logger.LogInformation("💪 Stress test requested: {Count} messages, {Interval}ms interval", 
            messageCount, intervalMs);
    }

    // ===== 消息处理器 =====

    private static async Task<bool> OnServerBroadcastReceived(Player? sender, ServerBroadcastMessage message)
    {
        Game.Logger.LogInformation("📡 Server broadcast received: {Type} - {Message}", 
            message.BroadcastType, message.Message);
        
        // 记录广播数据
        if (message.Data.Count > 0)
        {
            Game.Logger.LogDebug("📊 Broadcast data: {Data}", 
                string.Join(", ", message.Data.Select(kvp => $"{kvp.Key}={kvp.Value}")));
        }
        
        // 如果是欢迎消息，请求一次压力测试
        if (message.BroadcastType == "Welcome")
        {
            _ = Game.Delay(TimeSpan.FromSeconds(2)).ContinueWith(async _ =>
            {
                await RequestStressTest(15, 100);
            });
        }
        
        return await Task.FromResult(true);
    }

    private static async Task<bool> OnInteractiveTestReceived(Player? sender, InteractiveTestMessage message)
    {
        Game.Logger.LogInformation("🎮 Interactive response received: {Command}", message.Command);
        
        switch (message.Command.ToLowerInvariant())
        {
            case "pong":
                HandlePongResponse(message);
                break;
                
            case "echoresponse":
                HandleEchoResponse(message);
                break;
                
            case "stresstestcompleted":
                HandleStressTestCompleted(message);
                break;
                
            case "error":
                HandleErrorResponse(message);
                break;
                
            case "stateacknowledged":
                HandleStateAcknowledged(message);
                break;
                
            case "serializationtestresult":
                HandleSerializationTestResult(message);
                break;
                
            default:
                Game.Logger.LogDebug("❓ Unknown interactive command: {Command}", message.Command);
                break;
        }
        
        return await Task.FromResult(true);
    }

    private static void HandlePongResponse(InteractiveTestMessage message)
    {
        Game.Logger.LogInformation("🏓 Pong received for conversation: {ConversationId}", 
            message.ConversationId);
        
        if (message.Parameters.TryGetValue("ServerTime", out var serverTimeObj))
        {
            var clientTime = DateTime.UtcNow;
            if (DateTime.TryParse(serverTimeObj.ToString(), out var serverTime))
            {
                var timeDiff = clientTime - serverTime;
                Game.Logger.LogDebug("⏰ Client-Server time difference: {TimeDiff:F2}ms", 
                    timeDiff.TotalMilliseconds);
            }
        }
    }

    private static void HandleEchoResponse(InteractiveTestMessage message)
    {
        Game.Logger.LogInformation("🔄 Echo response received for conversation: {ConversationId}", 
            message.ConversationId);
        
        if (message.Parameters.TryGetValue("EchoData", out var echoData))
        {
            Game.Logger.LogDebug("📣 Echoed data: {Data}", echoData);
        }
    }

    private static void HandleStressTestCompleted(InteractiveTestMessage message)
    {
        var messagesSent = message.Parameters.TryGetValue("MessagesSent", out var countObj) 
            ? Convert.ToInt32(countObj) : 0;
        
        Game.Logger.LogInformation("💪 Stress test completed: {MessageCount} messages sent", messagesSent);
    }

    private static void HandleErrorResponse(InteractiveTestMessage message)
    {
        var error = message.Parameters.TryGetValue("Error", out var errorObj) 
            ? errorObj.ToString() : "Unknown";
        var originalCommand = message.Parameters.TryGetValue("OriginalCommand", out var cmdObj) 
            ? cmdObj.ToString() : "Unknown";
        
        Game.Logger.LogWarning("⚠️ Server error response: {Error} for command: {Command}", 
            error, originalCommand);
    }

    private static void HandleStateAcknowledged(InteractiveTestMessage message)
    {
        Game.Logger.LogInformation("✅ Client state acknowledged by server");
    }

    private static void HandleSerializationTestResult(InteractiveTestMessage message)
    {
        var isValid = message.Parameters.TryGetValue("IsValid", out var validObj) 
            && Convert.ToBoolean(validObj);
        
        if (isValid)
        {
            Game.Logger.LogInformation("✅ Serialization test passed");
        }
        else
        {
            Game.Logger.LogWarning("❌ Serialization test failed");
        }
    }

    private static async Task<bool> OnLatencyTestReceived(Player? sender, LatencyTestMessage message)
    {
        if (message.Direction == "ServerToClient")
        {
            // 服务器发送的延迟测试，客户端回应
            var responseMessage = new LatencyTestMessage
            {
                TestId = message.TestId,
                Direction = "ClientToServer",
                ClientSentTime = message.ClientSentTime,
                ServerReceivedTime = message.ServerReceivedTime,
                ServerSentTime = message.ServerSentTime,
                ClientReceivedTime = DateTime.UtcNow
            };

            await MessageBuilder.Create(responseMessage)
                .WithPriority(MessagePriority.Critical)
                .SendToServerAsync();
            
            // 计算延迟
            var oneWayLatency = DateTime.UtcNow - message.ServerSentTime;
            Game.Logger.LogInformation("⏱️ Server→Client latency: {Latency:F2}ms", 
                oneWayLatency.TotalMilliseconds);
        }
        else if (message.Direction == "ClientToServer")
        {
            // 客户端发送的延迟测试的回应
            if (_pendingLatencyTests.TryGetValue(message.TestId, out var originalTest))
            {
                message.ClientReceivedTime = DateTime.UtcNow;
                var roundTripTime = message.ClientReceivedTime - originalTest.ClientSentTime;
                var oneWayLatency = message.ServerReceivedTime - originalTest.ClientSentTime;
                
                Game.Logger.LogInformation("⏱️ Latency test {TestId} completed: RTT={RTT:F2}ms, OneWay={OneWay:F2}ms",
                    message.TestId, roundTripTime.TotalMilliseconds, oneWayLatency.TotalMilliseconds);
                
                _pendingLatencyTests.Remove(message.TestId);
            }
        }
        
        return true;
    }

    private static async Task<bool> OnBatchTestReceived(Player? sender, BatchTestMessage message)
    {
        // 收集批量消息
        if (!_batchMessages.ContainsKey(message.BatchId))
        {
            _batchMessages[message.BatchId] = new List<BatchTestMessage>();
        }
        
        _batchMessages[message.BatchId].Add(message);
        
        Game.Logger.LogDebug("📦 Batch message received: {BatchId} [{Index}/{Size}]", 
            message.BatchId, message.MessageIndex, message.BatchSize);
        
        // 检查是否收到完整批量
        if (_batchMessages[message.BatchId].Count == message.BatchSize)
        {
            var batch = _batchMessages[message.BatchId];
            var totalSize = batch.Sum(m => m.Data.Length);
            var duration = DateTime.UtcNow - message.BatchStartTime;
            
            Game.Logger.LogInformation("📦 Complete batch received: {BatchId}, {MessageCount} messages, {TotalSize}B, {Duration:F2}s",
                message.BatchId, batch.Count, totalSize, duration.TotalSeconds);
            
            _batchMessages.Remove(message.BatchId);
        }
        
        return await Task.FromResult(true);
    }

    private static async Task<bool> OnStressTestReceived(Player? sender, StressTestMessage message)
    {
        // 静默处理压力测试消息，只记录统计信息
        if (message.MessageNumber % 10 == 0) // 每10条消息记录一次
        {
            Game.Logger.LogDebug("💪 Stress test progress: Thread={ThreadId}, Message={MessageNumber}, PayloadSize={PayloadSize}",
                message.ThreadId, message.MessageNumber, message.LargePayload.Length);
        }
        
        return await Task.FromResult(true);
    }

    private static async Task<bool> OnNetworkTestReceived(Player? sender, NetworkTestMessage message)
    {
        Game.Logger.LogInformation("🌐 Network test response: {Condition}, Size={Size}B", 
            message.NetworkCondition, message.PacketSize);
        
        return await Task.FromResult(true);
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public static void Cleanup()
    {
        _periodicTimer?.Dispose();
        _periodicTimer = null;
        
        _pendingLatencyTests.Clear();
        _batchMessages.Clear();
        
        Game.Logger.LogInformation("🧹 TypedMessage Test Client cleaned up");
    }
} 
#endif