#if CLIENT
using GameUI.Control.Extensions;
using GameUI.Control.Primitive;
using GameUI.DesignSystem;
using GameUI.Device;
using GameUI.Enum;

using System.Drawing;

namespace GameEntry.UIFrameworkTest;

/// <summary>
/// UI框架测试模式 - 展示优化后的UI框架和设计系统
/// 测试内容包括：Flexbox布局、设计系统常量、响应式布局、现代化组件等
/// </summary>
public class UIFrameworkTestMode : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameUIInitialization += OnGameUIInitialization;
        Game.Logger.LogInformation("🎨 UI Framework Test Mode registered");
    }

    private static void OnGameUIInitialization()
    {
        Game.Logger.LogInformation("🎨 UIFrameworkTest: OnGameUIInitialization called. Current GameMode: {0}", Game.GameModeLink?.FriendlyName ?? "null");

        // 只在UIFrameworkTest模式下初始化UI
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.UIFrameworkTest)
        {
            Game.Logger.LogInformation("❌ UIFrameworkTest: Wrong game mode, skipping UI initialization");
            return;
        }

        Game.Logger.LogInformation("✅ UIFrameworkTest: Correct game mode, initializing UI...");

        // 直接创建测试UI并添加到视觉树
        var testUI = CreateTestUI();
        _ = testUI.HorizontalAlignment = HorizontalAlignment.Stretch;
        _ = testUI.Height = ScreenViewport.Primary.DesignResolution.Height;
        _ = testUI.AddToVisualTree();

        Game.Logger.LogInformation("🎨 UI Framework Test Mode UI initialized");

        // 取消事件注册避免重复初始化
        Game.OnGameUIInitialization -= OnGameUIInitialization;
    }

    private static PanelScrollable CreateTestUI()
    {
        return UI.ScrollableVStack(DesignTokens.SpacingM)
            //.FullScreen()
            .Background(DesignColors.Background)
            .VScroll()
            .ScrollBar(8, DesignColors.ScrollBar)
            .Padding(DesignTokens.SpacingL)
            .Add(
                    // 标题区域
                    CreateHeaderSection(),

                    // 设计系统展示区域
                    CreateDesignSystemSection(),

                    // Flexbox布局展示区域
                    CreateFlexboxSection(),

                    // 响应式布局展示区域
                    CreateResponsiveSection(),

                    // 现代化组件展示区域
                    CreateModernComponentsSection(),

                    // 间距工具展示区域
                    CreateSpacingToolsSection(),

                    // 底部按钮区域
                    CreateFooterSection()
            );
    }

    private static Panel CreateHeaderSection()
    {
        return UI.Panel()
            .Card(DesignTokens.SpacingL, DesignTokens.RadiusL, DesignTokens.ElevationM)
            .Background(DesignColors.Surface)
            .Add(
                UI.VStack(DesignTokens.SpacingS)
                    .Add(
                        UI.Title("UI框架测试").FontSize(DesignTokens.FontSizeXXL).Margin(0, DesignTokens.SpacingM).TextColor(DesignColors.OnSurface),
                        UI.Subtitle("展示优化后的Flexbox扩展和设计系统").FontSize(DesignTokens.FontSizeL).Margin(0, DesignTokens.SpacingS).TextColor(DesignColors.OnSurface),
                        UI.Body("基于现代UI设计原则，提供AI友好的布局API").FontSize(DesignTokens.FontSizeS).Margin(0, DesignTokens.SpacingXS).TextColor(DesignColors.OnSurface)
                    )
            )
        .Background(DesignColors.Surface)
        .StretchHorizontal();
    }

    private static Panel CreateDesignSystemSection()
    {
        return UI.Panel()
            .Card(DesignTokens.SpacingL, DesignTokens.RadiusM, DesignTokens.ElevationS)
            .Background(DesignColors.Surface)
            .Add(
                UI.VStack(DesignTokens.SpacingM)
                    .Add(
                        UI.Title("设计系统常量").FontSize(DesignTokens.FontSizeXL).Margin(0, DesignTokens.SpacingM).TextColor(DesignColors.OnSurface),

                    // 间距展示
                    UI.HStack(DesignTokens.SpacingM)
                        .Add(
                            UI.Label("间距系统:").TextColor(DesignColors.OnSurface),
                            UI.Label($"XS={DesignTokens.SpacingXS}px").TextColor(DesignColors.OnSurface),
                            UI.Label($"S={DesignTokens.SpacingS}px").TextColor(DesignColors.OnSurface),
                            UI.Label($"M={DesignTokens.SpacingM}px").TextColor(DesignColors.OnSurface),
                            UI.Label($"L={DesignTokens.SpacingL}px").TextColor(DesignColors.OnSurface),
                            UI.Label($"XL={DesignTokens.SpacingXL}px").TextColor(DesignColors.OnSurface)
                        ),

                    // 字体尺寸展示
                    UI.VStack(DesignTokens.SpacingS)
                        .Add(
                            UI.Label("字体尺寸系统:").FontSize(DesignTokens.FontSizeM).TextColor(DesignColors.OnSurface),
                            UI.Label("超大标题 (32px)").FontSize(DesignTokens.FontSizeXXL).Bold().TextColor(DesignColors.OnSurface),
                            UI.Label("大标题 (24px)").FontSize(DesignTokens.FontSizeXL).Bold().TextColor(DesignColors.OnSurface),
                            UI.Label("标题 (20px)").FontSize(DesignTokens.FontSizeL).Bold().TextColor(DesignColors.OnSurface),
                            UI.Label("副标题 (18px)").FontSize(DesignTokens.FontSizeM).TextColor(DesignColors.OnSurface),
                            UI.Label("正文 (16px)").FontSize(DesignTokens.FontSizeS).TextColor(DesignColors.OnSurface),
                            UI.Label("小字 (14px)").FontSize(DesignTokens.FontSizeXS).TextColor(DesignColors.OnSurface),
                            UI.Label("超小字 (12px)").FontSize(DesignTokens.FontSizeXXS).TextColor(DesignColors.OnSurface)
                        ),

                    // 圆角展示
                    UI.HStack(DesignTokens.SpacingM)
                        .Add(
                            UI.Label("圆角系统:").TextColor(DesignColors.OnSurface),
                            CreateRadiusDemo(DesignTokens.RadiusM, "中"),
                            CreateRadiusDemo(DesignTokens.RadiusL, "大"),
                            CreateRadiusDemo(DesignTokens.RadiusXL, "超大")
                        )
                    )
            )
        .Background(DesignColors.Surface)
        .StretchHorizontal();
    }

    private static Panel CreateFlexboxSection()
    {
        return UI.Panel()
            .Card(DesignTokens.SpacingL, DesignTokens.RadiusM, DesignTokens.ElevationS)
            .Background(DesignColors.Surface)
            .Add(
                UI.VStack(DesignTokens.SpacingM)
                    .Add(
                        UI.Title("Flexbox布局展示").FontSize(DesignTokens.FontSizeXL).Margin(0, DesignTokens.SpacingM).TextColor(DesignColors.OnSurface),

                    // 水平布局示例 - 使用新的HStack
                    UI.VStack(DesignTokens.SpacingM)
                        .Add(
                            UI.Label("水平布局 (HStack):").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                            UI.HStack(DesignTokens.SpacingM)
                                .Add(
                                    UI.TextButton("按钮1").Button(DesignTokens.ButtonHeightM, DesignTokens.SpacingM, DesignTokens.RadiusS)
                                        .Background(DesignColors.Primary).TextColor(DesignColors.OnPrimary),
                                    UI.TextButton("按钮2").Button(DesignTokens.ButtonHeightM, DesignTokens.SpacingM, DesignTokens.RadiusS)
                                        .Background(DesignColors.Secondary).TextColor(DesignColors.OnSecondary),
                                    UI.TextButton("按钮3").Button(DesignTokens.ButtonHeightM, DesignTokens.SpacingM, DesignTokens.RadiusS)
                                        .Background(DesignColors.Success).TextColor(DesignColors.OnSuccess)
                                )
                        ),

                    // 垂直布局示例 - 使用新的VStack
                    UI.VStack(DesignTokens.SpacingM)
                        .Add(
                            UI.Label("垂直布局 (VStack):").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                            UI.HStack(DesignTokens.SpacingL)
                                .Add(
                                    UI.VStack(DesignTokens.SpacingS)
                                        .Add(
                                            UI.Label("项目1").Body(),
                                            UI.Label("项目2").Body(),
                                            UI.Label("项目3").Body()
                                        )
                                        .Background(DesignColors.PrimarySurface)
                                        .Padding(DesignTokens.SpacingM)
                                        .CornerRadius(DesignTokens.RadiusM),

                                    UI.VStack(DesignTokens.SpacingS)
                                        .Add(
                                            UI.Label("项目A").Body(),
                                            UI.Label("项目B").Body(),
                                            UI.Label("项目C").Body()
                                        )
                                        .Background(DesignColors.SecondarySurface)
                                        .Padding(DesignTokens.SpacingM)
                                        .CornerRadius(DesignTokens.RadiusM)
                                )
                        ),

                    // Flex比例示例 - 使用新的GrowRatio方法
                    UI.VStack(DesignTokens.SpacingM)
                        .Add(
                            UI.Label("Flex比例展示:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                            UI.HStack(DesignTokens.SpacingS)
                                .Add(
                                    UI.Label("0.5").GrowRatio(0.5f, 0.5f).Background(DesignColors.Primary).Center().TextColor(DesignColors.OnPrimary),
                                    UI.Label("1.0").GrowRatio(1.0f, 1.0f).Background(DesignColors.Secondary).Center().TextColor(DesignColors.OnSecondary),
                                    UI.Label("0.5").GrowRatio(0.5f, 0.5f).Background(DesignColors.Success).Center().TextColor(Color.White)
                                )
                                .Height(200)
                                .Width(400)  // 设置固定宽度以展示比例效果
                                .Background(DesignColors.SurfaceContainer)  // 添加背景色突出父容器
                                .Border(2, DesignColors.Outline)  // 添加边框突出父容器边界
                        ),

                    // Flexbox布局特性展示
                    UI.VStack(DesignTokens.SpacingM)
                        .Add(
                            UI.Label("Flexbox布局特性:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                            
                            // WidthGrow示例 - 控制宽度增长比例
                            UI.VStack(DesignTokens.SpacingS)
                                .Add(
                                    UI.Label("WidthGrow - 控制宽度增长比例:").FontSize(DesignTokens.FontSizeS).TextColor(DesignColors.OnSurface),
                                    UI.HStack(DesignTokens.SpacingS)
                                        .Add(
                                            UI.Label("0.5").WidthGrow(0.5f).Background(DesignColors.Primary).Center().TextColor(DesignColors.OnPrimary),
                                            UI.Label("1.0").WidthGrow(1.0f).Background(DesignColors.Secondary).Center().TextColor(DesignColors.OnSecondary),
                                            UI.Label("0.5").WidthGrow(0.5f).Background(DesignColors.Success).Center().TextColor(Color.White)
                                        )
                                        .Height(DesignTokens.ButtonHeightS)
                                        .Width(350)  // 设置固定宽度以展示WidthGrow效果
                                        .Background(DesignColors.SurfaceContainer)  // 添加背景色突出父容器
                                        .Border(2, DesignColors.Outline)  // 添加边框突出父容器边界
                                ),

                            // ShrinkRatio示例 - 控制收缩比例（收缩效果不佳，暂时注释掉）
                            /*
                            UI.VStack(DesignTokens.SpacingS)
                                .Add(
                                    UI.Label("ShrinkRatio - 控制收缩比例:").FontSize(DesignTokens.FontSizeS).TextColor(DesignColors.OnSurface),
                                    UI.HStack(DesignTokens.SpacingS)
                                        .Add(
                                            UI.Label("0.5:0.5").Size(120, 120).ShrinkRatio(0.5f, 0.5f).Background(DesignColors.Warning).Center().TextColor(DesignColors.OnWarning),
                                            UI.Label("1.0:0.5").Size(120, 120).ShrinkRatio(1.0f, 2).Background(DesignColors.Error).Center().TextColor(DesignColors.OnError),
                                            UI.Label("0.5:1.0").Size(120, 120).ShrinkRatio(0.5f, 1.0f).Background(DesignColors.Info).Center().TextColor(DesignColors.OnInfo)
                                        )
                                        .Height(50)
                                        .Width(300)  // 设置固定宽度以展示ShrinkRatio效果
                                        .Background(DesignColors.SurfaceContainer)  // 添加背景色突出父容器
                                        .Border(2, DesignColors.Outline)  // 添加边框突出父容器边界
                                ),
                            */
                            
                            // FlexBasis完整展示
                            UI.VStack(DesignTokens.SpacingS)
                                .Add(
                                    UI.Label("FlexBasis - 基础尺寸:").FontSize(DesignTokens.FontSizeS).TextColor(DesignColors.OnSurface),
                                    UI.HStack(DesignTokens.SpacingS)
                                        .Add(
                                            UI.Label("100px").Size(60, 40).FlexBasis(100, 40).Background(DesignColors.Warning).Center().TextColor(DesignColors.OnWarning),
                                            UI.Label("150px").Size(60, 40).FlexBasis(150, 40).Background(DesignColors.Error).Center().TextColor(DesignColors.OnError),
                                            UI.Label("100px").Size(60, 40).FlexBasis(100, 40).Background(DesignColors.Info).Center().TextColor(DesignColors.OnInfo)
                                        )
                                        .Width(400)
                                        .Background(DesignColors.SurfaceContainer)
                                        .Border(1, DesignColors.Outline)
                                )
                        )
                    )
            )
        .Background(DesignColors.Surface)
        .StretchHorizontal();
    }

    private static Panel CreateResponsiveSection()
    {
        return UI.Panel()
            .Card(DesignTokens.SpacingL, DesignTokens.RadiusM, DesignTokens.ElevationS)
            .Background(DesignColors.Surface)
            .Add(
                UI.VStack(DesignTokens.SpacingM)
                    .Add(
                        UI.Title("响应式布局").FontSize(DesignTokens.FontSizeXL).Margin(0, DesignTokens.SpacingM).TextColor(DesignColors.OnSurface),

                    UI.VStack(DesignTokens.SpacingM)
                        .Add(
                            UI.Label("响应式容器:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                            UI.Panel()
                                .ResponsiveWidth(200, 400)
                                .Height(DesignTokens.ButtonHeightL)
                                .Background(DesignColors.Info)
                                .Center()
                                .CornerRadius(DesignTokens.RadiusM),

                            UI.Label("响应式字体:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                            UI.Label("这段文字会根据屏幕尺寸调整大小")
                                .ResponsiveFontSize(12, 16, 20)
                                .TextColor(DesignColors.OnSurface)
                                .Center()
                        )
                    )
            )
        .Background(DesignColors.Surface)
        .StretchHorizontal();
    }

    private static Panel CreateModernComponentsSection()
    {
        return UI.Panel()
            .Card(DesignTokens.SpacingL, DesignTokens.RadiusM, DesignTokens.ElevationS)
            .Background(DesignColors.Surface)
            .Add(
                UI.VStack(DesignTokens.SpacingM)
                    .Add(
                        UI.Title("现代化组件").FontSize(DesignTokens.FontSizeXL).Margin(0, DesignTokens.SpacingM).TextColor(DesignColors.OnSurface),

                    // 按钮样式展示
                    UI.VStack(DesignTokens.SpacingM)
                        .Add(
                            UI.Label("按钮样式:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                            UI.HStack(DesignTokens.SpacingM)
                                .Add(
                                    UI.Primary("主要按钮"),
                                    UI.Secondary("次要按钮"),
                                    UI.Success("成功按钮"),
                                    UI.Danger("危险按钮")
                                )
                        ),

                    // 输入框样式展示
                    UI.VStack(DesignTokens.SpacingM)
                        .Add(
                            UI.Label("输入框样式:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                            UI.VStack(DesignTokens.SpacingS)
                                .Add(
                                    UI.Input("请输入用户名...").Input(DesignTokens.InputHeightM, DesignTokens.InputWidthM),
                                    UI.Input("请输入密码...").Input(DesignTokens.InputHeightM, DesignTokens.InputWidthM),
                                    UI.Input("请输入邮箱地址...").Input(DesignTokens.InputHeightM, DesignTokens.InputWidthM)
                                )
                        ),

                    // 文本处理特性展示
                    UI.VStack(DesignTokens.SpacingM)
                        .Add(
                            UI.Label("文本处理特性:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                            
                            // TextWrap示例
                            UI.VStack(DesignTokens.SpacingS)
                                .Add(
                                    UI.Label("TextWrap - 文本换行:").FontSize(DesignTokens.FontSizeS).TextColor(DesignColors.OnSurface),
                                    UI.Label("这是一段很长的文本，用来演示文本换行功能。当文本超出控件宽度时，会自动换行显示。").Width(200).TextWrap(true).Background(DesignColors.Surface).TextColor(DesignColors.OnSurface).Padding(DesignTokens.SpacingS),
                                    UI.Label("这是不换行的文本，超出部分会被裁剪。").Width(200).TextWrap(false).Background(DesignColors.Surface).TextColor(DesignColors.OnSurface).Padding(DesignTokens.SpacingS)
                                ),
                            
                            // TextTrimming示例
                            UI.VStack(DesignTokens.SpacingS)
                                .Add(
                                    UI.Label("TextTrimming - 文本修剪:").FontSize(DesignTokens.FontSizeS).TextColor(DesignColors.OnSurface),
                                    UI.Label("这是一段很长的文本，用来演示不同的修剪方式").Width(150).TextTrimming(GameUI.Control.Enum.TextTrimming.None).Background(DesignColors.Surface).TextColor(DesignColors.OnSurface).Padding(DesignTokens.SpacingS),
                                    UI.Label("这是一段很长的文本，用来演示不同的修剪方式").Width(150).TextTrimming(GameUI.Control.Enum.TextTrimming.Ellipsis).Background(DesignColors.Surface).TextColor(DesignColors.OnSurface).Padding(DesignTokens.SpacingS),
                                    UI.Label("这是一段很长的文本，用来演示不同的修剪方式").Width(150).TextTrimming(GameUI.Control.Enum.TextTrimming.Clip).Background(DesignColors.Surface).TextColor(DesignColors.OnSurface).Padding(DesignTokens.SpacingS)
                                )
                        ),

                        // 列表项样式展示
                        UI.VStack(DesignTokens.SpacingM)
                            .Add(
                                UI.Label("列表项样式:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                                UI.VStack(DesignTokens.SpacingS)
                                    .Add(
                                        CreateListItem("列表项 1", DesignColors.Surface),
                                        CreateListItem("列表项 2", DesignColors.SurfaceVariant),
                                        CreateListItem("列表项 3", DesignColors.SurfaceContainer)
                                    )
                            ),

                        // 边框功能展示
                        UI.VStack(DesignTokens.SpacingM)
                            .Add(
                                UI.Label("边框功能:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                                UI.HStack(DesignTokens.SpacingM)
                                    .Add(
                                        UI.Label("无边框").Size(80, 40).Background(DesignColors.Primary).Center().TextColor(DesignColors.OnPrimary),
                                        UI.Label("有边框").Size(80, 40).Background(DesignColors.Secondary).Center().TextColor(DesignColors.OnSecondary).Border(2, DesignColors.Outline),
                                        UI.Label("粗边框").Size(80, 40).Background(DesignColors.Success).Center().TextColor(DesignColors.OnSuccess).Border(4, DesignColors.Error)
                                    )
                            ),

                        // 透明度功能展示
                        UI.VStack(DesignTokens.SpacingM)
                            .Add(
                                UI.Label("透明度功能:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                                UI.HStack(DesignTokens.SpacingM)
                                    .Add(
                                        UI.Label("100%").Size(80, 40).Background(DesignColors.Primary).Center().TextColor(DesignColors.OnPrimary).Opacity(1.0f),
                                        UI.Label("75%").Size(80, 40).Background(DesignColors.Primary).Center().TextColor(DesignColors.OnPrimary).Opacity(0.75f),
                                        UI.Label("50%").Size(80, 40).Background(DesignColors.Primary).Center().TextColor(DesignColors.OnPrimary).Opacity(0.5f),
                                        UI.Label("25%").Size(80, 40).Background(DesignColors.Primary).Center().TextColor(DesignColors.OnPrimary).Opacity(0.25f)
                                    )
                            ),

                        // 分隔线功能展示
                        UI.VStack(DesignTokens.SpacingM)
                            .Add(
                                UI.Label("分隔线功能:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                                UI.HStack(DesignTokens.SpacingM)
                                    .Add(
                                        UI.VStack(DesignTokens.SpacingS)
                                            .Add(
                                                UI.Label("垂直分隔线:").FontSize(DesignTokens.FontSizeS).TextColor(DesignColors.OnSurface),
                                                UI.HStack(DesignTokens.SpacingS)
                                                    .Add(
                                                        UI.Label("左").Size(60, 40).Background(DesignColors.Primary).Center().TextColor(DesignColors.OnPrimary),
                                                        UI.VDivider(2, DesignColors.Outline),
                                                        UI.Label("右").Size(60, 40).Background(DesignColors.Secondary).Center().TextColor(DesignColors.OnSecondary)
                                                    )
                                            ),
                                        UI.VStack(DesignTokens.SpacingS)
                                            .Add(
                                                UI.Label("水平分隔线:").FontSize(DesignTokens.FontSizeS).TextColor(DesignColors.OnSurface),
                                                UI.VStack(DesignTokens.SpacingS)
                                                    .Add(
                                                        UI.Label("上").Size(80, 30).Background(DesignColors.Success).Center().TextColor(DesignColors.OnSuccess),
                                                        UI.HDivider(2, DesignColors.Outline),
                                                        UI.Label("下").Size(80, 30).Background(DesignColors.Warning).Center().TextColor(DesignColors.OnWarning)
                                                    )
                                            )
                                    )
                            ),

                        // 固定间距功能展示
                        UI.VStack(DesignTokens.SpacingM)
                            .Add(
                                UI.Label("固定间距功能:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),
                                UI.VStack(DesignTokens.SpacingS)
                                    .Add(
                                        UI.Label("顶部内容").Size(100, 30).Background(DesignColors.Primary).Center().TextColor(DesignColors.OnPrimary),
                                        UI.Space(20), // 20px固定间距
                                        UI.Label("中间内容").Size(100, 30).Background(DesignColors.Secondary).Center().TextColor(DesignColors.OnSecondary),
                                        UI.Space(10), // 10px固定间距
                                        UI.Label("底部内容").Size(100, 30).Background(DesignColors.Success).Center().TextColor(DesignColors.OnSuccess)
                                    )
                            )
                    )
            )
        .Background(DesignColors.Surface)
        .StretchHorizontal();
    }

    private static Panel CreateSpacingToolsSection()
    {
        return UI.Panel()
            .Card(DesignTokens.SpacingL, DesignTokens.RadiusM, DesignTokens.ElevationS)
            .Background(DesignColors.Surface)
            .Add(
                UI.VStack(DesignTokens.SpacingM)
                    .Add(
                        UI.Title("间距工具").FontSize(DesignTokens.FontSizeXL).Margin(0, DesignTokens.SpacingM).TextColor(DesignColors.OnSurface),

                    UI.VStack(DesignTokens.SpacingM)
                        .Add(
                            UI.Label("间距工具演示:").FontSize(DesignTokens.FontSizeM).Bold().TextColor(DesignColors.OnSurface),

                            UI.Label("顶部间距").SpacingTop(DesignTokens.SpacingM).TextColor(DesignColors.OnSurface),
                            UI.Label("底部间距").SpacingBottom(DesignTokens.SpacingM).TextColor(DesignColors.OnSurface),
                            UI.Label("左侧间距").SpacingLeft(DesignTokens.SpacingM).TextColor(DesignColors.OnSurface),
                            UI.Label("右侧间距").SpacingRight(DesignTokens.SpacingM).TextColor(DesignColors.OnSurface),
                            UI.Label("垂直间距").SpacingVertical(DesignTokens.SpacingS).TextColor(DesignColors.OnSurface),
                            UI.Label("水平间距").SpacingHorizontal(DesignTokens.SpacingS).TextColor(DesignColors.OnSurface)
                        )
                    )
            )
        .Background(DesignColors.Surface)
        .StretchHorizontal();
    }

    private static Panel CreateFooterSection()
    {
        return UI.HStack(DesignTokens.SpacingM)
            .Add(
                UI.Secondary("返回")
                    .Button(DesignTokens.ButtonHeightM, DesignTokens.SpacingM, DesignTokens.RadiusS)
                    .Click(() =>
                    {
                        // 返回主菜单的逻辑
                    }),

                UI.Spacer(),

                UI.Primary("刷新")
                    .Button(DesignTokens.ButtonHeightM, DesignTokens.SpacingM, DesignTokens.RadiusS)
                    .Click(() =>
                    {
                        // 刷新测试界面的逻辑
                    })
            )
            .Padding(DesignTokens.SpacingL)     // ✅ 添加内边距
            .StretchHorizontal();
    }

    private static Panel CreateRadiusDemo(float radius, string label)
    {
        return UI.Panel()
            .Size(40, 40)
            .Background(DesignColors.Primary)
            .CornerRadius(radius)
            .Center()
            .Add(
                UI.Label(label)
                    .FontSize(DesignTokens.FontSizeXXS)
                    .TextColor(DesignColors.OnPrimary)
                    .Center()
            );
    }

    private static Panel CreateListItem(string text, Color backgroundColor)
    {
        return UI.Panel()
            .ListItem(DesignTokens.ButtonHeightL, DesignTokens.SpacingM)
            .Background(backgroundColor)  // 覆盖默认的Surface背景色
            .Add(
                UI.Label(text)
                    .Body()
                    .Center()
            );
    }
}
#endif
