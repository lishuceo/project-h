#if CLIENT
using GameUI.Control.Extensions;
using GameUI.Control.Primitive;
using GameUI.DesignSystem;
using static GameUI.Control.Extensions.UI;

namespace GameEntry.ModernUITest;

/// <summary>
/// 现代化流式UI API示例
/// </summary>
/// <remarks>
/// 展示重新设计后的直观、一致性API的使用方法。
/// 重点展示命名的改进和API的简洁性。
/// </remarks>
public static class ModernUIExample
{
    /// <summary>
    /// 展示基础API的改进
    /// </summary>
    public static Panel BasicAPIDemo()
    {
        return VStack(16,
            // ✅ 新API：直观的命名
            Title("现代化API演示")
                .Show()           // 代替 Visible(true)
                .Enable(),        // 代替 Enabled(true)
            
            Body("✅ Show() 方法 - 代替 Visible(true)")
                .Show(),
            
            Body("✅ Enable() 方法 - 代替 Enabled(true)")
                .Enable(),
            
            Body("✅ CornerRadius() 方法 - 设置圆角")
                .CornerRadius(DesignTokens.RadiusS),
            
            Body("这是正文内容示例")
                .CornerRadius(DesignTokens.RadiusS)       // 设置圆角
                .ZIndex(1),            // 设置层级
            
            Caption("这是说明文字 - opacity 0.7")
                .Opacity(0.7f),        // 设置透明度
            
            // 添加一些测试按钮
            HStack(8,
                Primary("主要按钮"),
                Secondary("次要按钮")
            )
        ).Background(DesignColors.Surface)  // 添加背景色让Panel可见
        .Padding(DesignTokens.SpacingL);
    }
    
    /// <summary>
    /// 展示语义化控件
    /// </summary>
    public static Panel SemanticControlsDemo()
    {
        return VStack(12,
            Title("语义化控件"),
            Subtitle("现代设计系统"),
            Body("使用语义化的控件名称，更符合设计系统的理念"),
            
            HStack(8,
                Primary("主要操作"),
                Secondary("次要操作"),
                Success("成功状态"),
                Danger("危险操作")
            )
        ).Background(DesignColors.Surface)
        .CornerRadius(DesignTokens.RadiusM)
        .Padding(DesignTokens.SpacingXL);
    }
    
    /// <summary>
    /// 展示简化的容器API
    /// </summary>
    public static Panel ContainerAPIDemo()
    {
        return VStack(16,
            Title("容器API简化"),
            
            // ✅ 新API：简洁的命名
            VStack(8,
                Body("垂直堆叠容器"),
                HStack(4,
                    Label("项目1"),
                    Label("项目2"),
                    UI.Spacer(),  // 使用UI.Spacer()
                    Label("项目3")
                )
            ).Background(DesignColors.Surface)
            .CornerRadius(DesignTokens.RadiusM)
            .Padding(DesignTokens.SpacingL),
            
            // ✅ 新API：直观的滚动容器
            UI.ScrollableVStack(8,
                Body("列表项 1"),
                Body("列表项 2"),
                Body("列表项 3"),
                Body("列表项 4"),
                Body("列表项 5")
            ).Size(300, 150),
            
            // ✅ 新API：水平滚动容器
            UI.ScrollableHStack(8,
                Panel().Size(100, 60).Background(DesignColors.Primary),
                Panel().Size(100, 60).Background(DesignColors.Secondary),
                Panel().Size(100, 60).Background(DesignColors.Success),
                Panel().Size(100, 60).Background(DesignColors.Warning),
                Panel().Size(100, 60).Background(DesignColors.Error)
            ).Size(300, 80),
            
            HStack(16,
                // ✅ 新API：简化的分隔线
                Panel().Size(100, 50).Background(DesignColors.Primary),
                UI.HDivider(),  // 水平分隔线
                Panel().Size(100, 50).Background(DesignColors.Secondary)
            ),
            
            // ✅ 新API：垂直分隔线示例
            HStack(16,
                VStack(8,
                    Body("左侧内容"),
                    Body("更多内容")
                ),
                UI.VDivider(),  // 垂直分隔线
                VStack(8,
                    Body("右侧内容"),
                    Body("更多内容")
                )
            )
        );
    }
    
