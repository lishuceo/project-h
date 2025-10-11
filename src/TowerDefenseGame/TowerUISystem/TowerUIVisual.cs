#if CLIENT
using GameCore;
using GameCore.EntitySystem;
using GameCore.Leveling;
using GameCore.Components;
using GameUI.Control.Primitive;
using GameUI.Control.Extensions;
using GameUI.Brush;
using GameUI.Enum;
using GameUI.Struct;
using GameUI.Device;
using GameUI.TriggerEvent;
using Events;
using GameEntry.TowerDefenseGame.TowerUpgradeSystem;
using System.Drawing;
using System.Numerics;
using static GameCore.ScopeData;

namespace GameEntry.TowerDefenseGame.TowerUISystem;

/// <summary>
/// 塔防可视化UI管理器 - 显示塔的升级/拆除按钮和详情面板
/// </summary>
public static class TowerUIVisual
{
    // 当前选中的塔
    private static Unit? selectedTower;
    
    // UI控件
    private static Button? upgradeButton;
    private static Button? demolishButton;
    private static Panel? infoPanel;
    private static Label? towerNameLabel;
    private static Label? towerLevelLabel;
    private static Label? towerAttackLabel;
    
    // 是否已初始化
    private static bool isInitialized = false;
    
    // 🔧 修复：保持对Trigger的引用，防止被垃圾回收
    private static Trigger<EventGamePointerButtonDown>? mouseTrigger;

    /// <summary>
    /// 初始化可视化UI系统
    /// </summary>
    public static void Initialize()
    {
        if (isInitialized) return;
        
        try
        {
            // 注册鼠标点击事件
            RegisterMouseClickEvents();
            
            isInitialized = true;
            Game.Logger.LogInformation("🏰 TowerUIVisual initialized successfully");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error initializing TowerUIVisual");
        }
    }

    /// <summary>
    /// 注册鼠标点击事件
    /// </summary>
    private static void RegisterMouseClickEvents()
    {
        // 🔧 修复：将Trigger存储为静态字段，防止被垃圾回收
        mouseTrigger = new Trigger<EventGamePointerButtonDown>(OnMouseClickAsync);
        mouseTrigger.Register(Game.Instance);
        
        Game.Logger.LogInformation("🖱️ Visual mouse click events registered");
    }

    /// <summary>
    /// 处理鼠标点击事件
    /// </summary>
    private static async Task<bool> OnMouseClickAsync(object sender, EventGamePointerButtonDown eventArgs)
    {
        try
        {
            if (!eventArgs.PointerPosition.HasValue)
                return false;

            // 使用射线检测获取点击的实体
            var clickedActors = DeviceInfo.PrimaryViewport.RaycastActor(eventArgs.PointerPosition.Value);
            
            Unit? clickedUnit = null;
            foreach (var actor in clickedActors)
            {
                if (actor is Unit unit && IsTowerUnit(unit))
                {
                    clickedUnit = unit;
                    break;
                }
            }

            if (clickedUnit != null)
            {
                // 点击了塔单位，显示UI
                ShowTowerUI(clickedUnit, eventArgs.PointerPosition.Value);
                Game.Logger.LogInformation("🏰 Tower clicked: {towerName}", clickedUnit.Cache?.Name ?? "Unknown");
            }
            else
            {
                // 点击了空地，隐藏UI
                HideTowerUI();
            }

            return false;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling visual mouse click");
            return false;
        }
    }

    /// <summary>
    /// 检查单位是否是塔（建筑）
    /// 使用 UnitFilter.Structure 来标识建筑类型的单位
    /// </summary>
    private static bool IsTowerUnit(Unit unit)
    {
        if (unit?.Cache?.Filter == null) return false;
        
        // 检查单位是否包含 Structure 过滤器标识
        return unit.Cache.Filter.Contains(UnitFilter.Structure);
    }

