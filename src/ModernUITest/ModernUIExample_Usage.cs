#if CLIENT
using GameUI.Control.Primitive;
using GameUI.Control.Extensions;
using GameUI.Control.Advanced;
using GameUI.DesignSystem;
using System.Drawing;
using static GameUI.Control.Extensions.UI;
using TriggerEncapsulation.UIProperty;
using GameCore.PlayerAndUsers;
using System.Text.Json;

namespace GameEntry.ModernUITest;

/// <summary>
/// ModernUIExample使用指南
/// </summary>
/// <remarks>
/// 展示如何在实际项目中使用现代化流式UI API示例
/// </remarks>
public static class ModernUIExampleUsage
{
    private static Panel? _currentDemo;
    private static Panel? _mainSelector;

    /// <summary>
    /// 创建一个简单的演示选择界面
    /// </summary>
    /// <returns>可以选择不同演示的界面</returns>
    public static Panel CreateDemoSelector()
    {
        _mainSelector = VStack(30,  // 🎯 增加间距: 20→30
            // 🎯 大尺寸标题: 24→48px
            Title("现代化流式UI API演示", 48)
                .Padding(0, 0, 0, 20),  // 底部额外间距
            
            // 🎯 大尺寸按钮，增加内边距和字体
            CreateLargeButton("基础API演示", DesignColors.Primary)
                .Click(() => ShowDemo(ModernUIExample.BasicAPIDemo(), "基础API演示")),
            
            CreateLargeButton("语义化控件演示", DesignColors.Secondary)
                .Click(() => ShowDemo(ModernUIExample.SemanticControlsDemo(), "语义化控件演示")),
            
            CreateLargeButton("容器API演示", DesignColors.Success)
                .Click(() => ShowDemo(ModernUIExample.ContainerAPIDemo(), "容器API演示")),
            
            CreateLargeButton("Flexbox API演示", DesignColors.Primary)
                .Click(() => ShowDemo(ModernUIExample.FlexboxAPIDemo(), "Flexbox API演示")),
            
            CreateLargeButton("事件处理演示", DesignColors.Secondary)
                .Click(() => ShowDemo(ModernUIExample.EventHandlingDemo(), "事件处理演示")),
            
            CreateLargeButton("按钮智能缩放演示", DesignColors.Warning)
                .Click(() => ShowDemo(ModernUIExample.ButtonSmartScalingDemo(), "按钮智能缩放演示")),
            
            CreateLargeButton("完整应用演示", DesignColors.Error)
                .Click(() => ShowDemo(ModernUIExample.CompleteAppDemo(), "完整应用演示")),
            
            CreateLargeButton("PropertyPlayerUI测试", DesignColors.Primary)
                .Click(() => ShowDemo(CreatePropertyPlayerUITestDemo(), "PropertyPlayerUI序列化测试"))
        ).Padding(40)  // 🎯 增加外边距: 20→40
         .Background(DesignColors.Background);
        
        return _mainSelector;
    }
    
    /// <summary>
    /// 创建大尺寸按钮，适合桌面环境
    /// </summary>
    private static TextButton CreateLargeButton(string text, System.Drawing.Color backgroundColor)
    {
        return TextButton(text)
            .Background(backgroundColor)
            .TextColor(DesignColors.OnPrimary)
            .FontSize(20)           // 🎯 大字体: 默认→20px
            .Padding(32, 16)        // 🎯 大内边距: 16x8→32x16
            .CornerRadius(DesignTokens.RadiusM)             // 🎯 大圆角: 4→8
            .MinWidth(300)          // 🎯 最小宽度保证一致性
            .Bold();                // 🎯 粗体文字更清晰
    }
    
