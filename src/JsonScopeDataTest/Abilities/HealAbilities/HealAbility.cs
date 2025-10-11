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
using GameCore.Struct;
using GameData;
using static GameCore.ScopeData;
using GameCore.Components;
using GameCore.Components.Data;

namespace GameEntry.JsonScopeDataTest.Abilities.HealAbilities;

/// <summary>
/// 治疗技能：治疗术
/// 单体目标治疗技能，瞬间恢复友军生命值
/// </summary>
public class HealAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Heal = new("Heal"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectUnitModifyVital> HealEffect = new("HealEffect"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> HealCooldown = new("HealCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> HealAnim = new("HealAnim"u8);
    #endregion

    #region 粒子和Actor定义
    // 治疗特效粒子 (目标身上的治疗特效)
    public static readonly GameLink<GameDataParticle, GameDataParticle> HealParticle = new("HealParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> HealActorEffect = new("HealActorEffect"u8);
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
        Game.Logger.LogInformation("💚 Initializing Heal Ability for Test...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(HealAnim)
        {
            Name = "治疗术动画",
            File = "anim/human/barehanded_anim/hand_05/skill_027.ani"u8, // 使用ini中相同的动画
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        // 治疗特效 (目标身上的治疗光效)
        _ = new GameDataParticle(HealParticle)
        {
            Asset = "effect/eff_autochess1/particle/buff/ps_huifu/particle.effect"u8, // 使用ini中的治疗特效
            Radius = 96f,
        };

        // ========== Actor配置 ==========
        // 治疗特效Actor
        _ = new GameDataActorParticle(HealActorEffect)
        {
            AutoPlay = true,
            Particle = HealParticle,
            KillOnFinish = true,
            ForceOneShot = true, // 一次性治疗特效
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(HealCooldown)
        {
            Time = static (context) => {
                // 基础冷却时间5秒
                double baseCooldown = 5.0;
                return TimeSpan.FromSeconds(baseCooldown);
            },
        };

        // ========== 效果配置 ==========
        // 治疗效果 - 直接修改生命值
        _ = new GameDataEffectUnitModifyVital(HealEffect)
        {
            Name = "治疗术效果",
            Modification = [
                new() {
                    Property = PropertyVital.Health, // 修改生命值属性
                    Value = static (_) => 200, // 恢复200点生命值，和ini中一样
                }
            ],
            Operation = PropertyModificationOperation.Add, // 加法操作
            ActorArray = [HealActorEffect], // 治疗特效
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Heal)
        {
            Name = "治疗术",
            DisplayName = "治疗术",
            Description = "治疗友军单位，瞬间恢复200点生命值",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.34), // 类似ini中的cast_start_time
                Channel = static (_) => TimeSpan.FromSeconds(0.21),  // 类似ini中的cast_shot_time
                Backswing = static (_) => TimeSpan.FromSeconds(0.95), // 类似ini中的cast_finish_time
            },
            
            Cost = new()
            {
                Cooldown = HealCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = HealEffect, // 主效果是治疗
            TargetType = AbilityTargetType.Unit, // 单体目标类型，和ini中target_type=1一致
            Range = static (_) => 600, // 施法距离
            
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
                TargetingIndicator = AreaIndicator, // 使用区域指示器
                CursorRadius = static (_) => 96, // 显示目标范围
            },
            
            Animation = [HealAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Heal Ability initialized successfully!");
    }
}
