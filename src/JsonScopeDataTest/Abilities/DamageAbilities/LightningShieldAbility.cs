using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.BuffSystem.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.ModelAnimation.Data;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.BaseType;
using GameCore.Struct;
using GameData;
using static GameCore.ScopeData;
using GameCore.Components;
using GameCore.Components.Data;

namespace GameEntry.JsonScopeDataTest.Abilities.DamageAbilities;

/// <summary>
/// 闪电之盾技能 ⚡
/// 为盟友单位创建环绕的闪电球，对碰撞到的敌人造成魔法伤害
/// </summary>
public class LightningShieldAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> LightningShield = new("JsonTestLightningShield"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> LightningShieldSearch = new("JsonTestLightningShieldSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectCreateUnit> LightningShieldCreateUnit = new("JsonTestLightningShieldCreateUnit"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectUnitMoverApply> LightningShieldMoverApply = new("JsonTestLightningShieldMoverApply"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> LightningShieldDamage = new("JsonTestLightningShieldDamage"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectLog> LightningShieldLog = new("JsonTestLightningShieldLog"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectLog> LightningShieldPrepareLog = new("JsonTestLightningShieldPrepareLog"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> LightningShieldDamageSet = new("JsonTestLightningShieldDamageSet"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> LightningShieldEffectSet = new("JsonTestLightningShieldEffectSet"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> LightningShieldPrepareSet = new("JsonTestLightningShieldPrepareSet"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> LightningOrbBuffAdd = new("JsonTestLightningOrbBuffAdd"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectUnitRemove> LightningOrbRemove = new("JsonTestLightningOrbRemove"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> LightningOrbInitSet = new("JsonTestLightningOrbInitSet"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectLog> LightningShieldEffectLog = new("JsonTestLightningShieldEffectLog"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectLog> LightningShieldPrepareSetLog = new("JsonTestLightningShieldPrepareSetLog"u8);
    #endregion

    #region 移动器定义
    public static readonly GameLink<GameDataMover, GameDataMoverOrbit> LightningShieldOrbitMover = new("JsonTestLightningShieldOrbitMover"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> LightningShieldCooldown = new("JsonTestLightningShieldCooldown"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> LightningOrbLifetimeBuff = new("JsonTestLightningOrbLifetimeBuff"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> LightningShieldAnim = new("JsonTestLightningShieldAnim"u8);
    #endregion

    #region 单位定义
    public static readonly GameLink<GameDataUnit, GameDataUnit> LightningOrbUnit = new("JsonTestLightningOrbUnit"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> LightningHitParticle = new("JsonTestLightningHitParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> LightningHitEffect = new("JsonTestLightningHitEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorAction> LightningShieldAction = new("JsonTestLightningShieldAction"u8);
    #endregion

    #region 瞄准指示器定义
    public static readonly GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator> CircleIndicator = new("p_0tja.ScopeData.GameDataTargetingIndicator.CircleIndicator.Root"u8);
    #endregion

    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        Game.Logger.LogInformation("⚡ Initializing Lightning Shield Ability...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(LightningShieldAnim)
        {
            Name = "闪电之盾动画",
            File = "anim/human/barehanded_anim/hand_05/skill_05.ani"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(LightningHitParticle)
        {
            Asset = "effect/samplespells/lightningshield/lightningshield_target/particle.effect"u8,
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(LightningHitEffect)
        {
            AutoPlay = true,
            Particle = LightningHitParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        _ = new GameDataActorAction(LightningShieldAction)
        {
            LaunchSocket = "socket_hand_r"u8,
            ImpactSocket = "socket_hit"u8,
        };

        // ========== 闪电球单位配置 ==========
        _ = new GameDataUnit(LightningOrbUnit)
        {
            Name = "闪电球",
            Filter = [UnitFilter.Missile],
            State = [UnitState.Invulnerable],
            CollisionRadius = 16, // 参考INI中UnitData.CollisionRadius
            AttackableRadius = 50, // 参考INI中AttackableRadius
            Particle = "effect/samplespells/lightningshield/lightningshield_orb/particle.effect"u8,
            UpdateFlags = new()
            {
                AllowMover = true,
            },
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(LightningShieldCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(40.0), // 参考INI中Cooldown = 40
        };

        // ========== Buff配置 ==========
        _ = new GameDataBuff(LightningOrbLifetimeBuff)
        {
            Name = "闪电球生命周期",
            Duration = static (_) => TimeSpan.FromSeconds(30), // 30秒持续时间
            ExpireEffect = LightningOrbRemove, // Buff到期时删除闪电球单位
        };

        // ========== 轨道移动器配置 ========== 
        _ = new GameDataMoverOrbit(LightningShieldOrbitMover)
        {
            Name = "闪电之盾轨道移动器",
            // 轨道角速度 - 每秒180度，参考INI中angle_speed = 180
            OrbitAngularVelocity = static (_) => 180f,
            // 轨道半径 - 参考INI中distance = 200
            OrbitRadius = static (_) => 200f,
            // 轨道高度 - 参考INI中height = 100
            OrbitHeight = static (_) => 100f,
            // 不跟随目标朝向 - 参考INI中angle_follow = false
            OrbitRelativeToTargetFacing = false,
            // 自转速率 - 参考INI中face_speed = 180
            SelfRotationRate = static (_) => 180f,
            // 自转相对于目标 - 默认false
            SelfRotationRelativeToTarget = false,
            // 高度相对于目标 - 参考INI中height_follow = true
            HeightRelativeToTarget = true,
            // 始终对轨道目标可见
            AlwaysVisibleToOrbitTarget = true,
            //间隔时间 - 参考INI中interval_time = 0.1
            ImpactSameTargetInterval = TimeSpan.FromSeconds(0.1),
            // 🔥 碰撞检测配置 - 参考PR代码
            DoImpactEntity = true,
            ImpactSearchFilter = [
                new() {
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }
            ],
            // 碰撞检测半径 - 参考INI中hit_area = 40
            ImpactSearchRadius = static (_) => 40f,
            ImpactEffect = LightningShieldDamageSet, // 碰撞时执行伤害效果集合（包含log和伤害）
            ImpactMaxCount = static (_) => 99, // 允许更多碰撞次数，参考PR代码
            // 允许法术修正
            AllowSpellModification = true,
            ImpactUnitLocVar = LocVarType.B, // 存储碰撞目标，参考PR代码
        };

        // ========== 效果配置 ==========
        // 闪电伤害效果
        _ = new GameDataEffectDamage(LightningShieldDamage)
        {
            Name = "闪电之盾伤害",
            Amount = static (_) => 50, // 参考INI中Amount = 50
            Type = DamageType.Magical,
            LogExecutionFailure = true,
            ActorArray = [LightningHitEffect], // 添加击中特效
        };

        // Log效果 - 打印目标信息 
        _ = new GameDataEffectLog(LightningShieldLog)
        {
            Name = "闪电之盾Log",
            Message = static (e) => $"⚡ LightningShield hit target: {e.Target}",
            LogExecutionFailure = true,
        };

        // 准备Log效果 - 在创建闪电球前打印准备信息
        _ = new GameDataEffectLog(LightningShieldPrepareLog)
        {
            Name = "闪电之盾准备Log",
            Message = static (e) => $"⚡ Preparing Lightning Shield for target: {e.Target}",
            LogExecutionFailure = true,
        };

        // 效果Log - 查看LightningShieldEffectSet的Target信息
        _ = new GameDataEffectLog(LightningShieldEffectLog)
        {
            Name = "闪电之盾效果Log",
            Message = static (e) => $"⚡ LightningShieldEffectSet executing with target: {e.Target}",
            LogExecutionFailure = true,
        };

        // 准备Set的Log - 查看LightningShieldPrepareSet的Target信息
        _ = new GameDataEffectLog(LightningShieldPrepareSetLog)
        {
            Name = "闪电之盾准备Set Log",
            Message = static (e) => $"⚡ LightningShieldPrepareSet executing with target: {e.Target}",
            LogExecutionFailure = true,
        };

        // 伤害效果集合 - 包含Log和伤害
        _ = new GameDataEffectSet(LightningShieldDamageSet)
        {
            Name = "闪电之盾伤害集合",
            Effects = [
                new() { Link = LightningShieldLog }, // 先打印log
                new() { Link = LightningShieldDamage }, // 再执行伤害
            ],
            LogExecutionFailure = true,
        };

        // 应用轨道移动器效果 - 参考RollAbility的实现
        _ = new GameDataEffectUnitMoverApply(LightningShieldMoverApply)
        {
            Name = "应用闪电之盾轨道移动器",
            Mover = LightningShieldOrbitMover,
            LaunchEntity = new TargetLocationExpression { Value = TargetLocation.Target }, // 发射实体是施法者
            // MoverTarget = new TargetLocationExpression { Value = TargetLocation.Caster }, // 轨道目标是搜索到的盟友单位
            MoverTarget = new TargetLocationExpression { Effect = LightningShieldPrepareSet, Value = TargetLocation.Default },
            LogExecutionFailure = true,
        };

        // 效果集合 - 应用移动器
        _ = new GameDataEffectSet(LightningShieldEffectSet)
        {
            Name = "闪电之盾效果集合", 
            Effects = [
                new() { Link = LightningShieldEffectLog }, // 先打印目标信息用于调试
                new() { Link = LightningShieldMoverApply }, // 应用轨道移动器
            ],
            LogExecutionFailure = true,
        };

        // 准备效果集合 - 在创建闪电球前执行的效果
        _ = new GameDataEffectSet(LightningShieldPrepareSet)
        {
            Name = "闪电之盾准备效果集合",
            Effects = [
                new() { Link = LightningShieldPrepareSetLog }, // 先打印这个效果集合的target信息用于调试
                new() { Link = LightningShieldPrepareLog }, // 打印准备信息
                new() { Link = LightningShieldCreateUnit }, // 然后创建闪电球单位
            ],
            LogExecutionFailure = true,
        };

        // 移除闪电球单位（用于Buff到期时）
        _ = new GameDataEffectUnitRemove(LightningOrbRemove)
        {
            Name = "移除闪电球单位",
            LogExecutionFailure = true,
        };

        // 给闪电球添加生命周期Buff
        _ = new GameDataEffectBuffAdd(LightningOrbBuffAdd)
        {
            Name = "添加闪电球生命周期Buff",
            BuffLink = LightningOrbLifetimeBuff,
            LogExecutionFailure = true,
        };

        // 闪电球初始化效果集合 - 先添加生命周期Buff，再应用轨道移动器
        _ = new GameDataEffectSet(LightningOrbInitSet)
        {
            Name = "闪电球初始化效果集合",
            Effects = [
                new() { Link = LightningOrbBuffAdd }, // 添加生命周期Buff
                new() { Link = LightningShieldMoverApply }, // 应用轨道移动器
            ],
            LogExecutionFailure = true,
        };

        // 创建闪电球单位
        _ = new GameDataEffectCreateUnit(LightningShieldCreateUnit)
        {
            Name = "创建闪电球单位",
            LogExecutionFailure = true,
            SpawnCount = static (_) => 1, // 创建1个闪电球
            SpawnEffect = LightningOrbInitSet, // 创建后执行初始化效果集合（添加Buff + 应用移动器）
            SpawnOwner = new EffectOwnerExpression { Value = EffectOwner.TargetLocation }, // 闪电球归属于施法者
            // 使用UnitLink指定闪电球单位类型
            UnitPicker = UnitPicker.UnitLink,
            // 指定要创建的闪电球单位类型 ⚡
            SpawnUnitTypePerLevel = [LightningOrbUnit], // 所有等级都使用闪电球单位
        };

        // 搜索盟友单位
        _ = new GameDataEffectSearch(LightningShieldSearch)
        {
            Name = "搜索盟友单位",
            SearchFilters = [new() {
                Required = [UnitFilter.Unit, UnitRelationship.Alliance], // 搜索单位类型的盟友 - 参考INI中SearchFilter = '盟友,单位;死亡'
                Excluded = [UnitState.Dead] // 排除死亡单位
            },
            new() {
                Required = [UnitFilter.Unit, UnitRelationship.Self], // 搜索单位类型的盟友 - 参考INI中SearchFilter = '盟友,单位;死亡'
                Excluded = [UnitState.Dead] // 排除死亡单位
            },
            new() {
                Required = [UnitFilter.Unit, UnitRelationship.Player], // 搜索单位类型的盟友 - 参考INI中SearchFilter = '盟友,单位;死亡'
                Excluded = [UnitState.Dead] // 排除死亡单位
            }],
            TargetLocation = new TargetLocationExpression { Value = TargetLocation.Caster }, // 以技能目标位置为中心
            Method = SearchMethod.Circle, // 圆形搜索
            Radius = static (_) => 200, // 参考INI中Radius = 200
            MaxCount = static (_) => 99, // 不限制搜索数量
            Effect = LightningShieldPrepareSet, // 对每个找到的盟友执行准备效果集合
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(LightningShield)
        {
            Name = "闪电之盾",
            DisplayName = "闪电之盾",
            Description = "为范围内盟友单位创建环绕的闪电球，对接触的敌人造成魔法伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.35), // 参考INI中cast_start_time
                Channel = static (_) => TimeSpan.FromSeconds(0.14), // 参考INI中cast_shot_time  
                Backswing = static (_) => TimeSpan.FromSeconds(0.37), // 参考INI中cast_finish_time
            },
            
            Cost = new()
            {
                Cooldown = LightningShieldCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = LightningShieldSearch,
            TargetType = AbilityTargetType.None, // 地面目标
            Range = static (_) => 200, // 参考INI中Range = 200
            
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                { 
                    Required = [UnitFilter.Unit], // 搜索友方单位
                    Excluded = [UnitState.Dead]
                }],
            },
            
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = CircleIndicator,
                CursorRadius = static (_) => 200,
                VectorLineWidth = static (_) => 64,
                VectorHighlightLimit = static (_) => 1
            },
            
            Animation = [LightningShieldAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Lightning Shield Ability initialized successfully! ⚡");
    }
}
