using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.EntitySystem;
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

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Slime;

/// <summary>
/// 史莱姆技能：主动近战攻击
/// 主动攻击近距离的敌人，造成伤害
/// </summary>
public class SlimeSearchAttack : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> SearchAttack = new("SlimeSearchAttack"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> SearchAttackDamage = new("SlimeSearchAttackDamage"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> SearchAttackCooldown = new("SlimeSearchAttackCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> SearchAttackAnim = new("SlimeSearchAttackAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> SlimeAttackEffectParticle = new("SlimeAttackEffectParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> SlimeHitEffectParticle = new("SlimeHitEffectParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> SlimeAttackEffect = new("SlimeAttackEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> SlimeHitEffect = new("SlimeHitEffect"u8);
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

        Game.Logger.LogInformation("🟢 Initializing Slime Active Attack Ability...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(SearchAttackAnim)
        {
            Name = "史莱姆攻击动画",
            File = "attack"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(SlimeAttackEffectParticle)
        {
            Name = "史莱姆攻击特效",
            Asset = "effect/effect_new/effect_slime/eff_slime_attack/particle.effect"u8, // 史莱姆攻击特效
        };

        _ = new GameDataParticle(SlimeHitEffectParticle)
        {
            Name = "史莱姆击中特效",
            Asset = "effect/effect_new/effect_hit/eff_hit_acid/particle.effect"u8, // 酸性击中特效
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(SlimeAttackEffect)
        {
            Name = "史莱姆攻击特效Actor",
            AutoPlay = true,
            Particle = SlimeAttackEffectParticle,
            KillOnFinish = true, // 播放完成后自动结束
            ForceOneShot = true, // 单次播放
        };

        _ = new GameDataActorParticle(SlimeHitEffect)
        {
            Name = "史莱姆击中特效Actor", 
            AutoPlay = true,
            Particle = SlimeHitEffectParticle,
            KillOnFinish = true, // 播放完成后自动结束
            ForceOneShot = true, // 单次播放
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(SearchAttackCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(2.0), // 2秒冷却
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectDamage(SearchAttackDamage)
        {
            Name = "史莱姆近战攻击伤害",
            Amount = static (_) => 40, // 基础伤害40
            Type = DamageType.Physical,
            LogExecutionFailure = true,
            ActorArray = [SlimeHitEffect], // 添加击中特效
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(SearchAttack)
        {
            Name = "粘液攻击",
            DisplayName = "粘液攻击",
            Description = "史莱姆用粘液攻击单个敌人，造成物理伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.3),
                Channel = static (_) => TimeSpan.FromSeconds(0.1),
                Backswing = static (_) => TimeSpan.FromSeconds(0.4),
            },
            
            Cost = new()
            {
                Cooldown = SearchAttackCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = SearchAttackDamage,
            TargetType = AbilityTargetType.Unit, // 单位目标
            Range = static (_) => 120, // 攻击范围120
            
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
            Animation = [SearchAttackAnim],
            ActorArray = [SlimeAttackEffect], // 添加攻击特效
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Slime Active Attack Ability initialized successfully!");
    }
}