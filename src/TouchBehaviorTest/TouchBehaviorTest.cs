#if CLIENT
using Events;
using GameCore.Event;
using GameUI.Control;
using GameUI.Control.Advanced;
using GameUI.Control.Primitive;
using GameUI.Control.Behavior;
using GameUI.Control.Struct;
using GameUI.Control.Extensions;
using GameUI.Struct;
using GameUI.Enum;
using GameUI.Brush;
using System.Drawing;
using static GameUI.Control.Extensions.BuilderExtensions;
using System.Linq;

namespace GameEntry.TouchBehaviorTest;

/// <summary>
/// TouchBehavior测试游戏模式，展示不同配置的触摸行为效果
/// </summary>
/// <remarks>
/// 此测试模式创建多个按钮，每个按钮配置不同的TouchBehavior参数，
/// 包括不同的缩放比例、动画时长、长按时长等，以全面测试TouchBehavior的功能。
/// 同时提供状态显示区域，实时显示当前的交互状态。
/// </remarks>
public class TouchBehaviorTestMode : IGameClass
{
    /// <summary>
    /// 游戏状态管理
    /// </summary>
    public static class GameState
    {
        public static bool IsInitialized { get; set; } = false;
        public static int ButtonClickCount { get; set; } = 0;
        public static int LongPressCount { get; set; } = 0;
        public static string LastActionDescription { get; set; } = "等待操作...";
        public static DateTime LastActionTime { get; set; } = DateTime.Now;
        
        // 长按与点击冲突测试专用状态
        public static int ConflictTestClickCount { get; set; } = 0;
        public static int ConflictTestLongPressCount { get; set; } = 0;
        public static string ConflictTestLastEvent { get; set; } = "未开始测试";
        public static bool IsInLongPress { get; set; } = false;
    }

    private static Panel? mainPanel;
    private static Label? statusLabel;
    private static Label? clickCountLabel;
    private static Label? longPressCountLabel;
    private static Label? lastActionLabel;
    
    // 冲突测试专用UI控件
    private static Label? conflictTestTitleLabel;
    private static Label? conflictTestStatusLabel;
    private static Label? conflictTestClickLabel;
    private static Label? conflictTestLongPressLabel;

    public static void OnRegisterGameClass()
    {
        Game.OnGameUIInitialization += OnGameUIInitialized;
    }

    private static void OnGameUIInitialized()
    {
        if (Game.GameModeLink != ScopeData.GameMode.TouchBehaviorTest)
        {
            return;
        }

        Game.Logger.LogInformation("🎮 TouchBehavior Test Mode initialized");
        CreateTestUI();
        GameState.IsInitialized = true;
    }

    /// <summary>
    /// 创建测试UI界面 - 使用响应式设计优化分辨率
    /// </summary>
    private static void CreateTestUI()
    {
        // 创建响应式主面板 - 使用新的Flexbox扩展
        mainPanel = new Panel
        {
            Name = "TouchBehaviorTestPanel",
            Background = new SolidColorBrush(Color.FromArgb(240, 248, 255)) // AliceBlue
        }
        .Stretch()  // 拉伸到全宽全高
        .GrowRatio(0.9f, 0.9f);  // 占可用空间的90%

        UIRoot.Instance.AddChild(mainPanel);

        // 使用相对布局创建各个区域
        CreateResponsiveTitleSection();
        CreateResponsiveStatusSection(); 
        CreateResponsiveButtonGrid();
        CreateResponsiveConflictTest();
        CreateResponsiveInstructions();

        Game.Logger.LogInformation("✅ TouchBehavior Test UI created with responsive design");
    }

    /// <summary>
    /// 创建响应式标题区域
    /// </summary>
    private static void CreateResponsiveTitleSection()
    {
        if (mainPanel == null)
        {
            return;
        }

        var titleLabel = new Label
        {
            Text = "TouchBehavior 测试界面",
            FontSize = 42,    // 更大的标题字体
            Bold = true,
            TextColor = Color.FromArgb(25, 25, 112), // MidnightBlue
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 25, 0, 0),  // 增加顶部间距
            HorizontalContentAlignment = HorizontalContentAlignment.Center,
            VerticalContentAlignment = VerticalContentAlignment.Center
        };
        
