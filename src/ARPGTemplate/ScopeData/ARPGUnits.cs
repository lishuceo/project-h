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

namespace GameEntry.ARPGTemplate.ScopeData;

/// <summary>
/// ARPG单位系统数据配置
/// 包含单位、模型、Actor、粒子效果等相关配置
/// </summary>
public class ARPGUnits : IGameClass
{
    #region 单位定义
    public static class Unit
    {
        public static readonly GameLink<GameDataUnit, GameDataUnit> SwordsmanHero = new("SwordsmanHero"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> WerewolfEnemy = new("WerewolfEnemy"u8); // 狼人敌人
        public static readonly GameLink<GameDataUnit, GameDataUnit> SlimeEnemy = new("SlimeEnemy"u8); // 史莱姆敌人
        public static readonly GameLink<GameDataUnit, GameDataUnit> SpiderEnemy = new("SpiderEnemy"u8); // 蜘蛛敌人
        // 新增英雄单位
        public static readonly GameLink<GameDataUnit, GameDataUnit> GunnerHero = new("GunnerHero"u8); // 枪手英雄
        public static readonly GameLink<GameDataUnit, GameDataUnit> MageHero = new("MageHero"u8); // 法师英雄
        public static readonly GameLink<GameDataUnit, GameDataUnit> WarriorHero = new("WarriorHero"u8); // 战士英雄
    }
    #endregion

    #region 模型定义
    public static class Model
    {
        public static readonly GameLink<GameDataModel, GameDataModel> SwordsmanModel = new("SwordsmanModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> SwordsmanWeapon = new("SwordsmanWeapon"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> WerewolfModel = new("WerewolfModel"u8); // 狼人模型
        public static readonly GameLink<GameDataModel, GameDataModel> SlimeModel = new("SlimeModel"u8); // 史莱姆模型
        public static readonly GameLink<GameDataModel, GameDataModel> SpiderModel = new("SpiderModel"u8); // 蜘蛛模型
        // 新增英雄模型
        public static readonly GameLink<GameDataModel, GameDataModel> GunnerModel = new("GunnerModel"u8); // 枪手模型
        public static readonly GameLink<GameDataModel, GameDataModel> GunnerWeapon = new("GunnerWeapon"u8); // 枪手武器模型
        public static readonly GameLink<GameDataModel, GameDataModel> MageModel = new("MageModel"u8); // 法师模型
        public static readonly GameLink<GameDataModel, GameDataModel> MageStaff = new("MageStaff"u8); // 法师法杖模型
        public static readonly GameLink<GameDataModel, GameDataModel> WarriorModel = new("WarriorModel"u8); // 战士模型
        public static readonly GameLink<GameDataModel, GameDataModel> WarriorWeapon = new("WarriorWeapon"u8); // 战士武器模型
    }
    #endregion

    #region Actor定义
    public static class Actor
    {
        public static readonly GameLink<GameDataActor, GameDataActorParticle> UnitsSlashEffect = new("UnitsSlashEffect"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> UnitsDashEffect = new("UnitsDashEffect"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> UnitsCrushingBlowEffect = new("UnitsCrushingBlowEffect"u8);
    }
    #endregion

    #region 粒子效果定义
    public static class Particle
    {
        public static readonly GameLink<GameDataParticle, GameDataParticle> SlashParticle = new("SlashParticle"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> DashParticle = new("DashParticle"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> CrushingBlowParticle = new("CrushingBlowParticle"u8);
    }
    #endregion

    public static void OnRegisterGameClass()
    {
        // 先移除可能已存在的订阅，避免重复注册
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    /// <summary>
    /// 初始化ARPG单位系统数据
    /// </summary>
    private static void OnGameDataInitialization()
    {
        // 只在ARPG模式下初始化
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.ARPGMode)
        {
            return;
        }

        Game.Logger.LogInformation("🛡️ Initializing ARPG Units System for ARPG Mode...");

        // ========== 模型配置 ==========
        _ = new GameDataModel(Model.SwordsmanModel)
        {
            Name = "剑客模型",
            Radius = 50,
            // Asset = "characters/general/sk_basic2/model.prefab"u8,
            Asset = "characters1/baiyijianke_e4wa/model.prefab"u8,
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "sword_idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "sword_move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "sword_attack"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "sword_skill1"u8, AnimationAlias = "skill1"u8 },
                new() { AnimationRaw = "sword_skill2"u8, AnimationAlias = "skill2"u8 }
            ]
        };

        _ = new GameDataModel(Model.SwordsmanWeapon)
        {
            Name = "剑客武器模型",
            Radius = 10,
            Asset = "eqpt/weapon/sm_dr_sword_04_02/model.prefab"u8,
        };

        _ = new GameDataModel(Model.WerewolfModel)
        {
            Name = "狼人模型",
            Radius = 60,
            Asset = "characters/monster/sk_werewolf/model.prefab"u8,
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "attack"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "death"u8, AnimationAlias = "death"u8 }
            ]
        };