    /// <summary>
    /// 显示塔的UI界面
    /// </summary>
    private static void ShowTowerUI(Unit tower, UIPosition clickPosition)
    {
        selectedTower = tower;
        
        // 获取塔的基本信息
        var towerName = tower.Cache?.Name ?? "未知塔";
        var towerType = GetTowerTypeFromUnit(tower);
        var towerLevel = GetTowerLevel(tower);
        var attack = GetTowerAttack(tower);
        
        // 获取升级和拆除信息
        var canUpgrade = TowerUpgradeDataManager.CanUpgradeTower(towerType, towerLevel);
        var upgradePrice = TowerUpgradeDataManager.GetTowerUpgradePrice(towerType, towerLevel);
        var demolishRefund = TowerUpgradeDataManager.GetTowerDemolishRefund(towerType, towerLevel);
        
        // 将塔的世界坐标转换为UI坐标
        var towerUIPosition = DeviceInfo.PrimaryViewport.RaycastWorldToUI(tower.Position);
        
        if (towerUIPosition.IsHit)
        {
            // 创建或更新升级按钮（在塔下方）
            CreateUpgradeButton(towerUIPosition.UIPosition, canUpgrade, upgradePrice);
            
            // 创建或更新拆除按钮（在升级按钮旁边）
            CreateDemolishButton(towerUIPosition.UIPosition, demolishRefund);
        }
        else
        {
            // 如果坐标转换失败，使用点击位置
            CreateUpgradeButton(clickPosition, canUpgrade, upgradePrice);
            CreateDemolishButton(clickPosition, demolishRefund);
        }
        
        // 创建或更新详情面板（在塔的右侧）
        var panelPosition = towerUIPosition.IsHit ? towerUIPosition.UIPosition : clickPosition;
        CreateInfoPanel(towerName, towerLevel, attack, panelPosition);
        
        Game.Logger.LogInformation("🎮 Tower UI displayed for {towerName} at UI position ({x}, {y})", 
            towerName, towerUIPosition.IsHit ? towerUIPosition.UIPosition.Left : clickPosition.Left, 
            towerUIPosition.IsHit ? towerUIPosition.UIPosition.Top : clickPosition.Top);
    }

    /// <summary>
    /// 创建升级按钮
    /// </summary>
    private static void CreateUpgradeButton(UIPosition clickPosition, bool canUpgrade, int price)
    {
        // 清理旧按钮
        upgradeButton?.RemoveFromParent();
        
        // 创建新按钮
        upgradeButton = new Button()
        {
            Width = 80,
            Height = 30,
            Background = new SolidColorBrush(canUpgrade ? Color.Green : Color.Gray),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            // 位置在塔的下方偏左
            Margin = new Thickness(clickPosition.Left - 180, clickPosition.Top + 30, 0, 0)
        };
        
        // 添加按钮文字标签
        var buttonLabel = new Label()
        {
            Text = canUpgrade ? $"升级 {price}💰" : "已满级",
            FontSize = 12,
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        buttonLabel.Parent = upgradeButton;
        
        // 绑定点击事件
        upgradeButton = upgradeButton.Click(OnUpgradeButtonClicked);
        
        // 添加到根节点
        upgradeButton.AddToRoot();
    }

    /// <summary>
    /// 创建拆除按钮
    /// </summary>
    private static void CreateDemolishButton(UIPosition clickPosition, int refund)
    {
        // 清理旧按钮
        demolishButton?.RemoveFromParent();
        
        // 创建新按钮
        demolishButton = new Button()
        {
            Width = 80,
            Height = 30,
            Background = new SolidColorBrush(Color.Red),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            // 位置在塔的下方偏右
            Margin = new Thickness(clickPosition.Left - 80, clickPosition.Top + 30, 0, 0)
        };
        
        // 添加按钮文字标签
        var buttonLabel = new Label()
        {
            Text = $"拆除 {refund}💰",
            FontSize = 12,
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        buttonLabel.Parent = demolishButton;
        
        // 绑定点击事件
        demolishButton = demolishButton.Click(OnDemolishButtonClicked);
        
        // 添加到根节点
        demolishButton.AddToRoot();
    }

    /// <summary>
    /// 创建详情面板
    /// </summary>
    private static void CreateInfoPanel(string towerName, int level, float attack, UIPosition towerPosition)
    {
        // 清理旧面板
        infoPanel?.RemoveFromParent();
        
        // 创建详情面板（在塔的右侧）
        infoPanel = new Panel()
        {
            Width = 200,
            Height = 120,
            Background = new SolidColorBrush(Color.FromArgb(200, 40, 40, 40)),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            // 位置在塔的右侧，调整偏移量
            Margin = new Thickness(towerPosition.Left + 30, towerPosition.Top - 120, 0, 0)
        };
        
        // 塔名称标签
        towerNameLabel = new Label()
        {
            Text = towerName,
            FontSize = 16,
            Bold = true,
            TextColor = Color.Gold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 10, 10, 10)
        };
        towerNameLabel.Parent = infoPanel;
        
        // 等级标签
        towerLevelLabel = new Label()
        {
            Text = $"等级: {level}",
            FontSize = 14,
            TextColor = Color.LightBlue,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 45, 10, 10)
        };
        towerLevelLabel.Parent = infoPanel;
        
        // 攻击力标签
        towerAttackLabel = new Label()
        {
            Text = $"攻击: {attack:F0}",
            FontSize = 14,
            TextColor = Color.LightGreen,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(10, 75, 10, 10)
        };
        towerAttackLabel.Parent = infoPanel;
        
        // 添加到根节点
        infoPanel.AddToRoot();
    }

