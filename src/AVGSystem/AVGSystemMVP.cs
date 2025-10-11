#if CLIENT
using GameCore;
using GameUI.Control;
using GameUI.Control.Primitive;
using GameUI.Control.Enum;
using GameUI.Control.Struct;
using GameUI.Control.Extensions;
using GameUI.Brush;
using GameUI.Struct;
using System.Drawing;
using GameUI.Enum;
using static GameUI.Control.Extensions.UI;

namespace GameEntry.AVGSystem;

/// <summary>
/// AVG系统MVP版本 - 提供基础对话功能
/// </summary>
public static class AVG
{
    // 当前对话UI实例
    private static SimpleDialogUI? currentDialogUI;
    private static bool isDialogActive = false;
    
    // 对话完成的回调
    private static TaskCompletionSource<int>? dialogCompletionSource;
    private static TaskCompletionSource<int>? choiceCompletionSource;

    /// <summary>
    /// 显示简单对话
    /// </summary>
    /// <param name="speaker">说话者名称</param>
    /// <param name="text">对话内容</param>
    public static async Task ShowDialog(string speaker, string text)
    {
        try
        {
            Game.Logger.LogInformation("💬 AVG对话：{Speaker} - {Text}", speaker, text);
            
            // 创建对话UI
            if (currentDialogUI == null)
            {
                currentDialogUI = new SimpleDialogUI();
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
            
            Game.Logger.LogInformation("✅ AVG对话完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ AVG对话显示失败");
        }
    }

    /// <summary>
    /// 显示选择分支
    /// </summary>
    /// <param name="title">选择标题</param>
    /// <param name="choices">选择选项</param>
    /// <returns>选择的索引（0开始）</returns>
    public static async Task<int> ShowChoice(string title, string[] choices)
    {
        try
        {
            Game.Logger.LogInformation("🎯 AVG选择：{Title} - 选项数量：{Count}", title, choices.Length);
            
            // 创建对话UI
            if (currentDialogUI == null)
            {
                currentDialogUI = new SimpleDialogUI();
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
            
            Game.Logger.LogInformation("✅ AVG选择完成：选择了第{Index}个选项", selectedIndex);
            return selectedIndex;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ AVG选择显示失败");
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
    /// 清理AVG系统资源
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
            
            Game.Logger.LogInformation("🧹 AVG系统资源清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ AVG系统清理失败");
        }
    }
}

/// <summary>
/// 简单对话UI组件 - 符合UI设计规范
/// </summary>
internal class SimpleDialogUI
{
    // UI组件
    private Panel? dialogPanel;
    private Label? speakerLabel;
    private Label? contentLabel;
    private Panel? choicePanel;
    private List<Button> choiceButtons = new();
    
    // UI层级 - 符合标准
    private const int LAYER_AVG_DIALOG = 10; // 全屏遮罩层级
    
    public SimpleDialogUI()
    {
        CreateDialogUI();
    }

    /// <summary>
    /// 创建对话UI - 符合20:9分辨率和UI设计规范
    /// </summary>
    private void CreateDialogUI()
    {
        // 主对话面板 - 底部1/3区域，适合20:9分辨率
        dialogPanel = new Panel
        {
            Width = 1600, // 20:9分辨率下的合适宽度
            Height = 300,  // 底部300像素高度
            ZIndex = LAYER_AVG_DIALOG,
            Background = new SolidColorBrush(Color.FromArgb(220, 20, 30, 50)), // 深色半透明
            Visible = false
        };
        
        dialogPanel.HorizontalAlignment = HorizontalAlignment.Center;
        dialogPanel.VerticalAlignment = VerticalAlignment.Bottom;
        dialogPanel.Margin = new Thickness(0, 0, 0, 50); // 底部留50像素间距

        // 说话者标签
        speakerLabel = new Label
        {
            FontSize = 40, // 符合规范：标题40px
            TextColor = new SolidColorBrush(Color.Gold),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(30, 20, 0, 0),
        };

        // 对话内容标签
        contentLabel = new Label
        {
            FontSize = 36, // 符合规范：正文36px
            TextColor = new SolidColorBrush(Color.White),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(30, 70, 30, 0),
            Width = 1540, // 留出左右边距
        };

        // 选择面板
        choicePanel = new Panel
        {
            Width = 1600,
            Height = 400, // 选择时需要更高
            ZIndex = LAYER_AVG_DIALOG,
            Background = new SolidColorBrush(Color.FromArgb(220, 20, 30, 50)),
            Visible = false
        };
        
        choicePanel.HorizontalAlignment = HorizontalAlignment.Center;
        choicePanel.VerticalAlignment = VerticalAlignment.Bottom;
        choicePanel.Margin = new Thickness(0, 0, 0, 50);

        // 设置父子关系
        speakerLabel.Parent = dialogPanel;
        contentLabel.Parent = dialogPanel;
        
        // 添加到根视图
        dialogPanel.AddToRoot();
        choicePanel.AddToRoot();
        
        // 添加点击事件 - 点击对话框继续
        dialogPanel.OnPointerClicked += (sender, e) => AVG.OnDialogContinue();
    }

    /// <summary>
    /// 设置对话内容
    /// </summary>
    public void SetDialog(string speaker, string content)
    {
        if (speakerLabel != null)
        {
            speakerLabel.Text = $"💬 {speaker}";
        }
        
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
        if (dialogPanel != null)
        {
            dialogPanel.Visible = true;
        }
    }

    /// <summary>
    /// 隐藏对话
    /// </summary>
    public void HideDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.Visible = false;
        }
    }

