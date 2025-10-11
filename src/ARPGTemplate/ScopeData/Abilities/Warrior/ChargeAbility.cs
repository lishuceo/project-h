using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.Components.Data;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.ModelAnimation.Data;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameData;
using static GameCore.ScopeData;
using Microsoft.Extensions.Logging;
using GameCore.Struct;

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Warrior;

/// <summary>
/// 战士技能：突击
/// A-B直线位移真实伤害技能
/// </summary>
public class ChargeAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Charge = new("Charge"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectUnitMoverApply> ChargeMoverApply = new("ChargeMoverApply"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> ChargeDamageSearch = new("ChargeDamageSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> ChargeDamage = new("ChargeDamage"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> ChargeEffectSet = new("ChargeEffectSet"u8);
    #endregion

    #region Mover定义
    public static readonly GameLink<GameDataMover, GameDataMoverTarget> ChargeMover = new("ChargeMover"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> ChargeCooldown = new("ChargeCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> ChargeAnim = new("ChargeAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> ChargeTrailParticle = new("ChargeTrailParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> ChargeHitParticle = new("ChargeHitParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ChargeTrailEffect = new("ChargeTrailEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ChargeHitEffect = new("ChargeHitEffect"u8);
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

        Game.Logger.LogInformation("⚡ Initializing Charge Ability for Warrior...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(ChargeAnim)
        {
            Name = "突击动画",
            File = "skill_01"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(ChargeTrailParticle)
        {
            Asset = "effect/effect_new1/effect_guanghuan/eff_boss_guanghuan/particle.effect"u8,
            AssetLayerScale = 0.6f,
            Radius = 64f,
        };

        _ = new GameDataParticle(ChargeHitParticle)
        {
            Asset = "effect/eff_tongyong/huoqiu_blast/particle.effect"u8,
            Radius = 80f,
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(ChargeTrailEffect)
        {
            AutoPlay = true,
            Particle = ChargeTrailParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        _ = new GameDataActorParticle(ChargeHitEffect)
        {
            AutoPlay = true,
            Particle = ChargeHitParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(ChargeCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(10.0), // 10秒冷却
        };

        // ========== Mover配置 ==========
        _ = new GameDataMoverTarget(ChargeMover)
        {
            Name = "突击移动器",
            Speed = static (_) => 1200, // 突击速度
            FinishArriveRadius = static (_) => 48,
            AllowSpellModification = true,
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectDamage(ChargeDamage)
        {
            Name = "突击伤害",
            Amount = static (_) => 160, // 基础伤害160
            Type = DamageType.Pure, // 真实伤害，无视护甲和魔抗
            LogExecutionFailure = true,
            ActorArray = [ChargeHitEffect], // 添加撞击特效
        };

        _ = new GameDataEffectSearch(ChargeDamageSearch)
        {
            Name = "突击直线搜索",
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Radius = static (_) => 450, // 搜索半径
            Effect = ChargeDamage,
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectUnitMoverApply(ChargeMoverApply)
        {
            Name = "突击位移",
            Mover = ChargeMover,
            LaunchEntity = new TargetLocationExpression { Value = TargetLocation.Caster },
            MoverTarget = new TargetLocationExpression { Value = TargetLocation.MainTarget },
            TargetLocation = new TargetLocationExpression { Value = TargetLocation.Caster },
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectSet(ChargeEffectSet)
        {
            Name = "突击效果组合",
            Effects = [
                new() { Link = ChargeMoverApply }, // 先执行移动
                new() { Link = ChargeDamageSearch } // 然后对路径上的敌人造成伤害
            ],
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Charge)
        {
            Name = "突击",
            DisplayName = "突击",
            Description = "向指定方向冲刺，对路径上的敌人造成真实伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.2),
                Channel = static (_) => TimeSpan.FromSeconds(0.8),
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            
            Cost = new()
            {
                Cooldown = ChargeCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { AlwaysAcquireTarget = true },
            Effect = ChargeEffectSet,
            TargetType = AbilityTargetType.Vector,
            Range = static (_) => 450, // 突击距离
            
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
                VectorLineWidth = static (_) => 80, // 显示冲刺路径宽度
                VectorHighlightLimit = static (_) => 4 // 可以命中多个目标
            },
            
            Animation = [ChargeAnim],
            ActorArray = [ChargeTrailEffect], // 添加冲刺拖尾特效
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Charge Ability initialized successfully!");
    }
}
