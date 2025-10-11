#if CLIENT
using GameCore.GameSystem.Data;
using GameUI.Control.Primitive;
using GameCore;
using GameUI.Struct;
using GameUI.Enum;
using System.Drawing;
using GameUI.Control.Extensions;
using GameCore.PlayerAndUsers;

namespace GameEntry.GameUITest;

/// <summary>
/// GameUITest主测试模式类
/// 提供不同UI组件测试的切换和管理功能
/// </summary>
public class GameUITestMode : IGameClass
{
    private static Panel? mainMenuPanel;
    private static Label? titleLabel;
    private static Button? joyStickTestButton;
    private static Button? buffListTestButton;
    private static Button? unitInfoTestButton;
    private static Button? returnMenuButton;
    private static Label? instructionLabel;

    private static TestModeState currentMode = TestModeState.Menu;

    /// <summary>
    /// 测试模式状态枚举
    /// </summary>
    private enum TestModeState
    {
        Menu,           // 主菜单
        JoyStickTest,   // 摇杆测试
        BuffListTest,   // Buff列表测试
        UnitInfoTest    // 单位信息测试
    }

    public static void OnRegisterGameClass()
    {
        // 由GameUITest主模块调用，直接初始化
        Game.OnGameUIInitialization += InitializeGameUITest;
    }

