using EngineInterface.BaseType;

using GameCore.AbilitySystem.Data;
using GameCore.SceneSystem;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.AISystem.Data;
using GameCore.AISystem.Data.Enum;
using GameCore.BuffSystem.Data;
using GameCore.BuffSystem.Data.Enum;
using GameCore.BuffSystem.Data.Struct;
using GameCore.Localization;

using GameUI.CameraSystem.Data;

using GameCore.Container;
using GameCore.Container.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.Execution.Data;
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

using GameUI.Brush;
using GameUI.Control.Data;
using GameUI.Control.Enum;
#if CLIENT
using GameUI.Control.Primitive;
#endif
using GameUI.Enum;

using System.Drawing;
using System.Numerics; // 添加Vector2支持

using static GameCore.ScopeData;

using GameEntry.TypedMessageTest;

using GameCore.ModelAnimation.Data;
using GameCore;
using GameCore.AbilitySystem.Manager;
using GameCore.EntitySystem;
using GameCore.OrderSystem;
using GameCore.Shape.Data;
using GameCore.PhysicsSystem.Struct;
using GameCore.PhysicsSystem.Enum;
using GameCore.CameraSystem.Enum;
using GameCore.Leveling.Data;
using GameUI.Data;
using GameCore.GameSystem.Enum;
using GameCore.DamageSystem.Data;
using GameEntry.BuildingSystem;
using GameEntry.TowerDefenseGame;
using GameEntry.TowerDefenseGame.SpawnSystem;
using GameCore.CameraSystem.Struct;
using GameCore.CollisionSystem.Data.Struct;

namespace GameEntry;
public partial class ScopeData : IGameClass
{
    public static class Control
    {
        public static readonly GameLink<GameDataControl, GameDataControlButton> TestButton = new("TestButton");
        public static readonly GameLink<GameDataControl, GameDataControlButton> TestButton2 = new("TestButton2");
    }