    /// <summary>
    /// 设置选择内容
    /// </summary>
    public void SetChoice(string title, string[] choices)
    {
        // 清理旧的选择按钮
        ClearChoiceButtons();
        
        if (choicePanel == null) return;

        // 选择标题
        var titleLabel = new Label
        {
            Text = $"🎯 {title}",
            FontSize = 40, // 标题40px
            TextColor = new SolidColorBrush(Color.Gold),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 30, 0, 0),
        };
        titleLabel.Parent = choicePanel;

        // 创建选择按钮 - 垂直排列
        for (int i = 0; i < choices.Length; i++)
        {
            var button = new Button
            {
                Width = 600,   // 合适的按钮宽度
                Height = 60,   // 符合最小触摸目标56px+
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 100 + i * 80, 0, 0), // 垂直间隔80px
                Background = new SolidColorBrush(Color.FromArgb(180, 50, 100, 150)),
                ZIndex = LAYER_AVG_DIALOG + 1, // 按钮层级稍高
            };

            // 为按钮添加文字标签
            var buttonLabel = new Label
            {
                Text = choices[i],
                FontSize = 36, // 按钮文字36px
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            buttonLabel.Parent = button;

            // 按钮点击事件
            var choiceIndex = i; // 捕获索引
            button.OnPointerClicked += (sender, e) => AVG.OnChoiceSelected(choiceIndex);
            
            button.Parent = choicePanel;
            choiceButtons.Add(button);
        }
    }

    /// <summary>
    /// 显示选择
    /// </summary>
    public void ShowChoice()
    {
        if (choicePanel != null)
        {
            choicePanel.Visible = true;
        }
    }

    /// <summary>
    /// 隐藏选择
    /// </summary>
    public void HideChoice()
    {
        if (choicePanel != null)
        {
            choicePanel.Visible = false;
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
        
        // 清理标题标签
        if (choicePanel?.Children != null)
        {
            var children = choicePanel.Children.ToList();
            foreach (var child in children)
            {
                if (child is Label)
                {
                    child.RemoveFromParent();
                }
            }
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
            
            if (dialogPanel != null)
            {
                dialogPanel.RemoveFromParent();
                dialogPanel = null;
            }
            
            if (choicePanel != null)
            {
                choicePanel.RemoveFromParent();
                choicePanel = null;
            }
            
            speakerLabel = null;
            contentLabel = null;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ SimpleDialogUI清理失败");
        }
    }
}
#endif