    /// <summary>
    /// 展示现代化的Flexbox API
    /// </summary>
    public static Panel FlexboxAPIDemo()
    {
        return VStack(16,
            Title("Flexbox API"),
            
            // ✅ 新API：CSS标准命名
            HStack(8,
                Panel()
                    .Background(DesignColors.Primary)
                    .Size(100, 50),     // 固定尺寸
                
                Panel()
                    .Background(DesignColors.Secondary)
                    .GrowRatio(1, 1),             // flex: 1 1 auto
                
                Panel()
                    .Background(DesignColors.Success)
                    .GrowRatio(2, 1)        // flex: 2 0 0
            ).Width(400).Background(DesignColors.SurfaceContainer).Border(2),
            
            // ✅ 新API：直观的增长/收缩
            HStack(8,
                Panel()
                    .Background(DesignColors.Warning)
                    .Size(80, 40)
                    .WidthGrow(0.5f),           // 代替 StretchRatio
                
                Panel()
                    .Background(DesignColors.Error)
                    .Size(80, 40)
                    .WidthGrow(1.0f)
                    .WidthShrink(0.5f)       // 代替 CompactRatio
            ).Width(350).Background(DesignColors.SurfaceContainer).Border(2),
            
            // ✅ 新API：Flexbox基础属性演示
            HStack(8,
                Panel()
                    .Background(DesignColors.Primary)
                    .Size(60, 40)
                    .FlexBasisWidth(100),    // flex-basis
                
                Panel()
                    .Background(DesignColors.Secondary)
                    .Size(60, 40)
                    .FlexBasisHeight(50),   // flex-basis
                
                Panel()
                    .Background(DesignColors.Success)
                    .Size(60, 40)
                    .FlexBasis(120, 60)     // flex-basis 宽高
            ).Width(400).Background(DesignColors.SurfaceContainer).Border(2),
            
            // ✅ 新API：Space() 方法演示
            VStack(8,
                Body("Space() 方法演示"),
                UI.Space(8),                // 固定8像素间距
                Body("这是间距后的内容"),
                UI.Space(16),               // 固定16像素间距
                Body("这是更大间距后的内容")
            )
        );
    }
    