        _ = new GameDataModel(Model.SlimeModel)
        {
            Name = "史莱姆模型",
            Radius = 40,
            Asset = "characters/monster/sm_slm_c/model.prefab"u8,
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "attack"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "death"u8, AnimationAlias = "death"u8 }
            ]
        };

        _ = new GameDataModel(Model.SpiderModel)
        {
            Name = "蜘蛛模型",
            Radius = 55,
            Asset = "characters/monster/sk_spider_burrow/model.prefab"u8,
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "attack"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "death"u8, AnimationAlias = "death"u8 }
            ]
        };

        // ========== 新英雄模型配置 ==========
        _ = new GameDataModel(Model.GunnerModel)
        {
            Name = "枪手模型",
            Radius = 50,
            Asset = "characters1/huoqiangshou_zhwa/model.prefab"u8,
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "attack"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "skill1"u8, AnimationAlias = "skill1"u8 },
                new() { AnimationRaw = "skill2"u8, AnimationAlias = "skill2"u8 }
            ]
        };

        _ = new GameDataModel(Model.GunnerWeapon)
        {
            Name = "枪手武器模型",
            Radius = 15,
            Asset = "eqpt/weapon/sk_strongmale_shooter_weapon_gun_01/model.prefab"u8,
        };

        _ = new GameDataModel(Model.MageModel)
        {
            Name = "法师模型",
            Radius = 50,
            Asset = "characters1/lieren_xggb/model.prefab"u8,
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "magic_attack"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "skill1"u8, AnimationAlias = "skill1"u8 },
                new() { AnimationRaw = "skill2"u8, AnimationAlias = "skill2"u8 }
            ]
        };

        _ = new GameDataModel(Model.MageStaff)
        {
            Name = "法师法杖模型",
            Radius = 15,
            Asset = "eqpt/weapon/sk_standardfemale_mage_02_low_weapon_magic_01/model.prefab"u8,
        };

        _ = new GameDataModel(Model.WarriorModel)
        {
            Name = "战士模型",
            Radius = 50,
            Asset = "characters1/dunzhanshi_x61b/model.prefab"u8,
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "spear_attack"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "skill1"u8, AnimationAlias = "skill1"u8 },
                new() { AnimationRaw = "skill2"u8, AnimationAlias = "skill2"u8 }
            ]
        };

        _ = new GameDataModel(Model.WarriorWeapon)
        {
            Name = "战士武器模型",
            Radius = 20,
            Asset = "eqpt/weapon/sk_strongman_warrior_hammer_01/model.prefab"u8,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(Particle.SlashParticle)
        {
            Name = "挥剑特效",
            Asset = "effect/eff_tongyong/huoqiu_blast/particle.effect"u8,
            Radius = 128
        };

        _ = new GameDataParticle(Particle.DashParticle)
        {
            Name = "冲刺特效",
            Asset = "effect/effect_new/effect_guanghuan/eff_boss_guanghuan/particle.effect"u8,
            AssetLayerScale = 0.3f,
            Radius = 64f,
        };

        _ = new GameDataParticle(Particle.CrushingBlowParticle)
        {
            Name = "痛击特效",
            Asset = "effect/eff_tongyong/huoqiu2/particle.effect"u8,
            Radius = 96
        };

        _ = new GameDataActorParticle(Actor.UnitsSlashEffect)
        {
            Name = "挥剑特效Actor",
            AutoPlay = true,
            Particle = Particle.SlashParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        _ = new GameDataActorParticle(Actor.UnitsDashEffect)
        {
            Name = "冲刺特效Actor",
            AutoPlay = true,
            Particle = Particle.DashParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        _ = new GameDataActorParticle(Actor.UnitsCrushingBlowEffect)
        {
            Name = "痛击特效Actor",
            AutoPlay = true,
            Particle = Particle.CrushingBlowParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 剑客单位配置 ==========
        _ = new GameDataUnit(Unit.SwordsmanHero)
        {
            Name = "剑客英雄",
            AttackableRadius = 50,
            Leveling = ARPGScopeData.UnitLeveling.HeroLeveling, // 🎯 使用ARPG专用的英雄升级系统
            Properties = new() {
                { UnitProperty.LifeMax, 1200 },
                { UnitProperty.ManaMax, 800 },
                { UnitProperty.Armor, 15 },
                { UnitProperty.MagicResistance, 8 },
                { UnitProperty.MoveSpeed, 380 },
                { UnitProperty.TurningSpeed, 1800 },
                { UnitProperty.AttackRange, 200 },
                { UnitProperty.AttackDamage, 80 }, // 基础攻击力，每级增加5点
                { UnitProperty.InventoryPickUpRange, 300 },
                { UnitProperty.LevelMax, 10 }, // 🎯 最大等级设置为10级
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 32,
            DynamicCollisionMask = DynamicCollisionMask.Hero | DynamicCollisionMask.Building | DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            
            // 背包系统配置
            Inventories = [ARPGScopeData.Inventory.HeroMainInventory, ARPGScopeData.Inventory.SwordsmanEquipInventory],
            
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Alliance, "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.MainUnit, "$$spark_core.bloodstrip.MAIN_HERO_NONE.root"u8 },
                },
            },
            
            Model = Model.SwordsmanModel,
            Abilities = [
                ARPGAbilities.Ability.SwordSlash,
                ARPGAbilities.Ability.Dash,
                ARPGAbilities.Ability.CrushingBlow,
            ],
        };

        // ========== 狼人敌人单位配置 ==========
        _ = new GameDataUnit(Unit.WerewolfEnemy)
        {
            Name = "狼人敌人",
            AttackableRadius = 60,
            Properties = new() {
                { UnitProperty.LifeMax, 800 },     // 中等血量
                { UnitProperty.ManaMax, 200 },
                { UnitProperty.Armor, 8 },         // 中等护甲
                { UnitProperty.MagicResistance, 5 },
                { UnitProperty.MoveSpeed, 320 },   // 比剑客稍慢
                { UnitProperty.TurningSpeed, 1200 },
                { UnitProperty.AttackRange, 350 }, // 🎯 搜敌范围350 = 技能范围150 + 200
                { UnitProperty.AttackDamage, 60 }, // 中等攻击力
                { UnitProperty.Sight, 500 },       // 视野范围
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax } // 🔧 修复：添加法力属性配置
            },
            CollisionRadius = 40,
            DynamicCollisionMask = DynamicCollisionMask.Unit | DynamicCollisionMask.Hero,
            Filter = [UnitFilter.Unit], // 普通单位，不是英雄
            DeathRemovalDelay = TimeSpan.FromSeconds(5), // 死亡5秒后移除
            
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Alliance, "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                },
            },
            
            Model = Model.WerewolfModel,
            TacticalAI = ARPGScopeData.AI.MonsterAI, // 🐺 使用专门的怪物AI
            Abilities = [
                ARPGAbilities.Ability.WerewolfMeleeAttack, // 狼人近战攻击技能
            ],
        };

        // ========== 史莱姆敌人单位配置 ==========
        _ = new GameDataUnit(Unit.SlimeEnemy)
        {
            Name = "史莱姆敌人",
            AttackableRadius = 40,
            Properties = new() {
                { UnitProperty.LifeMax, 600 },     // 较低血量
                { UnitProperty.ManaMax, 100 },
                { UnitProperty.Armor, 3 },         // 低护甲
                { UnitProperty.MagicResistance, 2 },
                { UnitProperty.MoveSpeed, 250 },   // 较慢移动速度
                { UnitProperty.TurningSpeed, 900 },
                { UnitProperty.AttackRange, 320 }, // 🎯 搜敌范围320 = 技能范围120 + 200
                { UnitProperty.AttackDamage, 40 }, // 低攻击力
                { UnitProperty.Sight, 300 },       // 较小视野范围
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 30,
            DynamicCollisionMask = DynamicCollisionMask.Unit | DynamicCollisionMask.Hero,
            Filter = [UnitFilter.Unit],
            DeathRemovalDelay = TimeSpan.FromSeconds(3),
            
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Alliance, "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                },
            },
            
            Model = Model.SlimeModel,
            TacticalAI = ARPGScopeData.AI.MonsterAI, // 🟢 使用专门的怪物AI
            Abilities = [
                ARPGAbilities.Ability.SlimeSearchAttack, // 史莱姆被动搜索攻击技能
            ],
        };

        // ========== 蜘蛛敌人单位配置 ==========
        _ = new GameDataUnit(Unit.SpiderEnemy)
        {
            Name = "蜘蛛敌人",
            AttackableRadius = 55,
            Properties = new() {
                { UnitProperty.LifeMax, 500 },     // 较低血量但比史莱姆稍强
                { UnitProperty.ManaMax, 150 },
                { UnitProperty.Armor, 2 },         // 极低护甲
                { UnitProperty.MagicResistance, 3 },
                { UnitProperty.MoveSpeed, 280 },   // 较快移动速度
                { UnitProperty.TurningSpeed, 1000 },
                { UnitProperty.AttackRange, 600 }, // 🎯 搜敌范围600 = 技能范围400 + 200
                { UnitProperty.AttackDamage, 50 }, // 中等攻击力
                { UnitProperty.Sight, 600 },       // 较大视野范围适合远程
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 35,
            DynamicCollisionMask = DynamicCollisionMask.Unit | DynamicCollisionMask.Hero,
            Filter = [UnitFilter.Unit],
            DeathRemovalDelay = TimeSpan.FromSeconds(4),
            
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Alliance, "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                },
            },
            
            Model = Model.SpiderModel,
            TacticalAI = ARPGScopeData.AI.MonsterAI, // 🕷️ 使用专门的怪物AI
            Abilities = [
                ARPGAbilities.Ability.SpiderRangedAttack, // 蜘蛛远程投掷攻击技能
            ],
        };

        // ========== 枪手英雄单位配置 ==========
        _ = new GameDataUnit(Unit.GunnerHero)
        {
            Name = "枪手英雄",
            AttackableRadius = 50,
            Leveling = ARPGScopeData.UnitLeveling.HeroLeveling,
            Properties = new() {
                { UnitProperty.LifeMax, 1000 }, // 中等血量
                { UnitProperty.ManaMax, 600 },
                { UnitProperty.Armor, 10 }, // 较低护甲
                { UnitProperty.MagicResistance, 5 },
                { UnitProperty.MoveSpeed, 420 }, // 较快移动速度
                { UnitProperty.TurningSpeed, 2000 },
                { UnitProperty.AttackRange, 500 }, // 远程攻击
                { UnitProperty.AttackDamage, 70 }, // 中等攻击力
                { UnitProperty.InventoryPickUpRange, 300 },
                { UnitProperty.LevelMax, 10 },
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 32,
            DynamicCollisionMask = DynamicCollisionMask.Hero | DynamicCollisionMask.Building | DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            
            Inventories = [ARPGScopeData.Inventory.HeroMainInventory, ARPGScopeData.Inventory.GunnerEquipInventory],
            
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Alliance, "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.MainUnit, "$$spark_core.bloodstrip.MAIN_HERO_NONE.root"u8 },
                },
            },
            
            Model = Model.GunnerModel,
            Abilities = [
                ARPGAbilities.Ability.Gunshot,
                ARPGAbilities.Ability.Roll,
                ARPGAbilities.Ability.Bandage,
            ],
        };

        // ========== 法师英雄单位配置 ==========
        _ = new GameDataUnit(Unit.MageHero)
        {
            Name = "法师英雄",
            AttackableRadius = 50,
            Leveling = ARPGScopeData.UnitLeveling.HeroLeveling,
            Properties = new() {
                { UnitProperty.LifeMax, 800 }, // 较低血量
                { UnitProperty.ManaMax, 1200 }, // 高法力值
                { UnitProperty.Armor, 5 }, // 低护甲
                { UnitProperty.MagicResistance, 20 }, // 高魔抗
                { UnitProperty.MoveSpeed, 350 }, // 较慢移动速度
                { UnitProperty.TurningSpeed, 1500 },
                { UnitProperty.AttackRange, 600 }, // 远程攻击
                { UnitProperty.AttackDamage, 50 }, // 低物理攻击，主要靠技能
                { UnitProperty.InventoryPickUpRange, 300 },
                { UnitProperty.LevelMax, 10 },
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 32,
            DynamicCollisionMask = DynamicCollisionMask.Hero | DynamicCollisionMask.Building | DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            
            Inventories = [ARPGScopeData.Inventory.HeroMainInventory, ARPGScopeData.Inventory.MageEquipInventory],
            
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Alliance, "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.MainUnit, "$$spark_core.bloodstrip.MAIN_HERO_NONE.root"u8 },
                },
            },
            
            Model = Model.MageModel,
            Abilities = [
                ARPGAbilities.Ability.Fireball,
                ARPGAbilities.Ability.Meteor,
                ARPGAbilities.Ability.Overload,
            ],
        };

        // ========== 战士英雄单位配置 ==========
        _ = new GameDataUnit(Unit.WarriorHero)
        {
            Name = "战士英雄",
            AttackableRadius = 50,
            Leveling = ARPGScopeData.UnitLeveling.HeroLeveling,
            Properties = new() {
                { UnitProperty.LifeMax, 1400 }, // 高血量
                { UnitProperty.ManaMax, 400 }, // 低法力值
                { UnitProperty.Armor, 20 }, // 高护甲
                { UnitProperty.MagicResistance, 10 },
                { UnitProperty.MoveSpeed, 360 }, // 中等移动速度
                { UnitProperty.TurningSpeed, 1600 },
                { UnitProperty.AttackRange, 180 }, // 近战攻击
                { UnitProperty.AttackDamage, 95 }, // 高攻击力
                { UnitProperty.InventoryPickUpRange, 300 },
                { UnitProperty.LevelMax, 10 },
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 32,
            DynamicCollisionMask = DynamicCollisionMask.Hero | DynamicCollisionMask.Building | DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            
            Inventories = [ARPGScopeData.Inventory.HeroMainInventory, ARPGScopeData.Inventory.WarriorEquipInventory],
            
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Alliance, "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.MainUnit, "$$spark_core.bloodstrip.MAIN_HERO_NONE.root"u8 },
                },
            },
            
            Model = Model.WarriorModel,
            Abilities = [
                ARPGAbilities.Ability.Thrust,
                ARPGAbilities.Ability.Charge,
                ARPGAbilities.Ability.BerserkerRage, // 被动技能
            ],
        };

        Game.Logger.LogInformation("✅ ARPG Units System initialized successfully for ARPG Mode!");
        Game.Logger.LogInformation("   - 剑客单位: SwordsmanHero");
        Game.Logger.LogInformation("   - 狼人敌人: WerewolfEnemy");
        Game.Logger.LogInformation("   - 史莱姆敌人: SlimeEnemy");
        Game.Logger.LogInformation("   - 蜘蛛敌人: SpiderEnemy");
        Game.Logger.LogInformation("   - 🔫 枪手英雄: GunnerHero (火枪手模型) - 射击、翻滚、绷带");
        Game.Logger.LogInformation("   - 🔥 法师英雄: MageHero (猎人法师模型) - 火球术、陨石术、超载");
        Game.Logger.LogInformation("   - ⚔️ 战士英雄: WarriorHero (盾战士模型) - 刺击、突击、越战越勇");
        Game.Logger.LogInformation("   - 专业模型和武器: 已配置完成");
    }
}