    /// <summary>
    /// 初始化GameUITest主界面
    /// </summary>
    private static void InitializeGameUITest()
    {
        // 检查当前游戏模式是否为GameUITest
        if (Game.GameModeLink != ScopeData.GameMode.GameUITest)
        {
            Game.Logger?.LogInformation("⏭️ GameUITestMode: 当前游戏模式不是GameUITest，跳过UI初始化");
            return;
        }

        Game.Logger?.LogInformation("🎮 初始化GameUITest主界面...");

        try
        {
            CreateMainMenu();
            ShowMainMenu();

            Game.Logger?.LogInformation("✅ GameUITest主界面初始化完成喵！");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ GameUITest主界面初始化失败: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 创建主菜单
    /// </summary>
    private static void CreateMainMenu()
    {
        mainMenuPanel = new Panel
        {
            WidthStretchRatio = 1.0f,
            HeightStretchRatio = 1.0f,
            Background = new GameUI.Brush.SolidColorBrush(Color.FromArgb(80, 0, 20, 40)) // 深蓝半透明背景
        };

        // 标题
        titleLabel = new Label
        {
            Text = "🎮 游戏UI测试模式 🐱",
            FontSize = 28,
            TextColor = Color.Cyan,
            Bold = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Position = new UIPosition(0, 80),
            Width = 600,
            Height = 50
        };

        // 摇杆测试按钮
        joyStickTestButton = CreateMenuButton("🕹️ 摇杆测试", 0, -80);
        joyStickTestButton = joyStickTestButton.Click(() => SwitchToJoyStickTest());

        // Buff列表测试按钮
        buffListTestButton = CreateMenuButton("🩸 Buff列表测试", 0, -20);
        buffListTestButton = buffListTestButton.Click(() => SwitchToBuffListTest());

        // 单位信息测试按钮
        unitInfoTestButton = CreateMenuButton("📊 单位信息面板", 0, 40);
        unitInfoTestButton = unitInfoTestButton.Click(() => SwitchToUnitInfoTest());

        // 返回菜单按钮（初始隐藏）
        returnMenuButton = CreateMenuButton("🔙 返回主菜单", 0, 100);
        returnMenuButton = returnMenuButton.Click(() => ReturnToMainMenu());
        returnMenuButton.Visible = false;

        // 说明文字
        instructionLabel = new Label
        {
            Text = "请选择要测试的UI组件\n\n" +
                   "📋 可用测试:\n" +
                   "• 摇杆测试 - 基于AbilityJoyStickGroup组件\n" +
                   "• Buff列表测试 - 基于BuffBar组件\n" +
                   "• 单位信息面板 - 展示单位属性和状态\n\n" +
                   "💡 提示: 测试需要在有单位的场景中运行",
            FontSize = 20,
            TextColor = Color.LightGray,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Position = new UIPosition(0, 180),
            Width = 500,
            Height = 200
        };

        mainMenuPanel.AddChild(titleLabel);
        mainMenuPanel.AddChild(joyStickTestButton);
        mainMenuPanel.AddChild(buffListTestButton);
        mainMenuPanel.AddChild(unitInfoTestButton);
        mainMenuPanel.AddChild(returnMenuButton);
        mainMenuPanel.AddChild(instructionLabel);
    }

    /// <summary>
    /// 创建菜单按钮
    /// </summary>
    private static Button CreateMenuButton(string text, float offsetX, float offsetY)
    {
        var button = new Button
        {
            Width = 250,
            Height = 45,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Position = new UIPosition(offsetX, offsetY),
            Background = new GameUI.Brush.SolidColorBrush(Color.FromArgb(200, 70, 130, 180)),
            Margin = new GameUI.Struct.Thickness(5)
        };

        var label = new Label
        {
            Text = text,
            FontSize = 16,
            TextColor = Color.White,
            Bold = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        button.AddChild(label);
        return button;
    }

    /// <summary>
    /// 显示主菜单
    /// </summary>
    private static void ShowMainMenu()
    {
        currentMode = TestModeState.Menu;
        
        if (mainMenuPanel != null)
        {
            mainMenuPanel.AddToRoot();
        }

        // 隐藏返回按钮，显示其他按钮
        if (returnMenuButton != null) returnMenuButton.Visible = false;
        if (joyStickTestButton != null) joyStickTestButton.Visible = true;
        if (buffListTestButton != null) buffListTestButton.Visible = true;
        if (unitInfoTestButton != null) unitInfoTestButton.Visible = true;
        if (instructionLabel != null) instructionLabel.Visible = true;

        Game.Logger?.LogInformation("📋 显示GameUITest主菜单");
    }

    /// <summary>
    /// 切换到摇杆测试
    /// </summary>
    private static void SwitchToJoyStickTest()
    {
        if (currentMode == TestModeState.JoyStickTest) return;

        Game.Logger?.LogInformation("🕹️ 切换到摇杆测试模式");
        
        currentMode = TestModeState.JoyStickTest;
        HideMainMenu();
        
        // 启动摇杆测试
        JoyStickTestExample.InitializeJoyStickTest();
        
        ShowReturnButton();
        UpdateTitle("🕹️ 摇杆测试模式");
    }

    /// <summary>
    /// 切换到Buff列表测试
    /// </summary>
    private static void SwitchToBuffListTest()
    {
        if (currentMode == TestModeState.BuffListTest) return;

        Game.Logger?.LogInformation("🩸 切换到Buff列表测试模式");
        
        currentMode = TestModeState.BuffListTest;
        HideMainMenu();
        
        // 启动Buff列表测试
        BuffListTestExample.InitializeBuffListTest();
        
        ShowReturnButton();
        UpdateTitle("🩸 Buff列表测试模式");
    }

    /// <summary>
    /// 切换到单位信息测试
    /// </summary>
    private static void SwitchToUnitInfoTest()
    {
        if (currentMode == TestModeState.UnitInfoTest) return;

        Game.Logger?.LogInformation("📊 切换到单位信息面板模式");
        
        currentMode = TestModeState.UnitInfoTest;
        HideMainMenu();
        
        // 启动单位信息面板
        UnitInfoPanel.InitializeUnitInfoPanel();
        
        ShowReturnButton();
        UpdateTitle("📊 单位信息面板模式");
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    private static void ReturnToMainMenu()
    {
        Game.Logger?.LogInformation("🔙 返回GameUITest主菜单");

        // 清理当前测试
        CleanupCurrentTest();
        
        // 显示主菜单
        ShowMainMenu();
        UpdateTitle("🎮 游戏UI测试模式 🐱");
    }

    /// <summary>
    /// 隐藏主菜单
    /// </summary>
    private static void HideMainMenu()
    {
        if (joyStickTestButton != null) joyStickTestButton.Visible = false;
        if (buffListTestButton != null) buffListTestButton.Visible = false;
        if (unitInfoTestButton != null) unitInfoTestButton.Visible = false;
        if (instructionLabel != null) instructionLabel.Visible = false;
    }

    /// <summary>
    /// 显示返回按钮
    /// </summary>
    private static void ShowReturnButton()
    {
        if (returnMenuButton != null) returnMenuButton.Visible = true;
    }

    /// <summary>
    /// 更新标题
    /// </summary>
    private static void UpdateTitle(string newTitle)
    {
        if (titleLabel != null)
        {
            titleLabel.Text = newTitle;
        }
    }

    /// <summary>
    /// 清理当前测试
    /// </summary>
    private static void CleanupCurrentTest()
    {
        switch (currentMode)
        {
            case TestModeState.JoyStickTest:
                JoyStickTestExample.Cleanup();
                break;
            case TestModeState.BuffListTest:
                BuffListTestExample.Cleanup();
                break;
            case TestModeState.UnitInfoTest:
                UnitInfoPanel.Cleanup();
                break;
        }
    }

    /// <summary>
    /// 清理所有资源
    /// </summary>
    public static void Cleanup()
    {
        CleanupCurrentTest();
        
        if (mainMenuPanel != null)
        {
            mainMenuPanel.RemoveFromParent();
            mainMenuPanel = null;
        }
        
        titleLabel = null;
        joyStickTestButton = null;
        buffListTestButton = null;
        unitInfoTestButton = null;
        returnMenuButton = null;
        instructionLabel = null;
    }
}
#endif