    /// <summary>
    /// 展示事件处理的改进
    /// </summary>
    public static Panel EventHandlingDemo()
    {
        var statusLabel = UI.Label("状态: 等待交互").TextColor(DesignColors.OnSurface);
        var counterLabel = UI.Label("点击计数: 0").TextColor(DesignColors.OnSurface);
        var mousePositionLabel = UI.Label("鼠标位置: (0, 0)").TextColor(DesignColors.OnSurface);
        var lastEventLabel = UI.Label("最后事件: 无").TextColor(DesignColors.OnSurface);
        
        int clickCount = 0;
        
        return VStack(16,
            Title("事件处理演示"),
            Body("展示各种事件处理功能和交互效果"),
            
            // 状态显示区域
            VStack(8,
                Subtitle("交互状态"),
                statusLabel,
                counterLabel,
                mousePositionLabel,
                lastEventLabel
            )
            .Background(DesignColors.SurfaceContainer)
            .CornerRadius(DesignTokens.RadiusM)
            .Padding(DesignTokens.SpacingL),
            
            // 基础点击事件
            VStack(12,
                Subtitle("基础点击事件"),
                UI.Primary("简单点击")
                    .Click((sender, e) => {
                        clickCount++;
                        counterLabel.Text = $"点击计数: {clickCount}";
                        lastEventLabel.Text = "最后事件: 简单点击";
                        statusLabel.Text = "状态: 简单点击完成";
                        if (e.PointerPosition.HasValue)
                            mousePositionLabel.Text = $"鼠标位置: ({e.PointerPosition.Value.Left:F0}, {e.PointerPosition.Value.Top:F0})";
                    }),
                
                UI.Secondary("详细点击事件")
                    .Click((sender, e) => {
                        clickCount++;
                        counterLabel.Text = $"点击计数: {clickCount}";
                        lastEventLabel.Text = "最后事件: 详细点击";
                        statusLabel.Text = "状态: 详细点击完成";
                        if (e.PointerPosition.HasValue)
                            mousePositionLabel.Text = $"鼠标位置: ({e.PointerPosition.Value.Left:F0}, {e.PointerPosition.Value.Top:F0})";
                    })
            )
            .Background(DesignColors.Surface)
            .CornerRadius(DesignTokens.RadiusM)
            .Padding(DesignTokens.SpacingL),
            
            // 鼠标事件
            VStack(12,
                Subtitle("鼠标事件"),
                UI.Success("鼠标进入/离开测试")
                    .MouseEnter(() => {
                        lastEventLabel.Text = "最后事件: 鼠标进入";
                        statusLabel.Text = "状态: 鼠标悬停中";
                        // 鼠标进入/离开事件无法获取位置信息
                    })
                    .MouseLeave(() => {
                        lastEventLabel.Text = "最后事件: 鼠标离开";
                        statusLabel.Text = "状态: 鼠标已离开";
                        // 鼠标进入/离开事件无法获取位置信息
                    }),
                
                UI.TextButton("双击测试")
                    .Background(DesignColors.Warning)
                    .TextColor(DesignColors.OnWarning)
                    .DoubleClick(() => {
                        clickCount += 2;
                        counterLabel.Text = $"点击计数: {clickCount}";
                        lastEventLabel.Text = "最后事件: 双击";
                        statusLabel.Text = "状态: 双击完成";
                        // 双击事件无法获取位置信息
                    })
            )
            .Background(DesignColors.Surface)
            .CornerRadius(DesignTokens.RadiusM)
            .Padding(DesignTokens.SpacingL),
            
            // 输入框事件
            VStack(12,
                Subtitle("输入框事件"),
                UI.Input("输入文本...")
                    .Width(200)
                    .Click((sender, e) => {
                        lastEventLabel.Text = "最后事件: 输入框点击";
                        statusLabel.Text = "状态: 输入框获得焦点";
                        if (e.PointerPosition.HasValue)
                            mousePositionLabel.Text = $"鼠标位置: ({e.PointerPosition.Value.Left:F0}, {e.PointerPosition.Value.Top:F0})";
                    })
            )
            .Background(DesignColors.Surface)
            .CornerRadius(DesignTokens.RadiusM)
            .Padding(DesignTokens.SpacingL),
            
            // 动态内容区域
            VStack(12,
                Subtitle("动态内容"),
                UI.TextButton("添加内容")
                    .Background(DesignColors.Info)
                    .TextColor(DesignColors.OnInfo)
                    .Click((sender, e) => {
                        // 这里可以动态添加内容
                        lastEventLabel.Text = "最后事件: 添加内容";
                        statusLabel.Text = "状态: 内容已添加";
                        if (e.PointerPosition.HasValue)
                            mousePositionLabel.Text = $"鼠标位置: ({e.PointerPosition.Value.Left:F0}, {e.PointerPosition.Value.Top:F0})";
                    }),
                
                UI.Danger("重置状态")
                    .Click((sender, e) => {
                        clickCount = 0;
                        counterLabel.Text = "点击计数: 0";
                        lastEventLabel.Text = "最后事件: 重置";
                        statusLabel.Text = "状态: 已重置";
                        if (e.PointerPosition.HasValue)
                            mousePositionLabel.Text = $"鼠标位置: ({e.PointerPosition.Value.Left:F0}, {e.PointerPosition.Value.Top:F0})";
                    })
            )
            .Background(DesignColors.Surface)
            .CornerRadius(DesignTokens.RadiusM)
            .Padding(DesignTokens.SpacingL),
            
            // 说明文本
            Caption("提示: 尝试点击不同的按钮，观察状态变化和事件信息更新")
        );
    }
    
