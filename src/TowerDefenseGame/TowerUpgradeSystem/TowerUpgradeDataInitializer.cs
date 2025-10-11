using GameCore;
using GameEntry.TowerDefenseGame.TowerUpgradeSystem;

namespace GameEntry.TowerDefenseGame.TowerUpgradeSystem;

/// <summary>
/// 塔防升级数据初始化器
/// </summary>
public class TowerUpgradeDataInitializer : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        InitializeTowerUpgradeConfigs();
        Game.Logger.LogInformation("🔧 Tower upgrade data initialized");
    }

    /// <summary>
    /// 初始化各种塔的升级配置
    /// </summary>
    private static void InitializeTowerUpgradeConfigs()
    {
        // 1. 减速塔升级配置
        _ = new GameDataTowerUpgradeConfig(new GameData.GameLink<GameDataTowerUpgrade, GameDataTowerUpgradeConfig>("SlowTowerUpgrade"u8))
        {
            Name = "减速塔升级配置",
            DisplayName = "减速塔升级配置",
            Description = "单体减速塔的升级数据配置",
            MaxLevel = 3,
            BaseUpgradePrice = 50, // 建造成本：50金币
            PriceMultiplier = 1.8f,
            DemolishRefundRatio = 0.5f,
            TowerType = TowerType.SlowTower,
            
            // 各等级升级价格
            LevelUpgradePrices = new Dictionary<int, int>
            {
                { 2, 50 },   // 1->2级: 50金币
                { 3, 90 }    // 2->3级: 90金币
            },
            
            // 各等级属性加成
            LevelStats = new Dictionary<int, TowerLevelStats>
            {
                { 1, new TowerLevelStats { Attack = 100, AttackRange = 400, AttackSpeed = 1.0f, Health = 800, EffectPower = 0.3f } },
                { 2, new TowerLevelStats { Attack = 150, AttackRange = 450, AttackSpeed = 1.2f, Health = 1000, EffectPower = 0.4f } },
                { 3, new TowerLevelStats { Attack = 220, AttackRange = 500, AttackSpeed = 1.5f, Health = 1300, EffectPower = 0.5f } }
            },
            
            // 各等级显示名称
            LevelNames = new Dictionary<int, string>
            {
                { 1, "基础减速塔" },
                { 2, "强化减速塔" },
                { 3, "精英减速塔" }
            },
            
            // 各等级描述
            LevelDescriptions = new Dictionary<int, string>
            {
                { 1, "发射减速投射物，降低敌人移动速度30%" },
                { 2, "增强的减速效果，降低敌人移动速度40%" },
                { 3, "精英级减速塔，降低敌人移动速度50%" }
            }
        };

        // 2. 光环减速塔升级配置
        _ = new GameDataTowerUpgradeConfig(new GameData.GameLink<GameDataTowerUpgrade, GameDataTowerUpgradeConfig>("AuraSlowTowerUpgrade"u8))
        {
            Name = "光环塔升级配置",
            DisplayName = "光环塔升级配置",
            Description = "光环减速塔的升级数据配置",
            MaxLevel = 3,
            BaseUpgradePrice = 60, // 建造成本：60金币
            PriceMultiplier = 1.9f,
            DemolishRefundRatio = 0.5f,
            TowerType = TowerType.AuraSlowTower,
            
            LevelUpgradePrices = new Dictionary<int, int>
            {
                { 2, 60 },   // 1->2级: 60金币
                { 3, 110 }   // 2->3级: 110金币
            },
            
            LevelStats = new Dictionary<int, TowerLevelStats>
            {
                { 1, new TowerLevelStats { Attack = 0, AttackRange = 300, AttackSpeed = 0, Health = 700, EffectPower = 0.25f, EffectRange = 300 } },
                { 2, new TowerLevelStats { Attack = 0, AttackRange = 350, AttackSpeed = 0, Health = 900, EffectPower = 0.35f, EffectRange = 350 } },
                { 3, new TowerLevelStats { Attack = 0, AttackRange = 400, AttackSpeed = 0, Health = 1200, EffectPower = 0.45f, EffectRange = 400 } }
            },
            
            LevelNames = new Dictionary<int, string>
            {
                { 1, "基础光环塔" },
                { 2, "强化光环塔" },
                { 3, "精英光环塔" }
            },
            
            LevelDescriptions = new Dictionary<int, string>
            {
                { 1, "释放减速光环，影响范围内所有敌人，减速25%" },
                { 2, "增强光环效果，影响范围扩大，减速35%" },
                { 3, "精英级光环塔，最大范围减速45%" }
            }
        };

        // 3. 爆炸塔升级配置
        _ = new GameDataTowerUpgradeConfig(new GameData.GameLink<GameDataTowerUpgrade, GameDataTowerUpgradeConfig>("AOETowerUpgrade"u8))
        {
            Name = "爆炸塔升级配置",
            DisplayName = "爆炸塔升级配置",
            Description = "群体伤害塔的升级数据配置",
            MaxLevel = 3,
            BaseUpgradePrice = 80, // 建造成本：80金币
            PriceMultiplier = 2.0f,
            DemolishRefundRatio = 0.5f,
            TowerType = TowerType.AOETower,
            
            LevelUpgradePrices = new Dictionary<int, int>
            {
                { 2, 80 },   // 1->2级: 80金币
                { 3, 160 }   // 2->3级: 160金币
            },
            
            LevelStats = new Dictionary<int, TowerLevelStats>
            {
                { 1, new TowerLevelStats { Attack = 200, AttackRange = 250, AttackSpeed = 0.8f, Health = 600, EffectPower = 150, EffectRange = 120 } },
                { 2, new TowerLevelStats { Attack = 300, AttackRange = 280, AttackSpeed = 1.0f, Health = 800, EffectPower = 220, EffectRange = 150 } },
                { 3, new TowerLevelStats { Attack = 450, AttackRange = 320, AttackSpeed = 1.3f, Health = 1100, EffectPower = 320, EffectRange = 180 } }
            },
            
            LevelNames = new Dictionary<int, string>
            {
                { 1, "基础爆炸塔" },
                { 2, "强化爆炸塔" },
                { 3, "精英爆炸塔" }
            },
            
            LevelDescriptions = new Dictionary<int, string>
            {
                { 1, "发射爆炸弹药，造成范围伤害" },
                { 2, "增强爆炸威力和范围" },
                { 3, "精英级爆炸塔，最大威力群体攻击" }
            }
        };

        // 4. 穿透塔升级配置
        _ = new GameDataTowerUpgradeConfig(new GameData.GameLink<GameDataTowerUpgrade, GameDataTowerUpgradeConfig>("PenetrateTowerUpgrade"u8))
        {
            Name = "穿透塔升级配置",
            DisplayName = "穿透塔升级配置",
            Description = "向量穿透塔的升级数据配置",
            MaxLevel = 3,
            BaseUpgradePrice = 100, // 建造成本：100金币
            PriceMultiplier = 2.2f,
            DemolishRefundRatio = 0.5f,
            TowerType = TowerType.PenetrateTower,
            
            LevelUpgradePrices = new Dictionary<int, int>
            {
                { 2, 100 },  // 1->2级: 100金币
                { 3, 220 }   // 2->3级: 220金币
            },
            
            LevelStats = new Dictionary<int, TowerLevelStats>
            {
                { 1, new TowerLevelStats { Attack = 300, AttackRange = 3000, AttackSpeed = 0.6f, Health = 900, EffectPower = 3 } }, // EffectPower = 穿透数量
                { 2, new TowerLevelStats { Attack = 450, AttackRange = 3500, AttackSpeed = 0.8f, Health = 1200, EffectPower = 5 } },
                { 3, new TowerLevelStats { Attack = 650, AttackRange = 4000, AttackSpeed = 1.0f, Health = 1600, EffectPower = 8 } }
            },
            
            LevelNames = new Dictionary<int, string>
            {
                { 1, "基础穿透塔" },
                { 2, "强化穿透塔" },
                { 3, "精英穿透塔" }
            },
            
            LevelDescriptions = new Dictionary<int, string>
            {
                { 1, "发射穿透攻击，可穿透3个敌人" },
                { 2, "增强穿透能力，可穿透5个敌人" },
                { 3, "精英级穿透塔，可穿透8个敌人" }
            }
        };
    }
}
