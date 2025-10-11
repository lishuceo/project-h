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

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Items;

/// <summary>
/// 生命药剂技能：生命恢复
/// 消耗品主动使用技能，瞬间恢复生命值
/// </summary>
public class HealthPotionAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> HealthRestore = new("HealthRestore"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectUnitModifyVital> HealthRestoreEffect = new("HealthRestoreEffect"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> HealthPotionCooldown = new("HealthPotionCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> HealthPotionAnim = new("HealthPotionAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> HealthRestoreParticle = new("HealthRestoreParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> HealthRestoreVisualEffect = new("HealthRestoreVisualEffect"u8);
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

        Game.Logger.LogInformation("🧪 Initializing Health Potion Ability...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(HealthPotionAnim)
        {
            Name = "使用生命药剂动画",
            File = "skill_01"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(HealthRestoreParticle)
        {
            Asset = "effect/effect_new/effect_buff/eff_buff_24/particle.effect"u8,
            AssetLayerScale = 1.2f,
            Radius = 80f,
        };

        _ = new GameDataActorParticle(HealthRestoreVisualEffect)
        {
            AutoPlay = true,
            Particle = HealthRestoreParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(HealthPotionCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(3.0), // 3秒短冷却，避免过度使用
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectUnitModifyVital(HealthRestoreEffect)
        {
            Name = "生命药剂恢复",
            TargetLocation = new() { Value = TargetLocation.Caster },
            Modification = [
                new() {
                    Property = PropertyVital.Health,
                    Value = static (_) => 300, // 恢复300点生命值
                }
            ],
            Operation = PropertyModificationOperation.Add, // 加法操作
            LogExecutionFailure = true,
            ActorArray = [HealthRestoreVisualEffect], // 添加治疗特效
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(HealthRestore)
        {
            Name = "生命恢复",
            DisplayName = "使用生命药剂",
            Description = "使用生命药剂瞬间恢复300点生命值",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.3),
                Channel = static (_) => TimeSpan.FromSeconds(0.5),
                Backswing = static (_) => TimeSpan.FromSeconds(0.2),
            },
            
            Cost = new()
            {
                Cooldown = HealthPotionCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = false },
            AbilityExecuteFlags = new() { },
            Effect = HealthRestoreEffect,
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
            
            Animation = [HealthPotionAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Health Potion Ability initialized successfully!");
        Game.Logger.LogInformation("   - 恢复生命值: 300点");
        Game.Logger.LogInformation("   - 冷却时间: 3秒");
        Game.Logger.LogInformation("   - 特效: 绿色治疗光芒");
    }
}
