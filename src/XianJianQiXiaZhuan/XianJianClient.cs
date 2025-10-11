#if CLIENT
using Events;
using GameCore;
using GameCore.Event;
using GameUI.Control;
using GameUI.Control.Primitive;
using GameUI.Control.Enum;
using GameUI.Control.Advanced;
using GameUI.Control.Struct;
using GameUI.Control.Extensions;
using GameUI.Brush;
using GameUI.Struct;
using System.Drawing;
using GameUI.Enum;
using GameSystemUI.AbilitySystemUI.Advanced;
using GameSystemUI.GameInventoryUI.Advanced;
using GameCore.ItemSystem;
using GameCore.Container;
using GameCore.EntitySystem;
using GameCore.PlayerAndUsers;
using GameSystemUI.MoveKeyBoard.Advanced;
using GameData;
// 流式布局扩展语法
using static GameUI.Control.Extensions.UI;
using GameEntry.AVGSystem.Data;
using GameEntry.AVGSystem.Engine;
using GameEntry.AVGSystem;

namespace GameEntry.XianJianQiXiaZhuan;

/// <summary>
/// 仙剑奇侠传游戏客户端逻辑 - 基于ARPGTemplate重构版本
/// </summary>
internal class XianJianClient : IGameClass
{
    // UI控件
    private static Label? statusLabel;
    private static Panel? gameUI;
    private static bool gameStarted = false;
    
    // 游戏系统UI组件
    private static AbilityJoyStickGroup? abilityJoyStickGroup;
    private static DefaultInventoryUI? inventoryUI;
    private static InventoryUIEntrance? inventoryEntrance;
    private static QuickBarUI? quickBarUI;
    private static PickButton? pickButton;
    private static MoveKeyBoard? moveKeyBoard;
    
    // 防止重复初始化的标志
    private static bool isInitialized = false;

    public static void OnRegisterGameClass()
    {
        Game.Logger.LogInformation("🗡️ 注册仙剑奇侠传客户端模块...");
        
        // 检查是否在仙剑奇侠传模式下
        // 先移除可能已存在的订阅，避免重复注册
        Game.OnGameDataInitialization -= CheckAndInitialize;
        Game.OnGameDataInitialization += CheckAndInitialize;
    }

    private static void CheckAndInitialize()
    {
        // 检查当前游戏模式是否为仙剑奇侠传
        if (Game.GameModeLink == GameEntry.ScopeData.GameMode.XianJianQiXiaZhuan)
        {
            Game.Logger.LogInformation("🗡️ 仙剑奇侠传模式检测到，初始化客户端...");
            
            // 防止重复注册：先取消注册再注册
            Game.OnGameUIInitialization -= InitializeXianJianClient;
            Game.OnGameUIInitialization += InitializeXianJianClient;
        }
    }

    private static void InitializeXianJianClient()
    {
        // 防止重复初始化
        if (isInitialized)
        {
            Game.Logger.LogInformation("🗡️ 仙剑奇侠传客户端已经初始化过，跳过重复初始化");
            return;
        }

        Game.Logger.LogInformation("🗡️ 初始化仙剑奇侠传客户端UI...");
        
        try
        {
            InitializeUI();
            isInitialized = true;
            
            // 启动仙剑奇侠传剧情 - 避免使用Task.Run（WebAssembly不支持）
            _ = StartXianJianStoryDelayed();
            
            Game.Logger.LogInformation("✅ 仙剑奇侠传客户端初始化完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 仙剑奇侠传客户端初始化失败");
        }
    }

    private static void InitializeUI()
    {
        Game.Logger.LogInformation("🎮 创建简洁实用的仙剑奇侠传游戏UI...");

        // 创建全屏主容器
        gameUI = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            WidthStretchRatio = 1.0f,
            HeightStretchRatio = 1.0f,
        };

        // 只创建简洁的游戏标题
        CreateSimpleTitle();

        // 只创建必要的状态显示
        CreateMinimalStatusBar();

        // 创建左侧任务面板
        CreateLeftQuestPanel();

        // 将游戏UI添加到根视图
        gameUI.AddToRoot();

        // 开场故事现在由AVG系统处理，不再创建旧的故事UI
        // CreateStoryUI(); // 已替换为AVG系统

