using EngineInterface.BaseType;
using GameCore.SceneSystem;
using GameCore.SceneSystem.Data;
using GameCore.SceneSystem.Data.Struct;
using GameCore.ActorSystem.Data;
using GameCore.ActorSystem.Data.Enum;
using GameCore.Container;
using GameCore.Container.Data;
using GameCore.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameData;
using System.Numerics;
using static GameCore.ScopeData;
using GameCore.Struct;
using GameData.Extension;
using GameCore;

namespace GameEntry.ARPGTemplate.ScopeData;

/// <summary>
/// ARPG物品分类扩展 📦
/// 使用EnumExtension机制扩展ItemCategory
/// 职业专属武器 + 通用护甲饰品分类 + 消耗品和任务道具
/// </summary>
[EnumExtension(Extends = typeof(ItemCategory))]
public enum EARPGItemCategory
{
    // 职业专属武器分类 - 从1000开始，避免与系统预定义冲突
    WeaponSword = 1001,   // 剑类武器 - 剑客专用
    WeaponGun = 1002,     // 手枪/火枪类武器 - 枪手专用  
    WeaponStaff = 1003,   // 法杖类武器 - 法师专用
    WeaponHammer = 1004,  // 锤子类武器 - 战士专用
    
    // 通用装备分类
    Armor = 1010,         // 护甲类 (包含头盔/胸甲/靴子/手套)
    Accessory = 1011,     // 饰品类 (包含项链/戒指)
    
    // 消耗品分类
    HealthPotion = 1020,  // 恢复道具 - 恢复生命值
    BuffPotion = 1021,    // 增益道具 - 提供临时buff
    
    // 任务道具分类
    QuestItem = 1030,     // 任务道具 - 无功能，仅用于任务
}

/// <summary>
/// 物品品质枚举 🌟
/// </summary>
public enum ItemQuality
{
    Common = 1,      // 普通（白色）
    Rare = 2,        // 稀有（蓝色）
    Epic = 3,        // 史诗（紫色）
    Legendary = 4    // 传说（橙色）
}

/// <summary>
/// 物品类型枚举 ⚔️
/// </summary>
public enum ItemType
{
    // 武器类
    Sword = 1,       // 剑
    Gun = 2,         // 手枪/火枪
    Staff = 3,       // 法杖
    Hammer = 4,      // 锤子
    
    // 护甲类
    Helmet = 10,     // 头盔
    Armor = 11,      // 护甲
    Boots = 12,      // 靴子
    Gloves = 13,     // 手套
    
    // 饰品类
    Necklace = 20,   // 项链
    Ring = 21,       // 戒指
    
    // 消耗品类
    HealthPotion = 30, // 恢复药剂
    BuffPotion = 31,   // 增益药剂
    
    // 任务道具类
    QuestItem = 40     // 任务道具
}

/// <summary>
/// 职业类型枚举 👤
/// </summary>
public enum HeroClass
{
    Swordsman = 1,   // 剑客
    Gunner = 2,      // 枪手
    Mage = 3,        // 法师
    Warrior = 4      // 战士
}

/// <summary>
/// 装备槽位枚举 🎒
/// </summary>
public enum EquipmentSlot
{
    Weapon = 0,      // 武器槽
    Armor = 1,       // 护甲槽
    Accessory = 2    // 饰品槽
}





/// <summary>
/// ARPG物品系统数据配置
/// 包含武器、装备、消耗品等物品配置
/// </summary>
public class ARPGItems : IGameClass
{
    #region 职业限制辅助方法 🚫
    /// <summary>
    /// 获取物品允许使用的职业列表
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns>允许使用该物品的职业列表，null表示所有职业都可以使用</returns>
    public static HeroClass[]? GetAllowedClasses(ItemType itemType)
    {
        return itemType switch
        {
            // 武器类职业限制
            ItemType.Sword => [HeroClass.Swordsman],  // 剑只能剑客用
            ItemType.Gun => [HeroClass.Gunner],       // 手枪只能枪手用  
            ItemType.Staff => [HeroClass.Mage],       // 法杖只能法师用
            ItemType.Hammer => [HeroClass.Warrior],   // 锤子只能战士用
            
            // 护甲类所有职业都能用
            ItemType.Helmet or ItemType.Armor or ItemType.Boots or ItemType.Gloves => null,
            
            // 饰品类所有职业都能用
            ItemType.Necklace or ItemType.Ring => null,
            
            _ => null // 默认所有职业都能用
        };
    }
    
    /// <summary>
    /// 检查指定职业是否可以使用该物品类型
    /// </summary>
    /// <param name="heroClass">英雄职业</param>
    /// <param name="itemType">物品类型</param>
    /// <returns>true表示可以使用，false表示不能使用</returns>
    public static bool CanUseItem(HeroClass heroClass, ItemType itemType)
    {
        var allowedClasses = GetAllowedClasses(itemType);
        return allowedClasses == null || allowedClasses.Contains(heroClass);
    }
    
