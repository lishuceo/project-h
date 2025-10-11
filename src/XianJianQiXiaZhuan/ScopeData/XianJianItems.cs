using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.SceneSystem;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.AISystem.Data;
using GameCore.AISystem.Data.Enum;
using GameCore.CollisionSystem.Data.Struct;
using GameCore.CollisionSystem.Data.Enum;
using GameCore.Behavior;
using GameCore.BuffSystem.Data;
using GameCore.Container;
using GameCore.Container.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.Execution.Data;
using GameCore.Leveling.Data;
using GameCore.Execution.Data.Enum;
using GameCore.GameSystem.Data;
using GameCore.PlayerAndUsers.Enum;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.SceneSystem.Data;
using GameCore.SceneSystem.Data.Struct;
using GameData;
using GameCore.TargetingSystem.Data;
using GameCore.ActorSystem.Data.Enum;
using GameCore.Animation.Enum;
using GameCore.ModelAnimation.Data;
using GameCore;
using GameCore.AbilitySystem.Manager;
using GameCore.EntitySystem;
using GameCore.OrderSystem;
using System.Numerics;
using static GameCore.ScopeData;
using GameCore.ItemSystem.Data;

namespace GameEntry.XianJianQiXiaZhuan.ScopeData;

/// <summary>
/// 仙剑奇侠传物品类别扩展 🗡️
/// 使用EnumExtension机制扩展ItemCategory
/// </summary>
[EnumExtension(Extends = typeof(ItemCategory))]
public enum EXianJianItemCategory
{
    // 仙剑专属分类 - 从2000开始，避免与ARPG冲突
    WeaponSword = 2001,   // 剑类武器
    Armor = 2010,         // 防具类
    Accessory = 2011,     // 饰品类
    Medicine = 2020,      // 药品类
}


/// <summary>
/// 仙剑奇侠传物品系统定义
/// </summary>
public class XianJianItems : IGameClass
{
    #region 物品定义
    public static class Item
    {
        // === 武器类 ===
        /// <summary>仙剑 - 神器级武器</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> XianJian = new("XianJian"u8);
        
        /// <summary>镇妖剑 - 专门对付妖魔的强力武器</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> ZhenYaoJian = new("ZhenYaoJian"u8);
        
        /// <summary>青罡剑 - 李逍遥初始武器</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> QingGangSword = new("QingGangSword"u8);
        
        /// <summary>烈焰剑 - 火系武器</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> FlameSword = new("FlameSword"u8);
        
        /// <summary>凤凰羽 - 赵灵儿专用武器</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> PhoenixFeather = new("PhoenixFeather"u8);
        
        /// <summary>龙泉剑 - 林月如专用武器</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> DragonSword = new("DragonSword"u8);
        
        // === 防具类 ===
        /// <summary>云纹袍 - 基础衣服</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> CloudRobe = new("CloudRobe"u8);
        
        /// <summary>仙人履 - 基础靴子</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> ImmortalShoes = new("ImmortalShoes"u8);
        
        /// <summary>护心镜 - 防护饰品</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> HeartMirror = new("HeartMirror"u8);
        
        // === 药品类 ===
        /// <summary>回阳草 - 回血药品</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> HealingHerb = new("HealingHerb"u8);
        
        /// <summary>天心草 - 回蓝药品</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> ManaHerb = new("ManaHerb"u8);
        
        /// <summary>千年人参 - 高级恢复药品</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> Ginseng = new("Ginseng"u8);
        
        // === 特殊物品 ===
        /// <summary>灵珠 - 法力增强物品</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> SpiritOrb = new("SpiritOrb"u8);
        
        /// <summary>玉佩 - 防护饰品</summary>
        public static readonly GameLink<GameDataItem, GameDataItemMod> JadePendant = new("JadePendant"u8);
    }
    #endregion

    #region 武器模型定义
    public static class WeaponModel
    {
        /// <summary>镇妖剑模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> ZhenYaoJianModel = new("ZhenYaoJianModel"u8);
    }
    #endregion

