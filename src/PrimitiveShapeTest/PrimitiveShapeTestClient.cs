#if CLIENT
using Events;
using GameCore.ActorSystem;
using GameCore.BaseInterface;
using GameCore.Event;
using GameCore.OrderSystem;
using GameCore.PlayerAndUsers;
using GameCore.ProtocolClientTransient;
using GameCore.SceneSystem;
using GameCore.Shape;
using GameCore.Shape.Data;
using GameUI.Brush;
using GameUI.Control;
using GameUI.Control.Advanced;
using GameUI.Control.Enum;
using GameUI.Control.Primitive;
using GameUI.Control.Struct;
using GameUI.Device;
using GameUI.Enum;
using GameUI.Struct;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Numerics;

namespace GameEntry.PrimitiveShapeTest;

/// <summary>
/// 基本形状测试游戏模式的客户端UI实现
/// 显示形状测试状态、进度和实时信息
/// </summary>
internal class PrimitiveShapeTestClient : IGameClass
{
    #region Fields

    private static Trigger<EventGameStart>? gameStartTrigger;
    private static Panel? mainDashboard;
    private static Label? titleLabel;
    private static Label? statusLabel;
    private static Label? unitsCountLabel;
    private static Label? themeLabel;
    private static Label? currentSelectionLabel;
    private static Label? logLabel;

    // 摇杆控制
    private static JoystickNormal? movementJoystick;
    private static bool isJoystickMoving = false;

    // 状态信息和当前选择
    private static ShapeTestStatusInfo currentStatus;
    private static readonly Queue<string> logMessages = new();
    private static readonly int maxLogMessages = 20;
    private static int currentShapeIndex = 0;
    private static int currentThemeIndex = 0;
    private static int currentScenarioIndex = 0;
    private static float currentScale = 1.0f;
    private static readonly PrimitiveShape[] allShapes = System.Enum.GetValues<PrimitiveShape>();
    private static readonly ShapeColorTheme[] allThemes = System.Enum.GetValues<ShapeColorTheme>();
    private static readonly ShapeTestScenario[] allScenarios = System.Enum.GetValues<ShapeTestScenario>();

    // 跟踪AIShapeComposer创建的ActorModel
    private static readonly List<ActorModel> trackedActorModels = new();

    #endregion

