#if CLIENT
using GameCore;
using GameUI.Control;
using GameUI.Control.Primitive;
using GameUI.Control.Enum;
using GameUI.Control.Struct;
using GameUI.Control.Extensions;
using GameUI.Brush;
using GameUI.Graphics;
using GameUI.Struct;
using System.Drawing;
using GameUI.Enum;
using static GameUI.Control.Extensions.UI;

namespace GameEntry.AVGSystem;

/// <summary>
/// 增强版对话UI - 基于设计稿实现，包含角色头像和现代化布局
/// </summary>
internal class AVGDialogUIEnhanced
{
    // UI组件
    private Panel? mainContainer;
    private Panel? dialogContainer;
    // 注意：背景图片现在由AVGFullScreen统一管理
    private Panel? gradientTopPanel;    // 上部20%渐变区域
    private Panel? solidBottomPanel;    // 下部80%纯色区域
    private Panel? choiceContainer;
    
    // 对话相关组件
    private Label? titleSpeakerLabel;   // 标题&说话人
    private Panel? separatorLine;       // 分割线
    private Label? contentLabel;        // 正文内容
    // 继续按钮已移除
    
    // 选择相关组件
    private Label? choiceTitleLabel;
    private List<Button> choiceButtons = new();
    
    // UI层级
    private const int LAYER_AVG_MAIN = 10;     // 主对话层级
    private const int LAYER_AVG_CHOICE = 11;   // 选择层级（稍高）
    
    // 20:9分辨率设计参数 - 动态高度
    private const double DIALOG_HEIGHT_RATIO = 0.3; // 对话框高度为屏幕高度的30%
    private const int MARGIN_STANDARD = 40;         // 标准间距
    
    // 选择UI参数
    private static readonly int CHOICE_WIDTH = (int)(1920 * 0.25); // 选择面板宽度：屏幕宽度的25%
    private const int CHOICE_RIGHT_MARGIN = 128;                   // 选择面板右边距

    public AVGDialogUIEnhanced()
    {
        CreateEnhancedDialogUI();
    }

    /// <summary>
    /// 创建增强版对话UI - 基于设计稿
    /// </summary>
    private void CreateEnhancedDialogUI()
    {
        // === 主容器 - 全屏覆盖，底部对话区域 ===
        mainContainer = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            WidthStretchRatio = 1.0f,
            HeightStretchRatio = 1.0f,
            ZIndex = LAYER_AVG_MAIN,
            Background = new SolidColorBrush(Color.Transparent), // 完全透明
            Visible = false
        };

        // 注意：背景图片现在由AVGFullScreen统一管理，不在对话容器中设置
        var dialogHeight = 360; // 固定高度360px