        mainPanel.AddChild(titleLabel);
    }

    /// <summary>
    /// 创建响应式状态区域
    /// </summary>
    private static void CreateResponsiveStatusSection()
    {
        if (mainPanel == null) return;

        // 状态面板容器 - 使用拉伸比例响应式布局
        var statusPanel = new Panel
        {
            VerticalAlignment = VerticalAlignment.Top,
            Height = 140,               // 增加高度容纳更大字体
            Margin = new Thickness(0, 85, 0, 0),  // 增加从标题下方的间距（25+60=85）
            Background = new SolidColorBrush(Color.FromArgb(248, 249, 250)), // 更亮的灰色
            CornerRadius = 12,
        }
        .StretchHorizontal()  // 拉伸到全宽
        .WidthGrow(0.92f);    // 占可用宽度的92%

        // 状态标题
        var statusTitle = new Label
        {
            Text = "📊 实时状态",
            FontSize = 28,
            Bold = true,
            TextColor = Color.FromArgb(70, 130, 180), // SteelBlue
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 12, 0, 0)
        };
        statusPanel.AddChild(statusTitle);

        // 左侧状态信息
        clickCountLabel = new Label
        {
            Text = "🖱️ 按钮点击次数: 0",
            FontSize = 20,
            TextColor = Color.FromArgb(40, 167, 69), // 更鲜明的绿色
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,  // 改为Top对齐
            Margin = new Thickness(40, 55, 0, 0)  // 调整为绝对位置
        };
        statusPanel.AddChild(clickCountLabel);

        longPressCountLabel = new Label
        {
            Text = "⏱️ 长按触发次数: 0",
            FontSize = 20,
            TextColor = Color.FromArgb(220, 53, 69), // 更鲜明的红色
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,  // 改为Top对齐
            Margin = new Thickness(40, 85, 0, 0)  // 与上一行间距30px
        };
        statusPanel.AddChild(longPressCountLabel);

        // 右侧状态信息
        lastActionLabel = new Label
        {
            Text = "📋 最后操作: 等待操作...",
            FontSize = 20,
            TextColor = Color.FromArgb(255, 193, 7), // 更鲜明的橙色
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,  // 改为Top对齐
            Margin = new Thickness(0, 70, 40, 0),  // 调整为绝对位置
            TextWrap = true
        };
        statusPanel.AddChild(lastActionLabel);

        mainPanel.AddChild(statusPanel);
    }

    /// <summary>
    /// 创建响应式按钮网格
    /// </summary>
    private static void CreateResponsiveButtonGrid()
    {
        if (mainPanel == null) return;

            var buttonContainer = new Panel
    {
        VerticalAlignment = VerticalAlignment.Top,
        Height = 380,               // 增加高度容纳更大按钮
        Margin = new Thickness(0, 250, 0, 0) // 增加偏移量，为状态区域留出足够空间（85+140+25=250）
    }
    .StretchHorizontal()  // 拉伸到全宽
    .WidthGrow(0.95f);    // 占95%的可用宽度

        // 创建网格化的按钮布局 - 3x3网格，优化尺寸和间距
        var buttonWidth = 200;  // 增加按钮宽度
        var buttonHeight = 80;  // 增加按钮高度
        var horizontalSpacing = 30; // 增加水平间距
        var verticalSpacing = 25;   // 垂直间距
        
        var buttons = new (string text, Color color, Action<TextButton> setup)[]
        {
            ("默认配置", Color.FromArgb(100, 149, 237), b => b.AddTouchBehavior()),
            ("小缩放(0.8)", Color.FromArgb(255, 99, 71), b => b.AddTouchBehavior(scaleFactor: 0.8f)),
            ("大缩放(1.1)", Color.FromArgb(60, 179, 113), b => b.AddTouchBehavior(scaleFactor: 1.1f)),
            ("快速动画", Color.FromArgb(138, 43, 226), b => b.AddTouchBehaviorWithDuration(0.95f, 50, 500)),
            ("慢速动画", Color.FromArgb(220, 20, 60), b => b.AddTouchBehaviorWithDuration(0.95f, 500, 500)),
            ("短长按", Color.FromArgb(255, 20, 147), b => b.AddTouchBehaviorWithDuration(0.95f, 150, 200)),
            ("禁用长按", Color.FromArgb(105, 105, 105), b => b.AddTouchBehavior(enableLongPress: false)),
            ("禁用动画", Color.FromArgb(218, 165, 32), b => b.AddTouchBehavior(enablePressAnimation: false)),
            ("重置计数", Color.FromArgb(32, 178, 170), b => { })
        };

        // 计算网格居中偏移
        var totalGridWidth = 3 * buttonWidth + 2 * horizontalSpacing;
        var gridOffsetX = Math.Max(0, (900 - totalGridWidth) / 2); // 假设容器大约900px宽

        for (int i = 0; i < buttons.Length; i++)
        {
            var (text, color, setup) = buttons[i];
            var row = i / 3;
            var col = i % 3;
            
            var button = new TextButton(text)
            {
                Width = buttonWidth,
                Height = buttonHeight,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(
                    gridOffsetX + col * (buttonWidth + horizontalSpacing), 
                    row * (buttonHeight + verticalSpacing), 
                    0, 0),
                Background = new SolidColorBrush(color),
                TextColor = Color.White,
                FontSize = 16,  // 增加字体大小
                Bold = true,
                CornerRadius = 10  // 更圆润的按钮
            };

            if (text == "重置计数")
            {
                button.OnPointerClicked += (s, e) => ResetCounters();
            }
            else
            {
                setup(button);
                SetupButtonEvents(button, text);
            }

            buttonContainer.AddChild(button);
        }

        mainPanel.AddChild(buttonContainer);
    }

    /// <summary>
    /// 设置按钮事件处理
    /// </summary>
    private static void SetupButtonEvents(TextButton button, string name)
    {
        var behavior = button.Behaviors?.OfType<TouchBehavior>().FirstOrDefault();
        if (behavior != null && behavior.EnableLongPress)
        {
            behavior.LongPressTriggered += (s, e) => OnLongPress(name);
        }
        button.OnPointerClicked += (s, e) => OnButtonClick(name);
    }

    /// <summary>
    /// 创建响应式冲突测试区域
    /// </summary>
    private static void CreateResponsiveConflictTest()
    {
        if (mainPanel == null) return;

            var conflictPanel = new Panel
    {
        VerticalAlignment = VerticalAlignment.Bottom,
        Height = 200,               // 增加高度容纳更多内容
        Background = new SolidColorBrush(Color.FromArgb(255, 248, 220)), // 更亮的米色
        CornerRadius = 12,
        Margin = new Thickness(0, 0, 0, 70) // 增加底部边距，为说明文字留出空间
    }
    .StretchHorizontal()  // 拉伸到全宽
    .WidthGrow(0.92f);    // 占可用宽度的92%

        // 冲突测试标题
        var title = new Label
        {
            Text = "🔍 长按与点击冲突测试区域",
            FontSize = 24,
            Bold = true,
            TextColor = Color.FromArgb(220, 20, 60), // 更鲜明的红色
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 12, 0, 0)
        };
        conflictPanel.AddChild(title);

        // 冲突测试按钮 - 更大更醒目
        var conflictButton = new TextButton("🧪 冲突测试按钮")
        {
            Width = 240,
            Height = 70,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(40, 10, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(255, 152, 0)), // 更鲜明的橙色
            TextColor = Color.White,
            FontSize = 18,
            Bold = true,
            CornerRadius = 12
        };

        var conflictBehavior = conflictButton.AddTouchBehaviorWithDuration(0.9f, 150, 300);
        conflictBehavior.LongPressStarted += (s, e) => OnConflictTestLongPressStarted();
        conflictBehavior.LongPressTriggered += (s, e) => OnConflictTestLongPress();
        conflictBehavior.LongPressEnded += (s, e) => OnConflictTestLongPressEnded();
        conflictButton.OnPointerClicked += (s, e) => OnConflictTestClick();
        
        conflictPanel.AddChild(conflictButton);

        // 重新布局状态显示标签
        conflictTestStatusLabel = new Label
        {
            Text = "🔄 状态: 未开始测试",
            FontSize = 16,
            TextColor = Color.FromArgb(25, 135, 84), // 更鲜明的绿色
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,  // 改为Top对齐
            Margin = new Thickness(0, 105, 0, 0)  // 调整为绝对位置
        };
        conflictPanel.AddChild(conflictTestStatusLabel);

        conflictTestClickLabel = new Label
        {
            Text = "🖱️ 点击次数: 0",
            FontSize = 16,
            TextColor = Color.FromArgb(13, 110, 253), // 更鲜明的蓝色
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,  // 改为Top对齐
            Margin = new Thickness(0, 130, 50, 0)  // 绝对位置，避免重叠
        };
        conflictPanel.AddChild(conflictTestClickLabel);

        conflictTestLongPressLabel = new Label
        {
            Text = "⏱️ 长按次数: 0",
            FontSize = 16,
            TextColor = Color.FromArgb(220, 53, 69), // 更鲜明的红色
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,  // 改为Top对齐
            Margin = new Thickness(0, 155, 50, 0)  // 与上一行间距25px
        };
        conflictPanel.AddChild(conflictTestLongPressLabel);

        mainPanel.AddChild(conflictPanel);
    }

    /// <summary>
    /// 创建响应式说明区域
    /// </summary>
    private static void CreateResponsiveInstructions()
    {
        if (mainPanel == null) return;

        var instructionLabel = new Label
        {
            Text = "💡 使用说明：点击不同按钮体验各种TouchBehavior效果，冲突测试区域验证长按与点击的互斥机制",
            FontSize = 16,
            TextColor = Color.FromArgb(108, 117, 125), // 更好的灰色
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(30, 0, 30, 45),  // 增加底部边距，避免与冲突测试区域重叠
            TextWrap = true,
            HorizontalContentAlignment = HorizontalContentAlignment.Center
        };
        
        mainPanel.AddChild(instructionLabel);
    }



    /// <summary>
    /// 创建状态显示面板（保持兼容性）
    /// </summary>
    private static void CreateStatusPanel()
    {
        if (mainPanel == null) return;

        var statusPanel = new Panel
        {
            Width = 750,
            Height = 100,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 70, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(245, 245, 245)), // WhiteSmoke
            CornerRadius = 8
        };
        mainPanel.AddChild(statusPanel);

        // 状态标题
        statusLabel = new Label
        {
            Text = "实时状态",
            FontSize = 16,
            Bold = true,
            TextColor = Color.FromArgb(105, 105, 105), // DimGray
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 10, 0, 0)
        };
        statusPanel.AddChild(statusLabel);

        // 点击计数
        clickCountLabel = new Label
        {
            Text = $"按钮点击次数: {GameState.ButtonClickCount}",
            FontSize = 12,
            TextColor = Color.FromArgb(0, 100, 0), // DarkGreen
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 35, 0, 0)
        };
        statusPanel.AddChild(clickCountLabel);

        // 长按计数
        longPressCountLabel = new Label
        {
            Text = $"长按触发次数: {GameState.LongPressCount}",
            FontSize = 12,
            TextColor = Color.FromArgb(255, 140, 0), // DarkOrange
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 55, 0, 0)
        };
        statusPanel.AddChild(longPressCountLabel);

        // 最后操作
        lastActionLabel = new Label
        {
            Text = $"最后操作: {GameState.LastActionDescription}",
            FontSize = 12,
            TextColor = Color.FromArgb(70, 130, 180), // SteelBlue
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 75, 0, 0)
        };
        statusPanel.AddChild(lastActionLabel);
    }

    /// <summary>
    /// 创建测试按钮
    /// </summary>
    private static void CreateTestButtons()
    {
        if (mainPanel == null) return;

        var buttonsPanel = new Panel
        {
            Width = 750,
            Height = 400,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 190, 0, 0)
        };
        mainPanel.AddChild(buttonsPanel);

        // 创建不同配置的测试按钮
        CreateTestButtonsRow(buttonsPanel);
    }

    /// <summary>
    /// 创建测试按钮行
    /// </summary>
    private static void CreateTestButtonsRow(Panel parent)
    {
        var buttonWidth = 150;
        var buttonHeight = 50;
        var spacing = 40;
        var startX = 50;

        // 按钮1: 默认配置
        var button1 = new TextButton("默认配置")
        {
            Width = buttonWidth,
            Height = buttonHeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(startX, 20, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(70, 130, 180)), // SteelBlue
            TextColor = Color.White,
            FontSize = 12,
            Bold = true
        };

        var behavior1 = button1.AddTouchBehavior(); // 默认配置
        behavior1.LongPressTriggered += (s, e) => OnLongPress("默认配置按钮");
        button1.OnPointerClicked += (s, e) => OnButtonClick("默认配置按钮");
        parent.AddChild(button1);

        // 按钮2: 小缩放
        var button2 = new TextButton("小缩放(0.8)")
        {
            Width = buttonWidth,
            Height = buttonHeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(startX + (buttonWidth + spacing) * 1, 20, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(255, 99, 71)), // Tomato
            TextColor = Color.White,
            FontSize = 12,
            Bold = true
        };

        var behavior2 = button2.AddTouchBehavior(scaleFactor: 0.8f);
        behavior2.LongPressTriggered += (s, e) => OnLongPress("小缩放按钮");
        button2.OnPointerClicked += (s, e) => OnButtonClick("小缩放按钮");
        parent.AddChild(button2);

        // 按钮3: 大缩放
        var button3 = new TextButton("大缩放(1.1)")
        {
            Width = buttonWidth,
            Height = buttonHeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(startX + (buttonWidth + spacing) * 2, 20, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(60, 179, 113)), // MediumSeaGreen
            TextColor = Color.White,
            FontSize = 12,
            Bold = true
        };

        var behavior3 = button3.AddTouchBehavior(scaleFactor: 1.1f);
        behavior3.LongPressTriggered += (s, e) => OnLongPress("大缩放按钮");
        button3.OnPointerClicked += (s, e) => OnButtonClick("大缩放按钮");
        parent.AddChild(button3);

        // 按钮4: 快速动画
        var button4 = new TextButton("快速动画")
        {
            Width = buttonWidth,
            Height = buttonHeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(startX, 90, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(138, 43, 226)), // BlueViolet
            TextColor = Color.White,
            FontSize = 12,
            Bold = true
        };

        var behavior4 = button4.AddTouchBehaviorWithDuration(0.95f, 50, 500);
        behavior4.LongPressTriggered += (s, e) => OnLongPress("快速动画按钮");
        button4.OnPointerClicked += (s, e) => OnButtonClick("快速动画按钮");
        parent.AddChild(button4);

        // 按钮5: 慢速动画
        var button5 = new TextButton("慢速动画")
        {
            Width = buttonWidth,
            Height = buttonHeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(startX + (buttonWidth + spacing) * 1, 90, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(220, 20, 60)), // Crimson
            TextColor = Color.White,
            FontSize = 12,
            Bold = true
        };

        var behavior5 = button5.AddTouchBehaviorWithDuration(0.95f, 500, 500);
        behavior5.LongPressTriggered += (s, e) => OnLongPress("慢速动画按钮");
        button5.OnPointerClicked += (s, e) => OnButtonClick("慢速动画按钮");
        parent.AddChild(button5);

        // 按钮6: 短长按
        var button6 = new TextButton("短长按")
        {
            Width = buttonWidth,
            Height = buttonHeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(startX + (buttonWidth + spacing) * 2, 90, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(255, 20, 147)), // DeepPink
            TextColor = Color.White,
            FontSize = 12,
            Bold = true
        };

        var behavior6 = button6.AddTouchBehaviorWithDuration(0.95f, 150, 200);
        behavior6.LongPressTriggered += (s, e) => OnLongPress("短长按按钮");
        button6.OnPointerClicked += (s, e) => OnButtonClick("短长按按钮");
        parent.AddChild(button6);

        // 按钮7: 禁用长按
        var button7 = new TextButton("禁用长按")
        {
            Width = buttonWidth,
            Height = buttonHeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(startX, 160, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(105, 105, 105)), // DimGray
            TextColor = Color.White,
            FontSize = 12,
            Bold = true
        };

        var behavior7 = button7.AddTouchBehavior(enableLongPress: false);
        button7.OnPointerClicked += (s, e) => OnButtonClick("禁用长按按钮");
        parent.AddChild(button7);

        // 按钮8: 禁用动画
        var button8 = new TextButton("禁用动画")
        {
            Width = buttonWidth,
            Height = buttonHeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(startX + (buttonWidth + spacing) * 1, 160, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(218, 165, 32)), // Goldenrod
            TextColor = Color.White,
            FontSize = 12,
            Bold = true
        };

        var behavior8 = button8.AddTouchBehavior(enablePressAnimation: false);
        behavior8.LongPressTriggered += (s, e) => OnLongPress("禁用动画按钮");
        button8.OnPointerClicked += (s, e) => OnButtonClick("禁用动画按钮");
        parent.AddChild(button8);

        // 重置按钮
        var resetButton = new TextButton("重置计数")
        {
            Width = buttonWidth,
            Height = buttonHeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(startX + (buttonWidth + spacing) * 2, 160, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(255, 69, 0)), // OrangeRed
            TextColor = Color.White,
            FontSize = 12,
            Bold = true
        };

        resetButton.OnPointerClicked += (s, e) => ResetCounters();
        parent.AddChild(resetButton);

        // 添加长按与点击冲突测试区域
        CreateConflictTestArea(parent);

        // 添加说明标签
        CreateInstructionLabels(parent);
    }

    /// <summary>
    /// 创建说明标签
    /// </summary>
    private static void CreateInstructionLabels(Panel parent)
    {
        var instructionLabel = new Label
        {
            Text = "使用说明：点击按钮测试不同的缩放和动画效果，长按按钮测试长按检测功能",
            FontSize = 12,
            TextColor = Color.FromArgb(105, 105, 105), // DimGray
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(50, 230, 0, 0),
            TextWrap = true
        };
        parent.AddChild(instructionLabel);

        var tipLabel = new Label
        {
            Text = "💡 提示：不同按钮配置了不同的TouchBehavior参数，体验各种交互效果的差异",
            FontSize = 11,
            TextColor = Color.FromArgb(255, 140, 0), // DarkOrange
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(50, 250, 0, 0), // 从260调整到250，为冲突测试区域腾出空间
            TextWrap = true
        };
        parent.AddChild(tipLabel);
    }

    /// <summary>
    /// 处理按钮点击事件
    /// </summary>
    private static void OnButtonClick(string buttonName)
    {
        GameState.ButtonClickCount++;
        GameState.LastActionDescription = $"点击了 {buttonName}";
        GameState.LastActionTime = DateTime.Now;

        UpdateStatusDisplay();

        Game.Logger.LogInformation("🖱️ Button clicked: {buttonName} (Total: {count})", 
            buttonName, GameState.ButtonClickCount);
    }

    /// <summary>
    /// 处理长按事件
    /// </summary>
    private static void OnLongPress(string buttonName)
    {
        GameState.LongPressCount++;
        GameState.LastActionDescription = $"长按了 {buttonName}";
        GameState.LastActionTime = DateTime.Now;

        UpdateStatusDisplay();

        Game.Logger.LogInformation("🔔 Long press triggered: {buttonName} (Total: {count})", 
            buttonName, GameState.LongPressCount);
    }

    /// <summary>
    /// 重置计数器
    /// </summary>
    private static void ResetCounters()
    {
        GameState.ButtonClickCount = 0;
        GameState.LongPressCount = 0;
        GameState.LastActionDescription = "计数器已重置";
        GameState.LastActionTime = DateTime.Now;
        
        // 重置冲突测试状态
        GameState.ConflictTestClickCount = 0;
        GameState.ConflictTestLongPressCount = 0;
        GameState.ConflictTestLastEvent = "测试已重置";
        GameState.IsInLongPress = false;

        UpdateStatusDisplay();
        UpdateConflictTestDisplay();

        Game.Logger.LogInformation("🔄 Counters reset");
    }

    /// <summary>
    /// 创建长按与点击冲突测试区域
    /// </summary>
    private static void CreateConflictTestArea(Panel parent)
    {
        // 测试区域背景面板 - 调整位置和高度
        var testPanel = new Panel
        {
            Width = 780,
            Height = 180, // 增加高度以更好容纳内容
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 290, 0, 0), // 从300调整到290，留出更多底部空间
            Background = new SolidColorBrush(Color.FromArgb(245, 245, 220)) // Beige
        };
        parent.AddChild(testPanel);

        // 测试标题
        conflictTestTitleLabel = new Label
        {
            Text = "🔍 长按与点击冲突测试区域",
            FontSize = 16,
            Bold = true,
            TextColor = Color.FromArgb(139, 0, 0), // DarkRed
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 10, 0, 0)
        };
        testPanel.AddChild(conflictTestTitleLabel);

        // 测试按钮 - 使用较短的长按时间便于测试
        var conflictTestButton = new TextButton("冲突测试按钮")
        {
            Width = 140,
            Height = 50,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 40, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(255, 165, 0)), // Orange
            TextColor = Color.White,
            FontSize = 12,
            Bold = true
        };

        // 配置TouchBehavior - 使用较短的长按时间(300ms)便于测试
        var conflictBehavior = conflictTestButton.AddTouchBehaviorWithDuration(0.9f, 150, 300);
        
        // 监听长按开始事件
        conflictBehavior.LongPressStarted += (s, e) => OnConflictTestLongPressStarted();
        // 监听长按触发事件  
        conflictBehavior.LongPressTriggered += (s, e) => OnConflictTestLongPress();
        // 监听长按结束事件
        conflictBehavior.LongPressEnded += (s, e) => OnConflictTestLongPressEnded();
        
        // 监听点击事件
        conflictTestButton.OnPointerClicked += (s, e) => OnConflictTestClick();
        
        testPanel.AddChild(conflictTestButton);

        // 状态显示标签
        conflictTestStatusLabel = new Label
        {
            Text = "状态: 未开始测试",
            FontSize = 12,
            TextColor = Color.FromArgb(0, 100, 0), // DarkGreen
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(180, 40, 0, 0)
        };
        testPanel.AddChild(conflictTestStatusLabel);

        conflictTestClickLabel = new Label
        {
            Text = "点击次数: 0",
            FontSize = 12,
            TextColor = Color.FromArgb(0, 0, 139), // DarkBlue
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(180, 60, 0, 0)
        };
        testPanel.AddChild(conflictTestClickLabel);

        conflictTestLongPressLabel = new Label
        {
            Text = "长按次数: 0",
            FontSize = 12,
            TextColor = Color.FromArgb(139, 0, 139), // DarkMagenta
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(180, 80, 0, 0)
        };
        testPanel.AddChild(conflictTestLongPressLabel);

        // 测试说明
        var testInstructionLabel = new Label
        {
            Text = "📋 测试说明：\n" +
                   "• 快速点击按钮 - 应该触发点击事件\n" +
                   "• 长按按钮(>300ms) - 应该只触发长按，不触发点击\n" +
                   "• 观察计数器验证ClickLockedPointerButtons是否正确工作",
            FontSize = 11,
            TextColor = Color.FromArgb(105, 105, 105), // DimGray
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(380, 40, 0, 0),
            TextWrap = true
        };
        testPanel.AddChild(testInstructionLabel);
    }

    /// <summary>
    /// 更新状态显示
    /// </summary>
    private static void UpdateStatusDisplay()
    {
        if (clickCountLabel != null)
        {
            clickCountLabel.Text = $"🖱️ 按钮点击次数: {GameState.ButtonClickCount}";
        }

        if (longPressCountLabel != null)
        {
            longPressCountLabel.Text = $"⏱️ 长按触发次数: {GameState.LongPressCount}";
        }

        if (lastActionLabel != null)
        {
            var timeString = GameState.LastActionTime.ToString("HH:mm:ss");
            lastActionLabel.Text = $"📋 最后操作: {GameState.LastActionDescription} ({timeString})";
        }
    }

    /// <summary>
    /// 更新冲突测试显示
    /// </summary>
    private static void UpdateConflictTestDisplay()
    {
        if (conflictTestStatusLabel == null || conflictTestClickLabel == null || conflictTestLongPressLabel == null)
        {
            return;
        }

        conflictTestStatusLabel.Text = $"🔄 状态: {GameState.ConflictTestLastEvent}";
        conflictTestClickLabel.Text = $"🖱️ 点击次数: {GameState.ConflictTestClickCount}";
        conflictTestLongPressLabel.Text = $"⏱️ 长按次数: {GameState.ConflictTestLongPressCount}";
    }

    /// <summary>
    /// 冲突测试 - 长按开始事件
    /// </summary>
    private static void OnConflictTestLongPressStarted()
    {
        GameState.IsInLongPress = true;
        GameState.ConflictTestLastEvent = "长按开始 - 锁定点击事件";
        UpdateConflictTestDisplay();
        
        Game.Logger.LogInformation("🔒 ConflictTest: Long press started - Click events should be locked");
    }

    /// <summary>
    /// 冲突测试 - 长按触发事件
    /// </summary>
    private static void OnConflictTestLongPress()
    {
        GameState.ConflictTestLongPressCount++;
        GameState.ConflictTestLastEvent = $"长按触发 #{GameState.ConflictTestLongPressCount}";
        UpdateConflictTestDisplay();
        
        Game.Logger.LogInformation("🔔 ConflictTest: Long press triggered #{count}", GameState.ConflictTestLongPressCount);
    }

    /// <summary>
    /// 冲突测试 - 长按结束事件
    /// </summary>
    private static void OnConflictTestLongPressEnded()
    {
        GameState.IsInLongPress = false;
        GameState.ConflictTestLastEvent = "长按结束 - 解锁点击事件";
        UpdateConflictTestDisplay();
        
        Game.Logger.LogInformation("🔓 ConflictTest: Long press ended - Click events unlocked");
    }

    /// <summary>
    /// 冲突测试 - 点击事件
    /// </summary>
    private static void OnConflictTestClick()
    {
        GameState.ConflictTestClickCount++;
        
        if (GameState.IsInLongPress)
        {
            // 这种情况不应该发生 - 说明ClickLockedPointerButtons有问题
            GameState.ConflictTestLastEvent = $"⚠️ 异常: 长按期间触发了点击事件! #{GameState.ConflictTestClickCount}";
            Game.Logger.LogWarning("❌ ConflictTest: Click event triggered during long press! This should not happen!");
        }
        else
        {
            GameState.ConflictTestLastEvent = $"正常点击 #{GameState.ConflictTestClickCount}";
            Game.Logger.LogInformation("✅ ConflictTest: Normal click triggered #{count}", GameState.ConflictTestClickCount);
        }
        
        UpdateConflictTestDisplay();
    }
}
#endif