    /// <summary>
    /// 展示按钮智能缩放功能
    /// </summary>
    /// <remarks>
    /// 测试按钮如何根据文本内容、字体大小自动调整尺寸
    /// </remarks>
    public static Panel ButtonSmartScalingDemo()
    {
        return VStack(20,
            Title("按钮智能缩放演示"),
            Body("演示按钮如何根据文本内容和字体大小自动调整尺寸"),
            
            // 测试不同长度的文本
            VStack(16,
                Subtitle("不同文本长度测试"),
                Caption("所有按钮都使用默认Auto尺寸，观察自动缩放效果"),
                
                VStack(12,
                    // 短文本
                    TextButton("短")
                        .Background(DesignColors.Primary)
                        .TextColor(DesignColors.OnPrimary),
                    
                    // 中等长度文本
                    TextButton("中等长度文本")
                        .Background(DesignColors.Primary)
                        .TextColor(DesignColors.OnPrimary),
                    
                    // 长文本
                    TextButton("这是一段很长的按钮文字内容用于测试智能缩放")
                        .Background(DesignColors.Primary)
                        .TextColor(DesignColors.OnPrimary),
                    
                    // 超长文本
                    TextButton("超长文本：这是一个非常非常长的按钮文字，用来测试按钮的自动宽度调整功能是否正常工作")
                        .Background(DesignColors.Primary)
                        .TextColor(DesignColors.OnPrimary)
                ).Center()
            ).Background(DesignColors.Surface)
            .CornerRadius(DesignTokens.RadiusM)
            .Padding(DesignTokens.SpacingL),
            
            // 测试不同字体大小
            VStack(16,
                Subtitle("不同字体大小测试"),
                Caption("相同文本内容，不同字体大小的缩放效果"),
                
                VStack(12,
                    // 小字体
                    TextButton("标准测试文本")
                        .Background(DesignColors.Secondary)
                        .TextColor(DesignColors.OnSecondary)
                        .FontSize(12),
                    
                    // 默认字体
                    TextButton("标准测试文本")
                        .Background(DesignColors.Secondary)
                        .TextColor(DesignColors.OnSecondary)
                        .FontSize(16),
                    
                    // 大字体
                    TextButton("标准测试文本")
                        .Background(DesignColors.Secondary)
                        .TextColor(DesignColors.OnSecondary)
                        .FontSize(20),
                    
                    // 超大字体
                    TextButton("标准测试文本")
                        .Background(DesignColors.Secondary)
                        .TextColor(DesignColors.OnSecondary)
                        .FontSize(24)
                ).Center()
            ).Background(DesignColors.Surface)
            .CornerRadius(DesignTokens.RadiusM)
            .Padding(DesignTokens.SpacingL)
        );
    }
    
    /// <summary>
    /// 展示完整应用界面
    /// </summary>
    public static Panel CompleteAppDemo()
    {
        return VStack(0,
            // 标题栏
            HStack(12,
                Title("应用标题", 20)
                    .TextColor(DesignColors.OnPrimary),
                UI.Spacer(),
                Secondary("设置")
                    .TextColor(DesignColors.OnPrimary)
            ).Background(DesignColors.Primary)
            .Height(60)
            .Padding(DesignTokens.SpacingL),
            
            // 主内容区域
            HStack(0,
                // 侧边栏
                VStack(12,
                    Subtitle("导航菜单"),
                    Primary("首页"),
                    Secondary("设置"),
                    Success("帮助"),
                    Danger("退出")
                ).Background(DesignColors.Surface)
                .Width(200)
                .Padding(DesignTokens.SpacingL),
                
                // 内容区域
                VStack(16,
                    Title("主要内容区域"),
                    Body("这是一个完整的应用界面示例，展示了现代化UI API的实际应用。"),
                    
                    VStack(12,
                        Subtitle("功能卡片"),
                        Body("这里可以放置各种功能模块"),
                        HStack(8,
                            Primary("操作1"),
                            Secondary("操作2")
                        )
                    ).Background(DesignColors.Surface)
                    .CornerRadius(DesignTokens.RadiusM)
                    .Padding(DesignTokens.SpacingL)
                ).Background(DesignColors.Background)
                .GrowRatio(1, 1)
                .Padding(DesignTokens.SpacingXL)
            )
        ).Background(DesignColors.Background)
        .Stretch()
        .GrowRatio(1, 1);
    }
}
#endif