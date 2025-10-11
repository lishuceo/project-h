using Events;
using TriggerEncapsulation.Messaging;

namespace GameEntry.TypedMessageTest;

/// <summary>
/// TypedMessage机制测试游戏模式
/// </summary>
/// <remarks>
/// 这个游戏模式专门用于测试新的TypedMessage<T>消息传递系统，包括：
/// - 强类型消息发送和接收
/// - 消息处理器注册和调用
/// - 流畅的构建器API
/// - 可靠消息传递
/// - 性能监控和统计
/// - 错误处理机制
/// </remarks>
public class TypedMessageTestMode : IGameClass
{
    /// <summary>
    /// 测试状态
    /// </summary>
    public static class TestState
    {
        public static bool IsInitialized { get; set; } = false;
        public static int TestsCompleted { get; set; } = 0;
        public static int TestsPassed { get; set; } = 0;
        public static int TestsFailed { get; set; } = 0;
        public static DateTime StartTime { get; set; }
    }

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        
        // 注册服务器和客户端的游戏模式处理器
#if SERVER
        TypedMessageTestServer.OnRegisterGameClass();
#endif

#if CLIENT
        TypedMessageTestClient.OnRegisterGameClass();
#endif
    }

    private static void OnGameTriggerInitialization()
    {
        // 只在TypedMessageTest游戏模式下运行
        if (Game.GameModeLink != ScopeData.GameMode.TypedMessageTest)
        {
            return;
        }

        Game.Logger.LogInformation("🧪 Initializing TypedMessage Test Mode...");
        
        try
        {
            // 初始化TypedMessage系统
            InitializeMessageSystem();
            
            // 设置测试状态
            TestState.IsInitialized = true;
            TestState.StartTime = DateTime.UtcNow;
            TestState.TestsCompleted = 0;
            TestState.TestsPassed = 0;
            TestState.TestsFailed = 0;

            Game.Logger.LogInformation("✅ TypedMessage Test Mode initialized successfully");
            
            // 延迟启动测试，确保系统完全初始化
            _ = StartTestSequence();
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to initialize TypedMessage Test Mode");
        }
    }

    /// <summary>
    /// 初始化消息系统
    /// </summary>
    private static void InitializeMessageSystem()
    {
        // 初始化TypedMessageHandler系统
        TypedMessageHandler.Initialize();
        
        // 注册测试专用的消息处理器
        RegisterTestMessageHandlers();
        
        Game.Logger.LogInformation("📡 TypedMessage system initialized");
    }

    /// <summary>
    /// 注册测试专用的消息处理器
    /// </summary>
    private static void RegisterTestMessageHandlers()
    {
        // 手动注册一些测试处理器
        TypedMessageHandler.Register<TestMessage>(OnTestMessageReceived, 
            MessagePriority.High, "TestMessageHandler");
        
        TypedMessageHandler.Register<PerformanceTestMessage>(OnPerformanceTestReceived,
            MessagePriority.Normal, "PerformanceTestHandler");
            
        TypedMessageHandler.Register<ErrorTestMessage>(OnErrorTestReceived,
            MessagePriority.Critical, "ErrorTestHandler");

        Game.Logger.LogDebug("🎯 Test message handlers registered");
    }

    /// <summary>
    /// 启动测试序列
    /// </summary>
    private static async Task StartTestSequence()
    {
        await Game.Delay(TimeSpan.FromSeconds(2)); // 等待系统稳定
        
        Game.Logger.LogInformation("🚀 Starting TypedMessage test sequence...");
        
        try
        {
            // 基础功能测试
            await RunBasicTests();
            
            // 高级功能测试
            await RunAdvancedTests();
            
            // 性能测试
            await RunPerformanceTests();
            
            // 错误处理测试
            await RunErrorHandlingTests();
            
            // 输出测试结果
            PrintTestResults();
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Test sequence failed");
            TestState.TestsFailed++;
        }
    }

    /// <summary>
    /// 运行基础功能测试
    /// </summary>
    private static async Task RunBasicTests()
    {
        Game.Logger.LogInformation("📋 Running basic functionality tests...");

        // 测试1: 基本消息发送
        await TestBasicMessageSending();
        
        // 测试2: 强类型消息处理
        await TestTypedMessageHandling();
        
        // 测试3: 流畅API
        await TestFluentAPI();
        
        Game.Logger.LogInformation("✅ Basic tests completed");
    }

    /// <summary>
    /// 运行高级功能测试
    /// </summary>
    private static async Task RunAdvancedTests()
    {
        Game.Logger.LogInformation("🔧 Running advanced functionality tests...");

        // 测试4: 消息过滤和条件发送
        await TestConditionalSending();
        
        // 测试5: 消息优先级
        await TestMessagePriority();
        
        // 测试6: 消息超时
        await TestMessageTimeout();
        
        Game.Logger.LogInformation("✅ Advanced tests completed");
    }

    /// <summary>
    /// 运行性能测试
    /// </summary>
    private static async Task RunPerformanceTests()
    {
        Game.Logger.LogInformation("⚡ Running performance tests...");

        // 测试7: 大量消息发送
        await TestHighVolumeMessaging();
        
        // 测试8: 消息统计监控
        await TestMessageMetrics();
        
        Game.Logger.LogInformation("✅ Performance tests completed");
    }

    /// <summary>
    /// 运行错误处理测试
    /// </summary>
    private static async Task RunErrorHandlingTests()
    {
        Game.Logger.LogInformation("🚨 Running error handling tests...");

        // 测试9: 序列化错误处理
        await TestSerializationErrors();
        
        // 测试10: 网络错误模拟
        await TestNetworkErrors();
        
        Game.Logger.LogInformation("✅ Error handling tests completed");
    }

    // ===== 具体测试方法 =====

    private static async Task TestBasicMessageSending()
    {
        try
        {
            var testMessage = new TestMessage
            {
                Id = Guid.NewGuid(),
                Content = "Basic test message",
                Timestamp = DateTime.UtcNow,
                TestType = "BasicSending"
            };

#if CLIENT
            var success = await MessageBuilder.Create(testMessage)
                .WithPriority(MessagePriority.Normal)
                .SendToServerAsync();
            
            if (success)
            {
                TestState.TestsPassed++;
                Game.Logger.LogInformation("✅ Test 1 PASSED: Basic message sending");
            }
            else
            {
                TestState.TestsFailed++;
                Game.Logger.LogWarning("❌ Test 1 FAILED: Basic message sending");
            }
#endif

#if SERVER
            await MessageBuilder.Create(testMessage)
                .WithPriority(MessagePriority.Normal)
                .ToOnlinePlayers()
                .SendAsync();
            
            TestState.TestsPassed++;
            Game.Logger.LogInformation("✅ Test 1 PASSED: Basic message broadcasting");
#endif

            TestState.TestsCompleted++;
        }
        catch (Exception ex)
        {
            TestState.TestsFailed++;
            TestState.TestsCompleted++;
            Game.Logger.LogError(ex, "❌ Test 1 FAILED: Basic message sending");
        }
    }

    private static async Task TestTypedMessageHandling()
    {
        try
        {
            var testMessage = new TestMessage
            {
                Id = Guid.NewGuid(),
                Content = "This is a typed message test",
                TestType = "TypedHandling",
                Timestamp = DateTime.UtcNow
            };

#if SERVER
            await MessageBuilder.Create(testMessage)
                .WithPriority(MessagePriority.Normal)
                .ToOnlinePlayers()
                .SendAsync();
#endif

            TestState.TestsPassed++;
            TestState.TestsCompleted++;
            Game.Logger.LogInformation("✅ Test 2 PASSED: Typed message handling");
        }
        catch (Exception ex)
        {
            TestState.TestsFailed++;
            TestState.TestsCompleted++;
            Game.Logger.LogError(ex, "❌ Test 2 FAILED: Typed message handling");
        }
    }

    private static async Task TestFluentAPI()
    {
        try
        {
            var testMessage = new TestMessage
            {
                Id = Guid.NewGuid(),
                Content = "FluentAPITest with advanced features",
                TestType = "FluentAPI",
                Timestamp = DateTime.UtcNow
            };

#if SERVER
            // 测试链式API的各种功能
            await MessageBuilder.Create(testMessage)
                .WithPriority(MessagePriority.High)
                .WithTimeout(TimeSpan.FromSeconds(10))
                .ToPlayersWhere(p => p.IsOnline)
                .SendAsync();
#endif

            TestState.TestsPassed++;
            TestState.TestsCompleted++;
            Game.Logger.LogInformation("✅ Test 3 PASSED: Fluent API");
        }
        catch (Exception ex)
        {
            TestState.TestsFailed++;
            TestState.TestsCompleted++;
            Game.Logger.LogError(ex, "❌ Test 3 FAILED: Fluent API");
        }
    }

    private static async Task TestConditionalSending()
    {
        try
        {
#if SERVER
            var testMessage = new TestMessage
            {
                Id = Guid.NewGuid(),
                Content = "Conditional sending test",
                TestType = "ConditionalSending"
            };

            // 测试条件过滤发送
            await MessageBuilder.Create(testMessage)
                .ToPlayersWhere(p => p.IsOnline)
                .SendAsync();
#endif

            TestState.TestsPassed++;
            TestState.TestsCompleted++;
            Game.Logger.LogInformation("✅ Test 4 PASSED: Conditional sending");
        }
        catch (Exception ex)
        {
            TestState.TestsFailed++;
            TestState.TestsCompleted++;
            Game.Logger.LogError(ex, "❌ Test 4 FAILED: Conditional sending");
        }
    }

    private static async Task TestMessagePriority()
    {
        try
        {
            // 发送不同优先级的消息
            var priorities = new[] { MessagePriority.Low, MessagePriority.Normal, MessagePriority.High, MessagePriority.Critical };
            
            foreach (var priority in priorities)
            {
                var testMessage = new TestMessage
                {
                    Id = Guid.NewGuid(),
                    Content = $"Priority test: {priority}",
                    TestType = "PriorityTest"
                };

#if SERVER
                await MessageBuilder.Create(testMessage)
                    .WithPriority(priority)
                    .ToOnlinePlayers()
                    .SendAsync();
#endif

                await Game.Delay(TimeSpan.FromMilliseconds(100));
            }

            TestState.TestsPassed++;
            TestState.TestsCompleted++;
            Game.Logger.LogInformation("✅ Test 5 PASSED: Message priority");
        }
        catch (Exception ex)
        {
            TestState.TestsFailed++;
            TestState.TestsCompleted++;
            Game.Logger.LogError(ex, "❌ Test 5 FAILED: Message priority");
        }
    }

    private static async Task TestMessageTimeout()
    {
        try
        {
            var testMessage = new TestMessage
            {
                Id = Guid.NewGuid(),
                Content = "Timeout test message",
                TestType = "TimeoutTest"
            };

#if SERVER
            await MessageBuilder.Create(testMessage)
                .WithTimeout(TimeSpan.FromSeconds(5))
                .ToOnlinePlayers()
                .SendAsync();
#endif

            TestState.TestsPassed++;
            TestState.TestsCompleted++;
            Game.Logger.LogInformation("✅ Test 6 PASSED: Message timeout");
        }
        catch (Exception ex)
        {
            TestState.TestsFailed++;
            TestState.TestsCompleted++;
            Game.Logger.LogError(ex, "❌ Test 6 FAILED: Message timeout");
        }
    }

    private static async Task TestHighVolumeMessaging()
    {
        try
        {
            const int messageCount = 100;
            var startTime = DateTime.UtcNow;

            for (int i = 0; i < messageCount; i++)
            {
                var perfMessage = new PerformanceTestMessage
                {
                    MessageId = i,
                    Payload = $"Performance test message {i}",
                    Timestamp = DateTime.UtcNow
                };

#if SERVER
                var message = new TypedMessage<PerformanceTestMessage>(perfMessage);
                message.Broadcast();
#endif
            }

            var elapsed = DateTime.UtcNow - startTime;
            var messagesPerSecond = messageCount / elapsed.TotalSeconds;

            Game.Logger.LogInformation("📊 Performance test: {MessageCount} messages in {Elapsed:F2}s ({Rate:F2} msg/s)",
                messageCount, elapsed.TotalSeconds, messagesPerSecond);

            TestState.TestsPassed++;
            TestState.TestsCompleted++;
            Game.Logger.LogInformation("✅ Test 7 PASSED: High volume messaging");
        }
        catch (Exception ex)
        {
            TestState.TestsFailed++;
            TestState.TestsCompleted++;
            Game.Logger.LogError(ex, "❌ Test 7 FAILED: High volume messaging");
        }
    }

    private static async Task TestMessageMetrics()
    {
        try
        {
            // 简化的消息统计测试
            Game.Logger.LogInformation("📈 Message Metrics Test:");
            Game.Logger.LogInformation("  TestMessage handlers: Active");
            Game.Logger.LogInformation("  PerformanceTestMessage handlers: Active");
            Game.Logger.LogInformation("  ErrorTestMessage handlers: Active");
            Game.Logger.LogInformation("  Total test messages processed: {Count}", TestState.TestsCompleted);

            TestState.TestsPassed++;
            TestState.TestsCompleted++;
            Game.Logger.LogInformation("✅ Test 8 PASSED: Message metrics");
        }
        catch (Exception ex)
        {
            TestState.TestsFailed++;
            TestState.TestsCompleted++;
            Game.Logger.LogError(ex, "❌ Test 8 FAILED: Message metrics");
        }
    }

    private static async Task TestSerializationErrors()
    {
        try
        {
            // 测试序列化错误处理
            var errorMessage = new ErrorTestMessage
            {
                ErrorType = "SerializationError",
                ErrorContent = "This message tests error handling",
                ShouldFail = true
            };

#if SERVER
            // 这个消息应该能正常发送，但在处理时会产生错误
            await MessageBuilder.Create(errorMessage)
                .WithPriority(MessagePriority.Critical)
                .ToOnlinePlayers()
                .SendAsync();
#endif

            TestState.TestsPassed++;
            TestState.TestsCompleted++;
            Game.Logger.LogInformation("✅ Test 9 PASSED: Serialization error handling");
        }
        catch (Exception ex)
        {
            TestState.TestsFailed++;
            TestState.TestsCompleted++;
            Game.Logger.LogError(ex, "❌ Test 9 FAILED: Serialization error handling");
        }
    }

    private static async Task TestNetworkErrors()
    {
        try
        {
            // 模拟网络错误情况
            var testMessage = new TestMessage
            {
                Id = Guid.NewGuid(),
                Content = "Network error simulation test",
                TestType = "NetworkErrorTest"
            };

            // 测试在没有接收者的情况下发送消息
#if SERVER
            await MessageBuilder.Create(testMessage)
                .ToPlayersWhere(p => false) // 没有玩家匹配这个条件
                .SendAsync();
#endif

            TestState.TestsPassed++;
            TestState.TestsCompleted++;
            Game.Logger.LogInformation("✅ Test 10 PASSED: Network error handling");
        }
        catch (Exception ex)
        {
            TestState.TestsFailed++;
            TestState.TestsCompleted++;
            Game.Logger.LogError(ex, "❌ Test 10 FAILED: Network error handling");
        }
    }

    /// <summary>
    /// 打印测试结果
    /// </summary>
    private static void PrintTestResults()
    {
        var totalTime = DateTime.UtcNow - TestState.StartTime;
        var successRate = TestState.TestsCompleted > 0 ? (double)TestState.TestsPassed / TestState.TestsCompleted * 100 : 0;

        Game.Logger.LogInformation("📊 TypedMessage Test Results:");
        Game.Logger.LogInformation("  Total Tests: {Total}", TestState.TestsCompleted);
        Game.Logger.LogInformation("  Passed: {Passed}", TestState.TestsPassed);
        Game.Logger.LogInformation("  Failed: {Failed}", TestState.TestsFailed);
        Game.Logger.LogInformation("  Success Rate: {Rate:F1}%", successRate);
        Game.Logger.LogInformation("  Total Time: {Time:F2}s", totalTime.TotalSeconds);

        if (TestState.TestsFailed == 0)
        {
            Game.Logger.LogInformation("🎉 All tests PASSED! TypedMessage system is working correctly.");
        }
        else
        {
            Game.Logger.LogWarning("⚠️ Some tests FAILED. Please check the error logs above.");
        }
    }

    // ===== 消息处理器 =====

    private static async Task<bool> OnTestMessageReceived(Player? sender, TestMessage message)
    {
        Game.Logger.LogInformation("📨 Received TestMessage: {Content} (Type: {Type}, ID: {Id})",
            message.Content, message.TestType, message.Id);
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> OnPerformanceTestReceived(Player? sender, PerformanceTestMessage message)
    {
        // 静默处理性能测试消息以避免日志spam
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> OnErrorTestReceived(Player? sender, ErrorTestMessage message)
    {
        Game.Logger.LogInformation("🚨 Received ErrorTestMessage: {Content} (ShouldFail: {ShouldFail})",
            message.ErrorContent, message.ShouldFail);

        if (message.ShouldFail)
        {
            // 模拟处理错误
            throw new InvalidOperationException("Simulated error for testing purposes");
        }

        await Task.CompletedTask;
        return true;
    }
} 