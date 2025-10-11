#if CLIENT
using GameCore.GameSystem;
using GameUI.Control.Primitive;
using GameUI.Control.Extensions;

namespace GameEntry.ModernUITest;

/// <summary>
/// Modern UI Test Mode - 现代化UI测试模式
/// </summary>
/// <remarks>
/// 专用于测试现代化流式UI API的游戏模式，无需场景，只专注于UI展示和交互
/// </remarks>
public class ModernUITestMode : IGameClass
{
    private static ModernUITestMode? _instance;
    private Panel? _mainUI;

    public static ModernUITestMode Instance => _instance ??= new ModernUITestMode();

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.OnGameUIInitialization += OnGameUIInitialization;
        Game.Logger.LogInformation("🎨 Modern UI Test Mode registered");
    }

    private static void OnGameTriggerInitialization()
    {
        Game.Logger.LogInformation("🔍 ModernUITest: OnGameTriggerInitialization called. Current GameMode: {0}", Game.GameModeLink?.FriendlyName ?? "null");
        
        // 只在ModernUITest模式下初始化
        if (Game.GameModeLink != ScopeData.GameMode.ModernUITest)
        {
            Game.Logger.LogInformation("❌ ModernUITest: Wrong game mode, skipping initialization");
            return;
        }

        // 初始化UI属性系统
        TriggerEncapsulation.UIProperty.UIPropertyUsageExample.Initialize();
        Game.Logger.LogInformation("🔧 Modern UI Test triggers and PropertyPlayerUI system initialized");
    }

    private static void OnGameUIInitialization()
    {
        Game.Logger.LogInformation("🎨 ModernUITest: OnGameUIInitialization called. Current GameMode: {0}", Game.GameModeLink?.FriendlyName ?? "null");
        
        // 只在ModernUITest模式下初始化UI
        if (Game.GameModeLink != ScopeData.GameMode.ModernUITest)
        {
            Game.Logger.LogInformation("❌ ModernUITest: Wrong game mode, skipping UI initialization");
            return;
        }

        Game.Logger.LogInformation("✅ ModernUITest: Correct game mode, initializing UI...");
        Instance.InitializeUI();
        Game.Logger.LogInformation("🎨 Modern UI Test Mode UI initialized");
        
        // 取消事件注册避免重复初始化
        Game.OnGameTriggerInitialization -= OnGameTriggerInitialization;
        Game.OnGameUIInitialization -= OnGameUIInitialization;
    }

    private void InitializeUI()
    {
        CreateMainUI();
        ShowUI();
        Game.Logger.LogInformation("✅ Modern UI Test interface created and displayed");
    }

    private void CreateMainUI()
    {
        // 使用ModernUIExampleUsage中的演示选择器作为主界面
        _mainUI = ModernUIExampleUsage.CreateDemoSelector();
        
        Game.Logger.LogInformation("🖼️ Main UI panel created with demo selector");
        Game.Logger.LogInformation("🔍 Main UI panel type: {0}, IsValid: {1}", _mainUI?.GetType().Name, _mainUI?.IsValid);
    }

    private void ShowUI()
    {
        if (_mainUI != null)
        {
            // 🎯 使用全屏显示 - 符合现代应用预期
            _mainUI.Stretch()
                   .GrowRatio(1, 1)
                   .Show()
                   .AddToRoot();
            Game.Logger.LogInformation("🖥️ Main UI panel set to full screen and added to root");
        }
        else
        {
            Game.Logger.LogWarning("⚠️ Cannot show UI: _mainUI is null");
        }
    }
}
#endif 