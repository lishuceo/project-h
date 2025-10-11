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
/// 完整AVG模式UI - 包含全屏背景、角色立绘和对话系统
/// </summary>
internal class AVGFullScreenMode
{
    // UI组件
    private Panel? fullScreenContainer;     // 全屏主容器
    private Panel? backgroundPanel;         // 背景图片层
    private Panel? characterPanel;          // 角色立绘层
    private AVGDialogUIEnhanced? dialogUI;  // 对话系统
    
    // UI层级
    private const int LAYER_AVG_BACKGROUND = 5;   // 背景层
    private const int LAYER_AVG_CHARACTER = 6;    // 角色层
    private const int LAYER_AVG_DIALOG = 10;      // 对话层（最高）

    public AVGFullScreenMode()
    {
        CreateAVGFullScreenUI();
    }

    /// <summary>
    /// 创建完整AVG模式UI
    /// </summary>
    private void CreateAVGFullScreenUI()
    {
        // === 全屏主容器 ===
        fullScreenContainer = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            WidthStretchRatio = 1.0f,
            HeightStretchRatio = 1.0f,
            HeightCompactRatio = 1.0f,
            ZIndex = LAYER_AVG_BACKGROUND,
            Background = new SolidColorBrush(Color.Black), // 黑色底色，防止闪烁
            Visible = false
        };

        // === 独立背景层 - 宽度填满，高度Auto，宽高比16:9 ===
        backgroundPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch, // 宽度填满屏幕
            VerticalAlignment = VerticalAlignment.Top,         // 上边缘对齐屏幕顶部
            WidthStretchRatio = 1.0f,                         // 宽度100%填满
            Height = new Auto<float>(),                        // 高度Auto，让系统根据AspectRatio计算
            AspectRatio = 16.0f / 9.0f,                       // 设置16:9宽高比
            Margin = new Thickness(0, 0, 0, 0),               // 确保无边距，贴屏幕顶部
            VerticalContentAlignment = GameUI.Enum.VerticalContentAlignment.Top, // 图片在Panel内顶部对齐
            HorizontalContentAlignment = GameUI.Enum.HorizontalContentAlignment.Center, // 图片在Panel内水平居中
            Image = "image/AVGSystem/Resources/bg4.png",
            ZIndex = LAYER_AVG_BACKGROUND + 1,
        };

        // === 角色立绘层 ===
        characterPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            WidthStretchRatio = 1.0f,
            HeightStretchRatio = 1.0f,
            Background = new SolidColorBrush(Color.Transparent),
            ZIndex = LAYER_AVG_CHARACTER,
        };

        // === 对话系统 ===
        dialogUI = new AVGDialogUIEnhanced();

        // === 设置父子关系 ===
        backgroundPanel.Parent = fullScreenContainer;   // 背景图片层
        characterPanel.Parent = fullScreenContainer;    // 角色立绘层
        
        // 添加到根视图
        fullScreenContainer.AddToRoot();
        
        Game.Logger.LogInformation("🎨 完整AVG模式UI创建完成");
    }

    /// <summary>
    /// 设置背景图片
    /// </summary>
    public void SetBackground(string imagePath)
    {
        if (backgroundPanel != null)
        {
            backgroundPanel.Image = imagePath;
            Game.Logger.LogInformation("🖼️ AVG背景设置为: {ImagePath}", imagePath);
        }
    }

    /// <summary>
    /// 显示角色立绘（预留接口）
    /// </summary>
    public void ShowCharacter(string characterName, string imagePath, CharacterPosition position = CharacterPosition.Center)
    {
        // TODO: 实现角色立绘显示
        Game.Logger.LogInformation("👤 显示角色: {CharacterName} 在 {Position} 位置", characterName, position);
    }

    /// <summary>
    /// 隐藏角色立绘（预留接口）
    /// </summary>
    public void HideCharacter(string characterName)
    {
        // TODO: 实现角色立绘隐藏
        Game.Logger.LogInformation("👤 隐藏角色: {CharacterName}", characterName);
    }

    /// <summary>
    /// 显示对话
    /// </summary>
    public void ShowDialog(string speaker, string content)
    {
        dialogUI?.SetDialog(speaker, content);
        dialogUI?.ShowDialog();
    }

    /// <summary>
    /// 显示选择
    /// </summary>
    public void ShowChoice(string title, string[] choices)
    {
        dialogUI?.SetChoice(title, choices);
        dialogUI?.ShowChoice();
    }

    /// <summary>
    /// 隐藏对话
    /// </summary>
    public void HideDialog()
    {
        dialogUI?.HideDialog();
    }

    /// <summary>
    /// 隐藏选择
    /// </summary>
    public void HideChoice()
    {
        dialogUI?.HideChoice();
    }

    /// <summary>
    /// 显示AVG模式
    /// </summary>
    public void Show()
    {
        if (fullScreenContainer != null)
        {
            fullScreenContainer.Visible = true;
        }
    }

    /// <summary>
    /// 隐藏AVG模式
    /// </summary>
    public void Hide()
    {
        if (fullScreenContainer != null)
        {
            fullScreenContainer.Visible = false;
        }
        dialogUI?.HideDialog();
        dialogUI?.HideChoice();
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Cleanup()
    {
        try
        {
            dialogUI?.Cleanup();
            
            if (fullScreenContainer != null)
            {
                fullScreenContainer.RemoveFromParent();
                fullScreenContainer = null;
            }
            
            backgroundPanel = null;
            characterPanel = null;
            dialogUI = null;
            
            Game.Logger.LogInformation("🧹 完整AVG模式UI清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 完整AVG模式UI清理失败");
        }
    }
}

