#if CLIENT
using GameCore.GameSystem.Data;
using GameUI.Control.Primitive;
using GameCore;
using GameUI.Struct;
using GameUI.Enum;
using System.Drawing;
using GameUI.Control.Extensions;
using GameCore.PlayerAndUsers;
using GameCore.EntitySystem;
using GameCore.Behavior;
using GameCore.VitalSystem;
using GameCore.BuffSystem.Manager;
using GameCore.Data;
using GameCore.AbilitySystem.Manager;
using GameCore.Components;
using GameCore.BaseType;
using GameData;
using System;
using System.Linq;
using System.Threading.Tasks;
using TriggerEncapsulation;
using Events;

namespace GameEntry.GameUITest;

/// <summary>
/// 单位信息面板 - 展示单位的各种属性
/// 显示攻击力、生命值、移动速度等详细信息
/// </summary>
public class UnitInfoPanel : IGameClass
{
    private static Panel? mainPanel;
    private static Panel? infoPanel;
    private static Label? titleLabel;
    private static Label? unitNameLabel;
    private static Label? propertiesLabel;
    private static Button? refreshButton;
    private static Button? selectTargetButton;
    
    // 实时刷新机制
    private static int refreshFrameCounter = 0;
    private static readonly int refreshFrameInterval = 60; // 60帧刷新一次（约1秒，假设60FPS）
    private static bool isRefreshActive = false;
    private static readonly Trigger<GameCore.Event.EventGameTick> refreshTrigger;
    
    // 当前显示的单位
    private static Unit? currentUnit;

