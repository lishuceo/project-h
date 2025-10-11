#if CLIENT
using Events;
using GameCore;
using GameCore.Event;
using GameUI.Control.Primitive;
using GameUI.Control.Enum;
using GameUI.Control.Extensions;
using GameUI.Brush;
using GameUI.Struct;
using System.Drawing;
using GameUI.Enum;
using GameEntry.TowerDefenseGame.ShopSystem;
using GameEntry.TowerDefenseGame.TowerUISystem;
using GameEntry.TowerDefenseGame.ResourceSystem;
using GameSystemUI.AbilitySystemUI.Advanced;
using GameSystemUI.GameInventoryUI.Advanced;
using GameCore.ItemSystem;
using GameCore.Container;

namespace GameEntry.TowerDefenseGame;

/// <summary>
/// 塔防游戏客户端逻辑
/// </summary>
internal class TowerDefenseClient : IGameClass
{
    // UI控件
    private static Button? startButton;
    private static Button? shopButton;
    private static Label? statusLabel;
    private static Label? healthLabel;
    private static Label? goldLabel;
    private static Panel? gameUI;
    private static Panel? resourcePanel;
    private static bool gameStarted = false; // 游戏是否已开始
    
    // 游戏系统UI组件
    private static AbilityJoyStickGroup? abilityJoyStickGroup;
    private static DefaultInventoryUI? inventoryUI;
    private static InventoryUIEntrance? inventoryEntrance;
    private static QuickBarUI? quickBarUI;
    private static PickButton? pickButton;

    public static void OnRegisterGameClass()
    {
        Game.Logger.LogInformation("🎮 注册TowerDefense客户端模块...");
        
        // 检查是否在TowerDefense模式下
        Game.OnGameDataInitialization += CheckAndInitialize;
    }

    /// <summary>
    /// 检查游戏模式并初始化
    /// </summary>
    private static void CheckAndInitialize()
    {
        // 检查当前是否为TowerDefense模式
        if (Game.GameModeLink == ScopeData.GameMode.TowerDefense)
        {
            Game.Logger.LogInformation("✅ 检测到TowerDefense模式，初始化塔防客户端...");
            
            // 注册UI初始化事件
            Game.OnGameUIInitialization += InitializeTowerDefenseClient;
        }
    }

    /// <summary>
    /// 初始化塔防客户端模块
    /// </summary>
    private static void InitializeTowerDefenseClient()
    {
        try
        {
            Game.Logger.LogInformation("🚀 启动TowerDefense客户端...");

            // 直接创建游戏UI
            CreateGameUI();

            Game.Logger.LogInformation("✅ TowerDefense客户端启动成功！");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ TowerDefense客户端启动失败: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 创建塔防游戏UI
    /// </summary>
    private static void CreateGameUI()
    {
        try
        {
            Game.Logger.LogInformation("🎮 Creating Tower Defense UI");

            // 创建主UI面板
            gameUI = new Panel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                WidthStretchRatio = 1.0f,
                HeightStretchRatio = 1.0f,
            };

            // 创建状态标签
            statusLabel = new Label()
            {
                Text = "塔防游戏 - 点击开始按钮开始游戏",
                FontSize = 18,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 0, 0),
            };

            // 创建资源信息面板（血量和金币的底色背景）
            resourcePanel = new Panel()
            {
                Width = 300,
                Height = 80,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 10, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)), // 半透明黑色背景
                Visible = false, // 游戏开始前隐藏
            };

