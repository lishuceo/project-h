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
using GameCore.Struct;
using GameData;
using static GameCore.ScopeData;
using GameEntry.ARPGTemplate.ScopeData;
using GameCore.Components;
using GameCore.Components.Data;
namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Mage;

/// <summary>
/// 法师技能：超载
/// 辅助自身增益技能，减少冷却时间
/// </summary>
public class OverloadAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Overload = new("Overload"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> OverloadBuffApply = new("OverloadBuffApply"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> OverloadBuff = new("OverloadBuff"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> OverloadCooldown = new("OverloadCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> OverloadAnim = new("OverloadAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> OverloadParticle = new("OverloadParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> OverloadEffect = new("OverloadEffect"u8);
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

        Game.Logger.LogInformation("⚡ Initializing Overload Ability for Mage...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(OverloadAnim)
        {
            Name = "超载动画",
            File = "skill_02"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(OverloadParticle)
        {
            Asset = "effect/effect_new/effect_buff/eff_ranxuemishu/particle.effect"u8,
            AssetLayerScale = 1.2f,
            Radius = 96f,
        };

        _ = new GameDataActorParticle(OverloadEffect)
        {
            AutoPlay = true,
            Particle = OverloadParticle,
            KillOnFinish = true,
            ForceOneShot = false,
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(OverloadCooldown)
        {
            Time = static (context) => {
                // 基础冷却时间20秒
                double baseCooldown = 20.0;
                
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

        // ========== Buff配置 ==========
        _ = new GameDataBuff(OverloadBuff)
        {
            Name = "超载状态",
            Description = "所有技能冷却时间减少20%",
            Duration = static (_) => TimeSpan.FromSeconds(15.0), // 15秒持续时间
            ActorArray = [OverloadEffect], // 添加持续特效
            Polarity = GameCore.BuffSystem.Data.Enum.BuffPolarity.Positive, // 正面效果
            
            // 🎯 冷却缩减属性修改 - 20%冷却缩减
            Modifications = [
                new() {
                    Property = ARPGUnitPropertyLink.CooldownReduction,
                    Value = static (_) => 0.2, // 20%冷却缩减
                    SubType = PropertySubType.Base
                }
            ],
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectBuffAdd(OverloadBuffApply)
        {
            Name = "超载增益效果",
            BuffLink = OverloadBuff,
            TargetLocation = new() { Value = TargetLocation.Caster },
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Overload)
        {
            Name = "超载",
            DisplayName = "超载",
            Description = "进入超载状态，提供视觉效果（冷却缩减功能待实现），持续15秒",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.3),
                Channel = static (_) => TimeSpan.FromSeconds(0.5),
                Backswing = static (_) => TimeSpan.FromSeconds(0.2),
            },
            
            Cost = new()
            {
                Cooldown = OverloadCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = OverloadBuffApply,
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
            
            Animation = [OverloadAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Overload Ability initialized successfully!");
    }
}
