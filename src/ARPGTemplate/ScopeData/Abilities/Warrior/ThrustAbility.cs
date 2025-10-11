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
using GameData;
using static GameCore.ScopeData;

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Warrior;

/// <summary>
/// 战士技能：刺击
/// 矩形近战范围物理伤害技能
/// </summary>
public class ThrustAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Thrust = new("Thrust"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> ThrustDamageSearch = new("ThrustDamageSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> ThrustDamage = new("ThrustDamage"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> ThrustCooldown = new("ThrustCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> ThrustAnim = new("ThrustAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> ThrustEffectParticle = new("ThrustEffectParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> ThrustHitParticle = new("ThrustHitParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ThrustEffect = new("ThrustEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ThrustHitEffect = new("ThrustHitEffect"u8);
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

        Game.Logger.LogInformation("🗡️ Initializing Thrust Ability for Warrior...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(ThrustAnim)
        {
            Name = "刺击动画",
            File = "attack_01"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(ThrustEffectParticle)
        {
            Asset = "effect/effect_new1/effect_knief/eff_knief_002/particle.effect"u8,
            AssetLayerScale = 1.2f,
            Radius = 64f,
        };

        _ = new GameDataParticle(ThrustHitParticle)
        {
            Asset = "effect/effect_new/effect_hit/eff_hit_29/particle.effect"u8,
            Radius = 48f,
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(ThrustEffect)
        {
            AutoPlay = true,
            Particle = ThrustEffectParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        _ = new GameDataActorParticle(ThrustHitEffect)
        {
            AutoPlay = true,
            Particle = ThrustHitParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(ThrustCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(0.5), // 0.5秒冷却
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectDamage(ThrustDamage)
        {
            Name = "刺击伤害",
            Amount = static (_) => 140, // 基础伤害140
            Type = DamageType.Physical, // 物理伤害
            LogExecutionFailure = true,
            ActorArray = [ThrustHitEffect], // 添加受击特效
        };

        _ = new GameDataEffectSearch(ThrustDamageSearch)
        {
            Name = "刺击矩形搜索",
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Radius = static (_) => 300, // 搜索半径
            Effect = ThrustDamage,
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Thrust)
        {
            Name = "刺击",
            DisplayName = "刺击",
            Description = "向前方矩形区域刺击，对范围内敌人造成物理伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.4),
                Channel = static (_) => TimeSpan.FromSeconds(0.2),
                Backswing = static (_) => TimeSpan.FromSeconds(0.6),
            },
            
            Cost = new()
            {
                Cooldown = ThrustCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { AlwaysAcquireTarget = true },
            Effect = ThrustDamageSearch,
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
                TargetingIndicator = ARPGScopeData.TargetingIndicator.LineIndicator,
                CursorRadius = static (_) => 150, // 矩形指示器大小
                VectorLineWidth = static (_) => 120, // 矩形宽度指示
                VectorHighlightLimit = static (_) => 3 // 可以命中多个目标
            },
            
            Animation = [ThrustAnim],
            ActorArray = [ThrustEffect], // 添加刺击特效
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Thrust Ability initialized successfully!");
    }
}
