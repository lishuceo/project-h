#if CLIENT
using GameUI.Brush;
using GameUI.Control.Primitive;
using GameUI.Control.Extensions;
using GameUI.Control.Enum;
using GameUI.Enum;
using System.Drawing;
using static GameUI.Control.Extensions.UI;

namespace GameEntry.VampireSurvivors3D;

/// <summary>
/// 增强的Vampire3D UI系统 - 改进用户体验和信息展示
/// </summary>
public static class EnhancedUI
{
    private static Panel? mainGamePanel;
    private static Panel? skillPanel;
    private static Panel? miniMapPanel;
    private static Progress? experienceBar;
    private static Label? levelLabel;
    private static Panel? upgradeSelectionPanel;
    
    // 🆕 添加升级选项相关字段
    private static Button[]? upgradeButtons;
    private static Label[]? upgradeNameLabels;
    private static Label[]? upgradeDescLabels;
    private static string[]? currentUpgradeOptions;

    /// <summary>
    /// 创建增强的游戏UI界面
    /// </summary>
    public static void CreateEnhancedGameUI()
    {
        // 主游戏面板 - 使用新的Flexbox扩展
        mainGamePanel = UI.Panel()
            .Stretch()
            .GrowRatio(1, 1);

        // 顶部信息栏
        CreateTopInfoBar();
        
        // 技能面板（显示当前激活的被动技能）
        CreateSkillPanel();
        
        // 迷你地图
        CreateMiniMap();
        
        // 经验值条
        CreateExperienceBar();
        
        // 升级选择面板（隐藏，升级时显示）
        CreateUpgradeSelectionPanel();

        // 添加到视觉树
        _ = mainGamePanel.AddToVisualTree();
    }

    private static void CreateTopInfoBar()
    {
        var topPanel = new Panel()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Height = 60,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
            Parent = mainGamePanel
        };

        // 血量显示
        currentHealthLabel = new Label()
        {
            Text = "❤️ 1000/1000",
            FontSize = 18,
            TextColor = new SolidColorBrush(Color.Red),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new(20, 0, 0, 0),
            Parent = topPanel
        };

        // 击杀数显示
        currentKillLabel = new Label()
        {
            Text = "💀 Kills: 0",
            FontSize = 16,
            TextColor = new SolidColorBrush(Color.Orange),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new(0, 0, 200, 0),
            Parent = topPanel
        };