    #region IGameClass Implementation

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.OnGameUIInitialization += OnGameUIInitialization;
        Game.Logger.LogInformation("🎯 Primitive Shape Test Client registered");
    }

    #endregion

    #region Initialization

    private static void OnGameUIInitialization()
    {
        if (Game.GameModeLink != ScopeData.GameMode.PrimitiveShapeTest)
        {
            return;
        }
    }

    private static void OnGameTriggerInitialization()
    {
        if (Game.GameModeLink != ScopeData.GameMode.PrimitiveShapeTest)
        {
            return;
        }

        gameStartTrigger = new Trigger<EventGameStart>(OnGameStartAsync, true);
        gameStartTrigger.Register(Game.Instance);
        Game.Logger.LogInformation("🎯 Primitive Shape Test client triggers initialized");
    }

    private static async Task<bool> OnGameStartAsync(object sender, EventGameStart eventArgs)
    {
        Game.Logger.LogInformation("🎯 Primitive Shape Test Client UI Started!");

        CreateMainUI();
        CreateJoystickControl();
        Game.Logger.LogInformation("🎯 Primitive Shape Test Client UI initialized");
        return true;
    }

    #endregion

    #region UI Creation

    private static void CreateMainUI()
    {
        try
        {
                         // 创建主面板 - 基于设计分辨率扩大尺寸
             mainDashboard = new Panel()
             {
                 Width = 600,  // 从480增加到600 (增加25%)
                 Height = AutoMode.Auto,
                 HorizontalAlignment = HorizontalAlignment.Right,
                 VerticalAlignment = VerticalAlignment.Top,
                 Margin = new Thickness(0, 30, 30, 0),  // 增加边距
                 Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                 CornerRadius = 16,  // 增加圆角
                 Padding = new Thickness(20, 20, 20, 20),  // 增加内边距
                 FlowOrientation = Orientation.Vertical,
                 VerticalContentAlignment = VerticalContentAlignment.Top
             };

                         // 创建标题 - 增大字体尺寸
             titleLabel = new Label()
             {
                 Text = "🎯 Primitive Shape Test Dashboard",
                 FontSize = 24,  // 从18增加到24 (增加33%)
                 TextColor = new SolidColorBrush(Color.FromArgb(255, 0, 200, 255)),
                 Width = AutoMode.Auto,
                 Height = AutoMode.Auto,
                 HorizontalContentAlignment = HorizontalContentAlignment.Center,
                 VerticalContentAlignment = VerticalContentAlignment.Center,
                 Margin = new Thickness(0, 0, 0, 16),  // 增加下边距
                 Parent = mainDashboard
             };

                         // 创建状态面板 - 增大尺寸和间距
             var statusPanel = new Panel()
             {
                 Width = AutoMode.Auto,
                 Height = AutoMode.Auto,
                 Background = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                 CornerRadius = 12,  // 增加圆角
                 Padding = new Thickness(16, 12, 16, 12),  // 增加内边距
                 Margin = new Thickness(0, 0, 0, 12),  // 增加下边距
                 FlowOrientation = Orientation.Vertical,
                 VerticalContentAlignment = VerticalContentAlignment.Top,
                 Parent = mainDashboard
             };

                         // 状态标签 - 增大字体
             statusLabel = new Label()
             {
                 Text = "📊 Status: Ready",
                 FontSize = 18,  // 从14增加到18 (增加29%)
                 TextColor = new SolidColorBrush(Color.White),
                 Width = AutoMode.Auto,
                 Height = AutoMode.Auto,
                 HorizontalContentAlignment = HorizontalContentAlignment.Left,
                 VerticalContentAlignment = VerticalContentAlignment.Center,
                 Margin = new Thickness(0, 0, 0, 6),  // 增加下边距
                 Parent = statusPanel
             };

                         // 单位数量标签 - 增大字体
             unitsCountLabel = new Label()
             {
                 Text = "🔢 Units: 0",
                 FontSize = 18,  // 从14增加到18 (增加29%)
                 TextColor = new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)),
                 Width = AutoMode.Auto,
                 Height = AutoMode.Auto,
                 HorizontalContentAlignment = HorizontalContentAlignment.Left,
                 VerticalContentAlignment = VerticalContentAlignment.Center,
                 Margin = new Thickness(0, 0, 0, 6),  // 增加下边距
                 Parent = statusPanel
             };

                         // 主题标签 - 增大字体
             themeLabel = new Label()
             {
                 Text = "🎨 Theme: Standard",
                 FontSize = 18,  // 从14增加到18 (增加29%)
                 TextColor = new SolidColorBrush(Color.FromArgb(255, 144, 238, 144)),
                 Width = AutoMode.Auto,
                 Height = AutoMode.Auto,
                 HorizontalContentAlignment = HorizontalContentAlignment.Left,
                 VerticalContentAlignment = VerticalContentAlignment.Center,
                 Parent = statusPanel
             };

                         // 当前选择标签 - 增大字体
             currentSelectionLabel = new Label()
             {
                 Text = $"🎯 Shape: {allShapes[currentShapeIndex]} | Theme: {allThemes[currentThemeIndex]} | Scale: {currentScale:F1}",
                 FontSize = 16,  // 从13增加到16 (增加23%)
                 TextColor = new SolidColorBrush(Color.FromArgb(255, 255, 182, 193)),
                 Width = AutoMode.Auto,
                 Height = AutoMode.Auto,
                 HorizontalContentAlignment = HorizontalContentAlignment.Left,
                 VerticalContentAlignment = VerticalContentAlignment.Center,
                 Margin = new Thickness(0, 12, 0, 0),  // 增加上边距
                 Parent = statusPanel
             };

            // 创建控制面板
            CreateControlButtons();

            // 创建日志面板
            CreateLogPanel();

            // 添加主面板到UI根
            UIRoot.Instance.AddChild(mainDashboard);

            // 定期更新UI
            _ = UpdateUILoop();

            Game.Logger.LogInformation("✅ Primitive Shape Test UI created successfully");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to create main UI");
        }
    }

    private static void CreateJoystickControl()
    {
        try
        {
            // 确保只在PrimitiveShapeTest模式下创建摇杆
            if (Game.GameModeLink != ScopeData.GameMode.PrimitiveShapeTest)
            {
                Game.Logger.LogDebug("🚫 Skipping joystick creation - not in PrimitiveShapeTest mode");
                return;
            }

            // 创建移动摇杆 - 使用官方安全区API动态计算位置
            var safeZone = ScreenViewport.Primary.SafeZonePadding;
            var joystickMargin = new Thickness(
                0,                                     // 左边距：不需要
                0,                                     // 顶边距：不需要
                Math.Max(30, safeZone.Right + 20),    // 右边距：安全区 + 额外边距，最小30
                Math.Max(30, safeZone.Bottom + 20)    // 底边距：安全区 + 额外边距，最小30
            );
            
            Game.Logger.LogInformation("🛡️ Device SafeZone - Left: {Left}, Top: {Top}, Right: {Right}, Bottom: {Bottom}", 
                safeZone.Left, safeZone.Top, safeZone.Right, safeZone.Bottom);
            Game.Logger.LogInformation("🕹️ Joystick Margin - Right: {Right}, Bottom: {Bottom}", 
                joystickMargin.Right, joystickMargin.Bottom);
            
                         // 优化摇杆尺寸和比例 - 基于设计分辨率大幅增大尺寸
             var radius = 120f;  // 从80增加到120 (增加50%)
             var knobSize = 50f; // 从35增加到50 (增加43%)
             var joystickSize = (radius * 2) + 30f; // 背景直径 + 边距，从180增加到270
            
            movementJoystick = new JoystickNormal()
            {
                Width = joystickSize,
                Height = joystickSize,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = joystickMargin,
                Radius = radius,
                KnobSize = knobSize,
                IsEnabled = true
            };

            // 优化摇杆外观 - 更好的视觉层次和定位
            movementJoystick.SetBackgroundStyle(new SolidColorBrush(Color.FromArgb(180, 60, 60, 60)));
            movementJoystick.SetKnobStyle(new SolidColorBrush(Color.FromArgb(240, 255, 255, 255)));
            
            // 强制更新摇杆外观，确保正确的尺寸和定位
            movementJoystick.UpdateAppearance();

            // 注册摇杆事件
            movementJoystick.ValueChanged += OnJoystickValueChanged;
            movementJoystick.DragStarted += OnJoystickDragStarted;
            movementJoystick.DragEnded += OnJoystickDragEnded;

            // 添加摇杆到UI根
            UIRoot.Instance.AddChild(movementJoystick);

            Game.Logger.LogInformation("🕹️ Optimized movement joystick initialized for PrimitiveShapeTest");
            Game.Logger.LogInformation("🎮 Joystick specs - Size: {Size}, Radius: {Radius}, Knob: {Knob}", 
                joystickSize, radius, knobSize);
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
            
            // 优化死区处理 - 更精确的阈值
            if (inputValue.Length() < 0.08f)
            {
                if (isJoystickMoving)
                {
                    SendStopMovementCommand();
                }
                return;
            }

            // 计算移动角度并添加平滑处理
            var angle = Math.Atan2(inputValue.Y, inputValue.X) * (180.0 / Math.PI);
            
            // 添加角度平滑 - 避免抖动
            var smoothedAngle = Math.Round(angle / 5.0) * 5.0;
            
            SendMovementCommand((float)smoothedAngle);
            
            // 添加调试信息（仅在开发模式下）
            if (Game.Logger.IsEnabled(LogLevel.Debug))
            {
                Game.Logger.LogDebug("🎮 Joystick input - Raw: ({X:F2}, {Y:F2}), Angle: {Angle:F1}°, Smoothed: {Smoothed:F1}°", 
                    inputValue.X, inputValue.Y, angle, smoothedAngle);
            }
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

         private static void CreateControlButtons()
     {
         // 创建控制面板 - 优化布局以适应面板宽度
         var controlPanel = new Panel()
         {
             Width = AutoMode.Auto,
             Height = AutoMode.Auto,
             Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
             CornerRadius = 12,  // 增加圆角
             Padding = new Thickness(16, 12, 16, 12),  // 增加内边距
             Margin = new Thickness(0, 0, 0, 12),  // 增加下边距
             FlowOrientation = Orientation.Vertical,
             VerticalContentAlignment = VerticalContentAlignment.Top,
             Parent = mainDashboard
         };

                 // 选择控制行 - 优化布局
         var selectionRow = new Panel()
         {
             Width = AutoMode.Auto,
             Height = AutoMode.Auto,
             FlowOrientation = Orientation.Horizontal,
             HorizontalContentAlignment = HorizontalContentAlignment.UniformSpacing,  // 使用UniformSpacing
             Margin = new Thickness(0, 0, 0, 10),  // 增加下边距
             Parent = controlPanel
         };

        // 形状选择按钮
        _ = CreateButton("🔄 Shape", selectionRow, OnNextShapeClick);
        _ = CreateButton("🎨 Theme", selectionRow, OnNextThemeClick);
        _ = CreateButton("🔍 Scale+", selectionRow, OnScaleUpClick);
        _ = CreateButton("🔎 Scale-", selectionRow, OnScaleDownClick);

                 // 第一行按钮 - 创建功能
         var firstRow = new Panel()
         {
             Width = AutoMode.Auto,
             Height = AutoMode.Auto,
             FlowOrientation = Orientation.Horizontal,
             HorizontalContentAlignment = HorizontalContentAlignment.UniformSpacing,  // 使用UniformSpacing
             Margin = new Thickness(0, 0, 0, 10),  // 增加下边距
             Parent = controlPanel
         };

        _ = CreateButton("🎯 Create Shape", firstRow, OnCreateShapeClick);
        _ = CreateButton("🤖 Composite", firstRow, OnCreateCompositeClick);

                 // 第二行按钮 - 场景功能
         var secondRow = new Panel()
         {
             Width = AutoMode.Auto,
             Height = AutoMode.Auto,
             FlowOrientation = Orientation.Horizontal,
             HorizontalContentAlignment = HorizontalContentAlignment.UniformSpacing,  // 使用UniformSpacing
             Margin = new Thickness(0, 0, 0, 10),  // 增加下边距
             Parent = controlPanel
         };

        _ = CreateButton("🌟 All Shapes", secondRow, OnCreateAllShapesClick);
        _ = CreateButton("🎬 Scenario", secondRow, OnCreateScenarioClick);

                 // 第三行按钮 - 测试和工具
         var thirdRow = new Panel()
         {
             Width = AutoMode.Auto,
             Height = AutoMode.Auto,
             FlowOrientation = Orientation.Horizontal,
             HorizontalContentAlignment = HorizontalContentAlignment.UniformSpacing,  // 使用UniformSpacing
             Parent = controlPanel
         };

        _ = CreateButton("⚡ Performance", thirdRow, OnPerformanceTestClick);
        _ = CreateButton("🗑️ Clear All", thirdRow, OnClearAllClick);

        // 第四行按钮 - AIShapeComposer 测试
        var fourthRow = new Panel()
        {
            Width = AutoMode.Auto,
            Height = AutoMode.Auto,
            FlowOrientation = Orientation.Horizontal,
            HorizontalContentAlignment = HorizontalContentAlignment.UniformSpacing,
            Margin = new Thickness(0, 0, 0, 10),
            Parent = controlPanel
        };

        CreateButton("👤 Character", fourthRow, OnCreateCharacterClick);
        CreateButton("🤖 Robot", fourthRow, OnCreateRobotClick);
        CreateButton("🏠 House", fourthRow, OnCreateHouseClick);
        CreateButton("🏰 Tower", fourthRow, OnCreateTowerClick);

        // 第五行按钮 - 载具测试
        var fifthRow = new Panel()
        {
            Width = AutoMode.Auto,
            Height = AutoMode.Auto,
            FlowOrientation = Orientation.Horizontal,
            HorizontalContentAlignment = HorizontalContentAlignment.UniformSpacing,
            Margin = new Thickness(0, 0, 0, 10),
            Parent = controlPanel
        };

        CreateButton("🚗 Car", fifthRow, OnCreateCarClick);
        CreateButton("🚀 Spaceship", fifthRow, OnCreateSpaceshipClick);
        CreateButton("🌳 Tree", fifthRow, OnCreateTreeClick);
        CreateButton("💡 Light", fifthRow, OnCreateLightClick);

        // 第六行按钮 - 游戏对象测试
        var sixthRow = new Panel()
        {
            Width = AutoMode.Auto,
            Height = AutoMode.Auto,
            FlowOrientation = Orientation.Horizontal,
            HorizontalContentAlignment = HorizontalContentAlignment.UniformSpacing,
            Margin = new Thickness(0, 0, 0, 10),
            Parent = controlPanel
        };

        CreateButton("🔫 Turret", sixthRow, OnCreateTurretClick);
        CreateButton("🏛️ Altar", sixthRow, OnCreateAltarClick);
        CreateButton("🌲 Forest", sixthRow, OnCreateForestClick);
        CreateButton("🏙️ City", sixthRow, OnCreateCityClick);
    }

         private static Button CreateButton(string text, Panel parent, EventHandler<PointerEventArgs> clickHandler)
     {
         var button = new Button()
         {
             Width = 120,  // 从140减少到120，避免超出面板宽度
             Height = 44,   // 保持高度不变
             Background = new SolidColorBrush(Color.FromArgb(180, 70, 130, 180)),
             CornerRadius = 8,  // 保持圆角
             Margin = new Thickness(2, 0, 2, 0),  // 减少边距，从4减少到2
             Parent = parent
         };

                 var label = new Label()
         {
             Text = text,
             FontSize = 14,  // 从11增加到14 (增加27%)
             TextColor = new SolidColorBrush(Color.White),
             HorizontalAlignment = HorizontalAlignment.Stretch,
             VerticalAlignment = VerticalAlignment.Stretch,
             HorizontalContentAlignment = HorizontalContentAlignment.Center,
             VerticalContentAlignment = VerticalContentAlignment.Center,
             Parent = button
         };

        button.OnPointerClicked += clickHandler;
        return button;
    }

         private static void CreateLogPanel()
     {
         // 创建日志面板 - 使用PanelScrollable支持滚动
         var logPanel = new PanelScrollable()
         {
             Width = AutoMode.Auto,
             Height = 250,  // 从200增加到250，提供更多空间
             Background = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
             CornerRadius = 12,  // 增加圆角
             Padding = new Thickness(12, 12, 12, 12),  // 增加内边距
             FlowOrientation = Orientation.Vertical,
             VerticalContentAlignment = VerticalContentAlignment.Top,
             ScrollEnabled = true,  // 启用滚动
             ScrollOrientation = Orientation.Vertical,  // 垂直滚动
             Parent = mainDashboard
         };

                 // 日志标题 - 增大字体
         var logTitle = new Label()
         {
             Text = "📝 Test Log",
             FontSize = 18,  // 从13增加到18 (增加38%)
             TextColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
             Width = AutoMode.Auto,
             Height = AutoMode.Auto,
             HorizontalContentAlignment = HorizontalContentAlignment.Left,
             VerticalContentAlignment = VerticalContentAlignment.Center,
             Margin = new Thickness(0, 0, 0, 12),  // 增加下边距
             Parent = logPanel
         };

                 // 日志内容 - 增大字体并启用文本换行
         logLabel = new Label()
         {
             Text = "🎯 Primitive Shape Test System Ready",
             FontSize = 14,  // 从11增加到14 (增加27%)
             TextColor = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220)),
             Width = AutoMode.Auto,
             Height = AutoMode.Auto,
             HorizontalContentAlignment = HorizontalContentAlignment.Left,
             VerticalContentAlignment = VerticalContentAlignment.Top,
             TextWrap = true,  // 启用文本换行
             Parent = logPanel
         };
    }

    #endregion

    #region Event Handlers

    private static void OnNextShapeClick(object sender, PointerEventArgs e)
    {
        currentShapeIndex = (currentShapeIndex + 1) % allShapes.Length;
        UpdateCurrentSelectionLabel();
        AddLogMessage($"Selected shape: {allShapes[currentShapeIndex]}");
    }

    private static void OnNextThemeClick(object sender, PointerEventArgs e)
    {
        currentThemeIndex = (currentThemeIndex + 1) % allThemes.Length;
        UpdateCurrentSelectionLabel();
        AddLogMessage($"Selected theme: {allThemes[currentThemeIndex]}");
    }

    private static void OnScaleUpClick(object sender, PointerEventArgs e)
    {
        currentScale = Math.Min(3.0f, currentScale + 0.2f);
        UpdateCurrentSelectionLabel();
        AddLogMessage($"Scale increased to: {currentScale:F1}");
    }

    private static void OnScaleDownClick(object sender, PointerEventArgs e)
    {
        currentScale = Math.Max(0.2f, currentScale - 0.2f);
        UpdateCurrentSelectionLabel();
        AddLogMessage($"Scale decreased to: {currentScale:F1}");
    }

    private static void OnCreateShapeClick(object sender, PointerEventArgs e)
    {
        try
        {
            var command = new ProtoShapeTestCommand
            {
                CommandType = ShapeTestCommandType.CreateSingleShape,
                TargetShape = allShapes[currentShapeIndex],
                ColorTheme = allThemes[currentThemeIndex],
                Position = GetRandomTestPosition(),
                Scale = new Vector3(currentScale, currentScale, currentScale)
            };

            bool success = command.SendToServer();
            if (success)
            {
                AddLogMessage($"Creating {allShapes[currentShapeIndex]} with {allThemes[currentThemeIndex]} theme");
            }
            else
            {
                AddLogMessage("❌ Failed to send create shape command");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"Error creating shape: {ex.Message}");
        }
    }

    private static void OnCreateCompositeClick(object sender, PointerEventArgs e)
    {
        try
        {
            var command = new ProtoShapeTestCommand
            {
                CommandType = ShapeTestCommandType.CreateCompositeShape,
                Position = GetRandomTestPosition()
            };

            bool success = command.SendToServer();
            if (success)
            {
                AddLogMessage("Creating composite robot shape");
            }
            else
            {
                AddLogMessage("❌ Failed to send create composite command");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"Error creating composite: {ex.Message}");
        }
    }

    private static void OnCreateAllShapesClick(object sender, PointerEventArgs e)
    {
        try
        {
            var command = new ProtoShapeTestCommand
            {
                CommandType = ShapeTestCommandType.CreateAllShapes,
                ColorTheme = allThemes[currentThemeIndex]
            };

            bool success = command.SendToServer();
            if (success)
            {
                AddLogMessage($"Creating all shapes with {allThemes[currentThemeIndex]} theme");
            }
            else
            {
                AddLogMessage("❌ Failed to send create all shapes command");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"Error creating all shapes: {ex.Message}");
        }
    }

    private static void OnCreateScenarioClick(object sender, PointerEventArgs e)
    {
        try
        {
            currentScenarioIndex = (currentScenarioIndex + 1) % allScenarios.Length;
            var scenario = allScenarios[currentScenarioIndex];

            var command = new ProtoShapeTestCommand
            {
                CommandType = ShapeTestCommandType.CreateScenario,
                TargetScenario = scenario
            };

            bool success = command.SendToServer();
            if (success)
            {
                AddLogMessage($"Creating {scenario} scenario");
            }
            else
            {
                AddLogMessage("❌ Failed to send create scenario command");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"Error creating scenario: {ex.Message}");
        }
    }

    private static void OnPerformanceTestClick(object sender, PointerEventArgs e)
    {
        try
        {
            var command = new ProtoShapeTestCommand
            {
                CommandType = ShapeTestCommandType.TestPerformance
            };

            bool success = command.SendToServer();
            if (success)
            {
                AddLogMessage("Starting performance test...");
            }
            else
            {
                AddLogMessage("❌ Failed to send performance test command");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"Error starting performance test: {ex.Message}");
        }
    }

    private static void OnClearAllClick(object sender, PointerEventArgs e)
    {
        try
        {
            var command = new ProtoShapeTestCommand
            {
                CommandType = ShapeTestCommandType.ClearAllUnits
            };

            bool success = command.SendToServer();
            if (success)
            {
                AddLogMessage("Clearing all test units");
                // 清理AIShapeComposer创建的ActorModel
                foreach (var model in trackedActorModels)
                {
                    model.Destroy();
                }
                trackedActorModels.Clear();
            }
            else
            {
                AddLogMessage("❌ Failed to send clear all command");
            }
        }
        catch (Exception ex)
        {
            AddLogMessage($"Error clearing units: {ex.Message}");
        }
    }

    #endregion

    #region Protocol Handlers

    public static void UpdateStatus(ShapeTestStatusInfo statusInfo)
    {
        try
        {
            currentStatus = statusInfo;
            
            if (statusLabel != null)
                statusLabel.Text = $"📊 Status: {(statusInfo.IsPerformanceTestRunning ? "Performance Testing" : "Ready")}";
            
            if (unitsCountLabel != null)
                unitsCountLabel.Text = $"🔢 Units: {statusInfo.CurrentActiveUnits} (Total: {statusInfo.TotalUnitsCreated})";
            
            if (themeLabel != null)
                themeLabel.Text = $"🎨 Theme: {statusInfo.CurrentTheme}";

            Game.Logger.LogDebug("📊 Updated shape test status from server");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error updating status from server");
        }
    }

    public static void OnTestComplete(TestType testType, bool success, int unitsCreated, double elapsedTime)
    {
        try
        {
            var status = success ? "✅" : "❌";
            var message = $"{status} {testType}: Created {unitsCreated} units in {elapsedTime:F2}s";
            AddLogMessage(message);
            
            Game.Logger.LogInformation("Test completed: {testType}, Success: {success}, Units: {unitsCreated}, Time: {elapsedTime:F2}s", 
                testType, success, unitsCreated, elapsedTime);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling test complete");
        }
    }

    #endregion

    #region Helper Methods

    private static void UpdateCurrentSelectionLabel()
    {
        if (currentSelectionLabel != null)
        {
            currentSelectionLabel.Text = $"🎯 Shape: {allShapes[currentShapeIndex]} | Theme: {allThemes[currentThemeIndex]} | Scale: {currentScale:F1}";
        }
    }

         private static void AddLogMessage(string message)
     {
         try
         {
             // 限制单条消息长度，避免过长
             var truncatedMessage = message.Length > 80 ? message.Substring(0, 77) + "..." : message;
             logMessages.Enqueue($"[{DateTime.Now:HH:mm:ss}] {truncatedMessage}");
             
             // 保持日志数量在合理范围内
             while (logMessages.Count > maxLogMessages)
             {
                 logMessages.Dequeue();
             }
 
             if (logLabel != null)
             {
                 // 使用换行符连接消息，确保正确显示
                 logLabel.Text = string.Join("\n", logMessages.Reverse());
             }
         }
         catch (Exception ex)
         {
             Game.Logger.LogError(ex, "❌ Error adding log message");
         }
     }

    private static Vector3 GetRandomTestPosition()
    {
        var random = new Random();
        var x = random.Next(6000, 10000);
        var y = random.Next(6000, 10000);
        return new Vector3(x, y, 0);
    }

    private static async Task UpdateUILoop()
    {
        while (true)
        {
            try
            {
                await Game.Delay(TimeSpan.FromSeconds(1));
                // 定期更新UI状态
            }
            catch (Exception ex)
            {
                Game.Logger.LogError(ex, "❌ Error in UI update loop");
                break;
            }
        }
    }

    #endregion

    #region AIShapeComposer Test Methods

    private static void OnCreateCharacterClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var character = AIShapeComposer.CreateSimpleCharacter(scenePoint, null, currentScale);
            AddLogMessage($"👤 Created character at {position}");
            
            Game.Logger.LogInformation("Created simple character at {position}", position);
            trackedActorModels.AddRange(character);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating character");
            AddLogMessage($"❌ Error creating character: {ex.Message}");
        }
    }

    private static void OnCreateRobotClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var robot = AIShapeComposer.CreateRobotCharacter(scenePoint, null, currentScale);
            AddLogMessage($"🤖 Created robot at {position}");
            
            Game.Logger.LogInformation("Created robot character at {position}", position);
            trackedActorModels.AddRange(robot);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating robot");
            AddLogMessage($"❌ Error creating robot: {ex.Message}");
        }
    }

    private static void OnCreateHouseClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var house = AIShapeComposer.CreateSimpleHouse(scenePoint, null, currentScale);
            AddLogMessage($"🏠 Created house at {position}");
            
            Game.Logger.LogInformation("Created simple house at {position}", position);
            trackedActorModels.AddRange(house);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating house");
            AddLogMessage($"❌ Error creating house: {ex.Message}");
        }
    }

    private static void OnCreateTowerClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var tower = AIShapeComposer.CreateCastleTower(scenePoint, null, currentScale);
            AddLogMessage($"🏰 Created tower at {position}");
            
            Game.Logger.LogInformation("Created castle tower at {position}", position);
            trackedActorModels.AddRange(tower);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating tower");
            AddLogMessage($"❌ Error creating tower: {ex.Message}");
        }
    }

    private static void OnCreateCarClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var car = AIShapeComposer.CreateSimpleCar(scenePoint, null, currentScale);
            AddLogMessage($"🚗 Created car at {position}");
            
            Game.Logger.LogInformation("Created simple car at {position}", position);
            trackedActorModels.AddRange(car);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating car");
            AddLogMessage($"❌ Error creating car: {ex.Message}");
        }
    }

    private static void OnCreateSpaceshipClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var spaceship = AIShapeComposer.CreateSimpleSpaceship(scenePoint, null, currentScale);
            AddLogMessage($"🚀 Created spaceship at {position}");
            
            Game.Logger.LogInformation("Created simple spaceship at {position}", position);
            trackedActorModels.AddRange(spaceship);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating spaceship");
            AddLogMessage($"❌ Error creating spaceship: {ex.Message}");
        }
    }

    private static void OnCreateTreeClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var tree = AIShapeComposer.CreateSimpleTree(scenePoint, null, currentScale);
            AddLogMessage($"🌳 Created tree at {position}");
            
            Game.Logger.LogInformation("Created simple tree at {position}", position);
            trackedActorModels.AddRange(tree);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating tree");
            AddLogMessage($"❌ Error creating tree: {ex.Message}");
        }
    }

    private static void OnCreateLightClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var light = AIShapeComposer.CreateStreetLight(scenePoint, null, currentScale);
            AddLogMessage($"💡 Created light at {position}");
            
            Game.Logger.LogInformation("Created simple light at {position}", position);
            trackedActorModels.AddRange(light);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating light");
            AddLogMessage($"❌ Error creating light: {ex.Message}");
        }
    }

    private static void OnCreateTurretClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var turret = AIShapeComposer.CreateSimpleTurret(scenePoint, null, currentScale);
            AddLogMessage($"🔫 Created turret at {position}");
            
            Game.Logger.LogInformation("Created simple turret at {position}", position);
            trackedActorModels.AddRange(turret);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating turret");
            AddLogMessage($"❌ Error creating turret: {ex.Message}");
        }
    }

    private static void OnCreateAltarClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var altar = AIShapeComposer.CreateCollectibleAltar(scenePoint, null, currentScale);
            AddLogMessage($"🏛️ Created altar at {position}");
            
            Game.Logger.LogInformation("Created simple altar at {position}", position);
            trackedActorModels.AddRange(altar);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating altar");
            AddLogMessage($"❌ Error creating altar: {ex.Message}");
        }
    }

    private static void OnCreateForestClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var forest = AIShapeComposer.CreateForest(scenePoint, 500f, 5, null);
            AddLogMessage($"🌲 Created forest at {position}");
            
            Game.Logger.LogInformation("Created simple forest at {position}", position);
            trackedActorModels.AddRange(forest.SelectMany(x => x));
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating forest");
            AddLogMessage($"❌ Error creating forest: {ex.Message}");
        }
    }

    private static void OnCreateCityClick(object sender, PointerEventArgs e)
    {
        try
        {
            var position = GetRandomTestPosition();
            var scenePoint = new ScenePoint(position.X, position.Y, Game.LocalScene);
            
            var city = AIShapeComposer.CreateCityBlock(scenePoint, (2, 2), 300f, null);
            AddLogMessage($"🏙️ Created city at {position}");
            
            Game.Logger.LogInformation("Created simple city at {position}", position);
            trackedActorModels.AddRange(city.SelectMany(x => x));
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating city");
            AddLogMessage($"❌ Error creating city: {ex.Message}");
        }
    }

    #endregion
}
#endif
