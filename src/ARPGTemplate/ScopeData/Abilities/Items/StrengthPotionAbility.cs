using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.BuffSystem.Data;
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

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Items;

/// <summary>
/// 力量药剂技能：力量强化
/// 消耗品主动使用技能，为自身提供临时力量增强buff
/// </summary>
public class StrengthPotionAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> StrengthEnhance = new("StrengthEnhance"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> StrengthBuffApply = new("StrengthBuffApply"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> StrengthBuff = new("StrengthBuff"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> StrengthPotionCooldown = new("StrengthPotionCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> StrengthPotionAnim = new("StrengthPotionAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> StrengthBuffParticle = new("StrengthBuffParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> StrengthBuffVisualEffect = new("StrengthBuffVisualEffect"u8);
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

        Game.Logger.LogInformation("💪 Initializing Strength Potion Ability...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(StrengthPotionAnim)
        {
            Name = "使用力量药剂动画",
            File = "skill_01"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(StrengthBuffParticle)
        {
            Asset = "effect/effect_new1/effect_buff/eff_zengyi/particle.effect"u8, // 红色增益特效
            AssetLayerScale = 1.0f,
            Radius = 64f,
        };

        _ = new GameDataActorParticle(StrengthBuffVisualEffect)
        {
            AutoPlay = true,
            Particle = StrengthBuffParticle,
            KillOnFinish = false, // 持续显示在buff期间
            ForceOneShot = false, // 允许循环播放
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(StrengthPotionCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(5.0), // 5秒冷却，避免叠加使用
        };

        // ========== Buff配置 ==========
        _ = new GameDataBuff(StrengthBuff)
        {
            Name = "力量强化",
            Duration = static (_) => TimeSpan.FromSeconds(30.0), // 持续30秒
            
            // 提供攻击力和移动速度加成
            Modifications = [
                new() 
                { 
                    Property = UnitProperty.AttackDamage,
                    SubType = PropertySubType.Base,
                    Value = static (_) => 50 // +50攻击力
                },
                new() 
                { 
                    Property = UnitProperty.MoveSpeed,
                    SubType = PropertySubType.Base,
                    Value = static (_) => 30 // +30移动速度
                },
                new() 
                { 
                    Property = UnitProperty.LifeMax,
                    SubType = PropertySubType.Base,
                    Value = static (_) => 100 // +100最大生命值
                }
            ],
            
            // 特效显示
            ActorArray = [StrengthBuffVisualEffect],
            
            // Buff不可叠加，后续使用会刷新时间
            // MaxStacks = 1,
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectBuffAdd(StrengthBuffApply)
        {
            Name = "力量药剂增强效果",
            BuffLink = StrengthBuff,
            TargetLocation = new() { Value = TargetLocation.Caster },
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(StrengthEnhance)
        {
            Name = "力量强化",
            DisplayName = "使用力量药剂",
            Description = "使用力量药剂获得30秒力量强化效果：+50攻击力，+30移动速度，+100最大生命值",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.4),
                Channel = static (_) => TimeSpan.FromSeconds(0.8),
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            
            Cost = new()
            {
                Cooldown = StrengthPotionCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = false },
            AbilityExecuteFlags = new() { },
            Effect = StrengthBuffApply,
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
            
            Animation = [StrengthPotionAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Strength Potion Ability initialized successfully!");
        Game.Logger.LogInformation("   - 攻击力加成: +50");
        Game.Logger.LogInformation("   - 移动速度加成: +30");
        Game.Logger.LogInformation("   - 最大生命值加成: +100");
        Game.Logger.LogInformation("   - 持续时间: 30秒");
        Game.Logger.LogInformation("   - 冷却时间: 5秒");
        Game.Logger.LogInformation("   - 特效: 红色力量光环");
    }
}
