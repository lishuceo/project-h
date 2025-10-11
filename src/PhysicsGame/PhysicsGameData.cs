using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.AISystem.Data.Enum;

using GameUI.CameraSystem.Data;

using GameCore.EntitySystem.Data.Enum;
using GameCore.GameSystem.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.SceneSystem.Data.Struct;
using GameCore.TargetingSystem.Data;
using GameCore.ActorSystem.Data.Enum;
using GameCore.Animation.Enum;

using GameUI.Brush;
using GameUI.Control.Data;
using GameUI.Control.Enum;
#if CLIENT
using GameUI.Control.Primitive;
#endif
using GameUI.Enum;

using System.Drawing;

using GameCore.ModelAnimation.Data;
using GameCore.PhysicsSystem.Struct;
using GameCore.PhysicsSystem.Enum;

using static GameCore.ScopeData;


namespace GameEntry;

public class PhysicsGameData : IGameClass
{
    public static class GameMode
    {
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> PhysicsGame = new("PhysicsGame"u8);
    }

    public static class Unit
    {
        public static readonly GameLink<GameDataUnit, GameDataUnit> PhysicsRole = new("PhysicsRole"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> PhysicsSmallMonster = new("PhysicsSmallMonster"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> PhysicsFloor = new("PhysicsFloor"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> PhysicsCube = new("PhysicsCube"u8);
    }

    public static class Scene
    {
        public static readonly GameLink<GameDataScene, GameDataScene> PhysicsScene = new("physics_scene"u8);
    }

    public static class Model
    {
        public static readonly GameLink<GameDataModel, GameDataModel> SmallMonsterModel = new("SmallMonsterModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> MediumMonsterModel = new("MediumMonsterModel"u8);
    }

    public static class Ability
    {
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> MonsterAttack = new("MonsterAttack"u8);
    }

    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        _ = new GameDataGameMode(GameMode.PhysicsGame)
        {
            Name = "Physics Game",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                ScopeData.Scene.DefaultScene,
                Scene.PhysicsScene,
            ],
            DefaultScene = Scene.PhysicsScene,
        };

        _ = new GameDataScene(Scene.PhysicsScene)
        {
            DefaultCamera = ScopeData.Camera.DefaultCamera,
            Name = "Physics Scene",
            HostedSceneTag = "new_scene_3"u8,
            Size = new(16 * 256, 16 * 256),
            OnLoaded = static (scene) => Game.Logger.LogInformation("Scene {scene} loaded", scene),
            PlacedPlayerObjects = new()
            {
                {
                    1, new PlacedUnit()
                    {
                        Link = Unit.PhysicsRole,
                        OwnerPlayerId = 1,
                        Position = new(3500,3000,0),
                        IsMainUnit = true,
                        TriggerGetter = true,
                        UniqueId = 1,
                    }
                },
                {
                    2, new PlacedUnit()
                    {
                        Link = Unit.PhysicsRole,
                        OwnerPlayerId = 3,
                        Position = new(3000,3500,0),
                        IsMainUnit = false,
                        TriggerGetter = true,
                        UniqueId = 2,
                        Facing = -90,
                    }
                },
                {
                    3, new PlacedUnit()
                    {
                        Link = Unit.PhysicsSmallMonster,
                        OwnerPlayerId = 1,
                        Position = new(2800,3500,0),
                        TriggerGetter = true,
                        UniqueId = 4,
                        Facing = -90,
                    }
                },
                {
                    4, new PlacedUnit()
                    {
                        Link = Unit.PhysicsFloor,
                        OwnerPlayerId = 1,
                        Position = new(2500,3500,0),
                        TriggerGetter = true,
                        UniqueId = 5,
                        Facing = -90,
                    }
                }
            }
        };

        _ = new GameDataUnit(Unit.PhysicsRole)
        {
            Name = "物理测试英雄",
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
            Abilities = [ScopeData.Ability.ChargedTestSpell], // 添加新的充能技能
            PhysicsAttributes = new PhysicsAttributes
            {
                CollisionType = CollisionType.Capsule,
                CollisionEventMode = CollisionEventMode.Always,
                CollisionLayer = 1,
                AngularFactor = Vector3.Zero,
                Radius = 64.0f,
                Height = 180.0f,
                Position = new Vector3(0, 0, 90.0f),
            },
        };
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

        _ = new GameDataUnit(Unit.PhysicsSmallMonster)
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
                AllowMover = false,
                Turnable = true,
                Walkable = false,
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
            },
            PhysicsAttributes = new PhysicsAttributes
            {
                CollisionType = CollisionType.Box,
                CollisionEventMode = CollisionEventMode.Active,
                CollisionLayer = 2,
                Radius = 64.0f,
                Position = new Vector3(0.0f, 0.0f, 64.0f),
            }
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
            ]
        };

        _ = new GameDataUnit(Unit.PhysicsFloor)
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
                AllowMover = false,
                Turnable = true,
                Walkable = false,
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
            },
            PhysicsAttributes = new PhysicsAttributes
            {
                CollisionType = CollisionType.Plane,
                CollisionEventMode = CollisionEventMode.Active,
                CollisionLayer = 2,
                Mass = 0.0f,
                Position = new Vector3(64.0f, 64.0f, 0.0f),
            }
        };
    }
}
