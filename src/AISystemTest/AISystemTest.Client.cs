#if CLIENT
using Events;
using GameCore.Event;
using GameCore.BaseType;
using GameCore.OrderSystem;
using GameCore.PlayerAndUsers;
using GameCore.Components;
using GameUI.Brush;
using GameUI.Control;
using GameUI.Control.Advanced;
using GameUI.Control.Enum;  // 添加这个using来使用AutoMode
using GameUI.Control.Primitive;
using GameUI.Control.Struct;
using GameUI.Device;
using GameUI.Enum;
using GameUI.Struct;
using System.Drawing;
using System.Numerics;

namespace GameEntry.AISystemTest;

/// <summary>
/// AI系统测试游戏模式的客户端UI实现
/// 显示AI系统状态、测试进度和实时信息
/// </summary>
internal class AISystemTestClient : IGameClass
{
    #region Fields

    private static Trigger<EventGameStart>? gameStartTrigger;
    private static Panel? mainDashboard;
    private static Label? titleLabel;
    private static Label? gameTimeLabel;
    private static Label? testPhaseLabel;
    private static Label? waveAIStatusLabel;
    private static Label? unitCountLabel;
    private static Label? positionLabel;
    private static Label? instructionsLabel;
    private static Label? currentBehaviorLabel; // 新增：当前阶段行为说明
    private static Panel? statusPanel;
    private static JoystickNormal? movementJoystick;
    private static bool isJoystickMoving = false;
    
    // 手动触发按钮
    private static Button? startTestButton;
    private static Button? startValidationButton;
    private static Button? resetTestButton;
    private static Button? nextPhaseButton;
    
    // AI战斗测试按钮
    private static Button? startCombatButton;
    private static Button? stopCombatButton;
    
    // 服务端状态同步
    private static AITestStatusInfo? serverStatus;

    #endregion

