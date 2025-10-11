
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.AISystem.Data;
using GameCore.Components;
using GameCore.CooldownSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.Execution.Data;
using GameCore.ModelAnimation.Data;
using GameCore.PlayerAndUsers.Enum;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.SceneSystem.Data;
using GameCore.SceneSystem.Data.Struct;

using GameData;

using static GameCore.ScopeData;
using static GameEntry.VampireSurvivors3D.PassiveAbilities;

namespace GameEntry;
public class ExtraScopeDataVampire3D :
    IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    public static class Unit
    {
        public static readonly GameLink<GameDataUnit, GameDataUnit> VampireSurvivorHero = new("VampireSurvivorHero"u8);
        // Vampire Survivor Monsters
        public static readonly GameLink<GameDataUnit, GameDataUnit> SmallMonster = new("SmallMonster"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> MediumMonster = new("MediumMonster"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> LargeMonster = new("LargeMonster"u8);
    }

    public static class Model
    {
        // Vampire Survivor Monster Models
        public static readonly GameLink<GameDataModel, GameDataModel> SmallMonsterModel = new("SmallMonsterModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> MediumMonsterModel = new("MediumMonsterModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> LargeMonsterModel = new("LargeMonsterModel"u8);
    }

    public static class Animation
    {
        public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> MonsterAttack = new("MonsterAttack"u8);
    }

    public static class Ability
    {
        // Vampire Survivor Auto-Attack Abilities

        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> MonsterAttack = new("MonsterAttack"u8);
    }

    public static class AI
    {

    }

    public static class Scene
    {
        public static readonly GameLink<GameDataScene, GameDataScene> VampireScene = new("vampire"u8);
    }

    private static void OnGameDataInitialization()
    {
        // Game Instance is not initialized yet, should always use Game.GameModeLink instead of Game.Instance.GameMode here.
        if (Game.GameModeLink != ScopeData.GameMode.VampireSurvivors3D)
        {
            return;
        }
        _ = new GameDataScene(Scene.VampireScene)
        {
            DefaultCamera = ScopeData.Camera.DefaultCamera,
            Name = "Vampire Scene",
            HostedSceneTag = "new_scene"u8,
            Size = new(64 * 256, 64 * 256),
            OnLoaded = static (scene) => Game.Logger.LogInformation("Scene {scene} loaded", scene),
            PlacedPlayerObjects = new()
            {
                {
                    1, new PlacedUnit()
                    {
                        Link = Unit.VampireSurvivorHero, // 修复：使用正确的吸血鬼英雄单位
                        OwnerPlayerId = 1,
                        Position = new(3500,3000,0),
                        IsMainUnit = true,
                        TriggerGetter = true,
                        UniqueId = 1,
                    }
                },
                // Vampire Survivor Monsters - Initial spawn distributed across the larger map
                // Small monsters in multiple waves around the player
                {
                    10, new PlacedUnit()
                    {
                        Link = Unit.SmallMonster,
                        OwnerPlayerId = 4, // Player 4 (Enemy Team 2)
                        Position = new(2500,2500,0),
                        TriggerGetter = true,
                        UniqueId = 10,
                    }
                },
                {
                    11, new PlacedUnit()
                    {
                        Link = Unit.SmallMonster,
                        OwnerPlayerId = 4,
                        Position = new(4500,2500,0),
                        TriggerGetter = true,
                        UniqueId = 11,
                    }
                },
                {
                    12, new PlacedUnit()
                    {
                        Link = Unit.SmallMonster,
                        OwnerPlayerId = 4,
                        Position = new(2500,4500,0),
                        TriggerGetter = true,
                        UniqueId = 12,
                    }
                },
                {
                    13, new PlacedUnit()
                    {
                        Link = Unit.SmallMonster,
                        OwnerPlayerId = 4,
                        Position = new(4500,4500,0),
                        TriggerGetter = true,
                        UniqueId = 13,
                    }
                },
                {
                    14, new PlacedUnit()
                    {
                        Link = Unit.SmallMonster,
                        OwnerPlayerId = 4,
                        Position = new(3250,2000,0),
                        TriggerGetter = true,
                        UniqueId = 14,
                    }
                },
                {
                    15, new PlacedUnit()
                    {
                        Link = Unit.SmallMonster,
                        OwnerPlayerId = 4,
                        Position = new(3750,5000,0),
                        TriggerGetter = true,
                        UniqueId = 15,
                    }
                },
                {
                    16, new PlacedUnit()
                    {
                        Link = Unit.SmallMonster,
                        OwnerPlayerId = 4,
                        Position = new(2000,3250,0),
                        TriggerGetter = true,
                        UniqueId = 16,
                    }
                },
                {
                    17, new PlacedUnit()
                    {
                        Link = Unit.SmallMonster,
                        OwnerPlayerId = 4,
                        Position = new(5000,3750,0),
                        TriggerGetter = true,
                        UniqueId = 17,
                    }
                },
                // Medium monsters in strategic positions
                {
                    20, new PlacedUnit()
                    {
                        Link = Unit.MediumMonster,
                        OwnerPlayerId = 4,
                        Position = new(1500,1500,0),
                        TriggerGetter = true,
                        UniqueId = 20,
                    }
                },
                {
                    21, new PlacedUnit()
                    {
                        Link = Unit.MediumMonster,
                        OwnerPlayerId = 4,
                        Position = new(5500,1500,0),
                        TriggerGetter = true,
                        UniqueId = 21,
                    }
                },
                {
                    22, new PlacedUnit()
                    {
                        Link = Unit.MediumMonster,
                        OwnerPlayerId = 4,
                        Position = new(1500,5500,0),
                        TriggerGetter = true,
                        UniqueId = 22,
                    }
                },
                {
                    23, new PlacedUnit()
                    {
                        Link = Unit.MediumMonster,
                        OwnerPlayerId = 4,
                        Position = new(5500,5500,0),
                        TriggerGetter = true,
                        UniqueId = 23,
                    }
                },
                // Large monsters as boss-like units further away
                {
                    30, new PlacedUnit()
                    {
                        Link = Unit.LargeMonster,
                        OwnerPlayerId = 4,
                        Position = new(1000,1000,0),
                        TriggerGetter = true,
                        UniqueId = 30,
                    }
                },
                {
                    31, new PlacedUnit()
                    {
                        Link = Unit.LargeMonster,
                        OwnerPlayerId = 4,
                        Position = new(6000,1000,0),
                        TriggerGetter = true,
                        UniqueId = 31,
                    }
                },
                {
                    32, new PlacedUnit()
                    {
                        Link = Unit.LargeMonster,
                        OwnerPlayerId = 4,
                        Position = new(1000,6000,0),
                        TriggerGetter = true,
                        UniqueId = 32,
                    }
                },
                {
                    33, new PlacedUnit()
                    {
                        Link = Unit.LargeMonster,
                        OwnerPlayerId = 4,
                        Position = new(6000,6000,0),
                        TriggerGetter = true,
                        UniqueId = 33,
                    }
                },
                // Additional scattered monsters across the map
                {
                    40, new PlacedUnit()
                    {
                        Link = Unit.SmallMonster,
                        OwnerPlayerId = 4,
                        Position = new(8000,3500,0),
                        TriggerGetter = true,
                        UniqueId = 40,
                    }
                },
                {
                    41, new PlacedUnit()
                    {
                        Link = Unit.SmallMonster,
                        OwnerPlayerId = 4,
                        Position = new(3500,8000,0),
                        TriggerGetter = true,
                        UniqueId = 41,
                    }
                },
                {
                    42, new PlacedUnit()
                    {
                        Link = Unit.MediumMonster,
                        OwnerPlayerId = 4,
                        Position = new(8000,8000,0),
                        TriggerGetter = true,
                        UniqueId = 42,
                    }
                },
            }
        };

        _ = new GameDataUnit(Unit.VampireSurvivorHero)
        {
            Name = "吸血鬼幸存者英雄",
            AttackableRadius = 50,
            Properties = new() {
                { UnitProperty.LifeMax, 1000 },
                { UnitProperty.ManaMax, 1000 },
                { UnitProperty.Armor, 10 },
                { UnitProperty.MagicResistance, 10 },
                { UnitProperty.MoveSpeed, 350 }, // 增加移动速度
                { UnitProperty.TurningSpeed, 1800 },
                { UnitProperty.AttackRange, 200 }, // 增加攻击范围
                { UnitProperty.InventoryPickUpRange, 300 }, // 增加拾取范围
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax }
            },
            CollisionRadius = 32,
            DynamicCollisionMask = DynamicCollisionMask.Hero | DynamicCollisionMask.Building,
            Inventories = [ScopeData.Inventory.TestInventory6, ScopeData.Inventory.TestInventory6Equip],
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            // Vampire Survivor passive-abilities
            Abilities = [
                VampireSurvivors3D.PassiveAbilities.Ability.FireballAura,
                VampireSurvivors3D.PassiveAbilities.Ability.LightningChain,
                VampireSurvivors3D.PassiveAbilities.Ability.HealingAura,
            ],
            ActorArray = [
        ScopeData.Actor.TestActorAdditionModel,
            ],
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
            Model = ScopeData.Model.HostTestHero,
        };


        // Vampire Survivor Monster Models
        _ = new GameDataModel(Model.SmallMonsterModel)
        {
            Radius = 40,
            Asset = "characters/monster/sm_slm_a/model.prefab"u8,
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new()
                {
                    AnimationRaw = "idle"u8,
                    AnimationAlias = "idle"u8,
                },
                new()
                {
                    AnimationRaw = "move_02"u8,
                    AnimationAlias = "move"u8,
                },
                new()
                {
                    AnimationRaw = "attack_01"u8,
                    AnimationAlias = "attack"u8,
                },
                new()
                {
                    AnimationRaw = "death"u8,
                    AnimationAlias = "death"u8,
                },
            ]
        };

        _ = new GameDataModel(Model.MediumMonsterModel)
        {
            Radius = 60,
            Asset = "characters/monster/sm_slm_b/model.prefab"u8,
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new()
                {
                    AnimationRaw = "idle"u8,
                    AnimationAlias = "idle"u8,
                },
                new()
                {
                    AnimationRaw = "move_02"u8,
                    AnimationAlias = "move"u8,
                },
                //new()
                //{
                //    AnimationRaw = "attack"u8,
                //    AnimationAlias = "attack"u8,
                //},
                //new()
                //{
                //    AnimationRaw = "death"u8,
                //    AnimationAlias = "death"u8,
                //},
            ]
        };

        _ = new GameDataModel(Model.LargeMonsterModel)
        {
            Radius = 80,
            Asset = "characters/monster/sm_slm_c/model.prefab"u8,
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new()
                {
                    AnimationRaw = "idle"u8,
                    AnimationAlias = "idle"u8,
                },
                new()
                {
                    AnimationRaw = "move_02"u8,
                    AnimationAlias = "move"u8,
                },
                //new()
                //{
                //    AnimationRaw = "attack"u8,
                //    AnimationAlias = "attack"u8,
                //},
                //new()
                //{
                //    AnimationRaw = "death"u8,
                //    AnimationAlias = "death"u8,
                //},
            ]
        };


        // Vampire Survivor Monsters
        _ = new GameDataUnit(Unit.SmallMonster)
        {
            Name = "小怪",
            AttackableRadius = 40,
            Properties = new() {
                { UnitProperty.LifeMax, 50 },
                { UnitProperty.ManaMax, 0 },
                { UnitProperty.Armor, 0 },
                { UnitProperty.MagicResistance, 0 },
                { UnitProperty.MoveSpeed, 200 },
                { UnitProperty.TurningSpeed, 1200 },
                { UnitProperty.AttackRange, 50 },
                { UnitProperty.AttackDamage, 15 },
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax }
            },
            CollisionRadius = 8,
            DynamicCollisionMask = DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Unit],
            Model = Model.SmallMonsterModel,
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_UNIT_NONE.root"u8,
            },
            Abilities = [
                Ability.MonsterAttack,
            ],
            DeathProcedure = new()
            {
                // TODO: 小怪死亡时没有死亡效果，是因为死亡时间太短，导致死亡效果没有播放么？
                Mode = DeathProcedureMode.Disintegrate
            }
        };

        _ = new GameDataUnit(Unit.MediumMonster)
        {
            Name = "中怪",
            AttackableRadius = 60,
            Properties = new() {
                { UnitProperty.LifeMax, 150 },
                { UnitProperty.ManaMax, 0 },
                { UnitProperty.Armor, 5 },
                { UnitProperty.MagicResistance, 5 },
                { UnitProperty.MoveSpeed, 150 },
                { UnitProperty.TurningSpeed, 1000 },
                { UnitProperty.AttackRange, 60 },
                { UnitProperty.AttackDamage, 30 },
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax }
            },
            CollisionRadius = 16,
            DynamicCollisionMask = DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Unit],
            Model = Model.MediumMonsterModel,
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_UNIT_NONE.root"u8,
            },
            Abilities = [
                Ability.MonsterAttack,
            ],
            DeathProcedure = new()
            {
                Mode = DeathProcedureMode.Disintegrate
            }
        };

        _ = new GameDataUnit(Unit.LargeMonster)
        {
            Name = "大怪",
            AttackableRadius = 80,
            Properties = new() {
                { UnitProperty.LifeMax, 400 },
                { UnitProperty.ManaMax, 0 },
                { UnitProperty.Armor, 15 },
                { UnitProperty.MagicResistance, 15 },
                { UnitProperty.MoveSpeed, 100 },
                { UnitProperty.TurningSpeed, 800 },
                { UnitProperty.AttackRange, 80 },
                { UnitProperty.AttackDamage, 60 },
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax }
            },
            CollisionRadius = 32,
            DynamicCollisionMask = DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Unit],
            Model = Model.LargeMonsterModel,
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_UNIT_NONE.root"u8,
            },

            Abilities = [
                Ability.MonsterAttack,
            ],
            DeathProcedure = new()
            {
                Mode = DeathProcedureMode.Disintegrate
            }
        };

        // Vampire Survivor Auto-Attack Abilities
        var meleeAttackCooldown = new GameLink<GameDataCooldown, GameDataCooldownActive>("AutoAttackMelee"u8);
        _ = new GameDataCooldownActive(meleeAttackCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(0.8),
        };

        var rangedAttackCooldown = new GameLink<GameDataCooldown, GameDataCooldownActive>("AutoAttackRanged"u8);
        _ = new GameDataCooldownActive(rangedAttackCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(1.2),
        };

        var circularAttackCooldown = new GameLink<GameDataCooldown, GameDataCooldownActive>("CircularAttack"u8);
        _ = new GameDataCooldownActive(circularAttackCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(3.0),
        };

        // 怪物伤害效果
        var monsterDamageEffect = new GameLink<GameDataEffect, GameDataEffectDamage>("MonsterDamage"u8);
        _ = new GameDataEffectDamage(monsterDamageEffect)
        {
            Amount = static (context) =>
            {
                // 从攻击者获取攻击力
                var attacker = context.Caster;
                return attacker.GetUnitPropertyFinal(UnitProperty.AttackDamage) ?? 0;  // 计算伤害值
            },
            Type = DamageType.Physical,    // 物理伤害
            LogExecutionFailure = true,
        };
        var monsterAttackAnimation = new GameDataAnimationSimple(Animation.MonsterAttack)
        {
            File = "attack"u8,
        };
        // 怪物攻击技能
        var monsterAttackAbility = new GameDataAbilityExecute(Ability.MonsterAttack)
        {
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.5),   // 攻击前摇
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),  // 攻击后摇
            },
            Effect = monsterDamageEffect,  // 🔥 伤害效果
            TargetType = AbilityTargetType.Unit,
            Range = static (e) => (float)(e.Caster.GetUnitPropertyFinal(UnitProperty.AttackRange) ?? 0),      // 攻击范围
            AbilityExecuteFlags = new()
            {
                IsAttack = true,
            },
            Animation = [Animation.MonsterAttack],
        };
    }
}
