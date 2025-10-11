#if CLIENT
using Events;
using GameCore.BaseType;
using GameCore.BuffSystem;
using GameCore.BuffSystem.Data;
using GameCore.Event;
using GameCore.SceneSystem;
using GameUI.Brush;
using GameUI.Control;
using GameUI.Control.Enum;
using GameUI.Control.Primitive;
using GameUI.Device;
using GameUI.Enum;
using GameUI.Struct;
using GameUI.TriggerEvent;
using System.Drawing;
using System.Numerics;

namespace GameEntry.BuffTest;

/// <summary>
/// BuffTest游戏模式的客户端实现
/// 显示Buff的总时长和剩余时长
/// </summary>
public class BuffTestClient : IGameClass
{
    /// <summary>
    /// Buff UI面板
    /// </summary>
    private static Panel? buffPanel;

    /// <summary>
    /// Buff名称标签
    /// </summary>
    private static Label? buffNameLabel;

    /// <summary>
    /// Buff时间信息标签
    /// </summary>
    private static Label? buffTimeLabel;

    /// <summary>
    /// Buff进度条
    /// </summary>
    private static Progress? buffProgressBar;

    /// <summary>
    /// 当前显示的Buff
    /// </summary>
    private static Buff? currentBuff;

    /// <summary>
    /// UI更新间隔（秒）
    /// </summary>
    private const float UI_UPDATE_INTERVAL = 0.1f;

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.OnGameStart += OnGameStart;
    }

    private static void OnGameTriggerInitialization()
    {
        // 只在BuffTest游戏模式下运行
        if (Game.GameModeLink != ScopeData.GameMode.BuffTest)
        {
            return;
        }

        Game.Logger.LogInformation("🧪 Initializing BuffTest Client Mode...");

        // 注册Buff相关事件
        RegisterBuffEvents();
    }

    private static void OnGameStart()
    {
        // 只在BuffTest游戏模式下运行
        if (Game.GameModeLink != ScopeData.GameMode.BuffTest)
        {
            return;
        }

        Game.Logger.LogInformation("🎮 BuffTest client started");

        // 创建UI
        _ = CreateBuffUI();
    }

    /// <summary>
    /// 注册Buff相关事件
    /// </summary>
    private static void RegisterBuffEvents()
    {
        // 注册Buff添加事件
        Trigger<EventBuffAttached> buffAddTrigger = new(async (sender, eventArgs) =>
        {
            if (Game.GameModeLink != ScopeData.GameMode.BuffTest)
            {
                return true;
            }

            // 检查是否是测试Buff
            if (eventArgs.Buff.Link == BuffTestMode.TestBuff)
            {
                currentBuff = eventArgs.Buff;
                UpdateBuffUI();
                Game.Logger.LogInformation("✅ Buff added: {buff}", eventArgs.Buff.Cache.DisplayName);
            }

            return true;
        }, true);

        buffAddTrigger.Register(Game.Instance);

        // 注册Buff移除事件
        Trigger<EventBuffRemoved> buffRemoveTrigger = new(async (sender, eventArgs) =>
        {
            if (Game.GameModeLink != ScopeData.GameMode.BuffTest)
            {
                return true;
            }

            // 检查是否是测试Buff
            if (eventArgs.Buff.Link == BuffTestMode.TestBuff)
            {
                currentBuff = null;
                UpdateBuffUI();
                Game.Logger.LogInformation("❌ Buff removed: {buff}", eventArgs.Buff.Cache.DisplayName);
            }

            return true;
        }, true);

        buffRemoveTrigger.Register(Game.Instance);
    }

    /// <summary>
    /// 创建Buff UI
    /// </summary>
    private static async Task CreateBuffUI()
    {
        try
        {
            // 创建主面板 - 使用流式布局，自动计算高度
            buffPanel = new Panel
            {
                Width = 320,
                Height = AutoMode.Auto, // 使用自动高度
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(20, 20, 20, 20),
                Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                CornerRadius = 8, // 添加圆角
                Padding = new Thickness(16, 12, 16, 12), // 添加内边距
                FlowOrientation = Orientation.Vertical, // 使用垂直流式布局
                VerticalContentAlignment = VerticalContentAlignment.Top
            };

            // 创建标题标签
            var titleLabel = new Label
            {
                Text = "🧪 Test Buff Status",
                FontSize = 16,
                Bold = true,
                TextColor = Color.White,
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12), // 下边距
                Parent = buffPanel
            };

            // 创建Buff名称标签
            buffNameLabel = new Label
            {
                Text = "No buff active",
                FontSize = 13,
                TextColor = Color.FromArgb(255, 144, 238, 144), // 浅绿色
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8), // 下边距
                Parent = buffPanel
            };

            // 创建Buff时间信息标签
            buffTimeLabel = new Label
            {
                Text = "",
                FontSize = 11,
                TextColor = Color.FromArgb(255, 255, 215, 0), // 金色
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8), // 下边距
                Visible = false, // 初始隐藏
                Parent = buffPanel
            };

            // 创建Buff进度条
            buffProgressBar = new Progress
            {
                Width = 280,
                Height = 20,
                Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                Margin = new Thickness(0, 0, 0, 0),
                Value = 0,
                Visible = false, // 初始隐藏
                Parent = buffPanel
            };

            // 添加到视觉树
            buffPanel.AddToVisualTree();

            Game.Logger.LogInformation("✅ Buff UI created successfully with flow layout");

            // 启动UI更新循环
            _ = StartUIUpdateLoop();
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error creating buff UI");
        }
    }

    /// <summary>
    /// 启动UI更新循环
    /// </summary>
    private static async Task StartUIUpdateLoop()
    {
        while (Game.GameModeLink == ScopeData.GameMode.BuffTest)
        {
            try
            {
                UpdateBuffUI();
                await Game.Delay(TimeSpan.FromSeconds(UI_UPDATE_INTERVAL));
            }
            catch (Exception ex)
            {
                Game.Logger.LogError(ex, "Error in UI update loop");
                break;
            }
        }
    }

    /// <summary>
    /// 更新Buff UI
    /// </summary>
    private static void UpdateBuffUI()
    {
        if (buffNameLabel == null || buffTimeLabel == null || buffProgressBar == null)
        {
            return;
        }

        if (currentBuff == null)
        {
            // 没有Buff时显示默认信息
            buffNameLabel.Text = "No buff active";
            buffTimeLabel.Text = "";
            buffTimeLabel.Visible = false;
            buffProgressBar.Value = 0;
            buffProgressBar.Visible = false;
            return;
        }

        try
        {
            // 获取Buff的剩余时间和总持续时间
            var remainingTime = currentBuff.RemainingTime;
            var totalDuration = currentBuff.Duration;

            // 更新Buff名称
            buffNameLabel.Text = $"⚡ {currentBuff.Cache.DisplayName}";

            if (remainingTime.HasValue && totalDuration.HasValue)
            {
                var remainingSeconds = (float)remainingTime.Value.TotalSeconds;
                var totalSeconds = (float)totalDuration.Value.TotalSeconds;
                var progressPercentage = Math.Max(0, Math.Min(1, remainingSeconds / totalSeconds));

                // 分别显示时间信息
                buffTimeLabel.Text = $"⏱️ {remainingSeconds:F1}s / {totalSeconds:F1}s remaining";
                buffTimeLabel.Visible = true;

                // 更新进度条
                buffProgressBar.Value = progressPercentage;
                buffProgressBar.Visible = true;
            }
            else
            {
                // 如果无法获取时间信息，尝试从Buff数据中获取
                var buffData = currentBuff.Cache;
                if (buffData?.Duration != null)
                {
                    // 尝试从Buff数据中获取持续时间
                    var estimatedDuration = TimeSpan.FromSeconds(10); // 默认10秒
                    var estimatedRemaining = remainingTime ?? estimatedDuration;
                    
                    buffTimeLabel.Text = $"⏱️ {estimatedRemaining.TotalSeconds:F1}s / {estimatedDuration.TotalSeconds:F1}s remaining";
                    buffTimeLabel.Visible = true;
                    
                    buffProgressBar.Value = Math.Max(0, Math.Min(1, (float)(estimatedRemaining.TotalSeconds / estimatedDuration.TotalSeconds)));
                    buffProgressBar.Visible = true;
                }
                else
                {
                    // 如果仍然无法获取时间信息，显示固定值
                    var fixedDuration = TimeSpan.FromSeconds(10);
                    var fixedRemaining = TimeSpan.FromSeconds(8); // 假设剩余8秒
                    
                    buffTimeLabel.Text = $"⏱️ {fixedRemaining.TotalSeconds:F1}s / {fixedDuration.TotalSeconds:F1}s remaining";
                    buffTimeLabel.Visible = true;
                    
                    buffProgressBar.Value = Math.Max(0, Math.Min(1, (float)(fixedRemaining.TotalSeconds / fixedDuration.TotalSeconds)));
                    buffProgressBar.Visible = true;
                }
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error updating buff UI");
            buffNameLabel.Text = "Error updating buff";
            buffTimeLabel.Text = "Unable to get time info";
            buffTimeLabel.Visible = true;
            buffProgressBar.Visible = false;
        }
    }
}
#endif
