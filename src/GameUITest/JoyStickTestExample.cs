#if CLIENT
using GameCore.GameSystem.Data;
using GameUI.Control.Primitive;
using GameSystemUI.AbilitySystemUI.Advanced;
using GameCore.EntitySystem;
using GameCore;
using GameCore.Platform.SDL;
using GameUI.Struct;
using GameUI.Enum;
using System.Threading.Tasks;
using GameUI.Control.Extensions;
using GameCore.PlayerAndUsers;

namespace GameEntry.GameUITest;

/// <summary>
/// 摇杆测试示例 - 基于AbilityJoyStickGroup组件
/// 演示如何在GameUITest模式下测试技能摇杆功能
/// </summary>
public class JoyStickTestExample : IGameClass
{
    private static Label? statusLabel;
    private static AbilityJoyStickGroup? joyStickGroup;
    private static Panel? mainPanel;

    public static void OnRegisterGameClass()
    {
        // 在GameUITest模式下注册摇杆测试
        // 注册到Game初始化事件，让主测试类来控制
        Game.OnGameDataInitialization += () =>
        {
            // 这个类由GameUITestMode统一管理，不直接注册UI初始化
        };
    }

    /// <summary>
    /// 初始化摇杆测试界面
    /// </summary>
    public static void InitializeJoyStickTest()
    {
        Game.Logger?.LogInformation("🎮 初始化摇杆测试界面...");

        try
        {
            CreateMainPanel();
            CreateStatusLabel();
            CreateJoyStickGroup();
            SetupJoyStickBinding();

            Game.Logger?.LogInformation("✅ 摇杆测试界面初始化完成喵！");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 摇杆测试界面初始化失败: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 创建主面板
    /// </summary>
    private static void CreateMainPanel()
    {
        mainPanel = new Panel
        {
            WidthStretchRatio = 1.0f,
            HeightStretchRatio = 1.0f,
            // Background = new GameUI.Brush.SolidColorBrush(System.Drawing.Color.FromArgb(50, 0, 0, 0)) // 半透明背景
        };

        mainPanel.AddToRoot();
    }

    /// <summary>
    /// 创建状态标签
    /// </summary>
    private static void CreateStatusLabel()
    {
        statusLabel = new Label
        {
            Text = "🎮 摇杆测试模式\n\n等待绑定单位...",
            FontSize = 18,
            TextColor = System.Drawing.Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Position = new UIPosition(50, 50),
            Width = 400,
            Height = 200
        };

        mainPanel?.AddChild(statusLabel);
    }

    /// <summary>
    /// 创建摇杆组
    /// </summary>
    private static void CreateJoyStickGroup()
    {
        joyStickGroup = new AbilityJoyStickGroup
        {
        };

        mainPanel?.AddChild(joyStickGroup);
    }

    /// <summary>
    /// 设置摇杆绑定
    /// </summary>
    private static void SetupJoyStickBinding()
    {
        try
        {
            // 直接获取并绑定主控单位，不使用异步检查
            var currentMainUnit = Player.LocalPlayer?.MainUnit;
            OnMainUnitChanged(currentMainUnit);
            
            Game.Logger?.LogInformation("🔍 BuffBar初始绑定完成");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "⚠️ 初始绑定主控单位时出错: {message}", ex.Message);
        }
    }


    /// <summary>
    /// 主控单位变化处理
    /// </summary>
    private static void OnMainUnitChanged(Unit? unit)
    {
        try
        {
            if (joyStickGroup != null)
            {
                joyStickGroup.BindUnit = unit;
                
                var unitData = unit?.GetType().Name ?? "未知单位";
                var unitId = unit?.GetHashCode() ?? 0;
                
                if (statusLabel != null)
                {
                    statusLabel.Text = $"🎮 摇杆测试模式\n\n✅ 已绑定单位: {unitData}\n" +
                                     $"🎯 单位ID: {unitId}\n" +
                                     $"⚡ 技能系统: 已连接\n\n" +
                                     "📋 操作说明:\n" +
                                     "• 点击摇杆按钮释放技能\n" +
                                     "• 拖拽摇杆指定方向\n" +
                                     "• 键盘1-6键快速释放";
                }

                Game.Logger?.LogInformation("🎮 摇杆已绑定到单位: {unitName} (ID: {unitId})", unitData, unitId);
            }
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 绑定摇杆到单位时出错: {message}", ex.Message);
            
            if (statusLabel != null)
            {
                statusLabel.Text = $"🎮 摇杆测试模式\n\n❌ 绑定失败: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public static void Cleanup()
    {
        if (mainPanel != null)
        {
            mainPanel.RemoveFromParent();
            mainPanel = null;
        }
        
        statusLabel = null;
        joyStickGroup = null;
    }
}
#endif