        // 游戏时间显示
        currentTimeLabel = new Label()
        {
            Text = "⏰ 0:00",
            FontSize = 16,
            TextColor = new SolidColorBrush(Color.White),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new(0, 0, 20, 0),
            Parent = topPanel
        };
    }

    private static void CreateMiniMap()
    {
        miniMapPanel = new Panel()
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Width = 150,
            Height = 150,
            Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)),
            Margin = new(0, 70, 10, 0),
            Parent = mainGamePanel
        };

        var mapLabel = new Label()
        {
            Text = "🗺️ Mini Map",
            FontSize = 12,
            TextColor = new SolidColorBrush(Color.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new(0, 5, 0, 0),
            Parent = miniMapPanel
        };
    }

    private static void CreateSkillPanel()
    {
        skillPanel = new Panel()
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 80,
            Height = 300,
            Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 50)),
            Margin = new(0, 0, 10, 0),
            Parent = mainGamePanel
        };

        // 技能图标位置（动态添加）
        CreateSkillIcon("🔥", "火球术", 0);
        CreateSkillIcon("⚡", "闪电链", 1);
        CreateSkillIcon("💚", "治疗光环", 2);
    }

    private static void CreateSkillIcon(string icon, string name, int index)
    {
        var skillButton = new Button()
        {
            Width = 60,
            Height = 60,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new(0, 10 + index * 70, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(200, 100, 50, 0)),
            Parent = skillPanel
        };

        // 添加技能图标文本
        var iconLabel = new Label()
        {
            Text = icon,
            FontSize = 24,
            TextColor = new SolidColorBrush(Color.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Parent = skillButton
        };

        // 添加技能冷却指示器
        var cooldownOverlay = new Panel()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0)),
            Visible = false,  // 冷却时显示
            Parent = skillButton
        };
    }

    private static void CreateExperienceBar()
    {
        var expBarContainer = new Panel()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 40,
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
            Margin = new(0, 0, 0, 0),
            Parent = mainGamePanel
        };

        // 等级标签
        levelLabel = new Label()
        {
            Text = "Lv.1",
            FontSize = 16,
            TextColor = new SolidColorBrush(Color.Gold),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new(20, 0, 0, 0),
            Parent = expBarContainer
        };

        // 经验值条背景
        var expBarBg = new Panel()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Height = 20,
            Margin = new(60, 0, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(200, 50, 50, 50)),
            Parent = expBarContainer
        };

        // 经验值条填充
        experienceBar = new Progress()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Value = 0,
            Parent = expBarBg
        };
    }

    private static void CreateUpgradeSelectionPanel()
    {
        upgradeSelectionPanel = new Panel()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Width = 600,
            Height = 400,
            Background = new SolidColorBrush(Color.FromArgb(240, 20, 20, 40)),
            Visible = false,  // 默认隐藏
            Parent = mainGamePanel
        };

        var titleLabel = new Label()
        {
            Text = "🎊 Level Up! Choose an Upgrade",
            FontSize = 24,
            TextColor = new SolidColorBrush(Color.Gold),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new(0, 20, 0, 0),
            Parent = upgradeSelectionPanel
        };

        // 🆕 初始化升级选项数组
        upgradeButtons = new Button[3];
        upgradeNameLabels = new Label[3];
        upgradeDescLabels = new Label[3];

        // 创建三个升级选项按钮
        for (int i = 0; i < 3; i++)
        {
            CreateUpgradeOption(i);
        }
    }

    private static void CreateUpgradeOption(int index)
    {
        upgradeButtons![index] = new Button()
        {
            Width = 160,
            Height = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new(60 + index * 180, 0, 0, 0),
            Background = new SolidColorBrush(Color.FromArgb(200, 80, 40, 0)),
            Parent = upgradeSelectionPanel
        };

        var iconLabel = new Label()
        {
            Text = "🔮", // 根据升级类型动态设置
            FontSize = 32,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new(0, 20, 0, 0),
            Parent = upgradeButtons[index]
        };

        upgradeNameLabels![index] = new Label()
        {
            Text = "Upgrade Name",
            FontSize = 16,
            TextColor = new SolidColorBrush(Color.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new(0, -20, 0, 0),
            Parent = upgradeButtons[index]
        };

        upgradeDescLabels![index] = new Label()
        {
            Text = "Upgrade description goes here...",
            FontSize = 12,
            TextColor = new SolidColorBrush(Color.LightGray),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new(0, 0, 0, 20),
            Parent = upgradeButtons[index]
        };

        // 🆕 关键修复：添加点击事件处理器
        int buttonIndex = index; // 创建局部变量避免闭包问题
        upgradeButtons[index].OnPointerClicked += (s, e) => SelectUpgrade(buttonIndex);
    }

    /// <summary>
    /// 🆕 处理升级选择的核心方法
    /// </summary>
    private static void SelectUpgrade(int upgradeIndex)
    {
        try
        {
            if (currentUpgradeOptions == null || upgradeIndex >= currentUpgradeOptions.Length)
            {
                Game.Logger.LogWarning("❌ Invalid upgrade selection: {index}", upgradeIndex);
                return;
            }

            var selectedUpgrade = currentUpgradeOptions[upgradeIndex];
            Game.Logger.LogInformation("✅ Player selected upgrade: {upgrade}", selectedUpgrade);

            // 🆕 发送升级选择到服务器
            _ = NetworkClientSync.SendUpgradeSelection(upgradeIndex, selectedUpgrade);

            // 隐藏升级面板
            HideUpgradeSelection();
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error selecting upgrade");
        }
    }

    /// <summary>
    /// 显示升级选择面板
    /// </summary>
    public static void ShowUpgradeSelection()
    {
        if (upgradeSelectionPanel != null)
        {
            // 🆕 生成升级选项
            currentUpgradeOptions = GenerateUpgradeOptions();
            
            // 更新UI显示
            for (int i = 0; i < 3 && i < currentUpgradeOptions.Length; i++)
            {
                if (upgradeNameLabels != null && upgradeDescLabels != null)
                {
                    var (name, desc, icon) = ParseUpgradeOption(currentUpgradeOptions[i]);
                    upgradeNameLabels[i].Text = name;
                    upgradeDescLabels[i].Text = desc;
                }
            }

            upgradeSelectionPanel.Visible = true;
            Game.Logger.LogInformation("🎊 Upgrade selection panel shown");
        }
    }

    /// <summary>
    /// 隐藏升级选择面板
    /// </summary>
    public static void HideUpgradeSelection()
    {
        if (upgradeSelectionPanel != null)
        {
            upgradeSelectionPanel.Visible = false;
            Game.Logger.LogInformation("📴 Upgrade selection panel hidden");
        }
    }

    /// <summary>
    /// 🆕 生成升级选项
    /// </summary>
    private static string[] GenerateUpgradeOptions()
    {
        var options = new[]
        {
            "增加生命值|+50 HP|❤️",
            "增加移动速度|+15% Speed|🏃",
            "火球术强化|+25% Damage|🔥", 
            "闪电链强化|+1 Chain Target|⚡",
            "治疗光环强化|+20% Healing|💚",
            "攻击速度|+20% Attack Speed|⚔️"
        };

        // 随机选择3个不同的选项
        var selected = new string[3];
        var random = Random.Shared;
        var usedIndices = new HashSet<int>();

        for (int i = 0; i < 3; i++)
        {
            int index;
            do
            {
                index = random.Next(options.Length);
            } while (usedIndices.Contains(index));

            usedIndices.Add(index);
            selected[i] = options[index];
        }

        return selected;
    }

    /// <summary>
    /// 🆕 解析升级选项格式：名称|描述|图标
    /// </summary>
    private static (string name, string desc, string icon) ParseUpgradeOption(string option)
    {
        var parts = option.Split('|');
        if (parts.Length >= 3)
        {
            return (parts[0], parts[1], parts[2]);
        }
        return (option, "Unknown upgrade", "🔮");
    }

    /// <summary>
    /// 更新UI数据
    /// </summary>
    public static void UpdateGameStats(int health, int maxHealth, int level, float experience, float maxExp, int kills, TimeSpan gameTime)
    {
        try
        {
            // 更新血量显示
            UpdateHealthDisplay(health, maxHealth);
            
            // 更新击杀数显示
            UpdateKillDisplay(kills);
            
            // 更新时间显示
            UpdateTimeDisplay(gameTime);
            
            // 更新等级和经验值
            UpdateLevelAndExp(level, experience, maxExp);
            
            Game.Logger.LogInformation("🔄 UI Updated - HP:{health}/{maxHealth}, Level:{level}, Kills:{kills}", 
                health, maxHealth, level, kills);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error updating Enhanced UI");
        }
    }

    // 存储UI元素的引用
    private static Label? currentHealthLabel;
    private static Label? currentKillLabel;
    private static Label? currentTimeLabel;

    private static void UpdateHealthDisplay(int health, int maxHealth)
    {
        if (currentHealthLabel != null)
        {
            currentHealthLabel.Text = $"❤️ {health}/{maxHealth}";
        }
    }

    private static void UpdateKillDisplay(int kills)
    {
        if (currentKillLabel != null)
        {
            currentKillLabel.Text = $"💀 Kills: {kills}";
        }
    }

    private static void UpdateTimeDisplay(TimeSpan gameTime)
    {
        if (currentTimeLabel != null)
        {
            currentTimeLabel.Text = $"⏰ {(int)gameTime.TotalMinutes}:{gameTime.Seconds:D2}";
        }
    }

    private static void UpdateLevelAndExp(int level, float experience, float maxExp)
    {
        if (levelLabel != null)
        {
            levelLabel.Text = $"Lv.{level}";
        }

        if (experienceBar != null && maxExp > 0)
        {
            experienceBar.Value = experience / maxExp;
        }
    }
}
#endif 