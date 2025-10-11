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
using GameCore.EntitySystem;
using GameCore.VitalSystem;
using GameCore.VitalSystem.Data;
using GameCore.Struct;
using GameData;
using static GameCore.ScopeData;

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Gunner;

/// <summary>
/// 枪手技能：绷带
/// 辅助自身治疗技能
/// </summary>
public class BandageAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Bandage = new("Bandage"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectUnitModifyVital> BandageHeal = new("BandageHeal"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> BandageCooldown = new("BandageCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> BandageAnim = new("BandageAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> HealingParticle = new("HealingParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> HealingEffect = new("HealingEffect"u8);
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

        Game.Logger.LogInformation("🩹 Initializing Bandage Ability for Gunner...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(BandageAnim)
        {
            Name = "绷带治疗动画",
            File = "skill_01"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(HealingParticle)
        {
            Asset = "effect/effect_new/effect_buff/eff_buff_24/particle.effect"u8,
            AssetLayerScale = 0.8f,
            Radius = 64f,
        };

        _ = new GameDataActorParticle(HealingEffect)
        {
            AutoPlay = true,
            Particle = HealingParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(BandageCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(12.0), // 12秒冷却
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectUnitModifyVital(BandageHeal)
        {
            Name = "绷带治疗",
            TargetLocation = new() { Value = TargetLocation.Caster },
            Modification = [
                new() {
                    Property = PropertyVital.Health,
                    Value = static (_) => 200, // 恢复200点生命值
                }
            ],
            Operation = PropertyModificationOperation.Add, // 加法操作
            LogExecutionFailure = true,
            ActorArray = [HealingEffect], // 添加治疗特效
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Bandage)
        {
            Name = "绷带",
            DisplayName = "绷带",
            Description = "使用绷带为自己恢复生命值",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.5),
                Channel = static (_) => TimeSpan.FromSeconds(1.0),
                Backswing = static (_) => TimeSpan.FromSeconds(0.2),
            },
            
            Cost = new()
            {
                Cooldown = BandageCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = BandageHeal,
            TargetType = AbilityTargetType.None, // 自身目标，无需选择
            Range = static (_) => 0,
            
            // 目标过滤器设置为自身
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                { 
                    Required = [UnitRelationship.Self]
                }],
            },
            
            Animation = [BandageAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Bandage Ability initialized successfully!");
    }
}