        // 延迟初始化游戏系统UI（这是核心功能）
        DelayedInitializeGameSystemUI();

        Game.Logger.LogInformation("✅ 简洁游戏UI创建完成 - 专注游戏体验");
    }

    /// <summary>
    /// 创建简洁的游戏标题
    /// </summary>
    private static void CreateSimpleTitle()
    {
        statusLabel = new Label
        {
            Text = "⚔️ 仙剑奇侠传",
            FontSize = 40, // 符合规范：标题40px
            TextColor = new SolidColorBrush(Color.Gold),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 10, 0, 0), // 很小的顶部间距
        };

        statusLabel.Parent = gameUI;
    }

    /// <summary>
    /// 创建最小化的状态栏 - 只显示关键信息
    /// </summary>
    private static void CreateMinimalStatusBar()
    {
        var statusBar = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 45, 0, 0), // 在标题下方
            Width = 800, // 增加宽度适应28px字体
            Height = 40, // 增加高度适应28px字体
            ZIndex = LAYER_STATUS, // 状态栏层级
            Background = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)), // 半透明黑色
        };

        var statusInfo = new Label
        {
            Text = "🗺️ 余杭镇 8192×8192  |  📍 世界中心  |  ⚡ Lv.1",
            FontSize = 28, // 符合规范：细节辅助信息28px
            TextColor = new SolidColorBrush(Color.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        statusInfo.Parent = statusBar;
        statusBar.Parent = gameUI;
    }

    // 当前任务状态
    private static int currentQuestIndex = 0;
    private static Label? currentQuestLabel;
    
    // 开场故事UI - 已替换为AVG系统
    // private static Panel? storyPanel;
    // private static Label? storyContent;
    // private static int storyStep = 0;

    // UI层级管理 - 按照标准UI渲染顺序设计
    private const int LAYER_STORY = 20;         // 开场故事UI（顶级对话框层级）
    private const int LAYER_INVENTORY = 5;      // 背包系统（弹出菜单层级）
    private const int LAYER_QUEST = 3;          // 任务面板（面板层级）
    private const int LAYER_PICKUP = 2;         // 拾取按钮（悬浮元素层级）
    private const int LAYER_GAME_CONTROL = 1;   // 游戏控制（卡片、按钮层级）
    private const int LAYER_STATUS = 0;         // 状态栏（基础内容层）

    /// <summary>
    /// 创建左侧任务面板 - 一次只显示一个任务，简洁设计
    /// </summary>
    private static void CreateLeftQuestPanel()
    {
        // ✅ 小尺寸面板，一次只显示一个任务 - 使用标准对齐方式
        var questPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20, 0, 0, 0),
            Width = 240, // 小面板，适合单个任务
            Height = 160, // 紧凑高度
            ZIndex = LAYER_QUEST, // 任务面板层级
            Background = new SolidColorBrush(Color.FromArgb(180, 20, 30, 50)),
        };

        // 简洁标题
        var questTitle = new Label
        {
            Text = "📜 当前任务",
            FontSize = 18, // 小面板使用小字体
            TextColor = new SolidColorBrush(Color.Gold),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 10, 0, 0),
        };

        // 当前任务显示
        currentQuestLabel = new Label
        {
            Text = GetCurrentQuestText(),
            FontSize = 16, // 紧凑字体
            TextColor = new SolidColorBrush(Color.LightCyan),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(15, 45, 15, 0),
        };

        // 进度提示
        var progressHint = new Label
        {
            Text = "💡 完成后自动获得下一个任务",
            FontSize = 14, // 小提示文字
            TextColor = new SolidColorBrush(Color.LightGray),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 120, 0, 0),
        };

        // 设置父子关系
        questTitle.Parent = questPanel;
        currentQuestLabel.Parent = questPanel;
        progressHint.Parent = questPanel;
        questPanel.Parent = gameUI;

        Game.Logger.LogInformation("✅ 简洁任务面板已创建 - 240x160，一次显示一个任务");
    }

    /// <summary>
    /// 获取当前任务文本
    /// </summary>
    private static string GetCurrentQuestText()
    {
        return currentQuestIndex switch
        {
            0 => "🎯 清理威胁\n击败1只蛇妖 (0/1)\n💰 奖励：经验 +50",
            1 => "💬 寻求指导\n与姜子牙对话 (0/1)\n💰 奖励：经验 +30",
            2 => "🗺️ 探索世界\n到达东北仙人台 (0/1)\n💰 奖励：经验 +40",
            _ => "✅ 所有任务完成！\n🎉 恭喜成为剑侠！"
        };
    }

    /// <summary>
    /// 完成当前任务，切换到下一个
    /// </summary>
    public static void CompleteCurrentQuest()
    {
        currentQuestIndex++;
        if (currentQuestLabel != null)
        {
            currentQuestLabel.Text = GetCurrentQuestText();
            currentQuestLabel.TextColor = currentQuestIndex >= 3 
                ? new SolidColorBrush(Color.Gold) 
                : new SolidColorBrush(Color.LightCyan);
        }
        
        Game.Logger.LogInformation("🎉 任务完成！当前任务索引：{Index}", currentQuestIndex);
    }

    // === 旧的故事UI系统已替换为AVG系统 ===
    // 以下方法已被AVG剧本系统替代：
    // - CreateStoryUI() 
    // - GetStoryText()
    // - StartStoryProgression()
    // - CloseStoryUI()
    // 
    // 现在使用数据驱动的AVG剧本：XianJian_Opening 和 XianJian_CharacterMeeting


    /// <summary>
    /// 延迟初始化游戏系统UI，等待玩家和主单位就绪
    /// </summary>
    private static async void DelayedInitializeGameSystemUI()
    {
        try
        {
            Game.Logger.LogInformation("🗡️ 开始等待玩家和主单位就绪...");
            
            // 最多等待10秒，每500ms检查一次
            for (int i = 0; i < 20; i++)
            {
                var localPlayer = Player.LocalPlayer;
                if (localPlayer?.MainUnit != null)
                {
                    Game.Logger.LogInformation("✅ 玩家和主单位已就绪，开始初始化仙剑游戏系统UI");
                    InitializeGameSystemUI();
                    return;
                }
                
                Game.Logger.LogDebug("⏳ 等待玩家和主单位就绪... ({attempt}/20)", i + 1);
                await Game.Delay(TimeSpan.FromMilliseconds(500));
            }
            
            Game.Logger.LogWarning("⚠️ 等待玩家和主单位就绪超时，跳过游戏系统UI初始化");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 延迟初始化仙剑游戏系统UI时发生错误");
        }
    }

    /// <summary>
    /// 初始化游戏系统UI（技能摇杆和背包系统）- 参考ARPGTemplate架构
    /// </summary>
    private static async void InitializeGameSystemUI()
    {
        try
        {
            var localPlayer = Player.LocalPlayer;
            if (localPlayer?.MainUnit == null)
            {
                Game.Logger.LogWarning("⚠️ 无法初始化仙剑奇侠传游戏系统UI：本地玩家或主单位为空");
                return;
            }

            var mainUnit = localPlayer.MainUnit;
            
            // 等待一段时间确保单位完全加载
            await Game.Delay(TimeSpan.FromSeconds(1));

            // 技能摇杆 - 确保清晰可见，不被遮挡
            try
            {
                if (abilityJoyStickGroup == null)
                {
                    abilityJoyStickGroup = new AbilityJoyStickGroup()
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(0, 0, 50, 50), // 简洁间距
                        ZIndex = LAYER_GAME_CONTROL, // 游戏控制层级
                        BindUnit = mainUnit
                    };
                    abilityJoyStickGroup.AddToRoot();
                    Game.Logger.LogInformation("✅ 技能摇杆已创建 - 右下角清晰可见");
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogError("❌ 初始化仙剑技能摇杆时发生错误: {error}", ex.Message);
            }

            // 初始化背包系统相关UI
            try
            {
                await Game.Delay(TimeSpan.FromMilliseconds(200));
                var inventoryManager = mainUnit.GetComponent<InventoryManager>();
                
                if (inventoryManager?.Inventories != null && inventoryManager.Inventories.Count > 0)
                {
                    // 创建背包UI
                    if (inventoryUI == null)
                    {
                        inventoryUI = new DefaultInventoryUI()
                        {
                            ZIndex = LAYER_INVENTORY, // 使用统一的背包层级
                            BindUnit = mainUnit,
                            Visible = false // 默认隐藏
                        };
                        inventoryUI.AddToRoot();
                        Game.Logger.LogInformation("✅ 仙剑奇侠传背包UI已创建");
                    }

                    // 背包入口 - 简洁位置，不干扰游戏
                    if (inventoryEntrance == null)
                    {
                        inventoryEntrance = new InventoryUIEntrance(inventoryUI)
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(20, 90, 0, 0), // 在状态栏下方
                            ZIndex = LAYER_INVENTORY // 背包系统层级
                        };
                        inventoryEntrance.AddToRoot();
                        Game.Logger.LogInformation("✅ 背包入口已创建 - 左上角简洁位置");
                    }

                    // 快捷栏 - 底部居中，清晰可见
                    if (quickBarUI == null)
                    {
                        quickBarUI = new QuickBarUI()
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Margin = new Thickness(0, 0, 0, 20), // 底部留小间距
                            ZIndex = LAYER_INVENTORY, // 背包系统层级
                            BindUnit = mainUnit
                        };
                        quickBarUI.AddToRoot();
                        Game.Logger.LogInformation("✅ 快捷栏已创建 - 底部居中清晰显示");
                    }
                }
                else
                {
                    Game.Logger.LogWarning("⚠️ 主单位没有背包管理器或背包为空，跳过背包系统UI创建");
                }

                // 拾取按钮 - 中心位置，简洁设计
                if (pickButton == null)
                {
                    pickButton = new PickButton(mainUnit)
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        ZIndex = LAYER_PICKUP // 拾取按钮层级
                    };
                    pickButton.AddToRoot();
                    Game.Logger.LogInformation("✅ 拾取按钮已创建 - 中心位置");
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogError("❌ 初始化仙剑背包系统时发生错误: {error}", ex.Message);
            }

            // 移动控制 - 左下角，与技能摇杆对称
            try
            {
                if (moveKeyBoard == null)
                {
                    moveKeyBoard = new MoveKeyBoard()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(50, 0, 0, 50), // 与技能摇杆对称
                        ZIndex = LAYER_GAME_CONTROL, // 游戏控制层级
                        BindUnit = mainUnit
                    };
                    moveKeyBoard.AddToRoot();
                    Game.Logger.LogInformation("✅ 移动控制已创建 - 左下角清晰可见");
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogError("❌ 初始化仙剑移动控制时发生错误: {error}", ex.Message);
            }

            Game.Logger.LogInformation("🎉 仙剑奇侠传游戏系统UI初始化完成！专注游戏核心功能！");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 初始化仙剑奇侠传游戏系统UI时发生错误");
        }
    }



    /// <summary>
    /// 更新状态显示
    /// </summary>
    public static void UpdateStatus(string message)
    {
        if (statusLabel != null)
        {
            statusLabel.Text = $"🗡️ 仙剑奇侠传 - {message}";
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public static void Cleanup()
    {
        Game.Logger.LogInformation("🧹 清理仙剑奇侠传客户端资源...");
        
        try
        {
            // 移除UI组件
            if (gameUI != null)
            {
                gameUI.RemoveFromParent();
                gameUI = null;
            }
            
            if (abilityJoyStickGroup != null)
            {
                abilityJoyStickGroup.RemoveFromParent();
                abilityJoyStickGroup = null;
            }

            if (inventoryUI != null)
            {
                inventoryUI.RemoveFromParent();
                inventoryUI = null;
            }

            if (inventoryEntrance != null)
            {
                inventoryEntrance.RemoveFromParent();
                inventoryEntrance = null;
            }

            if (quickBarUI != null)
            {
                quickBarUI.RemoveFromParent();
                quickBarUI = null;
            }

            if (pickButton != null)
            {
                pickButton.RemoveFromParent();
                pickButton = null;
            }

            if (moveKeyBoard != null)
            {
                moveKeyBoard.RemoveFromParent();
                moveKeyBoard = null;
            }

            if (statusLabel != null)
            {
                statusLabel.RemoveFromParent();
                statusLabel = null;
            }

            isInitialized = false;
            gameStarted = false;
            
            Game.Logger.LogInformation("✅ 仙剑奇侠传客户端资源清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 清理仙剑奇侠传客户端资源时发生错误");
        }
    }

    /// <summary>
    /// 延迟启动仙剑奇侠传剧情 - WebAssembly兼容版本
    /// </summary>
    private static async Task StartXianJianStoryDelayed()
    {
        try
        {
            // 等待UI完全加载
            await Game.Delay(TimeSpan.FromSeconds(3));
            
            Game.Logger.LogInformation("🎬 启动仙剑奇侠传剧情系统...");
            
            // 注册剧本数据
            XianJianScripts.RegisterAllScripts();
            
            // 播放开场剧情 - 使用普通对话模式
            await PlayXianJianOpeningDialog();
            
            // 播放角色相遇剧情
            await PlayCharacterMeetingDialog();
            
            Game.Logger.LogInformation("✅ 仙剑奇侠传剧情播放完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 仙剑奇侠传剧情播放失败");
        }
    }

    /// <summary>
    /// 播放仙剑开场对话 - 使用普通对话模式
    /// </summary>
    private static async Task PlayXianJianOpeningDialog()
    {
        try
        {
            // 使用增强对话UI，但不启用全屏背景模式
            await AVGEnhanced.ShowDialog("旁白", "余杭镇，一个江南小镇，青石板路，小桥流水...");
            await AVGEnhanced.ShowDialog("旁白", "在这里有一家小客栈，客栈里有一个名叫李逍遥的少年。");
            await AVGEnhanced.ShowDialog("李逍遥", "哎，又是平凡的一天。什么时候才能有点刺激的事情发生呢？");
            
            // 选择分支
            var choice = await AVGEnhanced.ShowChoice("这时，李逍遥决定...", new[]
            {
                "去镇上逛逛",
                "继续在客栈工作",
                "去后山练剑"
            });
            
            switch (choice)
            {
                case 0:
                    await AVGEnhanced.ShowDialog("李逍遥", "难得有空闲时间，去镇上走走看看有什么新鲜事。");
                    break;
                case 1:
                    await AVGEnhanced.ShowDialog("李逍遥", "还是老老实实工作吧，婶婶交代的事情不能马虎。");
                    break;
                case 2:
                    await AVGEnhanced.ShowDialog("李逍遥", "去后山练练剑法，说不定能遇到什么奇遇。");
                    break;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 仙剑开场对话播放失败");
        }
    }

    /// <summary>
    /// 播放角色相遇对话 - 使用普通对话模式
    /// </summary>
    private static async Task PlayCharacterMeetingDialog()
    {
        try
        {
            await AVGEnhanced.ShowDialog("李逍遥", "我是李逍遥，一个普通的客栈小二。");
            await AVGEnhanced.ShowDialog("赵灵儿", "逍遥哥哥，我是赵灵儿，很高兴认识你。");
            await AVGEnhanced.ShowDialog("林月如", "哼！我是林月如，你们好像很熟的样子。");
            
            var relationChoice = await AVGEnhanced.ShowChoice("李逍遥，你更喜欢谁？", new[]
            {
                "赵灵儿（温柔善良）",
                "林月如（活泼直率）", 
                "都喜欢（贪心！）"
            });
            
            switch (relationChoice)
            {
                case 0:
                    await AVGEnhanced.ShowDialog("赵灵儿", "逍遥哥哥...💕");
                    await AVGEnhanced.ShowDialog("林月如", "哼！没眼光！");
                    break;
                case 1:
                    await AVGEnhanced.ShowDialog("林月如", "哈哈！我就知道你有眼光！");
                    await AVGEnhanced.ShowDialog("赵灵儿", "逍遥哥哥...😢");
                    break;
                case 2:
                    await AVGEnhanced.ShowDialog("赵灵儿", "逍遥哥哥真贪心...");
                    await AVGEnhanced.ShowDialog("林月如", "花心大萝卜！");
                    await AVGEnhanced.ShowDialog("李逍遥", "我...我只是觉得你们都很好啊...");
                    break;
            }
            
            await AVGEnhanced.ShowDialog("旁白", "无论如何，三人的缘分就此开始...");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 角色相遇对话播放失败");
        }
    }
}
#endif