            // 创建血量标签
            healthLabel = new Label()
            {
                Text = "❤️ 血量: 20/20",
                FontSize = 18,
                TextColor = new SolidColorBrush(Color.LimeGreen),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 10, 0, 0),
            };

            // 创建金币标签
            goldLabel = new Label()
            {
                Text = "💰 金币: 10",
                FontSize = 18,
                TextColor = new SolidColorBrush(Color.Gold),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 45, 0, 0),
            };

            // 创建开始按钮（放在屏幕正中央）
            startButton = new Button()
            {
                Width = 200,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(200, 34, 139, 34)),
            };

            // 为按钮添加文字标签
            var startButtonLabel = new Label()
            {
                Text = "开始游戏",
                FontSize = 20,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            // 创建商店按钮（放在右上角）
            shopButton = new Button()
            {
                Width = 120,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 10, 0),
                Background = new SolidColorBrush(Color.FromArgb(200, 255, 165, 0)),
                Visible = false, // 游戏开始前隐藏
            };

            // 为商店按钮添加文字标签
            var shopButtonLabel = new Label()
            {
                Text = "商店",
                FontSize = 16,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            // 设置控件的父级
            statusLabel.Parent = gameUI;
            resourcePanel.Parent = gameUI;
            healthLabel.Parent = resourcePanel; // 血量标签放在资源面板里
            goldLabel.Parent = resourcePanel;   // 金币标签放在资源面板里
            startButton.Parent = gameUI;
            startButtonLabel.Parent = startButton; // 让标签成为按钮的子控件
            shopButton.Parent = gameUI;
            shopButtonLabel.Parent = shopButton; // 让标签成为按钮的子控件

            // 绑定按钮点击事件 - 使用扩展方法
            startButton = startButton.Click(() => OnStartButtonClicked(null, EventArgs.Empty));
            shopButton = shopButton.Click(() => OnShopButtonClicked(null, EventArgs.Empty));

            // 关键步骤：将UI面板添加到根节点，这样才能显示出来！
            gameUI.AddToRoot();

            // 初始化塔UI系统（可视化版本）
            TowerUISystem.TowerUIVisual.Initialize();
            
            // 初始化玩家资源
            var localPlayer = Player.LocalPlayer;
            if (localPlayer != null)
            {
                ResourceSystem.PlayerResourceManager.InitializePlayerResources(localPlayer.Id, 500); // 初始500金币
                Game.Logger.LogInformation("💰 Player {playerId} initialized with 500 gold", localPlayer.Id);
            }

            Game.Logger.LogInformation("✅ Tower Defense UI created successfully and added to root");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating Tower Defense UI");
        }
    }

    /// <summary>
    /// 处理开始按钮点击
    /// </summary>
    private static void OnStartButtonClicked(object? sender, EventArgs e)
    {
        Game.Logger.LogInformation("🎮 Start game button clicked by player");

        try
        {
            // 使用协议发送开始游戏命令到服务端
            bool success = TowerDefenseCommandSender.SendCommand(TowerDefenseCommandType.StartGame, 1);
            
            if (success)
            {
                // 隐藏开始按钮
                if (startButton != null)
                {
                    startButton.Visible = false;
                }
                
                // 显示游戏UI元素
                ShowGameUI();
                
                // 设置游戏开始标志
                gameStarted = true;
                
                // 暂时更新UI状态，真正的状态会从服务端返回
                UpdateStatusText("正在启动游戏...");
                Game.Logger.LogInformation("✅ Start game command sent to server, UI updated");
            }
            else
            {
                UpdateStatusText("发送开始游戏命令失败！");
                Game.Logger.LogError("❌ Failed to send start game command");
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling start button click");
            UpdateStatusText("处理按钮点击时发生错误！");
        }
    }

    /// <summary>
    /// 显示游戏UI元素
    /// </summary>
    private static void ShowGameUI()
    {
        try
        {
            // 显示资源面板
            if (resourcePanel != null)
            {
                resourcePanel.Visible = true;
            }
            
            // 显示商店按钮
            if (shopButton != null)
            {
                shopButton.Visible = true;
            }
            
            // 初始化游戏系统UI
            InitializeGameSystemUI();
            
            Game.Logger.LogInformation("✅ Game UI elements shown");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error showing game UI");
        }
    }

    /// <summary>
    /// 隐藏游戏UI元素
    /// </summary>
    private static void HideGameUI()
    {
        try
        {
            // 隐藏资源面板
            if (resourcePanel != null)
            {
                resourcePanel.Visible = false;
            }
            
            // 隐藏商店按钮
            if (shopButton != null)
            {
                shopButton.Visible = false;
            }
            
            // 显示开始按钮
            if (startButton != null)
            {
                startButton.Visible = true;
            }
            
            // 隐藏游戏系统UI
            HideGameSystemUI();
            
            // 重置游戏开始标志
            gameStarted = false;
            
            Game.Logger.LogInformation("✅ Game UI elements hidden, start button shown, gameStarted: {gameStarted}", gameStarted);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error hiding game UI");
        }
    }

    /// <summary>
    /// 处理商店按钮点击
    /// </summary>
    private static void OnShopButtonClicked(object? sender, EventArgs e)
    {
        Game.Logger.LogInformation("🛒 Shop button clicked by player");

        try
        {
            if (ShopClient.IsShopVisible)
            {
                // 关闭商店
                bool success = ShopCommandSender.SendCloseShopCommand(1);
                if (success)
                {
                    Game.Logger.LogInformation("✅ Close shop command sent to server");
                    UpdateShopButtonText("商店");
                }
            }
            else
            {
                // 打开商店
                bool success = ShopCommandSender.SendOpenShopCommand(1);
                if (success)
                {
                    Game.Logger.LogInformation("✅ Open shop command sent to server");
                    UpdateShopButtonText("关闭");
                }
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling shop button click");
        }
    }

    /// <summary>
    /// 更新状态文本
    /// </summary>
    public static void UpdateStatusText(string text)
    {
        try
        {
            if (statusLabel != null)
            {
                statusLabel.Text = text;
                Game.Logger.LogInformation("📊 Status updated: {text}", text);
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error updating status text");
        }
    }

    /// <summary>
    /// 重置UI状态
    /// </summary>
    public static void ResetUI()
    {
        try
        {
            UpdateStatusText("塔防游戏 - 点击开始按钮开始游戏");
            UpdateHealthDisplay(20); // 重置血量显示
            UpdateGoldDisplay(10);   // 重置金币显示
            
            // 隐藏游戏UI，显示开始按钮
            HideGameUI();
            
            Game.Logger.LogInformation("🔄 UI reset completed");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error resetting UI");
        }
    }

    /// <summary>
    /// 从服务端更新游戏状态UI
    /// </summary>
    public static void UpdateGameStateUI(TowerDefenseGameInfo gameInfo)
    {
        try
        {
            string statusText = gameInfo.GameState switch
            {
                TowerDefenseGameState.Idle => "塔防游戏 - 点击开始按钮开始游戏",
                TowerDefenseGameState.Playing => $"🎮 游戏进行中 - 波数: {gameInfo.WaveNumber}, 怪物: {gameInfo.MonstersSpawned}, 时间: {gameInfo.ElapsedTime:F1}s",
                TowerDefenseGameState.Paused => $"⏸️ 游戏已暂停 - 波数: {gameInfo.WaveNumber}, 怪物: {gameInfo.MonstersSpawned}",
                TowerDefenseGameState.GameOver => $"💀 游戏结束 - 波数: {gameInfo.WaveNumber}, 怪物: {gameInfo.MonstersSpawned}",
                _ => "未知游戏状态"
            };

            // 更新血量和金币显示
            UpdateHealthDisplay(gameInfo.PlayerHealth);
            UpdateGoldDisplay(gameInfo.PlayerGold);

            if (!gameInfo.IsSuccess)
            {
                statusText += " ❌ " + GetErrorMessage(gameInfo.ResultCode);
            }

            UpdateStatusText(statusText);

            // 根据游戏状态调整按钮可用性
            // TODO: 当找到正确的按钮启用/禁用方法时，可以取消注释
            // if (startButton != null)
            // {
            //     startButton.IsEnabled = (gameInfo.GameState == TowerDefenseGameState.Idle);
            // }

            Game.Logger.LogInformation("🔄 Game state UI updated: {GameState}", gameInfo.GameState);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error updating game state UI");
        }
    }

    /// <summary>
    /// 更新血量显示
    /// </summary>
    public static void UpdateHealthDisplay(int currentHealth)
    {
        try
        {
            if (healthLabel != null)
            {
                healthLabel.Text = $"❤️ 血量: {currentHealth}/20";
                
                // 根据血量变化颜色
                var color = currentHealth switch
                {
                    >= 15 => Color.Green,
                    >= 10 => Color.Yellow,
                    >= 5 => Color.Orange,
                    _ => Color.Red
                };
                
                healthLabel.TextColor = new SolidColorBrush(color);
                Game.Logger.LogDebug("🩺 Health display updated: {health}/20", currentHealth);
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error updating health display");
        }
    }

    /// <summary>
    /// 更新金币显示
    /// </summary>
    public static void UpdateGoldDisplay(int currentGold)
    {
        try
        {
            if (goldLabel != null)
            {
                goldLabel.Text = $"💰 金币: {currentGold}";
                Game.Logger.LogDebug("💰 Gold display updated: {gold}", currentGold);
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error updating gold display");
        }
    }

    /// <summary>
    /// 更新商店按钮文字
    /// </summary>
    private static void UpdateShopButtonText(string text)
    {
        try
        {
            if (shopButton?.Children != null && shopButton.Children.Count > 0)
            {
                if (shopButton.Children[0] is Label buttonLabel)
                {
                    buttonLabel.Text = text;
                }
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error updating shop button text");
        }
    }

    /// <summary>
    /// 获取错误消息
    /// </summary>
    private static string GetErrorMessage(int resultCode)
    {
        return resultCode switch
        {
            1 => "游戏已经开始",
            2 => "游戏尚未开始",
            3 => "操作失败",
            98 => "服务器处理异常",
            99 => "未知命令",
            _ => $"错误代码: {resultCode}"
        };
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
                Game.Logger.LogWarning("⚠️ 无法初始化游戏系统UI：本地玩家或主单位为空");
                return;
            }

            var mainUnit = localPlayer.MainUnit;
            Game.Logger.LogInformation("🎮 正在为主单位初始化游戏系统UI: {unit}", mainUnit.Cache?.Name ?? "Unknown");

            // 1. 创建技能摇杆组
            if (abilityJoyStickGroup == null)
            {
                abilityJoyStickGroup = new AbilityJoyStickGroup()
                {
                    // 调整位置避免与塔防UI冲突
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    WidthStretchRatio = 0.4f,  // 缩小一些
                    HeightStretchRatio = 0.4f, // 缩小一些
                    ZIndex = 800, // 确保在其他UI之上
                };
                
                abilityJoyStickGroup.BindUnit = mainUnit;
                abilityJoyStickGroup.AddToRoot();
                Game.Logger.LogInformation("✅ 技能摇杆组已创建并绑定到主单位");
            }


            // 2. 延迟获取背包管理器（等背包系统初始化完成）
            // 等一会，不然取不到背包
            await Game.Delay(TimeSpan.FromSeconds(1));
            
            try
            {
                var inventoryManager = mainUnit.GetComponent<InventoryManager>();
                if (inventoryManager?.Inventories?.Count > 0)
                {
                    // 创建背包UI
                    if (inventoryUI == null)
                    {
                        inventoryUI = new DefaultInventoryUI()
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            ZIndex = 900,
                            Visible = false, // 默认隐藏
                            BindUnit = mainUnit
                        };
                        inventoryUI.AddToRoot();
                        Game.Logger.LogInformation("✅ 背包UI已创建");
                    }
                    
                    // 创建背包入口按钮
                    if (inventoryEntrance == null)
                    {
                        inventoryEntrance = new InventoryUIEntrance(inventoryUI)
                        {
                            VerticalAlignment = VerticalAlignment.Top,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Position = new GameUI.Struct.UIPosition(350, 10), // 避免与资源面板冲突
                            ZIndex = 850,
                        };
                        inventoryEntrance.AddToRoot();
                        Game.Logger.LogInformation("✅ 背包入口按钮已创建");
                    }

                    // 创建快捷栏
                    if (quickBarUI == null)
                    {
                        quickBarUI = new QuickBarUI()
                        {
                            VerticalAlignment = VerticalAlignment.Bottom,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Position = new GameUI.Struct.UIPosition(0, 0), // 贴底显示，参考模板默认值
                            ZIndex = 850,
                            BindUnit = mainUnit
                        };
                        quickBarUI.AddToRoot();
                        Game.Logger.LogInformation("✅ 快捷栏已创建");
                    }

                    // 创建拾取按钮
                    if (pickButton == null)
                    {
                        pickButton = new PickButton(mainUnit)
                        {
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            Position = new GameUI.Struct.UIPosition(200, 0), // 向右偏移200
                            ZIndex = 850,
                        };
                        pickButton.AddToRoot();
                        Game.Logger.LogInformation("✅ 拾取按钮已创建");
                    }
                    
                    Game.Logger.LogInformation("✅ 背包系统和相关UI初始化完成");
                }
                else
                {
                    Game.Logger.LogWarning("⚠️ 延迟后仍无法获取到背包管理器或背包为空");
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogError("❌ 初始化背包系统时发生错误: {error}", ex.Message);
            }

            Game.Logger.LogInformation("🎉 游戏系统UI初始化完成！");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 初始化游戏系统UI时发生错误");
        }
    }

    /// <summary>
    /// 隐藏游戏系统UI（技能摇杆和背包系统）
    /// </summary>
    private static void HideGameSystemUI()
    {
        try
        {
            // 隐藏技能摇杆组
            if (abilityJoyStickGroup != null)
            {
                abilityJoyStickGroup.Visible = false;
                Game.Logger.LogInformation("🎮 技能摇杆组已隐藏");
            }

            // 隐藏背包UI
            if (inventoryUI != null)
            {
                inventoryUI.Visible = false;
                Game.Logger.LogInformation("🎒 背包UI已隐藏");
            }

            // 隐藏背包入口按钮
            if (inventoryEntrance != null)
            {
                inventoryEntrance.Visible = false;
                Game.Logger.LogInformation("🎒 背包入口按钮已隐藏");
            }

            // 隐藏快捷栏
            if (quickBarUI != null)
            {
                quickBarUI.Visible = false;
                Game.Logger.LogInformation("⚡ 快捷栏已隐藏");
            }

            // 隐藏拾取按钮
            if (pickButton != null)
            {
                pickButton.Visible = false;
                Game.Logger.LogInformation("📦 拾取按钮已隐藏");
            }

            Game.Logger.LogInformation("✅ 游戏系统UI已全部隐藏");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 隐藏游戏系统UI时发生错误");
        }
    }


}
#endif
