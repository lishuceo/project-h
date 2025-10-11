using GameCore.Extension;
using GameCore.Localization;
using GameData;
using System.Collections.Generic;

namespace GameEntry.TowerDefenseGame.TowerUpgradeSystem;

/// <summary>
/// 塔防升级数据基础类别 - 使用GameDataCategory特性
/// </summary>
[GameDataCategory]
public abstract partial class GameDataTowerUpgrade
{
    /// <summary>升级配置名称</summary>
    public new string Name { get; set; } = "";
    
    /// <summary>显示名称</summary>
    public LocalizedString? DisplayName { get; set; }
    
    /// <summary>描述</summary>
    public string Description { get; set; } = "";
    
    /// <summary>最大等级</summary>
    public int MaxLevel { get; set; } = 3;
    
    /// <summary>基础升级价格</summary>
    public int BaseUpgradePrice { get; set; } = 50;
    
    /// <summary>价格增长系数</summary>
    public float PriceMultiplier { get; set; } = 1.5f;
    
    /// <summary>拆除回收比例 (0.0 - 1.0)</summary>
    public float DemolishRefundRatio { get; set; } = 0.5f;
}

/// <summary>
/// 塔升级配置数据 - 使用GameDataNodeType特性
/// </summary>
[GameDataNodeType<GameDataTowerUpgrade, GameDataTowerUpgrade>]
public partial class GameDataTowerUpgradeConfig
{
    /// <summary>塔类型</summary>
    public TowerType TowerType { get; set; }
    
    /// <summary>各等级的升级价格配置</summary>
    public Dictionary<int, int> LevelUpgradePrices { get; set; } = new();
    
    /// <summary>各等级的属性加成配置</summary>
    public Dictionary<int, TowerLevelStats> LevelStats { get; set; } = new();
    
    /// <summary>各等级的显示名称</summary>
    public Dictionary<int, string> LevelNames { get; set; } = new();
    
    /// <summary>各等级的描述</summary>
    public Dictionary<int, string> LevelDescriptions { get; set; } = new();
    
    /// <summary>获取指定等级的升级价格</summary>
    public int GetUpgradePrice(int currentLevel)
    {
        if (currentLevel >= MaxLevel) return 0;
        
        int targetLevel = currentLevel + 1;
        if (LevelUpgradePrices.TryGetValue(targetLevel, out int price))
        {
            return price;
        }
        
        // 使用基础价格和增长系数计算
        return (int)(BaseUpgradePrice * Math.Pow(PriceMultiplier, currentLevel));
    }
    
    /// <summary>获取指定等级的属性加成</summary>
    public TowerLevelStats? GetLevelStats(int level)
    {
        LevelStats.TryGetValue(level, out var stats);
        return stats;
    }
    
    /// <summary>获取拆除回收金额 - 总投入成本的一半向下取整</summary>
    public int GetDemolishRefund(int currentLevel)
    {
        // 计算总投入成本：建造成本 + 所有升级成本
        int totalInvestment = BaseUpgradePrice; // 建造成本（1级的成本）
        
        // 加上所有升级成本
        for (int i = 1; i < currentLevel; i++)
        {
            totalInvestment += GetUpgradePrice(i);
        }
        
        // 总投入的一半，向下取整
        return (int)Math.Floor(totalInvestment * 0.5);
    }
}

/// <summary>
/// 塔等级属性数据
/// </summary>
public class TowerLevelStats
{
    /// <summary>攻击力</summary>
    public float Attack { get; set; } = 0;
    
    /// <summary>攻击范围</summary>
    public float AttackRange { get; set; } = 0;
    
    /// <summary>攻击速度</summary>
    public float AttackSpeed { get; set; } = 0;
    
    /// <summary>生命值</summary>
    public float Health { get; set; } = 0;
    
    /// <summary>特殊效果强度（如减速强度、爆炸伤害等）</summary>
    public float EffectPower { get; set; } = 0;
    
    /// <summary>特殊效果范围</summary>
    public float EffectRange { get; set; } = 0;
}

/// <summary>
/// 塔升级数据管理器 - 提供数据访问接口
/// </summary>
public static class TowerUpgradeDataManager
{
    /// <summary>
    /// 根据塔类型获取升级配置
    /// </summary>
    public static GameDataTowerUpgradeConfig? GetUpgradeConfig(TowerType towerType)
    {
        return GameDataCategory<GameDataTowerUpgrade>.Catalog
            .OfType<GameDataTowerUpgradeConfig>()
            .FirstOrDefault(config => config.TowerType == towerType);
    }
    
    /// <summary>
    /// 获取塔的升级价格
    /// </summary>
    public static int GetTowerUpgradePrice(TowerType towerType, int currentLevel)
    {
        var config = GetUpgradeConfig(towerType);
        return config?.GetUpgradePrice(currentLevel) ?? 0;
    }
    
    /// <summary>
    /// 获取塔的拆除回收金额
    /// </summary>
    public static int GetTowerDemolishRefund(TowerType towerType, int currentLevel)
    {
        var config = GetUpgradeConfig(towerType);
        return config?.GetDemolishRefund(currentLevel) ?? 0;
    }
    
    /// <summary>
    /// 获取塔的等级属性
    /// </summary>
    public static TowerLevelStats? GetTowerLevelStats(TowerType towerType, int level)
    {
        var config = GetUpgradeConfig(towerType);
        return config?.GetLevelStats(level);
    }
    
    /// <summary>
    /// 检查塔是否可以升级
    /// </summary>
    public static bool CanUpgradeTower(TowerType towerType, int currentLevel)
    {
        var config = GetUpgradeConfig(towerType);
        return config != null && currentLevel < config.MaxLevel;
    }
}
