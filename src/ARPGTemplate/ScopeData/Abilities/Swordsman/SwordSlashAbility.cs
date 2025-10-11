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

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Swordsman;

/// <summary>
/// 剑客技能：挥剑
/// 近战范围物理伤害技能
/// </summary>
public class SwordSlashAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> SwordSlash = new("SwordSlash"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> SlashDamageSearch = new("SlashDamageSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> SlashDamage = new("SlashDamage"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> SwordSlashCooldown = new("SwordSlashCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSequence> SwordSlashAnim = new("SwordSlashAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> HitEffectParticle = new("HitEffectParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> SlashEffectParticle = new("SlashEffectParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> HitEffect = new("HitEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> SlashEffect = new("SlashEffect"u8);
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

        Game.Logger.LogInformation("⚔️ Initializing Sword Slash Ability for Swordsman...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSequence(SwordSlashAnim)
        {
            Name = "挥剑动画序列",
            Playbacks = [new() 
            {
                AnimationRaw = "attack_01"u8,
                IsLooping = false,
                PlaybackDuration = TimeSpan.FromSeconds(1.0), // 动画播放时长
            }],
            SequenceActors = [new() 
            {
                Actor = SlashEffect, // 在动画播放时生成刀光特效
                SpawnOffset = TimeSpan.FromSeconds(0.2), // 动画开始0.2秒后显示刀光
                Duration = TimeSpan.FromSeconds(0.5), // 刀光持续0.5秒
            }]
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(HitEffectParticle)
        {
            Asset = "effect/effect_new/effect_hit/eff_hit_29/particle.effect"u8,
        };

        _ = new GameDataParticle(SlashEffectParticle)
        {
            Asset = "effect/effect_new/effect_knief/eff_knief_022/particle.effect"u8,
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(HitEffect)
        {
            AutoPlay = true,
            Particle = HitEffectParticle,
            KillOnFinish = true, // 播放完成后自动结束
            ForceOneShot = true, // 单次播放
        };

        _ = new GameDataActorParticle(SlashEffect)
        {
            AutoPlay = true,
            Particle = SlashEffectParticle,
            KillOnFinish = true, // 播放完成后自动结束
            ForceOneShot = true, // 单次播放
            Offset = new System.Numerics.Vector3(0, 0, 80), // 抬高80单位
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(SwordSlashCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(0.5), // 0.5秒冷却
        };

        // ========== 效果配置 ==========
        // 挥剑伤害效果
        _ = new GameDataEffectDamage(SlashDamage)
        {
            Name = "挥剑伤害",
            Amount = static (_) => 120, // 基础伤害120
            Type = DamageType.Physical,
            LogExecutionFailure = true,
            ActorArray = [HitEffect], // 添加受击特效
        };

        _ = new GameDataEffectSearch(SlashDamageSearch)
        {
            Name = "挥剑范围搜索",
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            TargetLocation = new() { Value = TargetLocation.Caster },
            Radius = static (_) => 250, // 搜索半径250
            Effect = SlashDamage,
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(SwordSlash)
        {
            Name = "挥剑",
            DisplayName = "挥剑",
            Description = "向前方扇形区域挥砍，对范围内敌人造成物理伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.3),
                Channel = static (_) => TimeSpan.FromSeconds(0.2),
                Backswing = static (_) => TimeSpan.FromSeconds(0.5),
            },
            
            Cost = new()
            {
                Cooldown = SwordSlashCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { AlwaysAcquireTarget = true },
            Effect = SlashDamageSearch,
            TargetType = AbilityTargetType.Vector,
            Range = static (_) => 200,
            
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
                CursorRadius = static (_) => 64,
                VectorLineWidth = static (_) => 64,
                VectorHighlightLimit = static (_) => 1
            },
            
            Animation = [SwordSlashAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Sword Slash Ability initialized successfully!");
    }
}
