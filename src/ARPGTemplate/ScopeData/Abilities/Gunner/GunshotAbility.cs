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
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.Struct;
using GameData;
using static GameCore.ScopeData;

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Gunner;

/// <summary>
/// 枪手技能：射击
/// 远程单体物理伤害技能
/// </summary>
public class GunshotAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Gunshot = new("Gunshot"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> GunshotDamage = new("GunshotDamage"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> GunshotLaunchMissile = new("GunshotLaunchMissile"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> GunshotCompleteEffect = new("GunshotCompleteEffect"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> GunshotCooldown = new("GunshotCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> GunshotAnim = new("GunshotAnim"u8);
    #endregion

    #region 单位定义
    public static readonly GameLink<GameDataUnit, GameDataUnit> BulletMissile = new("BulletMissile"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> MuzzleFlashParticle = new("MuzzleFlashParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> BulletHitParticle = new("BulletHitParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> MuzzleFlashEffect = new("MuzzleFlashEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> BulletHitEffect = new("BulletHitEffect"u8);
    #endregion

    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.ARPGMode)
        {
            return;
        }

        Game.Logger.LogInformation("🔫 Initializing Gunshot Ability for Gunner...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(GunshotAnim)
        {
            Name = "射击动画",
            File = "attack_01"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(MuzzleFlashParticle)
        {
            Asset = "effect/effect_new/effect_hit/eff_hit_29/particle.effect"u8,
        };

        _ = new GameDataParticle(BulletHitParticle)
        {
            Asset = "effect/effect_new/effect_hit/eff_hit_12/particle.effect"u8,
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(MuzzleFlashEffect)
        {
            AutoPlay = true,
            Particle = MuzzleFlashParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        _ = new GameDataActorParticle(BulletHitEffect)
        {
            AutoPlay = true,
            Particle = BulletHitParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 子弹投掷物单位配置 ==========
        _ = new GameDataUnit(BulletMissile)
        {
            Name = "子弹",
            Filter = [UnitFilter.Missile],
            State = [UnitState.Invulnerable],
            CollisionRadius = 8,
            AttackableRadius = 16,
            Particle = "effect/effect_new/effect_emit/eff_yumaoemit_01/particle.effect"u8, // 使用指定的弹道特效
            UpdateFlags = new()
            {
                AllowMover = true,
            },
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(GunshotCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(0.5), // 0.5秒冷却
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectDamage(GunshotDamage)
        {
            Name = "射击伤害",
            Amount = static (_) => 85, // 基础伤害85
            Type = DamageType.Physical,
            LogExecutionFailure = true,
            ActorArray = [BulletHitEffect], // 添加子弹命中特效
        };

        // 子弹完整撞击效果 - 包含伤害
        _ = new GameDataEffectSet(GunshotCompleteEffect)
        {
            Name = "子弹完整撞击效果",
            Effects = [
                new() { Link = GunshotDamage }, // 造成伤害
            ],
        };

        // 🚀 子弹发射效果 - 参考蜘蛛系统的LaunchMissile
        _ = new GameDataEffectLaunchMissile(GunshotLaunchMissile)
        {
            Name = "子弹发射",
            Method = EffectLaunchMissileMethod.CreateMissile,
            Missile = BulletMissile,
            LaunchHeight = static (_) => 100, // 发射高度
            TargetHeight = static (_) => 50,  // 目标高度  
            Speed = static (_) => 800f, // 子弹速度（比蜘蛛更快）
            CompleteEffect = GunshotCompleteEffect, // 撞击时执行的完整效果
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Gunshot)
        {
            Name = "射击",
            DisplayName = "射击",
            Description = "向目标射出子弹，造成物理伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.1),
                Channel = static (_) => TimeSpan.FromSeconds(0.1),
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            
            Cost = new()
            {
                Cooldown = GunshotCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true,},
            AbilityExecuteFlags = new() { },
            Effect = GunshotLaunchMissile, // 改为发射投掷物效果
            TargetType = AbilityTargetType.Unit,
            Range = static (_) => 600, // 远程攻击范围600
            
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
                CursorRadius = static (_) => 32,
                VectorLineWidth = static (_) => 16,
                VectorHighlightLimit = static (_) => 1
            },
            
            Animation = [GunshotAnim],
            ActorArray = [MuzzleFlashEffect], // 添加枪口火光特效
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Gunshot Ability initialized successfully!");
    }
}
