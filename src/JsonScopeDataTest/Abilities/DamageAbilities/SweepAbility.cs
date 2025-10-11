using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.ModelAnimation.Data;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.SceneSystem.Data;
using GameData;
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using static GameCore.ScopeData;

namespace GameEntry.JsonScopeDataTest.Abilities.DamageAbilities;

/// <summary>
/// 伤害技能：横扫
/// 弧形范围物理伤害技能，对前方180度范围内敌人造成伤害
/// </summary>
public class SweepAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Sweep = new("JsonTestSweepSlash"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> SweepDamageSearch = new("JsonTestSweepDamageSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> SweepDamage = new("JsonTestSweepDamage"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> SweepCooldown = new("JsonTestSweepCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSequence> SweepAnim = new("JsonTestSweepAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> HitEffectParticle = new("JsonTestHitEffectParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> SweepEffectParticle = new("JsonTestSweepEffectParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> HitEffect = new("JsonTestHitEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> SweepEffect = new("JsonTestSweepEffect"u8);
    #endregion

    #region 瞄准指示器定义
    public static readonly GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator> LineIndicator = new("p_0tja.ScopeData.GameDataTargetingIndicator.LineIndicator.Root"u8);
    #endregion

    public static void OnRegisterGameClass()
    {
        // 先移除可能已存在的订阅，避免重复注册
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        // 只在测试模式下初始化（移除了ARPG模式的限制）
        Game.Logger.LogInformation("🗡️ Initializing Sweep Ability for Test...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSequence(SweepAnim)
        {
            Name = "横扫动画序列",
            Playbacks = [new() 
            {
                AnimationRaw = "anim/human/barehanded_anim/hand_05/skill_03.ani"u8,
                IsLooping = false,
                PlaybackDuration = TimeSpan.FromSeconds(0.335), // 根据entry_data.ini的cast_finish_time
            }],
            SequenceActors = [new() 
            {
                Actor = SweepEffect, // 在动画播放时生成横扫特效
                SpawnOffset = TimeSpan.FromSeconds(0.24), // 根据entry_data.ini的cast_start_time
                Duration = TimeSpan.FromSeconds(0.6), // 横扫特效持续时间
            }]
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(HitEffectParticle)
        {
            Asset = "effect/effect_new/effect_hit/eff_hit_29/particle.effect"u8,
        };

        _ = new GameDataParticle(SweepEffectParticle)
        {
            Asset = "effect/eff_reverse/particle/hades/daoguang/ps_daoguang_1/particle.effect"u8,
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(HitEffect)
        {
            AutoPlay = true,
            Particle = HitEffectParticle,
            KillOnFinish = true, // 播放完成后自动结束
            ForceOneShot = true, // 单次播放
        };

        _ = new GameDataActorParticle(SweepEffect)
        {
            AutoPlay = true,
            Particle = SweepEffectParticle,
            KillOnFinish = true, // 播放完成后自动结束
            ForceOneShot = true, // 单次播放
            Offset = new System.Numerics.Vector3(0, 0, 80), // 抬高80单位
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(SweepCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(1.0), // 1秒冷却
        };

        // ========== 效果配置 ==========
        // 横扫伤害效果
        _ = new GameDataEffectDamage(SweepDamage)
        {
            Name = "横扫伤害",
            Amount = static (_) => 100, // 基础伤害100
            Type = DamageType.Physical,
            LogExecutionFailure = true,
            ActorArray = [HitEffect], // 添加受击特效
        };

        _ = new GameDataEffectSearch(SweepDamageSearch)
        {
            Name = "横扫范围搜索",
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            TargetLocation = new() { Value = TargetLocation.Caster },
            Method = SearchMethod.Cone, // 扇形搜索方法
            Radius = static (_) => 300, // 搜索半径300
            CentralAngle = static (_) => 180, // 圆心角180度，实现弧形搜索
            MaxCount = null, // 不限制最大搜索数量
            
            // 🎯 配置扇形搜索的朝向 - 从施法者朝向技能目标方向
            Facing = new() 
            {
                Method = GameCore.Struct.EffectAngleMethod.AngleBetweenTwoPoints, // 两点间角度
                Location = new() { Value = TargetLocation.Caster }, // 起始点：施法者
                OtherLocation = new() { Value = TargetLocation.MainTarget }, // 终点：技能主目标位置
            },
            
            Effect = SweepDamage,
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Sweep)
        {
            Name = "横扫",
            DisplayName = "横扫",
            Description = "对前方180度弧形区域横扫，对范围内敌人造成物理伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.24), // 根据entry_data.ini
                Channel = static (_) => TimeSpan.FromSeconds(0.09), // 根据entry_data.ini
                Backswing = static (_) => TimeSpan.FromSeconds(0.02), // 剩余时间，使总时长约0.35秒
            },
            
            Cost = new()
            {
                Cooldown = SweepCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = SweepDamageSearch,
            TargetType = AbilityTargetType.Vector,
            Range = static (_) => 300,
            
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
                TargetingIndicator = LineIndicator,
                CursorRadius = static (_) => 64,
                VectorLineWidth = static (_) => 64,
                VectorHighlightLimit = static (_) => 1
            },
            
            Animation = [SweepAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Sweep Ability initialized successfully!");
    }
}