    public static class GameMode
    {
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> VampireSurvivors3D = new("VampireSurvivors3D"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> VampireSurvivors2D = new("VampireSurvivors2D"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> FlappyBird = new("FlappyBird"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> JoyStickTest = new("JoyStickTest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> GameUITest = new("GameUITest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> TypedMessageTest = new("TypedMessageTest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> Gomoku = new("Gomoku"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> UserCloudDataTest = new("UserCloudDataTest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> AISystemTest = new("AISystemTest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> PrimitiveShapeTest = new("PrimitiveShapeTest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> RPGRoleTest = new("RPGRoleTest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> ModernUITest = new("ModernUITest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> BuffTest = new("BuffTest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> ScriptPerformanceTest = new("ScriptPerformanceTest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> Game2048 = new("Game2048"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> JumpJump = new("JumpJump"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> TouchBehaviorTest = new("TouchBehaviorTest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> UIFrameworkTest = new("UIFrameworkTest"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> TowerDefense = new("TowerDefense"u8);
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> ARPGMode = new("ARPGMode"u8); // ⚔️ ARPG剑客模式
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> UIShowcaseDemo = new("UIShowcaseDemo"u8); // 🎨 UI能力展示Demo
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> XianJianQiXiaZhuan = new("XianJianQiXiaZhuan"u8); // 🗡️ 仙剑奇侠传
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> AVGTest = new("AVGTest"u8); // 📖 AVG系统测试
        public static readonly GameLink<GameDataGameMode, GameDataGameMode> JsonScopeDataTest = new("$p_0tja.ScopeData.GameDataGameMode.TestGameMode.Root"u8);
    }

    public static class Animation
    {
        public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> Idle = new("Idle"u8);
        public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> Move = new("Move"u8);
        public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> Attack = new("Attack"u8);
        public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> Death = new("Death"u8);
        public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> TestSpell = new("TestSpell"u8);
    }

    public static class Camera
    {
        public static readonly GameLink<GameDataCamera, GameDataCamera> DefaultCamera = new("DefaultCamera"u8);
        public static readonly GameLink<GameDataCamera, GameDataCamera> MoveableCamera = new("MoveableCamera"u8);
        public static readonly GameLink<GameDataCamera, GameDataCamera> TowerDefenseCamera = new("TowerDefenseCamera"u8);
    }
    public static class Unit
    {
        public static readonly GameLink<GameDataUnit, GameDataUnit> HostTestHero = new("HostTestHero"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> AICombatTestHero = new("AICombatTestHero"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> TestItem = new("TestItem"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> TestMissileUnit = new("TestMissileUnit"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> TDMonster = new("TDMonster"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> TowerDefenseHero = new("TowerDefenseHero"u8);

        // 形状测试单位
        public static readonly GameLink<GameDataUnit, GameDataUnit> ShapeTestSphere = new("ShapeTestSphere"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> ShapeTestCube = new("ShapeTestCube"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> ShapeTestCylinder = new("ShapeTestCylinder"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> ShapeTestPlane = new("ShapeTestPlane"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> ShapeTestCone = new("ShapeTestCone"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> ShapeTestCapsule = new("ShapeTestCapsule"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> ShapeTestPyramid = new("ShapeTestPyramid"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> CompositeRobot = new("CompositeRobot"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> CastleTower = new("CastleTower"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> Spaceship = new("Spaceship"u8);

        // 物品附属表现测试用单位
        public static readonly GameLink<GameDataUnit, GameDataUnit> TestItemWithActors = new("TestItemWithActors"u8);
        // 塔防建筑单位
        public static readonly GameLink<GameDataUnit, GameDataUnit> SlowTower = new("SlowTower"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> AuraSlowTower = new("AuraSlowTower"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> AOETower = new("AOETower"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> PenetrateTower = new("PenetrateTower"u8);
    }


    public static class Actor
    {
        public static readonly GameLink<GameDataActor, GameDataActorModel> TestActorModel = new("TestActorModel"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> TestActorParticle = new("$TestParticle"u8);
        public static readonly GameLink<GameDataActor, GameDataActorModel> TestActorModelScript = new("TestActorModelScript"u8);
        public static readonly GameLink<GameDataActor, GameDataActorAdditionModel> TestActorAdditionModel = new("TestActorAdditionModel"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> RangeActor = new("RangeActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> AOEActor = new("AOEActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorSegmentedRectangle> LineSegment = new("LineSegment"u8);
        
        // 建造网格Actor
        public static readonly GameLink<GameDataActor, GameDataActorParticle> LineLaunchActor = new("LineLaunchActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> LineCenterActor = new("LineCenterActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> LineImpactActor = new("LineImpactActor"u8);

        // 建造预览Actor
        public static readonly GameLink<GameDataActor, GameDataActorModel> TowerPreviewActor = new("TowerPreviewActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorModel> WallPreviewActor = new("WallPreviewActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorGrid> BuildingValidGrid = new("BuildingValidGrid"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> PreTargetingCircle = new("PreTargetingCircle"u8);
        public static readonly GameLink<GameDataActor, GameDataActorHighlight> PreTargetingHighlight = new("PreTargetingHighlight"u8);
        
        // 四种塔的预览Actor
        public static readonly GameLink<GameDataActor, GameDataActorModel> SlowTowerPreviewActor = new("SlowTowerPreviewActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorModel> AuraSlowTowerPreviewActor = new("AuraSlowTowerPreviewActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorModel> AOETowerPreviewActor = new("AOETowerPreviewActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorModel> PenetrateTowerPreviewActor = new("PenetrateTowerPreviewActor"u8);

        // 物品附属表现测试用Actor
        public static readonly GameLink<GameDataActor, GameDataActorModel> ItemSelfActor = new("ItemSelfActor"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> ItemEquipEffect = new("ItemEquipEffect"u8);
        public static readonly GameLink<GameDataActor, GameDataActorModel> ItemCarrierActor = new("ItemCarrierActor"u8);

    }

    public static class Scene
    {
        public static readonly GameLink<GameDataScene, GameDataScene> DefaultScene = new("default"u8);
        public static readonly GameLink<GameDataScene, GameDataScene> AITestScene = new("ai_test_scene"u8);
        public static readonly GameLink<GameDataScene, GameDataScene> PrimitiveShapeTestScene = new("primitive_shape_test_scene"u8);
        public static readonly GameLink<GameDataScene, GameDataScene> JumpJumpScene = new("jump_jump_scene"u8);
        public static readonly GameLink<GameDataScene, GameDataScene> TowerDefenseScene = new("tower_defense_scene"u8);
        public static readonly GameLink<GameDataScene, GameDataScene> RPGRoleTestScene = new("rpg_role_test_scene"u8);
    }

    public static class Model
    {
        public static readonly GameLink<GameDataModel, GameDataModel> TestModelScript = new("TestActorModelScript"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> TestActorAdditionModel = new("TestActorAdditionModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> HostTestHero = new("HostTestHero"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> TestItem = new("TestItem"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> TestItemWithActors = new("TestItemWithActors"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> ItemEffectModel = new("ItemEffectModel"u8);
        
        // 建造网格模型
        public static readonly GameLink<GameDataModel, GameDataModel> BuildGridModel = new("GeneralBuildGrid.Model"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> BuildGridModel_1 = new("GeneralBuildGrid.Model_1"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> BuildGridModel_2 = new("GeneralBuildGrid.Model_2"u8);
        
        // 塔防建筑模型
        public static readonly GameLink<GameDataModel, GameDataModel> SlowTowerModel = new("SlowTowerModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> AuraSlowTowerModel = new("AuraSlowTowerModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> AOETowerModel = new("AOETowerModel"u8);
        public static readonly GameLink<GameDataModel, GameDataModel> PenetrateTowerModel = new("PenetrateTowerModel"u8);
    }
    
    public static class Particle
    {
        public static readonly GameLink<GameDataParticle, GameDataParticle> RangeParticle = new("RangeParticle"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> AOEParticle = new("AOEParticle"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> TestParticleScript = new("TestParticleScript"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> LineLaunchParticle = new("LineLaunchParticle"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> LineCenterParticle = new("LineCenterParticle"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> LineImpactParticle = new("LineImpactParticle"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> PreTargetingCircle = new("PreTargetingCircle"u8);

        // 物品附属表现测试用粒子效果
        public static readonly GameLink<GameDataParticle, GameDataParticle> ItemEquipParticle = new("ItemEquipParticle"u8);

    }
    public static class Ability
    {
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> TestSpell = new("TestSpell"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> ChargedTestSpell = new("ChargedTestSpell"u8);
        
        
        // 建造技能 - 四种塔防建筑
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecuteBuilding> BuildSlowTower = new("BuildSlowTower"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecuteBuilding> BuildAuraSlowTower = new("BuildAuraSlowTower"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecuteBuilding> BuildAOETower = new("BuildAOETower"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecuteBuilding> BuildPenetrateTower = new("BuildPenetrateTower"u8);
        
        // 塔防攻击技能 - 被动版本 (自动攻击) - 现已移动到 TowerDefense.PassiveAbilities.cs
        // 使用独立的GameLink定义避免循环引用问题
        public static readonly GameLink<GameDataAbility, GameDataAbility> SlowProjectilePassive = new("TowerDefense_SlowProjectilePassive"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbility> AuraSlowPassive = new("TowerDefense_AuraSlowPassive"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbility> AOEDamagePassive = new("TowerDefense_AOEDamagePassive"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbility> PenetrateAttackPassive = new("TowerDefense_PenetrateAttackPassive"u8);
        
    }
    
    public static class BuildingEffect
    {
        // 四种塔防建筑的建造效果
        public static readonly GameLink<GameDataEffect, GameDataBuildingEffect> BuildSlowTowerEffect = new("BuildSlowTowerEffect"u8);
        public static readonly GameLink<GameDataEffect, GameDataBuildingEffect> BuildAuraSlowTowerEffect = new("BuildAuraSlowTowerEffect"u8);
        public static readonly GameLink<GameDataEffect, GameDataBuildingEffect> BuildAOETowerEffect = new("BuildAOETowerEffect"u8);
        public static readonly GameLink<GameDataEffect, GameDataBuildingEffect> BuildPenetrateTowerEffect = new("BuildPenetrateTowerEffect"u8);
    }
    public static class Item
    {
        public static readonly GameLink<GameDataItem, GameDataItemMod> TestItem = new("TestItem"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> TestItemWithActors = new("TestItemWithActors"u8);
        
        // 塔防商店物品 - 四种塔的建造物品（保留用于兼容性）
        public static readonly GameLink<GameDataItem, GameDataItemMod> SlowTowerItem = new("SlowTowerItem"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> AuraSlowTowerItem = new("AuraSlowTowerItem"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> AOETowerItem = new("AOETowerItem"u8);
        public static readonly GameLink<GameDataItem, GameDataItemMod> PenetrateTowerItem = new("PenetrateTowerItem"u8);
    }
    public static class Inventory
    {
        public static readonly GameLink<GameDataInventory, GameDataInventory> TestInventory6 = new("TestInventory6"u8);
        public static readonly GameLink<GameDataInventory, GameDataInventory> TestInventory6Equip = new("TestInventory6Equip"u8);
    }

    public static class UnitLeveling
    {
        // 塔防建筑等级系统配置
        public static readonly GameLink<GameDataUnitLeveling, GameDataUnitLeveling> SlowTowerLeveling = new("SlowTowerLeveling"u8);
        public static readonly GameLink<GameDataUnitLeveling, GameDataUnitLeveling> AuraSlowTowerLeveling = new("AuraSlowTowerLeveling"u8);
        public static readonly GameLink<GameDataUnitLeveling, GameDataUnitLeveling> AOETowerLeveling = new("AOETowerLeveling"u8);
        public static readonly GameLink<GameDataUnitLeveling, GameDataUnitLeveling> PenetrateTowerLeveling = new("PenetrateTowerLeveling"u8);
        public static readonly GameLink<GameDataUnitLeveling, GameDataUnitLeveling> TestUnitLeveling = new("TestUnitLeveling"u8);
    }


    public static class TargetingIndicator
    {
        public static readonly GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator> TestTargetingIndicator = new("TestTargetingIndicator"u8);
        public static readonly GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator> LineIndicator = new("LineIndicator"u8);
        public static readonly GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator> AreaIndicator = new("AreaIndicator"u8);
    }

    public static class AI
    {
        public static readonly GameLink<GameDataAIThinkTree, GameDataAIThinkTree> CombatTest = new("CombatTest"u8);
    }

    public static class Effect
    {
        public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> AddStunBuff = new("AddStunBuff"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> AddTestBuff = new("AddTestBuff"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectBuffRemove> RemoveAllBuffs = new("RemoveAllBuffs"u8);
    }



    public static class Buff
    {
        public static readonly GameLink<GameDataBuff, GameDataBuff> SpeedDebuff = new("SpeedDebuff"u8);
        public static readonly GameLink<GameDataBuff, GameDataBuff> Stun = new("Stun"u8);
        public static readonly GameLink<GameDataBuff, GameDataBuff> TestBuff = new("TestBuff"u8);
        // 🐌 引用塔防模板的减速Buff
        public static readonly GameLink<GameDataBuff, GameDataBuff> SlowDebuff = PassiveAbilities.Buff.SlowDebuff;
    }

    public static class Spawner
    {
        // 基础怪物刷怪器
        public static readonly GameLink<GameDataSpawner, GameDataSpawnerBasic> WolfSpawner = new("WolfSpawner"u8);
        public static readonly GameLink<GameDataSpawner, GameDataSpawnerBasic> CrawlerSpawner = new("CrawlerSpawner"u8);
        public static readonly GameLink<GameDataSpawner, GameDataSpawnerBasic> VultureSpawner = new("VultureSpawner"u8);
        public static readonly GameLink<GameDataSpawner, GameDataSpawnerBasic> WaterElementalSpawner = new("WaterElementalSpawner"u8);
        public static readonly GameLink<GameDataSpawner, GameDataSpawnerBasic> BoarSpawner = new("BoarSpawner"u8);
    }

    public static class Wave
    {
        // 波次配置
        public static readonly GameLink<GameDataWave, GameDataWaveBasic> Wave1 = new("Wave1"u8);
        public static readonly GameLink<GameDataWave, GameDataWaveBasic> Wave2 = new("Wave2"u8);
        public static readonly GameLink<GameDataWave, GameDataWaveBasic> Wave3 = new("Wave3"u8);
    }

    public static class Level
    {
        // 关卡配置
        public static readonly GameLink<GameDataLevel, GameDataLevelBasic> DefaultLevel = new("DefaultLevel"u8);
    }

    private static class Formular
    {
#if SERVER
        // 临时函数，今后要从触发器取。
        public static bool DefaultDamageNotificationPredicate(Player player, Damage damage)
        {
            // 仅当伤害值大于0时才通知
            if (damage.Current <= 0)
            {
                return false;
            }
            // 仅当玩家是伤害来源或伤害目标时才通知
            var playerCheck = damage.CasterPlayer == player || damage.Target?.Player == player;
            if (!playerCheck)
            {
                return false;
            }
            // 仅当伤害目标对玩家可见且处于同一场景时才通知
            var visibilityCheck = damage.Target != null && damage.Target.IsVisibleToTransient(player);
            return visibilityCheck;
        }
#endif
    }

    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        Game.Logger.LogInformation("Initializing Game Data...");
        _ = new GameDataActorScope(ActorScope.Default)
        {
            Name = "Default Actor Scope",
        };
        _ = new GameDataUnitProperty(UnitProperty.LifeMax)
        {
            Name = "Life Max",
        };
        _ = new GameDataUnitProperty(UnitProperty.ManaMax)
        {
            Name = "Mana Max",
        };
        _ = new GameDataUnitProperty(UnitProperty.AttackRange)
        {
            Name = "Attack Range",
        };
        _ = new GameDataUnitProperty(UnitProperty.AttackDamage)
        {
            Name = "Attack Damage",
        };
        _ = new GameDataUnitProperty(UnitProperty.Sight)
        {
            Name = "Sight Range",
        };
        _ = new GameDataUnitProperty(UnitProperty.Armor)
        {
            Name = "Armor",
        };
        _ = new GameDataUnitProperty(UnitProperty.MagicResistance)
        {
            Name = "Magic Resistance",
        };
        _ = new GameDataUnitProperty(UnitProperty.MoveSpeed)
        {
            Name = "Move Speed",
        };
        _ = new GameDataUnitProperty(UnitProperty.ShrubSight)
        {
            Name = "Shrub Sight",
        };
        _ = new GameDataUnitProperty(UnitProperty.Height)
        {
            Name = "Height",
        };
        _ = new GameDataUnitProperty(UnitProperty.SightBlockRadius)
        {
            Name = "Sight Block Radius",
        };
        _ = new GameDataUnitProperty(UnitProperty.TurningSpeed)
        {
            Name = "Turning Speed",
        };
        _ = new GameDataUnitProperty(UnitProperty.InventoryPickUpRange)
        {
            Name = "Inventory Pick Up Range"
        };
        _ = new GameDataUnitProperty(UnitProperty.LevelMax)
        {
            Name = "Attackable Radius",
        };
        _ = new GameDataUnitProperty(UnitProperty.ExperienceDistributionMultiplier)
        {
            Name = "Experience Distribution Multiplier",
        };
        _ = new GameDataCamera(Camera.DefaultCamera)
        {
            Name = "Default Camera",
            TargetZOffset = 10,
            Rotation = new(-90, -70, 0),
            TargetX = 2500,
            TargetY = 2500,
            FollowMainUnitByDefault = true,
            DisplayDebugInfo = true,
        };
        _ = new GameDataCamera(Camera.MoveableCamera)
        {
            Name = "Moveable Camera",
            TargetZOffset = 10,
            Rotation = new(-90, -70, 0),
            TargetX = 2500,
            TargetY = 2500,
            FollowMainUnitByDefault = false,
            DisplayDebugInfo = true,
            TargetingMode = CameraTargetingMode.Gesture,
        };
        // 🏰 塔防镜头配置 - 基于Lua配置参数设置
        _ = new GameDataCamera(Camera.TowerDefenseCamera)
        {
            Name = "塔防镜头",
            // 🎯 镜头模式设置为手势驱动，允许玩家自由操控镜头
            TargetingMode = CameraTargetingMode.Gesture,
            // 场景边界设置 (对应scene_border)
            // SceneBounds = new(1400, 3800, 1000, 3800), // Left, Up, Right, Down
            SceneBounds = new(3800, 1400, 3800, 1000), // Left, Up, Right, Down
            // 焦距设置 (对应min_distance/max_distance)
            FocalLength = new(2800, 2800), // Min, Max 都设为2800
            // 默认旋转角度 (对应default_rotation)
            Rotation = new(-90, -65, 0), // X, Y, Z
            // 🔒 镜头旋转角度限制 - 固定在-65度
            ZoomRotationRange = new(
                new CameraRotation(-90, -65, 0),  // 最小角度限制 (最低-75度)
                new CameraRotation(-90, -65, 0)   // 最大角度限制 (最高-55度)
            ),
            // 视野角度 (对应filed_of_view，注意原配置中的拼写错误)
            FieldOfView = new(55), // 55度视野
            // 裁剪平面 (对应near_clip/far_clip)
            NearClipPlane = 1.0f,
            FarClipPlane = 100000.0f,
            // 目标位置 (对应init_position)
            TargetX = 5632,
            TargetY = 3264,
            // 其他设置
            DisplayDebugInfo = false, // 塔防模式不需要显示调试信息
            FollowMainUnitByDefault = false, // 塔防镜头通常固定位置
        };
        // 注册形状测试单位
        _ = new GameDataUnit(Unit.ShapeTestSphere)
        {
            Name = "Shape Test Sphere",
            AttackableRadius = 64f,
            CollisionRadius = 32f,
            PrimitiveShape = new PrimitiveShapeConfig
            {
                Shape = PrimitiveShape.Sphere,
                Scale = Vector3.One,
                ColorTheme = ShapeColorTheme.Standard,
                ColorMode = ShapeColorMode.SmartDefaults
            }
        };

        _ = new GameDataUnit(Unit.ShapeTestCube)
        {
            Name = "Shape Test Cube",
            AttackableRadius = 64f,
            CollisionRadius = 32f,
            PrimitiveShape = new PrimitiveShapeConfig
            {
                Shape = PrimitiveShape.Cube,
                Scale = Vector3.One,
                ColorTheme = ShapeColorTheme.Standard,
                ColorMode = ShapeColorMode.SmartDefaults
            }
        };

        _ = new GameDataUnit(Unit.ShapeTestCylinder)
        {
            Name = "Shape Test Cylinder",
            AttackableRadius = 64f,
            CollisionRadius = 32f,
            PrimitiveShape = new PrimitiveShapeConfig
            {
                Shape = PrimitiveShape.Cylinder,
                Scale = Vector3.One,
                ColorTheme = ShapeColorTheme.Standard,
                ColorMode = ShapeColorMode.SmartDefaults
            }
        };

        _ = new GameDataUnit(Unit.ShapeTestPlane)
        {
            Name = "Shape Test Plane",
            AttackableRadius = 64f,
            CollisionRadius = 32f,
            PrimitiveShape = new PrimitiveShapeConfig
            {
                Shape = PrimitiveShape.Plane,
                Scale = Vector3.One,
                ColorTheme = ShapeColorTheme.Standard,
                ColorMode = ShapeColorMode.SmartDefaults
            }
        };

        _ = new GameDataUnit(Unit.ShapeTestCone)
        {
            Name = "Shape Test Cone",
            AttackableRadius = 64f,
            CollisionRadius = 32f,
            PrimitiveShape = new PrimitiveShapeConfig
            {
                Shape = PrimitiveShape.Cone,
                Scale = Vector3.One,
                ColorTheme = ShapeColorTheme.Standard,
                ColorMode = ShapeColorMode.SmartDefaults
            }
        };

        _ = new GameDataUnit(Unit.ShapeTestCapsule)
        {
            Name = "Shape Test Capsule",
            AttackableRadius = 64f,
            CollisionRadius = 32f,
            PrimitiveShape = new PrimitiveShapeConfig
            {
                Shape = PrimitiveShape.Capsule,
                Scale = Vector3.One,
                ColorTheme = ShapeColorTheme.Standard,
                ColorMode = ShapeColorMode.SmartDefaults
            }
        };

        _ = new GameDataUnit(Unit.ShapeTestPyramid)
        {
            Name = "Shape Test Pyramid",
            AttackableRadius = 64f,
            CollisionRadius = 32f,
            PrimitiveShape = new PrimitiveShapeConfig
            {
                Shape = PrimitiveShape.Pyramid,
                Scale = Vector3.One,
                ColorTheme = ShapeColorTheme.Standard,
                ColorMode = ShapeColorMode.SmartDefaults
            }
        };

        _ = new GameDataUnit(Unit.CompositeRobot)
        {
            Name = "Composite Robot",
            AttackableRadius = 64f,
            CollisionRadius = 32f,
            CompositeShapes = new List<PrimitiveShapeConfig>
            {
                // 身体 - 胶囊
                new PrimitiveShapeConfig
                {
                    Shape = PrimitiveShape.Capsule,
                    Scale = new Vector3(1.0f, 1.0f, 1.5f),
                    ColorTheme = ShapeColorTheme.Gaming,
                    ColorMode = ShapeColorMode.SmartDefaults,
                    AttachToRoot = true,
                    Tag = "robot_body"
                },
                // 头部 - 球体
                new PrimitiveShapeConfig
                {
                    Shape = PrimitiveShape.Sphere,
                    Scale = new Vector3(0.8f, 0.8f, 0.8f),
                    Offset = new Vector3(0, 0, 120f), // 胶囊体高度150 + 球体半径40 = 190，稍微调整到120
                    ColorTheme = ShapeColorTheme.Gaming,
                    ColorMode = ShapeColorMode.SmartDefaults,
                    AttachToRoot = false,
                    Tag = "robot_head"
                },
                // 左臂 - 圆柱体
                new PrimitiveShapeConfig
                {
                    Shape = PrimitiveShape.Cylinder,
                    Scale = new Vector3(0.3f, 0.3f, 1.0f),
                    Offset = new Vector3(-80f, 0, 50f), // 胶囊体半径50 + 圆柱体半径15 = 65，稍微调整到80
                    Rotation = new Vector3(0, 0, 45),
                    ColorTheme = ShapeColorTheme.Gaming,
                    ColorMode = ShapeColorMode.SmartDefaults,
                    AttachToRoot = false,
                    Tag = "robot_left_arm"
                },
                // 右臂 - 圆柱体
                new() {
                    Shape = PrimitiveShape.Cylinder,
                    Scale = new Vector3(0.3f, 0.3f, 1.0f),
                    Offset = new Vector3(80f, 0, 50f), // 胶囊体半径50 + 圆柱体半径15 = 65，稍微调整到80
                    Rotation = new Vector3(0, 0, -45),
                    ColorTheme = ShapeColorTheme.Gaming,
                    ColorMode = ShapeColorMode.SmartDefaults,
                    AttachToRoot = false,
                    Tag = "robot_right_arm"
                }
            }
        };

        // 城堡塔楼配置 - 参考AIShapeComposer.CreateCastleTower
        _ = new GameDataUnit(Unit.CastleTower)
        {
            Name = "Castle Tower",
            AttackableRadius = 64f,
            CollisionRadius = 32f,
            CompositeShapes = new List<PrimitiveShapeConfig>
            {
                // 基座 - 大立方体
                new PrimitiveShapeConfig
                {
                    Shape = PrimitiveShape.Cube,
                    Scale = new Vector3(1.5f, 1.5f, 0.8f),
                    ColorTheme = ShapeColorTheme.Gaming,
                    ColorMode = ShapeColorMode.SmartDefaults,
                    AttachToRoot = true,
                    Tag = "tower_base"
                },
                // 塔身 - 圆柱体
                new PrimitiveShapeConfig
                {
                    Shape = PrimitiveShape.Cylinder,
                    Scale = new Vector3(1.0f, 1.0f, 2.0f),
                    Offset = new Vector3(0, 0, 80f), // 基座高度80，塔身在顶部
                    ColorTheme = ShapeColorTheme.Gaming,
                    ColorMode = ShapeColorMode.SmartDefaults,
                    AttachToRoot = false,
                    Tag = "tower_body"
                },
                // 塔顶 - 圆锥
                new PrimitiveShapeConfig
                {
                    Shape = PrimitiveShape.Cone,
                    Scale = new Vector3(1.2f, 1.2f, 1.0f),
                    Offset = new Vector3(0, 0, 280f), // 塔身高度200，塔顶紧贴塔身顶部
                    ColorTheme = ShapeColorTheme.Gaming,
                    ColorMode = ShapeColorMode.SmartDefaults,
                    AttachToRoot = false,
                    Tag = "tower_top"
                }
            }
        };

        // 太空船配置 - 参考AIShapeComposer.CreateSimpleSpaceship
        _ = new GameDataUnit(Unit.Spaceship)
        {
            Name = "Spaceship",
            AttackableRadius = 64f,
            CollisionRadius = 32f,
            CompositeShapes = new List<PrimitiveShapeConfig>
            {
                // 船身 - 银色胶囊
                new PrimitiveShapeConfig
                {
                    Shape = PrimitiveShape.Capsule,
                    Scale = new Vector3(0.6f, 0.6f, 2.0f),
                    ColorTheme = ShapeColorTheme.Gaming,
                    ColorMode = ShapeColorMode.SmartDefaults,
                    AttachToRoot = true,
                    Tag = "spaceship_body"
                },
                // 尾翼 - 绿色楔形
                new PrimitiveShapeConfig
                {
                    Shape = PrimitiveShape.Wedge,
                    Scale = new Vector3(1.2f, 0.3f, 0.8f),
                    Offset = new Vector3(0, 0, 50f), // 在飞船底部后方
                    ColorTheme = ShapeColorTheme.Gaming,
                    ColorMode = ShapeColorMode.SmartDefaults,
                    AttachToRoot = false,
                    Tag = "spaceship_tail"
                },
                // 引擎 - 红色圆锥
                new PrimitiveShapeConfig
                {
                    Shape = PrimitiveShape.Cone,
                    Scale = new Vector3(0.4f, 0.4f, 0.6f),
                    Offset = new Vector3(0, 0, 20f), // 在飞船底部
                    Rotation = new Vector3(180, 0, 0), // 倒置
                    ColorTheme = ShapeColorTheme.Gaming,
                    ColorMode = ShapeColorMode.SmartDefaults,
                    AttachToRoot = false,
                    Tag = "spaceship_engine"
                }
            }
        };

        _ = new GameDataScene(Scene.DefaultScene)
        {
            DefaultCamera = Camera.DefaultCamera,
            Name = "Default Scene",
            HostedSceneTag = "default"u8,
            Size = new(16 * 256, 16 * 256),
            OnLoaded = static (scene) => Game.Logger.LogInformation("Scene {scene} loaded", scene),
            PlacedPlayerObjects = new()
            {
                {
                    1, new PlacedUnit()
                    {
                        Link = Unit.HostTestHero,
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
                        Link = Unit.HostTestHero,
                        OwnerPlayerId = 3,
                        Position = new(3000,3500,0),
                        IsMainUnit = true,
                        TriggerGetter = true,
                        UniqueId = 2,
                        Facing = -90,
                    }
                },
                {
                    3, new PlacedItem()
                    {
                        Link = Item.TestItem,
                        Position = new(3500,3500,0),
                        TriggerGetter = true,
                        UniqueId = 3,
                    }
                }
            }
        };
        _ = new GameDataScene(Scene.AITestScene)
        {
            DefaultCamera = Camera.DefaultCamera,
            Name = "AI System Test Scene",
            HostedSceneTag = "new_scene_1"u8,
            Size = new(64 * 256, 64 * 256),  // Large test area: 16384 x 16384 units
            OnLoaded = static (scene) => Game.Logger.LogInformation("AI Test Scene {scene} loaded", scene),
            PlacedPlayerObjects = new()
            {
                {
                    1, new PlacedUnit()
                    {
                        Link = Unit.HostTestHero,
                        OwnerPlayerId = 1,
                        Position = new(8000, 8000, 0),  // Center player in the large map
                        IsMainUnit = true,
                        TriggerGetter = true,
                        UniqueId = 1,
                    }
                },
                // AI Test target units spread across the large map
                {
                    10, new PlacedUnit()
                    {
                        Link = Unit.HostTestHero,
                        OwnerPlayerId = 2,
                        Position = new(10000, 10000, 0),  // Target 1 - Northeast
                        TriggerGetter = true,
                        UniqueId = 10,
                    }
                },
                {
                    11, new PlacedUnit()
                    {
                        Link = Unit.HostTestHero,
                        OwnerPlayerId = 2,
                        Position = new(6000, 10000, 0),   // Target 2 - Northwest
                        TriggerGetter = true,
                        UniqueId = 11,
                    }
                },
                {
                    12, new PlacedUnit()
                    {
                        Link = Unit.HostTestHero,
                        OwnerPlayerId = 2,
                        Position = new(10000, 6000, 0),   // Target 3 - Southeast
                        TriggerGetter = true,
                        UniqueId = 12,
                    }
                },
                {
                    13, new PlacedUnit()
                    {
                        Link = Unit.HostTestHero,
                        OwnerPlayerId = 2,
                        Position = new(6000, 6000, 0),    // Target 4 - Southwest
                        TriggerGetter = true,
                        UniqueId = 13,
                    }
                }
            }
        };

        // 基本形状测试场景 - 复用AITestScene的素材但有独立的逻辑空间
        _ = new GameDataScene(Scene.PrimitiveShapeTestScene)
        {
            DefaultCamera = Camera.DefaultCamera,
            Name = "Primitive Shape Test Scene",
            // FIXME: 共享AITestScene的素材无法正常生效。
            HostedSceneTag = new HostedSceneTag("primitive_shape_test_scene"u8, "new_scene_1"u8), // 复用AITest的素材
            Size = new(64 * 256, 64 * 256),  // 与AITestScene相同的大地图
            OnLoaded = static (scene) => Game.Logger.LogInformation("🎯 Primitive Shape Test Scene {scene} loaded", scene),
            PlacedPlayerObjects = new()
            {
                // 测试用主单位，放在地图中央
                {
                    1, new PlacedUnit()
                    {
                        Link = Unit.HostTestHero,
                        OwnerPlayerId = 1,
                        Position = new(8000, 8000, 0),  // 地图中心位置
                        IsMainUnit = true,
                        TriggerGetter = true,
                        UniqueId = 1,
                    }
                },
                // 预设一些测试位置的单位
                {
                    20, new PlacedUnit()
                    {
                        Link = Unit.HostTestHero,
                        OwnerPlayerId = 2,
                        Position = new(9000, 8000, 0),  // 东侧测试位置
                        TriggerGetter = true,
                        UniqueId = 20,
                    }
                },
                {
                    21, new PlacedUnit()
                    {
                        Link = Unit.HostTestHero,
                        OwnerPlayerId = 2,
                        Position = new(7000, 8000, 0),  // 西侧测试位置
                        TriggerGetter = true,
                        UniqueId = 21,
                    }
                },
                {
                    22, new PlacedUnit()
                    {
                        Link = Unit.HostTestHero,
                        OwnerPlayerId = 2,
                        Position = new(8000, 9000, 0),  // 北侧测试位置
                        TriggerGetter = true,
                        UniqueId = 22,
                    }
                },
                {
                    23, new PlacedUnit()
                    {
                        Link = Unit.HostTestHero,
                        OwnerPlayerId = 2,
                        Position = new(8000, 7000, 0),  // 南侧测试位置
                        TriggerGetter = true,
                        UniqueId = 23,
                    }
                }
            }
        };

        // RPG角色测试场景 - 复用AITestScene的素材，创建RPG角色测试环境
        _ = new GameDataScene(Scene.RPGRoleTestScene)
        {
            DefaultCamera = Camera.DefaultCamera,
            Name = "RPG Role Test Scene",
            HostedSceneTag = new HostedSceneTag("rpg_role_test_scene"u8, "new_scene_1"u8), // 复用AITest的素材
            Size = new(64 * 256, 64 * 256),  // 与AITestScene相同的大地图
            OnLoaded = static (scene) => Game.Logger.LogInformation("⚔️ RPG Role Test Scene {scene} loaded", scene),
        };

        // JumpJump游戏专用场景 - 干净的场景，只包含必要的游戏元素
        _ = new GameDataScene(Scene.JumpJumpScene)
        {
            DefaultCamera = Camera.MoveableCamera,
            Name = "Jump Jump Game Scene",
            HostedSceneTag = new HostedSceneTag("jump_jump_scene"u8, "new_scene_1"u8), // 复用AITest的素材
            Size = new(64 * 256, 64 * 256),  // 与AITestScene相同的大地图
            OnLoaded = static (scene) => Game.Logger.LogInformation("🎮 Jump Jump Scene {scene} loaded", scene),
            PlacedPlayerObjects = [] // 不预设任何单位，让游戏自己创建
        };

        
        // 塔防游戏场景配置 - 使用new_scene_2地图
        _ = new GameDataScene(Scene.TowerDefenseScene)
        {
            DefaultCamera = Camera.TowerDefenseCamera, // 🎯 使用专用的塔防镜头
            Name = "Tower Defense Scene",
            HostedSceneTag = "new_scene_2"u8,
            Size = new(26 * 256, 44 * 256),  // 大地图用于塔防游戏
            OnLoaded = static (scene) => Game.Logger.LogInformation("🏰 Tower Defense Scene {scene} loaded", scene),
            PlacedPlayerObjects = new()
            {
                // 主单位放在地图中央 - 可以作为塔防游戏的指挥中心
                {
                    1, new PlacedUnit()
                    {
                        Link = Unit.TowerDefenseHero,  // 使用专门的塔防英雄单位
                        OwnerPlayerId = 1,
                        Position = new(4000, 1600, 0),
                        IsMainUnit = true,
                        TriggerGetter = true,
                        UniqueId = 1,
                    }
                },
            }
        };

        _ = new GameDataInventory(Inventory.TestInventory6)
        {
            Slots = [
                new (),
                new (),
                new (),
                new (),
                new (),
                new (),
                ]
        };
        _ = new GameDataInventory(Inventory.TestInventory6Equip)
        {
            InventoryFlags = new()
            {
                HandlePickUpRequest = false
            },
            Slots = [
                new (){
                    Type=ItemSlotType.Equip
                },
                new (){
                    Type=ItemSlotType.Equip
                },
                new (){
                    Type=ItemSlotType.Equip
                },
                new (){
                    Type=ItemSlotType.Equip
                },
                new (){
                    Type=ItemSlotType.Equip
                },
                new (){
                    Type=ItemSlotType.Equip
                },
                ]
        };
        _ = new GameDataActorModel(Actor.TestActorModel)
        {
            Model = Model.TestModelScript,
            Socket = "socket_root"u8,
        };
        _ = new GameDataActorModel(Actor.TestActorModelScript)
        {
            Model = Model.TestModelScript,
            Socket = "socket_root"u8,
        };
        _ = new GameDataModel(Model.TestModelScript)
        {
            Radius = 50,
            Asset = "characters/palu_hx/sk_daershu/model.prefab"u8,
        };
        _ = new GameDataActorSite(GameCore.ScopeData.Actor.ScopeStaticSiteTransient)
        {
            ReleaseOnGC = true,
            AttachForwardOnce = true,
        };
        _ = new GameDataActorSite(GameCore.ScopeData.Actor.ScopeStaticSitePersist)
        {
        };
        _ = new GameDataGameMode(GameMode.FlappyBird)
        {
            Name = "Flappy Bird Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                ],
        };
        _ = new GameDataGameMode(GameMode.VampireSurvivors3D)
        {
            Name = "Vampire 3D Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.DefaultScene,
                ExtraScopeDataVampire3D.Scene.VampireScene
            ],
            DefaultScene = ExtraScopeDataVampire3D.Scene.VampireScene,
        };
        _ = new GameDataGameMode(GameMode.VampireSurvivors2D)
        {
            Name = "Vampire 2D Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
            ],
        };
        _ = new GameDataGameMode(GameMode.JoyStickTest)
        {
            Name = "JoyStick Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.DefaultScene,
                ],
            DefaultScene = Scene.DefaultScene,
        };
        _ = new GameDataGameMode(GameMode.GameUITest)
        {
            Name = "Game UI Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.DefaultScene,
                ],
            DefaultScene = Scene.DefaultScene,
        };
        _ = new GameDataGameMode(GameMode.TypedMessageTest)
        {
            Name = "TypedMessage Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.DefaultScene,
                ],
            DefaultScene = Scene.DefaultScene,
        };
        _ = new GameDataGameMode(GameMode.Gomoku)
        {
            Name = "Gomoku Game Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.DefaultScene,
                ],
            DefaultScene = Scene.DefaultScene,
        };
        _ = new GameDataGameMode(GameMode.UserCloudDataTest)
        {
            Name = "UserCloudData Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.DefaultScene,
                ],
            DefaultScene = Scene.DefaultScene,
        };
        _ = new GameDataGameMode(GameMode.AISystemTest)
        {
            Name = "AI System Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.AITestScene,
                ],
            DefaultScene = Scene.AITestScene,
        };
        _ = new GameDataGameMode(GameMode.PrimitiveShapeTest)
        {
            Name = "Primitive Shape Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.PrimitiveShapeTestScene,
                ],
            DefaultScene = Scene.PrimitiveShapeTestScene,
        };
        _ = new GameDataGameMode(GameMode.RPGRoleTest)
        {
            Name = "RPG Role Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.RPGRoleTestScene,
                ],
            DefaultScene = Scene.RPGRoleTestScene,
        };
        _ = new GameDataGameMode(GameMode.ModernUITest)
        {
            Name = "Modern UI Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.DefaultScene,
                ],
            DefaultScene = Scene.DefaultScene,
        };
        _ = new GameDataGameMode(GameMode.BuffTest)
        {
            Name = "Buff Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.DefaultScene,
            ],
            DefaultScene = Scene.DefaultScene,
        };
        _ = new GameDataGameMode(GameMode.ScriptPerformanceTest)
        {
            Name = "Script Performance Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                // 无需场景，仅有UI
            ],
        };
        _ = new GameDataGameMode(GameMode.Game2048)
        {
            Name = "2048 Game Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                // 无需场景，仅有UI
            ],
        };
        _ = new GameDataGameMode(GameMode.JumpJump)
        {
            Name = "Jump Jump Game Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.JumpJumpScene,
            ],
            DefaultScene = Scene.JumpJumpScene,
        };
        _ = new GameDataGameMode(GameMode.TouchBehaviorTest)
        {
            Name = "TouchBehavior Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                // 无需场景，仅有UI
            ],
        };
        _ = new GameDataGameMode(GameMode.TowerDefense)
        {
            Name = "Tower Defense Game",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.TowerDefenseScene,
                ],
            DefaultScene = Scene.TowerDefenseScene,
        };
        _ = new GameDataGameMode(GameMode.ARPGMode)
        {
            Name = "ARPG剑客模式",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                ARPGTemplate.ScopeData.ARPGScopeData.Scene.ARPGScene, // 使用ARPG专用场景
            ],
            DefaultScene = ARPGTemplate.ScopeData.ARPGScopeData.Scene.ARPGScene,
        };
        _ = new GameDataGameMode(GameMode.UIShowcaseDemo)
        {
            Name = "UI能力展示Demo",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.DefaultScene,
            ],
            DefaultScene = Scene.DefaultScene,
        };
        _ = new GameDataGameMode(GameMode.XianJianQiXiaZhuan)
        {
            Name = "仙剑奇侠传",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                XianJianQiXiaZhuan.ScopeData.XianJianScopeData.Scene.YuHangTown,
                XianJianQiXiaZhuan.ScopeData.XianJianScopeData.Scene.XianLingIsland,
                XianJianQiXiaZhuan.ScopeData.XianJianScopeData.Scene.SuoYaoTower,
            ],
            DefaultScene = XianJianQiXiaZhuan.ScopeData.XianJianScopeData.Scene.YuHangTown,
        };
        _ = new GameDataGameMode(GameMode.AVGTest)
        {
            Name = "AVG系统测试",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.DefaultScene,
            ],
            DefaultScene = Scene.DefaultScene,
        };
        _ = new GameDataGameMode(GameMode.UIFrameworkTest)
        {
            Name = "UI Framework Test Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                // 无需场景，仅有UI
            ],
            GameUI = GameUI.ScopeData.GameUI.Default,
        };
        _ = new GameDataGameMode(GameCore.ScopeData.GameMode.Default)
        {
            Name = "Default Mode",
            Gameplay = Gameplay.Default,
            PlayerSettings = PlayerSettings.Default,
            SceneList = [
                Scene.DefaultScene,
            ],
            GameUI = GameUI.ScopeData.GameUI.Default,
            DefaultScene = Scene.DefaultScene,
        };

        _ = new GameDataDamageType(DamageType.Physical)
        {
            Name = "Physical Damage",
#if SERVER
            // 自定义伤害公式
            // CustomFormular =
            // 自定义暴击公式
            // CustomFormularIsCritical
            NotificationPredicate = Formular.DefaultDamageNotificationPredicate,
#endif
            FloatingTextDealt = GameCore.ScopeData.FloatingText.PhysicalDamage,
            FloatingTextDealtCritical = GameCore.ScopeData.FloatingText.CriticalPhysicalDamage,
            FloatingTextReceived = GameCore.ScopeData.FloatingText.DamageReceived,
        };
        _ = new GameDataDamageType(DamageType.Magical)
        {
            Name = "Magical Damage",
#if SERVER
            NotificationPredicate = Formular.DefaultDamageNotificationPredicate,
#endif
            FloatingTextDealt = GameCore.ScopeData.FloatingText.MagicDamage,
            FloatingTextDealtCritical = GameCore.ScopeData.FloatingText.CriticalMagicDamage,
            FloatingTextReceived = GameCore.ScopeData.FloatingText.DamageReceived,
        };
        _ = new GameDataDamageType(DamageType.Pure)
        {
            Name = "Pure Damage",
#if SERVER
            NotificationPredicate = Formular.DefaultDamageNotificationPredicate,
#endif
            FloatingTextDealt = GameCore.ScopeData.FloatingText.PureDamage,
            FloatingTextReceived = GameCore.ScopeData.FloatingText.DamageReceived,
        };
        // 浮动文本，暂时无法通过代码生成，需要手动创建
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.PhysicalDamage)
        {
            Name = "Physical Damage",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.CriticalPhysicalDamage)
        {
            Name = "Critical Physical Damage",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.PhysicalAccumulated)
        {
            Name = "Physical Accumulated",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.MagicDamage)
        {
            Name = "Magic Damage",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.CriticalMagicDamage)
        {
            Name = "Critical Magic Damage",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.MagicAccumulated)
        {
            Name = "Magic Accumulated",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.PureDamage)
        {
            Name = "Pure Damage",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.DamageReceived)
        {
            Name = "Damage Received",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.Heal)
        {
            Name = "Heal",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.ManaSpent)
        {
            Name = "Mana Cost",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.Gold)
        {
            Name = "Gold",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.Exp)
        {
            Name = "Exp",
        };
        _ = new GameDataFloatingText(GameCore.ScopeData.FloatingText.Missed)
        {
            Name = "Missed",
        };
        _ = new GameDataGameUI(GameUI.ScopeData.GameUI.Default)
        {
            Name = "Default Game UI",
            StandardUIRenderingOrder = [
                // 基础游戏控制层 - 最底层，基础交互元素
                StandardUIType.Joystick,
                StandardUIType.Map,
                
                // 游戏内常驻UI层 - 游戏进行中始终可见的UI元素
                StandardUIType.Minimap,
                StandardUIType.Hotbar,
                StandardUIType.StatusBar,
                
                // 信息面板层 - 游戏核心信息界面
                StandardUIType.CharacterSheet,
                StandardUIType.Inventory,
                StandardUIType.Quest,
                StandardUIType.QuestLog,
                StandardUIType.Crafting,
                StandardUIType.TalentTree,
                
                // 系统功能层 - 系统级功能界面
                StandardUIType.Leaderboards,
                StandardUIType.Settings,
                StandardUIType.Shop,
                StandardUIType.Party,
                StandardUIType.Social,
                StandardUIType.Achievement,
                
                // 交互反馈层 - 即时交互和反馈
                StandardUIType.Chat,
                StandardUIType.Dialogue,
                
                // 通知提示层 - 重要信息通知
                StandardUIType.Notifications,
                
                // 教程引导层 - 新手引导和帮助
                StandardUIType.Tutorial,
                
                // 游戏内模态层 - 需要暂停游戏的选择界面
                StandardUIType.Reward,
                
                // 全屏模态层 - 覆盖所有游戏内容的界面
                StandardUIType.MainMenu,
                
                // 最高优先级层 - 系统级对话框，不应被任何UI遮挡
                StandardUIType.ConfirmDialog,
                StandardUIType.ErrorDialog,
                StandardUIType.Loading,
                ],
            StandardUIBaseZIndex = 0,
            StandardUIZIndexStep = 100,
        };
        var defaultWaveAI = new GameLink<GameDataWaveAI, GameDataWaveAI>("default"u8);
        _ = new GameDataWaveAI(defaultWaveAI)
        {
            // ⚔️ 战斗配置 - 启用AI战斗功能
            EnableCombat = true,

            // 🔍 扫描和攻击范围配置
            MinimalScanRange = 500f,
            MaximalScanRange = 1000f,
            MinimalApproachRange = 200f,

            // 🏃‍♂️ 牵引和撤退配置
            CombatLeash = 1500f,
            CombatResetRange = 1800f,

            // ⏱️ 战斗持续时间
            InCombatMinimalDuration = TimeSpan.FromSeconds(2),

            // 🌊 群体AI配置
            EnableWaveFormation = false, // 个体战斗不需要编队
            EnableLinkedAggro = true,    // 启用连锁仇恨

            // 🔄 AI生命周期
            AutoDisposeOnEmpty = true,

            // 📍 默认行为类型
            Type = WaveType.Guard,
        };

        // 数编数据作为控件模板的样例
        _ = new GameDataControlButton(Control.TestButton)
        {
            Background = new SolidColorBrush(Color.AliceBlue),
            Layout = new()
            {
                Width = 500,
                Height = 90,
                // 位于左上角
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
            // 按特定屏幕旋转改变属性
            OverrideByOrientation = new()
            {
                {   
                    // 竖直时将宽高互换
                    DisplayOrientations.Portrait, new GameDataControlButton(new GameLink<GameDataControl, GameDataControlButton>("TestButtonOverrideByOrientation1"))
                    {
                        Layout = new()
                        {
                            Width = 250,
                            Height = 300,
                        },
                    }.Link
                },
            },
            Children =
            [
                // 按钮有一个默认Label子控件
                new GameDataControlLabel(new("TestButtonLabel"))
                {
                    Text = "模板创建的按钮",
                    TextColor = Color.Green,
                    FontSize = 24,
                    Bold = true,
#if CLIENT
                    OnPostInitialization = static (c) =>
                    {
                        if (c is not Label label)
                        {
                            return;
                        }
                        label.Text = label.DataContext?.ToString();
                    },
#endif
                    // 按特定主题改变属性
                    OverrideByTheme = new()
                    {
                        {
                            // 符剑主题时显示符剑
                            Theme.FuJian, new GameDataControlLabel(new GameLink<GameDataControl, GameDataControlLabel>("TestButtonLabelOverrideByTheme1"))
                            {
                                Text = "符剑",
                            }.Link
                        },
                        {
                            // 回响主题时显示回响
                            Theme.ProjectEcho, new GameDataControlLabel(new GameLink<GameDataControl, GameDataControlLabel>("TestButtonLabelOverrideByTheme2"))
                            {
                                Text = "回响",
                            }.Link
                        }
                    }
                }.Link,
            ],
#if CLIENT
            OnVirtualizationPhase =
            [
                static (c) =>
                {
                    if (c.Children?[0] is Label label)
                    {
                        label.Text = "??";
                    }
                },
                static (c)=>
                {
                    if (c.Children?[0] is Label label)
                    {
                        label.Text = label.DataContext?.ToString();
                    }
                }
                ]
#endif
        };
        // 数编数据作为控件模板的样例2
        _ = new GameDataControlButton(Control.TestButton2)
        {
            Background = new SolidColorBrush(Color.OrangeRed),
            Layout = new()
            {
                Width = 500,
                Height = 90,
                // 位于左上角
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
            Children =
            [
                // 按钮有一个默认Label子控件，内容与TestButton1的Label相同
                new GameLink<GameDataControl, GameDataControlLabel>("TestButtonLabel"),
            ],
#if CLIENT
            OnVirtualizationPhase =
            [
                static (c) =>
                {
                    if (c.Children?[0] is Label label)
                    {
                        label.Text = "??";
                    }
                },
                static (c)=>
                {
                    if (c.Children?[0] is Label label)
                    {
                        label.Text = label.DataContext?.ToString();
                    }
                }
                ]
#endif
        };
        _ = new GameDataGameplay(Gameplay.Default)
        {
            Name = "Default Gameplay",
            DefaultWaveAI = defaultWaveAI,
            ItemQualityList = [
                ItemQuality.Poor,
                ItemQuality.Common,
                ItemQuality.Uncommon,
                ItemQuality.Rare,
                ItemQuality.Epic,
                ItemQuality.Legendary,
                ItemQuality.Mythic,
            ],
        };
        _ = new GameDataPlayerSettings(PlayerSettings.Default)
        {
            PlayerTeamData = [
                [
                    new() { Controller = ControllerType.Computer, Id = 0, IsNeutral = true }
                ],
                [
                    new() { Controller = ControllerType.User, Id = 1 },
                    new() { Controller = ControllerType.Computer, Id = 2 }
                ],
                [
                    new() { Controller = ControllerType.Computer, Id = 3 },
                    new() { Controller = ControllerType.Computer, Id = 4 }
                ],
                [
                    new() { Controller = ControllerType.User, Id = 5 },
                ],
            ]
        };
        _ = new GameDataUnitLeveling(UnitLeveling.TestUnitLeveling)
        {
            ExperienceRequiredForEachLevel = [0, 0, 200],
            ExtraLevelExperienceRequiredLevelFactor = 100,
            MaxLevelLeech = true,
        };
        _ = new GameDataItemQuality(ItemQuality.Poor)
        {
            Name = "垃圾",
            Color = Color.Gray,
            BackgroundImage = "image/底框=垃圾.png",
            BorderImage = "image/品质框=垃圾.png",
        };
        _ = new GameDataItemQuality(ItemQuality.Common)
        {
            Name = "普通",
            Color = Color.FromArgb(146, 150, 155), //#92969B
            BackgroundImage = "image/底框=普通.png",
            BorderImage = "image/品质框=普通.png",
        };
        _ = new GameDataItemQuality(ItemQuality.Uncommon)
        {
            Name = "优秀",
            Color = Color.FromArgb(59, 151, 62), //#3B973E
            BackgroundImage = "image/底框=优秀.png",
            BorderImage = "image/品质框=优秀.png",
        };
        _ = new GameDataItemQuality(ItemQuality.Rare)
        {
            Name = "稀有",
            Color = Color.FromArgb(35, 141, 227), //#238DE3
            BackgroundImage = "image/底框=稀有.png",
            BorderImage = "image/品质框=稀有.png",
        };
        _ = new GameDataItemQuality(ItemQuality.Epic)
        {
            Name = "史诗",
            Color = Color.FromArgb(187, 137, 250), //#BB89FA
            BackgroundImage = "image/底框=史诗.png",
            BorderImage = "image/品质框=史诗.png",
        };
        _ = new GameDataItemQuality(ItemQuality.Legendary)
        {
            Name = "传说",
            Color = Color.FromArgb(208, 143, 60), //#D08F3C
            BackgroundImage = "image/底框=传说.png",
            BorderImage = "image/品质框=传说.png",
        };
        _ = new GameDataItemQuality(ItemQuality.Mythic)
        {
            Name = "神话",
            Color = Color.FromArgb(215, 46, 50), //#D72E32
            BackgroundImage = "image/底框=神话.png",
            BorderImage = "image/品质框=神话.png",
        };
        var defaultCombatBehavior = new GameLink<GameDataAINode, GameDataAINodeSequence>("defaultCombatBehavior"u8);

        // 子节点1：扫描敌人节点
        var scanEnemies = new GameLink<GameDataAINode, GameDataAINodeValidateScan>("scanEnemies"u8);
        _ = new GameDataAINodeValidateScan(scanEnemies)
        {
            // 不忽略牵引限制
            IgnoreLeash = false,
        };

        // 子节点2：对扫描目标施法 - 直接使用ScanTargetThisTick，即 scanEnemies 的输出
        var castAtTarget = new GameLink<GameDataAINode, GameDataAINodeValidateCast>("castAtTarget"u8);
        _ = new GameDataAINodeValidateCast(castAtTarget)
        {
            DoRecast = true,
        };

        // 主行为树：简化的序列节点（扫描 -> 施法）
        _ = new GameDataAINodeSequence(defaultCombatBehavior)
        {
            // 🔧 简化的两步序列：扫描 -> 施法
            Nodes = [scanEnemies, castAtTarget],
        };
        // Default AI Definitions
        _ = new GameDataAIThinkTree(GameCore.ScopeData.AI.Default)
        {
            ScanFilters = [new() {
                Required=[UnitRelationship.Enemy, UnitFilter.Unit, UnitRelationship.Visible],
                Excluded=[UnitFilter.Item, UnitState.Invulnerable, UnitState.Dead]
            }],
            CombatBehaviorTree = defaultCombatBehavior,
        };

        // AI Combat Test Definitions - optimized for unit vs unit combat
        _ = new GameDataAIThinkTree(AI.CombatTest)
        {
            ScanFilters = [new() {
                Required=[UnitRelationship.Enemy, UnitFilter.Unit, UnitRelationship.Visible],
                Excluded=[UnitFilter.Item, UnitState.Invulnerable, UnitState.Dead]
            }],
            ScanSorts = [UnitFilter.Hero, UnitFilter.Unit], // Prioritize heroes, then units
            CombatBehaviorTree = defaultCombatBehavior,
        };

        _ = new GameDataActorAdditionModel(Actor.TestActorAdditionModel)
        {
            Model = Model.TestActorAdditionModel,
        };
        _ = new GameDataModel(Model.TestActorAdditionModel)
        {
            Radius = 50,
            Asset = "eqpt/weapon/sm_dr_sword_04_02/model.prefab"u8,
        };
        _ = new GameDataModel(Model.HostTestHero)
        {
            Radius = 50,
            Asset = "characters/general/sk_basic2/model.prefab"u8,
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new()
                {
                    AnimationRaw= "sword_idle"u8,
                    AnimationAlias= "idle"u8,
                },
                new()
                {
                    AnimationRaw= "sword_move"u8,
                    AnimationAlias= "move"u8,
                },
                ]
        };
        _ = new GameDataUnit(Unit.HostTestHero)
        {
            Name = "测试英雄",
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
            Inventories = [Inventory.TestInventory6, Inventory.TestInventory6Equip],
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            ActorArray = [
                Actor.TestActorAdditionModel,
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
            Leveling = UnitLeveling.TestUnitLeveling,
            Model = Model.HostTestHero,
            Abilities = [Ability.ChargedTestSpell], // 添加新的充能技能
        };

        // AI Combat Test Hero - 复制HostTestHero配置并添加TacticalAI
        _ = new GameDataUnit(Unit.AICombatTestHero)
        {
            Name = "AI战斗测试英雄",
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
            Inventories = [Inventory.TestInventory6, Inventory.TestInventory6Equip],
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            ActorArray = [
                Actor.TestActorAdditionModel,
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
            Model = Model.HostTestHero,
            TacticalAI = AI.CombatTest, // 关键：添加TacticalAI配置
            Abilities = [Ability.TestSpell], // 关键：添加攻击技能
        };

        _ = new GameDataModel(Model.TestItem)
        {
            Radius = 50,
            Asset = "deco/handpaintedvol2_campsite/sm_backpack_01_base/model.prefab"u8,
        };

        _ = new GameDataUnit(Unit.TestItem)
        {
            Name = "测试物品单位",
            AttackableRadius = 60,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 32,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.TestItem,
        };


        _ = new GameDataItemMod(Item.TestItem)
        {
            Unit = Unit.TestItem,
            Icon = "image/inventory/星火币大图标.png"u8,
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [new(){
                        Property = UnitProperty.LifeMax,
                        Value = (_) => 100,
                        Random = 75,
                    }],
                    Ability = Ability.TestSpell,
                }
            },
            StackStart = 5,
            Level = 6,
            Quality = 7,
        };
       // ========== 塔防商店物品配置 ==========
        
        // 1. 减速塔物品
        _ = new GameDataItemMod(Item.SlowTowerItem)
        {
            Name = "减速塔",
            Unit = Unit.TestItem, // 暂时使用现有物品单位作为显示
            Modifications = new()
            {
                [ItemSlotType.Carry] = new()
                {
                    Ability = Ability.BuildSlowTower, // 主动技能是建造减速塔
                }
            },
            StackStart = 1,
            StackMax = 10, // 最多堆叠10个
            Level = 1,
            Quality = 1,
        };

        // 2. 光环塔物品
        _ = new GameDataItemMod(Item.AuraSlowTowerItem)
        {
            Unit = Unit.TestItem,
            Modifications = new()
            {
                [ItemSlotType.Carry] = new()
                {
                    Ability = Ability.BuildAuraSlowTower, // 主动技能是建造光环塔
                }
            },
            StackStart = 1,
            StackMax = 10,
            Level = 2,
            Quality = 2,
        };

        // 3. 爆炸塔物品
        _ = new GameDataItemMod(Item.AOETowerItem)
        {
            Unit = Unit.TestItem,
            Modifications = new()
            {
                [ItemSlotType.Carry] = new()
                {
                    Ability = Ability.BuildAOETower, // 主动技能是建造爆炸塔
                }
            },
            StackStart = 1,
            StackMax = 10,
            Level = 3,
            Quality = 3,
        };

        // 4. 穿透塔物品
        _ = new GameDataItemMod(Item.PenetrateTowerItem)
        {
            Unit = Unit.TestItem,
            Modifications = new()
            {
                [ItemSlotType.Carry] = new()
                {
                    Ability = Ability.BuildPenetrateTower, // 主动技能是建造穿透塔
                }
            },
            StackStart = 1,
            StackMax = 10,
            Level = 4,
            Quality = 4,
        };

        // 🎭 创建带有附属表现的测试物品数据

        // 1. 创建粒子效果
        _ = new GameDataParticle(Particle.ItemEquipParticle)
        {
            Name = "物品装备特效",
            Asset = "effect/eff_tongyong/huoqiu2/particle.effect"u8,
        };

        // 2. 创建模型
        _ = new GameDataModel(Model.TestItemWithActors)
        {
            Name = "测试物品模型（带附属表现）",
            Radius = 60,
            Asset = "deco/handpaintedvol2_campsite/sm_backpack_01_base/model.prefab"u8,
        };

        _ = new GameDataModel(Model.ItemEffectModel)
        {
            Name = "物品特效模型",
            Radius = 30,
            Asset = "characters/palu_hx/sk_daershu/model.prefab"u8,
        };

        // 3. 创建Actor
        _ = new GameDataActorModel(Actor.ItemSelfActor)
        {
            Name = "物品自身附属Actor",
            Model = Model.ItemEffectModel,
            Socket = "socket_root"u8,
        };

        _ = new GameDataActorParticle(Actor.ItemEquipEffect)
        {
            Name = "物品装备特效Actor",
            Particle = Particle.ItemEquipParticle,
        };

        _ = new GameDataActorModel(Actor.ItemCarrierActor)
        {
            Name = "物品持有者附属Actor",
            Model = Model.ItemEffectModel,
            Socket = "socket_weapon_r"u8,
        };

        // 4. 创建带ActorArray的物品单位
        _ = new GameDataUnit(Unit.TestItemWithActors)
        {
            Name = "测试物品单位（带附属表现）",
            AttackableRadius = 60,
            Filter = [UnitFilter.Item],
            State = [UnitState.Invulnerable],
            CollisionRadius = 32,
            DynamicCollisionMask = DynamicCollisionMask.Item,
            Model = Model.TestItemWithActors,
            // 物品单位自身的ActorArray - 物品在地面时的附属表现
            ActorArray = [
                Actor.ItemSelfActor,
            ],
        };

        // 5. 创建带ActorArray的ItemMod数据
        _ = new GameDataItemMod(Item.TestItemWithActors)
        {
            Name = "测试物品（带附属表现）",
            Unit = Unit.TestItemWithActors,
            Modifications = new()
            {
                [ItemSlotType.Equip] = new()
                {
                    Modifications = [new(){
                        Property = UnitProperty.LifeMax,
                        Value = (_) => 150,
                        Random = 50,
                    }],
                    Ability = Ability.TestSpell,
                    // 装备时给持有者添加的ActorArray - 装备者的附属表现
                    ActorArray = [
                        Actor.ItemEquipEffect,
                        Actor.ItemCarrierActor,
                    ],
                }
            },
            StackStart = 3,
            Level = 5,
            Quality = 8,
        };

        _ = new GameDataUnit(Unit.TestMissileUnit)
        {
            Name = "TestMissileUnit",
            Filter = [UnitFilter.Missile],
            State = [UnitState.Invulnerable],
            CollisionRadius = 32,
            AttackableRadius = 60,
            Particle = "effect/eff_tongyong/huoqiu2/particle.effect"u8,
            UpdateFlags = new()
            {
                AllowMover = true,
            },
        };
        var linkTestEffectDamage = new GameLink<GameDataEffect, GameDataEffectDamage>("TestEffectDamage"u8);
        _ = new GameDataEffectDamage(linkTestEffectDamage)
        {
            Amount = static (_) => 100,
            Type = DamageType.Physical,
            LogExecutionFailure = true,
        };
        var linkTestPrint = new GameLink<GameDataEffect, GameDataEffectLog>("TestEffectPrint"u8);
        _ = new GameDataEffectLog(linkTestPrint)
        {
            Message = static (e) => $"Found {e.Target}",
            LogExecutionFailure = true,
        };
        var linkTestSearch = new GameLink<GameDataEffect, GameDataEffectSearch>("TestEffectSearch"u8);
        _ = new GameDataEffectSearch(linkTestSearch)
        {
            SearchFilters = [new() { Excluded = [UnitState.Dead] }],
            Radius = static (_) => 1000,
            Effect = linkTestPrint,
            LogExecutionFailure = true,
        };
        var linkTestDelay = new GameLink<GameDataEffect, GameDataEffectPersistDelay>("TestEffectDelay"u8);
        _ = new GameDataEffectPersistDelay(linkTestDelay)
        {
            Amount = static (_) => TimeSpan.FromSeconds(1),
            CompleteEffect = linkTestSearch,
            LogExecutionFailure = true,
        };
        var linkImpactSet = new GameLink<GameDataEffect, GameDataEffectSet>("ImpactSet"u8);
        _ = new GameDataEffectSet(linkImpactSet)
        {
            Effects = [
                new() { Link= linkTestEffectDamage },
                new() { Link= linkTestDelay }
                ],

            LogExecutionFailure = true,
        };
        _ = new GameDataParticle(Particle.TestParticleScript)
        {
            Asset = "effect/eff_tongyong/huoqiu_blast/particle.effect"u8,
            Radius = 128
        };
        _ = new GameDataActorParticle(Actor.TestActorParticle)
        {
            AutoPlay = true,
            Particle = Particle.TestParticleScript,
            KillOnFinish = true,
            ForceOneShot = true,
        };
        var linkTestEffectLaunchMissile = new GameLink<GameDataEffect, GameDataEffectLaunchMissile>("TestEffectLaunchMissile"u8);
        _ = new GameDataEffectLaunchMissile(linkTestEffectLaunchMissile)
        {
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Unit.TestMissileUnit,
            DoImpactEntity = true,
            DoStaticCollision = true,
            ImpactMaxCount = static (_) => 1,
            ImpactSearchRadius = static (_) => 20,
            Speed = static (_) => 1250,
            ImpactEffect = linkImpactSet,
            CompleteEffect = linkTestDelay,
            ImpactSearchFilter = [
                new() {
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }
                ],
            LogExecutionFailure = true,
            ImpactActors = [Actor.TestActorParticle],
            LaunchHeight = (_) => 110
        };
        _ = new GameDataAnimationSimple(Animation.Idle)
        {
            Name = "Idle Animation",
            File = "idle"u8,
            IsLooping = true,
        };
        _ = new GameDataAnimationSimple(Animation.Move)
        {
            Name = "Move Animation",
            File = "move"u8,
            IsLooping = true,
        };
        _ = new GameDataAnimationSimple(Animation.Attack)
        {
            Name = "Attack Animation",
            File = "attack"u8,
            IsLooping = false,
        };
        _ = new GameDataAnimationSimple(Animation.Death)
        {
            Name = "Death Animation",
            File = "death"u8,
            IsLooping = false,
        };
        _ = new GameDataAnimationSimple(Animation.TestSpell)
        {
            Name = "Test Spell Animation",
            File = "anim/human/barehanded_anim/hand_05/skill_025.ani"u8,
            IsLooping = false,
        };
        var cooldownLink = new GameLink<GameDataCooldown, GameDataCooldownActive>("TestSpell"u8);
        _ = new GameDataCooldownActive(cooldownLink)
        {
            Time = static (_) => TimeSpan.FromSeconds(0.6),
        };
        _ = new GameDataAbilityExecute(Ability.TestSpell)
        {
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.391),
                Channel = static (_) => TimeSpan.FromSeconds(0.148),
                Backswing = static (_) => TimeSpan.FromSeconds(0.395),
            },
            Cost = new()
            {
                Cooldown = cooldownLink
            },
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { }, // 🔧 移除IsAttack标记，测试缺省扫描范围
            Effect = linkTestEffectLaunchMissile,
            TargetType = AbilityTargetType.Vector,
            Range = static (_) => 800,
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                { 
                    // UnitRelationship.Visible 通常不需要在Required中指定，用户请求的指令必定会检测目标可见性。若在Required中指定，则代表即使是脚本调用的指令也需要检测目标可见性。
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }],
            },
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = TargetingIndicator.TestTargetingIndicator,
                CursorRadius = static (_) => 256,
                VectorLineWidth = static (_) => 128,
                VectorHighlightLimit = static (_) => 1
            },
            LogExecutionFailure = true,
            Animation = [Animation.TestSpell],
        };

        // ChargedTestSpell: 2秒冷却 + 10秒充能时间，最大充能数为2的技能
        var chargedSpellCooldownLink = new GameLink<GameDataCooldown, GameDataCooldownActive>("ChargedTestSpellCooldown"u8);
        _ = new GameDataCooldownActive(chargedSpellCooldownLink)
        {
            Time = static (_) => TimeSpan.FromSeconds(2),
        };
        var chargedSpellChargeLink = new GameLink<GameDataCooldown, GameDataCooldownCharge>("ChargedTestSpellCharge"u8);
        _ = new GameDataCooldownCharge(chargedSpellChargeLink)
        {
            Time = static (_) => TimeSpan.FromSeconds(10),
            ChargeMax = static (_) => 2,
            ChargeStart = static (_) => 0,
        };
        _ = new GameDataAbilityExecute(Ability.ChargedTestSpell)
        {
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.3),
                Backswing = static (_) => TimeSpan.FromSeconds(0.5),
            },
            Cost = new()
            {
                Cooldown = chargedSpellCooldownLink,
                Charge = chargedSpellChargeLink
            },
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = linkTestEffectLaunchMissile,
            TargetType = AbilityTargetType.Vector,
            Range = static (_) => 600,
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                {
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }],
            },
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = TargetingIndicator.TestTargetingIndicator,
                CursorRadius = static (_) => 256,
                VectorLineWidth = static (_) => 128,
                VectorHighlightLimit = static (_) => 1
            },
            LogExecutionFailure = true,
            Animation = [Animation.TestSpell],
        };

        _ = new GameDataTargetingIndicator(TargetingIndicator.TestTargetingIndicator)
        {
            CursorActors = [Actor.AOEActor],
            RangeActors = [Actor.RangeActor],
            VectorLineActors = [Actor.LineSegment],
            PreTargetingHighlightActors = [
                Actor.PreTargetingHighlight,
                ],
            Flags = new()
            {
                // DebugDraw = true
            },
        };
        
        // 直线型指示器 - 没有cursorActor
        _ = new GameDataTargetingIndicator(TargetingIndicator.LineIndicator)
        {
            RangeActors = [Actor.RangeActor],
            VectorLineActors = [Actor.LineSegment],
            PreTargetingHighlightActors = [
                Actor.PreTargetingHighlight,
                ],
            Flags = new()
            {
                // DebugDraw = true
            },
        };
        
        // 区域指示器 - 没有vectorLineActors
        _ = new GameDataTargetingIndicator(TargetingIndicator.AreaIndicator)
        {
            CursorActors = [Actor.AOEActor],
            RangeActors = [Actor.RangeActor],
            PreTargetingHighlightActors = [
                Actor.PreTargetingHighlight,
                ],
            Flags = new()
            {
                // DebugDraw = true
            },
        };
        _ = new GameDataActorParticle(Actor.RangeActor)
        {
            Particle = Particle.RangeParticle,
        };
        _ = new GameDataActorParticle(Actor.AOEActor)
        {
            Particle = Particle.AOEParticle,
        };
        _ = new GameDataActorParticle(Actor.LineLaunchActor)
        {
            Particle = Particle.LineLaunchParticle,
        };
        _ = new GameDataActorParticle(Actor.LineCenterActor)
        {
            Particle = Particle.LineCenterParticle,
        };
        _ = new GameDataActorParticle(Actor.LineImpactActor)
        {
            Particle = Particle.LineImpactParticle,
        };
        _ = new GameDataActorParticle(Actor.PreTargetingCircle)
        {
            Particle = Particle.PreTargetingCircle,
            InheritRotation = false,
        };
        _ = new GameDataParticle(Particle.RangeParticle)
        {
            Asset = "effect/effect_new/effect_zhishiqi/eff_zhishiqi/ps_yuanxing_1/particle.effect"u8,
            Radius = 70,
        };
        _ = new GameDataParticle(Particle.PreTargetingCircle)
        {
            Asset = "effect/effect_new/effect_guanghuan/eff_boss_guanghuan/particle.effect"u8,
            AssetLayerScale = 0.4f,
            Radius = 51.2f,
        };
        _ = new GameDataParticle(Particle.AOEParticle)
        {
            Asset = "effect/effect_new/effect_zhishiqi/eff_zhishiqi/ps_jineng_1/particle.effect"u8,
            Radius = 66,
        };
        _ = new GameDataParticle(Particle.LineLaunchParticle)
        {
            Asset = "effect/effect_new/effect_zhishiqi/eff_juxing/ps_wei_1/particle.effect"u8,
            Volume = new(128, 64, 128),
        };
        _ = new GameDataParticle(Particle.LineCenterParticle)
        {
            Asset = "effect/effect_new/effect_zhishiqi/eff_juxing/ps_zhong_1/particle.effect"u8,
            Volume = new(128, 64, 128),
        };
        _ = new GameDataParticle(Particle.LineImpactParticle)
        {
            Asset = "effect/effect_new/effect_zhishiqi/eff_zhixianxing/ps_tou_1/particle.effect"u8,
            Volume = new(128, 64, 128),
        };
        _ = new GameDataActorSegmentedRectangle(Actor.LineSegment)
        {
            HeightTest = ActorSegmentedHeight.LaunchSiteGround,
            LaunchActor = Actor.LineLaunchActor,
            CenterActor = Actor.LineCenterActor,
            ImpactActor = Actor.LineImpactActor,
            SegmentedFlags = new()
            {
                // DebugDraw = true,
            }
        };
        _ = new GameDataActorHighlight(Actor.PreTargetingHighlight)
        {
            From = new()
            {
                Value = new(255, 192, 192)
            },
            To = new()
            {
                Value = new(255, 128, 128)
            },
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            Duration = TimeSpan.FromSeconds(0.15),
        };

        // 创建-150移动速度Buff数据
        _ = new GameDataBuff(Buff.SpeedDebuff)
        {
            DisplayName = new LocalizedString("速度降低"),
            Description = new LocalizedString("移动速度降低150点"),
            SyncType = SyncType.Self,
            Polarity = BuffPolarity.Negative,
            StackStart = 1,
            Duration = (_) => TimeSpan.FromSeconds(5),
            BuffFlags = new BuffFlags
            {
                SingleInstancePerCaster = true,
                Channeling = false
            },
            Modifications = new()
            {
                new()
                {
                    Property = UnitProperty.MoveSpeed,
                    SubType = PropertySubType.Base,
                    Value = (_) => -150.0 // 减少150移动速度
                }
            }
        };


        // Buff definitions
        _ = new GameDataBuff(Buff.Stun)
        {
            Name = "眩晕",
            // DisplayName = "眩晕",
            Description = "无法移动和施法",
            Duration = static (_) => TimeSpan.FromSeconds(5),
            Icon = "image/buff/buff_1.png",
            ActorArray = [Actor.AOEActor],
            Polarity = GameCore.BuffSystem.Data.Enum.BuffPolarity.Negative, // 🎯 设置为负面效果
        };

        _ = new GameDataBuff(Buff.TestBuff)
        {
            Name = "测试Buff",
            Description = "用于测试的永久Buff效果",
            Icon = "image/buff/buff_2.png",
            Polarity = GameCore.BuffSystem.Data.Enum.BuffPolarity.Positive, // 🎯 设置为正面效果
            StackStart = 2, // 🎯 起始堆叠数改为2
            StackMax = static (_) => 5, // 🎯 最大堆叠数改为5
            BuffFlags = new()
            {
                Permanent = true
            }
        };

        // Effect definitions for adding buffs
        _ = new GameDataEffectBuffAdd(Effect.AddStunBuff)
        {
            BuffLink = Buff.Stun,
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectBuffAdd(Effect.AddTestBuff)
        {
            BuffLink = Buff.TestBuff,
            LogExecutionFailure = true,
        };

        // Effect definition for removing all buffs
        _ = new GameDataEffectBuffRemove(Effect.RemoveAllBuffs)
        {
            LogExecutionFailure = true,
        };

        // 塔防怪物单位配置
        _ = new GameDataUnit(Unit.TDMonster)
        {
            Name = "塔防小怪",
            AttackableRadius = 40,
            Properties = new() {
                { UnitProperty.LifeMax, 200 },
                { UnitProperty.ManaMax, 100 },
                { UnitProperty.Armor, 5 },
                { UnitProperty.MagicResistance, 5 },
                { UnitProperty.MoveSpeed, 200 }, // 较慢移动速度
                { UnitProperty.TurningSpeed, 900 },
                { UnitProperty.AttackRange, 100 },
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
            CollisionRadius = 24,
            DynamicCollisionMask = DynamicCollisionMask.None, // 敌方单位无视碰撞，可以穿过其他单位
            Filter = [UnitFilter.Unit],
            State = [UnitState.SuppressStaticCollision], // 无视静态碰撞，可以穿过地形障碍
            DeathRemovalDelay = TimeSpan.FromSeconds(5), // 死亡5秒后移除
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                },
            },
            Model = Model.HostTestHero, // 暂时使用现有模型
            TacticalAI = AI.CombatTest, // 添加AI行为
        };

        // 塔防英雄单位配置 - 专门用于塔防游戏
        _ = new GameDataUnit(Unit.TowerDefenseHero)
        {
            Name = "塔防英雄",
            AttackableRadius = 50,
            Properties = new() {
                { UnitProperty.LifeMax, 20 }, // 塔防英雄血量设置为20点
                { UnitProperty.ManaMax, 100 },
                { UnitProperty.Armor, 0 },
                { UnitProperty.MagicResistance, 0 },
                { UnitProperty.MoveSpeed, 400 }, // 英雄移动速度
                { UnitProperty.TurningSpeed, 1800 },
                { UnitProperty.AttackRange, 150 },
                { UnitProperty.InventoryPickUpRange, 300 },
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
            DynamicCollisionMask = DynamicCollisionMask.None, // 塔防英雄无视动态碰撞，可以穿过其他单位
            Inventories = [Inventory.TestInventory6, Inventory.TestInventory6Equip],
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            State = [UnitState.SuppressStaticCollision], // 无视静态碰撞，可以穿过地形障碍
            // Model = Model.HostTestHero, // 使用现有的英雄模型
            Abilities = [ // 四种建造技能
                // Ability.TestSpell,
                // Ability.BuildSlowTower,
                // Ability.BuildAuraSlowTower,
                // Ability.BuildAOETower,
                // Ability.BuildPenetrateTower,
            ],
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.FRIENDLY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                },
            },
        };

        // ========== 四种塔防建筑的冷却时间配置 ==========
        _ = new GameDataCooldownActive(new GameLink<GameDataCooldown, GameDataCooldownActive>("BuildSlowTowerCooldown"u8))
        {
            Time = static (_) => TimeSpan.FromSeconds(1.5)
        };

        _ = new GameDataCooldownActive(new GameLink<GameDataCooldown, GameDataCooldownActive>("BuildAuraSlowTowerCooldown"u8))
        {
            Time = static (_) => TimeSpan.FromSeconds(1.2)
        };

        _ = new GameDataCooldownActive(new GameLink<GameDataCooldown, GameDataCooldownActive>("BuildAOETowerCooldown"u8))
        {
            Time = static (_) => TimeSpan.FromSeconds(2.0)
        };

        _ = new GameDataCooldownActive(new GameLink<GameDataCooldown, GameDataCooldownActive>("BuildPenetrateTowerCooldown"u8))
        {
            Time = static (_) => TimeSpan.FromSeconds(2.5)
        };

        // ========== 四种塔防建筑的建造效果配置 ==========
        _ = new GameDataBuildingEffect(BuildingEffect.BuildSlowTowerEffect)
        {
            BuildingUnit = Unit.SlowTower,
            SuccessRate = 1.0f,
            CheckCollision = true,
            Offset = Vector3.Zero
        };

        _ = new GameDataBuildingEffect(BuildingEffect.BuildAuraSlowTowerEffect)
        {
            BuildingUnit = Unit.AuraSlowTower,
            SuccessRate = 1.0f,
            CheckCollision = true,
            Offset = Vector3.Zero
        };

        _ = new GameDataBuildingEffect(BuildingEffect.BuildAOETowerEffect)
        {
            BuildingUnit = Unit.AOETower,
            SuccessRate = 1.0f,
            CheckCollision = true,
            Offset = Vector3.Zero
        };

        _ = new GameDataBuildingEffect(BuildingEffect.BuildPenetrateTowerEffect)
        {
            BuildingUnit = Unit.PenetrateTower,
            SuccessRate = 1.0f,
            CheckCollision = true,
            Offset = Vector3.Zero
        };

        // ========== 四种塔防建筑的建造技能配置 ==========
        
        // 1. 建造单体减速塔
        _ = new GameDataAbilityExecuteBuilding(Ability.BuildSlowTower)
        {
            Name = "BuildSlowTower",
            DisplayName = "建造减速塔",
            Description = "建造一座发射减速投射物的防御塔",
            TargetType = AbilityTargetType.Ground,
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.2),
                Channel = static (_) => TimeSpan.FromSeconds(1.2),
                Backswing = static (_) => TimeSpan.FromSeconds(0.5)
            },
            
            Range = static (_) => 10000f,
            Cost = new()
            {
                Cooldown = new GameLink<GameDataCooldown, GameDataCooldownActive>("BuildSlowTowerCooldown"u8)
            },
            
            Effect = BuildingEffect.BuildSlowTowerEffect,
            Unit = Unit.SlowTower,
            PreviewActor = Actor.SlowTowerPreviewActor,
            PreviewOffset = Vector3.Zero,
            PreviewAlpha = 0.7f,
            GridSnapSize = 64f * 4, // 4x4网格
            ShowInvalidPreview = true
        };

        // 2. 建造光环减速塔
        _ = new GameDataAbilityExecuteBuilding(Ability.BuildAuraSlowTower)
        {
            Name = "BuildAuraSlowTower",
            DisplayName = "建造光环塔",
            Description = "建造一座释放减速光环的防御塔",
            TargetType = AbilityTargetType.Ground,
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.2),
                Channel = static (_) => TimeSpan.FromSeconds(1.0),
                Backswing = static (_) => TimeSpan.FromSeconds(0.5)
            },
            
            Range = static (_) => 10000f,
            Cost = new()
            {
                Cooldown = new GameLink<GameDataCooldown, GameDataCooldownActive>("BuildAuraSlowTowerCooldown"u8)
            },
            
            Effect = BuildingEffect.BuildAuraSlowTowerEffect,
            Unit = Unit.AuraSlowTower,
            PreviewActor = Actor.AuraSlowTowerPreviewActor,
            PreviewOffset = Vector3.Zero,
            PreviewAlpha = 0.7f,
            GridSnapSize = 64f * 4, // 4x4网格
            ShowInvalidPreview = true
        };

        // 3. 建造群体伤害塔
        _ = new GameDataAbilityExecuteBuilding(Ability.BuildAOETower)
        {
            Name = "BuildAOETower",
            DisplayName = "建造爆炸塔",
            Description = "建造一座造成群体伤害的防御塔",
            TargetType = AbilityTargetType.Ground,
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.2),
                Channel = static (_) => TimeSpan.FromSeconds(1.3),
                Backswing = static (_) => TimeSpan.FromSeconds(0.5)
            },
            
            Range = static (_) => 10000f,
            Cost = new()
            {
                Cooldown = new GameLink<GameDataCooldown, GameDataCooldownActive>("BuildAOETowerCooldown"u8)
            },
            
            Effect = BuildingEffect.BuildAOETowerEffect,
            Unit = Unit.AOETower,
            PreviewActor = Actor.AOETowerPreviewActor,
            PreviewOffset = Vector3.Zero,
            PreviewAlpha = 0.7f,
            GridSnapSize = 64f * 4, // 4x4网格
            ShowInvalidPreview = true
        };

        // 4. 建造向量穿透塔
        _ = new GameDataAbilityExecuteBuilding(Ability.BuildPenetrateTower)
        {
            Name = "BuildPenetrateTower",
            DisplayName = "建造穿透塔",
            Description = "建造一座发射穿透攻击的防御塔",
            TargetType = AbilityTargetType.Ground,
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.2),
                Channel = static (_) => TimeSpan.FromSeconds(1.5),
                Backswing = static (_) => TimeSpan.FromSeconds(0.5)
            },
            
            Range = static (_) => 10000f,
            Cost = new()
            {
                Cooldown = new GameLink<GameDataCooldown, GameDataCooldownActive>("BuildPenetrateTowerCooldown"u8)
            },
            
            Effect = BuildingEffect.BuildPenetrateTowerEffect,
            Unit = Unit.PenetrateTower,
            PreviewActor = Actor.PenetrateTowerPreviewActor,
            PreviewOffset = Vector3.Zero,
            PreviewAlpha = 0.7f,
            GridSnapSize = 64f * 4, // 4x4网格
            ShowInvalidPreview = true
        };

        // 建造预览Actor配置
        _ = new GameDataActorModel(Actor.TowerPreviewActor)
        {
            Name = "防御塔预览Actor",
            Model = Model.HostTestHero, // 暂时使用现有模型，后续可以替换为专门的预览模型
            Offset = Vector3.Zero,
            // 添加更多配置确保客户端可以创建
            // 可能需要的其他属性...
        };

        _ = new GameDataActorModel(Actor.WallPreviewActor)
        {
            Name = "城墙预览Actor",
            Model = Model.HostTestHero, // 暂时使用现有模型，后续可以替换为专门的预览模型
            Offset = Vector3.Zero
        };

        // 四种塔的预览Actor配置
        _ = new GameDataActorModel(Actor.SlowTowerPreviewActor)
        {
            Name = "减速塔预览Actor",
            Model = Model.SlowTowerModel, // 使用减速塔的模型
            Offset = Vector3.Zero,
        };

        _ = new GameDataActorModel(Actor.AuraSlowTowerPreviewActor)
        {
            Name = "光环塔预览Actor",
            Model = Model.AuraSlowTowerModel, // 使用光环塔的模型
            Offset = Vector3.Zero,
        };

        _ = new GameDataActorModel(Actor.AOETowerPreviewActor)
        {
            Name = "爆炸塔预览Actor",
            Model = Model.AOETowerModel, // 使用爆炸塔的模型
            Offset = Vector3.Zero,
        };

        _ = new GameDataActorModel(Actor.PenetrateTowerPreviewActor)
        {
            Name = "穿透塔预览Actor",
            Model = Model.PenetrateTowerModel, // 使用穿透塔的模型
            Offset = Vector3.Zero,
        };

        // 建造网格模型配置
        _ = new GameDataModel(Model.BuildGridModel)
        {
            Name = "建造网格模型_红色",
            Asset = "deco/engine/SM_Plane_A01/model.prefab", // 红色网格
            AssetLayerScale = 0.5f,
        };

        _ = new GameDataModel(Model.BuildGridModel_1)
        {
            Name = "建造网格模型_绿色", 
            Asset = "deco/engine/SM_Plane_A02/model.prefab", // 绿色网格
            AssetLayerScale = 0.5f,
        };

        _ = new GameDataModel(Model.BuildGridModel_2)
        {
            Name = "建造网格模型_黄色",
            Asset = "deco/engine/SM_Plane_A03/model.prefab", // 黄色网格
            AssetLayerScale = 0.5f,
        };

        // 建造网格Actor配置（用于显示有效/无效位置）
        _ = new GameDataActorGrid(Actor.BuildingValidGrid)
        {
            Name = "建造有效网格",
            GridSize = 64f, // 增大网格尺寸以显示2x2区域
            StateModels = new List<IGameLink<GameDataModel>?>
            {
                null,                   // 0: 隐藏格子
                Model.BuildGridModel_1, // 1: 默认显示（绿色 - SM_Plane_A02）
                Model.BuildGridModel_1, // 2: 有效高亮（绿色 - SM_Plane_A02）
                Model.BuildGridModel,   // 3: 无效高亮（红色 - SM_Plane_A01）
                Model.BuildGridModel_1  // 4: 保留（绿色 - SM_Plane_A02）
            }
        };

        Game.Logger.LogInformation("🏗️ 建造系统数编表初始化完成");
        Game.Logger.LogInformation("🎭 建造预览Actor配置完成");
        Game.Logger.LogInformation("🦸 英雄单位已配置建造技能作为默认技能");

        // ========== 塔防建筑系统配置 ==========
        InitializeTowerDefenseBuildings();
        
        Game.Logger.LogInformation("🏰 塔防建筑系统初始化完成");
    }

    /// <summary>
    /// 初始化塔防建筑系统
    /// </summary>
    private static void InitializeTowerDefenseBuildings()
    {
        // ========== 模型配置 ==========
        _ = new GameDataModel(Model.SlowTowerModel)
        {
            Name = "单体减速塔模型",
            Asset = "deco/dungeon/sk_garden_stonetower_a03/model.prefab",
        };

        _ = new GameDataModel(Model.AuraSlowTowerModel)
        {
            Name = "光环减速塔模型",
            Asset = "deco/dungeon/sk_garden_stonetower_a/model.prefab",
        };

        _ = new GameDataModel(Model.AOETowerModel)
        {
            Name = "群体伤害塔模型",
            Asset = "deco/dungeon/sk_garden_stonetower_a05/model.prefab",
        };

        _ = new GameDataModel(Model.PenetrateTowerModel)
        {
            Name = "向量穿透塔模型",
            Asset = "deco/dungeon/sk_garden_stonetower_a04/model.prefab",
        };
        // ========== 塔防建筑等级系统配置 ==========
        
        // 1. 减速塔等级系统
        _ = new GameDataUnitLeveling(UnitLeveling.SlowTowerLeveling)
        {
            ExperienceRequiredForEachLevel = [0, 100, 200, 400, 800, 1600], // 每级所需经验：1级0经验，2级100经验，3级200经验...
            Modifications = new()
            {
                // 每级提升攻击力和血量
                new() { Property = UnitProperty.AttackDamage, SubType = PropertySubType.Base, Value = (context) => 50.0 }, // 1级攻击力
                new() { Property = UnitProperty.LifeMax, SubType = PropertySubType.Base, Value = (context) => 800.0 }, // 1级血量
            }
        };

        // 2. 光环减速塔等级系统
        _ = new GameDataUnitLeveling(UnitLeveling.AuraSlowTowerLeveling)
        {
            ExperienceRequiredForEachLevel = [0, 120, 240, 480, 960, 1920],
            Modifications = new()
            {
                new() { Property = UnitProperty.AttackDamage, SubType = PropertySubType.Base, Value = (context) => 40.0 }, // 1级攻击力
                new() { Property = UnitProperty.LifeMax, SubType = PropertySubType.Base, Value = (context) => 700.0 }, // 1级血量
            }
        };

        // 3. 群体伤害塔等级系统
        _ = new GameDataUnitLeveling(UnitLeveling.AOETowerLeveling)
        {
            ExperienceRequiredForEachLevel = [0, 150, 300, 600, 1200, 2400],
            Modifications = new()
            {
                new() { Property = UnitProperty.AttackDamage, SubType = PropertySubType.Base, Value = (context) => 80.0 }, // 1级攻击力
                new() { Property = UnitProperty.LifeMax, SubType = PropertySubType.Base, Value = (context) => 600.0 }, // 1级血量
            }
        };

        // 4. 向量穿透塔等级系统
        _ = new GameDataUnitLeveling(UnitLeveling.PenetrateTowerLeveling)
        {
            ExperienceRequiredForEachLevel = [0, 200, 400, 800, 1600, 3200],
            Modifications = new()
            {
                new() { Property = UnitProperty.AttackDamage, SubType = PropertySubType.Base, Value = (context) => 100.0 }, // 1级攻击力
                new() { Property = UnitProperty.LifeMax, SubType = PropertySubType.Base, Value = (context) => 900.0 }, // 1级血量
            }
        };

        // ========== 塔单位配置 ==========
        
        // 1. 单体减速塔
        _ = new GameDataUnit(Unit.SlowTower)
        {
            Name = "单体减速塔",
            AttackableRadius = 400,
            Leveling = UnitLeveling.SlowTowerLeveling, // 🔧 添加等级系统
            Properties = new()
            {
                { UnitProperty.LifeMax, 800 },
                { UnitProperty.AttackRange, 400 },
                { UnitProperty.AttackDamage, 50 }, // 基础攻击力（1级）
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax }
            },
            CollisionRadius = 48,
            DynamicCollisionMask = DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Structure], // 🏗️ 改为建筑类型标识
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            Model = Model.SlowTowerModel,
            Abilities = [
                Ability.SlowProjectilePassive  // ✅ 只保留被动技能 (自动攻击)
            ],
            // 4x4网格足迹
            Footprint = new Footprint(4, 4, GameCore.CollisionSystem.Data.Enum.CollisionType.Static)
            {
                // 设置所有4x4格子都被占用
                [0, 0] = true, [1, 0] = true, [2, 0] = true, [3, 0] = true,
                [0, 1] = true, [1, 1] = true, [2, 1] = true, [3, 1] = true,
                [0, 2] = true, [1, 2] = true, [2, 2] = true, [3, 2] = true,
                [0, 3] = true, [1, 3] = true, [2, 3] = true, [3, 3] = true
            },
        };

        // 2. 光环减速塔
        _ = new GameDataUnit(Unit.AuraSlowTower)
        {
            Name = "光环减速塔",
            AttackableRadius = 300,
            Leveling = UnitLeveling.AuraSlowTowerLeveling, // 🔧 添加等级系统
            Properties = new()
            {
                { UnitProperty.LifeMax, 700 },
                { UnitProperty.AttackRange, 300 },
                { UnitProperty.AttackDamage, 40 }, // 基础攻击力（1级）
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax }
            },
            CollisionRadius = 48,
            DynamicCollisionMask = DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Structure], // 🏗️ 改为建筑类型标识
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            Model = Model.AuraSlowTowerModel,
            Abilities = [
                Ability.AuraSlowPassive  // ✅ 只保留被动技能 (自动攻击)
            ],            
            // 4x4网格足迹
            Footprint = new Footprint(4, 4, GameCore.CollisionSystem.Data.Enum.CollisionType.Static)
            {
                // 设置所有4x4格子都被占用
                [0, 0] = true, [1, 0] = true, [2, 0] = true, [3, 0] = true,
                [0, 1] = true, [1, 1] = true, [2, 1] = true, [3, 1] = true,
                [0, 2] = true, [1, 2] = true, [2, 2] = true, [3, 2] = true,
                [0, 3] = true, [1, 3] = true, [2, 3] = true, [3, 3] = true
            },
        };

        // 3. 群体伤害塔
        _ = new GameDataUnit(Unit.AOETower)
        {
            Name = "群体伤害塔",
            AttackableRadius = 250,
            Leveling = UnitLeveling.AOETowerLeveling, // 🔧 添加等级系统
            Properties = new()
            {
                { UnitProperty.LifeMax, 600 },
                { UnitProperty.AttackRange, 250 },
                { UnitProperty.AttackDamage, 80 }, // 基础攻击力（1级）
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax }
            },
            CollisionRadius = 48,
            DynamicCollisionMask = DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Structure], // 🏗️ 改为建筑类型标识
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            Model = Model.AOETowerModel,
            Abilities = [
                Ability.AOEDamagePassive  // ✅ 只保留被动技能 (自动攻击)
            ],
            // 4x4网格足迹
            Footprint = new Footprint(4, 4, GameCore.CollisionSystem.Data.Enum.CollisionType.Static)
            {
                // 设置所有4x4格子都被占用
                [0, 0] = true, [1, 0] = true, [2, 0] = true, [3, 0] = true,
                [0, 1] = true, [1, 1] = true, [2, 1] = true, [3, 1] = true,
                [0, 2] = true, [1, 2] = true, [2, 2] = true, [3, 2] = true,
                [0, 3] = true, [1, 3] = true, [2, 3] = true, [3, 3] = true
            },
        };

        // 4. 向量穿透塔
        _ = new GameDataUnit(Unit.PenetrateTower)
        {
            Name = "向量穿透塔",
            AttackableRadius = 3000, // 🎯 更新攻击范围为3000 (如主人要求)
            Leveling = UnitLeveling.PenetrateTowerLeveling, // 🔧 添加等级系统
            Properties = new()
            {
                { UnitProperty.LifeMax, 900 },
                { UnitProperty.AttackRange, 3000 }, // 🎯 更新攻击范围为3000
                { UnitProperty.AttackDamage, 100 }, // 基础攻击力（1级）
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax }
            },
            CollisionRadius = 48,
            DynamicCollisionMask = DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Structure], // 🏗️ 改为建筑类型标识
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            Model = Model.PenetrateTowerModel,
            Abilities = [
                Ability.PenetrateAttackPassive  // ✅ 只保留被动技能 (自动攻击)
            ],
            // 4x4网格足迹
            Footprint = new Footprint(4, 4, GameCore.CollisionSystem.Data.Enum.CollisionType.Static)
            {
                // 设置所有4x4格子都被占用
                [0, 0] = true, [1, 0] = true, [2, 0] = true, [3, 0] = true,
                [0, 1] = true, [1, 1] = true, [2, 1] = true, [3, 1] = true,
                [0, 2] = true, [1, 2] = true, [2, 2] = true, [3, 2] = true,
                [0, 3] = true, [1, 3] = true, [2, 3] = true, [3, 3] = true
            },
        };

        // ============================================================
        // 塔防刷怪系统配置数据
        // ============================================================

        // 创建刷怪器配置
        _ = new GameDataSpawnerBasic(Spawner.WolfSpawner)
        {
            Monster = Unit.TDMonster, // 使用现有的塔防怪物
            Times = 5,
            Number = 1,
            Pulse = 1.0f,
            Delay = 0.0f,
            LineEx = "玩家1前进路线",
            Enabled = true
        };

        _ = new GameDataSpawnerBasic(Spawner.CrawlerSpawner)
        {
            Monster = Unit.TDMonster,
            Times = 2,
            Number = 1,
            Pulse = 1.0f,
            Delay = 5.0f,
            LineEx = "玩家1前进路线",
            Enabled = true
        };

        _ = new GameDataSpawnerBasic(Spawner.VultureSpawner)
        {
            Monster = Unit.TDMonster,
            Times = 2,
            Number = 1,
            Pulse = 1.0f,
            Delay = 0.0f,
            LineEx = "玩家1前进路线",
            Enabled = true
        };

        _ = new GameDataSpawnerBasic(Spawner.WaterElementalSpawner)
        {
            Monster = Unit.TDMonster,
            Times = 2, // 减少到2个
            Number = 1,
            Pulse = 2.0f, // 增加间隔到2秒
            Delay = 0.0f,
            LineEx = "玩家1前进路线",
            Enabled = true
        };

        _ = new GameDataSpawnerBasic(Spawner.BoarSpawner)
        {
            Monster = Unit.TDMonster,
            Times = 3, // 减少到3个
            Number = 1,
            Pulse = 2.0f, // 增加间隔到2秒
            Delay = 5.0f,
            LineEx = "玩家1前进路线",
            Enabled = true
        };

        // 创建波次配置
        _ = new GameDataWaveBasic(Wave.Wave1)
        {
            WaveName = "第一波",
            WaveDelay = 5.0f,
            WaveData = [
                Spawner.WolfSpawner,
                Spawner.CrawlerSpawner
            ],
            Enabled = true
        };

        _ = new GameDataWaveBasic(Wave.Wave2)
        {
            WaveName = "第二波",
            WaveDelay = 15.0f,
            WaveData = [
                Spawner.VultureSpawner,
                Spawner.VultureSpawner
            ],
            Enabled = true
        };

        _ = new GameDataWaveBasic(Wave.Wave3)
        {
            WaveName = "第三波",
            WaveDelay = 30.0f,
            WaveData = [
                Spawner.WaterElementalSpawner,
                Spawner.BoarSpawner
            ],
            Enabled = true
        };

        // 创建关卡配置
        _ = new GameDataLevelBasic(Level.DefaultLevel)
        {
            LevelName = "默认塔防关卡",
            Description = "塔防游戏的默认关卡，包含3个波次的怪物",
            DifficultyLevel = 1,
            InitialPlayerHealth = 20,
            InitialPlayerGold = 10,
            Waves = [
                Wave.Wave1,
                Wave.Wave2,
                Wave.Wave3
            ],
            Enabled = true
        };

        Game.Logger.LogInformation("✅ Tower Defense spawn system data initialized successfully!");
    }
}