    static UnitInfoPanel()
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
            Game.Logger?.LogError("创建 UnitInfoPanel 更新触发器失败: {ex}", ex.Message);
            throw;
        }
    }

    public static void OnRegisterGameClass()
    {
        // 由GameUITestMode统一管理
    }

    /// <summary>
    /// 初始化单位信息面板
    /// </summary>
    public static void InitializeUnitInfoPanel()
    {
        Game.Logger?.LogInformation("📊 初始化单位信息面板...");

        try
        {
            CreateMainPanel();
            CreateInfoPanel();
            CreateButtons();
            SetupInitialUnit();
            StartRealtimeRefresh();

            Game.Logger?.LogInformation("✅ 单位信息面板初始化完成喵！");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 单位信息面板初始化失败: {message}", ex.Message);
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
    /// 创建信息显示面板
    /// </summary>
    private static void CreateInfoPanel()
    {
        // 主信息面板
        infoPanel = new Panel
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Position = new UIPosition(50, 50),
            Width = 400,
            Height = 600,
            Background = new GameUI.Brush.SolidColorBrush(Color.FromArgb(200, 30, 30, 50)) // 深色半透明背景
        };

        // 标题
        titleLabel = new Label
        {
            Text = "📊 单位信息面板",
            FontSize = 18,
            TextColor = Color.Cyan,
            Bold = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Position = new UIPosition(0, 10),
            Width = 400,
            Height = 30
        };

        // 单位名称
        unitNameLabel = new Label
        {
            Text = "📍 单位: 未选择",
            FontSize = 16,
            TextColor = Color.Yellow,
            Bold = true,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Position = new UIPosition(10, 50),
            Width = 380,
            Height = 25
        };

        // 属性信息标签
        propertiesLabel = new Label
        {
            Text = "等待刷新...",
            FontSize = 14,
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Position = new UIPosition(10, 80),
            Width = 380,
            Height = 450
        };

        infoPanel.AddChild(titleLabel);
        infoPanel.AddChild(unitNameLabel);
        infoPanel.AddChild(propertiesLabel);
        mainPanel?.AddChild(infoPanel);
    }

    /// <summary>
    /// 创建操作按钮
    /// </summary>
    private static void CreateButtons()
    {
        // 刷新按钮
        refreshButton = new Button
        {
            Width = 120,
            Height = 35,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Position = new UIPosition(-180, 50),
            Background = new GameUI.Brush.SolidColorBrush(Color.Green)
        };

        var refreshLabel = new Label
        {
            Text = "🔄 刷新",
            FontSize = 14,
            TextColor = Color.White,
            Bold = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        refreshButton.AddChild(refreshLabel);

        // 选择目标按钮
        selectTargetButton = new Button
        {
            Width = 120,
            Height = 35,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Position = new UIPosition(-50, 50),
            Background = new GameUI.Brush.SolidColorBrush(Color.Blue)
        };

        var selectLabel = new Label
        {
            Text = "🎯 选择目标",
            FontSize = 14,
            TextColor = Color.White,
            Bold = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        selectTargetButton.AddChild(selectLabel);

        mainPanel?.AddChild(refreshButton);
        mainPanel?.AddChild(selectTargetButton);

        // 绑定按钮事件
        SetupButtonEvents();
    }

    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private static void SetupButtonEvents()
    {
        if (refreshButton != null)
        {
            refreshButton = refreshButton.Click(() =>
            {
                RefreshUnitInfo();
            });
        }

        if (selectTargetButton != null)
        {
            selectTargetButton = selectTargetButton.Click(() =>
            {
                SelectMainUnit();
            });
        }
    }

    /// <summary>
    /// 设置初始显示的单位
    /// </summary>
    private static void SetupInitialUnit()
    {
        SelectMainUnit();
    }

    /// <summary>
    /// 选择主控单位
    /// </summary>
    private static void SelectMainUnit()
    {
        try
        {
            var mainUnit = Player.LocalPlayer?.MainUnit as Unit;
            if (mainUnit != null)
            {
                currentUnit = mainUnit;
                Game.Logger?.LogInformation("🎯 已选择主控单位: {unitName}", mainUnit.GetType().Name);
                RefreshUnitInfo();
            }
            else
            {
                Game.Logger?.LogWarning("⚠️ 没有找到主控单位");
                currentUnit = null;
                UpdateUnitInfo("❌ 没有找到主控单位");
            }
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 选择主控单位时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 刷新单位信息
    /// </summary>
    private static void RefreshUnitInfo()
    {
        try
        {
            if (currentUnit == null)
            {
                UpdateUnitInfo("❌ 没有选择单位");
                return;
            }

            var unitInfo = GetUnitDetailedInfo(currentUnit);
            UpdateUnitInfo(unitInfo);
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ 刷新单位信息时出错: {message}", ex.Message);
            UpdateUnitInfo($"❌ 获取单位信息失败: {ex.Message}");
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
            info.AppendLine($"  • 单位类型: {unit.GetType().Name}");
            info.AppendLine($"  • SyncID: {unit.SyncId}");
            info.AppendLine();

            // 核心属性 - 使用GetUnitPropertyFinal获取最终值
            info.AppendLine("⚔️ 核心属性:");
            
            // 生命相关
            var healthVital = unit.GetTagComponent<GameCore.VitalSystem.Vital>(PropertyVital.Health);
            var currentHealth = healthVital?.Current ?? 0;
            var maxHealth = healthVital?.Max ?? GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.LifeMax);
            info.AppendLine($"  • 生命值: {currentHealth:F0} / {maxHealth:F0}");
            
            // 魔法相关
            var manaVital = unit.GetTagComponent<GameCore.VitalSystem.Vital>(PropertyVital.Mana);
            var currentMana = manaVital?.Current ?? 0;
            var maxMana = manaVital?.Max ?? GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.ManaMax);
            info.AppendLine($"  • 魔法值: {currentMana:F0} / {maxMana:F0}");
            
            // 攻击属性
            var attackDamage = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.AttackDamage);
            var attackRange = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.AttackRange);
            info.AppendLine($"  • 攻击力: {attackDamage:F1}");
            info.AppendLine($"  • 攻击范围: {attackRange:F0}");
            
            // 防御属性
            var armor = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.Armor);
            var magicResistance = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.MagicResistance);
            info.AppendLine($"  • 护甲: {armor:F1}");
            info.AppendLine($"  • 魔法抗性: {magicResistance:F1}");
            
            // 移动属性
            var moveSpeed = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.MoveSpeed);
            var turningSpeed = GetUnitPropertyValue(unit, GameCore.ScopeData.UnitProperty.TurningSpeed);
            info.AppendLine($"  • 移动速度: {moveSpeed:F1}");
            info.AppendLine($"  • 转向速度: {turningSpeed:F1}");
            info.AppendLine();

            // 位置信息
            info.AppendLine("📍 位置信息:");
            var position = unit.Position;
            info.AppendLine($"  • 坐标: ({position.X:F1}, {position.Y:F1}, {position.Z:F1})");
            var facing = unit.Facing;
            info.AppendLine($"  • 朝向: {facing:F1}°");
            info.AppendLine();

            // Buff信息
            info.AppendLine("🩸 Buff状态:");
            var buffManager = unit.GetComponent<BuffManager>();
            if (buffManager != null)
            {
                var allBuffs = buffManager.GetAll().ToList();
                if (allBuffs.Count > 0)
                {
                    info.AppendLine($"  • Buff数量: {allBuffs.Count}");
                    foreach (var buff in allBuffs.Take(5)) // 只显示前5个
                    {
                        if (buff is GameCore.DisplayInfo.IDisplayInfo displayInfo)
                        {
                            var name = displayInfo.DisplayName ?? "未知Buff";
                            var stack = displayInfo.Stack ?? 1;
                            var remaining = displayInfo.Cooldown ?? 0;
                            info.AppendLine($"    - {name} (层数:{stack}, 剩余:{remaining:F1}s)");
                        }
                    }
                    if (allBuffs.Count > 5)
                    {
                        info.AppendLine($"    - ... 还有 {allBuffs.Count - 5} 个Buff");
                    }
                }
                else
                {
                    info.AppendLine("  • 无Buff效果");
                }
            }
            else
            {
                info.AppendLine("  • 无BuffManager组件");
            }
            info.AppendLine();

            // 技能信息
            info.AppendLine("🎯 技能信息:");
            var abilityManager = unit.GetComponent<AbilityManager>();
            if (abilityManager != null)
            {
                // 这里可以添加技能信息获取逻辑
                info.AppendLine("  • 已装备技能管理器");
            }
            else
            {
                info.AppendLine("  • 无技能管理器");
            }
            info.AppendLine();

            // 更新时间
            var currentTime = DateTime.Now.ToString("HH:mm:ss");
            info.AppendLine($"🕐 最后更新: {currentTime}");

            return info.ToString();
        }
        catch (Exception ex)
        {
            return $"❌ 获取单位信息时出错: {ex.Message}";
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
    /// 更新单位信息显示
    /// </summary>
    private static void UpdateUnitInfo(string info)
    {
        if (propertiesLabel != null)
        {
            propertiesLabel.Text = info;
        }

        if (unitNameLabel != null && currentUnit != null)
        {
            unitNameLabel.Text = $"📍 单位: {currentUnit.GetType().Name} (SyncID: {currentUnit.SyncId})";
        }
        else if (unitNameLabel != null)
        {
            unitNameLabel.Text = "📍 单位: 未选择";
        }
    }

    /// <summary>
    /// 启动实时刷新
    /// </summary>
    private static void StartRealtimeRefresh()
    {
        try
        {
            isRefreshActive = true;
            refreshFrameCounter = 0;
            Game.Logger?.LogInformation("✅ 单位信息面板实时刷新已启动");
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
            Game.Logger?.LogInformation("🛑 单位信息面板实时刷新已停止");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "停止实时刷新时出错: {message}", ex.Message);
        }
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
                RefreshUnitInfo();
            }
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "单位信息面板帧更新回调时出错: {message}", ex.Message);
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public static void Cleanup()
    {
        StopRealtimeRefresh();
        
        if (mainPanel != null)
        {
            mainPanel.RemoveFromParent();
            mainPanel = null;
        }
        
        infoPanel = null;
        titleLabel = null;
        unitNameLabel = null;
        propertiesLabel = null;
        refreshButton = null;
        selectTargetButton = null;
        currentUnit = null;
        
        Game.Logger?.LogInformation("🧹 UnitInfoPanel资源已清理");
    }
}
#endif
