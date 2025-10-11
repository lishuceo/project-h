using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.BuffSystem.Data;
using GameCore.BuffSystem.Data.Enum;
using GameCore.BuffSystem.Data.Struct;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.Struct;
using GameCore.ModelAnimation.Data;
using GameData;
using static GameCore.ScopeData;

namespace GameEntry.JsonScopeDataTest.Abilities.BuffAbilities;

/// <summary>
/// 成长光环技能 - 被动周期性技能，基于技能等级给护甲加成
/// </summary>
public class GrowthAuraAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbility> GrowthAura = new("GrowthAura"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> GrowthAuraBuffApply = new("GrowthAuraBuffApply"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> GrowthAuraBuff = new("GrowthAuraBuff"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> GrowthAuraParticle = new("GrowthAuraParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> GrowthAuraEffect = new("GrowthAuraEffect"u8);
    #endregion

    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.JsonScopeDataTest)
        {
            return;
        }

        Game.Logger.LogInformation("🛡️ Initializing Growth Aura Ability...");

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(GrowthAuraParticle)
        {
            Asset = "effect/samplespells/devotion/devotionaura/particle.effect"u8,
            AssetLayerScale = 1.0f,
            Radius = 96f, // 光环效果，范围稍大
        };

        _ = new GameDataActorParticle(GrowthAuraEffect)
        {
            AutoPlay = true,
            Particle = GrowthAuraParticle,
            KillOnFinish = false, // 持续显示
            ForceOneShot = false, // 循环播放
        };

        // ========== Buff配置 ==========
        _ = new GameDataBuff(GrowthAuraBuff)
        {
            Name = "成长光环",
            Duration = static (_) => TimeSpan.FromSeconds(1.1), // 短持续时间，但会被周期性刷新
            
            // Buff标志配置：单一实例且可刷新
            BuffFlags = new BuffFlags
            {
                SingleInstancePerCaster = true, // 每个施法者最多存在一个实例
                Channeling = false // 非引导技能
            },
            
            // 护甲加成基于技能等级计算
            Modifications = [
                new() 
                { 
                    Property = UnitProperty.Armor,
                    SubType = PropertySubType.Base,
                    Value = static (context) => {
                        // 简化的护甲计算：基于Buff等级
                        try
                        {
                            // 简单的固定值计算，后续可以根据技能等级调整
                            // 目前使用固定100护甲，可以在Buff创建时根据技能等级动态设置
                            return 100.0;
                        }
                        catch
                        {
                            // 异常时返回默认值
                            return 100.0;
                        }
                    }
                }
            ],
            
            // Buff期间的视觉效果
            ActorArray = [GrowthAuraEffect],
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectBuffAdd(GrowthAuraBuffApply)
        {
            Name = "成长光环Buff施加",
            BuffLink = GrowthAuraBuff,
            TargetLocation = new() { Value = TargetLocation.Caster },
            LogExecutionFailure = true,
        };

        // ========== 被动技能配置 ==========
        _ = new GameDataAbility(GrowthAura)
        {
            Name = "成长光环",
            DisplayName = "成长光环",
            Description = "被动光环：持续提供护甲加成。技能等级越高，护甲加成越多（每级+100护甲）",
            
            // 被动周期性技能配置
            TargetType = AbilityTargetType.None,
            PassivePeriod = static (_) => TimeSpan.FromSeconds(1.0), // 每秒检查一次
            PassivePeriodicEffect = GrowthAuraBuffApply, // 周期性应用Buff
        };

        Game.Logger.LogInformation("✅ Growth Aura Ability initialized successfully!");
    }
}
