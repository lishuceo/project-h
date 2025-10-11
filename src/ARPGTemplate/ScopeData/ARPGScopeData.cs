using GameCore.Container;
using GameCore.Container.Data;
using GameCore.Data;
using GameCore.Leveling.Data;
using GameCore.SceneSystem;
using GameCore.SceneSystem.Data;
using GameCore.SceneSystem.Data.Struct;
using GameData;
using System.Numerics;
using static GameCore.ScopeData;
using GameCore.Struct;
using GameData.Extension;
using GameCore.EntitySystem;
using GameCore.AbilitySystem.Data;
using GameCore.AISystem.Data;
using GameUI.CameraSystem.Data;
using GameCore.TargetingSystem.Data;
using GameCore.ActorSystem.Data;
using GameCore.ActorSystem.Data.Enum;
using GameCore.Behavior;
using GameUI.Brush;
using System;

namespace GameEntry.ARPGTemplate.ScopeData;

/// <summary>
/// ARPG扩展单位属性GameLink定义 🔗
/// 为扩展属性创建对应的GameLink，用于Buff修改等场景
/// </summary>
public static class ARPGUnitPropertyLink
{
    /// <summary>
    /// 冷却缩减率属性 - 用于Buff修改
    /// 使用扩展属性的数值ID来确保正确对应
    /// </summary>
    public static readonly GameLink<GameDataUnitProperty, GameDataUnitProperty> CooldownReduction = 
        new(2001);
}

/// <summary>
/// ARPG模板主数据配置 - 场景和整体初始化
/// 技能系统请参考 ScopeData/ARPGAbilities.cs
/// 单位系统请参考 ScopeData/ARPGUnits.cs
/// 物品系统请参考 ScopeData/ARPGItems.cs
/// </summary>
public class ARPGScopeData : IGameClass
{
    #region 场景定义
    public static class Scene
    {
        public static readonly GameLink<GameDataScene, GameDataScene> ARPGScene = new("ARPGScene"u8);
    }
    #endregion

    #region 背包系统定义
    public static class Inventory
    {
        // 主背包（通用）
        public static readonly GameLink<GameDataInventory, GameDataInventory> HeroMainInventory = new("HeroMainInventory"u8);
        
        // 职业专属装备背包
        public static readonly GameLink<GameDataInventory, GameDataInventory> SwordsmanEquipInventory = new("SwordsmanEquipInventory"u8);
        public static readonly GameLink<GameDataInventory, GameDataInventory> GunnerEquipInventory = new("GunnerEquipInventory"u8);
        public static readonly GameLink<GameDataInventory, GameDataInventory> MageEquipInventory = new("MageEquipInventory"u8);
        public static readonly GameLink<GameDataInventory, GameDataInventory> WarriorEquipInventory = new("WarriorEquipInventory"u8);
    }
    #endregion

    #region 升级系统定义
    public static class UnitLeveling
    {
        // ARPG英雄升级系统 - 可升到10级，每级加5攻击力
        public static readonly GameLink<GameDataUnitLeveling, GameDataUnitLeveling> HeroLeveling = new("ARPGHeroLeveling"u8);
    }
    #endregion

    #region Particle系统定义
    public static class Particle
    {
        // 范围指示器粒子 - 显示技能有效范围
        public static readonly GameLink<GameDataParticle, GameDataParticle> RangeParticle = new("ARPGRangeParticle"u8);
        
        // AOE区域指示器粒子 - 显示范围攻击区域
        public static readonly GameLink<GameDataParticle, GameDataParticle> AOEParticle = new("ARPGAOEParticle"u8);
        
        // 预瞄准圆圈粒子 - 显示圆形瞄准区域
        public static readonly GameLink<GameDataParticle, GameDataParticle> PreTargetingCircle = new("ARPGPreTargetingCircle"u8);
        
