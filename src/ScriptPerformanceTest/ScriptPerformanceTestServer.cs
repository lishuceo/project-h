#if SERVER
using System.Diagnostics;
using TriggerEncapsulation.Messaging;

namespace GameEntry.ScriptPerformanceTest;

/// <summary>
/// 脚本性能测试 - 服务端处理器
/// </summary>
public class ScriptPerformanceTestServer : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        // 只在ScriptPerformanceTest游戏模式下运行
        if (Game.GameModeLink != ScopeData.GameMode.ScriptPerformanceTest)
        {
            return;
        }

        Game.Logger.LogInformation("🖥️ Initializing Script Performance Test Server...");
        
        // 注册消息处理器
        RegisterMessageHandlers();
        
        Game.Logger.LogInformation("✅ Script Performance Test Server initialized");
    }

    /// <summary>
    /// 注册消息处理器
    /// </summary>
    private static void RegisterMessageHandlers()
    {
        TypedMessageHandler.Register<ScriptPerformanceTestRequest>(OnPerformanceTestRequest,
            MessagePriority.High, "ServerPerformanceTestHandler");
        
        Game.Logger.LogDebug("🎯 Server performance test message handlers registered");
    }

    /// <summary>
    /// 处理性能测试请求
    /// </summary>
    private static async Task<bool> OnPerformanceTestRequest(Player? sender, ScriptPerformanceTestRequest request)
    {
        if (sender == null)
        {
            Game.Logger.LogWarning("❌ Performance test request from null player");
            return false;
        }

        Game.Logger.LogInformation("🧪 Received performance test request: {TestType} x{Count} from Player {PlayerId}",
            request.TestType, request.TestCount, sender.Id);

        var result = new ScriptPerformanceTestResult
        {
            TestType = request.TestType,
            TestCount = request.TestCount,
            ClientSentTime = request.ClientSentTime,
            ServerReceivedTime = DateTime.UtcNow,
            TestId = request.TestId
        };

        try
        {
            // 执行对应的性能测试
            var stopwatch = Stopwatch.StartNew();
            
            switch (request.TestType.ToLowerInvariant())
            {
                case "addition":
                    ExecuteAdditionTest(request.TestCount, result);
                    break;
                
                case "scripttohostcall":
                    ExecuteScriptToHostCallTest(request.TestCount, result);
                    break;
                
                case "hosttoscriptcall":
                    ExecuteHostToScriptCallTest(request.TestCount, result);
                    break;
                
                default:
                    throw new ArgumentException($"Unknown test type: {request.TestType}");
            }
            
            stopwatch.Stop();
            result.ServerElapsedMs = stopwatch.ElapsedMilliseconds;
            result.ServerCompletedTime = DateTime.UtcNow;
            result.Success = true;

            Game.Logger.LogInformation("✅ Performance test completed: {TestType} x{Count} in {ElapsedMs}ms",
                request.TestType, request.TestCount, result.ServerElapsedMs);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ServerCompletedTime = DateTime.UtcNow;
            
            Game.Logger.LogError(ex, "❌ Performance test failed: {TestType}", request.TestType);
        }

        // 发送结果回客户端
        var responseMessage = new TypedMessage<ScriptPerformanceTestResult>(result);
        await responseMessage.SendToAsync(sender);

        return true;
    }

    /// <summary>
    /// 执行加法性能测试
    /// </summary>
    private static void ExecuteAdditionTest(int count, ScriptPerformanceTestResult result)
    {
        
        double total = 0;
        for (int i = 0; i < count; i++)
        {
            total += 1;
        }
    }

    /// <summary>
    /// 执行脚本调用宿主性能测试
    /// </summary>
    private static void ExecuteScriptToHostCallTest(int count, ScriptPerformanceTestResult result)
    {
        
        int callCount = 0;
        for (int i = 0; i < count; i++)
        {
            Game.TestInvokeHostOnce();
        }
        
        result.AdditionalInfo["CallCount"] = callCount;
        result.AdditionalInfo["TestDescription"] = "Script-to-host call simulation (placeholder for actual script engine)";
        
        Game.Logger.LogDebug("📞 Script-to-host test completed: {Count} calls", count);
    }

    /// <summary>
    /// 执行宿主调用脚本性能测试
    /// </summary>
    private static void ExecuteHostToScriptCallTest(int count, ScriptPerformanceTestResult result)
    {        
        Game.TestInvokedByHost(count);
        
        result.AdditionalInfo["TestDescription"] = "Host-to-script call simulation (placeholder for actual script engine)";
        
        Game.Logger.LogDebug("📱 Host-to-script test completed: {Count} calls", count);
    }
}
#endif