/// <summary>
/// 角色立绘位置枚举
/// </summary>
public enum CharacterPosition
{
    Left,
    Center, 
    Right
}

/// <summary>
/// 完整AVG模式系统 - 使用全屏背景和角色立绘
/// </summary>
public static class AVGFullScreen
{
    // 当前AVG模式UI实例
    private static AVGFullScreenMode? currentAVGMode;
    private static bool isAVGModeActive = false;
    
    // 对话完成的回调
    private static TaskCompletionSource<int>? dialogCompletionSource;
    private static TaskCompletionSource<int>? choiceCompletionSource;

    /// <summary>
    /// 显示完整AVG对话
    /// </summary>
    public static async Task ShowDialog(string speaker, string content)
    {
        try
        {
            Game.Logger.LogInformation("🎬 AVG全屏对话：{Speaker} - {Content}", speaker, content);
            
            // 创建AVG模式UI
            if (currentAVGMode == null)
            {
                currentAVGMode = new AVGFullScreenMode();
                currentAVGMode.Show();
            }
            
            // 设置对话内容
            currentAVGMode.ShowDialog(speaker, content);
            
            isAVGModeActive = true;
            
            // 等待玩家点击继续
            dialogCompletionSource = new TaskCompletionSource<int>();
            await dialogCompletionSource.Task;
            
            // 隐藏对话
            currentAVGMode.HideDialog();
            isAVGModeActive = false;
            
            Game.Logger.LogInformation("✅ AVG全屏对话完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ AVG全屏对话显示失败");
        }
    }

    /// <summary>
    /// 显示完整AVG选择分支
    /// </summary>
    public static async Task<int> ShowChoice(string title, string[] choices)
    {
        try
        {
            Game.Logger.LogInformation("🎯 AVG全屏选择：{Title} - 选项数量：{Count}", title, choices.Length);
            
            // 创建AVG模式UI
            if (currentAVGMode == null)
            {
                currentAVGMode = new AVGFullScreenMode();
                currentAVGMode.Show();
            }
            
            // 设置选择内容
            currentAVGMode.ShowChoice(title, choices);
            
            isAVGModeActive = true;
            
            // 等待玩家选择
            choiceCompletionSource = new TaskCompletionSource<int>();
            var selectedIndex = await choiceCompletionSource.Task;
            
            // 隐藏选择
            currentAVGMode.HideChoice();
            isAVGModeActive = false;
            
            Game.Logger.LogInformation("✅ AVG全屏选择完成：选择了第{Index}个选项", selectedIndex);
            return selectedIndex;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ AVG全屏选择显示失败");
            return -1;
        }
    }

    /// <summary>
    /// 设置背景图片
    /// </summary>
    public static void SetBackground(string imagePath)
    {
        currentAVGMode?.SetBackground(imagePath);
    }

    /// <summary>
    /// 显示角色立绘
    /// </summary>
    public static void ShowCharacter(string characterName, string imagePath, CharacterPosition position = CharacterPosition.Center)
    {
        currentAVGMode?.ShowCharacter(characterName, imagePath, position);
    }

    /// <summary>
    /// 隐藏角色立绘
    /// </summary>
    public static void HideCharacter(string characterName)
    {
        currentAVGMode?.HideCharacter(characterName);
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
    /// 检查是否有AVG模式正在进行
    /// </summary>
    public static bool IsAVGModeActive => isAVGModeActive;

    /// <summary>
    /// 清理AVG全屏模式资源
    /// </summary>
    public static void Cleanup()
    {
        try
        {
            if (currentAVGMode != null)
            {
                currentAVGMode.Cleanup();
                currentAVGMode = null;
            }
            
            isAVGModeActive = false;
            dialogCompletionSource = null;
            choiceCompletionSource = null;
            
            Game.Logger.LogInformation("🧹 AVG全屏模式资源清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ AVG全屏模式清理失败");
        }
    }
}
#endif