    /// <summary>
    /// 隐藏塔UI界面
    /// </summary>
    private static void HideTowerUI()
    {
        selectedTower = null;
        
        // 移除所有UI控件
        upgradeButton?.RemoveFromParent();
        demolishButton?.RemoveFromParent();
        infoPanel?.RemoveFromParent();
        
        upgradeButton = null;
        demolishButton = null;
        infoPanel = null;
        
        Game.Logger.LogInformation("🚫 Tower UI hidden");
    }

    /// <summary>
    /// 处理升级按钮点击
    /// </summary>
    private static void OnUpgradeButtonClicked()
    {
        if (selectedTower == null) return;
        
        var towerType = GetTowerTypeFromUnit(selectedTower);
        var currentLevel = GetTowerLevel(selectedTower);
        var upgradePrice = TowerUpgradeDataManager.GetTowerUpgradePrice(towerType, currentLevel);
        
        Game.Logger.LogInformation("🔧 Upgrade button clicked: {towerName}, Level {level} -> {nextLevel}, Price: {price}", 
            selectedTower.Cache?.Name ?? "Unknown", currentLevel, currentLevel + 1, upgradePrice);
        
        // 发送升级命令到服务端（使用新的协议）
        bool success = TowerUISystem.TowerCommandSender.SendUpgradeCommand(selectedTower, upgradePrice);
        
        if (success)
        {
            Game.Logger.LogInformation("✅ Tower upgrade processed successfully");
            
            // 刷新UI显示以反映新的等级和价格
            if (selectedTower != null)
            {
                // 使用塔的当前屏幕位置刷新UI
                var towerUIPosition = DeviceInfo.PrimaryViewport.RaycastWorldToUI(selectedTower.Position);
                var refreshPosition = towerUIPosition.IsHit ? towerUIPosition.UIPosition : new UIPosition(400, 300);
                ShowTowerUI(selectedTower, refreshPosition);
            }
        }
        else
        {
            Game.Logger.LogError("❌ Failed to upgrade tower");
        }
    }

