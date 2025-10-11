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

namespace GameEntry.ARPGTemplate;

/// <summary>
/// ARPG剑客游戏客户端逻辑
/// </summary>
internal class ARPGClient : IGameClass
{
    // UI控件
    private static Label? statusLabel;
    private static Panel? gameUI;
    
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
        Game.Logger.LogInformation("⚔️ 注册ARPG剑客客户端模块...");
        
        // 检查是否在ARPG模式下
        // 先移除可能已存在的订阅，避免重复注册
        Game.OnGameDataInitialization -= CheckAndInitialize;
        Game.OnGameDataInitialization += CheckAndInitialize;
    }

    private static void CheckAndInitialize()
    {
        // 检查当前游戏模式是否为ARPG
        if (Game.GameModeLink == GameEntry.ScopeData.GameMode.ARPGMode)
        {
            Game.Logger.LogInformation("⚔️ ARPG模式检测到，初始化ARPG客户端...");
            
            // 防止重复注册：先取消注册再注册
            Game.OnGameUIInitialization -= InitializeARPGClient;
            Game.OnGameUIInitialization += InitializeARPGClient;
        }
    }

    private static void InitializeARPGClient()
    {
        // 防止重复初始化
        if (isInitialized)
        {
            Game.Logger.LogInformation("⚔️ ARPG客户端已经初始化过，跳过重复初始化");
            return;
        }

        Game.Logger.LogInformation("⚔️ 初始化ARPG客户端UI...");
        
        try
        {
            InitializeUI();
            isInitialized = true;
            
            Game.Logger.LogInformation("✅ ARPG客户端初始化完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ ARPG客户端初始化失败");
        }
    }

    private static void InitializeUI()
    {
        // 创建主面板
        gameUI = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            WidthStretchRatio = 1.0f,
            HeightStretchRatio = 1.0f,
        };

        // 状态标签
        statusLabel = new Label
        {
            Text = "ARPG剑客模式",
            FontSize = 18,
            TextColor = new SolidColorBrush(Color.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 20, 0, 0),
        };

        // 设置父子关系
        statusLabel.Parent = gameUI;

        // 将游戏UI添加到根视图
        gameUI.AddToRoot();
        
        // 延迟初始化游戏系统UI，等待玩家和主单位就绪
        DelayedInitializeGameSystemUI();
        
        Game.Logger.LogInformation("⚔️ ARPG UI界面创建完成");
    }

    /// <summary>
    /// 延迟初始化游戏系统UI，等待玩家和主单位就绪
    /// </summary>
    private static async void DelayedInitializeGameSystemUI()
    {
        try
        {
            Game.Logger.LogInformation("⚔️ 开始等待玩家和主单位就绪...");
            
            // 最多等待10秒，每500ms检查一次
            for (int i = 0; i < 20; i++)
            {
                var localPlayer = Player.LocalPlayer;
                if (localPlayer?.MainUnit != null)
                {
                    Game.Logger.LogInformation("✅ 玩家和主单位已就绪，开始初始化游戏系统UI");
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
            Game.Logger.LogError(ex, "❌ 延迟初始化游戏系统UI时发生错误");
        }
    }

    /// <summary>
    /// 初始化游戏系统UI（技能摇杆和背包系统）
    /// </summary>
    private static async void InitializeGameSystemUI()
    {
        try
        {
            var localPlayer = Player.LocalPlayer;
            if (localPlayer?.MainUnit == null)
            {
                Game.Logger.LogWarning("⚠️ 无法初始化ARPG游戏系统UI：本地玩家或主单位为空");
                return;
            }

            var mainUnit = localPlayer.MainUnit;
            
            // 等待一段时间确保单位完全加载
            await Game.Delay(TimeSpan.FromSeconds(1));

            // 初始化技能摇杆组
            try
            {
                if (abilityJoyStickGroup == null)
                {
                    abilityJoyStickGroup = new AbilityJoyStickGroup()
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(0, 0, 100, 120),
                        ZIndex = 900,
                        BindUnit = mainUnit
                    };
                    abilityJoyStickGroup.AddToRoot();
                    Game.Logger.LogInformation("✅ ARPG技能摇杆已创建");
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogError("❌ 初始化技能摇杆时发生错误: {error}", ex.Message);
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
                            ZIndex = 850,
                            BindUnit = mainUnit,
                            Visible = false // 默认隐藏
                        };
                        inventoryUI.AddToRoot();
                        Game.Logger.LogInformation("✅ ARPG背包UI已创建");
                    }

                    // 创建背包入口按钮
                    if (inventoryEntrance == null)
                    {
                        inventoryEntrance = new InventoryUIEntrance(inventoryUI)
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(20, 20, 0, 0),
                            ZIndex = 850
                        };
                        inventoryEntrance.AddToRoot();
                        Game.Logger.LogInformation("✅ ARPG背包入口按钮已创建");
                    }

                    // 创建快捷栏
                    if (quickBarUI == null)
                    {
                        quickBarUI = new QuickBarUI()
                        {
                            VerticalAlignment = VerticalAlignment.Bottom,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Position = new UIPosition(0, 0), // 贴底显示，参考模板默认值
                            ZIndex = 850,
                            BindUnit = mainUnit
                        };
                        quickBarUI.AddToRoot();
                        Game.Logger.LogInformation("✅ ARPG快捷栏已创建");
                    }

                    // 创建拾取按钮
                    if (pickButton == null)
                    {
                        pickButton = new PickButton(mainUnit)
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Position = new UIPosition(200, 0), // 向右偏移200
                            ZIndex = 850,
                        };
                        pickButton.AddToRoot();
                        Game.Logger.LogInformation("✅ ARPG拾取按钮已创建");
                    }
                    
                    Game.Logger.LogInformation("✅ ARPG背包系统和相关UI初始化完成");
                }
                else
                {
                    Game.Logger.LogWarning("⚠️ 延迟后仍无法获取到背包管理器或背包为空");
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogError("❌ 初始化ARPG背包系统时发生错误: {error}", ex.Message);
            }

            // 初始化移动键盘
            try
            {
                if (moveKeyBoard == null)
                {
                    moveKeyBoard = new MoveKeyBoard()
                    {
                        ZIndex = 850,
                        BindUnit = mainUnit
                    };
                    moveKeyBoard.AddToRoot();
                    Game.Logger.LogInformation("✅ ARPG移动键盘已创建");
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogError("❌ 初始化移动键盘时发生错误: {error}", ex.Message);
            }

            Game.Logger.LogInformation("🎉 ARPG游戏系统UI初始化完成！");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 初始化ARPG游戏系统UI时发生错误");
        }
    }

    /// <summary>
    /// 更新状态显示
    /// </summary>
    public static void UpdateStatus(string message)
    {
        if (statusLabel != null)
        {
            statusLabel.Text = message;
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public static void Cleanup()
    {
        Game.Logger.LogInformation("🧹 清理ARPG客户端资源...");
        
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

            statusLabel = null;
            isInitialized = false; // 重置初始化标志
            
            Game.Logger.LogInformation("✅ ARPG客户端资源清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 清理ARPG客户端资源时出错");
        }
    }
}
#endif