    /// <summary>
    /// 显示指定的演示界面
    /// </summary>
    /// <param name="demoPanel">要显示的演示面板</param>
    /// <param name="demoName">演示名称</param>
    private static void ShowDemo(Panel demoPanel, string demoName)
    {
        Game.Logger.LogInformation("🔄 ShowDemo called for: {0}", demoName);
        
        // 隐藏主选择器
        _mainSelector?.Hide();
        Game.Logger.LogInformation("📱 Main selector hidden");
        
        // 隐藏并移除之前的演示
        if (_currentDemo != null)
        {
            _currentDemo.Hide()
                        .RemoveFromParent(); // 🎯 链式调用更简洁
            Game.Logger.LogInformation("🗑️ Previous demo removed");
        }
        
        // 创建带返回按钮的演示界面
        _currentDemo = VStack(24,  // 🎯 增加间距: 16→24
            HStack(12,  // 🎯 增加水平间距: 8→12
                // 🎯 大尺寸返回按钮
                CreateLargeButton("← 返回", DesignColors.Secondary)
                    .Click(() => BackToSelector())
                    .MinWidth(120),  // 返回按钮较小宽度
                UI.Spacer(),
                // 🎯 大尺寸页面标题: 24→36px
                Title(demoName, 36)
                    .Bold()
            ),
            demoPanel
        ).Padding(30)  // 🎯 增加外边距: 20→30
         .Background(DesignColors.Background);
        
        // 确保演示界面全屏显示
        _currentDemo.Stretch()
                   .GrowRatio(1, 1)
                   .Show()
                   .AddToRoot();
        Game.Logger.LogInformation("✅ Demo '{0}' created and set to full screen", demoName);
    }
    
    /// <summary>
    /// 返回到主选择器
    /// </summary>
    private static void BackToSelector()
    {
        Game.Logger.LogInformation("🔙 BackToSelector called");
        
        // 隐藏并移除当前演示
        if (_currentDemo != null)
        {
            _currentDemo.Hide()
                        .RemoveFromParent(); // 🎯 链式调用更简洁
            _currentDemo = null;
            Game.Logger.LogInformation("🗑️ Current demo removed");
        }
        
        // 显示主选择器
        _mainSelector?.Stretch()
                     .GrowRatio(1, 1)
                     .Show()
                     .AddToRoot();
        Game.Logger.LogInformation("📱 Main selector restored to full screen");
    }
    
    /// <summary>
    /// 集成到现有AI系统测试界面的示例
    /// </summary>
    /// <returns>可以嵌入到AISystemTest中的界面</returns>
    public static Panel CreateEmbeddedDemo()
    {
        return VStack(12,
            Title("现代化UI API"),
            Body("展示重新设计的直观、一致性API"),
            
            HStack(8,
                Primary("查看演示")
                    .Click(() => {
                        // 这里可以打开完整演示
                    }),
                
                Secondary("API文档")
                    .Click(() => {
                        // 这里可以打开API文档
                    })
            )
        ).Background(DesignColors.Surface)
        .CornerRadius(DesignTokens.RadiusM)
        .Padding(DesignTokens.SpacingL);
    }
    