        // 直线指示器相关粒子
        public static readonly GameLink<GameDataParticle, GameDataParticle> LineLaunchParticle = new("ARPGLineLaunchParticle"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> LineCenterParticle = new("ARPGLineCenterParticle"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> LineImpactParticle = new("ARPGLineImpactParticle"u8);
        
        // 技能释放特效粒子 - 显示技能释放效果
        public static readonly GameLink<GameDataParticle, GameDataParticle> CastEffectParticle = new("ARPGCastEffectParticle"u8);
        
        // 技能命中特效粒子 - 显示技能命中效果
        public static readonly GameLink<GameDataParticle, GameDataParticle> HitEffectParticle = new("ARPGHitEffectParticle"u8);
    }
    #endregion

    #region Actor系统定义
    public static class Actor
    {
        // 范围指示器Actor - 显示技能有效范围
        public static readonly GameLink<GameDataActor, GameDataActorParticle> RangeActor = new("ARPGRangeActor"u8);
        
        // AOE区域指示器Actor - 显示范围攻击区域
        public static readonly GameLink<GameDataActor, GameDataActorParticle> AOEActor = new("ARPGAOEActor"u8);
        
        // 直线指示器Actor - 显示直线攻击轨迹
        public static readonly GameLink<GameDataActor, GameDataActorSegmentedRectangle> LineSegment = new("ARPGLineSegment"u8);
        
        // 直线指示器子Actor
        public static readonly GameLink<GameDataActor, GameDataActorParticle> LineLaunchActor = new("ARPGLineLaunchActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> LineCenterActor = new("ARPGLineCenterActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> LineImpactActor = new("ARPGLineImpactActor"u8);
        
        // 预瞄准高亮Actor - 高亮显示目标
        public static readonly GameLink<GameDataActor, GameDataActorHighlight> PreTargetingHighlight = new("ARPGPreTargetingHighlight"u8);
        
        // 预瞄准圆圈Actor - 显示圆形瞄准区域
        public static readonly GameLink<GameDataActor, GameDataActorParticle> PreTargetingCircle = new("ARPGPreTargetingCircle"u8);
        
        // 技能释放特效Actor - 显示技能释放效果
        public static readonly GameLink<GameDataActor, GameDataActorParticle> CastEffectActor = new("ARPGCastEffectActor"u8);
    }
    #endregion

    #region 指示器系统定义
    public static class TargetingIndicator
    {
        // 完整测试指示器 - 包含所有指示器功能
        public static readonly GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator> TestTargetingIndicator = new("ARPGTestTargetingIndicator"u8);
        
        // 直线指示器 - 用于技能瞄准
        public static readonly GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator> LineIndicator = new("ARPGLineIndicator"u8);
        
        // 区域指示器 - 用于范围技能
        public static readonly GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator> AreaIndicator = new("ARPGAreaIndicator"u8);
    }
    #endregion

    #region AI系统定义
    public static class AI
    {
        // 战斗测试AI - 独立战斗逻辑
        public static readonly GameLink<GameDataAIThinkTree, GameDataAIThinkTree> CombatTest = new("ARPGCombatTestAI"u8);
        
        // 怪物AI - 标准怪物行为逻辑
        public static readonly GameLink<GameDataAIThinkTree, GameDataAIThinkTree> MonsterAI = new("ARPGMonsterAI"u8);
        
        // Boss AI - 高级Boss行为逻辑
        public static readonly GameLink<GameDataAIThinkTree, GameDataAIThinkTree> BossAI = new("ARPGBossAI"u8);
        
        // 跟随AI - NPC跟随玩家行为
        public static readonly GameLink<GameDataAIThinkTree, GameDataAIThinkTree> FollowAI = new("ARPGFollowAI"u8);
        
        // 巡逻AI - 守卫巡逻行为
        public static readonly GameLink<GameDataAIThinkTree, GameDataAIThinkTree> PatrolAI = new("ARPGPatrolAI"u8);
    }
    #endregion

    #region 镜头系统定义
    public static class Camera
    {
        // 默认镜头 - ARPG专用镜头配置
        public static readonly GameLink<GameDataCamera, GameDataCamera> DefaultCamera = new("ARPGDefaultCamera"u8);
        
        // ARPG战斗镜头 - 专为激烈战斗场景优化的镜头
        public static readonly GameLink<GameDataCamera, GameDataCamera> CombatCamera = new("ARPGCombatCamera"u8);
        
        // ARPG探索镜头 - 用于地图探索和任务进行
        public static readonly GameLink<GameDataCamera, GameDataCamera> ExplorationCamera = new("ARPGExplorationCamera"u8);
    }
    #endregion

    #region 技能系统定义  
    public static class Ability
    {
        // 测试技能 - 用于调试和测试
        public static readonly GameLink<GameDataAbility, GameDataAbility> TestSpell = new("ARPGTestSpell"u8);
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
        // 只在ARPG模式下初始化
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.ARPGMode)
        {
            return;
        }

        Game.Logger.LogInformation("⚔️ Initializing ARPG Scene System for ARPG Mode...");

        // 初始化各个子系统
        InitializeARPGUnitProperties(); // 🎯 首先初始化扩展属性
        InitializeParticles();       // ✨ 粒子系统
        InitializeActors();          // 🎭 Actor系统
        InitializeTargetingIndicators(); // 🎯 指示器系统
        InitializeAI();              // 🤖 AI系统
        InitializeCameras();         // 📹 镜头系统
        InitializeAbilities();       // ⚡ 技能系统
        InitializeUnitLeveling();    // ⬆️ 升级系统
        InitializeInventories();     // 🎒 背包系统
        InitializeScenes();          // 🌍 场景系统

        Game.Logger.LogInformation("✅ ARPG Scene System initialized successfully for ARPG Mode!");
    }

    /// <summary>
    /// 初始化ARPG场景配置
    /// </summary>
    private static void InitializeScenes()
    {
        Game.Logger.LogInformation("🌍 Configuring ARPG Scene...");

        // ========== 场景配置 ==========
        _ = new GameDataScene(Scene.ARPGScene)
        {
            DefaultCamera = Camera.DefaultCamera,
            Name = "ARPG多职业场景",
            HostedSceneTag = "default"u8, // 使用默认场景资源
            Size = new(16 * 256, 16 * 256),
            OnLoaded = static (scene) => Game.Logger.LogInformation("⚔️ ARPG多职业场景 {scene} 已加载", scene),
            PlacedPlayerObjects = new()
            {
                // 玩家1 - 剑客英雄（默认）
                {
                    1, new PlacedUnit()
                    {
                        Link = ARPGUnits.Unit.SwordsmanHero,
                        OwnerPlayerId = 1,
                        Position = new(3000, 3000, 0),
                        IsMainUnit = true,
                        TriggerGetter = true,
                        UniqueId = 1,
                    }
                },
                // 玩家2 - 枪手英雄（可选）
                {
                    2, new PlacedUnit()
                    {
                        Link = ARPGUnits.Unit.GunnerHero,
                        OwnerPlayerId = 1,
                        Position = new(2800, 3000, 0),
                        IsMainUnit = false,
                        TriggerGetter = true,
                        UniqueId = 2,
                    }
                },
                // 玩家3 - 法师英雄（可选）
                {
                    3, new PlacedUnit()
                    {
                        Link = ARPGUnits.Unit.MageHero,
                        OwnerPlayerId = 1,
                        Position = new(3200, 3000, 0),
                        IsMainUnit = false,
                        TriggerGetter = true,
                        UniqueId = 3,
                    }
                },
                // 玩家4 - 战士英雄（可选）
                {
                    4, new PlacedUnit()
                    {
                        Link = ARPGUnits.Unit.WarriorHero,
                        OwnerPlayerId = 1,
                        Position = new(3000, 2800, 0),
                        IsMainUnit = false,
                        TriggerGetter = true,
                        UniqueId = 4,
                    }
                },
                // ========== 测试物品区域 🎁 ==========
                
                // 放置在地上的龙纹剑
                {
                    5, new PlacedItem()
                    {
                        Link = ARPGItems.Item.DragonSword, // 龙纹剑物品
                        Position = new(3300, 2800, 0), // 剑客左前方位置，方便拾取
                        TriggerGetter = true,
                        UniqueId = 5,
                    }
                },

                // ========== 剑类武器测试 🗡️ ==========
                {
                    10, new PlacedItem()
                    {
                        Link = ARPGItems.Item.IronSword, // 铁剑 (普通品质)
                        Position = new(3400, 2700, 0),
                        TriggerGetter = true,
                        UniqueId = 10,
                    }
                },
                {
                    11, new PlacedItem()
                    {
                        Link = ARPGItems.Item.LegendarySword, // 传说之剑 (传说品质)
                        Position = new(3500, 2700, 0),
                        TriggerGetter = true,
                        UniqueId = 11,
                    }
                },

                // ========== 手枪类武器测试 🔫 ==========
                {
                    20, new PlacedItem()
                    {
                        Link = ARPGItems.Item.BasicPistol, // 基础手枪 (普通品质)
                        Position = new(2600, 2800, 0),
                        TriggerGetter = true,
                        UniqueId = 20,
                    }
                },
                {
                    21, new PlacedItem()
                    {
                        Link = ARPGItems.Item.EpicPistol, // 史诗手枪 (史诗品质)
                        Position = new(2600, 2700, 0),
                        TriggerGetter = true,
                        UniqueId = 21,
                    }
                },

                // ========== 法杖类武器测试 🪄 ==========
                {
                    30, new PlacedItem()
                    {
                        Link = ARPGItems.Item.WoodenStaff, // 木制法杖 (普通品质)
                        Position = new(3300, 3200, 0),
                        TriggerGetter = true,
                        UniqueId = 30,
                    }
                },
                {
                    31, new PlacedItem()
                    {
                        Link = ARPGItems.Item.CrystalStaff, // 水晶法杖 (史诗品质)
                        Position = new(3400, 3200, 0),
                        TriggerGetter = true,
                        UniqueId = 31,
                    }
                },

                // ========== 锤子类武器测试 🔨 ==========
                {
                    40, new PlacedItem()
                    {
                        Link = ARPGItems.Item.IronHammer, // 铁制锤子 (稀有品质)
                        Position = new(2900, 2600, 0),
                        TriggerGetter = true,
                        UniqueId = 40,
                    }
                },
                {
                    41, new PlacedItem()
                    {
                        Link = ARPGItems.Item.DragonHammer, // 龙纹锤子 (史诗品质)
                        Position = new(3000, 2600, 0),
                        TriggerGetter = true,
                        UniqueId = 41,
                    }
                },

                // ========== 护甲类装备测试 🛡️ ==========
                {
                    50, new PlacedItem()
                    {
                        Link = ARPGItems.Item.LeatherArmor, // 皮革护甲 (普通品质)
                        Position = new(2700, 3100, 0),
                        TriggerGetter = true,
                        UniqueId = 50,
                    }
                },
                {
                    51, new PlacedItem()
                    {
                        Link = ARPGItems.Item.DragonArmor, // 龙鳞护甲 (史诗品质)
                        Position = new(2800, 3100, 0),
                        TriggerGetter = true,
                        UniqueId = 51,
                    }
                },

                // ========== 饰品类装备测试 💎 ==========
                {
                    60, new PlacedItem()
                    {
                        Link = ARPGItems.Item.GoldNecklace, // 黄金项链 (稀有品质)
                        Position = new(3100, 3300, 0),
                        TriggerGetter = true,
                        UniqueId = 60,
                    }
                },
                {
                    61, new PlacedItem()
                    {
                        Link = ARPGItems.Item.DiamondRing, // 钻石戒指 (史诗品质)
                        Position = new(3200, 3300, 0),
                        TriggerGetter = true,
                        UniqueId = 61,
                    }
                },

                // ========== 消耗品区域 🧪 ==========
                {
                    70, new PlacedItem()
                    {
                        Link = ARPGItems.Item.HealthPotion, // 生命药剂 (恢复道具)
                        Position = new(2500, 3300, 0),
                        TriggerGetter = true,
                        UniqueId = 70,
                    }
                },
                {
                    71, new PlacedItem()
                    {
                        Link = ARPGItems.Item.StrengthPotion, // 力量药剂 (增益道具)
                        Position = new(2600, 3300, 0),
                        TriggerGetter = true,
                        UniqueId = 71,
                    }
                },

                // ========== 任务道具区域 📋 ==========
                {
                    80, new PlacedItem()
                    {
                        Link = ARPGItems.Item.MysteriousOrb, // 神秘法球 (任务道具)
                        Position = new(3500, 3300, 0),
                        TriggerGetter = true,
                        UniqueId = 80,
                    }
                }
            }
        };

        Game.Logger.LogInformation("✅ ARPG Scene configured successfully!");
        Game.Logger.LogInformation("🎁 测试物品已放置在地面:");
        Game.Logger.LogInformation("   🗡️  剑类武器区域 (3400-3500, 2700): 铁剑, 龙纹剑, 传说之剑");
        Game.Logger.LogInformation("   🔫  手枪类武器区域 (2600, 2700-2800): 基础手枪, 史诗手枪");
        Game.Logger.LogInformation("   🪄  法杖类武器区域 (3300-3400, 3200): 木制法杖, 水晶法杖");
        Game.Logger.LogInformation("   🔨  锤子类武器区域 (2900-3000, 2600): 铁制锤子, 龙纹锤子");
        Game.Logger.LogInformation("   🛡️  护甲装备区域 (2700-2800, 3100): 皮革护甲, 龙鳞护甲");
        Game.Logger.LogInformation("   💎  饰品装备区域 (3100-3200, 3300): 黄金项链, 钻石戒指");
        Game.Logger.LogInformation("   🧪  消耗品区域 (2500-2600, 3300): 生命药剂, 力量药剂");
        Game.Logger.LogInformation("   📋  任务道具区域 (3500, 3300): 神秘法球");
    }

    /// <summary>
    /// 初始化ARPG背包系统配置
    /// </summary>
    private static void InitializeInventories()
    {
        Game.Logger.LogInformation("🎒 Configuring ARPG Inventory System...");

        // ========== 英雄主背包配置 ==========
        _ = new GameDataInventory(Inventory.HeroMainInventory)
        {
            Name = "英雄主背包",
            Slots = [
                new (), // 第1格
                new (), // 第2格
                new (), // 第3格
                new (), // 第4格
                new (), // 第5格
                new (), // 第6格
                new (), // 第7格
                new (), // 第8格
                new (), // 第9格
                new (), // 第10格
                new (), // 第11格
                new (), // 第12格
            ]
        };

        // ========== 剑客装备背包配置 ==========
        _ = new GameDataInventory(Inventory.SwordsmanEquipInventory)
        {
            Name = "剑客装备栏",
            InventoryFlags = new()
            {
                HandlePickUpRequest = false // 装备栏不自动拾取，需要手动装备
            },
            Slots = [
                new () // 武器槽 (槽位0) - 只能放剑类武器
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.WeaponSword], // ⚔️ 只能放剑类
                    // DisallowItemWithFailedRequirement = true
                },
                new () // 护甲槽 (槽位1)
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.Armor], // 🛡️ 护甲类
                    // DisallowItemWithFailedRequirement = true
                },
                new () // 饰品槽 (槽位2)
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.Accessory], // 💎 饰品类
                    // DisallowItemWithFailedRequirement = true
                },
            ]
        };

        // ========== 枪手装备背包配置 ==========
        _ = new GameDataInventory(Inventory.GunnerEquipInventory)
        {
            Name = "枪手装备栏",
            InventoryFlags = new()
            {
                HandlePickUpRequest = false
            },
            Slots = [
                new () // 武器槽 (槽位0) - 只能放手枪类武器
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.WeaponGun], // 🔫 只能放手枪类
                    // DisallowItemWithFailedRequirement = true
                },
                new () // 护甲槽 (槽位1)
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.Armor], // 🛡️ 护甲类
                    // DisallowItemWithFailedRequirement = true
                },
                new () // 饰品槽 (槽位2)
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.Accessory], // 💎 饰品类
                    // DisallowItemWithFailedRequirement = true
                },
            ]
        };

        // ========== 法师装备背包配置 ==========
        _ = new GameDataInventory(Inventory.MageEquipInventory)
        {
            Name = "法师装备栏",
            InventoryFlags = new()
            {
                HandlePickUpRequest = false
            },
            Slots = [
                new () // 武器槽 (槽位0) - 只能放法杖类武器
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.WeaponStaff], // 🪄 只能放法杖类
                    // DisallowItemWithFailedRequirement = true
                },
                new () // 护甲槽 (槽位1)
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.Armor], // 🛡️ 护甲类
                    // DisallowItemWithFailedRequirement = true
                },
                new () // 饰品槽 (槽位2)
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.Accessory], // 💎 饰品类
                    // DisallowItemWithFailedRequirement = true
                },
            ]
        };

        // ========== 战士装备背包配置 ==========
        _ = new GameDataInventory(Inventory.WarriorEquipInventory)
        {
            Name = "战士装备栏",
            InventoryFlags = new()
            {
                HandlePickUpRequest = false
            },
            Slots = [
                new () // 武器槽 (槽位0) - 只能放锤子类武器
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.WeaponHammer], // 🔨 只能放锤子类
                    // DisallowItemWithFailedRequirement = true
                },
                new () // 护甲槽 (槽位1)
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.Armor], // 🛡️ 护甲类
                    // DisallowItemWithFailedRequirement = true
                },
                new () // 饰品槽 (槽位2)
                {
                    Type = ItemSlotType.Equip,
                    Required = [ARPGItemCategory.Accessory], // 💎 饰品类
                    // DisallowItemWithFailedRequirement = true
                },
            ]
        };

        Game.Logger.LogInformation("✅ ARPG Inventory System configured successfully!");
        Game.Logger.LogInformation("   - 主背包: 12格物品存储");
        Game.Logger.LogInformation("   - 装备栏: 4个职业专属装备背包");
        Game.Logger.LogInformation("   - 🎯 槽位限制: 职业专属武器分类系统启用");
        Game.Logger.LogInformation("     ⚔️ 剑客武器槽: 只能放剑类武器");
        Game.Logger.LogInformation("     🔫 枪手武器槽: 只能放手枪类武器");
        Game.Logger.LogInformation("     🪄 法师武器槽: 只能放法杖类武器");
        Game.Logger.LogInformation("     🔨 战士武器槽: 只能放锤子类武器");
        Game.Logger.LogInformation("     🛡️ 护甲槽: 通用护甲类 (所有职业)");
        Game.Logger.LogInformation("     💎 饰品槽: 通用饰品类 (所有职业)");
    }

    /// <summary>
    /// 初始化ARPG升级系统配置
    /// </summary>
    private static void InitializeUnitLeveling()
    {
        Game.Logger.LogInformation("⬆️ Configuring ARPG Unit Leveling System...");

        // ========== ARPG英雄升级系统配置 ==========
        _ = new GameDataUnitLeveling(UnitLeveling.HeroLeveling)
        {
            Name = "ARPG英雄升级系统",
            // 配置10级升级系统：1级0经验，2级100经验，3级300经验...
            ExperienceRequiredForEachLevel = [0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500],
            
            // 每级属性加成配置 - 每级增加5点攻击力  
            Modifications = new()
            {
                new() { 
                    Property = UnitProperty.AttackDamage, 
                    SubType = PropertySubType.Base, 
                    Value = (context) => 5.0 // 每级增加5点攻击力
                }
            }
        };

        Game.Logger.LogInformation("✅ ARPG Unit Leveling System configured successfully!");
        Game.Logger.LogInformation("   - 英雄: 可升到10级，每级+5攻击力");
        Game.Logger.LogInformation("   - 经验需求: 1级0经验 → 10级4500经验");
    }

    /// <summary>
    /// 初始化ARPG扩展单位属性
    /// </summary>
    private static void InitializeARPGUnitProperties()
    {
        Game.Logger.LogInformation("🎯 Initializing ARPG Extended Unit Properties...");

        // ========== 冷却缩减属性配置 ==========
        _ = new GameDataUnitProperty(ARPGUnitPropertyLink.CooldownReduction)
        {
            Name = "冷却缩减率",
        };

        Game.Logger.LogInformation("✅ ARPG Extended Unit Properties initialized successfully!");
        Game.Logger.LogInformation("   - 冷却缩减率: 扩展属性ID=2001，范围0.0-1.0 (0%-100%缩减)");
    }

    /// <summary>
    /// 初始化ARPG粒子系统
    /// </summary>
    private static void InitializeParticles()
    {
        Game.Logger.LogInformation("✨ Initializing ARPG Particle System...");

        // ========== 范围指示器粒子配置 ==========
        _ = new GameDataParticle(Particle.RangeParticle)
        {
            Name = "ARPG范围指示器粒子",
            Asset = "effect/effect_new/effect_zhishiqi/eff_zhishiqi/ps_yuanxing_1/particle.effect"u8,
            Radius = 70,
        };

        // ========== AOE区域指示器粒子配置 ==========
        _ = new GameDataParticle(Particle.AOEParticle)
        {
            Name = "ARPGAOE区域指示器粒子",
            Asset = "effect/effect_new/effect_zhishiqi/eff_zhishiqi/ps_jineng_1/particle.effect"u8,
            Radius = 66,
        };

        // ========== 预瞄准圆圈粒子配置 ==========
        _ = new GameDataParticle(Particle.PreTargetingCircle)
        {
            Name = "ARPG预瞄准圆圈粒子",
            Asset = "effect/effect_new/effect_guanghuan/eff_boss_guanghuan/particle.effect"u8,
            AssetLayerScale = 0.4f,
            Radius = 51.2f,
        };

        // ========== 直线指示器相关粒子配置 ==========
        _ = new GameDataParticle(Particle.LineLaunchParticle)
        {
            Name = "ARPG直线发射粒子",
            Asset = "effect/effect_new/effect_zhishiqi/eff_juxing/ps_wei_1/particle.effect"u8,
            Volume = new(128, 64, 128),
        };

        _ = new GameDataParticle(Particle.LineCenterParticle)
        {
            Name = "ARPG直线中心粒子",
            Asset = "effect/effect_new/effect_zhishiqi/eff_juxing/ps_zhong_1/particle.effect"u8,
            Volume = new(128, 64, 128),
        };

        _ = new GameDataParticle(Particle.LineImpactParticle)
        {
            Name = "ARPG直线撞击粒子",
            Asset = "effect/effect_new/effect_zhishiqi/eff_zhixianxing/ps_tou_1/particle.effect"u8,
            Volume = new(128, 64, 128),
        };

        // ========== 技能释放特效粒子配置 ==========
        _ = new GameDataParticle(Particle.CastEffectParticle)
        {
            Name = "ARPG技能释放特效粒子",
            Asset = "effect/effect_new/effect_zhishiqi/eff_juxing/ps_wei_1/particle.effect"u8,
            Volume = new(128, 64, 128),
        };

        // ========== 技能命中特效粒子配置 ==========
        _ = new GameDataParticle(Particle.HitEffectParticle)
        {
            Name = "ARPG技能命中特效粒子",
            Asset = "effect/effect_new/effect_zhishiqi/eff_zhixianxing/ps_tou_1/particle.effect"u8,
            Volume = new(128, 64, 128),
        };

        Game.Logger.LogInformation("✅ ARPG Particle System initialized successfully!");
        Game.Logger.LogInformation("   - 范围指示器粒子: 显示技能有效范围");
        Game.Logger.LogInformation("   - AOE区域指示器粒子: 显示范围攻击区域");
        Game.Logger.LogInformation("   - 预瞄准圆圈粒子: 显示圆形瞄准区域");
        Game.Logger.LogInformation("   - 直线发射粒子: 显示直线起点");
        Game.Logger.LogInformation("   - 直线中心粒子: 显示直线中段");
        Game.Logger.LogInformation("   - 直线撞击粒子: 显示直线终点");
        Game.Logger.LogInformation("   - 技能释放特效粒子: 显示技能释放效果");
        Game.Logger.LogInformation("   - 技能命中特效粒子: 显示技能命中效果");
    }

    /// <summary>
    /// 初始化ARPG Actor系统
    /// </summary>
    private static void InitializeActors()
    {
        Game.Logger.LogInformation("🎭 Initializing ARPG Actor System...");

        // ========== 范围指示器Actor配置 ==========
        _ = new GameDataActorParticle(Actor.RangeActor)
        {
            Name = "ARPG范围指示器Actor",
            Particle = Particle.RangeParticle,
        };

        // ========== AOE区域指示器Actor配置 ==========
        _ = new GameDataActorParticle(Actor.AOEActor)
        {
            Name = "ARPGAOE区域指示器Actor",
            Particle = Particle.AOEParticle,
        };

        // ========== 预瞄准圆圈Actor配置 ==========
        _ = new GameDataActorParticle(Actor.PreTargetingCircle)
        {
            Name = "ARPG预瞄准圆圈Actor",
            Particle = Particle.PreTargetingCircle,
        };

        // ========== 直线指示器子Actor配置 ==========
        _ = new GameDataActorParticle(Actor.LineLaunchActor)
        {
            Name = "ARPG直线发射Actor",
            Particle = Particle.LineLaunchParticle,
        };

        _ = new GameDataActorParticle(Actor.LineCenterActor)
        {
            Name = "ARPG直线中心Actor",
            Particle = Particle.LineCenterParticle,
        };

        _ = new GameDataActorParticle(Actor.LineImpactActor)
        {
            Name = "ARPG直线撞击Actor",
            Particle = Particle.LineImpactParticle,
        };

        // ========== 技能释放特效Actor配置 ==========
        _ = new GameDataActorParticle(Actor.CastEffectActor)
        {
            Name = "ARPG技能释放特效Actor",
            Particle = Particle.CastEffectParticle,
        };

        // ========== 直线指示器Actor配置 ==========
        _ = new GameDataActorSegmentedRectangle(Actor.LineSegment)
        {
            Name = "ARPG直线指示器Actor",
            HeightTest = ActorSegmentedHeight.LaunchSiteGround,
            LaunchActor = Actor.LineLaunchActor,     // 发射点特效
            CenterActor = Actor.LineCenterActor,     // 中心段特效
            ImpactActor = Actor.LineImpactActor,     // 击中点特效
            SegmentedFlags = new()
            {
                // DebugDraw = true,  // 可以启用来调试显示
            }
        };

        // ========== 预瞄准高亮Actor配置 ==========
        _ = new GameDataActorHighlight(Actor.PreTargetingHighlight)
        {
            Name = "ARPG预瞄准高亮Actor",
            From = new()
            {
                Value = new(255, 255, 0)  // 黄色
            },
            To = new()
            {
                Value = new(255, 128, 0)  // 橙色
            },
            AutoReverse = true,
            // RepeatBehavior = RepeatBehavior.Forever,  // 暂时注释，找不到此类型
            Duration = TimeSpan.FromSeconds(0.5),
        };

        Game.Logger.LogInformation("✅ ARPG Actor System initialized successfully!");
        Game.Logger.LogInformation("   - 范围指示器Actor: 显示技能有效范围，连接到RangeParticle");
        Game.Logger.LogInformation("   - AOE区域指示器Actor: 显示范围攻击区域，连接到AOEParticle");
        Game.Logger.LogInformation("   - 预瞄准圆圈Actor: 显示圆形瞄准区域，连接到PreTargetingCircle");
        Game.Logger.LogInformation("   - 直线发射Actor: 直线起点特效，连接到LineLaunchParticle");
        Game.Logger.LogInformation("   - 直线中心Actor: 直线中段特效，连接到LineCenterParticle");
        Game.Logger.LogInformation("   - 直线撞击Actor: 直线终点特效，连接到LineImpactParticle");
        Game.Logger.LogInformation("   - 技能释放特效Actor: 显示技能释放效果，连接到CastEffectParticle");
        Game.Logger.LogInformation("   - 直线指示器Actor: 分段矩形指示器，包含发射-中心-撞击三部分");
        Game.Logger.LogInformation("   - 预瞄准高亮Actor: 高亮显示目标，黄色到橙色渐变");
    }

    /// <summary>
    /// 初始化ARPG指示器系统
    /// </summary>
    private static void InitializeTargetingIndicators()
    {
        Game.Logger.LogInformation("🎯 Initializing ARPG Targeting Indicators...");

        // ========== 完整测试指示器配置 ==========
        _ = new GameDataTargetingIndicator(TargetingIndicator.TestTargetingIndicator)
        {
            Name = "ARPG完整测试指示器",
            CursorActors = [Actor.AOEActor],
            RangeActors = [Actor.RangeActor],
            VectorLineActors = [Actor.LineSegment],
            PreTargetingHighlightActors = [
                Actor.PreTargetingHighlight,
                Actor.PreTargetingCircle,
            ]
        };

        // ========== 直线指示器配置 ==========
        _ = new GameDataTargetingIndicator(TargetingIndicator.LineIndicator)
        {
            Name = "ARPG直线指示器",
            RangeActors = [Actor.RangeActor],
            VectorLineActors = [Actor.LineSegment],
            PreTargetingHighlightActors = [
                Actor.PreTargetingHighlight,
                Actor.PreTargetingCircle,
            ]
        };

        // ========== 区域指示器配置 ==========
        _ = new GameDataTargetingIndicator(TargetingIndicator.AreaIndicator)
        {
            Name = "ARPG区域指示器",
            CursorActors = [Actor.AOEActor],
            RangeActors = [Actor.RangeActor],
            PreTargetingHighlightActors = [
                Actor.PreTargetingHighlight,
                Actor.PreTargetingCircle,
            ]
        };

        Game.Logger.LogInformation("✅ ARPG Targeting Indicators initialized successfully!");
        Game.Logger.LogInformation("   - 完整测试指示器: 包含所有指示器功能的综合测试器");
        Game.Logger.LogInformation("   - 直线指示器: 用于技能瞄准，支持范围和直线显示");
        Game.Logger.LogInformation("   - 区域指示器: 用于范围技能，支持AOE区域显示");
    }

    /// <summary>
    /// 初始化ARPG AI系统
    /// </summary>
    private static void InitializeAI()
    {
        Game.Logger.LogInformation("🤖 Initializing ARPG AI System...");

        // ========== 创建ARPG怪物战斗行为树 ==========
        var monsterCombatBehavior = new GameLink<GameDataAINode, GameDataAINodeSequence>("ARPGMonsterCombatBehavior"u8);
        
        // 子节点1：扫描敌人节点
        var monsterScanEnemies = new GameLink<GameDataAINode, GameDataAINodeValidateScan>("ARPGMonsterScanEnemies"u8);
        _ = new GameDataAINodeValidateScan(monsterScanEnemies)
        {
            // 不忽略牵引限制，怪物有活动范围限制
            IgnoreLeash = false,
        };
        
        // 子节点2：对扫描目标施法 - 使用怪物的攻击技能
        var monsterCastAtTarget = new GameLink<GameDataAINode, GameDataAINodeValidateCast>("ARPGMonsterCastAtTarget"u8);
        _ = new GameDataAINodeValidateCast(monsterCastAtTarget)
        {
            DoRecast = true, // 允许重复施法攻击
        };
        
        // 怪物行为树：序列节点（扫描敌人 -> 攻击目标）
        _ = new GameDataAINodeSequence(monsterCombatBehavior)
        {
            // 🐺 怪物AI行为：先扫描敌人，然后攻击目标
            Nodes = [monsterScanEnemies, monsterCastAtTarget],
        };

        // ========== 战斗测试AI配置 ==========
        _ = new GameDataAIThinkTree(AI.CombatTest)
        {
            Name = "ARPG战斗测试AI",
            ScanFilters = [new() {
                Required=[UnitRelationship.Enemy, UnitFilter.Unit, UnitRelationship.Visible],
                Excluded=[UnitFilter.Item, UnitState.Invulnerable, UnitState.Dead]
            }],
            CombatBehaviorTree = monsterCombatBehavior, // 使用相同的行为树
        };

        // ========== 怪物AI配置 ==========
        _ = new GameDataAIThinkTree(AI.MonsterAI)
        {
            Name = "ARPG怪物AI",
            // 🎯 扫描过滤器：寻找可见的敌方单位，排除物品、无敌、死亡状态
            ScanFilters = [new() {
                Required=[UnitRelationship.Enemy, UnitFilter.Unit, UnitRelationship.Visible],
                Excluded=[UnitFilter.Item, UnitState.Invulnerable, UnitState.Dead]
            }],
            // 🎯 优先级排序：优先攻击英雄，然后是普通单位
            ScanSorts = [UnitFilter.Hero, UnitFilter.Unit],
            // 🤖 使用怪物专用的战斗行为树
            CombatBehaviorTree = monsterCombatBehavior,
        };

        // ========== Boss AI配置 ==========
        _ = new GameDataAIThinkTree(AI.BossAI)
        {
            Name = "ARPG Boss AI",
            // 高级Boss行为：技能释放、阶段变换、特殊攻击模式
        };

        // ========== 跟随AI配置 ==========
        _ = new GameDataAIThinkTree(AI.FollowAI)
        {
            Name = "ARPG跟随AI",
            // NPC跟随玩家：保持距离、协助战斗、避开障碍
        };

        // ========== 巡逻AI配置 ==========
        _ = new GameDataAIThinkTree(AI.PatrolAI)
        {
            Name = "ARPG巡逻AI",
            // 守卫巡逻：定点巡逻、警戒、发现入侵者后报警
        };

        Game.Logger.LogInformation("✅ ARPG AI System initialized successfully!");
        Game.Logger.LogInformation("   - 战斗测试AI: 独立的战斗逻辑系统");
        Game.Logger.LogInformation("   - 怪物AI: 标准怪物行为AI");
        Game.Logger.LogInformation("   - Boss AI: 高级Boss行为AI");
        Game.Logger.LogInformation("   - 跟随AI: NPC跟随玩家AI");
        Game.Logger.LogInformation("   - 巡逻AI: 守卫巡逻行为AI");
    }

    /// <summary>
    /// 初始化ARPG镜头系统
    /// </summary>
    private static void InitializeCameras()
    {
        Game.Logger.LogInformation("📹 Initializing ARPG Camera System...");

        // ========== 默认镜头配置 ==========
        _ = new GameDataCamera(Camera.DefaultCamera)
        {
            Name = "ARPG默认镜头",
            TargetZOffset = 10,
            Rotation = new(-90, -70, 0),
            TargetX = 2500,
            TargetY = 2500,
            FollowMainUnitByDefault = true,
            DisplayDebugInfo = true,
        };

        // ========== 战斗镜头配置 ==========
        _ = new GameDataCamera(Camera.CombatCamera)
        {
            Name = "ARPG战斗镜头",
            TargetZOffset = 8,  // 更近的镜头距离，突出战斗紧张感
            Rotation = new(-90, -65, 0),  // 稍微低一点的角度
            TargetX = 3000,
            TargetY = 3000,
            FollowMainUnitByDefault = true,
            DisplayDebugInfo = false,  // 战斗时不显示调试信息
            FieldOfView = new(60),  // 更宽的视野角度
        };

        // ========== 探索镜头配置 ==========
        _ = new GameDataCamera(Camera.ExplorationCamera)
        {
            Name = "ARPG探索镜头",
            TargetZOffset = 15,  // 更远的镜头距离，便于探索环境
            Rotation = new(-90, -75, 0),  // 更高的角度，俯视感更强
            TargetX = 3000,
            TargetY = 3000,
            FollowMainUnitByDefault = true,
            DisplayDebugInfo = false,
            FieldOfView = new(50),  // 较窄的视野，聚焦于角色
            FarClipPlane = 150.0f,  // 更远的裁剪平面，看到更远的景物
        };

        Game.Logger.LogInformation("✅ ARPG Camera System initialized successfully!");
        Game.Logger.LogInformation("   - 默认镜头: ARPG专用标准镜头配置");
        Game.Logger.LogInformation("   - 战斗镜头: 专为激烈战斗场景优化的镜头");
        Game.Logger.LogInformation("   - 探索镜头: 用于地图探索和任务进行的镜头");
    }

    /// <summary>
    /// 初始化ARPG技能系统
    /// </summary>
    private static void InitializeAbilities()
    {
        Game.Logger.LogInformation("⚡ Initializing ARPG Ability System...");

        // ========== 测试技能配置 ==========
        _ = new GameDataAbility(Ability.TestSpell)
        {
            Name = "ARPG测试技能",
            // 可以在此配置技能属性、效果等
        };

        Game.Logger.LogInformation("✅ ARPG Ability System initialized successfully!");
        Game.Logger.LogInformation("   - 测试技能: 用于调试和测试的独立技能");
    }
}
