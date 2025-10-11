using GameCore;
using GameCore.EntitySystem;
using GameCore.OrderSystem;
using GameCore.Leveling;
using GameEntry.TowerDefenseGame.TowerUpgradeSystem;
using GameEntry.TowerDefenseGame.ResourceSystem;

namespace GameEntry.TowerDefenseGame.TowerUISystem;

/// <summary>
/// 塔防命令类型枚举
/// </summary>
public enum TowerCommandType
{
    Upgrade = 1,    // 升级塔
    Demolish = 2    // 拆除塔
}

/// <summary>
/// 塔防命令数据
/// </summary>
public class TowerCommandData
{
    public TowerCommandType CommandType { get; set; }
    public int TowerId { get; set; }
    public int PlayerId { get; set; }
    public TowerType TowerType { get; set; }
    public int CurrentLevel { get; set; }
    public int Price { get; set; }
    public int RefundAmount { get; set; }
}

/// <summary>
/// 塔防命令处理器 - 处理塔相关的游戏命令
/// </summary>
public static class TowerCommandProcessor
{
    /// <summary>
    /// 处理塔升级命令
    /// </summary>
    public static bool ProcessUpgradeCommand(Unit tower, Player player, int upgradePrice)
    {
        try
        {
            if (tower == null || player == null)
            {
                Game.Logger.LogError("❌ Invalid tower or player for upgrade command");
                return false;
            }

            var towerType = GetTowerTypeFromUnit(tower);
            var currentLevel = GetTowerLevel(tower);
            
            // 验证升级条件
            if (!TowerUpgradeDataManager.CanUpgradeTower(towerType, currentLevel))
            {
                Game.Logger.LogError("❌ Tower cannot be upgraded: max level reached");
                return false;
            }

            var expectedPrice = TowerUpgradeDataManager.GetTowerUpgradePrice(towerType, currentLevel);
            if (upgradePrice != expectedPrice)
            {
                Game.Logger.LogError("❌ Upgrade price mismatch: expected {expected}, got {actual}", expectedPrice, upgradePrice);
                return false;
            }

            // 检查玩家金币
            if (!PlayerResourceManager.HasEnoughGold(player.Id, upgradePrice))
            {
                Game.Logger.LogError("❌ Player {playerId} doesn't have enough gold for upgrade: needs {price}, has {current}", 
                    player.Id, upgradePrice, PlayerResourceManager.GetPlayerGold(player.Id));
                return false;
            }

            // 扣除金币
            if (!PlayerResourceManager.DeductGold(player.Id, upgradePrice))
            {
                Game.Logger.LogError("❌ Failed to deduct gold from player {playerId}", player.Id);
                return false;
            }

            // 执行升级
            bool upgradeSuccess = UpgradeTowerLevel(tower, towerType, currentLevel + 1);
            
            if (upgradeSuccess)
            {
                Game.Logger.LogInformation("✅ Tower {towerName} upgraded from level {oldLevel} to {newLevel}", 
                    tower.Cache?.Name ?? "Unknown", currentLevel, currentLevel + 1);
                return true;
            }
            else
            {
                Game.Logger.LogError("❌ Failed to upgrade tower");
                return false;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error processing tower upgrade command");
            return false;
        }
    }

    /// <summary>
    /// 处理塔拆除命令
    /// </summary>
    public static bool ProcessDemolishCommand(Unit tower, Player player, int refundAmount)
    {
        try
        {
            if (tower == null || player == null)
            {
                Game.Logger.LogError("❌ Invalid tower or player for demolish command");
                return false;
            }

            var towerType = GetTowerTypeFromUnit(tower);
            var currentLevel = GetTowerLevel(tower);
            
            // 验证回收金额
            var expectedRefund = TowerUpgradeDataManager.GetTowerDemolishRefund(towerType, currentLevel);
            if (refundAmount != expectedRefund)
            {
                Game.Logger.LogError("❌ Refund amount mismatch: expected {expected}, got {actual}", expectedRefund, refundAmount);
                return false;
            }

            // 给玩家金币
            PlayerResourceManager.AddGold(player.Id, refundAmount);

            // 执行拆除
            bool demolishSuccess = DemolishTower(tower);
            
            if (demolishSuccess)
            {
                Game.Logger.LogInformation("✅ Tower {towerName} demolished, refunded {refund} gold", 
                    tower.Cache?.Name ?? "Unknown", refundAmount);
                return true;
            }
            else
            {
                Game.Logger.LogError("❌ Failed to demolish tower");
                return false;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error processing tower demolish command");
            return false;
        }
    }

    /// <summary>
    /// 升级塔的等级和属性
    /// </summary>
    private static bool UpgradeTowerLevel(Unit tower, TowerType towerType, int newLevel)
    {
        try
        {
            // 获取新等级的属性数据
            var levelStats = TowerUpgradeDataManager.GetTowerLevelStats(towerType, newLevel);
            if (levelStats == null)
            {
                Game.Logger.LogError("❌ Cannot get level stats for tower type {towerType} level {level}", towerType, newLevel);
                return false;
            }

            // 🔧 修复：设置塔的新等级到Unit的等级系统
            SetTowerLevel(tower, newLevel);

            // TODO: 更新塔的属性（需要正确的UnitProperty API）
            // 暂时只记录日志，等API可用时再实现
            Game.Logger.LogInformation("🔧 Tower upgraded: NewLevel={level}, Attack={attack}, Range={range}, Health={health}", 
                newLevel, levelStats.Attack, levelStats.AttackRange, levelStats.Health);

            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error upgrading tower level");
            return false;
        }
    }

    /// <summary>
    /// 拆除塔（客户端版本 - 仅处理本地数据，不删除单位）
    /// </summary>
    private static bool DemolishTower(Unit tower)
    {
        try
        {
            // 客户端只处理本地数据清理，不删除单位
            // 单位的删除应该由服务器端处理
            // 注意：等级数据现在存储在Unit本身，无需额外清理
            
            Game.Logger.LogInformation("🔧 Client-side tower data cleaned, waiting for server to remove unit");
            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error cleaning tower data on client");
            return false;
        }
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
    /// 设置塔的等级
    /// </summary>
    private static void SetTowerLevel(Unit tower, int level)
    {
        try
        {
#if SERVER
            // 🔧 直接使用UnitLeveling系统设置等级
            var unitLeveling = tower.GetComponent<UnitLeveling>();
            if (unitLeveling != null)
            {
                // 使用ForceSetLevel方法直接设置等级
                unitLeveling.ForceSetLevel(level);
                Game.Logger.LogInformation("✅ 塔等级已设置到UnitLeveling: {towerName} -> Level {level}", 
                    tower.Cache?.Name ?? "Unknown", level);
            }
            else
            {
                Game.Logger.LogError("❌ 塔 {towerName} 没有UnitLeveling组件，无法设置等级", tower.Cache?.Name ?? "Unknown");
            }
#endif  
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 设置塔等级失败: {towerName}", tower.Cache?.Name ?? "Unknown");
        }
    }
}