    #region Initialization

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.OnGameUIInitialization += OnGameUIInitialization;
        Game.Logger.LogInformation("🖥️ AI System Test Client registered");
    }

    private static void OnGameUIInitialization()
    {
        if(Game.GameModeLink != ScopeData.GameMode.AISystemTest )
        {
            return;
        }

    }

    private static void OnGameTriggerInitialization()
    {
        // 只在AI测试模式下初始化
        if (Game.GameModeLink != ScopeData.GameMode.AISystemTest)
        {
            return;
        }

        gameStartTrigger = new Trigger<EventGameStart>(OnGameStartAsync, true);
        gameStartTrigger.Register(Game.Instance);
        Game.Logger.LogInformation("🎯 AI System Test client triggers initialized");
    }

    #endregion

    #region Game Start

    private static async Task<bool> OnGameStartAsync(object sender, EventGameStart eventArgs)
    {
        Game.Logger.LogInformation("🖥️ AI System Test Client UI Started!");

        // 初始化UI仪表板
        InitializeUIDashboard();

        // 初始化摇杆控制
        InitializeJoystickControl();

        // 启动定期更新
        _ = UpdateUILoop();

        Game.Logger.LogInformation("✅ AI System Test client UI initialization complete");
        return true;
    }

    #endregion

    #region UI Creation

    private static void InitializeUIDashboard()
    {
        try
        {
            // 创建主面板 - 充分利用屏幕空间
            mainDashboard = new Panel()
            {
                Width = 520,  // 增加宽度，充分利用屏幕空间
                Height = AutoMode.Auto,  // 使用自动高度适应内容
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 20, 0),
                Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),  // 稍微增加不透明度
                CornerRadius = 12,
                Padding = new Thickness(16, 16, 16, 16),  // 添加内边距
                FlowOrientation = Orientation.Vertical,   // 使用垂直流式布局
                VerticalContentAlignment = VerticalContentAlignment.Top
            };

            // 创建标题
            titleLabel = new Label()
            {
                Text = "🤖 AI System Test Dashboard - AITestScene",
                FontSize = 20,  // 增大字体
                TextColor = new SolidColorBrush(Color.FromArgb(255, 0, 200, 255)),  // 更亮的青色
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12),  // 只设置底边距
                Parent = mainDashboard
            };

            // 创建基础状态面板 - 紧凑的关键信息
            var basicStatusPanel = new Panel()
            {
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                CornerRadius = 8,
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 0, 8),
                FlowOrientation = Orientation.Horizontal,  // 水平布局关键信息
                HorizontalContentAlignment = HorizontalContentAlignment.UniformSpacing,
                Parent = mainDashboard
            };

            // 创建详细状态面板 - 垂直布局的详细信息
            statusPanel = new Panel()
            {
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                Background = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                CornerRadius = 8,
                Padding = new Thickness(12, 12, 12, 12),
                Margin = new Thickness(0, 0, 0, 8),
                FlowOrientation = Orientation.Vertical,
                VerticalContentAlignment = VerticalContentAlignment.Top,
                Parent = mainDashboard
            };

            // 游戏时间标签 - 放在基础状态面板
            gameTimeLabel = new Label()
            {
                Text = "⏱️ 0:00",
                FontSize = 13,
                TextColor = new SolidColorBrush(Color.White),
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Parent = basicStatusPanel
            };

            // 测试阶段标签 - 放在基础状态面板
            testPhaseLabel = new Label()
            {
                Text = "🎬 Ready",
                FontSize = 13,
                TextColor = new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)),  // 金黄色
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Parent = basicStatusPanel
            };

            // WaveAI状态标签 - 详细状态面板
            waveAIStatusLabel = new Label()
            {
                Text = "🌊 WaveAI: Not initialized",
                FontSize = 14,
                TextColor = new SolidColorBrush(Color.FromArgb(255, 144, 238, 144)),  // 浅绿色
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                HorizontalContentAlignment = HorizontalContentAlignment.Left,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4),  // 只设置底边距
                Parent = statusPanel
            };

            // 单位数量标签 - 放在基础状态面板
            unitCountLabel = new Label()
            {
                Text = "👥 0 Units",
                FontSize = 13,
                TextColor = new SolidColorBrush(Color.FromArgb(255, 255, 165, 0)),  // 橙色
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Parent = basicStatusPanel
            };

            // 位置标签 - 详细状态面板
            positionLabel = new Label()
            {
                Text = "📍 Position: (0, 0)",
                FontSize = 14,
                TextColor = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)),  // 青色
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                HorizontalContentAlignment = HorizontalContentAlignment.Left,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8),  // 底边距
                Parent = statusPanel
            };

            // 创建说明面板 - 专门用于操作说明
            var instructionsPanel = new Panel()
            {
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                Background = new SolidColorBrush(Color.FromArgb(80, 128, 128, 128)),
                CornerRadius = 6,
                Padding = new Thickness(10, 8, 10, 8),
                Margin = new Thickness(0, 0, 0, 8),
                Parent = mainDashboard
            };

            // 说明标签 - 简化的操作说明
            instructionsLabel = new Label()
            {
                Text = "📋 Controls: Use joystick to move • Move to (2000,2000) for AI combat area • Click 'Start Combat' to test Default AI",
                FontSize = 12,
                TextColor = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220)),
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                HorizontalContentAlignment = HorizontalContentAlignment.Left,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Parent = instructionsPanel
            };

            // 创建行为说明面板 - 专门用于AI行为指导
            var behaviorPanel = new Panel()
            {
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                Background = new SolidColorBrush(Color.FromArgb(120, 255, 215, 0)),  // 金黄色背景
                CornerRadius = 8,
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 8),
                Parent = mainDashboard
            };

            // 当前阶段行为说明标签 - 重要的新功能
            currentBehaviorLabel = new Label()
            {
                Text = "🎯 Expected Behavior: Ready to start testing\n" +
                       "Click 'Start Test' to begin the AI behavior demonstration.",
                FontSize = 13,
                TextColor = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40)), // 深色文字在金色背景上
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                HorizontalContentAlignment = HorizontalContentAlignment.Left,
                VerticalContentAlignment = VerticalContentAlignment.Top,
                Parent = behaviorPanel
            };

            // 创建按钮面板 - 使用流式布局
            var buttonPanel = new Panel()
            {
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
                CornerRadius = 8,
                Padding = new Thickness(12, 10, 12, 10),
                FlowOrientation = Orientation.Vertical,  // 垂直流式布局
                VerticalContentAlignment = VerticalContentAlignment.Top,
                Parent = mainDashboard
            };

            // 第一行按钮组 - 水平排列
            var firstRowPanel = new Panel()
            {
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                FlowOrientation = Orientation.Horizontal,  // 水平流式布局
                HorizontalContentAlignment = HorizontalContentAlignment.UniformSpacing,
                Margin = new Thickness(0, 0, 0, 8),  // 与第二行的间距
                Parent = buttonPanel
            };

            // 第一行按钮 - 开始测试按钮
            startTestButton = new Button()
            {
                Width = 110,  // 稍微增加宽度
                Height = 36,
                Background = new SolidColorBrush(Color.FromArgb(200, 0, 120, 215)),  // 增加不透明度
                CornerRadius = 6,
                Margin = new Thickness(0, 0, 8, 0),  // 右边距
                Parent = firstRowPanel
            };
            
            // 添加按钮文本标签
            var startTestLabel = new Label()
            {
                Text = "🎬 Start Test",
                FontSize = 12,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Parent = startTestButton
            };
            
            startTestButton.OnPointerClicked += OnStartTestButtonClick;

            // 第一行按钮 - 开始验证按钮
            startValidationButton = new Button()
            {
                Width = 100,
                Height = 36,
                Background = new SolidColorBrush(Color.FromArgb(200, 0, 150, 0)),  // 绿色
                CornerRadius = 6,
                Margin = new Thickness(0, 0, 8, 0),  // 右边距
                Parent = firstRowPanel
            };
            
            // 添加验证按钮文本标签
            var validationLabel = new Label()
            {
                Text = "🔍 Validate",
                FontSize = 12,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Parent = startValidationButton
            };
            
            startValidationButton.OnPointerClicked += OnStartValidationButtonClick;

            // 第一行按钮 - 重置按钮
            resetTestButton = new Button()
            {
                Width = 90,
                Height = 36,
                Background = new SolidColorBrush(Color.FromArgb(200, 180, 0, 0)),  // 红色
                CornerRadius = 6,
                Parent = firstRowPanel  // 最后一个按钮不需要右边距
            };
            
            // 添加重置按钮文本标签
            var resetLabel = new Label()
            {
                Text = "🔄 Reset",
                FontSize = 12,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Parent = resetTestButton
            };
            
            resetTestButton.OnPointerClicked += OnResetTestButtonClick;

            // 第二行按钮组 - AI战斗测试按钮
            var secondRowPanel = new Panel()
            {
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                FlowOrientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalContentAlignment.UniformSpacing,
                Margin = new Thickness(0, 0, 0, 8),  // 与第三行的间距
                Parent = buttonPanel
            };

            // 第二行按钮 - 开始AI战斗测试按钮
            startCombatButton = new Button()
            {
                Width = 120,
                Height = 36,
                Background = new SolidColorBrush(Color.FromArgb(200, 138, 43, 226)),  // 紫色，突出AI战斗
                CornerRadius = 6,
                Margin = new Thickness(0, 0, 8, 0),  // 右边距
                Parent = secondRowPanel
            };
            
            // 添加开始战斗按钮文本标签
            var startCombatLabel = new Label()
            {
                Text = "⚔️ Start Combat",
                FontSize = 12,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Parent = startCombatButton
            };
            
            startCombatButton.OnPointerClicked += OnStartCombatButtonClick;

            // 第二行按钮 - 停止AI战斗测试按钮
            stopCombatButton = new Button()
            {
                Width = 120,
                Height = 36,
                Background = new SolidColorBrush(Color.FromArgb(200, 220, 20, 60)),  // 深红色
                CornerRadius = 6,
                Parent = secondRowPanel
            };
            
            // 添加停止战斗按钮文本标签
            var stopCombatLabel = new Label()
            {
                Text = "🛑 Stop Combat",
                FontSize = 12,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Parent = stopCombatButton
            };
            
            stopCombatButton.OnPointerClicked += OnStopCombatButtonClick;

            // 第三行按钮组 - 下一阶段按钮
            var thirdRowPanel = new Panel()
            {
                Width = AutoMode.Auto,
                Height = AutoMode.Auto,
                FlowOrientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                Parent = buttonPanel
            };

            // 第三行按钮 - 下一阶段按钮
            nextPhaseButton = new Button()
            {
                Width = 130,  // 稍微增加宽度，因为它是单独一行
                Height = 38,   // 稍微增加高度，突出重要性
                Background = new SolidColorBrush(Color.FromArgb(200, 255, 140, 0)),  // 橙色，突出显示
                CornerRadius = 6,
                Parent = thirdRowPanel
            };
            
            // 添加下一阶段按钮文本标签
            var nextPhaseLabel = new Label()
            {
                Text = "⏭️ Next Phase",
                FontSize = 12,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                Parent = nextPhaseButton
            };
            
            nextPhaseButton.OnPointerClicked += OnNextPhaseButtonClick;

            // 添加主面板到UI根
            UIRoot.Instance.AddChild(mainDashboard);

            Game.Logger.LogInformation("🎨 AI Test Dashboard UI created successfully");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to create AI Test Dashboard UI");
        }
    }

    #endregion

    #region UI Updates

    private static async Task UpdateUILoop()
    {
        while (true)
        {
            try
            {
                await Game.Delay(TimeSpan.FromSeconds(1)); // 每秒更新一次
                UpdateDashboardInfo();
            }
            catch (Exception ex)
            {
                Game.Logger.LogError(ex, "❌ Error in UI update loop");
                await Game.Delay(TimeSpan.FromSeconds(5)); // 发生错误时等待5秒再重试
            }
        }
    }

    private static void UpdateDashboardInfo()
    {
        try
        {
            // 更新游戏时间 - 简化显示
            if (gameTimeLabel != null)
            {
                var elapsed = Game.ElapsedTime;
                gameTimeLabel.Text = $"⏱️ {elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            }

            // 更新测试阶段 - 优先使用服务端状态，回退到客户端推断
            if (testPhaseLabel != null)
            {
                if (serverStatus.HasValue)
                {
                    var phaseName = GetPhaseNameFromStatus(serverStatus.Value);
                    testPhaseLabel.Text = $"🎬 {phaseName}";  // 简化显示
                }
                else
                {
                    testPhaseLabel.Text = "🎬 Ready";
                }
            }

            // 更新WaveAI状态 - 优先使用服务端状态
            if (waveAIStatusLabel != null)
            {
                if (serverStatus.HasValue)
                {
                    var waveAIStatus = GetWaveAIStatusFromPhase(serverStatus.Value.CurrentPhase);
                    waveAIStatusLabel.Text = $"🌊 WaveAI: {waveAIStatus}";
                }
                else
                {
                    waveAIStatusLabel.Text = "🌊 WaveAI: Awaiting server status";
                }
            }

            // 更新单位数量 - 优先使用服务端状态
            if (unitCountLabel != null)
            {
                if (serverStatus.HasValue)
                {
                    unitCountLabel.Text = $"👥 {serverStatus.Value.UnitCount} Units";  // 简化显示
                }
                else
                {
                    unitCountLabel.Text = "👥 Awaiting...";
                }
            }

            // 更新位置信息
            if (positionLabel != null)
            {
                var localPlayer = Player.LocalPlayer;
                var mainUnit = localPlayer?.MainUnit;
                if (mainUnit != null && mainUnit.IsValid)
                {
                    var pos = mainUnit.Position.Vector3;
                    var distance = Vector3.Distance(pos, new Vector3(1000, 1000, 0));
                    positionLabel.Text = $"📍 Position: ({pos.X:F0}, {pos.Y:F0}) | Distance to AI area: {distance:F0}";
                }
                else
                {
                    positionLabel.Text = "📍 Position: No unit";
                }
            }

            // 更新当前测试阶段行为说明
            UpdateCurrentBehaviorDescription();

            // 更新下一阶段按钮状态 - 客户端显示
            if (nextPhaseButton != null)
            {
                // 客户端始终显示按钮，但默认禁用状态
                nextPhaseButton.Visible = true;
                nextPhaseButton.Disabled = true; // 默认禁用，等待服务端激活
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error updating dashboard info");
        }
    }

    /// <summary>
    /// 更新当前测试阶段的行为说明
    /// </summary>
    private static void UpdateCurrentBehaviorDescription()
    {
        if (currentBehaviorLabel == null) return;

        try
        {
            string behaviorText;
            
            // 优先使用服务端状态生成行为说明
            if (serverStatus.HasValue)
            {
                behaviorText = GetBehaviorDescriptionFromServerStatus(serverStatus.Value);
            }
            else
            {
                // 回退到基于客户端时间的推断
                var elapsed = Game.ElapsedTime;
                var totalSeconds = elapsed.TotalSeconds;
                behaviorText = GetBehaviorDescriptionForTime(totalSeconds);
            }
            
            // 更新标签文本
            currentBehaviorLabel.Text = behaviorText;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error updating behavior description");
        }
    }

    /// <summary>
    /// 根据服务端状态生成阶段名称
    /// </summary>
    private static string GetPhaseNameFromStatus(AITestStatusInfo status)
    {
        return status.CurrentPhase switch
        {
            0 => "Ready for Testing",
            1 => "Guard Behavior Test",
            2 => "Move Behavior Test", 
            3 => "Patrol Behavior Test",
            4 => "Formation Behavior Test",
            5 => "Command System Test",
            6 => "All Tests Completed",
            _ => $"Phase {status.CurrentPhase}"
        };
    }

    /// <summary>
    /// 根据测试阶段生成WaveAI状态描述
    /// </summary>
    private static string GetWaveAIStatusFromPhase(int phase)
    {
        return phase switch
        {
            0 => "Not initialized",
            1 => "Guard Type - Active",
            2 => "Move Type - Active",
            3 => "Patrol Type - Active", 
            4 => "Formation Type - Active",
            5 => "Disabled for Command Test",
            6 => "Tests Completed",
            _ => $"Phase {phase} Active"
        };
    }

    /// <summary>
    /// 根据服务端状态生成行为描述
    /// </summary>
    private static string GetBehaviorDescriptionFromServerStatus(AITestStatusInfo status)
    {
        return status.CurrentPhase switch
        {
            0 => "🎯 Expected Behavior: Initialization Phase\n" +
                 "• 5 test units should spawn at (4000, 4000) area\n" +
                 "• Units should be created with proper AI components\n" +
                 "• Leader unit should be identifiable\n" +
                 "• All units should have valid Player assignments\n" +
                 "Ready to start testing - Click 'Start Test' button!",
                 
            1 => "🛡️ Current Test: Guard Behavior (Phase 1)\n" +
                 "• Units should stay within LEASH range of target (600 units)\n" +
                 "• When target moves, units should follow and reposition\n" +
                 "• Units should maintain protective formation around target\n" +
                 "• Movement should be smooth and coordinated\n" +
                 "• Watch for: Units not straying too far from guard point",
                 
            2 => "🏃 Current Test: Move Behavior (Phase 2)\n" +
                 "• Units should move toward target and stop at approach range (200 units)\n" +
                 "• Control should switch between WaveAI and AIThinkTree smoothly\n" +
                 "• Units should reach target with precision positioning\n" +
                 "• Watch for: Oscillation prevention and smooth handoffs\n" +
                 "• Movement should be direct and efficient",
                 
            3 => "🔄 Current Test: Patrol Behavior (Phase 3)\n" +
                 "• Units should patrol between OriginTarget and WaveTarget\n" +
                 "• Movement pattern should be back-and-forth between two points\n" +
                 "• Units should turn around when reaching either endpoint\n" +
                 "• Watch for: Consistent patrol rhythm and proper waypoint handling\n" +
                 "• Path should adapt to terrain and obstacles",
                 
            4 => "📐 Current Test: Formation Behavior (Phase 4)\n" +
                 "• Units should maintain formation during long-distance movement\n" +
                 "• Formation should be preserved through turns and obstacles\n" +
                 "• Units should spread out in organized pattern around leader\n" +
                 "• Watch for: Formation cohesion and collision avoidance\n" +
                 "• Formation should adapt to different movement speeds",
                 
            5 => "⚡ Current Test: Command System (Phase 5)\n" +
                 "• Direct AI commands should execute properly with IsAI flag\n" +
                 "• Units should respond to individual movement orders\n" +
                 "• Command queue integration should work seamlessly\n" +
                 "• Watch for: Proper Player validation and command execution\n" +
                 "• Commands should not conflict with AI decision making",
                 
            6 => "🎉 All Tests Completed!\n" +
                 "• Guard behavior: Units stayed within leash range ✓\n" +
                 "• Move behavior: Precise positioning and control handoffs ✓\n" +
                 "• Patrol behavior: Consistent back-and-forth movement ✓\n" +
                 "• Formation behavior: Maintained cohesion during movement ✓\n" +
                 "• Command system: Proper AI command integration ✓\n" +
                 "Click 'Reset' to run tests again or review the logs.",
                 
            _ => $"🔧 Test Phase {status.CurrentPhase}\n" +
                 $"• Running for {status.ElapsedSeconds:F1} seconds\n" +
                 $"• Units: {status.UnitCount}\n" +
                 $"• Status: {(status.IsTestRunning ? "Running" : "Stopped")}\n" +
                 "Monitoring AI behavior..."
        };
    }

    /// <summary>
    /// 从服务端更新状态信息（由协议处理器调用）
    /// </summary>
    public static void UpdateStatusFromServer(AITestStatusInfo statusInfo)
    {
        try
        {
            serverStatus = statusInfo;
            Game.Logger.LogDebug("📊 Updated AI test status from server: Phase {Phase}, Running: {Running}", 
                statusInfo.CurrentPhase, statusInfo.IsTestRunning);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error updating status from server");
        }
    }

    /// <summary>
    /// 根据游戏时间获取对应阶段的行为说明
    /// </summary>
    private static string GetBehaviorDescriptionForTime(double totalSeconds)
    {
        // 根据测试序列的时间安排判断当前阶段
        if (totalSeconds < 30) // 初始化阶段
        {
            return "🎯 Expected Behavior: Initialization Phase\n" +
                   "• 5 test units should spawn at (4000, 4000) area\n" +
                   "• Units should be created with proper AI components\n" +
                   "• Leader unit should be identifiable\n" +
                   "• All units should have valid Player assignments\n" +
                   "Ready to start testing - Click 'Start Test' button!";
        }
        else if (totalSeconds < 45) // Guard行为测试阶段
        {
            return "🛡️ Current Test: Guard Behavior (Phase 1)\n" +
                   "• Units should stay within LEASH range of target (600 units)\n" +
                   "• When target moves, units should follow and reposition\n" +
                   "• Units should maintain protective formation around target\n" +
                   "• Movement should be smooth and coordinated\n" +
                   "• Watch for: Units not straying too far from guard point";
        }
        else if (totalSeconds < 65) // Move行为测试阶段
        {
            return "🏃 Current Test: Move Behavior (Phase 2)\n" +
                   "• Units should move toward target and stop at approach range (200 units)\n" +
                   "• Control should switch between WaveAI and AIThinkTree smoothly\n" +
                   "• Units should reach target with precision positioning\n" +
                   "• Watch for: Oscillation prevention and smooth handoffs\n" +
                   "• Movement should be direct and efficient";
        }
        else if (totalSeconds < 90) // Patrol行为测试阶段
        {
            return "🔄 Current Test: Patrol Behavior (Phase 3)\n" +
                   "• Units should patrol between OriginTarget and WaveTarget\n" +
                   "• Movement pattern should be back-and-forth between two points\n" +
                   "• Units should turn around when reaching either endpoint\n" +
                   "• Watch for: Consistent patrol rhythm and proper waypoint handling\n" +
                   "• Path should adapt to terrain and obstacles";
        }
        else if (totalSeconds < 115) // Formation编队测试阶段
        {
            return "📐 Current Test: Formation Behavior (Phase 4)\n" +
                   "• Units should maintain formation during long-distance movement\n" +
                   "• Formation should be preserved through turns and obstacles\n" +
                   "• Units should spread out in organized pattern around leader\n" +
                   "• Watch for: Formation cohesion and collision avoidance\n" +
                   "• Formation should adapt to different movement speeds";
        }
        else if (totalSeconds < 140) // Command系统测试阶段
        {
            return "⚡ Current Test: Command System (Phase 5)\n" +
                   "• Direct AI commands should execute properly with IsAI flag\n" +
                   "• Units should respond to individual movement orders\n" +
                   "• Command queue integration should work seamlessly\n" +
                   "• Watch for: Proper Player validation and command execution\n" +
                   "• Commands should not conflict with AI decision making";
        }
        else // 测试完成阶段
        {
            return "🎉 All Tests Completed!\n" +
                   "• Guard behavior: Units stayed within leash range ✓\n" +
                   "• Move behavior: Precise positioning and control handoffs ✓\n" +
                   "• Patrol behavior: Consistent back-and-forth movement ✓\n" +
                   "• Formation behavior: Maintained cohesion during movement ✓\n" +
                   "• Command system: Proper AI command integration ✓\n" +
                   "Click 'Reset' to run tests again or review the logs.";
        }
    }

    #endregion

    #region Joystick Control

    private static void InitializeJoystickControl()
    {
        try
        {
            // 确保只在AI测试模式下创建摇杆
            if (Game.GameModeLink != ScopeData.GameMode.AISystemTest)
            {
                Game.Logger.LogDebug("🚫 Skipping joystick creation - not in AI test mode");
                return;
            }

            // 创建移动摇杆 - 使用官方安全区API动态计算位置
            var safeZone = ScreenViewport.Primary.SafeZonePadding;
            var joystickMargin = new Thickness(
                Math.Max(30, safeZone.Left + 20),     // 左边距：安全区 + 额外边距，最小30
                0,                                     // 顶边距：不需要
                0,                                     // 右边距：不需要  
                Math.Max(30, safeZone.Bottom + 20)    // 底边距：安全区 + 额外边距，最小30
            );
            
            Game.Logger.LogInformation("🛡️ Device SafeZone - Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}", 
                safeZone.Left, safeZone.Top, safeZone.Right, safeZone.Bottom);
            Game.Logger.LogInformation("🕹️ Joystick Margin - Left: {Left}, Bottom: {Bottom}", 
                joystickMargin.Left, joystickMargin.Bottom);
            
            movementJoystick = new JoystickNormal()
            {
                Width = 140,
                Height = 140,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = joystickMargin,
                Radius = 60f,
                KnobSize = 25f,
                IsEnabled = true
            };

            // 自定义摇杆外观
            movementJoystick.SetBackgroundStyle(new SolidColorBrush(Color.FromArgb(150, 80, 80, 80)));
            movementJoystick.SetKnobStyle(new SolidColorBrush(Color.FromArgb(220, 220, 220, 220)));

            // 注册摇杆事件
            movementJoystick.ValueChanged += OnJoystickValueChanged;
            movementJoystick.DragStarted += OnJoystickDragStarted;
            movementJoystick.DragEnded += OnJoystickDragEnded;

            // 添加摇杆到UI根
            UIRoot.Instance.AddChild(movementJoystick);

            Game.Logger.LogInformation("🕹️ Movement joystick initialized");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to initialize joystick control");
        }
    }

    private static void OnJoystickValueChanged(object? sender, JoystickValueChangedEventArgs e)
    {
        try
        {
            var inputValue = e.NewValue;
            
            // 死区处理
            if (inputValue.Length() < 0.1f)
            {
                if (isJoystickMoving)
                {
                    SendStopMovementCommand();
                }
                return;
            }

            // 计算移动角度
            var angle = Math.Atan2(inputValue.Y, inputValue.X) * (180.0 / Math.PI);
            SendMovementCommand((float)angle);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling joystick value change");
        }
    }

    private static void OnJoystickDragStarted(object? sender, EventArgs e)
    {
        Game.Logger.LogDebug("🕹️ Joystick drag started");
    }

    private static void OnJoystickDragEnded(object? sender, EventArgs e)
    {
        Game.Logger.LogDebug("🕹️ Joystick drag ended");
        if (isJoystickMoving)
        {
            SendStopMovementCommand();
        }
    }

    private static void SendMovementCommand(float angle)
    {
        try
        {
            var localPlayer = Player.LocalPlayer;
            var mainUnit = localPlayer?.MainUnit;
            
            if (mainUnit == null)
            {
                Game.Logger.LogWarning("⚠️ No main unit available for movement");
                return;
            }

            var command = new Command
            {
                Index = CommandIndex.VectorMove,
                Type = ComponentTag.Walkable,
                Target = new Angle(angle),
                Player = localPlayer,
                Flag = CommandFlag.IsUser
            };

            var result = command.IssueOrder(mainUnit);
            if (result.IsSuccess)
            {
                isJoystickMoving = true;
            }
            else
            {
                Game.Logger.LogWarning("⚠️ Failed to issue movement command: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error sending movement command");
        }
    }

    private static void SendStopMovementCommand()
    {
        try
        {
            var localPlayer = Player.LocalPlayer;
            var mainUnit = localPlayer?.MainUnit;
            
            if (mainUnit == null)
            {
                return;
            }

            var command = new Command
            {
                Index = CommandIndex.VectorMoveStop,
                Type = ComponentTag.Walkable,
                Player = localPlayer,
                Flag = CommandFlag.IsUser
            };

            command.IssueOrder(mainUnit);
            isJoystickMoving = false;
            
            Game.Logger.LogDebug("🛑 Movement stopped");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error stopping movement");
        }
    }

    #endregion

    #region Button Events

    /// <summary>
    /// 开始测试按钮点击事件
    /// </summary>
    private static async void OnStartTestButtonClick(object sender, PointerEventArgs e)
    {
        try
        {
            Game.Logger.LogInformation("🎬 Start Test button clicked");
#if SERVER
            await AISystemTestServer.StartAITestSequence();
#else
            Game.Logger.LogInformation("📡 Sending start test command to server...");
            bool success = AITestCommandSender.SendCommand(AITestCommandType.StartTest);
            if (success)
            {
                Game.Logger.LogInformation("✅ Start test command sent successfully");
            }
            else
            {
                Game.Logger.LogError("❌ Failed to send start test command");
            }
#endif
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error starting AI test");
        }
    }

    /// <summary>
    /// 开始验证按钮点击事件
    /// </summary>
    private static async void OnStartValidationButtonClick(object sender, PointerEventArgs e)
    {
        try
        {
            Game.Logger.LogInformation("🔍 Start Validation button clicked");
#if SERVER
            await AISystemTestServer.StartAIValidation();
#else
            Game.Logger.LogInformation("📡 Sending start validation command to server...");
            bool success = AITestCommandSender.SendCommand(AITestCommandType.StartValidation);
            if (success)
            {
                Game.Logger.LogInformation("✅ Start validation command sent successfully");
            }
            else
            {
                Game.Logger.LogError("❌ Failed to send start validation command");
            }
#endif
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error starting AI validation");
        }
    }

    /// <summary>
    /// 重置按钮点击事件
    /// </summary>
    private static async void OnResetTestButtonClick(object sender, PointerEventArgs e)
    {
        try
        {
            Game.Logger.LogInformation("🔄 Reset Test button clicked");
#if SERVER
            AISystemTestServer.ResetTestState();
#else
            Game.Logger.LogInformation("📡 Sending reset command to server...");
            bool success = AITestCommandSender.SendCommand(AITestCommandType.Reset);
            if (success)
            {
                Game.Logger.LogInformation("✅ Reset command sent successfully");
            }
            else
            {
                Game.Logger.LogError("❌ Failed to send reset command");
            }
#endif
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error resetting AI test");
        }
    }

    /// <summary>
    /// 开始AI战斗测试按钮点击事件
    /// </summary>
    private static async void OnStartCombatButtonClick(object sender, PointerEventArgs e)
    {
        try
        {
            Game.Logger.LogInformation("⚔️ Start Combat button clicked");
#if SERVER
            await AISystemTestServer.StartAICombatTestCommand();
#else
            Game.Logger.LogInformation("📡 Sending start combat command to server...");
            bool success = AITestCommandSender.SendCommand(AITestCommandType.StartCombat);
            if (success)
            {
                Game.Logger.LogInformation("✅ Start combat command sent successfully");
            }
            else
            {
                Game.Logger.LogError("❌ Failed to send start combat command");
            }
#endif
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error starting AI combat test");
        }
    }

    /// <summary>
    /// 停止AI战斗测试按钮点击事件
    /// </summary>
    private static async void OnStopCombatButtonClick(object sender, PointerEventArgs e)
    {
        try
        {
            Game.Logger.LogInformation("🛑 Stop Combat button clicked");
#if SERVER
            await AISystemTestServer.StopAICombatTestCommand();
#else
            Game.Logger.LogInformation("📡 Sending stop combat command to server...");
            bool success = AITestCommandSender.SendCommand(AITestCommandType.StopCombat);
            if (success)
            {
                Game.Logger.LogInformation("✅ Stop combat command sent successfully");
            }
            else
            {
                Game.Logger.LogError("❌ Failed to send stop combat command");
            }
#endif
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error stopping AI combat test");
        }
    }

    /// <summary>
    /// 下一阶段按钮点击事件
    /// </summary>
    private static async void OnNextPhaseButtonClick(object sender, PointerEventArgs e)
    {
        try
        {
            Game.Logger.LogInformation("⏭️ Next Phase button clicked");
#if SERVER
            AISystemTestServer.TriggerNextPhase();
#else
            Game.Logger.LogInformation("📡 Sending next phase command to server...");
            bool success = AITestCommandSender.SendCommand(AITestCommandType.NextPhase);
            if (success)
            {
                Game.Logger.LogInformation("✅ Next phase command sent successfully");
            }
            else
            {
                Game.Logger.LogError("❌ Failed to send next phase command");
            }
#endif
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error triggering next phase");
        }
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// 清理UI资源
    /// </summary>
    public static void CleanupUI()
    {
        try
        {
            mainDashboard?.RemoveFromParent();
            mainDashboard = null;
            
            movementJoystick?.RemoveFromParent();
            movementJoystick = null;
            
            startTestButton = null;
            startValidationButton = null;
            resetTestButton = null;
            nextPhaseButton = null;
            
            // 清理新增的UI组件
            titleLabel = null;
            gameTimeLabel = null;
            testPhaseLabel = null;
            waveAIStatusLabel = null;
            unitCountLabel = null;
            positionLabel = null;
            instructionsLabel = null;
            currentBehaviorLabel = null;
            statusPanel = null;
            
            Game.Logger.LogInformation("🧹 AI Test Dashboard UI cleaned up");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error cleaning up UI");
        }
    }

    #endregion
}
#endif 