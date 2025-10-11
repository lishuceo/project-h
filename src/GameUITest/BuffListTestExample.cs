#if CLIENT
using GameCore.GameSystem.Data;
using GameUI.Control.Primitive;
using GameSystemUI.BuffSystemUI.Advanced;
using GameCore.BuffSystem.Data.Enum;
using GameCore.EntitySystem;
using GameCore;
using GameUI.Struct;
using GameUI.Enum;
using System.Drawing;
using System.Threading.Tasks;
using GameUI.Control.Extensions;
using GameCore.PlayerAndUsers;
using GameCore.Behavior;
using GameCore.BuffSystem;
using GameCore.BuffSystem.Manager;
using GameCore.BuffSystem.Data;
using GameCore.Components;
using GameCore.BaseType;
using System.Linq;
using GameData;
using System;
using System.Threading.Tasks;
using TriggerEncapsulation;
using Events;


namespace GameEntry.GameUITest;

/// <summary>
/// Buff列表测试示例 - 基于BuffBar组件
/// 演示如何在GameUITest模式下测试Buff显示功能
/// </summary>
public class BuffListTestExample : IGameClass
{
    private static Label? statusLabel;
    private static BuffBar? buffBar;
    private static Panel? mainPanel;
    private static Panel? buffTestPanel;
    private static Button? addBuffButton;
    private static Button? addPermanentTestBuffButton;
    private static Button? addSlowDebuffButton;
    private static Button? clearBuffButton;

    private static Panel? testIconPanel;
    private static BuffIcon? singleBuffIcon;
    
    // 单位信息面板相关
    private static Panel? unitInfoPanel;
    private static Label? unitInfoTitleLabel;
    private static Label? unitPropertiesLabel;
    
    // 实时刷新机制 - 使用游戏帧更新代替Timer
    private static int refreshFrameCounter = 0;
    private static readonly int refreshFrameInterval = 30; // 30帧刷新一次（约500ms，假设60FPS）
    private static bool isRefreshActive = false;
    private static readonly Trigger<GameCore.Event.EventGameTick> refreshTrigger;

    static BuffListTestExample()
    {
        try
        {
            refreshTrigger = new Trigger<GameCore.Event.EventGameTick>(async (s, e) => 
            {
                OnFrameUpdate();
                await Task.CompletedTask;
                return true;
            });
            refreshTrigger.Register(Game.Instance);
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError("创建 BuffListTestExample 更新触发器失败: {ex}", ex.Message);
            throw;
        }
    }

    public static void OnRegisterGameClass()
    {
        // 在GameUITest模式下注册Buff列表测试
        // 注册到Game初始化事件，让主测试类来控制
        Game.OnGameDataInitialization += () =>
        {
            // 这个类由GameUITestMode统一管理，不直接注册UI初始化
        };
    }