    #region 武器Actor定义  
    public static class WeaponActor
    {
        /// <summary>镇妖剑Actor - 装备时显示的武器模型</summary>
        public static readonly GameLink<GameDataActor, GameDataActorAdditionModel> ZhenYaoJianActor = new("ZhenYaoJianActor"u8);
    }
    #endregion

    #region 物品单位定义
    public static class ItemUnit
    {
        public static readonly GameLink<GameDataUnit, GameDataUnit> XianJianUnit = new("XianJianUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> QingGangSwordUnit = new("QingGangSwordUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> FlameSwordUnit = new("FlameSwordUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> PhoenixFeatherUnit = new("PhoenixFeatherUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> DragonSwordUnit = new("DragonSwordUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> CloudRobeUnit = new("CloudRobeUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> ImmortalShoesUnit = new("ImmortalShoesUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> HeartMirrorUnit = new("HeartMirrorUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> HealingHerbUnit = new("HealingHerbUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> ManaHerbUnit = new("ManaHerbUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> GinsengUnit = new("GinsengUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> SpiritOrbUnit = new("SpiritOrbUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> JadePendantUnit = new("JadePendantUnit"u8);
    }
    #endregion

    #region 物品模型定义
    public static class ItemModel
    {
        public static readonly GameLink<GameDataModel, GameDataModel> XianJianModel = new("XianJianModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> SwordModel = new("SwordModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> HerbModel = new("HerbModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> ArmorModel = new("ArmorModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> OrbModel = new("OrbModel"u8);
    }
    #endregion

    public static void OnRegisterGameClass()
    {
        // 先移除可能已存在的订阅，避免重复注册
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        // 只在仙剑奇侠传模式下初始化
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.XianJianQiXiaZhuan)
        {
            return;
        }

        Game.Logger.LogInformation("📦 初始化仙剑奇侠传物品系统...");

        // 初始化各个子系统
        InitializeItemModels();     // 物品模型
        InitializeWeaponActors();   // 武器Actor（装备效果）
        InitializeItemUnits();      // 物品单位
        InitializeItems();          // 物品配置

        Game.Logger.LogInformation("✅ 仙剑奇侠传物品系统初始化完成!");
    }

    /// <summary>
    /// 初始化物品模型配置
    /// </summary>
    private static void InitializeItemModels()
    {
        Game.Logger.LogInformation("🎨 配置仙剑物品模型...");

        // ========== 仙剑模型 ==========
        _ = new GameDataModel(ItemModel.XianJianModel)
        {
            Name = "仙剑",
            Radius = 50,
            Asset = "eqpt/weapon/sm_dr_sword_04_02/model.prefab"u8, // 暂用现有剑模型
        };

        // ========== 镇妖剑模型 ==========
        _ = new GameDataModel(WeaponModel.ZhenYaoJianModel)
        {
            Name = "镇妖剑",
            Radius = 45,
            Asset = "eqpt/weapon/sm_dr_sword_04_02/model.prefab"u8, // 使用龙纹剑模型
        };

        // ========== 一般剑类模型 ==========
        _ = new GameDataModel(ItemModel.SwordModel)
        {
            Name = "剑类武器",
            Radius = 45,
            Asset = "eqpt/weapon/sm_dr_sword_04_02/model.prefab"u8,
        };

        // ========== 草药模型 ==========
        _ = new GameDataModel(ItemModel.HerbModel)
        {
            Name = "草药",
            Radius = 30,
            Asset = "deco/handpaintedvol2_campsite/sm_backpack_01_base/model.prefab"u8, // 暂用背包模型
        };

        // ========== 防具模型 ==========
        _ = new GameDataModel(ItemModel.ArmorModel)
        {
            Name = "防具",
            Radius = 40,
            Asset = "deco/handpaintedvol2_campsite/sm_backpack_01_base/model.prefab"u8,
        };

        // ========== 灵珠模型 ==========
        _ = new GameDataModel(ItemModel.OrbModel)
        {
            Name = "灵珠",
            Radius = 25,
            Asset = "deco/handpaintedvol2_campsite/sm_backpack_01_base/model.prefab"u8,
        };

        Game.Logger.LogInformation("✅ 仙剑物品模型配置完成!");
    }

    /// <summary>
    /// 初始化武器Actor配置 - 装备时显示的武器模型
    /// </summary>
    private static void InitializeWeaponActors()
    {
        Game.Logger.LogInformation("⚔️ 配置武器装备效果Actor...");

        // ========== 镇妖剑Actor ==========
        _ = new GameDataActorAdditionModel(WeaponActor.ZhenYaoJianActor)
        {
            Name = "镇妖剑装备效果",
            Model = WeaponModel.ZhenYaoJianModel, // 使用镇妖剑模型
        };

        Game.Logger.LogInformation("✅ 武器装备效果Actor配置完成!");
    }

    /// <summary>
    /// 初始化物品单位配置
    /// </summary>
    private static void InitializeItemUnits()
    {
        Game.Logger.LogInformation("🎭 配置仙剑物品单位...");

        // ========== 仙剑单位 ==========
        _ = new GameDataUnit(ItemUnit.XianJianUnit)
        {
            Name = "仙剑",
            AttackableRadius = 60,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 32,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = ItemModel.XianJianModel,
        };

        // ========== 青罡剑单位 ==========
        _ = new GameDataUnit(ItemUnit.QingGangSwordUnit)
        {
            Name = "青罡剑",
            AttackableRadius = 55,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 30,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = ItemModel.SwordModel,
        };

        // ========== 其他武器单位 (使用统一模板) ==========
        CreateWeaponUnit(ItemUnit.FlameSwordUnit, "烈焰剑");
        CreateWeaponUnit(ItemUnit.PhoenixFeatherUnit, "凤凰羽");
        CreateWeaponUnit(ItemUnit.DragonSwordUnit, "龙泉剑");

        // ========== 防具单位 ==========
        CreateArmorUnit(ItemUnit.CloudRobeUnit, "云纹袍");
        CreateArmorUnit(ItemUnit.ImmortalShoesUnit, "仙人履");
        CreateArmorUnit(ItemUnit.HeartMirrorUnit, "护心镜");

        // ========== 药品单位 ==========
        CreateHerbUnit(ItemUnit.HealingHerbUnit, "回阳草");
        CreateHerbUnit(ItemUnit.ManaHerbUnit, "天心草");
        CreateHerbUnit(ItemUnit.GinsengUnit, "千年人参");

        // ========== 特殊物品单位 ==========
        CreateSpecialUnit(ItemUnit.SpiritOrbUnit, "灵珠");
        CreateSpecialUnit(ItemUnit.JadePendantUnit, "玉佩");

        Game.Logger.LogInformation("✅ 仙剑物品单位配置完成!");
    }

    /// <summary>
    /// 创建武器单位的辅助方法
    /// </summary>
    private static void CreateWeaponUnit(GameLink<GameDataUnit, GameDataUnit> unitLink, string name)
    {
        _ = new GameDataUnit(unitLink)
        {
            Name = name,
            AttackableRadius = 55,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 30,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = ItemModel.SwordModel,
        };
    }

    /// <summary>
    /// 创建防具单位的辅助方法
    /// </summary>
    private static void CreateArmorUnit(GameLink<GameDataUnit, GameDataUnit> unitLink, string name)
    {
        _ = new GameDataUnit(unitLink)
        {
            Name = name,
            AttackableRadius = 50,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 28,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = ItemModel.ArmorModel,
        };
    }

    /// <summary>
    /// 创建草药单位的辅助方法
    /// </summary>
    private static void CreateHerbUnit(GameLink<GameDataUnit, GameDataUnit> unitLink, string name)
    {
        _ = new GameDataUnit(unitLink)
        {
            Name = name,
            AttackableRadius = 45,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 25,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = ItemModel.HerbModel,
        };
    }

    /// <summary>
    /// 创建特殊物品单位的辅助方法
    /// </summary>
    private static void CreateSpecialUnit(GameLink<GameDataUnit, GameDataUnit> unitLink, string name)
    {
        _ = new GameDataUnit(unitLink)
        {
            Name = name,
            AttackableRadius = 40,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 20,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = ItemModel.OrbModel,
        };
    }

    /// <summary>
    /// 初始化物品配置
    /// </summary>
    private static void InitializeItems()
    {
        Game.Logger.LogInformation("⚔️ 配置仙剑奇侠传物品...");

        // ========== 仙剑 - 神器 ==========
        _ = new GameDataItemMod(Item.XianJian)
        {
            Name = "仙剑",
            Unit = ItemUnit.XianJianUnit,
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 150, Random = 30 },
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 200, Random = 50 },
                        new() { Property = UnitProperty.ManaMax, Value = (_) => 100, Random = 25 },
                        new() { Property = UnitProperty.MagicResistance, Value = (_) => 20, Random = 10 },
                    ],
                    Ability = XianJianAbilities.Ability.SwordSlash,
                }
            },
            StackStart = 1,
            StackMax = 1, // 神器不能堆叠
            Level = 10,   // 最高等级
            Quality = 10, // 最高品质
        };

        // ========== 镇妖剑 - 专业驱魔武器 ==========
        _ = new GameDataItemMod(Item.ZhenYaoJian)
        {
            DisplayName = "镇妖剑",
            Description = "专门对付妖魔的强力武器，剑身散发着神圣的光芒。",
            Unit = ItemUnit.XianJianUnit,
            Categories = [XianJianItemCategory.WeaponSword], // ⚔️ 剑类武器
            Modifications = new()
            {
                [ItemSlotType.Equip] = new ItemUnitModificationData()
                {
                    Modifications = new List<UnitPropertyModification>
                    {
                        new UnitPropertyModification() { 
                            Property = UnitProperty.AttackDamage, 
                            Value = (_) => 500, 
                            Random = 50,
                            SubType = PropertySubType.Base 
                        },
                        new UnitPropertyModification() { 
                            Property = UnitProperty.LifeMax, 
                            Value = (_) => 150,
                            Random = 25,
                            SubType = PropertySubType.Base 
                        },
                    },
                    ActorArray = new List<IGameLink<GameDataActor>> { WeaponActor.ZhenYaoJianActor },
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 8,
            Quality = 8,
        };

        // ========== 青罡剑 - 初始武器 ==========
        _ = new GameDataItemMod(Item.QingGangSword)
        {
            Name = "青罡剑",
            Unit = ItemUnit.QingGangSwordUnit,
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 50, Random = 15 },
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 50, Random = 15 },
                    ],
                    Ability = XianJianAbilities.Ability.SwordSlash,
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 3,
            Quality = 5,
        };

        // ========== 烈焰剑 - 火系武器 ==========
        _ = new GameDataItemMod(Item.FlameSword)
        {
            Name = "烈焰剑",
            Unit = ItemUnit.FlameSwordUnit,
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 80, Random = 20 },
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 80, Random = 20 },
                        new() { Property = UnitProperty.MagicResistance, Value = (_) => 5, Random = 5 },
                    ],
                    Ability = XianJianAbilities.Ability.SwordSlash,
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 5,
            Quality = 7,
        };

        // ========== 凤凰羽 - 赵灵儿专用 ==========
        _ = new GameDataItemMod(Item.PhoenixFeather)
        {
            Name = "凤凰羽",
            Unit = ItemUnit.PhoenixFeatherUnit,
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 60, Random = 15 },
                        new() { Property = UnitProperty.ManaMax, Value = (_) => 120, Random = 30 },
                        new() { Property = UnitProperty.MagicResistance, Value = (_) => 15, Random = 8 },
                    ],
                    Ability = XianJianAbilities.Ability.LightningSpell,
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 6,
            Quality = 8,
        };

        // ========== 龙泉剑 - 林月如专用 ==========
        _ = new GameDataItemMod(Item.DragonSword)
        {
            Name = "龙泉剑",
            Unit = ItemUnit.DragonSwordUnit,
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.AttackDamage, Value = (_) => 100, Random = 25 },
                        new() { Property = UnitProperty.MoveSpeed, Value = (_) => 50, Random = 15 },
                        new() { Property = UnitProperty.Armor, Value = (_) => 10, Random = 5 },
                    ],
                    Ability = XianJianAbilities.Ability.SwordCombo,
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 7,
            Quality = 8,
        };

        // ========== 云纹袍 - 基础防具 ==========
        _ = new GameDataItemMod(Item.CloudRobe)
        {
            Name = "云纹袍",
            Unit = ItemUnit.CloudRobeUnit,
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 100, Random = 25 },
                        new() { Property = UnitProperty.Armor, Value = (_) => 15, Random = 8 },
                        new() { Property = UnitProperty.MagicResistance, Value = (_) => 10, Random = 5 },
                    ],
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 4,
            Quality = 6,
        };

        // ========== 护心镜 - 防护饰品 ==========
        _ = new GameDataItemMod(Item.HeartMirror)
        {
            Name = "护心镜",
            Unit = ItemUnit.HeartMirrorUnit,
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 150, Random = 40 },
                        new() { Property = UnitProperty.Armor, Value = (_) => 20, Random = 10 },
                        new() { Property = UnitProperty.MagicResistance, Value = (_) => 20, Random = 10 },
                    ],
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 6,
            Quality = 8,
        };

        // ========== 回阳草 - 基础回血药 ==========
        _ = new GameDataItemMod(Item.HealingHerb)
        {
            Name = "回阳草",
            Unit = ItemUnit.HealingHerbUnit,
            Modifications = new()
            {
                [ItemSlotType.Carry] = new()
                {
                    Ability = XianJianAbilities.Ability.HealingSpell, // 使用时触发治疗
                }
            },
            StackStart = 3,
            StackMax = 20,
            Level = 1,
            Quality = 3,
        };

        // ========== 天心草 - 回蓝药 ==========
        _ = new GameDataItemMod(Item.ManaHerb)
        {
            Name = "天心草",
            Unit = ItemUnit.ManaHerbUnit,
            Modifications = new()
            {
                [ItemSlotType.Carry] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.ManaMax, Value = (_) => 50, Random = 15 },
                    ],
                }
            },
            StackStart = 2,
            StackMax = 15,
            Level = 1,
            Quality = 3,
        };

        // ========== 千年人参 - 高级恢复药 ==========
        _ = new GameDataItemMod(Item.Ginseng)
        {
            Name = "千年人参",
            Unit = ItemUnit.GinsengUnit,
            Modifications = new()
            {
                [ItemSlotType.Carry] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 200, Random = 50 },
                        new() { Property = UnitProperty.ManaMax, Value = (_) => 100, Random = 30 },
                    ],
                    Ability = XianJianAbilities.Ability.HealingSpell,
                }
            },
            StackStart = 1,
            StackMax = 5,
            Level = 8,
            Quality = 9,
        };

        // ========== 灵珠 - 法力增强 ==========
        _ = new GameDataItemMod(Item.SpiritOrb)
        {
            Name = "灵珠",
            Unit = ItemUnit.SpiritOrbUnit,
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.ManaMax, Value = (_) => 180, Random = 45 },
                        new() { Property = UnitProperty.MagicResistance, Value = (_) => 25, Random = 12 },
                    ],
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 7,
            Quality = 8,
        };

        // ========== 玉佩 - 防护饰品 ==========
        _ = new GameDataItemMod(Item.JadePendant)
        {
            Name = "玉佩",
            Unit = ItemUnit.JadePendantUnit,
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [
                        new() { Property = UnitProperty.LifeMax, Value = (_) => 120, Random = 30 },
                        new() { Property = UnitProperty.Armor, Value = (_) => 12, Random = 6 },
                        new() { Property = UnitProperty.MoveSpeed, Value = (_) => 30, Random = 10 },
                    ],
                }
            },
            StackStart = 1,
            StackMax = 1,
            Level = 5,
            Quality = 7,
        };

        Game.Logger.LogInformation("✅ 仙剑奇侠传物品配置完成!");
        Game.Logger.LogInformation("   - 武器: 仙剑(神器), 青罡剑, 烈焰剑, 凤凰羽, 龙泉剑");
        Game.Logger.LogInformation("   - 防具: 云纹袍, 仙人履, 护心镜");
        Game.Logger.LogInformation("   - 药品: 回阳草, 天心草, 千年人参");
        Game.Logger.LogInformation("   - 饰品: 灵珠, 玉佩");
    }
}
