using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.Components;
using GameCore.Components.Data;
using GameCore.Execution.Data;
using GameCore.ModelAnimation.Data;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameData;
using GameCore.TargetingSystem.Data;
using static GameCore.ScopeData;
using Microsoft.Extensions.Logging;
using GameCore.Struct;
using GameCore.ActorSystem.Data;

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Swordsman;

/// <summary>
/// 剑客技能：冲刺
/// 辅助位移技能
/// </summary>
public class DashAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Dash = new("Dash"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectUnitMoverApply> DashMoverApply = new("DashMoverApply"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectLog> DashLog = new("DashLog"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> DashEffectSet = new("DashEffectSet"u8);
    #endregion

    #region Mover定义
    public static readonly GameLink<GameDataMover, GameDataMoverTarget> DashMover = new("DashMover"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> DashCooldown = new("DashCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSequence> DashAnim = new("DashAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> DashEffectParticle = new("DashEffectParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> DashEffect = new("DashEffect"u8);
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

        Game.Logger.LogInformation("🏃‍♂️ Initializing Dash Ability for Swordsman...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSequence(DashAnim)
        {
            Name = "冲刺动画序列",
            Playbacks = [new() 
            {
                AnimationRaw = "anim/cartoonbody/plhx_nanzhu/kongshou/dash.ani"u8,
                IsLooping = false,
                PlaybackDuration = TimeSpan.FromSeconds(0.4), // 冲刺动画时长
            }],
            SequenceActors = [new() 
            {
                Actor = DashEffect, // 在冲刺时生成刺击特效
                SpawnOffset = TimeSpan.FromSeconds(0.05), // 动画开始0.05秒后显示刺击特效
                Duration = TimeSpan.FromSeconds(0.3), // 刺击特效持续时间
            }]
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(DashEffectParticle)
        {
            Asset = "effect/effect_new1/effect_knief/eff_knief01/particle.effect"u8,
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(DashEffect)
        {
            AutoPlay = true,
            Particle = DashEffectParticle,
            KillOnFinish = true, // 播放完成后自动结束
            ForceOneShot = true, // 单次播放
            Offset = new System.Numerics.Vector3(0, 0, 60), // 抬高60单位
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(DashCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(1.0), // 1秒冷却
        };

        // ========== Mover配置 ==========
        _ = new GameDataMoverTarget(DashMover)
        {
            Name = "冲刺移动器",
            Speed = static (_) => 1500, // 移动速度
            FinishArriveRadius = static (_) => 64, // 到达半径
            AllowSpellModification = true,
        };

        // ========== 效果配置 ==========
        // 冲刺Log效果 - 打印caster和target信息
        _ = new GameDataEffectLog(DashLog)
        {
            Name = "冲刺日志",
            Message = context => 
            {
                var targetInfo = context.Target?.ToString() ?? "null";
                var casterInfo = context.Caster?.ToString() ?? "null";
                var targetPos = context.Target?.Position.ToString() ?? "null";
                var casterPos = context.Caster?.Position.ToString() ?? "null";
                
                return $"🏃‍♂️ 冲刺技能执行 - " +
                       $"Caster: {casterInfo} at {casterPos}, " +
                       $"Target: {targetInfo} at {targetPos}";
            },
            Level = LogLevel.Information,
            LogExecutionFailure = true,
        };

        // 冲刺位移效果
        _ = new GameDataEffectUnitMoverApply(DashMoverApply)
        {
            Name = "冲刺位移",
            Mover = DashMover,
            LaunchEntity = new TargetLocationExpression { Value = TargetLocation.Caster },
            MoverTarget = new TargetLocationExpression { Value = TargetLocation.MainTarget },
            TargetLocation = new TargetLocationExpression { Value = TargetLocation.Caster },
            LogExecutionFailure = true,
        };

        // 冲刺效果集合 - 先打印日志，再执行移动
        _ = new GameDataEffectSet(DashEffectSet)
        {
            Name = "冲刺效果组合",
            Effects = [
                new() { Link = DashLog },
                new() { Link = DashMoverApply }
            ],
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Dash)
        {
            Name = "冲刺",
            DisplayName = "冲刺",
            Description = "快速向指定方向冲刺一段距离",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.1),
                Channel = static (_) => TimeSpan.FromSeconds(0.2),
                Backswing = static (_) => TimeSpan.FromSeconds(0.1),
            },
            
            Cost = new()
            {
                Cooldown = DashCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { AlwaysAcquireTarget = true },
            Effect = DashEffectSet,
            TargetType = AbilityTargetType.Vector,
            Range = static (_) => 400,
            
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = ARPGScopeData.TargetingIndicator.LineIndicator,
                CursorRadius = static (_) => 96,
                VectorLineWidth = static (_) => 64,
                VectorHighlightLimit = static (_) => 1
            },
            
            Animation = [DashAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Dash Ability initialized successfully!");
    }
}
