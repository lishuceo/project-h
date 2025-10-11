#if SERVER
using GameCore;
using GameCore.EntitySystem;
using GameCore.OrderSystem;
using GameCore.EntitySystem.Data.Enum;
using GameCore.Leveling;
using GameEntry.TowerDefenseGame.TowerUpgradeSystem;


namespace GameEntry.TowerDefenseGame.TowerUISystem;

/// <summary>
/// 塔命令处理器 - 服务端处理塔相关命令
/// </summary>
public static class TowerCommandHandler
{
    /// <summary>
    /// 初始化命令处理器
    /// </summary>
    public static void Initialize()
    {
        // 注册自定义命令处理器
        // TODO: 根据实际的命令系统API注册处理器
        Game.Logger.LogInformation("🏰 TowerCommandHandler initialized");
    }

    /// <summary>
    /// 处理塔升级命令
    /// </summary>
    /// <param name="command">命令对象</param>
    /// <param name="tower">目标塔单位</param>
    /// <returns>命令处理结果</returns>
    public static CommandResult HandleUpgradeCommand(Command command, Unit tower)
    {
        try
        {
            if (tower == null)
            {
                return CommandResult.Failure("目标塔单位不存在");
            }

            var player = command.Player;
            if (player == null)
            {
                return CommandResult.Failure("玩家不存在");
            }

            // 暂时跳过UserData检查，直接使用默认值
            // TODO: 实现Command.UserData支持后再启用
            var towerType = GetTowerTypeFromUnit(tower);
            int upgradePrice = TowerUpgradeDataManager.GetTowerUpgradePrice(towerType, GetTowerLevel(tower));

            // 获取当前塔等级
            int currentLevel = GetTowerLevel(tower);
            
            // 检查是否可以升级
            if (!TowerUpgradeDataManager.CanUpgradeTower(towerType, currentLevel))
            {
                return CommandResult.Failure("塔已达到最高等级");
            }

            // 验证升级价格
            int expectedPrice = TowerUpgradeDataManager.GetTowerUpgradePrice(towerType, currentLevel);
            if (upgradePrice != expectedPrice)
            {
                return CommandResult.Failure($"升级价格不匹配，期望: {expectedPrice}, 实际: {upgradePrice}");
            }

            // 检查玩家金币是否足够
            if (!HasEnoughGold(player, upgradePrice))
            {
                return CommandResult.Failure("金币不足");
            }

            // 扣除金币
            if (!DeductGold(player, upgradePrice))
            {
                return CommandResult.Failure("扣除金币失败");
            }

            // 执行升级
            bool upgradeSuccess = UpgradeTower(tower, towerType, currentLevel + 1);
            
            if (upgradeSuccess)
            {
                Game.Logger.LogInformation("✅ Tower {towerName} upgraded from level {oldLevel} to {newLevel}", 
                    tower.Cache?.Name ?? "Unknown", currentLevel, currentLevel + 1);
                return CommandResult.Success();
            }
            else
            {
                // 升级失败，退还金币
                AddGold(player, upgradePrice);
                return CommandResult.Failure("塔升级失败");
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling tower upgrade command");
            return CommandResult.Failure("处理升级命令时发生错误");
        }
    }

    /// <summary>
    /// 处理塔拆除命令
    /// </summary>
    /// <param name="command">命令对象</param>
    /// <param name="tower">目标塔单位</param>
    /// <returns>命令处理结果</returns>
    public static CommandResult HandleDemolishCommand(Command command, Unit tower)
    {
        try
        {
            if (tower == null)
            {
                return CommandResult.Failure("目标塔单位不存在");
            }

            var player = command.Player;
            if (player == null)
            {
                return CommandResult.Failure("玩家不存在");
            }

            // 暂时跳过UserData检查，直接使用默认值
            // TODO: 实现Command.UserData支持后再启用
            var towerType = GetTowerTypeFromUnit(tower);
            int refundAmount = TowerUpgradeDataManager.GetTowerDemolishRefund(towerType, GetTowerLevel(tower));

            // 获取当前塔等级
            int currentLevel = GetTowerLevel(tower);
            
            // 验证回收金额
            int expectedRefund = TowerUpgradeDataManager.GetTowerDemolishRefund(towerType, currentLevel);
            if (refundAmount != expectedRefund)
            {
                return CommandResult.Failure($"回收金额不匹配，期望: {expectedRefund}, 实际: {refundAmount}");
            }

            // 给玩家返还金币
            AddGold(player, refundAmount);

            // 移除塔单位
            bool demolishSuccess = DemolishTower(tower);
            
            if (demolishSuccess)
            {
                Game.Logger.LogInformation("✅ Tower {towerName} demolished, refunded {refund} gold", 
                    tower.Cache?.Name ?? "Unknown", refundAmount);
                return CommandResult.Success();
            }
            else
            {
                // 拆除失败，扣回金币
                DeductGold(player, refundAmount);
                return CommandResult.Failure("塔拆除失败");
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling tower demolish command");
            return CommandResult.Failure("处理拆除命令时发生错误");
        }
    }

    /// <summary>
    /// 获取塔的当前等级
    /// </summary>
    private static int GetTowerLevel(Unit tower)
    {
        var unitLeveling = GameCore.Leveling.UnitLevelingExtension.GetComponent<GameCore.Leveling.UnitLeveling>(tower);
        if (unitLeveling == null)
        {
            Game.Logger.LogError("❌ 塔 {towerName} 没有UnitLeveling组件，无法获取等级", tower.Cache?.Name ?? "Unknown");
            return 1;
        }
        return unitLeveling.Level;
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
    /// 检查玩家是否有足够的金币
    /// </summary>
    private static bool HasEnoughGold(Player player, int amount)
    {
        // TODO: 实现金币检查逻辑，需要与资源系统集成
        // 暂时返回true
        return true;
    }

    /// <summary>
    /// 扣除玩家金币
    /// </summary>
    private static bool DeductGold(Player player, int amount)
    {
        // TODO: 实现金币扣除逻辑，需要与资源系统集成
        Game.Logger.LogInformation("💰 Deducted {amount} gold from player {playerId}", amount, player.Id);
        return true;
    }

    /// <summary>
    /// 给玩家增加金币
    /// </summary>
    private static void AddGold(Player player, int amount)
    {
        // TODO: 实现金币增加逻辑，需要与资源系统集成
        Game.Logger.LogInformation("💰 Added {amount} gold to player {playerId}", amount, player.Id);
    }

    /// <summary>
    /// 升级塔
    /// </summary>
    private static bool UpgradeTower(Unit tower, TowerType towerType, int newLevel)
    {
        try
        {
            // 🔧 使用UnitLeveling系统进行升级
            var unitLeveling = GameCore.Leveling.UnitLevelingExtension.GetComponent<GameCore.Leveling.UnitLeveling>(tower);
            if (unitLeveling == null)
            {
                Game.Logger.LogError("❌ 塔 {towerName} 没有UnitLeveling组件，无法升级", tower.Cache?.Name ?? "Unknown");
                return false;
            }

            // 使用UnitLeveling系统升级
            unitLeveling.ForceSetLevel(newLevel);
            
            Game.Logger.LogInformation("✅ 塔 {towerName} 已通过UnitLeveling升级到等级 {newLevel}", 
                tower.Cache?.Name ?? "Unknown", unitLeveling.Level);
            
            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error upgrading tower");
            return false;
        }
    }

    /// <summary>
    /// 拆除塔（服务器端版本 - 真正删除单位）
    /// </summary>
    private static bool DemolishTower(Unit tower)
    {
        try
        {
            // 服务器端负责真正删除单位
            // 注意：等级数据现在存储在Unit本身，无需额外清理
            // TowerLevelManager.RemoveTowerLevel(tower); // 不再需要
            
            // 删除单位
            tower.Kill(DeathType.Destroy);
            Game.Logger.LogInformation("💥 Tower unit removed from game by server");
            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error demolishing tower on server");
            return false;
        }
    }
}

/// <summary>
/// 命令处理结果
/// </summary>
public class CommandResult
{
    public bool IsSuccess { get; private set; }
    public string ErrorMessage { get; private set; } = "";

    private CommandResult(bool success, string errorMessage = "")
    {
        IsSuccess = success;
        ErrorMessage = errorMessage;
    }

    public static CommandResult Success() => new(true);
    public static CommandResult Failure(string errorMessage) => new(false, errorMessage);
}
#endif