    /// <summary>
    /// 初始化Buff列表测试界面
    /// </summary>
    public static void InitializeBuffListTest()
    {
        Game.Logger?.LogInformation("🩸 初始化Buff列表测试界面...");

        try
        {
            CreateMainPanel();
            CreateStatusLabel();
            CreateBuffBar();
            CreateTestButtons();
            CreateUnitInfoPanel(); // 🆕 添加单位信息面板
            SetupBuffBinding();
            StartRealtimeRefresh();
            SetupFrameUpdate();

            Game.Logger?.LogInformation("✅ Buff列表测试界面初始化完成喵！");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ Buff列表测试界面初始化失败: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 创建主面板
    /// </summary>
    private static void CreateMainPanel()
    {
        mainPanel = new Panel
        {
            WidthStretchRatio = 1.0f,
            HeightStretchRatio = 1.0f
        };

        mainPanel.AddToRoot();
    }

    /// <summary>
    /// 创建状态标签
    /// </summary>
    private static void CreateStatusLabel()
    {
        statusLabel = new Label
        {
            Text = "🩸 Buff列表测试模式\n\n等待绑定单位...",
            FontSize = 16,
            TextColor = Color.Yellow,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Position = new UIPosition(50, 400),
            Width = 400,
            Height = 150
        };

        mainPanel?.AddChild(statusLabel);
    }

    /// <summary>
    /// 创建Buff条和测试BuffIcon
    /// </summary>
    private static void CreateBuffBar()
    {
        buffBar = new BuffBar
        {
            // 设置BuffBar位置到屏幕上方中央
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Position = new UIPosition(0, 200),
            Width = 600,
            Height = 80
        };

        mainPanel?.AddChild(buffBar);
        
        // 创建单独的BuffIcon来显示第一个buff
        CreateSingleBuffIcon();
    }
    
    /// <summary>
    /// 创建单独的BuffIcon来显示BuffManager中的第一个buff
    /// </summary>
    private static void CreateSingleBuffIcon()
    {
        try
        {
            Game.Logger?.LogInformation("🧪 创建单独的BuffIcon来显示第一个buff...");
            
            // 创建一个容器面板来放置单个BuffIcon
            testIconPanel = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Position = new UIPosition(0, 300), // 在BuffBar下方
                Width = 200,
                Height = 100,
                FlowOrientation = Orientation.Horizontal
            };
            
            // 创建单个BuffIcon，用于显示第一个buff
            singleBuffIcon = new BuffIcon
            {
                Width = 64,
                Height = 64,
                Margin = new GameUI.Struct.Thickness(5, 0, 5, 0)
            };
            
            // 绑定到当前的主控单位（和BuffBar使用相同的单位）
            SetupSingleBuffIconBinding();
            
            testIconPanel.AddChild(singleBuffIcon);
            
            Game.Logger?.LogInformation("🔍 将BuffIcon面板添加到主面板...");
            mainPanel?.AddChild(testIconPanel);
            
            // 添加说明标签
            var instructionLabel = new Label
            {
                Text = "🧪 BuffBar显示全部的Buff\n" +
                    "🧪 单个BuffIcon显示BuffManager中的第一个Buff\n" +
                    "🔄 会自动绑定到BuffManager中的第一个buff\n" +
                    "🟢 绿框=正面Buff  🔴 红框=负面Buff  ⚫ 灰框=中性Buff",
                FontSize = 14,
                TextColor = Color.Cyan,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Position = new UIPosition(0, 500),
                Width = 600,
                Height = 80
            };
            
            mainPanel?.AddChild(instructionLabel);
            
            Game.Logger?.LogInformation("✅ 单个BuffIcon创建完成！");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 创建单个BuffIcon时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 设置单个BuffIcon的绑定 - 获取单位的第一个buff并绑定
    /// </summary>
    private static void SetupSingleBuffIconBinding()
    {
        try
        {
            UpdateSingleBuffIconFromUnit();
            Game.Logger?.LogInformation("🔗 BuffIcon绑定设置完成");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "⚠️ 设置BuffIcon绑定时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 从当前单位获取第一个buff并更新BuffIcon
    /// </summary>
    private static void UpdateSingleBuffIconFromUnit()
    {
        try
        {
            if (singleBuffIcon == null) return;

            // 获取当前主控单位
            var currentMainUnit = Player.LocalPlayer?.MainUnit;
            if (currentMainUnit == null)
            {
                Game.Logger?.LogDebug("没有主控单位，清除BuffIcon");
                singleBuffIcon.Buff = null;
                return;
            }

            // 获取单位的BuffManager
            if (currentMainUnit is Unit unit)
            {
                var buffManager = unit.GetComponent<BuffManager>();
                if (buffManager == null)
                {
                    Game.Logger?.LogDebug("单位 {unit} 没有 BuffManager 组件", unit);
                    singleBuffIcon.Buff = null;
                    return;
                }

                // 获取第一个buff
                var allBuffs = buffManager.GetAll();
                var firstBuff = allBuffs?.FirstOrDefault();
                
                if (firstBuff is Buff typedBuff)
                {
                    // Game.Logger?.LogDebug("BuffIcon获取到第一个buff: {type}", firstBuff.GetType().Name);
                    singleBuffIcon.Buff = typedBuff;
                }
                else
                {
                    // Game.Logger?.LogDebug("单位 {unit} 没有buff", unit);
                    singleBuffIcon.Buff = null;
                }
            }
            else
            {
                Game.Logger?.LogDebug("主控单位不是Unit类型: {type}", currentMainUnit?.GetType().Name ?? "null");
                singleBuffIcon.Buff = null;
            }
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "从单位获取buff时出错: {message}", ex.Message);
            if (singleBuffIcon != null)
                singleBuffIcon.Buff = null;
        }
    }

    /// <summary>
    /// 创建测试按钮
    /// </summary>
    private static void CreateTestButtons()
    {
        // 创建测试按钮面板
        buffTestPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Position = new UIPosition(-50, 50),
            Width = 200,
            Height = 300, // 增加高度以容纳新按钮
            FlowOrientation = Orientation.Vertical
        };

        // 添加模拟Buff按钮
        addBuffButton = new Button
        {
            Width = 180,
            Height = 40,
            Margin = new GameUI.Struct.Thickness(0, 5, 0, 5),
            Background = new GameUI.Brush.SolidColorBrush(Color.Green)
        };

        var addBuffLabel = new Label
        {
            Text = "⚡ 添加眩晕Buff",
            FontSize = 14,
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        addBuffButton.AddChild(addBuffLabel);

        // 添加永久测试Buff按钮
        addPermanentTestBuffButton = new Button
        {
            Width = 180,
            Height = 40,
            Margin = new GameUI.Struct.Thickness(0, 5, 0, 5),
            Background = new GameUI.Brush.SolidColorBrush(Color.Purple)
        };

        var addPermanentTestBuffLabel = new Label
        {
            Text = "💎 添加永久测试Buff",
            FontSize = 14,
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        addPermanentTestBuffButton.AddChild(addPermanentTestBuffLabel);

        // 🐌 添加减速Buff按钮
        addSlowDebuffButton = new Button
        {
            Width = 180,
            Height = 40,
            Margin = new GameUI.Struct.Thickness(0, 5, 0, 5),
            Background = new GameUI.Brush.SolidColorBrush(Color.Blue)
        };

        var addSlowDebuffLabel = new Label
        {
            Text = "🐌 添加减速Buff",
            FontSize = 14,
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        addSlowDebuffButton.AddChild(addSlowDebuffLabel);

        // 清除Buff按钮
        clearBuffButton = new Button
        {
            Width = 180,
            Height = 40,
            Margin = new GameUI.Struct.Thickness(0, 5, 0, 5),
            Background = new GameUI.Brush.SolidColorBrush(Color.Red)
        };

        var clearBuffLabel = new Label
        {
            Text = "🗑️ 清除Buff",
            FontSize = 14,
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        clearBuffButton.AddChild(clearBuffLabel);
        
        buffTestPanel.AddChild(addBuffButton);
        buffTestPanel.AddChild(addPermanentTestBuffButton);
        buffTestPanel.AddChild(addSlowDebuffButton);
        buffTestPanel.AddChild(clearBuffButton);
        mainPanel?.AddChild(buffTestPanel);

        // 绑定按钮事件
        SetupButtonEvents();
    }

    /// <summary>
    /// 创建单位信息面板 - 显示在界面右侧
    /// </summary>
    private static void CreateUnitInfoPanel()
    {
        try
        {
            Game.Logger?.LogInformation("📊 创建单位信息面板...");
            
            // 创建单位信息面板
            unitInfoPanel = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Position = new UIPosition(-450, 50), // 在右侧按钮左边
                Width = 380,
                Height = 500,
                Background = new GameUI.Brush.SolidColorBrush(Color.FromArgb(180, 20, 30, 50)) // 深色半透明背景
            };

            // 标题
            unitInfoTitleLabel = new Label
            {
                Text = "📊 单位属性信息",
                FontSize = 16,
                TextColor = Color.Cyan,
                Bold = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Position = new UIPosition(0, 10),
                Width = 380,
                Height = 25
            };

            // 属性信息标签
            unitPropertiesLabel = new Label
            {
                Text = "等待刷新...",
                FontSize = 12,
                TextColor = Color.White,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Position = new UIPosition(10, 40),
                Width = 360,
                Height = 450
            };

            unitInfoPanel.AddChild(unitInfoTitleLabel);
            unitInfoPanel.AddChild(unitPropertiesLabel);
            mainPanel?.AddChild(unitInfoPanel);
            
            Game.Logger?.LogInformation("✅ 单位信息面板创建完成！");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 创建单位信息面板时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private static void SetupButtonEvents()
    {
        if (addBuffButton != null)
        {
            addBuffButton = addBuffButton.Click(() =>
            {
                AddTestBuff();
            });
        }

        if (addPermanentTestBuffButton != null)
        {
            addPermanentTestBuffButton = addPermanentTestBuffButton.Click(() =>
            {
                AddPermanentTestBuff();
            });
        }

        if (addSlowDebuffButton != null)
        {
            addSlowDebuffButton = addSlowDebuffButton.Click(() =>
            {
                AddSlowDebuff();
            });
        }

        if (clearBuffButton != null)
        {
            clearBuffButton = clearBuffButton.Click(() =>
            {
                ClearAllBuffs();
            });
        }
        

    }



    /// <summary>
    /// 添加测试Buff - 只发送请求，依赖帧刷新显示结果
    /// </summary>
    private static void AddTestBuff()
    {
        try
        {
            var currentUnit = buffBar?.BindUnit;
            if (currentUnit == null)
            {
                Game.Logger?.LogWarning("❌ 没有绑定的单位，无法添加Buff");
                return;
            }

            Game.Logger?.LogInformation("🧪 发送添加眩晕Buff请求到单位: {unit}", currentUnit.GetType().Name);

#if CLIENT
            // 客户端：只发送请求，不等待响应
            try
            {
                var success = BuffTestCommandSender.SendCommand(
                    BuffTestCommandType.AddStunBuff, 
                    currentUnit.SyncId,
                    5.0f
                );

                if (success)
                {
                    Game.Logger?.LogInformation("📡 已发送添加Buff请求 (眩晕 5秒)");
                }
                else
                {
                    Game.Logger?.LogWarning("❌ 发送Buff请求失败");
                }
            }
            catch (Exception ex)
            {
                Game.Logger?.LogWarning(ex, "发送buff请求失败");
            }
#else
            // 服务端：直接执行Buff添加逻辑
            var buffManager = currentUnit.GetComponent<BuffManager>();
            if (buffManager == null)
            {
                Game.Logger?.LogWarning("❌ 单位没有BuffManager组件");
                return;
            }

            // 使用ScopeData中预定义的眩晕Buff
            var addResult = buffManager.AddBuff(ScopeData.Buff.Stun, currentUnit);
            
            if (!addResult.IsSuccess)
            {
                Game.Logger?.LogError("❌ 添加眩晕Buff失败: {ErrorMessage}", addResult.ErrorMessage);
                return;
            }

            Game.Logger?.LogInformation("✅ 服务端成功添加眩晕Buff！");
#endif
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 添加Buff时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 添加永久测试Buff - 只发送请求，依赖帧刷新显示结果
    /// </summary>
    private static void AddPermanentTestBuff()
    {
        try
        {
            var currentUnit = buffBar?.BindUnit;
            if (currentUnit == null)
            {
                Game.Logger?.LogWarning("❌ 没有绑定的单位，无法添加永久测试Buff");
                return;
            }

            Game.Logger?.LogInformation("💎 发送添加永久测试Buff请求到单位: {unit}", currentUnit.GetType().Name);

#if CLIENT
            // 客户端：只发送请求，不等待响应
            try
            {
                var success = BuffTestCommandSender.SendCommand(
                    BuffTestCommandType.AddTestBuff, 
                    currentUnit.SyncId,
                    0.0f  // 永久Buff不需要持续时间
                );

                if (success)
                {
                    Game.Logger?.LogInformation("📡 已发送添加永久测试Buff请求");
                }
                else
                {
                    Game.Logger?.LogWarning("❌ 发送永久测试Buff请求失败");
                }
            }
            catch (Exception ex)
            {
                Game.Logger?.LogWarning(ex, "发送永久测试buff请求失败");
            }
#else
            // 服务端：直接执行Buff添加逻辑
            var buffManager = currentUnit.GetComponent<BuffManager>();
            if (buffManager == null)
            {
                Game.Logger?.LogWarning("❌ 单位没有BuffManager组件");
                return;
            }

            // 使用ScopeData中预定义的永久测试Buff
            var addResult = buffManager.AddBuff(ScopeData.Buff.TestBuff, currentUnit);
            
            if (!addResult.IsSuccess)
            {
                Game.Logger?.LogError("❌ 添加永久测试Buff失败: {ErrorMessage}", addResult.ErrorMessage);
                return;
            }

            Game.Logger?.LogInformation("✅ 服务端成功添加永久测试Buff！");
#endif
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 添加永久测试Buff时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 添加减速Buff - 只发送请求，依赖帧刷新显示结果
    /// </summary>
    private static void AddSlowDebuff()
    {
        try
        {
            var currentUnit = buffBar?.BindUnit;
            if (currentUnit == null)
            {
                Game.Logger?.LogWarning("❌ 没有绑定的单位，无法添加减速Buff");
                return;
            }

            Game.Logger?.LogInformation("🐌 发送添加减速Buff请求到单位: {unit}", currentUnit.GetType().Name);

#if CLIENT
            // 客户端：只发送请求，不等待响应
            try
            {
                var success = BuffTestCommandSender.SendCommand(
                    BuffTestCommandType.AddSlowDebuff, 
                    currentUnit.SyncId,
                    2.0f  // 减速Buff持续2秒
                );

                if (success)
                {
                    Game.Logger?.LogInformation("📡 已发送添加减速Buff请求 (持续2秒，减速50%)");
                }
                else
                {
                    Game.Logger?.LogWarning("❌ 发送减速Buff请求失败");
                }
            }
            catch (Exception ex)
            {
                Game.Logger?.LogWarning(ex, "发送减速buff请求失败");
            }
#else
            // 服务端：直接执行Buff添加逻辑
            var buffManager = currentUnit.GetComponent<BuffManager>();
            if (buffManager == null)
            {
                Game.Logger?.LogWarning("❌ 单位没有BuffManager组件");
                return;
            }

            // 使用ScopeData中的SlowDebuff
            var addResult = buffManager.AddBuff(ScopeData.Buff.SlowDebuff, currentUnit);
            
            if (!addResult.IsSuccess)
            {
                Game.Logger?.LogError("❌ 添加减速Buff失败: {ErrorMessage}", addResult.ErrorMessage);
                return;
            }

            Game.Logger?.LogInformation("✅ 服务端成功添加减速Buff！");
#endif
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 添加减速Buff时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 清除所有Buff - 只发送请求，依赖帧刷新显示结果
    /// </summary>
    private static void ClearAllBuffs()
    {
        try
        {
            var currentUnit = buffBar?.BindUnit;
            if (currentUnit == null)
            {
                Game.Logger?.LogWarning("❌ 没有绑定的单位，无法清除Buff");
                return;
            }

            Game.Logger?.LogInformation("🗑️ 发送清除所有Buff请求到单位: {unit}", currentUnit.GetType().Name);

#if CLIENT
            // 客户端：只发送请求，不等待响应
            try
            {
                var success = BuffTestCommandSender.SendCommand(
                    BuffTestCommandType.ClearAllBuffs, 
                    currentUnit.SyncId
                );

                if (success)
                {
                    Game.Logger?.LogInformation("📡 已发送清除所有Buff请求");
                }
                else
                {
                    Game.Logger?.LogWarning("❌ 发送清除Buff请求失败");
                }
            }
            catch (Exception ex)
            {
                Game.Logger?.LogWarning(ex, "发送清除buff请求失败");
            }
#endif
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 清除Buff时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 设置Buff绑定 - 参考AbilityJoyStickGroup的方式，直接绑定
    /// </summary>
    private static void SetupBuffBinding()
    {
        try
        {
            // 直接获取并绑定主控单位，不使用异步检查
            var currentMainUnit = Player.LocalPlayer?.MainUnit;
            OnMainUnitChanged(currentMainUnit);
            
            Game.Logger?.LogInformation("🔍 BuffBar初始绑定完成");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "⚠️ 初始绑定主控单位时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 主控单位变化处理 - 使用新的绑定方式，模仿摇杆
    /// </summary>
    private static void OnMainUnitChanged(Unit? unit)
    {
        try
        {
            if (buffBar != null)
            {
                // 使用Unit属性绑定单位，兼容新的绑定方式
                buffBar.BindUnit = unit;
                
                var unitData = unit?.GetType().Name ?? "未知单位";
                var unitId = unit?.GetHashCode() ?? 0;
                
                // 单位绑定成功，状态显示由定时器自动刷新，无需手动更新
                Game.Logger?.LogInformation("🩸 BuffBar已绑定到单位: {unitName} (ID: {unitId})", 
                    unitData, unitId);
            }

            // 同时更新单个BuffIcon的绑定
            UpdateSingleBuffIconFromUnit();
            Game.Logger?.LogInformation("🔗 BuffIcon也已更新绑定");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 绑定BuffBar和BuffIcon到单位时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 更新状态标签 - 支持实时刷新当前Buff状态
    /// </summary>
    private static void UpdateStatus(string message = "")
    {
        if (statusLabel != null)
        {
            try
            {
                // 如果没有提供消息，则获取实时状态
                if (string.IsNullOrEmpty(message))
                {
                    message = GetCurrentBuffStatus();
                }
                
                statusLabel.Text = $"🩸 Buff列表测试模式\n\n{message}";
            }
            catch (Exception ex)
            {
                Game.Logger?.LogError(ex, "更新状态标签时出错: {message}", ex.Message);
                statusLabel.Text = $"🩸 Buff列表测试模式\n\n❌ 状态更新出错: {ex.Message}";
            }
        }
    }
    
    /// <summary>
    /// 获取当前实时的Buff状态信息
    /// </summary>
    private static string GetCurrentBuffStatus()
    {
        try
        {
            var currentUnit = buffBar?.BindUnit;
            if (currentUnit == null)
            {
                return "⚠️ 没有绑定的单位\n等待绑定单位...";
            }
            
            var unitData = currentUnit?.GetType().Name ?? "未知单位";
            var unitId = currentUnit?.GetHashCode() ?? 0;
            
            // 获取实时BuffManager状态信息
            string buffSystemStatus = "未连接";
            int buffCount = 0;
            string buffDetails = "";
            
            var buffManager = currentUnit?.GetComponent<BuffManager>();
            if (buffManager != null)
            {
                var allBuffs = buffManager.GetAll();
                buffCount = allBuffs.Count();
                buffSystemStatus = "已连接";
                
                // 获取详细的Buff信息用于显示
                if (buffCount > 0)
                {
                    var buffList = allBuffs.Take(5).Select((buff, index) => 
                    {
                        try
                        {
                            if (buff is Buff realBuff && buff is GameCore.DisplayInfo.IDisplayInfo displayInfo)
                            {
                                // 通过Cache获取配置的显示名称，如果没有则使用备用方案
                                // var name = realBuff.Cache?.Name ?? 
                                //           realBuff.GetType().Name ?? 
                                //           $"Buff_{index + 1}";
                                var name = displayInfo.DisplayName;
                                          
                                var remainingTime = displayInfo.Cooldown ?? 0;       // IDisplayInfo.Cooldown = 剩余时间
                                var duration = displayInfo.CoolDownMax ?? 999;        // IDisplayInfo.CoolDownMax = 最大时间
                                var stack = displayInfo.Stack ?? 0;                 // IDisplayInfo.Stack = 堆叠数
                                
                                // 判断是否为永久buff（参考BuffIcon的判断逻辑）
                                bool isPermanent = false;
                                try
                                {
                                    if (realBuff.Cache?.BuffFlags != null)
                                    {
                                        isPermanent = realBuff.Cache.BuffFlags.Permanent;
                                    }
                                }
                                catch
                                {
                                    // 忽略错误，继续使用默认值
                                }
                                if (isPermanent == false && displayInfo.CoolDownMax == null)
                                {
                                    isPermanent = true;
                                }
                                
                                // 根据是否永久选择显示格式
                                string timeInfo;
                                if (isPermanent || (duration <= 0 && remainingTime <= 0))
                                {
                                    timeInfo = "永久";
                                }
                                else
                                {
                                    timeInfo = $"{remainingTime:F1}s/{duration:F1}s";
                                }
                                
                                return $"  • {name} (层数:{stack}, {timeInfo})";
                            }
                            return $"  • Buff_{index + 1}";
                        }
                        catch (Exception ex)
                        {
                            Game.Logger?.LogWarning(ex, "解析Buff_{index}信息时出错", index + 1);
                            return $"  • Buff_{index + 1} (解析出错)";
                        }
                    });
                    
                    buffDetails = "\n\n🩸 当前Buff列表:\n" + string.Join("\n", buffList);
                    if (buffCount > 5)
                    {
                        buffDetails += $"\n  • ... 还有 {buffCount - 5} 个Buff";
                    }
                }
            }
            else
            {
                buffSystemStatus = "无BuffManager组件";
            }
            
            // 获取当前时间用于显示实时性
            var currentTime = DateTime.Now.ToString("HH:mm:ss");
            
            return $"✅ 已绑定单位: {unitData}\n" +
                   $"🎯 单位ID: {unitId}\n" +
                   $"🩸 Buff系统: {buffSystemStatus}\n" +
                   $"📊 当前Buff数量: {buffCount}\n" +
                   $"🕐 最后更新: {currentTime}\n\n" +
                   "📋 操作说明:\n" +
                   "• BuffBar会自动显示单位的所有Buff\n" +
                   "• 正面Buff显示绿色边框\n" +
                   "• 负面Buff显示红色边框\n" +
                   "• 显示CD倒计时和堆叠层数\n" +
                   "• 支持实时刷新（500ms间隔）\n" +
                   "• 显示格式: 名称 (层数:X, remainingTime/duration)" +
                   buffDetails;
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "获取当前Buff状态时出错: {message}", ex.Message);
            return $"❌ 获取状态失败: {ex.Message}";
        }
    }
    
    /// <summary>
    /// 启动实时刷新 - 使用帧更新机制
    /// </summary>
    private static void StartRealtimeRefresh()
    {
        try
        {
            isRefreshActive = true;
            refreshFrameCounter = 0;
            Game.Logger?.LogInformation("✅ 实时刷新已启动 (帧更新机制，每{interval}帧刷新一次)", refreshFrameInterval);
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "启动实时刷新时出错: {message}", ex.Message);
        }
    }
    
    /// <summary>
    /// 停止实时刷新
    /// </summary>
    private static void StopRealtimeRefresh()
    {
        try
        {
            isRefreshActive = false;
            Game.Logger?.LogInformation("🛑 实时刷新已停止");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "停止实时刷新时出错: {message}", ex.Message);
        }
    }
    
    /// <summary>
    /// 设置帧更新监听
    /// </summary>
    private static void SetupFrameUpdate()
    {
        // 帧更新监听已在静态构造函数中设置，这里只需要激活刷新
        Game.Logger?.LogInformation("✅ 帧更新监听已在静态构造函数中设置");
    }
    
    /// <summary>
    /// 移除帧更新监听
    /// </summary>
    private static void RemoveFrameUpdate()
    {
        // 帧更新监听由静态trigger管理，无需手动移除
        Game.Logger?.LogInformation("🛑 帧更新监听由静态trigger管理");
    }
    
    /// <summary>
    /// 帧更新回调
    /// </summary>
    private static void OnFrameUpdate()
    {
        try
        {
            if (!isRefreshActive) return;
            
            refreshFrameCounter++;
            
            // 每隔指定帧数刷新一次状态
            if (refreshFrameCounter >= refreshFrameInterval)
            {
                refreshFrameCounter = 0;
                UpdateStatus(); // 调用无参数版本，实时获取状态
                UpdateSingleBuffIconFromUnit(); // 同时更新BuffIcon的buff绑定
                UpdateUnitInfoPanel(); // 🆕 更新单位信息面板
            }
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "帧更新回调时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 更新单位信息面板
    /// </summary>
    private static void UpdateUnitInfoPanel()
    {
        try
        {
            if (unitPropertiesLabel == null) return;

            var currentUnit = buffBar?.BindUnit;
            if (currentUnit == null)
            {
                unitPropertiesLabel.Text = "❌ 没有绑定单位";
                return;
            }

            var unitInfo = GetUnitDetailedInfo(currentUnit);
            unitPropertiesLabel.Text = unitInfo;
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 更新单位信息面板时出错: {message}", ex.Message);
            if (unitPropertiesLabel != null)
                unitPropertiesLabel.Text = $"❌ 更新失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 获取单位详细信息
    /// </summary>
    private static string GetUnitDetailedInfo(Unit unit)
    {
        try
        {
            var info = new System.Text.StringBuilder();
            
            // 基本信息
            info.AppendLine("🏷️ 基本信息:");
            info.AppendLine($"  • 类型: {unit.GetType().Name}");
            info.AppendLine($"  • SyncID: {unit.SyncId}");
            info.AppendLine();

            // 核心属性 - 使用GetUnitPropertyFinal获取最终值
            info.AppendLine("⚔️ 核心属性:");
            
            // 生命相关
            var healthVital = unit.GetTagComponent<GameCore.VitalSystem.Vital>(GameCore.BaseType.PropertyVital.Health);
            var currentHealth = healthVital?.Current ?? 0;
            var maxHealth = healthVital?.Max ?? GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.LifeMax);
            info.AppendLine($"  • 生命: {currentHealth:F0}/{maxHealth:F0}");
            
            // 魔法相关
            var manaVital = unit.GetTagComponent<GameCore.VitalSystem.Vital>(GameCore.BaseType.PropertyVital.Mana);
            var currentMana = manaVital?.Current ?? 0;
            var maxMana = manaVital?.Max ?? GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.ManaMax);
            info.AppendLine($"  • 魔法: {currentMana:F0}/{maxMana:F0}");
            
            // 攻击属性
            var attackDamage = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.AttackDamage);
            var attackRange = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.AttackRange);
            info.AppendLine($"  • 攻击力: {attackDamage:F1}");
            info.AppendLine($"  • 攻击范围: {attackRange:F0}");
            
            // 防御属性
            var armor = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.Armor);
            var magicResistance = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.MagicResistance);
            info.AppendLine($"  • 护甲: {armor:F1}");
            info.AppendLine($"  • 魔抗: {magicResistance:F1}");
            
            // 移动属性
            var moveSpeed = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.MoveSpeed);
            var turningSpeed = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.TurningSpeed);
            info.AppendLine($"  • 移速: {moveSpeed:F1}");
            info.AppendLine($"  • 转速: {turningSpeed:F1}");
            info.AppendLine();

            // 位置信息
            info.AppendLine("📍 位置:");
            var position = unit.Position;
            info.AppendLine($"  • 坐标: ({position.X:F0},{position.Y:F0})");
            info.AppendLine($"  • 朝向: {unit.Facing:F0}°");

            return info.ToString();
        }
        catch (Exception ex)
        {
            return $"❌ 获取单位信息失败: {ex.Message}";
        }
    }

    /// <summary>
    /// 获取单位属性值 - 使用GetUnitPropertyFinal获取最终值（包含Buff修改）
    /// </summary>
    private static double GetUnitPropertyValue(Unit unit, GameData.IGameLink<GameCore.Data.GameDataUnitProperty> property)
    {
        try
        {
            var result = unit.GetUnitPropertyFinal(property);
            return result ?? 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public static void Cleanup()
    {
        // 首先停止实时刷新和帧更新监听
        StopRealtimeRefresh();
        RemoveFrameUpdate();
        
        if (mainPanel != null)
        {
            mainPanel.RemoveFromParent();
            mainPanel = null;
        }
        
        statusLabel = null;
        buffBar = null;
        buffTestPanel = null;
        addBuffButton = null;
        addPermanentTestBuffButton = null;
        addSlowDebuffButton = null;
        clearBuffButton = null;

        testIconPanel = null;
        singleBuffIcon = null;
        
        // 清理单位信息面板
        unitInfoPanel = null;
        unitInfoTitleLabel = null;
        unitPropertiesLabel = null;
        
        Game.Logger?.LogInformation("🧹 BuffListTestExample资源已清理，包括帧更新监听");
    }
}
#endif
