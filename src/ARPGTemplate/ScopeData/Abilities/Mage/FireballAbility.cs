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
using GameEntry.ARPGTemplate.ScopeData;
using GameCore.Components;
using GameCore.Components.Data;

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Mage;

/// <summary>
/// 法师技能：火球术
/// 远程单体魔法伤害技能
/// </summary>
public class FireballAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Fireball = new("Fireball"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> FireballDamage = new("FireballDamage"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> FireballLaunchMissile = new("FireballLaunchMissile"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> FireballCompleteEffect = new("FireballCompleteEffect"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> FireballCooldown = new("FireballCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> FireballAnim = new("FireballAnim"u8);
    #endregion

    #region 单位定义
    public static readonly GameLink<GameDataUnit, GameDataUnit> FireballProjectileMissile = new("FireballProjectileMissile"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> FireballExplosionParticle = new("FireballExplosionParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> FireballExplosionEffect = new("FireballExplosionEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorAction> FireballLaunchAction = new("FireballLaunchAction"u8);
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

        Game.Logger.LogInformation("🔥 Initializing Fireball Ability for Mage...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(FireballAnim)
        {
            Name = "火球术动画",
            File = "attack_01"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(FireballExplosionParticle)
        {
            Asset = "effect/eff_tongyong/huoqiu_blast/particle.effect"u8,
            Radius = 96f,
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(FireballExplosionEffect)
        {
            AutoPlay = true,
            Particle = FireballExplosionParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== ActorAction配置 - 从右手发出弹道 ==========
        _ = new GameDataActorAction(FireballLaunchAction)
        {
            LaunchSocket = "socket_hand_r"u8, // 从右手绑点发射
            ImpactSocket = "socket_hit"u8,  // 撞击位置
        };

        // ========== 投掷物单位配置 ==========
        _ = new GameDataUnit(FireballProjectileMissile)
        {
            Name = "火球投掷物",
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
        _ = new GameDataCooldownActive(FireballCooldown)
        {
            Time = static (context) => {
                // 基础冷却时间0.5秒
                double baseCooldown = 0.5;
                
                // 获取冷却缩减属性
                if (context?.Caster is GameCore.EntitySystem.Unit caster)
                {
                    var cooldownReduction = caster.GetUnitPropertyFinal(ARPGUnitPropertyLink.CooldownReduction);
                    if (cooldownReduction.HasValue)
                    {
                        // 应用冷却缩减：最终冷却时间 = 基础冷却时间 * (1 - 缩减率)
                        baseCooldown *= (1.0 - cooldownReduction.Value);
                    }
                    
                }
                
                return TimeSpan.FromSeconds(baseCooldown);
            },
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectDamage(FireballDamage)
        {
            Name = "火球术伤害",
            Amount = static (_) => 110, // 基础伤害110
            Type = DamageType.Magical, // 魔法伤害
            LogExecutionFailure = true,
            ActorArray = [FireballExplosionEffect], // 添加爆炸特效
        };

        // 火球撞击完整效果 - 包含伤害
        _ = new GameDataEffectSet(FireballCompleteEffect)
        {
            Name = "火球撞击完整效果",
            Effects = [
                new() { Link = FireballDamage }, // 造成伤害
            ],
        };

        // 🚀 火球弹道发射效果 - 参考蜘蛛系统的LaunchMissile
        _ = new GameDataEffectLaunchMissile(FireballLaunchMissile)
        {
            Name = "火球弹道发射",
            Method = EffectLaunchMissileMethod.CreateMissile,
            Missile = FireballProjectileMissile,
            LaunchHeight = static (_) => 150, // 发射高度
            TargetHeight = static (_) => 50, // 目标高度
            Speed = static (_) => 500f, // 火球速度
            ActorArray = [FireballLaunchAction], // 添加弹道发射Actor作为后处理
            CompleteEffect = FireballCompleteEffect, // 撞击时执行的完整效果
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Fireball)
        {
            Name = "火球术",
            DisplayName = "火球术",
            Description = "发射火球攻击单个敌人，造成魔法伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.4),
                Channel = static (_) => TimeSpan.FromSeconds(0.3),
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            
            Cost = new()
            {
                Cooldown = FireballCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = FireballLaunchMissile, // 改为发射弹道，而不是直接伤害
            TargetType = AbilityTargetType.Unit,
            Range = static (_) => 650, // 远程攻击范围650
            
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
                CursorRadius = static (_) => 48,
                VectorLineWidth = static (_) => 24,
                VectorHighlightLimit = static (_) => 1
            },
            
            Animation = [FireballAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Fireball Ability initialized successfully!");
    }
}
