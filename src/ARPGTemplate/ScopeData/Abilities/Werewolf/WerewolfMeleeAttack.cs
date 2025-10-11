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

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Werewolf;

/// <summary>
/// 狼人技能：近战攻击
/// 单体目标物理伤害技能
/// </summary>
public class WerewolfMeleeAttack : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> MeleeAttack = new("WerewolfMeleeAttack"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> MeleeAttackDamage = new("WerewolfMeleeAttackDamage"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> MeleeAttackCooldown = new("WerewolfMeleeAttackCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> MeleeAttackAnim = new("WerewolfMeleeAttackAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> ClawHitEffectParticle = new("WerewolfClawHitEffectParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> ClawAttackEffectParticle = new("WerewolfClawAttackEffectParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ClawHitEffect = new("WerewolfClawHitEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ClawAttackEffect = new("WerewolfClawAttackEffect"u8);
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

        Game.Logger.LogInformation("🐺 Initializing Werewolf Melee Attack Ability...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(MeleeAttackAnim)
        {
            Name = "狼人攻击动画",
            File = "attack"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(ClawHitEffectParticle)
        {
            Asset = "effect/effect_new/effect_hit/eff_hit_29/particle.effect"u8,
        };

        _ = new GameDataParticle(ClawAttackEffectParticle)
        {
            Asset = "effect/effect_new/effect_claw/eff_claw_001/particle.effect"u8,
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(ClawHitEffect)
        {
            AutoPlay = true,
            Particle = ClawHitEffectParticle,
            KillOnFinish = true, // 播放完成后自动结束
            ForceOneShot = true, // 单次播放
        };

        _ = new GameDataActorParticle(ClawAttackEffect)
        {
            AutoPlay = true,
            Particle = ClawAttackEffectParticle,
            KillOnFinish = true, // 播放完成后自动结束
            ForceOneShot = true, // 单次播放
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(MeleeAttackCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(1.5), // 1.5秒冷却
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectDamage(MeleeAttackDamage)
        {
            Name = "狼人爪击伤害",
            Amount = static (_) => 60, // 基础伤害60
            Type = DamageType.Physical,
            LogExecutionFailure = true,
            ActorArray = [ClawHitEffect], // 添加受击特效
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(MeleeAttack)
        {
            Name = "爪击",
            DisplayName = "爪击",
            Description = "狼人用锋利的爪子攻击单个敌人，造成物理伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.2),
                Channel = static (_) => TimeSpan.FromSeconds(0.1),
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            
            Cost = new()
            {
                Cooldown = MeleeAttackCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = MeleeAttackDamage,
            TargetType = AbilityTargetType.Unit, // 单位目标
            Range = static (_) => 150, // 攻击范围150
            
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
                TargetingIndicator = ARPGScopeData.TargetingIndicator.AreaIndicator,
                CursorRadius = static (_) => 96,
                VectorLineWidth = static (_) => 72,
                VectorHighlightLimit = static (_) => 1
            },
            Animation = [MeleeAttackAnim],
            ActorArray = [ClawAttackEffect], // 添加攻击特效
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Werewolf Melee Attack Ability initialized successfully!");
    }
}
