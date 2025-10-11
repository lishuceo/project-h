using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.BuffSystem.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.BaseType; // 添加PropertyVital支持
using GameCore.VitalSystem; // 添加生命值系统支持
using GameData;
using static GameCore.ScopeData;

namespace GameEntry.ARPGTemplate.ScopeData.Abilities.Warrior;

/// <summary>
/// 战士技能：越战越勇
/// 被动技能，生命值越低攻击力越高
/// </summary>
public class BerserkerRageAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbility> BerserkerRage = new("BerserkerRage"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> BerserkerRageBuffApply = new("BerserkerRageBuffApply"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> BerserkerRageBuff = new("BerserkerRageBuff"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> BerserkerRageParticle = new("BerserkerRageParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> BerserkerRageEffect = new("BerserkerRageEffect"u8);
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

        Game.Logger.LogInformation("💢 Initializing Berserker Rage Ability for Warrior...");

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(BerserkerRageParticle)
        {
            Asset = "effect/effect_new1/effect_debuff/eff_zhuoshao/particle.effect"u8,
            AssetLayerScale = 0.6f,
            Radius = 48f,
        };

        _ = new GameDataActorParticle(BerserkerRageEffect)
        {
            AutoPlay = true,
            Particle = BerserkerRageParticle,
            KillOnFinish = true, // 不持续显示
            ForceOneShot = false, // 允许循环播放
        };

        // ========== Buff配置 ==========
        _ = new GameDataBuff(BerserkerRageBuff)
        {
            Name = "越战越勇",
            Duration = static (_) => TimeSpan.FromSeconds(1.0),
            
            // 根据生命值百分比动态计算攻击力加成：生命越低，攻击力越高
            Modifications = [
                new() 
                { 
                    Property = UnitProperty.AttackDamage,
                    SubType = PropertySubType.Base,
                    Value = static (context) => {
                        // 获取目标单位
                        var unit = context.Target?.Unit;
                        if (unit == null) return 0;
                        
                        try
                        {
                            // 获取生命值组件
                            var healthVital = unit.GetTagComponent<Vital>(PropertyVital.Health);
                            if (healthVital == null) return 0;
                            
                            var currentHealth = healthVital.Current;
                            var maxHealth = healthVital.Max;
                            
                            // 防止除零错误
                            if (maxHealth <= 0) return 0;
                            
                            // 计算生命值百分比 (0.0 到 1.0)
                            var healthPercent = currentHealth / maxHealth;
                            
                            // 限制百分比范围，防止异常值
                            healthPercent = Math.Max(0.0, Math.Min(1.0, healthPercent));
                            
                            // 计算攻击力加成：生命值100%时+0，生命值0%时+50
                            // 使用反向线性插值：(1 - healthPercent) * 50
                            var attackBonus = (1.0 - healthPercent) * 50.0;
                            
                            return attackBonus;
                        }
                        catch
                        {
                            // 发生异常时返回0，避免崩溃
                            return 0;
                        }
                    }
                }
            ],
            
            // 特效显示
            ActorArray = [BerserkerRageEffect],
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectBuffAdd(BerserkerRageBuffApply)
        {
            Name = "越战越勇被动效果",
            BuffLink = BerserkerRageBuff,
            TargetLocation = new() { Value = TargetLocation.Caster },
            LogExecutionFailure = true,
        };

        // ========== 被动技能配置 ==========
        _ = new GameDataAbility(BerserkerRage)
        {
            Name = "越战越勇",
            DisplayName = "越战越勇",
            Description = "被动技能：生命值越低，攻击力越高。生命值0%时+50攻击力，生命值100%时+0攻击力",
            
            // 被动技能配置
            PassivePeriod = static (_) => TimeSpan.FromSeconds(1.0), // 每秒检查一次
            PassivePeriodicEffect = BerserkerRageBuffApply, // 应用或刷新Buff
            TargetType = AbilityTargetType.None,
        };

        Game.Logger.LogInformation("✅ Berserker Rage Ability initialized successfully!");
    }
}