    /// <summary>
    /// 处理拆除按钮点击
    /// </summary>
    private static void OnDemolishButtonClicked()
    {
        if (selectedTower == null) return;
        
        var towerType = GetTowerTypeFromUnit(selectedTower);
        var currentLevel = GetTowerLevel(selectedTower);
        var refundAmount = TowerUpgradeDataManager.GetTowerDemolishRefund(towerType, currentLevel);
        
        Game.Logger.LogInformation("💥 Demolish button clicked: {towerName}, Level {level}, Refund: {refund}", 
            selectedTower.Cache?.Name ?? "Unknown", currentLevel, refundAmount);
        
        // 发送拆除命令到服务端（使用新的协议）
        bool success = TowerUISystem.TowerCommandSender.SendDemolishCommand(selectedTower, refundAmount);
        
        if (success)
        {
            Game.Logger.LogInformation("✅ Tower demolish command sent successfully");
        }
        else
        {
            Game.Logger.LogError("❌ Failed to send tower demolish command");
        }
        
        // 隐藏UI（因为塔被拆除）
        HideTowerUI();
    }

    /// <summary>
    /// 从单位获取塔类型
    /// </summary>
    private static TowerType GetTowerTypeFromUnit(Unit unit)
    {
        var unitName = unit.Cache?.Name ?? "";
        return unitName switch
        {
            "单体减速塔" => TowerType.SlowTower,
            "光环减速塔" => TowerType.AuraSlowTower,
            "群体伤害塔" => TowerType.AOETower,
            "向量穿透塔" => TowerType.PenetrateTower,
            _ => TowerType.SlowTower
        };
    }

    /// <summary>
    /// 获取塔的当前等级
    /// </summary>
    private static int GetTowerLevel(Unit tower)
    {
        try
        {
            var level = tower.GetProperty<int>(PropertyUnit.Level);
            return level;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 获取塔等级失败: {towerName}", tower.Cache?.Name ?? "Unknown");
            return 1; // 默认等级
        }
    }

    /// <summary>
    /// 获取塔的攻击力
    /// </summary>
    private static float GetTowerAttack(Unit tower)
    {
        try
        {
            // // 🎯 推荐方式：使用 UnitPropertyComplex 的 GetFinal 方法获取最终计算值
            // var unitPropertyComplex = tower.GetComponent<UnitPropertyComplex>();
            // if (unitPropertyComplex != null)
            // {
            //     // 使用 UnitProperty.AttackDamage 获取攻击伤害（最终计算值）
            //     var attackDamage = unitPropertyComplex.GetFinal(UnitProperty.AttackDamage);
            //     return (float)attackDamage;
            // }
            
            // // 备用方案：使用扩展方法直接获取
            var finalAttack = tower.GetUnitPropertyFinal(UnitProperty.AttackDamage);
            if (finalAttack.HasValue)
            {
                return (float)finalAttack.Value;
            }
            
            return 100f; // 默认攻击力
        }
        catch (Exception ex)
        {
            Game.Logger.LogWarning(ex, "⚠️ 获取塔攻击力失败，使用默认值: {towerName}", tower.Cache?.Name ?? "Unknown");
            return 100f; // 默认攻击力
        }
    }

    /// <summary>
    /// 刷新选中塔的UI显示
    /// </summary>
    public static void RefreshSelectedTowerUI()
    {
        try
        {
            if (selectedTower != null)
            {
                // 重新显示塔UI以刷新等级信息
                ShowTowerUI(selectedTower, new UIPosition(0, 0));
                Game.Logger.LogDebug("🔄 Refreshed selected tower UI");
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error refreshing selected tower UI");
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public static void Cleanup()
    {
        try
        {
            HideTowerUI();
            
            // 🔧 修复：正确清理Trigger引用
            if (mouseTrigger != null)
            {
                mouseTrigger.Destroy();
                mouseTrigger = null;
                Game.Logger.LogInformation("🗑️ Mouse trigger destroyed");
            }
            
            isInitialized = false;
            
            Game.Logger.LogInformation("🧹 TowerUIVisual cleaned up");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error cleaning up TowerUIVisual");
        }
    }
}
#endif
