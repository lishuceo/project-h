using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.BuffSystem.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.Components.Data;
using GameCore.Execution.Data;
using GameCore.ModelAnimation.Data;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameData;
using static GameCore.ScopeData;
using Microsoft.Extensions.Logging;
using GameCore.Struct;

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Gunner;

/// <summary>
/// 枪手技能：翻滚
/// 辅助位移技能，带无敌效果
/// </summary>
public class RollAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Roll = new("Roll"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectUnitMoverApply> RollMoverApply = new("RollMoverApply"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> RollInvulnerabilityApply = new("RollInvulnerabilityApply"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> RollEffectSet = new("RollEffectSet"u8);
    #endregion

    #region Mover定义
    public static readonly GameLink<GameDataMover, GameDataMoverTarget> RollMover = new("RollMover"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> RollInvulnerabilityBuff = new("RollInvulnerabilityBuff"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> RollCooldown = new("RollCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> RollAnim = new("RollAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> RollTrailParticle = new("RollTrailParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> RollTrailEffect = new("RollTrailEffect"u8);
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

        Game.Logger.LogInformation("🤸‍♂️ Initializing Roll Ability for Gunner...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(RollAnim)
        {
            Name = "翻滚动画",
            File = "anim/cartoonbody/plhx_nanzhu/shuqin/roll.ani"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(RollTrailParticle)
        {
            Asset = "effect/effect_new1/effect_guanghuan/eff_boss_guanghuan/particle.effect"u8,
            AssetLayerScale = 0.4f,
            Radius = 48f,
        };

        _ = new GameDataActorParticle(RollTrailEffect)
        {
            AutoPlay = true,
            Particle = RollTrailParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(RollCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(8.0), // 8秒冷却
        };

        // ========== Buff配置 ==========
        _ = new GameDataBuff(RollInvulnerabilityBuff)
        {
            Name = "翻滚无敌",
            Duration = static (_) => TimeSpan.FromSeconds(0.6), // 0.6秒无敌时间
        };

        // ========== Mover配置 ==========
        _ = new GameDataMoverTarget(RollMover)
        {
            Name = "翻滚移动器",
            Speed = static (_) => 800, // 翻滚速度
            FinishArriveRadius = static (_) => 32,
            AllowSpellModification = true,
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectUnitMoverApply(RollMoverApply)
        {
            Name = "翻滚位移",
            Mover = RollMover,
            LaunchEntity = new TargetLocationExpression { Value = TargetLocation.Caster },
            MoverTarget = new TargetLocationExpression { Value = TargetLocation.MainTarget },
            TargetLocation = new TargetLocationExpression { Value = TargetLocation.Caster },
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectBuffAdd(RollInvulnerabilityApply)
        {
            Name = "翻滚无敌效果",
            BuffLink = RollInvulnerabilityBuff,
            TargetLocation = new TargetLocationExpression { Value = TargetLocation.Caster },
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectSet(RollEffectSet)
        {
            Name = "翻滚效果组合",
            Effects = [
                new() { Link = RollInvulnerabilityApply }, // 先给无敌状态
                new() { Link = RollMoverApply } // 再执行移动
            ],
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Roll)
        {
            Name = "翻滚",
            DisplayName = "翻滚",
            Description = "向指定方向翻滚，翻滚期间获得短暂无敌效果",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.1),
                Channel = static (_) => TimeSpan.FromSeconds(0.6),
                Backswing = static (_) => TimeSpan.FromSeconds(0.1),
            },
            
            Cost = new()
            {
                Cooldown = RollCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { AlwaysAcquireTarget = true },
            Effect = RollEffectSet,
            TargetType = AbilityTargetType.Vector,
            Range = static (_) => 300, // 翻滚距离
            
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = ARPGScopeData.TargetingIndicator.LineIndicator,
                CursorRadius = static (_) => 64,
                VectorLineWidth = static (_) => 48,
                VectorHighlightLimit = static (_) => 1
            },
            
            Animation = [RollAnim],
            ActorArray = [RollTrailEffect], // 翻滚拖尾特效
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Roll Ability initialized successfully!");
    }
}
