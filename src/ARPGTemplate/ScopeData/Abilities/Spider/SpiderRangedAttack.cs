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
using GameCore.Struct;
using static GameCore.ScopeData;

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Spider;

/// <summary>
/// 蜘蛛技能：主动远程攻击
/// 主动发射投掷物攻击远程敌人
/// </summary>
public class SpiderRangedAttack : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> RangedAttack = new("SpiderRangedAttack"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> RangedAttackDamage = new("SpiderRangedAttackDamage"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> RangedAttackLaunchMissile = new("SpiderRangedAttackLaunchMissile"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> RangedAttackCompleteEffect = new("SpiderRangedAttackCompleteEffect"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> RangedAttackCooldown = new("SpiderRangedAttackCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> RangedAttackAnim = new("SpiderRangedAttackAnim"u8);
    #endregion

    #region 单位定义
    public static readonly GameLink<GameDataUnit, GameDataUnit> SpiderProjectileMissile = new("SpiderProjectileMissile"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> SpiderAttackEffectParticle = new("SpiderAttackEffectParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> SpiderHitEffectParticle = new("SpiderHitEffectParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> SpiderAttackEffect = new("SpiderAttackEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> SpiderHitEffect = new("SpiderHitEffect"u8);
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

        Game.Logger.LogInformation("🕷️ Initializing Spider Active Ranged Attack Ability...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(RangedAttackAnim)
        {
            Name = "蜘蛛远程攻击动画",
            File = "attack"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(SpiderAttackEffectParticle)
        {
            Name = "蜘蛛攻击特效",
            Asset = "effect/effect_new/effect_spider/eff_spider_spit/particle.effect"u8, // 蜘蛛吐丝特效
        };

        _ = new GameDataParticle(SpiderHitEffectParticle)
        {
            Name = "蜘蛛击中特效",
            Asset = "effect/effect_new/effect_hit/eff_hit_poison/particle.effect"u8, // 毒性击中特效
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(SpiderAttackEffect)
        {
            Name = "蜘蛛攻击特效Actor",
            AutoPlay = true,
            Particle = SpiderAttackEffectParticle,
            KillOnFinish = true, // 播放完成后自动结束
            ForceOneShot = true, // 单次播放
        };

        _ = new GameDataActorParticle(SpiderHitEffect)
        {
            Name = "蜘蛛击中特效Actor",
            AutoPlay = true,
            Particle = SpiderHitEffectParticle,
            KillOnFinish = true, // 播放完成后自动结束
            ForceOneShot = true, // 单次播放
        };

        // ========== 投掷物单位配置 ==========
        _ = new GameDataUnit(SpiderProjectileMissile)
        {
            Name = "蜘蛛投掷物",
            Filter = [UnitFilter.Missile],
            State = [UnitState.Invulnerable],
            CollisionRadius = 12,
            AttackableRadius = 24,
            Particle = "effect/effect_new/effect_emit/eff_duqiemit_01/particle.effect"u8, // 使用用户指定的弹道特效
            UpdateFlags = new()
            {
                AllowMover = true,
            },
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(RangedAttackCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(3.0), // 3秒冷却
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectDamage(RangedAttackDamage)
        {
            Name = "蜘蛛远程攻击伤害",
            Amount = static (_) => 50, // 基础伤害50
            Type = DamageType.Physical,
            LogExecutionFailure = true,
            ActorArray = [SpiderHitEffect], // 添加击中特效
        };

        // 投掷物完整撞击效果 - 包含伤害
        _ = new GameDataEffectSet(RangedAttackCompleteEffect)
        {
            Name = "蜘蛛投掷物完整撞击效果",
            Effects = [
                new() { Link = RangedAttackDamage }, // 造成伤害
            ],
        };

        // 🚀 蜘蛛投掷物发射效果 - 参考塔防系统的LaunchMissile
        _ = new GameDataEffectLaunchMissile(RangedAttackLaunchMissile)
        {
            Name = "蜘蛛投掷物发射",
            Method = EffectLaunchMissileMethod.CreateMissile,
            Missile = SpiderProjectileMissile,
            LaunchHeight = static (_) => 200, // 发射高度
            TargetHeight = static (_) => 50, // 目标高度
            Speed = static (_) => 400f, // 投掷物速度
            CompleteEffect = RangedAttackCompleteEffect, // 撞击时执行的完整效果
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(RangedAttack)
        {
            Name = "毒液射击",
            DisplayName = "毒液射击",
            Description = "蜘蛛发射毒液投掷物攻击远程敌人，造成物理伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.4),
                Channel = static (_) => TimeSpan.FromSeconds(0.2),
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            
            Cost = new()
            {
                Cooldown = RangedAttackCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = RangedAttackLaunchMissile,
            TargetType = AbilityTargetType.Unit, // 单位目标
            Range = static (_) => 400, // 攻击范围400
            
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
                TargetingIndicator = ARPGScopeData.TargetingIndicator.LineIndicator,
                CursorRadius = static (_) => 96,
                VectorLineWidth = static (_) => 72,
                VectorHighlightLimit = static (_) => 1
            },
            Animation = [RangedAttackAnim],
            ActorArray = [SpiderAttackEffect], // 添加攻击特效
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Spider Active Ranged Attack Ability initialized successfully!");
    }
}