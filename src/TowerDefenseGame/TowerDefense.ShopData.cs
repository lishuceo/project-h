using GameCore.ItemSystem.Data;
using GameCore.AbilitySystem.Data;
using GameCore.Struct;
using GameCore.Localization;
using GameCore.ResourceType;
using GameData;

namespace GameEntry.TowerDefenseGame;

/// <summary>
/// 塔防商店数据系统 - 定义商店物品的数据驱动配置
/// </summary>
public class ShopData : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    // 商店物品链接定义
    public static class ShopItem
    {
        // 四种塔的蓝图物品
        public static readonly GameLink<GameDataShopItem, GameDataShopItemTower> SlowTowerBlueprint = new("SlowTowerBlueprint"u8);
        public static readonly GameLink<GameDataShopItem, GameDataShopItemTower> AuraSlowTowerBlueprint = new("AuraSlowTowerBlueprint"u8);
        public static readonly GameLink<GameDataShopItem, GameDataShopItemTower> AOETowerBlueprint = new("AOETowerBlueprint"u8);
        public static readonly GameLink<GameDataShopItem, GameDataShopItemTower> PenetrateTowerBlueprint = new("PenetrateTowerBlueprint"u8);
    }

    private static void OnGameDataInitialization()
    {
        // 创建商店物品数据
        CreateShopItems();
    }

    private static void CreateShopItems()
    {
        // 1. 减速塔蓝图
        _ = new GameDataShopItemTower(ShopItem.SlowTowerBlueprint)
        {
            Name = "减速塔蓝图",
            DisplayName = "减速塔蓝图",
            Description = "建造一座发射减速投射物的防御塔",
            Price = 1,
            Icon = "image/item/tower_slow.png",
            TowerType = TowerType.SlowTower,
            BuildAbility = GameEntry.ScopeData.Ability.BuildSlowTower,
            ItemLink = GameEntry.ScopeData.Item.SlowTowerItem
        };

        // 2. 光环减速塔蓝图
        _ = new GameDataShopItemTower(ShopItem.AuraSlowTowerBlueprint)
        {
            Name = "光环塔蓝图",
            DisplayName = "光环塔蓝图", 
            Description = "建造一座释放减速光环的防御塔",
            Price = 1,
            Icon = "image/item/tower_aura.png",
            TowerType = TowerType.AuraSlowTower,
            BuildAbility = GameEntry.ScopeData.Ability.BuildAuraSlowTower,
            ItemLink = GameEntry.ScopeData.Item.AuraSlowTowerItem
        };

        // 3. 爆炸塔蓝图
        _ = new GameDataShopItemTower(ShopItem.AOETowerBlueprint)
        {
            Name = "爆炸塔蓝图",
            DisplayName = "爆炸塔蓝图",
            Description = "建造一座造成群体伤害的防御塔",
            Price = 1,
            Icon = "image/item/tower_aoe.png",
            TowerType = TowerType.AOETower,
            BuildAbility = GameEntry.ScopeData.Ability.BuildAOETower,
            ItemLink = GameEntry.ScopeData.Item.AOETowerItem
        };

        // 4. 穿透塔蓝图
        _ = new GameDataShopItemTower(ShopItem.PenetrateTowerBlueprint)
        {
            Name = "穿透塔蓝图",
            DisplayName = "穿透塔蓝图",
            Description = "建造一座发射穿透攻击的防御塔",
            Price = 1,
            Icon = "image/item/tower_penetrate.png",
            TowerType = TowerType.PenetrateTower,
            BuildAbility = GameEntry.ScopeData.Ability.BuildPenetrateTower,
            ItemLink = GameEntry.ScopeData.Item.PenetrateTowerItem
        };
    }
}

/// <summary>
/// 塔类型枚举
/// </summary>
public enum TowerType
{
    SlowTower = 1,      // 减速塔
    AuraSlowTower = 2,  // 光环减速塔
    AOETower = 3,       // 爆炸塔
    PenetrateTower = 4  // 穿透塔
}

/// <summary>
/// 商店物品基础数据类别
/// </summary>
[GameDataCategory]
public abstract partial class GameDataShopItem
{
    /// <summary>物品名称</summary>
    public new string Name { get; set; } = "";
    
    /// <summary>显示名称</summary>
    public LocalizedString? DisplayName { get; set; }
    
    /// <summary>物品描述</summary>
    public string Description { get; set; } = "";
    
    /// <summary>价格</summary>
    public int Price { get; set; }
    
    /// <summary>图标</summary>
    public Icon Icon { get; set; } = "";
    
    /// <summary>是否可用</summary>
    public bool IsAvailable { get; set; } = true;
    
    /// <summary>获取物品ID - 从GameLink的HashCodeLocal获取</summary>
    public int ItemId => Link.HashCodeLocal;
}

/// <summary>
/// 塔蓝图商店物品数据
/// </summary>
[GameDataNodeType<GameDataShopItem, GameDataShopItem>]
public partial class GameDataShopItemTower
{
    /// <summary>塔类型</summary>
    public TowerType TowerType { get; set; }
    
    /// <summary>关联的建造技能</summary>
    public IGameLink<GameDataAbility>? BuildAbility { get; set; }
    
    /// <summary>关联的物品链接（用于兼容现有系统）</summary>
    public IGameLink<GameDataItem>? ItemLink { get; set; }
    
    /// <summary>建造消耗的资源</summary>
    public int BuildCost { get; set; } = 0;
    
    /// <summary>解锁等级要求</summary>
    public int RequiredLevel { get; set; } = 1;
}