    /// <summary>
    /// 获取物品应该装备在哪个槽位
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns>装备槽位，null表示无法装备</returns>
    public static EquipmentSlot? GetEquipmentSlot(ItemType itemType)
    {
        return itemType switch
        {
            // 武器类装备在武器槽
            ItemType.Sword or ItemType.Gun or ItemType.Staff or ItemType.Hammer => EquipmentSlot.Weapon,
            
            // 护甲类装备在护甲槽 (这里简化处理，所有护甲都装备在护甲槽)
            ItemType.Helmet or ItemType.Armor or ItemType.Boots or ItemType.Gloves => EquipmentSlot.Armor,
            
            // 饰品类装备在饰品槽
            ItemType.Necklace or ItemType.Ring => EquipmentSlot.Accessory,
            
            // 消耗品和任务道具无法装备
            ItemType.HealthPotion or ItemType.BuffPotion or ItemType.QuestItem => null,
            
            _ => null // 不能装备
        };
    }
    
    /// <summary>
    /// 获取物品的分类类别 📦
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns>物品分类，编译后会生成ARPGItemCategory静态类</returns>
    public static ItemCategory GetItemCategory(ItemType itemType)
    {
        // 注意：编译时会生成ARPGItemCategory静态类，包含扩展的ItemCategory值
        return itemType switch
        {
            // 武器分类映射 - 按职业专属分类
            ItemType.Sword => default,   // ARPGItemCategory.WeaponSword
            ItemType.Gun => default,     // ARPGItemCategory.WeaponGun
            ItemType.Staff => default,   // ARPGItemCategory.WeaponStaff
            ItemType.Hammer => default,  // ARPGItemCategory.WeaponHammer
            
            // 护甲分类映射 - 所有护甲都归为Armor类
            ItemType.Helmet or ItemType.Armor or ItemType.Boots or ItemType.Gloves 
                => default, // ARPGItemCategory.Armor
            
            // 饰品分类映射 - 所有饰品都归为Accessory类
            ItemType.Necklace or ItemType.Ring 
                => default, // ARPGItemCategory.Accessory
            
            // 消耗品分类映射
            ItemType.HealthPotion => default, // ARPGItemCategory.HealthPotion
            ItemType.BuffPotion => default,   // ARPGItemCategory.BuffPotion
            
            // 任务道具分类映射
            ItemType.QuestItem => default,    // ARPGItemCategory.QuestItem
            
            _ => default // 默认值，编译后替换为具体分类
        };
    }
    #endregion
    #region 物品定义
    public static class Item
    {
        #region 武器类 ⚔️
        // 剑类 - 剑客专用
        public static readonly GameLink<GameDataItem, GameDataItemMod> IronSword = new("IronSword"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> DragonSword = new("DragonSword"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> LegendarySword = new("LegendarySword"u8);
        
        // 手枪类 - 枪手专用
        public static readonly GameLink<GameDataItem, GameDataItemMod> BasicPistol = new("BasicPistol"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> EpicPistol = new("EpicPistol"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> LegendaryPistol = new("LegendaryPistol"u8);
        
        // 法杖类 - 法师专用
        public static readonly GameLink<GameDataItem, GameDataItemMod> WoodenStaff = new("WoodenStaff"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> CrystalStaff = new("CrystalStaff"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> LegendaryStaff = new("LegendaryStaff"u8);
        
        // 锤子类 - 战士专用
        public static readonly GameLink<GameDataItem, GameDataItemMod> IronHammer = new("IronHammer"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> DragonHammer = new("DragonHammer"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> LegendaryHammer = new("LegendaryHammer"u8);
        #endregion

        #region 护甲类 🛡️
        // 头盔
        public static readonly GameLink<GameDataItem, GameDataItemMod> LeatherHelmet = new("LeatherHelmet"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> IronHelmet = new("IronHelmet"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> DragonHelmet = new("DragonHelmet"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> LegendaryHelmet = new("LegendaryHelmet"u8);
        
        // 护甲
        public static readonly GameLink<GameDataItem, GameDataItemMod> LeatherArmor = new("LeatherArmor"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> IronArmor = new("IronArmor"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> DragonArmor = new("DragonArmor"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> LegendaryArmor = new("LegendaryArmor"u8);
        
        // 靴子
        public static readonly GameLink<GameDataItem, GameDataItemMod> LeatherBoots = new("LeatherBoots"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> IronBoots = new("IronBoots"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> DragonBoots = new("DragonBoots"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> LegendaryBoots = new("LegendaryBoots"u8);
        
        // 手套
        public static readonly GameLink<GameDataItem, GameDataItemMod> LeatherGloves = new("LeatherGloves"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> IronGloves = new("IronGloves"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> DragonGloves = new("DragonGloves"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> LegendaryGloves = new("LegendaryGloves"u8);
        #endregion

        #region 饰品类 💎
        // 项链
        public static readonly GameLink<GameDataItem, GameDataItemMod> IronNecklace = new("IronNecklace"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> GoldNecklace = new("GoldNecklace"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> DiamondNecklace = new("DiamondNecklace"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> LegendaryNecklace = new("LegendaryNecklace"u8);
        
        // 戒指
        public static readonly GameLink<GameDataItem, GameDataItemMod> IronRing = new("IronRing"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> GoldRing = new("GoldRing"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> DiamondRing = new("DiamondRing"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> LegendaryRing = new("LegendaryRing"u8);
        #endregion

        #region 消耗品类 🧪
        // 恢复道具 - 可主动使用，消耗品
        public static readonly GameLink<GameDataItem, GameDataItemMod> HealthPotion = new("HealthPotion"u8);
        
        // 增益道具 - 提供临时buff，消耗品
        public static readonly GameLink<GameDataItem, GameDataItemMod> StrengthPotion = new("StrengthPotion"u8);
        #endregion

        #region 任务道具类 📋
        // 任务道具 - 无功能，仅用于任务
        public static readonly GameLink<GameDataItem, GameDataItemMod> MysteriousOrb = new("MysteriousOrb"u8);
        #endregion
    }
    #endregion

    #region 单位定义（物品的物理表现）
    public static class Unit
    {
        // 武器单位
        public static readonly GameLink<GameDataUnit, GameDataUnit> IronSwordUnit = new("IronSwordUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> DragonSwordUnit = new("DragonSwordUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> LegendarySwordUnit = new("LegendarySwordUnit"u8);
        
        public static readonly GameLink<GameDataUnit, GameDataUnit> BasicPistolUnit = new("BasicPistolUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> EpicPistolUnit = new("EpicPistolUnit"u8);
        
        public static readonly GameLink<GameDataUnit, GameDataUnit> WoodenStaffUnit = new("WoodenStaffUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> CrystalStaffUnit = new("CrystalStaffUnit"u8);
        
        public static readonly GameLink<GameDataUnit, GameDataUnit> IronHammerUnit = new("IronHammerUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> DragonHammerUnit = new("DragonHammerUnit"u8);
        
        // 护甲单位
        public static readonly GameLink<GameDataUnit, GameDataUnit> LeatherArmorUnit = new("LeatherArmorUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> DragonArmorUnit = new("DragonArmorUnit"u8);
        
        // 饰品单位
        public static readonly GameLink<GameDataUnit, GameDataUnit> GoldNecklaceUnit = new("GoldNecklaceUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> DiamondRingUnit = new("DiamondRingUnit"u8);
        
        // 消耗品单位
        public static readonly GameLink<GameDataUnit, GameDataUnit> HealthPotionUnit = new("HealthPotionUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> StrengthPotionUnit = new("StrengthPotionUnit"u8);
        
        // 任务道具单位
        public static readonly GameLink<GameDataUnit, GameDataUnit> MysteriousOrbUnit = new("MysteriousOrbUnit"u8);
    }
    #endregion

    #region 模型定义
    public static class Model
    {
        // 武器模型
        public static readonly GameLink<GameDataModel, GameDataModel> IronSwordModel = new("IronSwordModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> DragonSwordModel = new("DragonSwordModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> LegendarySwordModel = new("LegendarySwordModel"u8);
        
        public static readonly GameLink<GameDataModel, GameDataModel> BasicPistolModel = new("BasicPistolModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> EpicPistolModel = new("EpicPistolModel"u8);
        
        public static readonly GameLink<GameDataModel, GameDataModel> WoodenStaffModel = new("WoodenStaffModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> CrystalStaffModel = new("CrystalStaffModel"u8);
        
        public static readonly GameLink<GameDataModel, GameDataModel> IronHammerModel = new("IronHammerModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> DragonHammerModel = new("DragonHammerModel"u8);
        
        // 护甲模型
        public static readonly GameLink<GameDataModel, GameDataModel> LeatherArmorModel = new("LeatherArmorModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> DragonArmorModel = new("DragonArmorModel"u8);
        
        // 饰品模型
        public static readonly GameLink<GameDataModel, GameDataModel> GoldNecklaceModel = new("GoldNecklaceModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> DiamondRingModel = new("DiamondRingModel"u8);
        
        // 消耗品模型
        public static readonly GameLink<GameDataModel, GameDataModel> HealthPotionModel = new("HealthPotionModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> StrengthPotionModel = new("StrengthPotionModel"u8);
        
        // 任务道具模型
        public static readonly GameLink<GameDataModel, GameDataModel> MysteriousOrbModel = new("MysteriousOrbModel"u8);
    }
    #endregion

    #region Actor定义
    public static class Actor
    {
        // 剑类Actor
        public static readonly GameLink<GameDataActor, GameDataActorAdditionModel> SwordActor = new("SwordActor"u8);
        
        // 手枪类Actor  
        public static readonly GameLink<GameDataActor, GameDataActorAdditionModel> PistolActor = new("PistolActor"u8);
        
        // 法杖类Actor
        public static readonly GameLink<GameDataActor, GameDataActorAdditionModel> StaffActor = new("StaffActor"u8);
        
        // 锤子类Actor
        public static readonly GameLink<GameDataActor, GameDataActorAdditionModel> HammerActor = new("HammerActor"u8);
        
        // 兼容旧的名称
        public static readonly GameLink<GameDataActor, GameDataActorAdditionModel> DragonSwordActor = SwordActor;
    }
    #endregion

    // 粒子效果定义已移除，不再需要Glow相关的粒子特效

    public static void OnRegisterGameClass()
    {
        // 先移除可能已存在的订阅，避免重复注册
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    /// <summary>
    /// 初始化ARPG物品系统数据
    /// </summary>
    private static void OnGameDataInitialization()
    {
        // 只在ARPG模式下初始化
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.ARPGMode)
        {
            return;
        }

        Game.Logger.LogInformation("⚔️ Initializing ARPG Items System for ARPG Mode...");

        // 创建模型
        CreateItemModels();
        
        // 创建单位
        CreateItemUnits();
        
        // 创建物品
        CreateItems();

        // 粒子效果配置已移除，不再需要Glow相关的特效

        // ========== Actor配置 ==========
        // 剑类Actor
        _ = new GameDataActorAdditionModel(Actor.SwordActor)
        {
            Name = "剑类武器Actor",
            Model = Model.DragonSwordModel, // 使用龙纹剑模型作为基础
        };



        // 手枪类Actor
        _ = new GameDataActorAdditionModel(Actor.PistolActor)
        {
            Name = "手枪类武器Actor",
            Model = Model.BasicPistolModel,
        };



        // 法杖类Actor
        _ = new GameDataActorAdditionModel(Actor.StaffActor)
        {
            Name = "法杖类武器Actor",
            Model = Model.WoodenStaffModel,
        };



        // 锤子类Actor
        _ = new GameDataActorAdditionModel(Actor.HammerActor)
        {
            Name = "锤子类武器Actor",
            Model = Model.IronHammerModel,
        };



        Game.Logger.LogInformation("✅ ARPG Items System initialized successfully for ARPG Mode!");
        Game.Logger.LogInformation("   - 🗡️ 武器系统: 剑/手枪/法杖/锤子 × 多品质 + ActorArray特效");
        Game.Logger.LogInformation("   - 🛡️ 护甲系统: 头盔/护甲/靴子/手套 × 多品质");  
        Game.Logger.LogInformation("   - 💎 饰品系统: 项链/戒指 × 多品质");
        Game.Logger.LogInformation("   - ⚔️ 职业限制: 剑客用剑, 枪手用火枪, 法师用杖, 战士用锤子");
        Game.Logger.LogInformation("   - 🎒 装备栏位: 武器/护甲/饰品 (3槽位)");
        Game.Logger.LogInformation("   - 📦 物品分类: 职业专属武器分类系统已启用");
        Game.Logger.LogInformation("   - ✅ 槽位限制: 职业专属武器+通用护甲饰品分类生效!");
    }

    /// <summary>
    /// 创建物品模型配置 🎨
    /// </summary>
    private static void CreateItemModels()
    {
        Game.Logger.LogInformation("🎨 Creating item models...");

        // ========== 武器模型 ==========
        _ = new GameDataModel(Model.IronSwordModel)
        {
            Name = "铁剑模型",
            Radius = 15,
            Asset = "eqpt/weapon/sk_jk_wp1/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        _ = new GameDataModel(Model.DragonSwordModel)
        {
            Name = "龙纹剑模型",
            Radius = 20,
            Asset = "eqpt/weapon/sk_jk_wp1/model.prefab"u8, // 暂时使用铁剑模型资产
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        _ = new GameDataModel(Model.LegendarySwordModel)
        {
            Name = "传说之剑模型",
            Radius = 25,
            Asset = "eqpt/weapon/sk_jk_wp1/model.prefab"u8, // 使用相同模型，后续可替换
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        _ = new GameDataModel(Model.BasicPistolModel)
        {
            Name = "基础手枪模型",
            Radius = 10,
            Asset = "eqpt/weapon/sk_strongmale_shooter_weapon_gun_01/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        _ = new GameDataModel(Model.EpicPistolModel)
        {
            Name = "史诗手枪模型", 
            Radius = 12,
            Asset = "eqpt/weapon/sk_strongmale_shooter_weapon_gun_01/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        _ = new GameDataModel(Model.WoodenStaffModel)
        {
            Name = "木制法杖模型",
            Radius = 15,
            Asset = "eqpt/weapon/sk_standardfemale_mage_02_low_weapon_magic_01/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        _ = new GameDataModel(Model.CrystalStaffModel)
        {
            Name = "水晶法杖模型",
            Radius = 20,
            Asset = "eqpt/weapon/sk_standardfemale_mage_02_low_weapon_magic_01/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        _ = new GameDataModel(Model.IronHammerModel)
        {
            Name = "铁制锤子模型",
            Radius = 18,
            Asset = "eqpt/weapon/sk_strongman_warrior_hammer_01/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        _ = new GameDataModel(Model.DragonHammerModel)
        {
            Name = "龙纹锤子模型",
            Radius = 22,
            Asset = "eqpt/weapon/sk_strongman_warrior_hammer_01/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        // ========== 护甲模型 ==========
        _ = new GameDataModel(Model.LeatherArmorModel)
        {
            Name = "皮革护甲模型",
            Radius = 8,
            Asset = "eqpt/armor/leather_chest/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        _ = new GameDataModel(Model.DragonArmorModel)
        {
            Name = "龙鳞护甲模型", 
            Radius = 12,
            Asset = "eqpt/armor/leather_chest/model.prefab"u8, // 使用相同模型
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        // ========== 饰品模型 ==========
        _ = new GameDataModel(Model.GoldNecklaceModel)
        {
            Name = "黄金项链模型",
            Radius = 5,
            Asset = "eqpt/accessory/golden_necklace/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        _ = new GameDataModel(Model.DiamondRingModel)
        {
            Name = "钻石戒指模型",
            Radius = 3,
            Asset = "eqpt/accessory/diamond_ring/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        // ========== 消耗品模型 ==========
        _ = new GameDataModel(Model.HealthPotionModel)
        {
            Name = "生命药剂模型",
            Radius = 8,
            Asset = "deco/handpaintedvol2_campsite/sm_backpack_01_base/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        _ = new GameDataModel(Model.StrengthPotionModel)
        {
            Name = "力量药剂模型",
            Radius = 8,
            Asset = "deco/handpaintedvol2_campsite/sm_backpack_01_base/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };

        // ========== 任务道具模型 ==========
        _ = new GameDataModel(Model.MysteriousOrbModel)
        {
            Name = "神秘法球模型",
            Radius = 10,
            Asset = "deco/handpaintedvol2_campsite/sm_backpack_01_base/model.prefab"u8,
            ShadowSetting = new() { ShadowType = ShadowType.DeviceDependentShadow },
        };
    }

    /// <summary>
    /// 创建物品单位配置 🏺
    /// </summary>
    private static void CreateItemUnits()
    {
        Game.Logger.LogInformation("🏺 Creating item units...");

        // ========== 武器单位 ==========
        // 剑类武器单位
        _ = new GameDataUnit(Unit.IronSwordUnit)
        {
            Name = "铁剑物品",
            AttackableRadius = 80,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 30,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.IronSwordModel,
        };

        _ = new GameDataUnit(Unit.DragonSwordUnit)
        {
            Name = "龙纹剑物品",
            AttackableRadius = 80,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 40,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.DragonSwordModel,
        };

        _ = new GameDataUnit(Unit.LegendarySwordUnit)
        {
            Name = "传说之剑物品",
            AttackableRadius = 100,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 50,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.LegendarySwordModel,
        };

        // 手枪类武器单位
        _ = new GameDataUnit(Unit.BasicPistolUnit)
        {
            Name = "基础手枪物品",
            AttackableRadius = 60,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 25,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.BasicPistolModel,
        };

        _ = new GameDataUnit(Unit.EpicPistolUnit)
        {
            Name = "史诗手枪物品",
            AttackableRadius = 70,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 30,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.EpicPistolModel,
        };

        // ========== 法杖单位 ==========
        _ = new GameDataUnit(Unit.WoodenStaffUnit)
        {
            Name = "木制法杖物品",
            AttackableRadius = 70,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 35,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.WoodenStaffModel,
        };

        _ = new GameDataUnit(Unit.CrystalStaffUnit)
        {
            Name = "水晶法杖物品",
            AttackableRadius = 80,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 40,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.CrystalStaffModel,
        };

        // ========== 锤子单位 ==========
        _ = new GameDataUnit(Unit.IronHammerUnit)
        {
            Name = "铁制锤子物品",
            AttackableRadius = 90,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 35,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.IronHammerModel,
        };

        _ = new GameDataUnit(Unit.DragonHammerUnit)
        {
            Name = "龙纹锤子物品",
            AttackableRadius = 100,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 45,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.DragonHammerModel,
        };

        // ========== 护甲单位 ==========
        _ = new GameDataUnit(Unit.LeatherArmorUnit)
        {
            Name = "皮革护甲物品",
            AttackableRadius = 50,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 25,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.LeatherArmorModel,
        };

        _ = new GameDataUnit(Unit.DragonArmorUnit)
        {
            Name = "龙鳞护甲物品",
            AttackableRadius = 60,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 30,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.DragonArmorModel,
        };

        // ========== 饰品单位 ==========
        _ = new GameDataUnit(Unit.GoldNecklaceUnit)
        {
            Name = "黄金项链物品",
            AttackableRadius = 40,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 15,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.GoldNecklaceModel,
        };

        _ = new GameDataUnit(Unit.DiamondRingUnit)
        {
            Name = "钻石戒指物品",
            AttackableRadius = 35,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 10,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.DiamondRingModel,
        };

        // ========== 消耗品单位 ==========
        _ = new GameDataUnit(Unit.HealthPotionUnit)
        {
            Name = "生命药剂物品",
            AttackableRadius = 40,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 15,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.HealthPotionModel,
        };

        _ = new GameDataUnit(Unit.StrengthPotionUnit)
        {
            Name = "力量药剂物品",
            AttackableRadius = 40,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 15,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.StrengthPotionModel,
        };

        // ========== 任务道具单位 ==========
        _ = new GameDataUnit(Unit.MysteriousOrbUnit)
        {
            Name = "神秘法球物品",
            AttackableRadius = 45,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 20,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.MysteriousOrbModel,
        };
    }

    /// <summary>
    /// 创建物品配置 ⚔️
    /// </summary>
    private static void CreateItems()
    {
        Game.Logger.LogInformation("⚔️ Creating items...");

        // ========== 剑类武器（剑客专用）==========
        _ = new GameDataItemMod(Item.IronSword)
        {
            DisplayName = "铁剑",
            Description = "一把普通的铁制长剑，剑身坚固耐用。虽然制作工艺朴素，但足以陪伴新手剑客踏上冒险之路。",
            Icon = "@p_0tja/image/icons/swords_11_t.png",
            Unit = Unit.IronSwordUnit,
            Categories = [ARPGItemCategory.WeaponSword], // ⚔️ 剑类武器
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 25 },
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 20 },
                    ],
                    ActorArray = [Actor.SwordActor], // 使用剑类Actor
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 1,
            Quality = (int)ItemQuality.Common,
        };

        _ = new GameDataItemMod(Item.DragonSword)
        {
            DisplayName = "龙纹剑",
            Description = "剑身雕刻着神秘的龙纹图案，散发着淡淡的魔力光辉。传说中巨龙的力量被封印在剑中，为持有者带来额外的生命力。",
            Icon = "@p_0tja/image/icons/swords_11_t.png",
            Unit = Unit.DragonSwordUnit,
            Categories = [ARPGItemCategory.WeaponSword], // ⚔️ 剑类武器
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 45 },
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 50 },
                    ],
                    ActorArray = [Actor.SwordActor], // 使用剑类Actor
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 5,
            Quality = (int)ItemQuality.Epic,
        };

        _ = new GameDataItemMod(Item.LegendarySword)
        {
            DisplayName = "传说之剑",
            Description = "传说中的神兵利器，剑身流转着耀眼的光芒。只有真正的剑道大师才能发挥出它的全部威力。握住它的瞬间，仿佛感受到了无数英雄的意志。",
            Icon = "@p_0tja/image/icons/swords_11_t.png",
            Unit = Unit.LegendarySwordUnit,
            Categories = [ARPGItemCategory.WeaponSword], // ⚔️ 剑类武器
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 80 },
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 100 },
                        new() { Property = UnitProperty.MoveSpeed, Value = (_) => 30 },
                    ],
                    ActorArray = [Actor.SwordActor], // 使用剑类Actor
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 10,
            Quality = (int)ItemQuality.Legendary,
        };

        // ========== 手枪类武器（枪手专用）==========
        _ = new GameDataItemMod(Item.BasicPistol)
        {
            DisplayName = "基础手枪",
            Description = "一把结构简单但可靠的手枪，是新手枪手的理想选择。射击精度适中，维护简便，是远程作战的入门武器。",
            Unit = Unit.BasicPistolUnit,
            Categories = [ARPGItemCategory.WeaponGun], // 🔫 手枪类武器
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 30 },
                        new() { Property = UnitProperty.AttackRange, Value = (_) => 100 },
                    ],
                    ActorArray = [Actor.PistolActor], // 使用手枪类Actor
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 2,
            Quality = (int)ItemQuality.Common,
        };

        _ = new GameDataItemMod(Item.EpicPistol)
        {
            DisplayName = "史诗手枪",
            Description = "工艺精湛的精锻手枪，枪身镀有神秘的符文。射程更远，威力更强，是经验丰富的枪手梦寐以求的武器。",
            Icon = "@p_0tja/image/icons/gun_09_t.png",
            Unit = Unit.EpicPistolUnit,
            Categories = [ARPGItemCategory.WeaponGun], // 🔫 手枪类武器
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 55 },
                        new() { Property = UnitProperty.AttackRange, Value = (_) => 150 },
                        new() { Property = UnitProperty.MoveSpeed, Value = (_) => 20 },
                    ],
                    ActorArray = [Actor.PistolActor], // 使用手枪类Actor
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 6,
            Quality = (int)ItemQuality.Epic,
        };

        // ========== 法杖类武器（法师专用）==========
        _ = new GameDataItemMod(Item.WoodenStaff)
        {
            DisplayName = "木制法杖",
            Description = "一根由古老橡木制成的法杖，杖头镶嵌着一颗小水晶。虽然材质普通，但能够有效地引导魔法力量，是新手法师的良好伙伴。",
            Icon = "@p_0tja/image/icons/stave_009_t.png",
            Unit = Unit.WoodenStaffUnit,
            Categories = [ARPGItemCategory.WeaponStaff], // 🪄 法杖类武器
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 15 },
                        new() { Property = UnitProperty.ManaMax, Value = (_) => 100 },
                        new() { Property = UnitProperty.AttackRange, Value = (_) => 200 },
                    ],
                    ActorArray = [Actor.StaffActor], // 使用法杖类Actor
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 1,
            Quality = (int)ItemQuality.Common,
        };

        _ = new GameDataItemMod(Item.CrystalStaff)
        {
            DisplayName = "水晶法杖",
            Description = "杖身由稀有的秘银打造，杖头上定着一颗纯净的水晶。水晶中充满了魔法能量，可以显著提升法师的魔法攻击力和抗性。",
            Icon = "@p_0tja/image/icons/stave_009_t.png",
            Unit = Unit.CrystalStaffUnit,
            Categories = [ARPGItemCategory.WeaponStaff], // 🪄 法杖类武器
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 40 },
                        new() { Property = UnitProperty.ManaMax, Value = (_) => 200 },
                        new() { Property = UnitProperty.AttackRange, Value = (_) => 250 },
                        new() { Property = UnitProperty.MagicResistance, Value = (_) => 10 },
                    ],
                    ActorArray = [Actor.StaffActor], // 使用法杖类Actor
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 7,
            Quality = (int)ItemQuality.Epic,
        };

        // ========== 锤子类武器（战士专用）==========
        _ = new GameDataItemMod(Item.IronHammer)
        {
            DisplayName = "铁制锤子",
            Description = "一把沉重的铁制战锤，锤头巨大厚实。虽然笨重了些，但每一击都能带来毁灭性的力量。是战士们的可靠伙伴。",
            Icon = "@p_0tja/image/icons/iron_hammer_t.png",
            Unit = Unit.IronHammerUnit,
            Categories = [ARPGItemCategory.WeaponHammer], // 🔨 锤子类武器
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 40 },
                        new() { Property = UnitProperty.AttackRange, Value = (_) => 50 },
                        new() { Property = UnitProperty.Armor, Value = (_) => 5 },
                    ],
                    ActorArray = [Actor.HammerActor], // 使用锤子类Actor
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 3,
            Quality = (int)ItemQuality.Rare,
        };

        _ = new GameDataItemMod(Item.DragonHammer)
        {
            DisplayName = "龙纹锤子",
            Description = "传说中的巨龙鳞片融入锤头中，使这把战锤拥有了超凡的威力。每次挥舞时都会发出低沉的龙吿声，令敌人胆战心惊。",
            Icon = "@p_0tja/image/icons/iron_hammer_t.png",
            Unit = Unit.DragonHammerUnit,
            Categories = [ARPGItemCategory.WeaponHammer], // 🔨 锤子类武器
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 70 },
                        new() { Property = UnitProperty.AttackRange, Value = (_) => 80 },
                        new() { Property = UnitProperty.Armor, Value = (_) => 15 },
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 80 },
                    ],
                    ActorArray = [Actor.HammerActor], // 使用锤子类Actor
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 8,
            Quality = (int)ItemQuality.Epic,
        };

        // ========== 护甲类装备 ==========
        _ = new GameDataItemMod(Item.LeatherArmor)
        {
            DisplayName = "皮革护甲",
            Description = "用韧性良好的野兽皮革制成的轻型护甲。虽然防护力有限，但不会影响行动的灵活性。是初级冒险者的理想选择。",
            Icon = "@p_0tja/image/icons/dw_pnc_t_01.png",
            Unit = Unit.LeatherArmorUnit,
            Categories = [ARPGItemCategory.Armor], // 🛡️ 护甲类装备
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.Armor, Value = (_) => 8 },
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 30 },
                    ],
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 1,
            Quality = (int)ItemQuality.Common,
        };

        _ = new GameDataItemMod(Item.DragonArmor)
        {
            DisplayName = "龙鳞护甲",
            Description = "由古代巨龙的鳞片打造而成的神级护甲。鳞片闪烁着神秘的光芒，不仅提供极强的物理防护，还具有抵抗魔法攻击的能力。",
            Icon = "@p_0tja/image/icons/dw_pnc_t_01.png",
            Unit = Unit.DragonArmorUnit,
            Categories = [ARPGItemCategory.Armor], // 🛡️ 护甲类装备
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.Armor, Value = (_) => 25 },
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 100 },
                        new() { Property = UnitProperty.MagicResistance, Value = (_) => 15 },
                    ],
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 9,
            Quality = (int)ItemQuality.Epic,
        };

        // ========== 饰品类装备 ==========
        _ = new GameDataItemMod(Item.GoldNecklace)
        {
            DisplayName = "黄金项链",
            Description = "由纯金打造的精美项链，项链上镶嵌着小巧的蓝宝石。优雅的设计不仅为佩戴者增添魅力，还能提供魔法力量的增幅。",
            Icon = "@p_0tja/image/icons/nk_t_01.png",
            Unit = Unit.GoldNecklaceUnit,
            Categories = [ARPGItemCategory.Accessory], // 💎 饰品类装备
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.ManaMax, Value = (_) => 50 },
                        new() { Property = UnitProperty.MoveSpeed, Value = (_) => 15 },
                    ],
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 3,
            Quality = (int)ItemQuality.Rare,
        };

        _ = new GameDataItemMod(Item.DiamondRing)
        {
            DisplayName = "钻石戒指",
            Description = "戒托上的钻石在阳光下闪闪发光，散发着迷人的光芒。这枚戒指不仅是财富的象征，更能全面提升佩戴者的战斗能力。",
            Icon = "@p_0tja/image/icons/nk_t_01.png",
            Unit = Unit.DiamondRingUnit,
            Categories = [ARPGItemCategory.Accessory], // 💎 饰品类装备
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 20 },
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 40 },
                        new() { Property = UnitProperty.ManaMax, Value = (_) => 60 },
                        new() { Property = UnitProperty.MoveSpeed, Value = (_) => 25 },
                    ],
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 7,
            Quality = (int)ItemQuality.Epic,
        };

        // ========== 消耗品类物品 ==========
        _ = new GameDataItemMod(Item.HealthPotion)
        {
            DisplayName = "生命药剂",
            Description = "一瓶无色的治疗药剂，散发着淡淡的草药香气。使用后能够迅速恢复300点生命值，是冒险者们在危险时刻的救命稻草。",
            Icon = "@p_0tja/image/icons/b_m_02_t.png",
            Unit = Unit.HealthPotionUnit,
            Categories = [ARPGItemCategory.HealthPotion], // 🧪 恢复道具
            Modifications = new()
            {
                [ItemSlotType.Carry] = new()
                {
                    // 恢复道具可以在背包中携带，使用主动技能恢复生命值
                    Modifications = [
                        // 携带时无属性加成，但可以使用主动技能恢复生命值
                    ],
                    Ability = ARPGAbilities.Ability.HealthRestore, // 添加生命恢复技能
                }
            },
            StackStart = 1,
            StackMax = 5, // 可以堆叠5个
            Level = 1,
            Quality = (int)ItemQuality.Common,
        };

        _ = new GameDataItemMod(Item.StrengthPotion)
        {
            DisplayName = "力量药剂",
            Description = "一瓶散发着金色光芒的神秘药剂，入口略带辛辣。使用后获得30秒强化效果：+50攻击力、+30移动速度、+100最大生命值。",
            Icon = "@p_0tja/image/icons/b_m_02_t.png",
            Unit = Unit.StrengthPotionUnit,
            Categories = [ARPGItemCategory.BuffPotion], // 🧪 增益道具
            Modifications = new()
            {
                [ItemSlotType.Carry] = new()
                {
                    // 增益道具可以在背包中携带，使用主动技能提供临时buff
                    Modifications = [
                        // 携带时无属性加成，但可以使用主动技能提供临时buff
                    ],
                    Ability = ARPGAbilities.Ability.StrengthEnhance, // 添加力量强化技能
                }
            },
            StackStart = 1,
            StackMax = 3, // 可以堆叠3个
            Level = 2,
            Quality = (int)ItemQuality.Rare,
        };

        // ========== 任务道具类物品 ==========
        _ = new GameDataItemMod(Item.MysteriousOrb)
        {
            DisplayName = "神秘法球",
            Description = "一颗散发着神秘能量的球体，内部似乎有着星云般的光芒在流转。其真正的用途仍是一个谜，或许只有完成特定任务时才能了解它的秘密。",
            Unit = Unit.MysteriousOrbUnit,
            Categories = [ARPGItemCategory.QuestItem], // 📋 任务道具
            Modifications = new()
            {
                // 任务道具没有任何功能，仅用于任务系统
            },
            StackStart = 1,
            StackMax = 1, // 任务道具不可堆叠
            Level = 1,
            Quality = (int)ItemQuality.Common,
        };
    }
}