    /// <summary>
    /// 创建PropertyPlayerUI测试演示界面
    /// 测试UI属性的序列化和服务端保存功能
    /// </summary>
    /// <returns>PropertyPlayerUI测试面板</returns>
    private static Panel CreatePropertyPlayerUITestDemo()
    {
        // 状态显示文本
        var statusText = Body("🔄 准备开始测试...", 16);
        var resultText = Body("", 14);
        
        return VStack(24,
            // 标题和说明
            Title("PropertyPlayerUI 序列化测试", 32).Bold(),
            Body("测试客户端UI属性向服务端的序列化保存功能", 16)
                .TextColor(DesignColors.Secondary),
            
            UI.HDivider(), // 分隔线
            
            // 测试选项区域
            VStack(16,
                Subtitle("测试选项", 20).Bold(),
                
                // 基础类型测试
                HStack(12,
                    Primary("测试布尔值")
                        .Click(async () => await TestBooleanProperty(statusText, resultText)),
                    Secondary("测试整数")
                        .Click(async () => await TestIntegerProperty(statusText, resultText)),
                    Success("测试字符串")
                        .Click(async () => await TestStringProperty(statusText, resultText))
                ),
                
                // 复杂类型测试
                HStack(12,
                    TextButton("测试JSON对象")
                        .Background(DesignColors.Warning)
                        .TextColor(DesignColors.OnWarning)
                        .Click(async () => await TestJsonObjectProperty(statusText, resultText)),
                    TextButton("批量测试")
                        .Background(DesignColors.Primary)
                        .TextColor(DesignColors.OnPrimary)
                        .Click(async () => await TestBatchProperties(statusText, resultText)),
                    TextButton("清空所有属性")
                        .Background(DesignColors.Error)
                        .TextColor(DesignColors.OnError)
                        .Click(async () => await ClearAllProperties(statusText, resultText))
                ),
                
                // 扩展属性测试
                UI.Space(8), // 间距
                Subtitle("🔧 扩展属性测试", 16).Bold().TextColor(DesignColors.Primary),
                HStack(12,
                    TextButton("测试主题颜色")
                        .Background(DesignColors.Secondary)
                        .TextColor(DesignColors.OnSecondary)
                        .Click(async () => await TestThemeColorProperty(statusText, resultText)),
                    TextButton("测试通知设置")
                        .Background(DesignColors.Success)
                        .TextColor(DesignColors.OnSuccess)
                        .Click(async () => await TestNotificationSettings(statusText, resultText)),
                    TextButton("测试窗口布局")
                        .Background(DesignColors.Warning)
                        .TextColor(DesignColors.OnWarning)
                        .Click(async () => await TestWindowLayoutData(statusText, resultText))
                ),
                
                HStack(12,
                    TextButton("测试音频设置")
                        .Background(DesignColors.Primary)
                        .TextColor(DesignColors.OnPrimary)
                        .Click(async () => await TestAudioSettings(statusText, resultText)),
                    TextButton("测试扩展批量")
                        .Background(DesignColors.Secondary)
                        .TextColor(DesignColors.OnSecondary)
                        .Click(async () => await TestExtendedBatchProperties(statusText, resultText)),
                    TextButton("清空扩展属性")
                        .Background(DesignColors.Error)
                        .TextColor(DesignColors.OnError)
                        .Click(async () => await ClearExtendedProperties(statusText, resultText))
                )
            ).Background(DesignColors.Surface)
            .CornerRadius(DesignTokens.RadiusM)
            .Padding(DesignTokens.SpacingXL),
            
            UI.HDivider(), // 分隔线
            
            // 状态显示区域
            VStack(12,
                Subtitle("测试状态", 18).Bold(),
                statusText,
                resultText
            ).Background(DesignColors.Surface)
            .CornerRadius(DesignTokens.RadiusM)
            .Padding(DesignTokens.SpacingL),
            
            UI.HDivider(), // 分隔线
            
            // 属性查看区域
            VStack(12,
                Subtitle("当前属性值", 18).Bold(),
                Primary("刷新属性显示")
                    .Click(() => RefreshPropertyDisplay(resultText)),
                Body("点击上方按钮查看当前保存的属性值", 14)
                    .TextColor(DesignColors.Secondary)
            ).Background(DesignColors.Surface)
            .CornerRadius(DesignTokens.RadiusM)
            .Padding(DesignTokens.SpacingL)
        ).Padding(30)
         .Background(DesignColors.Background);
    }
    
