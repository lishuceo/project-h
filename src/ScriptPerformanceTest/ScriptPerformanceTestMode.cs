#if CLIENT
using GameCore.GameSystem;
using GameUI.Control.Primitive;
using GameUI.Control.Extensions;
using GameUI.Enum;
using GameUI.Brush;
using static GameUI.Control.Extensions.UI;
using System.Diagnostics;
using TriggerEncapsulation.Messaging;

namespace GameEntry.ScriptPerformanceTest;

/// <summary>
/// Script Performance Test Mode - 脚本执行效率测试模式
/// </summary>
/// <remarks>
/// 专用于测试脚本环境执行效率的游戏模式，无需场景，只专注于性能测试
/// </remarks>
public class ScriptPerformanceTestMode : IGameClass
{
    private static ScriptPerformanceTestMode? _instance;
    private Panel? _mainUI;
    private Input? _countInput;
    private Label? _resultDisplay;
    
    // 用于跟踪待处理的测试请求
    private readonly Dictionary<Guid, DateTime> _pendingTests = new();

    public static ScriptPerformanceTestMode Instance => _instance ??= new ScriptPerformanceTestMode();

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.OnGameUIInitialization += OnGameUIInitialization;
        Game.Logger.LogInformation("🚀 Script Performance Test Mode registered");
    }

    private static void OnGameTriggerInitialization()
    {
        Game.Logger.LogInformation("🔍 ScriptPerformanceTest: OnGameTriggerInitialization called. Current GameMode: {0}", Game.GameModeLink?.FriendlyName ?? "null");
        
        // 只在ScriptPerformanceTest模式下初始化
        if (Game.GameModeLink != ScopeData.GameMode.ScriptPerformanceTest)
        {
            Game.Logger.LogInformation("❌ ScriptPerformanceTest: Wrong game mode, skipping initialization");
            return;
        }

        Game.Logger.LogInformation("🔧 Script Performance Test triggers initialized");
    }

    private static void OnGameUIInitialization()
    {
        Game.Logger.LogInformation("🎨 ScriptPerformanceTest: OnGameUIInitialization called. Current GameMode: {0}", Game.GameModeLink?.FriendlyName ?? "null");
        
        // 只在ScriptPerformanceTest模式下初始化UI
        if (Game.GameModeLink != ScopeData.GameMode.ScriptPerformanceTest)
        {
            Game.Logger.LogInformation("❌ ScriptPerformanceTest: Wrong game mode, skipping UI initialization");
            return;
        }

        Game.Logger.LogInformation("✅ ScriptPerformanceTest: Correct game mode, initializing...");
        
        // 注册消息处理器
        Instance.RegisterMessageHandlers();
        
        // 初始化UI
        Instance.InitializeUI();
        Game.Logger.LogInformation("🎨 Script Performance Test Mode initialized");
        
        // 取消事件注册避免重复初始化
        Game.OnGameTriggerInitialization -= OnGameTriggerInitialization;
        Game.OnGameUIInitialization -= OnGameUIInitialization;
    }

    /// <summary>
    /// 注册消息处理器
    /// </summary>
    private void RegisterMessageHandlers()
    {
        TypedMessageHandler.Register<ScriptPerformanceTestResult>(OnPerformanceTestResult,
            MessagePriority.High, "ClientPerformanceTestResultHandler");
        
        Game.Logger.LogDebug("🎯 Client performance test message handlers registered");
    }

    /// <summary>
    /// 处理来自服务端的性能测试结果
    /// </summary>
    private async Task<bool> OnPerformanceTestResult(Player? sender, ScriptPerformanceTestResult result)
    {
        Game.Logger.LogInformation("📊 Received performance test result: {TestType} x{Count} = {ElapsedMs}ms",
            result.TestType, result.TestCount, result.ServerElapsedMs);

        if (_pendingTests.ContainsKey(result.TestId))
        {
            _pendingTests.Remove(result.TestId);
            
            // 计算网络往返时间
            var networkLatency = DateTime.UtcNow - result.ClientSentTime;
            var serverProcessTime = result.ServerCompletedTime - result.ServerReceivedTime;
            
            // 显示详细结果（将毫秒转换为纳秒显示平均耗时）
            long serverElapsedNs = result.ServerElapsedMs * 1_000_000;
            var resultText = $"[SERVER] {GetTestDisplayName(result.TestType)}\n" +
                           $"执行次数: {result.TestCount:N0}\n" +
                           $"服务端耗时: {result.ServerElapsedMs} ms\n" +
                           $"平均耗时: {(double)serverElapsedNs / result.TestCount:F0} ns\n" +
                           $"每秒执行: {(result.TestCount * 1000.0 / result.ServerElapsedMs):N0} 次\n" +
                           $"网络往返: {networkLatency.TotalMilliseconds:F1} ms\n";

            if (result.AdditionalInfo.Count > 0)
            {
                resultText += $"额外信息: {string.Join(", ", result.AdditionalInfo.Select(kvp => $"{kvp.Key}={kvp.Value}"))}\n";
            }

            if (!result.Success)
            {
                resultText += $"❌ 错误: {result.ErrorMessage}\n";
            }

            resultText += "\n";

            DisplayResult(resultText);
        }

        return await Task.FromResult(true);
    }

    private void InitializeUI()
    {
        CreateMainUI();
        ShowUI();
        Game.Logger.LogInformation("✅ Script Performance Test interface created and displayed");
    }

    private void CreateMainUI()
    {
        // 创建主面板
        _mainUI = Panel()
            .Stretch()
            .GrowRatio(1, 1)
            .Padding(40)
            .Background(Colors.Surface);

        // 输入框用于指定测试次数
        _countInput = new Input();
        _countInput.Size(200, 40);
        _countInput.Text = "1000000"; // 默认值
        _countInput.TextColor = System.Drawing.Color.Black; // 设置文字颜色为黑色
        _countInput.Background = new SolidColorBrush(System.Drawing.Color.LightGray); // 设置背景色为浅灰色，便于识别

        // 结果显示区域
        _resultDisplay = Label("点击按钮开始测试...")
            .TextColor(Colors.OnSurface)
            .FontSize(14);

        // 结果显示Panel（可滚动）
        var resultPanel = ScrollPanel()
            .VScroll()
            .Background(Colors.Background)
            .Padding(20)
            .CornerRadius(8)
            .Size(800, 300)  // 设置固定高度，使其能够滚动
            .ScrollBar(8, Colors.Primary);
        
        resultPanel.AddChild(
            VStack(spacing: 10,
                Label("测试结果:")
                    .FontSize(18)
                    .Bold()
                    .TextColor(Colors.Primary),
                _resultDisplay
            )
        );

        // 创建UI布局
        var content = VStack(spacing: 20,
            // 标题
            Title("脚本执行效率测试", 28)
                .Center(),

            // 测试次数输入
            HStack(spacing: 10,
                Label("测试次数:")
                    .FontSize(16)
                    .AlignVertical(VerticalAlignment.Center),
                _countInput
            ).Center(),

            // 服务端测试区域
            CreateServerTestSection(),

            // 客户端测试区域  
            CreateClientTestSection(),

            // 结果显示区域
            resultPanel
        );

        _mainUI.AddChild(content);
        Game.Logger.LogInformation("🖼️ Main UI panel created with performance test interface");
    }

    private Panel CreateServerTestSection()
    {
        var panel = Panel()
            .Background(Colors.Primary)
            .Padding(20)
            .CornerRadius(8);
        
        panel.AddChild(
            VStack(spacing: 15,
                Label("服务端脚本测试")
                    .FontSize(20)
                    .Bold()
                    .TextColor(Colors.OnPrimary)
                    .Center(),

                HStack(spacing: 10,
                    Primary("测试加法X次")
                        .Click(() => RunServerAdditionTest()),
                    
                    Primary("测试脚本调用宿主X次")
                        .Click(() => RunServerScriptToHostTest()),
                    
                    Primary("测试宿主调用脚本X次")
                        .Click(() => RunServerHostToScriptTest())
                ).Center()
            )
        );
        
        return panel;
    }

    private Panel CreateClientTestSection()
    {
        var panel = Panel()
            .Background(Colors.Secondary)
            .Padding(20)
            .CornerRadius(8);
        
        panel.AddChild(
            VStack(spacing: 15,
                Label("客户端脚本测试")
                    .FontSize(20)
                    .Bold()
                    .TextColor(Colors.OnSecondary)
                    .Center(),

                HStack(spacing: 10,
                    Secondary("测试加法X次")
                        .Click(() => RunClientAdditionTest()),
                    
                    Secondary("测试脚本调用宿主X次")
                        .Click(() => RunClientScriptToHostTest()),
                    
                    Secondary("测试宿主调用脚本X次")
                        .Click(() => RunClientHostToScriptTest())
                ).Center()
            )
        );
        
        return panel;
    }

    private void ShowUI()
    {
        if (_mainUI != null)
        {
            _mainUI.Show().AddToRoot();
            Game.Logger.LogInformation("🖥️ Main UI panel set to full screen and added to root");
        }
        else
        {
            Game.Logger.LogWarning("⚠️ Cannot show UI: _mainUI is null");
        }
    }

    private int GetTestCount()
    {
        string inputText = _countInput?.Text ?? "null";
        Game.Logger.LogInformation("🔍 Reading input text: '{InputText}'", inputText);
        
        if (_countInput?.Text != null && int.TryParse(_countInput.Text, out int count))
        {
            Game.Logger.LogInformation("✅ Parsed count: {Count}", count);
            return Math.Max(1, count); // 至少1次
        }
        
        Game.Logger.LogInformation("⚠️ Using default count: 10000");
        return 10000; // 默认值
    }

    private void DisplayResult(string resultText)
    {
        if (_resultDisplay != null)
        {
            _resultDisplay.Text = (_resultDisplay.Text == "点击按钮开始测试..." ? "" : _resultDisplay.Text) + resultText;
        }
    }

    private void DisplayResult(string testName, long elapsedMs, int count, string side)
    {
        // 将毫秒转换为纳秒进行平均耗时计算
        long elapsedNs = elapsedMs * 1_000_000;
        var result = $"[{side}] {testName}\n" +
                    $"执行次数: {count:N0}\n" +
                    $"总耗时: {elapsedMs} ms\n" +
                    $"平均耗时: {(double)elapsedNs / count:F0} ns\n" +
                    $"每秒执行: {(count * 1000.0 / elapsedMs):N0} 次\n\n";

        DisplayResult(result);
        Game.Logger.LogInformation("📊 {TestName} completed: {Count} iterations in {ElapsedMs}ms", testName, count, elapsedMs);
    }

    private string GetTestDisplayName(string testType)
    {
        return testType.ToLowerInvariant() switch
        {
            "addition" => "服务端加法测试",
            "scripttohostcall" => "服务端脚本调用宿主测试",
            "hosttoscriptcall" => "服务端宿主调用脚本测试",
            _ => testType
        };
    }

    // ==================== 服务端测试方法 ====================
    private void RunServerAdditionTest()
    {
        _ = SendServerTestRequest("Addition");
    }

    private void RunServerScriptToHostTest()
    {
        _ = SendServerTestRequest("ScriptToHostCall");
    }

    private void RunServerHostToScriptTest()
    {
        _ = SendServerTestRequest("HostToScriptCall");
    }

    /// <summary>
    /// 发送服务端测试请求
    /// </summary>
    private Task SendServerTestRequest(string testType)
    {
        int count = GetTestCount();
        Game.Logger.LogInformation("🔧 Sending server test request: {TestType} with {Count} iterations", testType, count);
        
        var request = new ScriptPerformanceTestRequest
        {
            TestType = testType,
            TestCount = count,
            ClientSentTime = DateTime.UtcNow
        };

        // 记录待处理的测试
        _pendingTests[request.TestId] = DateTime.UtcNow;

        // 设置超时清理
        _ = Game.Delay(TimeSpan.FromSeconds(30)).ContinueWith(t =>
        {
            if (_pendingTests.ContainsKey(request.TestId))
            {
                _pendingTests.Remove(request.TestId);
                Game.Logger.LogWarning("⏱️ Server test {TestType} {TestId} timed out", testType, request.TestId);
                
                // 在UI中显示超时信息
                var timeoutMessage = $"[SERVER] {GetTestDisplayName(testType)}\n❌ 测试超时 (30秒)\n\n";
                DisplayResult(timeoutMessage);
            }
        });

        // 发送请求到服务端
        var message = new TypedMessage<ScriptPerformanceTestRequest>(request);
        bool success = message.SendToServer();

        if (success)
        {
            Game.Logger.LogDebug("📤 Server test request sent: {TestType}, TestId={TestId}", testType, request.TestId);
            
            // 在UI中显示发送状态
            var statusMessage = $"[SERVER] {GetTestDisplayName(testType)}\n📤 已发送到服务端，等待结果...\n\n";
            DisplayResult(statusMessage);
        }
        else
        {
            _pendingTests.Remove(request.TestId);
            Game.Logger.LogError("❌ Failed to send server test request: {TestType}", testType);
            
            // 在UI中显示错误信息
            var errorMessage = $"[SERVER] {GetTestDisplayName(testType)}\n❌ 发送请求失败\n\n";
            DisplayResult(errorMessage);
        }

        return Task.CompletedTask;
    }

    // ==================== 客户端测试方法 ====================
    private void RunClientAdditionTest()
    {
        int count = GetTestCount();
        Game.Logger.LogInformation("🎨 Starting client addition test with {Count} iterations", count);
        
        var stopwatch = Stopwatch.StartNew();
        

        double result = 0;
        for (int i = 0; i < count; i++)
        {
            result += 1;
        }
        
        stopwatch.Stop();
        DisplayResult("客户端加法测试", stopwatch.ElapsedMilliseconds, count, "CLIENT");
    }

    private void RunClientScriptToHostTest()
    {
        int count = GetTestCount();
        Game.Logger.LogInformation("🎨 Starting client script-to-host test with {Count} iterations", count);
        
        var stopwatch = Stopwatch.StartNew();
        
        // 占位符逻辑 - 用户可以替换为实际的脚本调用宿主测试
        for (int i = 0; i < count; i++)
        {
            Game.TestInvokeHostOnce();
        }
        
        stopwatch.Stop();
        DisplayResult("客户端脚本调用宿主测试", stopwatch.ElapsedMilliseconds, count, "CLIENT");
    }

    private void RunClientHostToScriptTest()
    {
        int count = GetTestCount();
        Game.Logger.LogInformation("🎨 Starting client host-to-script test with {Count} iterations", count);
        
        var stopwatch = Stopwatch.StartNew();
        
        // 占位符逻辑 - 用户可以替换为实际的宿主调用脚本测试
        Game.TestInvokedByHost(count);
        
        stopwatch.Stop();
        DisplayResult("客户端宿主调用脚本测试", stopwatch.ElapsedMilliseconds, count, "CLIENT");
    }
}
#endif