        // === 对话容器 - 全屏宽度，贴底边显示，透明容器 ===
        dialogContainer = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            WidthStretchRatio = 1.0f, // 全屏宽度
            Height = dialogHeight,
            Margin = new Thickness(0, 0, 0, 0), // 贴底边，无边距
            Background = new SolidColorBrush(Color.Transparent), // 透明容器，用于放置文字
            ZIndex = LAYER_AVG_MAIN, // 文字层级
            Visible = false
        };

        // === 下部60%区域 - 纯90%黑色半透明 ===
        solidBottomPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            WidthStretchRatio = 1.0f,
            Height = (int)(dialogHeight * 0.6), // 下部60% = 216px
            Margin = new Thickness(0, 0, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(230, 0, 0, 0)), // 90%黑色半透明
            ZIndex = LAYER_AVG_MAIN - 1, // 在文字容器后面
            Visible = false
        };

        // === 上部40%区域 - 从完全透明渐变到90%黑色半透明 ===
        gradientTopPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            WidthStretchRatio = 1.0f,
            Height = (int)(dialogHeight * 0.4), // 上部40% = 144px
            Margin = new Thickness(0, 0, 0, (int)(dialogHeight * 0.6)), // 定位到上部区域
            Background = new GradientBrush
            {
                // 上部40%的渐变：从完全透明到深色
                TopLeft = Color.FromArgb(0, 0, 0, 0),       // 顶部：完全透明 (透明度0)
                TopRight = Color.FromArgb(0, 0, 0, 0),      // 顶部：完全透明 (透明度0)
                BottomLeft = Color.FromArgb(230, 0, 0, 0),  // 底部：90%黑色半透明
                BottomRight = Color.FromArgb(230, 0, 0, 0)  // 底部：90%黑色半透明
            },
            ZIndex = LAYER_AVG_MAIN - 1, // 在文字容器后面
            Visible = false
        };

        // === 对话容器内布局设计 ===
        // 计算文字区域：屏幕左右各15%，中间70%用于文字
        var screenWidth = 1920; // 20:9标准宽度
        var textAreaWidth = (int)(screenWidth * 0.7); // 中间70%
        var sideMargin = (int)(screenWidth * 0.15);   // 左右各15%

        // === 1. 标题&说话人（可为空） ===
        titleSpeakerLabel = new Label
        {
            Text = "", // 默认为空
            FontSize = 40, // 符合规范：标题40px
            TextColor = new SolidColorBrush(Color.FromArgb(255, 0xD1, 0xCC, 0xC6)), // D1CCC6颜色
            Bold = true, // 加粗效果
            HorizontalAlignment = HorizontalAlignment.Center, // Label容器居中
            VerticalAlignment = VerticalAlignment.Top,
            Width = textAreaWidth, // 70%宽度
            Margin = new Thickness(0, 80, 0, 0), // 增加顶部边距到80px
            HorizontalContentAlignment = GameUI.Enum.HorizontalContentAlignment.Left, // 文字在Label内左对齐
        };

        // === 2. 分割线（容器宽度的80%） ===
        separatorLine = new Panel
        {
            Width = (int)(screenWidth * 0.8), // 容器宽度的80%
            Height = 2, // 2px高度的线
            HorizontalAlignment = HorizontalAlignment.Center, // 居中显示
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 140, 0, 0), // 在标题下方，增大间距到140px
            Background = new SolidColorBrush(Color.FromArgb(77, 0xD9, 0xD9, 0xD9)), // D9D9D9颜色，30%透明度
        };

        // === 3. 正文内容 ===
        contentLabel = new Label
        {
            Text = "对话内容",
            FontSize = 36, // 符合规范：正文36px
            TextColor = new SolidColorBrush(Color.FromArgb(255, 0xD1, 0xCC, 0xC6)), // D1CCC6颜色，与人名相同
            HorizontalAlignment = HorizontalAlignment.Center, // Label容器居中
            VerticalAlignment = VerticalAlignment.Top,
            Width = textAreaWidth, // 70%宽度
            Margin = new Thickness(0, 180, 0, 0), // 增加顶部边距到180px，在分割线下方
            HorizontalContentAlignment = GameUI.Enum.HorizontalContentAlignment.Left, // 文字在Label内左对齐
        };

        // 继续按钮已移除 - 现在只通过点击对话框继续

        // === 选择容器 - 右侧独立选择面板 ===
        choiceContainer = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Right,    // 右侧对齐
            VerticalAlignment = VerticalAlignment.Center,       // 垂直居中
            Width = CHOICE_WIDTH,                               // 宽度为屏幕的25%
            Height = 600,                                       // 足够的高度容纳选项
            Margin = new Thickness(0, 0, CHOICE_RIGHT_MARGIN, 0), // 右边距128px
            Background = new SolidColorBrush(Color.FromArgb(240, 20, 25, 35)), // 深色半透明背景
            ZIndex = LAYER_AVG_CHOICE,
            Visible = false
        };

        // === 设置父子关系 ===
        // 背景层 - 添加到主容器（按层级顺序）
        // 注意：背景图片现在由AVGFullScreen统一管理
        solidBottomPanel.Parent = mainContainer;     // 下部80%纯色背景
        gradientTopPanel.Parent = mainContainer;     // 上部20%渐变背景
        
        // 对话内容系统 - 三部分结构
        titleSpeakerLabel.Parent = dialogContainer;  // 1. 标题&说话人
        separatorLine.Parent = dialogContainer;      // 2. 分割线
        contentLabel.Parent = dialogContainer;       // 3. 正文内容
        
        // 继续按钮已移除
        
        // 主容器关系
        dialogContainer.Parent = mainContainer;
        
        // 添加到根视图
        mainContainer.AddToRoot();
        choiceContainer.AddToRoot();
        
        // === 事件绑定 ===
        // 点击对话框继续 - 支持多种模式
        dialogContainer.OnPointerClicked += (sender, e) => {
            AVGEnhanced.OnDialogContinue();
            AVGFullScreen.OnDialogContinue();
        };
        // 继续按钮事件已移除
        
        Game.Logger.LogInformation("✨ 增强版AVG对话UI创建完成");
    }


    /// <summary>
    /// 设置对话内容
    /// </summary>
    public void SetDialog(string speaker, string content)
    {
        // 设置标题&说话人（如果说话人不为空）
        if (titleSpeakerLabel != null)
        {
            titleSpeakerLabel.Text = string.IsNullOrEmpty(speaker) ? "" : speaker;
            titleSpeakerLabel.Visible = !string.IsNullOrEmpty(speaker); // 空时隐藏
        }
        
        // 设置分割线（只有说话人不为空时显示）
        if (separatorLine != null)
        {
            separatorLine.Visible = !string.IsNullOrEmpty(speaker);
        }
        
        // 设置正文内容
        if (contentLabel != null)
        {
            contentLabel.Text = content;
        }
    }


    /// <summary>
    /// 显示对话
    /// </summary>
    public void ShowDialog()
    {
        if (mainContainer != null && dialogContainer != null)
        {
            mainContainer.Visible = true;
            
            // 显示分层背景（从底层到顶层）
            if (solidBottomPanel != null) solidBottomPanel.Visible = true;         // 下部80%纯色
            if (gradientTopPanel != null) gradientTopPanel.Visible = true;         // 上部20%渐变
            
            dialogContainer.Visible = true; // 显示文字内容
        }
        
        // 隐藏选择容器
        if (choiceContainer != null)
        {
            choiceContainer.Visible = false;
        }
    }

    /// <summary>
    /// 隐藏对话
    /// </summary>
    public void HideDialog()
    {
        if (dialogContainer != null)
        {
            dialogContainer.Visible = false;
        }
        
        // 隐藏分层背景
        if (solidBottomPanel != null) solidBottomPanel.Visible = false;
        if (gradientTopPanel != null) gradientTopPanel.Visible = false;
        
        if (mainContainer != null)
        {
            mainContainer.Visible = false;
        }
    }

    /// <summary>
    /// 设置选择内容
    /// </summary>
    public void SetChoice(string title, string[] choices)
    {
        ClearChoiceButtons();
        
        if (choiceContainer == null) return;

        // === 选择标题 ===
        choiceTitleLabel = new Label
        {
            Text = title,
            FontSize = 32,                                          // 适合右侧面板的字体大小
            TextColor = new SolidColorBrush(Color.FromArgb(255, 255, 215, 0)), // 金色
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 20, 0, 0),                   // 顶部边距20px
            Bold = true,                                            // 加粗标题
        };
        choiceTitleLabel.Parent = choiceContainer;

        // === 创建选择按钮 - 右侧面板设计 ===
        for (int i = 0; i < choices.Length; i++)
        {
            var button = new Button
            {
                Width = CHOICE_WIDTH - 40,                          // 面板宽度减去左右边距
                Height = 60,                                        // 适中的按钮高度
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 80 + i * 70, 0, 0),     // 从80px开始，间隔70px
                Background = new SolidColorBrush(Color.FromArgb(220, 45, 55, 75)), // 深色背景，更高透明度
                ZIndex = LAYER_AVG_CHOICE + 1,
            };

            // 按钮文字
            var buttonLabel = new Label
            {
                Text = $"{i + 1}. {choices[i]}",                    // 添加序号
                FontSize = 28,                                      // 适合右侧面板的字体大小
                TextColor = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220)), // 浅色文字
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = GameUI.Enum.HorizontalContentAlignment.Left, // 文字左对齐
            };
            buttonLabel.Parent = button;

            // 按钮点击事件 - 支持多种模式
            var choiceIndex = i;
            button.OnPointerClicked += (sender, e) => {
                AVGEnhanced.OnChoiceSelected(choiceIndex);
                AVGFullScreen.OnChoiceSelected(choiceIndex);
            };
            
            button.Parent = choiceContainer;
            choiceButtons.Add(button);
        }
    }

    /// <summary>
    /// 显示选择
    /// </summary>
    public void ShowChoice()
    {
        // 隐藏对话，显示选择
        if (dialogContainer != null)
        {
            dialogContainer.Visible = false;
        }
        
        if (choiceContainer != null)
        {
            choiceContainer.Visible = true;
        }
        
        if (mainContainer != null)
        {
            mainContainer.Visible = true;
        }
    }

    /// <summary>
    /// 隐藏选择
    /// </summary>
    public void HideChoice()
    {
        if (choiceContainer != null)
        {
            choiceContainer.Visible = false;
        }
        
        if (mainContainer != null)
        {
            mainContainer.Visible = false;
        }
        
        ClearChoiceButtons();
    }

    /// <summary>
    /// 清理选择按钮
    /// </summary>
    private void ClearChoiceButtons()
    {
        foreach (var button in choiceButtons)
        {
            button.RemoveFromParent();
        }
        choiceButtons.Clear();
        
        if (choiceTitleLabel != null)
        {
            choiceTitleLabel.RemoveFromParent();
            choiceTitleLabel = null;
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Cleanup()
    {
        try
        {
            ClearChoiceButtons();
            
            if (mainContainer != null)
            {
                mainContainer.RemoveFromParent();
                mainContainer = null;
            }
            
            if (choiceContainer != null)
            {
                choiceContainer.RemoveFromParent();
                choiceContainer = null;
            }
            
            // 清理所有引用
            dialogContainer = null;
            gradientTopPanel = null;
            solidBottomPanel = null;
            titleSpeakerLabel = null;
            separatorLine = null;
            contentLabel = null;
            // continueButton = null; // 已移除
            
            Game.Logger.LogInformation("🧹 增强版AVG对话UI清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 增强版AVG对话UI清理失败");
        }
    }
}

/// <summary>
/// AVG系统增强版 - 使用新的对话UI
/// </summary>
public static class AVGEnhanced
{
    // 当前对话UI实例
    private static AVGDialogUIEnhanced? currentDialogUI;
    private static bool isDialogActive = false;
    
    // 对话完成的回调
    private static TaskCompletionSource<int>? dialogCompletionSource;
    private static TaskCompletionSource<int>? choiceCompletionSource;

    /// <summary>
    /// 显示增强版对话
    /// </summary>
    public static async Task ShowDialog(string speaker, string text)
    {
        try
        {
            Game.Logger.LogInformation("💬 AVG增强对话：{Speaker} - {Text}", speaker, text);
            
            // 创建对话UI
            if (currentDialogUI == null)
            {
                currentDialogUI = new AVGDialogUIEnhanced();
            }
            
            // 设置对话内容
            currentDialogUI.SetDialog(speaker, text);
            currentDialogUI.ShowDialog();
            
            isDialogActive = true;
            
            // 等待玩家点击继续
            dialogCompletionSource = new TaskCompletionSource<int>();
            await dialogCompletionSource.Task;
            
            // 隐藏对话
            currentDialogUI.HideDialog();
            isDialogActive = false;
            
            Game.Logger.LogInformation("✅ AVG增强对话完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ AVG增强对话显示失败");
        }
    }

    /// <summary>
    /// 显示增强版选择分支
    /// </summary>
    public static async Task<int> ShowChoice(string title, string[] choices)
    {
        try
        {
            Game.Logger.LogInformation("🎯 AVG增强选择：{Title} - 选项数量：{Count}", title, choices.Length);
            
            // 创建对话UI
            if (currentDialogUI == null)
            {
                currentDialogUI = new AVGDialogUIEnhanced();
            }
            
            // 设置选择内容
            currentDialogUI.SetChoice(title, choices);
            currentDialogUI.ShowChoice();
            
            isDialogActive = true;
            
            // 等待玩家选择
            choiceCompletionSource = new TaskCompletionSource<int>();
            var selectedIndex = await choiceCompletionSource.Task;
            
            // 隐藏选择
            currentDialogUI.HideChoice();
            isDialogActive = false;
            
            Game.Logger.LogInformation("✅ AVG增强选择完成：选择了第{Index}个选项", selectedIndex);
            return selectedIndex;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ AVG增强选择显示失败");
            return -1;
        }
    }

    /// <summary>
    /// 对话点击继续回调
    /// </summary>
    internal static void OnDialogContinue()
    {
        dialogCompletionSource?.SetResult(0);
    }

    /// <summary>
    /// 选择点击回调
    /// </summary>
    internal static void OnChoiceSelected(int index)
    {
        choiceCompletionSource?.SetResult(index);
    }

    /// <summary>
    /// 检查是否有对话正在进行
    /// </summary>
    public static bool IsDialogActive => isDialogActive;

    /// <summary>
    /// 清理AVG增强系统资源
    /// </summary>
    public static void Cleanup()
    {
        try
        {
            if (currentDialogUI != null)
            {
                currentDialogUI.Cleanup();
                currentDialogUI = null;
            }
            
            isDialogActive = false;
            dialogCompletionSource = null;
            choiceCompletionSource = null;
            
            Game.Logger.LogInformation("🧹 AVG增强系统资源清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ AVG增强系统清理失败");
        }
    }
}
#endif
