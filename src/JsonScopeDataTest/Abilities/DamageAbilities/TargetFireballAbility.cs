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

namespace GameEntry.JsonScopeDataTest.Abilities.DamageAbilities;

/// <summary>
/// 伤害技能：目标火球术
/// 远程单体魔法伤害技能
/// </summary>
public class TargetFireballAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> TargetFireball = new("TargetFireball"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> TargetFireballDamage = new("TargetFireballDamage"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> TargetFireballLaunchMissile = new("TargetFireballLaunchMissile"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> TargetFireballCompleteEffect = new("TargetFireballCompleteEffect"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> TargetFireballCooldown = new("TargetFireballCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> TargetFireballAnim = new("TargetFireballAnim"u8);
    #endregion

    #region 单位定义
    public static readonly GameLink<GameDataUnit, GameDataUnit> TargetFireballProjectileMissile = new("TargetFireballProjectileMissile"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> TargetFireballExplosionParticle = new("TargetFireballExplosionParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> TargetFireballExplosionEffect = new("TargetFireballExplosionEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorAction> TargetFireballLaunchAction = new("TargetFireballLaunchAction"u8);
    #endregion

    #region 瞄准指示器定义
    public static readonly GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator> LineIndicator = new("p_0tja.ScopeData.GameDataTargetingIndicator.LineIndicator.Root"u8);
    #endregion

    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        // 测试模式下也可以初始化目标火球术技能
        Game.Logger.LogInformation("🎯 Initializing Target Fireball Ability for Test...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(TargetFireballAnim)
        {
            Name = "目标火球术动画",
            File = "anim/human/barehanded_anim/hand_05/skill_025.ani"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(TargetFireballExplosionParticle)
        {
            Asset = "effect/eff_tongyong/huoqiu_blast/particle.effect"u8,
            Radius = 96f,
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(TargetFireballExplosionEffect)
        {
            AutoPlay = true,
            Particle = TargetFireballExplosionParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== ActorAction配置 - 从右手发出弹道 ==========
        _ = new GameDataActorAction(TargetFireballLaunchAction)
        {
            LaunchSocket = "socket_hand_r"u8, // 从右手绑点发射
            ImpactSocket = "socket_hit"u8,  // 撞击位置
        };

        // ========== 投掷物单位配置 ==========
        _ = new GameDataUnit(TargetFireballProjectileMissile)
        {
            Name = "目标火球投掷物",
            Filter = [UnitFilter.Missile],
            State = [UnitState.Invulnerable],
            CollisionRadius = 16,
            AttackableRadius = 32,
            Particle = "effect/eff_tongyong/huoqiu2/particle.effect"u8, // 使用火球弹道特效
            UpdateFlags = new()
            {
                AllowMover = true,
            },
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(TargetFireballCooldown)
        {
            Time = static (context) => {
                // 基础冷却时间2.0秒
                double baseCooldown = 2.0;
                
                // 简化版本，不需要复杂的冷却缩减逻辑
                return TimeSpan.FromSeconds(baseCooldown);
            },
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectDamage(TargetFireballDamage)
        {
            Name = "目标火球术伤害",
            Amount = static (_) => 30, // 基础伤害30
            Type = DamageType.Magical, // 魔法伤害
            LogExecutionFailure = true,
            ActorArray = [TargetFireballExplosionEffect], // 添加爆炸特效
        };

        // 目标火球撞击完整效果 - 包含伤害
        _ = new GameDataEffectSet(TargetFireballCompleteEffect)
        {
            Name = "目标火球撞击完整效果",
            Effects = [
                new() { Link = TargetFireballDamage }, // 造成伤害
            ],
        };

        // 🚀 目标火球弹道发射效果
        _ = new GameDataEffectLaunchMissile(TargetFireballLaunchMissile)
        {
            Name = "目标火球弹道发射",
            Method = EffectLaunchMissileMethod.CreateMissile,
            Missile = TargetFireballProjectileMissile,
            LaunchHeight = static (_) => 150, // 发射高度
            TargetHeight = static (_) => 50, // 目标高度
            Speed = static (_) => 500f, // 火球速度
            ActorArray = [TargetFireballLaunchAction], // 添加弹道发射Actor作为后处理
            CompleteEffect = TargetFireballCompleteEffect, // 撞击时执行的完整效果
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(TargetFireball)
        {
            Name = "目标火球术",
            DisplayName = "目标火球术",
            Description = "发射目标火球攻击单个敌人，造成魔法伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.4),
                Channel = static (_) => TimeSpan.FromSeconds(0.3),
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            
            Cost = new()
            {
                Cooldown = TargetFireballCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = TargetFireballLaunchMissile,
            TargetType = AbilityTargetType.Unit,
            Range = static (_) => 1200, // 远程攻击范围1200
            
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
                CursorRadius = static (_) => 1200,
                VectorLineWidth = static (_) => 24,
                VectorHighlightLimit = static (_) => 1
            },
            
            Animation = [TargetFireballAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Target Fireball Ability initialized successfully!");
    }
}