    /// <summary>
    /// 测试布尔值属性
    /// </summary>
    private static async Task TestBooleanProperty(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在测试布尔值属性...");
            
            // 获取当前值并切换
            var currentValue = player.GetUIPropertyAs<bool>(PropertyPlayerUI.UIPanelCollapsed, false);
            var testValue = !currentValue;
            
            var success = await player.SetUIPropertyAsync(PropertyPlayerUI.UIPanelCollapsed, testValue);
            
            if (success)
            {
                UpdateStatus(statusText, $"✅ 布尔值属性设置成功: {testValue}");
                
                // 验证读取
                await Game.Delay(500);
                var readValue = player.GetUIPropertyAs<bool>(PropertyPlayerUI.UIPanelCollapsed, false);
                var isMatch = testValue == readValue;
                
                UpdateResult(resultText, $"设置值: {testValue}, 读取值: {readValue}, 匹配: {isMatch}");
            }
            else
            {
                UpdateStatus(statusText, "❌ 布尔值属性设置失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ 布尔值测试出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试整数属性
    /// </summary>
    private static async Task TestIntegerProperty(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在测试整数属性...");
            
            // 设置随机整数值
            var testValue = new Random().Next(1, 100);
            var success = await player.SetUIPropertyAsync(PropertyPlayerUI.ChatChannelPreference, testValue);
            
            if (success)
            {
                UpdateStatus(statusText, $"✅ 整数属性设置成功: {testValue}");
                
                // 验证读取
                await Game.Delay(100);
                var readValue = player.GetUIPropertyAs<int>(PropertyPlayerUI.ChatChannelPreference, 0);
                UpdateResult(resultText, $"设置值: {testValue}, 读取值: {readValue}, 匹配: {testValue == readValue}");
            }
            else
            {
                UpdateStatus(statusText, "❌ 整数属性设置失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ 整数测试出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试字符串属性
    /// </summary>
    private static async Task TestStringProperty(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在测试字符串属性...");
            
            // 设置字符串值
            var testValue = $"测试配置_{DateTime.Now:HHmmss}";
            var success = await player.SetUIPropertyAsync(PropertyPlayerUI.UILayoutConfig, testValue);
            
            if (success)
            {
                UpdateStatus(statusText, $"✅ 字符串属性设置成功: {testValue}");
                
                // 验证读取
                await Game.Delay(100);
                var readValue = player.GetUIPropertyAs<string>(PropertyPlayerUI.UILayoutConfig, "");
                UpdateResult(resultText, $"设置值: {testValue}, 读取值: {readValue}, 匹配: {testValue == readValue}");
            }
            else
            {
                UpdateStatus(statusText, "❌ 字符串属性设置失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ 字符串测试出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试JSON对象属性
    /// </summary>
    private static async Task TestJsonObjectProperty(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在测试JSON对象属性...");
            
            // 创建测试对象
            var testObject = new Dictionary<string, object>
            {
                { "Attack", "Space" },
                { "Inventory", "Tab" },
                { "Chat", "Enter" },
                { "timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "testNumber", new Random().Next(1, 1000) }
            };
            
            var success = await player.SetUIPropertyJsonAsync(PropertyPlayerUI.KeyBindingSettings, testObject);
            
            if (success)
            {
                UpdateStatus(statusText, "✅ JSON对象属性设置成功");
                
                // 验证读取
                await Game.Delay(100);
                var readValue = player.GetUIPropertyFromJson<Dictionary<string, object>>(
                    PropertyPlayerUI.KeyBindingSettings, new Dictionary<string, object>());
                
                var originalJson = JsonSerializer.Serialize(testObject);
                var readJson = JsonSerializer.Serialize(readValue);
                
                UpdateResult(resultText, $"原始JSON: {originalJson}\n读取JSON: {readJson}\n数据完整性: {readValue.Count} 项");
            }
            else
            {
                UpdateStatus(statusText, "❌ JSON对象属性设置失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ JSON对象测试出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试批量属性设置
    /// </summary>
    private static async Task TestBatchProperties(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在测试批量属性设置...");
            
            // 创建批量属性
            var batchProperties = new Dictionary<PropertyPlayerUI, object>
            {
                { PropertyPlayerUI.UIPanelCollapsed, true },
                { PropertyPlayerUI.ChatChannelPreference, 99 },
                { PropertyPlayerUI.UILayoutConfig, "batch_test_layout" }
            };
            
            var success = await player.SetUIPropertiesAsync(batchProperties);
            
            if (success)
            {
                UpdateStatus(statusText, "✅ 批量属性设置成功");
                
                // 验证读取
                await Game.Delay(100);
                var results = new List<string>();
                
                foreach (var kvp in batchProperties)
                {
                    object readValue = kvp.Key.InnerValue switch
                    {
                        EPropertyPlayerUI.UIPanelCollapsed => player.GetUIPropertyAs<bool>(kvp.Key, false),
                        EPropertyPlayerUI.ChatChannelPreference => player.GetUIPropertyAs<int>(kvp.Key, 0),
                        EPropertyPlayerUI.UILayoutConfig => player.GetUIPropertyAs<string>(kvp.Key, ""),
                        _ => "未知类型"
                    };
                    
                    results.Add($"{kvp.Key}: {kvp.Value} → {readValue}");
                }
                
                UpdateResult(resultText, $"批量设置验证:\n{string.Join("\n", results)}");
            }
            else
            {
                UpdateStatus(statusText, "❌ 批量属性设置失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ 批量测试出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 清空所有属性
    /// </summary>
    private static async Task ClearAllProperties(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在清空所有属性...");
            
            // 清空所有属性为默认值
            var clearProperties = new Dictionary<PropertyPlayerUI, object>
            {
                { PropertyPlayerUI.UIPanelCollapsed, false },
                { PropertyPlayerUI.ChatChannelPreference, 0 },
                { PropertyPlayerUI.UILayoutConfig, "" },
                { PropertyPlayerUI.KeyBindingSettings, "" }
            };
            
            var success = await player.SetUIPropertiesAsync(clearProperties);
            
            if (success)
            {
                UpdateStatus(statusText, "✅ 所有属性已清空");
                UpdateResult(resultText, "所有UI属性已重置为默认值");
            }
            else
            {
                UpdateStatus(statusText, "❌ 清空属性失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ 清空操作出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 刷新属性显示
    /// </summary>
    private static void RefreshPropertyDisplay(GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateResult(resultText, "❌ 找不到本地玩家");
            return;
        }
        
        try
        {
            // 基础属性
            var collapsed = player.GetUIPropertyAs<bool>(PropertyPlayerUI.UIPanelCollapsed, false);
            var chatChannel = player.GetUIPropertyAs<int>(PropertyPlayerUI.ChatChannelPreference, 0);
            var layoutConfig = player.GetUIPropertyAs<string>(PropertyPlayerUI.UILayoutConfig, "");
            var keyBindings = player.GetUIPropertyAs<string>(PropertyPlayerUI.KeyBindingSettings, "");
            
            // 扩展属性
            var theme = player.GetUIPropertyAs<string>(PropertyPlayerUIExtended.ThemeColorPreference, "");
            var language = player.GetUIPropertyAs<string>(PropertyPlayerUIExtended.LanguagePreference, "");
            var transparency = player.GetUIPropertyAs<float>(PropertyPlayerUIExtended.UITransparencyLevel, 0.0f);
            var autoSave = player.GetUIPropertyAs<int>(PropertyPlayerUIExtended.AutoSaveInterval, 0);
            var notification = player.GetUIPropertyAs<string>(PropertyPlayerUIExtended.NotificationSettings, "");
            var audio = player.GetUIPropertyAs<string>(PropertyPlayerUIExtended.AudioSettings, "");
            
            var display = $"🔧 基础属性:\n" +
                         $"• UIPanelCollapsed: {collapsed}\n" +
                         $"• ChatChannelPreference: {chatChannel}\n" +
                         $"• UILayoutConfig: \"{layoutConfig}\"\n" +
                         $"• KeyBindingSettings: {(string.IsNullOrEmpty(keyBindings) ? "空" : "有数据")}\n\n" +
                         $"🚀 扩展属性:\n" +
                         $"• 主题颜色: {(string.IsNullOrEmpty(theme) ? "未设置" : theme)}\n" +
                         $"• 语言: {(string.IsNullOrEmpty(language) ? "未设置" : language)}\n" +
                         $"• 透明度: {(transparency > 0 ? transparency.ToString("F2") : "未设置")}\n" +
                         $"• 自动保存: {(autoSave > 0 ? $"{autoSave}秒" : "未设置")}\n" +
                         $"• 通知设置: {(string.IsNullOrEmpty(notification) ? "空" : "有数据")}\n" +
                         $"• 音频设置: {(string.IsNullOrEmpty(audio) ? "空" : "有数据")}";
            
            UpdateResult(resultText, display);
        }
        catch (Exception ex)
        {
            UpdateResult(resultText, $"❌ 刷新属性出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 更新状态文本
    /// </summary>
    private static void UpdateStatus(GameUI.Control.Control statusText, string message)
    {
        if (statusText is Label label)
        {
            label.Text = message;
        }
    }
    
    /// <summary>
    /// 更新结果文本
    /// </summary>
    private static void UpdateResult(GameUI.Control.Control resultText, string message)
    {
        if (resultText is Label label)
        {
            label.Text = message;
        }
    }
    
    #region 扩展属性测试方法
    
    /// <summary>
    /// 测试主题颜色属性
    /// </summary>
    private static async Task TestThemeColorProperty(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在测试主题颜色属性...");
            
            // 设置主题颜色值 (模拟深色/浅色模式切换)
            var themes = new[] { "light", "dark", "auto", "high-contrast" };
            var testValue = themes[new Random().Next(themes.Length)];
            var success = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.ThemeColorPreference, testValue);
            
            if (success)
            {
                UpdateStatus(statusText, $"✅ 主题颜色属性设置成功: {testValue}");
                
                // 验证读取
                await Game.Delay(100);
                var readValue = player.GetUIPropertyAs<string>(PropertyPlayerUIExtended.ThemeColorPreference, "");
                UpdateResult(resultText, $"扩展属性 - 主题颜色:\n设置值: {testValue}\n读取值: {readValue}\n匹配: {testValue == readValue}");
            }
            else
            {
                UpdateStatus(statusText, "❌ 主题颜色属性设置失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ 主题颜色测试出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试通知设置属性
    /// </summary>
    private static async Task TestNotificationSettings(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在测试通知设置属性...");
            
            // 创建复杂的通知设置对象
            var notificationSettings = new
            {
                enableSound = true,
                enableVibration = false,
                enablePopups = true,
                muteHours = new { start = "22:00", end = "08:00" },
                categories = new[]
                {
                    new { name = "system", enabled = true, priority = "high" },
                    new { name = "game", enabled = true, priority = "normal" },
                    new { name = "social", enabled = false, priority = "low" }
                },
                customRingtone = "notification_sound_1.wav"
            };
            
            var success = await player.SetUIPropertyJsonAsync(PropertyPlayerUIExtended.NotificationSettings, notificationSettings);
            
            if (success)
            {
                UpdateStatus(statusText, "✅ 通知设置属性设置成功");
                
                // 验证读取
                await Game.Delay(100);
                var readValue = player.GetUIPropertyFromJson<object>(PropertyPlayerUIExtended.NotificationSettings, null);
                var readJson = JsonSerializer.Serialize(readValue);
                
                UpdateResult(resultText, $"扩展属性 - 通知设置:\n数据大小: {readJson.Length} 字符\n包含音效: {readJson.Contains("enableSound")}\n包含分类: {readJson.Contains("categories")}");
            }
            else
            {
                UpdateStatus(statusText, "❌ 通知设置属性设置失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ 通知设置测试出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试窗口布局数据属性
    /// </summary>
    private static async Task TestWindowLayoutData(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在测试窗口布局数据属性...");
            
            // 创建窗口布局数据
            var layoutData = new
            {
                mainWindow = new { x = 100, y = 50, width = 1200, height = 800, maximized = false },
                panels = new[]
                {
                    new { id = "inventory", x = 50, y = 100, width = 300, height = 400, visible = true },
                    new { id = "chat", x = 900, y = 500, width = 280, height = 200, visible = true },
                    new { id = "minimap", x = 1000, y = 50, width = 150, height = 150, visible = false }
                },
                dockingLayout = "left-right-split",
                theme = "modern-dark",
                savedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            var success = await player.SetUIPropertyJsonAsync(PropertyPlayerUIExtended.WindowLayoutData, layoutData);
            
            if (success)
            {
                UpdateStatus(statusText, "✅ 窗口布局数据设置成功");
                
                // 验证读取
                await Game.Delay(100);
                var readValue = player.GetUIPropertyFromJson<object>(PropertyPlayerUIExtended.WindowLayoutData, null);
                
                UpdateResult(resultText, $"扩展属性 - 窗口布局:\n布局方案: {layoutData.dockingLayout}\n主题: {layoutData.theme}\n面板数量: {layoutData.panels.Length}\n序列化验证: {(readValue != null ? "成功" : "失败")}");
            }
            else
            {
                UpdateStatus(statusText, "❌ 窗口布局数据设置失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ 窗口布局测试出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试音频设置属性
    /// </summary>
    private static async Task TestAudioSettings(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在测试音频设置属性...");
            
            // 创建音频设置
            var audioSettings = new
            {
                masterVolume = 0.8f,
                musicVolume = 0.6f,
                soundEffectVolume = 0.9f,
                voiceVolume = 0.7f,
                muteAll = false,
                audioQuality = "high",
                outputDevice = "default",
                enableEcho = false,
                enable3DSound = true,
                compressionEnabled = true
            };
            
            var success = await player.SetUIPropertyJsonAsync(PropertyPlayerUIExtended.AudioSettings, audioSettings);
            
            if (success)
            {
                UpdateStatus(statusText, "✅ 音频设置属性设置成功");
                
                // 验证读取
                await Game.Delay(100);
                var readValue = player.GetUIPropertyFromJson<object>(PropertyPlayerUIExtended.AudioSettings, null);
                
                UpdateResult(resultText, $"扩展属性 - 音频设置:\n主音量: {audioSettings.masterVolume}\n音效音量: {audioSettings.soundEffectVolume}\n3D音效: {(audioSettings.enable3DSound ? "启用" : "禁用")}\n质量: {audioSettings.audioQuality}");
            }
            else
            {
                UpdateStatus(statusText, "❌ 音频设置属性设置失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ 音频设置测试出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 测试扩展属性批量设置
    /// </summary>
    private static async Task TestExtendedBatchProperties(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在测试扩展属性批量设置...");
            
            // 逐个设置扩展属性，因为扩展类型不能直接批量转换为基础类型
            var success1 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.LanguagePreference, "zh-CN");
            var success2 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.UITransparencyLevel, 0.85f);
            var success3 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.AutoSaveInterval, 300); // 5分钟
            var success4 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.CustomQuickActions, JsonSerializer.Serialize(new[] { "screenshot", "record", "bookmark" }));
            
            var success = success1 && success2 && success3 && success4;
            
            if (success)
            {
                UpdateStatus(statusText, "✅ 扩展属性批量设置成功");
                
                // 验证读取
                await Game.Delay(100);
                var results = new List<string>();
                
                var language = player.GetUIPropertyAs<string>(PropertyPlayerUIExtended.LanguagePreference, "");
                var transparency = player.GetUIPropertyAs<float>(PropertyPlayerUIExtended.UITransparencyLevel, 0.0f);
                var autoSave = player.GetUIPropertyAs<int>(PropertyPlayerUIExtended.AutoSaveInterval, 0);
                var actions = player.GetUIPropertyAs<string>(PropertyPlayerUIExtended.CustomQuickActions, "");
                
                results.Add($"语言: {language}");
                results.Add($"透明度: {transparency:F2}");
                results.Add($"自动保存: {autoSave}秒");
                results.Add($"快捷操作: {(string.IsNullOrEmpty(actions) ? "无" : "已设置")}");
                
                UpdateResult(resultText, $"扩展属性批量验证:\n{string.Join("\n", results)}");
            }
            else
            {
                UpdateStatus(statusText, "❌ 扩展属性批量设置失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ 扩展属性批量测试出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 清空扩展属性
    /// </summary>
    private static async Task ClearExtendedProperties(GameUI.Control.Control statusText, GameUI.Control.Control resultText)
    {
        var player = Player.LocalPlayer;
        if (player == null)
        {
            UpdateStatus(statusText, "❌ 错误：找不到本地玩家");
            return;
        }
        
        try
        {
            UpdateStatus(statusText, "🔄 正在清空扩展属性...");
            
            // 逐个清空所有扩展属性为默认值
            var success1 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.ThemeColorPreference, "");
            var success2 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.NotificationSettings, "");
            var success3 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.WindowLayoutData, "");
            var success4 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.CustomQuickActions, "");
            var success5 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.UITransparencyLevel, 1.0f);
            var success6 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.LanguagePreference, "");
            var success7 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.AdvancedDisplayOptions, "");
            var success8 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.AudioSettings, "");
            var success9 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.AutoSaveInterval, 0);
            var success10 = await player.SetUIPropertyAsync(PropertyPlayerUIExtended.GameplayPreferences, "");
            
            var success = success1 && success2 && success3 && success4 && success5 && 
                         success6 && success7 && success8 && success9 && success10;
            
            if (success)
            {
                UpdateStatus(statusText, "✅ 所有扩展属性已清空");
                UpdateResult(resultText, "所有扩展UI属性已重置为默认值\n共清空10个扩展属性");
            }
            else
            {
                UpdateStatus(statusText, "❌ 清空扩展属性失败");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus(statusText, $"❌ 清空扩展属性出错: {ex.Message}");
        }
    }
    
    #endregion
}
#endif 