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
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.BuffSystem.Data;
using GameCore.Struct;
using GameData;
using static GameCore.ScopeData;
using GameCore.Components;
using GameCore.Components.Data;

namespace GameEntry.JsonScopeDataTest.Abilities.HealAbilities;

/// <summary>
/// 治疗技能：回春
/// 群体目标治疗技能，为范围内友军添加持续治疗buff
/// </summary>
public class RejuvenationAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Rejuvenation = new("Rejuvenation"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> RejuvenationSearch = new("RejuvenationSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> RejuvenationAddBuff = new("RejuvenationAddBuff"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectUnitModifyVital> RejuvenationHeal = new("RejuvenationHeal"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> RejuvenationBuff = new("RejuvenationBuff"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> RejuvenationCooldown = new("RejuvenationCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSequence> RejuvenationAnim = new("RejuvenationAnim"u8);
    #endregion

    #region 粒子和Actor定义
    // 目标身上的回春特效 (类似ini中的Particle_1)
    public static readonly GameLink<GameDataParticle, GameDataParticle> RejuvenationTargetParticle = new("RejuvenationTargetParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> RejuvenationTargetEffect = new("RejuvenationTargetEffect"u8);
    
    // 施法者右手特效 (类似ini中的ActorEffect_2)
    public static readonly GameLink<GameDataParticle, GameDataParticle> RejuvenationCasterParticle = new("RejuvenationCasterParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> RejuvenationCasterRightHand = new("RejuvenationCasterRightHand"u8);
    
    // 施法者左手特效 (类似ini中的ActorEffect_4)
    public static readonly GameLink<GameDataActor, GameDataActorParticle> RejuvenationCasterLeftHand = new("RejuvenationCasterLeftHand"u8);
    
    // buff持续特效中使用的治疗粒子
    public static readonly GameLink<GameDataActor, GameDataActorParticle> RejuvenationBuffEffect = new("RejuvenationBuffEffect"u8);
    #endregion

    #region 瞄准指示器定义
    public static readonly GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator> AreaIndicator = new("p_0tja.ScopeData.GameDataTargetingIndicator.AreaIndicator.Root"u8);
    #endregion

    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        Game.Logger.LogInformation("🌿 Initializing Rejuvenation Ability for Test...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSequence(RejuvenationAnim)
        {
            Name = "回春动画序列",
            Playbacks = [new() 
            {
                AnimationRaw = "skill_05"u8, // 使用ini中相同的动画
                IsLooping = false,
                PlaybackDuration = TimeSpan.FromSeconds(1.0), // 动画播放时长
            }],
            SequenceActors = [
                new() 
                {
                    Actor = RejuvenationCasterRightHand, // 右手特效
                    SpawnOffset = TimeSpan.FromSeconds(0.1), // 施法开始0.1秒后显示
                    Duration = TimeSpan.FromSeconds(0.8), // 持续0.8秒
                },
                new() 
                {
                    Actor = RejuvenationCasterLeftHand, // 左手特效
                    SpawnOffset = TimeSpan.FromSeconds(0.1), // 施法开始0.1秒后显示  
                    Duration = TimeSpan.FromSeconds(0.8), // 持续0.8秒
                }
            ]
        };

        // ========== 粒子效果配置 ==========
        // 目标身上的回春特效
        _ = new GameDataParticle(RejuvenationTargetParticle)
        {
            Asset = "effect/samplespells/rejuvenation/rejuvenation/particle.effect"u8, // 使用ini中的特效
            Radius = 128f,
        };

        // 施法者手部特效粒子
        _ = new GameDataParticle(RejuvenationCasterParticle)
        {
            Asset = "effect/samplespells/rejuvenation/rejuvenation_caster/particle.effect"u8, // 使用ini中的施法者特效
            Radius = 64f,
        };

        // ========== Actor配置 ==========
        // 目标身上的回春特效Actor (施法时一次性显示)
        _ = new GameDataActorParticle(RejuvenationTargetEffect)
        {
            AutoPlay = true,
            Particle = RejuvenationTargetParticle,
            KillOnFinish = true,
            ForceOneShot = true, // 一次性特效
        };

        // 施法者右手特效Actor (类似ini中绑定到socket_hand_r)
        _ = new GameDataActorParticle(RejuvenationCasterRightHand)
        {
            AutoPlay = true,
            Particle = RejuvenationCasterParticle,
            KillOnFinish = false,
            ForceOneShot = false,
            Socket = "socket_hand_r"u8, // 绑定到右手
        };

        // 施法者左手特效Actor (类似ini中绑定到socket_hand_l)
        _ = new GameDataActorParticle(RejuvenationCasterLeftHand)
        {
            AutoPlay = true,
            Particle = RejuvenationCasterParticle,
            KillOnFinish = false,
            ForceOneShot = false,
            Socket = "socket_hand_l"u8, // 绑定到左手
        };

        // buff持续治疗特效Actor (在buff期间持续显示)
        _ = new GameDataActorParticle(RejuvenationBuffEffect)
        {
            AutoPlay = true,
            Particle = RejuvenationTargetParticle, // 重用目标特效
            KillOnFinish = false,
            ForceOneShot = false, // 跟随buff持续时间
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(RejuvenationCooldown)
        {
            Time = static (context) => {
                // 基础冷却时间12秒
                double baseCooldown = 12.0;
                return TimeSpan.FromSeconds(baseCooldown);
            },
        };

        // ========== 治疗效果配置 ==========
        // 周期性治疗效果
        _ = new GameDataEffectUnitModifyVital(RejuvenationHeal)
        {
            Name = "回春治疗效果",
            Modification = [
                new() {
                    Property = PropertyVital.Health, // 修改生命值属性
                    Value = static (_) => 80, // 每次恢复80点生命值
                }
            ],
            Operation = PropertyModificationOperation.Add, // 加法操作
            ActorArray = [RejuvenationBuffEffect], // 治疗时显示特效
            LogExecutionFailure = true,
        };

        // ========== Buff配置 ==========
        // 回春Buff - 持续治疗效果
        _ = new GameDataBuff(RejuvenationBuff)
        {
            Name = "回春",
            DisplayName = "回春",
            Description = "持续恢复生命值，每3秒治疗80点生命",
            Duration = static (_) => TimeSpan.FromSeconds(9), // 持续9秒，和ini中一样
            SyncType = SyncType.Sight, // 设置同步类型
            
            // 周期性效果
            Period = static (_) => TimeSpan.FromSeconds(3), // 每3秒触发一次，和ini中一样
            PeriodicEffect = RejuvenationHeal, // 周期性触发治疗
            InitialEffect = RejuvenationHeal, // 初始也触发一次治疗
            
            // Buff身上的持续特效
            ActorArray = [RejuvenationBuffEffect],
        };

        // ========== 效果配置 ==========
        // 添加回春Buff效果
        _ = new GameDataEffectBuffAdd(RejuvenationAddBuff)
        {
            Name = "回春添加Buff",
            BuffLink = RejuvenationBuff,
            LogExecutionFailure = true,
        };

        // 范围搜索效果 (类似ini中的Search_1)
        _ = new GameDataEffectSearch(RejuvenationSearch)
        {
            Name = "回春范围搜索",
            SearchFilters = [new() {
                Required = [UnitRelationship.Alliance], // 只搜索友军
                Excluded = [UnitState.Dead] // 排除死亡单位
            },
            new() {
                Required = [UnitRelationship.Self], // 只能对自己使用
                Excluded = [UnitState.Dead] // 排除死亡单位
            }],
            Radius = static (_) => 200, // 搜索半径200，和ini中一样
            ActorArray = [RejuvenationTargetEffect], // 搜索时的目标特效
            Effect = RejuvenationAddBuff, // 对搜索到的友军添加回春Buff
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Rejuvenation)
        {
            Name = "回春",
            DisplayName = "回春",
            Description = "为范围内友军施加回春效果，持续9秒，每3秒恢复80点生命值",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.35), // 类似ini中的cast_start_time
                Channel = static (_) => TimeSpan.FromSeconds(0.14),  // 类似ini中的cast_shot_time
                Backswing = static (_) => TimeSpan.FromSeconds(0.37), // 类似ini中的cast_finish_time
            },
            
            Cost = new()
            {
                Cooldown = RejuvenationCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = RejuvenationSearch, // 主效果是范围搜索
            TargetType = AbilityTargetType.Ground, // 需要目标单位作为搜索中心
            Range = static (_) => 800, // 施法距离800，和ini中一样
            
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                { 
                    Required = [UnitRelationship.Alliance], // 只能对友军使用
                    Excluded = [UnitState.Dead] // 排除死亡单位
                },
                new()
                {
                    Required = [UnitRelationship.Self], // 只能对自己使用
                    Excluded = [UnitState.Dead] // 排除死亡单位
                }
                ],
            },
            
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = AreaIndicator, // 群体技能用区域指示器
                CursorRadius = static (_) => 200, // 显示影响范围
            },
            
            // 施法动画（包含手部特效）
            Animation = [RejuvenationAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Rejuvenation Ability initialized successfully!");
    }
}
