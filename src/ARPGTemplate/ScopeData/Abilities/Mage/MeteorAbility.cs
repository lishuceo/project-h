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
using GameEntry.ARPGTemplate.ScopeData;
using GameCore.Components;
using GameCore.Components.Data;
namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Mage;

/// <summary>
/// 法师技能：陨石术
/// 远程范围魔法伤害技能
/// </summary>
public class MeteorAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Meteor = new("Meteor"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> MeteorDamageSearch = new("MeteorDamageSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> MeteorDamage = new("MeteorDamage"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> MeteorCooldown = new("MeteorCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> MeteorAnim = new("MeteorAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> MeteorImpactParticle = new("MeteorImpactParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> MeteorImpactEffect = new("MeteorImpactEffect"u8);
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

        Game.Logger.LogInformation("☄️ Initializing Meteor Ability for Mage...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(MeteorAnim)
        {
            Name = "陨石术动画",
            File = "skill_01"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(MeteorImpactParticle)
        {
            Asset = "effect/effect_new/effect_mofa/eff_touzhiwu_01/particle.effect"u8,
            AssetLayerScale = 1.5f, // 更大的爆炸效果
            Radius = 192f,
        };

        // ========== Actor配置 ==========
        _ = new GameDataActorParticle(MeteorImpactEffect)
        {
            AutoPlay = true,
            Particle = MeteorImpactParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(MeteorCooldown)
        {
            Time = static (context) => {
                // 基础冷却时间8秒
                double baseCooldown = 8.0;
                
                // 获取冷却缩减属性
                if (context?.Caster is GameCore.EntitySystem.Unit caster)
                {
                    var cooldownReduction = caster.GetUnitPropertyFinal(ARPGUnitPropertyLink.CooldownReduction);
                    if (cooldownReduction.HasValue)
                    {
                        Game.Logger.LogInformation($"冷却缩减率: {cooldownReduction}");
                        // 应用冷却缩减：最终冷却时间 = 基础冷却时间 * (1 - 缩减率)
                        baseCooldown *= (1.0 - cooldownReduction.Value);
                    }
                    
                }
                
                return TimeSpan.FromSeconds(baseCooldown);
            },
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectDamage(MeteorDamage)
        {
            Name = "陨石术伤害",
            Amount = static (_) => 180, // 基础伤害180，比火球术高
            Type = DamageType.Magical, // 魔法伤害
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectSearch(MeteorDamageSearch)
        {
            Name = "陨石术范围搜索",
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Radius = static (_) => 300, // 搜索半径300，范围攻击
            ActorArray = [MeteorImpactEffect], // 添加撞击特效
            Effect = MeteorDamage,
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(Meteor)
        {
            Name = "陨石术",
            DisplayName = "陨石术",
            Description = "召唤陨石坠落到指定区域，对范围内敌人造成魔法伤害",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.8),
                Channel = static (_) => TimeSpan.FromSeconds(0.5),
                Backswing = static (_) => TimeSpan.FromSeconds(0.4),
            },
            
            Cost = new()
            {
                Cooldown = MeteorCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = MeteorDamageSearch,
            TargetType = AbilityTargetType.Ground,
            Range = static (_) => 700, // 远程施法范围700
            
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
                TargetingIndicator = ARPGScopeData.TargetingIndicator.AreaIndicator,
                CursorRadius = static (_) => 300, // 显示影响范围
            },
            
            Animation = [MeteorAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Meteor Ability initialized successfully!");
    }
}
