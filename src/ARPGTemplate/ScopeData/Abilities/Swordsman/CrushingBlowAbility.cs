using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.BuffSystem.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.ModelAnimation.Data;
using GameData;
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using static GameCore.ScopeData;
using GameCore.Components.Data;

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Swordsman;

/// <summary>
/// 剑客技能：痛击
/// 近战单体减益破防技能
/// </summary>
public class CrushingBlowAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> CrushingBlow = new("CrushingBlow"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> CrushingBlowSearch = new("CrushingBlowSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> CrushingBlowSet = new("CrushingBlowSet"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> CrushingBlowDamage = new("CrushingBlowDamage"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> AddArmorReductionBuff = new("AddArmorReductionBuff"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> ArmorReduction = new("ArmorReduction"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> CrushingBlowCooldown = new("CrushingBlowCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSequence> CrushingBlowAnim = new("CrushingBlowAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> HitEffectParticle = new("CrushingBlowHitEffectParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> SlashEffectParticle = new("CrushingBlowSlashEffectParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> ArmorReductionParticle = new("ArmorReductionParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> HitEffect = new("CrushingBlowHitEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> SlashEffect = new("CrushingBlowSlashEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ArmorReductionEffect = new("ArmorReductionEffect"u8);
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

        Game.Logger.LogInformation("💥 Initializing Crushing Blow Ability for Swordsman...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSequence(CrushingBlowAnim)
        {
            Name = "痛击动画序列",
            Playbacks = [new() 
            {
                AnimationRaw = "attack_02"u8,
                IsLooping = false,
                PlaybackDuration = TimeSpan.FromSeconds(1.3), // 痛击动画更长
            }],
            SequenceActors = [new() 
            {
                Actor = SlashEffect, // 在动画播放时生成痛击刀光特效
                SpawnOffset = TimeSpan.FromSeconds(0.3), // 动画开始0.3秒后显示刀光（比普通挥剑稍晚）
                Duration = TimeSpan.FromSeconds(0.6), // 痛击刀光持续更久
            }]
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(HitEffectParticle)
        {
            Asset = "effect/effect_new/effect_hit/eff_hit_29/particle.effect"u8,
        };

        _ = new GameDataParticle(SlashEffectParticle)
        {
            Asset = "effect/effect_new1/effect_knief/eff_knief01/particle.effect"u8,
        };

        _ = new GameDataParticle(ArmorReductionParticle)
        {
            Asset = "effect/effect_new1/effect_debuff/eff_yanshi_debuff/particle.effect"u8,
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
        };

        _ = new GameDataActorParticle(ArmorReductionEffect)
        {
            AutoPlay = true,
            Particle = ArmorReductionParticle,
            KillOnFinish = false, // 不要在播放完成后自动结束
            ForceOneShot = false, // 允许循环播放
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(CrushingBlowCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(5.0), // 5秒冷却
        };

        // ========== Buff效果配置 ==========
        _ = new GameDataBuff(ArmorReduction)
        {
            Name = "破甲",
            Description = "降低目标的护甲值，使其更容易受到物理伤害",
            Duration = static (_) => TimeSpan.FromSeconds(8),
            Icon = "image/buff/debuff_armor.png",
            Polarity = GameCore.BuffSystem.Data.Enum.BuffPolarity.Negative,
            Modifications = [
                new() {
                    Property = UnitProperty.Armor,
                    Value = (_) => -5, // 减少5点护甲
                    SubType = PropertySubType.Base
                }
            ],
            // 🔄 同步类型 - 目标可视范围内生效
            SyncType = SyncType.Sight,
            // 🎨 视觉特效配置 - 持续性破甲特效附着在目标身上，buff移除后自动移除
            ActorArray = [ArmorReductionEffect]
        };

        // ========== 效果配置 ==========
        // 痛击伤害效果
        _ = new GameDataEffectDamage(CrushingBlowDamage)
        {
            Name = "痛击伤害",
            Amount = static (_) => 180, // 基础伤害180，比挥剑高
            Type = DamageType.Physical,
            LogExecutionFailure = true,
            ActorArray = [HitEffect], // 添加受击特效
        };

        _ = new GameDataEffectBuffAdd(AddArmorReductionBuff)
        {
            Name = "添加破甲效果",
            BuffLink = ArmorReduction,
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectSet(CrushingBlowSet)
        {
            Name = "痛击效果组合",
            Effects = [
                new() { Link = CrushingBlowDamage },
                new() { Link = AddArmorReductionBuff }
            ],
            LogExecutionFailure = true,
        };

        // 痛击搜索效果
        _ = new GameDataEffectSearch(CrushingBlowSearch)
        {
            Name = "痛击范围搜索",
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            TargetLocation = new() { Value = TargetLocation.Caster },
            Radius = static (_) => 200, // 搜索半径200，匹配技能范围
            MaxCount = static (_) => 1, // 单体攻击，只攻击最近的一个敌人
            Effect = CrushingBlowSet, // 对找到的目标施加痛击效果组合
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(CrushingBlow)
        {
            Name = "痛击",
            DisplayName = "痛击",
            Description = "对目标造成强力物理伤害并降低其护甲值",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.4),
                Channel = static (_) => TimeSpan.FromSeconds(0.3),
                Backswing = static (_) => TimeSpan.FromSeconds(0.6),
            },
            
            Cost = new()
            {
                Cooldown = CrushingBlowCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { AlwaysAcquireTarget = true },
            Effect = CrushingBlowSearch, // 使用搜索效果而不是直接效果
            TargetType = AbilityTargetType.Vector, // 改为向量目标
            Range = static (_) => 200, // 近战范围
            
            AcquireSettings = new()
            {
                TargetingFilters = [new() {
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }],
            },
            
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = ARPGScopeData.TargetingIndicator.LineIndicator,
                CursorRadius = static (_) => 72,
                VectorLineWidth = static (_) => 72,
                VectorHighlightLimit = static (_) => 1
            },
            
            Animation = [CrushingBlowAnim],
            // ActorArray = [SlashEffect], // 添加刀光特效
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Crushing Blow Ability initialized successfully!");
    }
}
