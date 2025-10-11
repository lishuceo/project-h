using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.CooldownSystem.Data;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.Container;
using GameData;
using System.Numerics;
using GameCore.TargetingSystem.Data;
using GameCore.Behavior;
using GameCore.BuffSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.ModelAnimation.Data;
using GameCore.SceneSystem.Data;
using GameCore.ActorSystem.Data;
using GameCore.Data;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using Microsoft.Extensions.Logging;
using EngineInterface.BaseType;
using static GameCore.ScopeData;
using GameEntry.ArtAsset;

namespace GameEntry.XianJianQiXiaZhuan.ScopeData;

/// <summary>
/// 仙剑奇侠传技能系统定义
/// </summary>
public class XianJianAbilities : IGameClass
{
    #region 技能定义
    public static class Ability
    {
        // === 李逍遥技能 ===
        /// <summary>剑气斩 - 李逍遥基础剑技</summary>
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> SwordSlash = new("SwordSlash"u8);
        
        /// <summary>仙风云体术 - 李逍遥身法技能</summary>
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> XianFengSpell = new("XianFengSpell"u8);
        
        /// <summary>万剑诀 - 李逍遥群体攻击技能，发射多把剑气攻击范围内所有敌人</summary>
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> TenThousandSwords = new("TenThousandSwords"u8);
        
        /// <summary>御剑术 - 李逍遥远程技能，发射飞剑攻击敌人</summary>
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> SwordControl = new("SwordControl"u8);
        
        // === 赵灵儿技能 ===
        /// <summary>治疗术 - 赵灵儿回复技能</summary>
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> HealingSpell = new("HealingSpell"u8);
        
        /// <summary>雷系法术 - 赵灵儿攻击法术</summary>
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> LightningSpell = new("LightningSpell"u8);
        
        /// <summary>水系法术 - 赵灵儿控制法术</summary>
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> WaterSpell = new("WaterSpell"u8);
        
        // === 林月如技能 ===
        /// <summary>剑法连击 - 林月如连续攻击</summary>
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> SwordCombo = new("SwordCombo"u8);
        
        /// <summary>疾风剑 - 林月如高速攻击</summary>
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> QuickStrike = new("QuickStrike"u8);
        
        // === 敌人技能 ===
        /// <summary>毒攻击 - 蛇妖技能</summary>
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> PoisonAttack = new("PoisonAttack"u8);
    }
    #endregion

    #region 冷却时间定义
    public static class Cooldown
    {
        public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> SwordSlashCooldown = new("SwordSlashCooldown"u8);
        public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> XianFengSpellCooldown = new("XianFengSpellCooldown"u8);
        public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> TenThousandSwordsCooldown = new("TenThousandSwordsCooldown"u8);
        public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> SwordControlCooldown = new("SwordControlCooldown"u8);
        public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> HealingSpellCooldown = new("HealingSpellCooldown"u8);
        public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> LightningSpellCooldown = new("LightningSpellCooldown"u8);
        public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> WaterSpellCooldown = new("WaterSpellCooldown"u8);
        public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> SwordComboCooldown = new("SwordComboCooldown"u8);
        public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> QuickStrikeCooldown = new("QuickStrikeCooldown"u8);
        public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> PoisonAttackCooldown = new("PoisonAttackCooldown"u8);
    }
    #endregion

    #region 效果定义
    public static class Effect
    {
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> SwordSlashDamage = new("SwordSlashDamage"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectLog> HealingEffect = new("HealingEffect"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> LightningDamage = new("LightningDamage"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> WaterDamage = new("WaterDamage"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> SwordComboDamage = new("SwordComboDamage"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> QuickStrikeDamage = new("QuickStrikeDamage"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> PoisonDamage = new("PoisonDamage"u8);
        
        // === 剑气斩效果 ===
        /// <summary>剑气斩日志效果</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLog> SwordSlashLog = new("SwordSlashLog"u8);
        
        /// <summary>剑气斩范围搜索效果 - 立即生效</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectSearch> SwordSlashSearch = new("SwordSlashSearch"u8);
        
        /// <summary>剑气斩投射物发射效果（保留备用）</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> SwordSlashLaunchMissile = new("SwordSlashLaunchMissile"u8);
        
        /// <summary>剑气斩完整效果集合</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> SwordSlashCompleteEffect = new("SwordSlashCompleteEffect"u8);
        
        // === 仙风云体术效果 ===
        /// <summary>仙风云体术Buff添加效果</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> XianFengSpeedBuffAdd = new("XianFengSpeedBuffAdd"u8);
        
        /// <summary>仙风云体术日志效果</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLog> XianFengSpellLog = new("XianFengSpellLog"u8);
        
        /// <summary>仙风云体术完整效果集合</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> XianFengSpellCompleteEffect = new("XianFengSpellCompleteEffect"u8);
        
        // === 万剑诀效果 ===
        /// <summary>万剑诀伤害效果</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> TenThousandSwordsDamage = new("TenThousandSwordsDamage"u8);
        
        /// <summary>万剑诀目标搜索效果</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectSearch> TenThousandSwordsSearch = new("TenThousandSwordsSearch"u8);
        
        /// <summary>万剑诀投射物发射效果 - 第1把剑</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> TenThousandSwordsLaunchMissile1 = new("TenThousandSwordsLaunchMissile1"u8);
        /// <summary>万剑诀投射物发射效果 - 第2把剑</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> TenThousandSwordsLaunchMissile2 = new("TenThousandSwordsLaunchMissile2"u8);
        /// <summary>万剑诀投射物发射效果 - 第3把剑</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> TenThousandSwordsLaunchMissile3 = new("TenThousandSwordsLaunchMissile3"u8);
        /// <summary>万剑诀投射物发射效果 - 第4把剑</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> TenThousandSwordsLaunchMissile4 = new("TenThousandSwordsLaunchMissile4"u8);
        /// <summary>万剑诀投射物发射效果 - 第5把剑</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> TenThousandSwordsLaunchMissile5 = new("TenThousandSwordsLaunchMissile5"u8);
        /// <summary>万剑诀投射物发射效果 - 第6把剑</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> TenThousandSwordsLaunchMissile6 = new("TenThousandSwordsLaunchMissile6"u8);
        /// <summary>万剑诀投射物发射效果 - 第7把剑</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> TenThousandSwordsLaunchMissile7 = new("TenThousandSwordsLaunchMissile7"u8);
        /// <summary>万剑诀投射物发射效果 - 第8把剑</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> TenThousandSwordsLaunchMissile8 = new("TenThousandSwordsLaunchMissile8"u8);
        
        /// <summary>万剑诀日志效果</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLog> TenThousandSwordsLog = new("TenThousandSwordsLog"u8);
        
        /// <summary>万剑诀延迟效果 - 0.5秒后发射所有剑</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectPersistDelay> TenThousandSwordsDelay = new("TenThousandSwordsDelay"u8);
        
        /// <summary>万剑诀发射集合 - 同时发射8把剑</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> TenThousandSwordsLaunchSet = new("TenThousandSwordsLaunchSet"u8);
        
        /// <summary>万剑诀完整效果集合</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> TenThousandSwordsCompleteEffect = new("TenThousandSwordsCompleteEffect"u8);
        
        // === 御剑术效果 ===
        /// <summary>御剑术投射物发射效果</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> SwordControlLaunchMissile = new("SwordControlLaunchMissile"u8);
        
        /// <summary>御剑术伤害效果</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> SwordControlDamage = new("SwordControlDamage"u8);
        
        /// <summary>御剑术日志效果</summary>
        public static readonly GameLink<GameDataEffect, GameDataEffectLog> SwordControlLog = new("SwordControlLog"u8);
    }
    #endregion

    #region 动画定义
    public static class Animation
    {
        /// <summary>剑气斩施法动画</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataAnimation, GameCore.ModelAnimation.Data.GameDataAnimationSimple> SwordSlashAnim = new("SwordSlashAnim"u8);
        
        /// <summary>万剑诀施法动画</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataAnimation, GameCore.ModelAnimation.Data.GameDataAnimationSimple> TenThousandSwordsAnim = new("TenThousandSwordsAnim"u8);
        
        /// <summary>御剑术施法动画</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataAnimation, GameCore.ModelAnimation.Data.GameDataAnimationSimple> SwordControlAnim = new("SwordControlAnim"u8);
    }
    #endregion

    #region 粒子效果定义
    public static class Particle
    {
        /// <summary>剑气发射特效粒子</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataParticle, GameCore.ResourceType.Data.GameDataParticle> SwordQiParticle = new("SwordQiParticle"u8);
        
        /// <summary>剑气击中特效粒子</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataParticle, GameCore.ResourceType.Data.GameDataParticle> SwordQiHitParticle = new("SwordQiHitParticle"u8);
        
        /// <summary>施法光效粒子</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataParticle, GameCore.ResourceType.Data.GameDataParticle> CastEffectParticle = new("CastEffectParticle"u8);
        
        /// <summary>仙风云体术持续特效粒子</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataParticle, GameCore.ResourceType.Data.GameDataParticle> XianFengBuffParticle = new("XianFengBuffParticle"u8);
        
        // === 万剑诀粒子效果 ===
        /// <summary>万剑诀剑气粒子</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataParticle, GameCore.ResourceType.Data.GameDataParticle> TenThousandSwordsParticle = new("TenThousandSwordsParticle"u8);
        
        /// <summary>万剑诀击中粒子</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataParticle, GameCore.ResourceType.Data.GameDataParticle> TenThousandSwordsHitParticle = new("TenThousandSwordsHitParticle"u8);
        
        // === 御剑术粒子效果 ===
        /// <summary>御剑术飞剑弹道粒子</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataParticle, GameCore.ResourceType.Data.GameDataParticle> SwordControlMissileParticle = new("SwordControlMissileParticle"u8);
        
        /// <summary>御剑术击中粒子</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataParticle, GameCore.ResourceType.Data.GameDataParticle> SwordControlHitParticle = new("SwordControlHitParticle"u8);
        
        /// <summary>御剑术施法粒子</summary>
        public static readonly GameLink<GameCore.ResourceType.Data.GameDataParticle, GameCore.ResourceType.Data.GameDataParticle> SwordControlCastParticle = new("SwordControlCastParticle"u8);
    }
    #endregion

    #region Actor定义
    public static class Actor
    {
        /// <summary>剑气发射特效Actor</summary>
        public static readonly GameLink<GameCore.ActorSystem.Data.GameDataActor, GameCore.ActorSystem.Data.GameDataActorParticle> SwordQiEffect = new("SwordQiEffect"u8);
        
        /// <summary>剑气击中特效Actor</summary>
        public static readonly GameLink<GameCore.ActorSystem.Data.GameDataActor, GameCore.ActorSystem.Data.GameDataActorParticle> SwordQiHitEffect = new("SwordQiHitEffect"u8);
        
        /// <summary>施法光效Actor</summary>
        public static readonly GameLink<GameCore.ActorSystem.Data.GameDataActor, GameCore.ActorSystem.Data.GameDataActorParticle> CastEffect = new("CastEffect"u8);
        
        /// <summary>仙风云体术持续特效Actor</summary>
        public static readonly GameLink<GameCore.ActorSystem.Data.GameDataActor, GameCore.ActorSystem.Data.GameDataActorParticle> XianFengBuffEffect = new("XianFengBuffEffect"u8);
        
        // === 万剑诀Actor ===
        /// <summary>万剑诀剑气Actor</summary>
        public static readonly GameLink<GameCore.ActorSystem.Data.GameDataActor, GameCore.ActorSystem.Data.GameDataActorParticle> TenThousandSwordsEffect = new("TenThousandSwordsEffect"u8);
        
        /// <summary>万剑诀击中Actor</summary>
        public static readonly GameLink<GameCore.ActorSystem.Data.GameDataActor, GameCore.ActorSystem.Data.GameDataActorParticle> TenThousandSwordsHitEffect = new("TenThousandSwordsHitEffect"u8);
        
        // === 御剑术Actor ===
        /// <summary>御剑术击中Actor</summary>
        public static readonly GameLink<GameCore.ActorSystem.Data.GameDataActor, GameCore.ActorSystem.Data.GameDataActorParticle> SwordControlHitEffect = new("SwordControlHitEffect"u8);
        
        /// <summary>御剑术施法Actor</summary>
        public static readonly GameLink<GameCore.ActorSystem.Data.GameDataActor, GameCore.ActorSystem.Data.GameDataActorParticle> SwordControlCastEffect = new("SwordControlCastEffect"u8);
    }
    #endregion

    #region 投射物单位定义
    public static class Missile
    {
        /// <summary>剑气投射物单位</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> SwordQiMissile = new("SwordQiMissile"u8);
        
        /// <summary>万剑诀剑气投射物单位</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> TenThousandSwordsMissile = new("TenThousandSwordsMissile"u8);
        
        /// <summary>御剑术飞剑投射物单位</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> SwordControlMissile = new("SwordControlMissile"u8);
    }
    #endregion

    #region Buff定义
    public static class Buff
    {
        /// <summary>仙风云体术移动速度增益Buff</summary>
        public static readonly GameLink<GameDataBuff, GameDataBuff> XianFengSpeedBuff = new("XianFengSpeedBuff"u8);
    }
    #endregion

    public static void OnRegisterGameClass()
    {
        // 先移除可能已存在的订阅，避免重复注册
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        // 只在仙剑奇侠传模式下初始化
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.XianJianQiXiaZhuan)
        {
            return;
        }

        Game.Logger.LogInformation("⚔️ 初始化仙剑奇侠传技能系统...");

        // 初始化各个子系统
        InitializeCooldowns();    // 冷却时间
        InitializeBuffs();        // Buff效果
        InitializeAnimations();   // 动画配置
        InitializeParticles();    // 粒子效果
        InitializeActors();       // Actor配置
        InitializeMissiles();     // 投射物单位
        InitializeEffects();      // 技能效果
        InitializeAbilities();    // 技能配置

        Game.Logger.LogInformation("✅ 仙剑奇侠传技能系统初始化完成!");
    }

    /// <summary>
    /// 初始化冷却时间配置
    /// </summary>
    private static void InitializeCooldowns()
    {
        Game.Logger.LogInformation("⏰ 配置仙剑技能冷却时间...");

        // ========== 冷却时间配置 ==========
        _ = new GameDataCooldownActive(Cooldown.SwordSlashCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(0.8) // 剑气斩冷却0.8秒（更快连击）
        };

        _ = new GameDataCooldownActive(Cooldown.XianFengSpellCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(8.0) // 仙风云体术冷却8秒
        };

        _ = new GameDataCooldownActive(Cooldown.TenThousandSwordsCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(12.0) // 万剑诀冷却12秒（群体大招）
        };

        _ = new GameDataCooldownActive(Cooldown.SwordControlCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(1.0) // 御剑术冷却1秒（快速攻击技能）
        };

        _ = new GameDataCooldownActive(Cooldown.HealingSpellCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(3.0) // 治疗术冷却3秒
        };

        _ = new GameDataCooldownActive(Cooldown.LightningSpellCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(2.5) // 雷系法术冷却2.5秒
        };

        _ = new GameDataCooldownActive(Cooldown.WaterSpellCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(4.0) // 水系法术冷却4秒
        };

        _ = new GameDataCooldownActive(Cooldown.SwordComboCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(6.0) // 剑法连击冷却6秒
        };

        _ = new GameDataCooldownActive(Cooldown.QuickStrikeCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(3.5) // 疾风剑冷却3.5秒
        };

        _ = new GameDataCooldownActive(Cooldown.PoisonAttackCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(2.0) // 毒攻击冷却2秒
        };

        Game.Logger.LogInformation("✅ 仙剑技能冷却时间配置完成!");
    }

    /// <summary>
    /// 初始化技能效果配置
    /// </summary>
    private static void InitializeEffects()
    {
        Game.Logger.LogInformation("💥 配置仙剑技能效果...");

        // ========== 伤害效果 ==========
        _ = new GameDataEffectDamage(Effect.SwordSlashDamage)
        {
            Name = "剑气斩伤害",
            Amount = static (context) => {
                // 获取施法者的最终攻击力
                if (context?.Caster is GameCore.EntitySystem.Unit caster)
                {
                    var attackDamage = caster.GetUnitPropertyFinal(UnitProperty.AttackDamage);
                    if (attackDamage.HasValue)
                    {
                        // 剑气斩伤害 = 最终攻击力 × 50%
                        var damage = attackDamage.Value * 0.5;
                        Game.Logger.LogInformation($"🔍 剑气斩: 攻击力{attackDamage.Value:F0} → 伤害{damage:F0}");
                        return damage;
                    }
                }
                
                // 默认伤害
                Game.Logger.LogWarning("⚠️ 使用默认伤害17.5");
                return 17.5;
            },
            Type = GameCore.ScopeData.DamageType.Physical,
            ActorArray = [Actor.SwordQiHitEffect], // 添加击中特效
            LogExecutionFailure = true,
        };

        // 剑气斩日志效果 - 增强调试信息
        _ = new GameDataEffectLog(Effect.SwordSlashLog)
        {
            Name = "剑气斩日志",
            Message = context => 
            {
                // 计算实际伤害值用于显示
                double damage = 40; // 默认伤害
                if (context?.Caster is GameCore.EntitySystem.Unit caster)
                {
                    var attackDamage = caster.GetUnitPropertyFinal(UnitProperty.AttackDamage);
                    if (attackDamage.HasValue)
                    {
                        damage = attackDamage.Value * 0.5;
                    }
                }
                return $"⚔️ 剑气斩范围攻击生效！造成{damage:F0}点物理伤害（攻击力50%）";
            },
            Level = LogLevel.Information,
            LogExecutionFailure = true,
        };

        // 剑气投射物撞击效果集合（包含伤害和击中特效）
        _ = new GameDataEffectSet(Effect.SwordSlashCompleteEffect)
        {
            Name = "剑气撞击完整效果",
            Effects = [
                new() { Link = Effect.SwordSlashLog },
                new() { Link = Effect.SwordSlashDamage },
            ],
        };

        // 剑气斩范围搜索效果 - 立即生效（完全参考ARPGTemplate）
        _ = new GameDataEffectSearch(Effect.SwordSlashSearch)
        {
            Name = "剑气斩范围搜索",
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            TargetLocation = new() { Value = TargetLocation.Caster }, // 以施法者为中心（参考ARPG）
            Radius = static (_) => 250, // 搜索半径250（参考ARPG）
            Effect = Effect.SwordSlashDamage, // 直接使用伤害效果（参考ARPG）
            LogExecutionFailure = true,
        };

        // 剑气投射物发射效果（保留备用）
        _ = new GameDataEffectLaunchMissile(Effect.SwordSlashLaunchMissile)
        {
            Name = "剑气发射",
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Missile.SwordQiMissile,
            LaunchHeight = static (_) => 150, // 发射高度
            TargetHeight = static (_) => 80,  // 目标高度
            Speed = static (_) => 2400f, // 剑气飞行速度（超高速，几乎瞬间到达）
            CompleteEffect = Effect.SwordSlashCompleteEffect, // 撞击时执行的完整效果
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectDamage(Effect.LightningDamage)
        {
            Amount = static (_) => 120, // 雷系法术伤害
            Type = GameCore.ScopeData.DamageType.Magical,
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectDamage(Effect.WaterDamage)
        {
            Amount = static (_) => 90, // 水系法术伤害
            Type = GameCore.ScopeData.DamageType.Magical,
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectDamage(Effect.SwordComboDamage)
        {
            Amount = static (_) => 150, // 剑法连击伤害
            Type = GameCore.ScopeData.DamageType.Physical,
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectDamage(Effect.QuickStrikeDamage)
        {
            Amount = static (_) => 100, // 疾风剑伤害
            Type = GameCore.ScopeData.DamageType.Physical,
            LogExecutionFailure = true,
        };

        _ = new GameDataEffectDamage(Effect.PoisonDamage)
        {
            Amount = static (_) => 60, // 毒攻击伤害
            Type = GameCore.ScopeData.DamageType.Magical,
            LogExecutionFailure = true,
        };

        // ========== 治疗效果 ==========
        _ = new GameDataEffectLog(Effect.HealingEffect)
        {
            Message = static (e) => $"治疗术回复了生命值",
            LogExecutionFailure = true,
        };

        // ========== 仙风云体术效果 ==========
        // 日志效果
        _ = new GameDataEffectLog(Effect.XianFengSpellLog)
        {
            Name = "仙风云体术日志",
            Message = context => 
            {
                var casterName = "李逍遥"; // 直接使用固定名称
                return $"💨 {casterName} 施展仙风云体术，身法如风，移动速度大幅提升！";
            },
            Level = LogLevel.Information,
            LogExecutionFailure = true,
        };

        // Buff添加效果
        _ = new GameDataEffectBuffAdd(Effect.XianFengSpeedBuffAdd)
        {
            BuffLink = Buff.XianFengSpeedBuff,
            LogExecutionFailure = true,
        };

        // 完整效果集合
        _ = new GameDataEffectSet(Effect.XianFengSpellCompleteEffect)
        {
            Effects = [
                new() { Link = Effect.XianFengSpellLog },
                new() { Link = Effect.XianFengSpeedBuffAdd },
            ],
        };

        // ========== 万剑诀效果 ==========
        // 万剑诀伤害效果
        _ = new GameDataEffectDamage(Effect.TenThousandSwordsDamage)
        {
            Name = "万剑诀伤害",
            Amount = static (context) => {
                // 获取施法者的最终攻击力
                if (context?.Caster is GameCore.EntitySystem.Unit caster)
                {
                    var attackDamage = caster.GetUnitPropertyFinal(UnitProperty.AttackDamage);
                    if (attackDamage.HasValue)
                    {
                        // 万剑诀伤害 = 最终攻击力 × 80%（比剑气斩更强）
                        var damage = attackDamage.Value * 0.8;
                        Game.Logger.LogInformation($"🔍 万剑诀: 攻击力{attackDamage.Value:F0} → 伤害{damage:F0}");
                        return damage;
                    }
                }
                
                // 默认伤害
                Game.Logger.LogWarning("⚠️ 万剑诀使用默认伤害80");
                return 80;
            },
            Type = GameCore.ScopeData.DamageType.Physical,
            ActorArray = [Actor.TenThousandSwordsHitEffect], // 添加击中特效
            LogExecutionFailure = true,
        };

        // 万剑诀日志效果
        _ = new GameDataEffectLog(Effect.TenThousandSwordsLog)
        {
            Name = "万剑诀日志",
            Message = context => 
            {
                // 计算实际伤害值用于显示
                double damage = 80; // 默认伤害
                if (context?.Caster is GameCore.EntitySystem.Unit caster)
                {
                    var attackDamage = caster.GetUnitPropertyFinal(UnitProperty.AttackDamage);
                    if (attackDamage.HasValue)
                    {
                        damage = attackDamage.Value * 0.8;
                    }
                }
                return $"⚔️ 万剑诀群体攻击！万剑齐发，造成{damage:F0}点物理伤害（攻击力80%）";
            },
            Level = LogLevel.Information,
            LogExecutionFailure = true,
        };

        // 万剑诀目标搜索效果（已废弃，现在是单体技能）
        _ = new GameDataEffectSearch(Effect.TenThousandSwordsSearch)
        {
            Name = "万剑诀目标搜索（废弃）",
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            TargetLocation = new() { Value = TargetLocation.Caster }, // 以施法者为中心
            Radius = static (_) => 400, // 搜索半径400
            Effect = Effect.TenThousandSwordsDamage, // 直接造成伤害
            MaxCount = static (_) => 8, // 最多攻击8个目标
            LogExecutionFailure = true,
        };

        // ========== 万剑诀8个发射效果 - 每个都有不同的发射位置 ==========
        
        // 第1把剑 - 最左侧
        _ = new GameDataEffectLaunchMissile(Effect.TenThousandSwordsLaunchMissile1)
        {
            Name = "万剑诀第1剑",
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Missile.TenThousandSwordsMissile,
            LaunchHeight = static (_) => 150,
            TargetHeight = static (_) => 80,
            Speed = static (_) => 2000f,
            CompleteEffect = Effect.TenThousandSwordsDamage,
            // LaunchOffset 暂时移除，通过其他方式实现位置偏移
            DoImpactEntity = true,
            DoStaticCollision = true,
            ImpactMaxCount = static (_) => 1,
            ImpactSearchRadius = static (_) => 32f,
            ImpactSearchFilter = [new() { Excluded = [UnitState.Dead, UnitState.Invulnerable], Required = [UnitRelationship.Enemy] }],
            ImpactEffect = Effect.TenThousandSwordsDamage,
            LogExecutionFailure = true,
        };
        
        // 第2把剑
        _ = new GameDataEffectLaunchMissile(Effect.TenThousandSwordsLaunchMissile2)
        {
            Name = "万剑诀第2剑",
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Missile.TenThousandSwordsMissile,
            LaunchHeight = static (_) => 150,
            TargetHeight = static (_) => 80,
            Speed = static (_) => 2000f,
            CompleteEffect = Effect.TenThousandSwordsDamage,
            // LaunchOffset 暂时移除，通过其他方式实现位置偏移
            DoImpactEntity = true,
            DoStaticCollision = true,
            ImpactMaxCount = static (_) => 1,
            ImpactSearchRadius = static (_) => 32f,
            ImpactSearchFilter = [new() { Excluded = [UnitState.Dead, UnitState.Invulnerable], Required = [UnitRelationship.Enemy] }],
            ImpactEffect = Effect.TenThousandSwordsDamage,
            LogExecutionFailure = true,
        };
        
        // 第3把剑
        _ = new GameDataEffectLaunchMissile(Effect.TenThousandSwordsLaunchMissile3)
        {
            Name = "万剑诀第3剑",
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Missile.TenThousandSwordsMissile,
            LaunchHeight = static (_) => 150,
            TargetHeight = static (_) => 80,
            Speed = static (_) => 2000f,
            CompleteEffect = Effect.TenThousandSwordsDamage,
            // LaunchOffset 暂时移除，通过其他方式实现位置偏移
            DoImpactEntity = true,
            DoStaticCollision = true,
            ImpactMaxCount = static (_) => 1,
            ImpactSearchRadius = static (_) => 32f,
            ImpactSearchFilter = [new() { Excluded = [UnitState.Dead, UnitState.Invulnerable], Required = [UnitRelationship.Enemy] }],
            ImpactEffect = Effect.TenThousandSwordsDamage,
            LogExecutionFailure = true,
        };
        
        // 第4把剑
        _ = new GameDataEffectLaunchMissile(Effect.TenThousandSwordsLaunchMissile4)
        {
            Name = "万剑诀第4剑",
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Missile.TenThousandSwordsMissile,
            LaunchHeight = static (_) => 150,
            TargetHeight = static (_) => 80,
            Speed = static (_) => 2000f,
            CompleteEffect = Effect.TenThousandSwordsDamage,
            // LaunchOffset 暂时移除，通过其他方式实现位置偏移
            DoImpactEntity = true,
            DoStaticCollision = true,
            ImpactMaxCount = static (_) => 1,
            ImpactSearchRadius = static (_) => 32f,
            ImpactSearchFilter = [new() { Excluded = [UnitState.Dead, UnitState.Invulnerable], Required = [UnitRelationship.Enemy] }],
            ImpactEffect = Effect.TenThousandSwordsDamage,
            LogExecutionFailure = true,
        };
        
        // 第5把剑
        _ = new GameDataEffectLaunchMissile(Effect.TenThousandSwordsLaunchMissile5)
        {
            Name = "万剑诀第5剑",
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Missile.TenThousandSwordsMissile,
            LaunchHeight = static (_) => 150,
            TargetHeight = static (_) => 80,
            Speed = static (_) => 2000f,
            CompleteEffect = Effect.TenThousandSwordsDamage,
            // LaunchOffset 暂时移除，通过其他方式实现位置偏移
            DoImpactEntity = true,
            DoStaticCollision = true,
            ImpactMaxCount = static (_) => 1,
            ImpactSearchRadius = static (_) => 32f,
            ImpactSearchFilter = [new() { Excluded = [UnitState.Dead, UnitState.Invulnerable], Required = [UnitRelationship.Enemy] }],
            ImpactEffect = Effect.TenThousandSwordsDamage,
            LogExecutionFailure = true,
        };
        
        // 第6把剑
        _ = new GameDataEffectLaunchMissile(Effect.TenThousandSwordsLaunchMissile6)
        {
            Name = "万剑诀第6剑",
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Missile.TenThousandSwordsMissile,
            LaunchHeight = static (_) => 150,
            TargetHeight = static (_) => 80,
            Speed = static (_) => 2000f,
            CompleteEffect = Effect.TenThousandSwordsDamage,
            // LaunchOffset 暂时移除，通过其他方式实现位置偏移
            DoImpactEntity = true,
            DoStaticCollision = true,
            ImpactMaxCount = static (_) => 1,
            ImpactSearchRadius = static (_) => 32f,
            ImpactSearchFilter = [new() { Excluded = [UnitState.Dead, UnitState.Invulnerable], Required = [UnitRelationship.Enemy] }],
            ImpactEffect = Effect.TenThousandSwordsDamage,
            LogExecutionFailure = true,
        };
        
        // 第7把剑
        _ = new GameDataEffectLaunchMissile(Effect.TenThousandSwordsLaunchMissile7)
        {
            Name = "万剑诀第7剑",
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Missile.TenThousandSwordsMissile,
            LaunchHeight = static (_) => 150,
            TargetHeight = static (_) => 80,
            Speed = static (_) => 2000f,
            CompleteEffect = Effect.TenThousandSwordsDamage,
            // LaunchOffset 暂时移除，通过其他方式实现位置偏移
            DoImpactEntity = true,
            DoStaticCollision = true,
            ImpactMaxCount = static (_) => 1,
            ImpactSearchRadius = static (_) => 32f,
            ImpactSearchFilter = [new() { Excluded = [UnitState.Dead, UnitState.Invulnerable], Required = [UnitRelationship.Enemy] }],
            ImpactEffect = Effect.TenThousandSwordsDamage,
            LogExecutionFailure = true,
        };
        
        // 第8把剑 - 最右侧
        _ = new GameDataEffectLaunchMissile(Effect.TenThousandSwordsLaunchMissile8)
        {
            Name = "万剑诀第8剑",
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Missile.TenThousandSwordsMissile,
            LaunchHeight = static (_) => 150,
            TargetHeight = static (_) => 80,
            Speed = static (_) => 2000f,
            CompleteEffect = Effect.TenThousandSwordsDamage,
            // LaunchOffset 暂时移除，通过其他方式实现位置偏移
            DoImpactEntity = true,
            DoStaticCollision = true,
            ImpactMaxCount = static (_) => 1,
            ImpactSearchRadius = static (_) => 32f,
            ImpactSearchFilter = [new() { Excluded = [UnitState.Dead, UnitState.Invulnerable], Required = [UnitRelationship.Enemy] }],
            ImpactEffect = Effect.TenThousandSwordsDamage,
            LogExecutionFailure = true,
        };

        // 万剑诀发射集合 - 同时发射8把剑
        _ = new GameDataEffectSet(Effect.TenThousandSwordsLaunchSet)
        {
            Name = "万剑诀8剑齐发",
            Effects = [
                new() { Link = Effect.TenThousandSwordsLaunchMissile1 }, // 第1把剑
                new() { Link = Effect.TenThousandSwordsLaunchMissile2 }, // 第2把剑
                new() { Link = Effect.TenThousandSwordsLaunchMissile3 }, // 第3把剑
                new() { Link = Effect.TenThousandSwordsLaunchMissile4 }, // 第4把剑
                new() { Link = Effect.TenThousandSwordsLaunchMissile5 }, // 第5把剑
                new() { Link = Effect.TenThousandSwordsLaunchMissile6 }, // 第6把剑
                new() { Link = Effect.TenThousandSwordsLaunchMissile7 }, // 第7把剑
                new() { Link = Effect.TenThousandSwordsLaunchMissile8 }, // 第8把剑
            ],
        };

        // 万剑诀延迟效果 - 0.5秒后发射所有剑
        _ = new GameDataEffectPersistDelay(Effect.TenThousandSwordsDelay)
        {
            Name = "万剑诀延迟发射",
            Amount = static (_) => TimeSpan.FromSeconds(0.5), // 延迟0.5秒
            CompleteEffect = Effect.TenThousandSwordsLaunchSet, // 延迟后执行8剑齐发
            LogExecutionFailure = true,
        };

        // 万剑诀完整效果集合
        _ = new GameDataEffectSet(Effect.TenThousandSwordsCompleteEffect)
        {
            Name = "万剑诀完整效果",
            Effects = [
                new() { Link = Effect.TenThousandSwordsLog }, // 先显示日志
                new() { Link = Effect.TenThousandSwordsDelay }, // 延迟0.5秒后发射8把剑
            ],
        };

        // ========== 御剑术效果 ==========
        // 御剑术伤害效果
        _ = new GameDataEffectDamage(Effect.SwordControlDamage)
        {
            Name = "御剑术飞剑伤害",
            Amount = static (context) => {
                // 获取施法者的最终攻击力
                if (context?.Caster is GameCore.EntitySystem.Unit caster)
                {
                    var attackDamage = caster.GetUnitPropertyFinal(UnitProperty.AttackDamage);
                    if (attackDamage.HasValue)
                    {
                        // 御剑术伤害 = 最终攻击力 × 120%（比剑气斩更强）
                        var damage = attackDamage.Value * 1.2;
                        Game.Logger.LogInformation($"🔍 御剑术: 攻击力{attackDamage.Value:F0} → 伤害{damage:F0}");
                        return damage;
                    }
                }
                
                // 默认伤害
                Game.Logger.LogWarning("⚠️ 御剑术使用默认伤害60");
                return 60;
            },
            Type = GameCore.ScopeData.DamageType.Physical,
            ActorArray = [Actor.SwordControlHitEffect], // 使用击中特效
            LogExecutionFailure = true,
        };

        // 御剑术日志效果
        _ = new GameDataEffectLog(Effect.SwordControlLog)
        {
            Name = "御剑术日志",
            Message = context => 
            {
                // 计算实际伤害值用于显示
                double damage = 60; // 默认伤害
                if (context?.Caster is GameCore.EntitySystem.Unit caster)
                {
                    var attackDamage = caster.GetUnitPropertyFinal(UnitProperty.AttackDamage);
                    if (attackDamage.HasValue)
                    {
                        damage = attackDamage.Value * 1.2;
                    }
                }
                return $"🗡️ 御剑术！飞剑破空而去，造成{damage:F0}点物理伤害（攻击力120%）";
            },
            Level = LogLevel.Information,
            LogExecutionFailure = true,
        };

        // 御剑术投射物发射效果
        _ = new GameDataEffectLaunchMissile(Effect.SwordControlLaunchMissile)
        {
            Name = "御剑术飞剑发射",
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Missile.SwordControlMissile,
            LaunchHeight = static (_) => 120, // 发射高度
            TargetHeight = static (_) => 80,  // 目标高度
            Speed = static (_) => 1800f, // 飞剑速度（中等速度）
            CompleteEffect = Effect.SwordControlDamage, // 撞击时执行伤害效果
            
            // 撞击配置 - 这是关键！
            DoImpactEntity = true,  // 允许撞击实体
            DoStaticCollision = true, // 允许静态碰撞
            ImpactMaxCount = static (_) => 1, // 最多撞击1个目标
            ImpactSearchRadius = static (_) => 32f, // 撞击搜索半径
            ImpactSearchFilter = [
                new()
                {
                    Excluded = [UnitState.Dead, UnitState.Invulnerable],
                    Required = [UnitRelationship.Enemy], // 只撞击敌人
                }
            ],
            ImpactEffect = Effect.SwordControlDamage, // 撞击时执行的效果
            
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ 仙剑技能效果配置完成!");
    }

    /// <summary>
    /// 初始化技能配置
    /// </summary>
    private static void InitializeAbilities()
    {
        Game.Logger.LogInformation("🎯 配置仙剑奇侠传技能...");

        // ========== 剑气斩 - 李逍遥基础攻击 ==========
        _ = new GameDataAbilityExecute(Ability.SwordSlash)
        {
            Name = "剑气斩",
            DisplayName = "剑气斩",
            Description = "⚔️ 发射高速剑气攻击敌人，造成攻击力50%的物理伤害。装备更强的武器可提升伤害！",
            // Icon = "GameEntry/ArtAsset/Icons/Skills/SwordSlash.svg"u8, // 添加技能图标
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.2), // 前摇0.2秒（更快响应）
                Channel = static (_) => TimeSpan.FromSeconds(0.1),  // 持续0.1秒（更快）
                Backswing = static (_) => TimeSpan.FromSeconds(0.3), // 后摇0.3秒
            },
            Cost = new()
            {
                Cooldown = Cooldown.SwordSlashCooldown,
                // 法力消耗：20点 - 暂时注释
            },
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { IsAttack = true },
            Effect = Effect.SwordSlashLaunchMissile, // 恢复原来能工作的投射物效果
            TargetType = AbilityTargetType.Unit, // 恢复单体目标
            Range = static (_) => 500, // 恢复原来的射程
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                {
                    Required = [UnitFilter.Unit], // 恢复原来的过滤器
                    Excluded = [UnitState.Dead, UnitState.Invulnerable, UnitFilter.Hero] // 恢复原来的排除条件
                }],
            },
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = GameEntry.ScopeData.TargetingIndicator.TestTargetingIndicator,
                CursorRadius = static (_) => 96, // 恢复原来的大小
                VectorLineWidth = static (_) => 64,
                VectorHighlightLimit = static (_) => 1 // 恢复原来的目标数
            },
            Animation = [Animation.SwordSlashAnim], // 添加施法动画
            ActorArray = [Actor.CastEffect], // 恢复原来的施法特效
            LogExecutionFailure = true,
        };

        // ========== 仙风云体术 - 李逍遥身法技能 ==========
        _ = new GameDataAbilityExecute(Ability.XianFengSpell)
        {
            Name = "仙风云体术",
            DisplayName = "仙风云体术",
            Description = "🌪️ 提升移动速度200点，持续8秒，如御风而行",
            // Icon = "GameEntry/ArtAsset/Icons/Skills/XianFengSpell.png"u8, // 添加技能图标 (PNG格式)
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.0),
                Channel = static (_) => TimeSpan.FromSeconds(0.1),
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            Cost = new()
            {
                Cooldown = Cooldown.XianFengSpellCooldown,
                // 法力消耗：50点 - 暂时注释
            },
            Effect = Effect.XianFengSpellCompleteEffect, // 添加效果！
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            TargetType = AbilityTargetType.None, // 自我释放技能
            LogExecutionFailure = true,
        };

        // ========== 万剑诀 - 李逍遥单体大招 ==========
        _ = new GameDataAbilityExecute(Ability.TenThousandSwords)
        {
            Name = "万剑诀",
            DisplayName = "万剑诀",
            Description = "⚔️ 万剑齐发！在面前生成8把剑，停留0.5秒后射向目标敌人，每把剑造成攻击力80%的物理伤害",
            // Icon = "GameEntry/ArtAsset/Icons/Skills/TenThousandSwords.svg"u8, // 添加技能图标
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.3), // 前摇0.3秒
                Channel = static (_) => TimeSpan.FromSeconds(1.0),  // 持续1秒（包含0.5秒停留时间）
                Backswing = static (_) => TimeSpan.FromSeconds(0.2), // 后摇0.2秒
            },
            Cost = new()
            {
                Cooldown = Cooldown.TenThousandSwordsCooldown,
                // 法力消耗：100点 - 暂时注释
            },
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { IsAttack = true },
            Effect = Effect.TenThousandSwordsCompleteEffect, // 使用完整效果集合（包含延迟）
            TargetType = AbilityTargetType.Unit, // 单体目标技能
            Range = static (_) => 600, // 技能范围
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                {
                    Required = [UnitFilter.Unit], // 需要是单位
                    Excluded = [UnitState.Dead, UnitState.Invulnerable, UnitFilter.Hero] // 排除死亡、无敌和英雄
                }],
            },
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = GameEntry.ScopeData.TargetingIndicator.TestTargetingIndicator,
                CursorRadius = static (_) => 96, // 目标指示器大小
                VectorLineWidth = static (_) => 64,
                VectorHighlightLimit = static (_) => 1 // 单体目标
            },
            Animation = [Animation.TenThousandSwordsAnim], // 添加施法动画
            ActorArray = [Actor.TenThousandSwordsEffect], // 添加施法特效
            LogExecutionFailure = true,
        };

        // ========== 御剑术 - 李逍遥远程攻击技能 ==========
        _ = new GameDataAbilityExecute(Ability.SwordControl)
        {
            Name = "御剑术",
            DisplayName = "御剑术",
            Description = "🗡️ 御剑飞行！发射飞剑攻击目标敌人，造成攻击力120%的物理伤害，飞剑飞行一段距离后自动销毁",
            // Icon = "GameEntry/ArtAsset/Icons/Skills/SwordControl.svg"u8, // 添加技能图标
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.4), // 前摇0.4秒（蓄力感）
                Channel = static (_) => TimeSpan.FromSeconds(0.2),  // 持续0.2秒
                Backswing = static (_) => TimeSpan.FromSeconds(0.3), // 后摇0.3秒
            },
            Cost = new()
            {
                Cooldown = Cooldown.SwordControlCooldown,
                // 法力消耗：60点 - 暂时注释
            },
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { IsAttack = true },
            Effect = Effect.SwordControlLaunchMissile, // 使用投射物发射效果
            TargetType = AbilityTargetType.Unit, // 单体目标技能
            Range = static (_) => 600, // 技能范围
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                {
                    Required = [UnitFilter.Unit], // 需要是单位
                    Excluded = [UnitState.Dead, UnitState.Invulnerable, UnitFilter.Hero] // 排除死亡、无敌和英雄
                }],
            },
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = GameEntry.ScopeData.TargetingIndicator.TestTargetingIndicator,
                CursorRadius = static (_) => 96, // 目标指示器大小
                VectorLineWidth = static (_) => 64,
                VectorHighlightLimit = static (_) => 1 // 单体目标
            },
            Animation = [Animation.SwordControlAnim], // 添加施法动画
            ActorArray = [Actor.SwordControlCastEffect], // 添加施法特效
            LogExecutionFailure = true,
        };

        // ========== 治疗术 - 赵灵儿治疗技能 ==========
        _ = new GameDataAbilityExecute(Ability.HealingSpell)
        {
            Name = "治疗术",
            DisplayName = "治疗术",
            Description = "治疗目标或自己，恢复生命值",
            // Icon = "GameEntry/ArtAsset/Icons/Skills/HealingSpell.svg"u8, // 添加技能图标
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.4),
                Channel = static (_) => TimeSpan.FromSeconds(0.8),
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            Cost = new()
            {
                Cooldown = Cooldown.HealingSpellCooldown,
                // 法力消耗：40点 - 暂时注释
            },
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            Effect = Effect.HealingEffect,
            TargetType = AbilityTargetType.Unit,
            Range = static (_) => 400,
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                {
                    Required = [UnitRelationship.Alliance],
                    Excluded = [UnitState.Dead]
                }],
            },
            LogExecutionFailure = true,
        };

        // ========== 雷系法术 - 赵灵儿攻击法术 ==========
        _ = new GameDataAbilityExecute(Ability.LightningSpell)
        {
            Name = "雷系法术",
            DisplayName = "掌心雷",
            Description = "召唤雷电攻击敌人",
            // Icon = "GameEntry/ArtAsset/Icons/Skills/LightningSpell.svg"u8, // 添加技能图标
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.6),
                Channel = static (_) => TimeSpan.FromSeconds(0.4),
                Backswing = static (_) => TimeSpan.FromSeconds(0.2),
            },
            Cost = new()
            {
                Cooldown = Cooldown.LightningSpellCooldown,
                // 法力消耗：60点 - 暂时注释
            },
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            Effect = Effect.LightningDamage,
            TargetType = AbilityTargetType.Unit,
            Range = static (_) => 500,
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                {
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }],
            },
            LogExecutionFailure = true,
        };

        // ========== 水系法术 - 赵灵儿控制法术 ==========
        _ = new GameDataAbilityExecute(Ability.WaterSpell)
        {
            Name = "水系法术",
            DisplayName = "流水剑",
            Description = "操控水流攻击敌人",
            // Icon = "GameEntry/ArtAsset/Icons/Skills/WaterSpell.svg"u8, // 添加技能图标
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.7),
                Channel = static (_) => TimeSpan.FromSeconds(0.5),
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            Cost = new()
            {
                Cooldown = Cooldown.WaterSpellCooldown,
                // 法力消耗：70点 - 暂时注释
            },
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            Effect = Effect.WaterDamage,
            TargetType = AbilityTargetType.Unit,
            Range = static (_) => 450,
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                {
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }],
            },
            LogExecutionFailure = true,
        };

        // ========== 剑法连击 - 林月如连续攻击 ==========
        _ = new GameDataAbilityExecute(Ability.SwordCombo)
        {
            Name = "剑法连击",
            DisplayName = "百花错拳",
            Description = "连续多次攻击敌人",
            // Icon = "GameEntry/ArtAsset/Icons/Skills/SwordCombo.svg"u8, // 添加技能图标
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.2),
                Channel = static (_) => TimeSpan.FromSeconds(1.0), // 连击需要较长时间
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            Cost = new()
            {
                Cooldown = Cooldown.SwordComboCooldown,
                // 法力消耗：30点 - 暂时注释
            },
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { IsAttack = true },
            Effect = Effect.SwordComboDamage,
            TargetType = AbilityTargetType.Unit,
            Range = static (_) => 200,
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                {
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }],
            },
            LogExecutionFailure = true,
        };

        // ========== 疾风剑 - 林月如高速攻击 ==========
        _ = new GameDataAbilityExecute(Ability.QuickStrike)
        {
            Name = "疾风剑",
            DisplayName = "疾风剑",
            Description = "快速突进攻击敌人",
            // Icon = "GameEntry/ArtAsset/Icons/Skills/QuickStrike.svg"u8, // 添加技能图标
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.1),
                Channel = static (_) => TimeSpan.FromSeconds(0.3),
                Backswing = static (_) => TimeSpan.FromSeconds(0.2),
            },
            Cost = new()
            {
                Cooldown = Cooldown.QuickStrikeCooldown,
                // 法力消耗：25点 - 暂时注释
            },
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { IsAttack = true },
            Effect = Effect.QuickStrikeDamage,
            TargetType = AbilityTargetType.Unit,
            Range = static (_) => 250,
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                {
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }],
            },
            LogExecutionFailure = true,
        };

        // ========== 毒攻击 - 蛇妖技能 ==========
        _ = new GameDataAbilityExecute(Ability.PoisonAttack)
        {
            Name = "毒攻击",
            DisplayName = "毒牙",
            Description = "释放毒素攻击敌人",
            // Icon = "GameEntry/ArtAsset/Icons/Skills/PoisonAttack.svg"u8, // 添加技能图标
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.4),
                Channel = static (_) => TimeSpan.FromSeconds(0.2),
                Backswing = static (_) => TimeSpan.FromSeconds(0.3),
            },
            Cost = new()
            {
                Cooldown = Cooldown.PoisonAttackCooldown,
                // 法力消耗：20点 - 暂时注释
            },
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { IsAttack = true },
            Effect = Effect.PoisonDamage,
            TargetType = AbilityTargetType.Unit,
            Range = static (_) => 150,
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                {
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }],
            },
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ 仙剑奇侠传技能配置完成!");
        Game.Logger.LogInformation("   - 李逍遥: 剑气斩, 仙风云体术, 万剑诀, 御剑术");
        Game.Logger.LogInformation("   - 赵灵儿: 治疗术, 掌心雷, 流水剑");
        Game.Logger.LogInformation("   - 林月如: 百花错拳, 疾风剑");
        Game.Logger.LogInformation("   - 蛇妖: 毒牙攻击");
    }

    /// <summary>
    /// 初始化Buff配置
    /// </summary>
    private static void InitializeBuffs()
    {
        Game.Logger.LogInformation("💨 配置仙剑技能Buff效果...");

        // ========== 仙风云体术Buff配置 ==========
        _ = new GameDataBuff(Buff.XianFengSpeedBuff)
        {
            Name = "仙风云体术",
            DisplayName = "仙风云体术",
            Description = "移动速度大幅提升，如御风而行",
            Duration = static (_) => TimeSpan.FromSeconds(8), // 持续8秒
            // Icon = "image/buff/speed_buff.png",
            Polarity = GameCore.BuffSystem.Data.Enum.BuffPolarity.Positive, // 正面效果
            
            // 叠加配置 - 不能叠加
            StackStart = 1,
            StackMax = static (_) => 1,
            InstanceMax = 1,
            
            // 移动速度增益 - 增加200点移动速度
            Modifications = [
                new()
                {
                    Property = UnitProperty.MoveSpeed,
                    Value = static (_) => 200 // 增加200点移动速度，从380提升到580
                }
            ],
            
            // 持续特效 - Buff期间显示的视觉效果
            ActorArray = [Actor.XianFengBuffEffect], // 添加持续特效
            
            SyncType = EngineInterface.BaseType.SyncType.Sight
        };


        Game.Logger.LogInformation("✅ 仙剑技能Buff效果配置完成");
    }

    /// <summary>
    /// 初始化动画配置
    /// </summary>
    private static void InitializeAnimations()
    {
        Game.Logger.LogInformation("🎬 配置仙剑技能动画...");

        // ========== 剑气斩施法动画 ==========
        _ = new GameDataAnimationSimple(Animation.SwordSlashAnim)
        {
            Name = "剑气斩施法动画",
            File = "skill1"u8, // 使用skill1别名，对应attack_02动画
            IsLooping = false,
        };

        // ========== 万剑诀施法动画 ==========
        _ = new GameDataAnimationSimple(Animation.TenThousandSwordsAnim)
        {
            Name = "万剑诀施法动画",
            File = "skill2"u8, // 使用skill2别名，对应attack_03动画
            IsLooping = false,
        };

        // ========== 御剑术施法动画 ==========
        _ = new GameDataAnimationSimple(Animation.SwordControlAnim)
        {
            Name = "御剑术施法动画",
            File = "skill3"u8, // 使用skill3别名，对应attack_04动画
            IsLooping = false,
        };

        Game.Logger.LogInformation("✅ 仙剑技能动画配置完成");
    }

    /// <summary>
    /// 初始化粒子效果配置
    /// </summary>
    private static void InitializeParticles()
    {
        Game.Logger.LogInformation("✨ 配置仙剑技能粒子效果...");

        // ========== 剑气发射特效粒子 ==========
        _ = new GameDataParticle(Particle.SwordQiParticle)
        {
            Name = "剑气特效",
            Asset = "effect/effect_new1/effect_knief/eff_knief_002/particle.effect"u8, // 剑光特效
        };

        // ========== 剑气击中特效粒子 ==========
        _ = new GameDataParticle(Particle.SwordQiHitParticle)
        {
            Name = "剑气击中特效",
            Asset = "effect/effect_new/effect_hit/eff_hit_29/particle.effect"u8, // 击中特效
        };

        // ========== 施法光效粒子 ==========
        _ = new GameDataParticle(Particle.CastEffectParticle)
        {
            Name = "施法光效",
            Asset = "effect/effect_new1/effect_knief/eff_knief_001/particle.effect"u8, // 施法光芒
        };

        // ========== 仙风云体术持续特效粒子 ==========
        _ = new GameDataParticle(Particle.XianFengBuffParticle)
        {
            Name = "仙风云体术持续特效",
            Asset = "effect/effect_new/effect_buff/eff_buff_04/particle.effect"u8, // 你指定的Buff特效
            Radius = 128f, // 放大特效半径，让效果更显眼
        };

        // ========== 万剑诀粒子效果 ==========
        // 万剑诀剑气粒子（使用和御剑术相同的特效）
        _ = new GameDataParticle(Particle.TenThousandSwordsParticle)
        {
            Name = "万剑诀剑气特效",
            Asset = "effect/effect_new/effect_mofa/effect_qingzhujian_01/particle.effect"u8, // 使用和御剑术相同的特效
        };

        // 万剑诀击中粒子
        _ = new GameDataParticle(Particle.TenThousandSwordsHitParticle)
        {
            Name = "万剑诀击中特效",
            Asset = "effect/effect_new/effect_hit/eff_hit_29/particle.effect"u8, // 使用标准击中特效
        };

        // ========== 御剑术粒子效果 ==========
        // 御剑术弹道粒子
        _ = new GameDataParticle(Particle.SwordControlMissileParticle)
        {
            Name = "御剑术弹道特效",
            Asset = "effect/effect_new/effect_mofa/effect_qingzhujian_01/particle.effect"u8, // 使用指定的特效
            Radius = 96f, // 增加弹道特效半径
        };

        // 御剑术击中粒子
        _ = new GameDataParticle(Particle.SwordControlHitParticle)
        {
            Name = "御剑术击中特效",
            Asset = "effect/effect_new/effect_hit/eff_hit_29/particle.effect"u8, // 击中特效
            Radius = 80f, // 增加击中特效半径
        };

        // 御剑术施法粒子
        _ = new GameDataParticle(Particle.SwordControlCastParticle)
        {
            Name = "御剑术施法特效",
            Asset = "effect/effect_new1/effect_knief/eff_knief_001/particle.effect"u8, // 施法光芒
        };

        Game.Logger.LogInformation("✅ 仙剑技能粒子效果配置完成");
    }

    /// <summary>
    /// 初始化Actor配置
    /// </summary>
    private static void InitializeActors()
    {
        Game.Logger.LogInformation("🎭 配置仙剑技能Actor...");

        // ========== 剑气发射特效Actor ==========
        _ = new GameDataActorParticle(Actor.SwordQiEffect)
        {
            Name = "剑气发射特效Actor",
            AutoPlay = true,
            Particle = Particle.SwordQiParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 剑气击中特效Actor ==========
        _ = new GameDataActorParticle(Actor.SwordQiHitEffect)
        {
            Name = "剑气击中特效Actor",
            AutoPlay = true,
            Particle = Particle.SwordQiHitParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 施法光效Actor ==========
        _ = new GameDataActorParticle(Actor.CastEffect)
        {
            Name = "施法光效Actor",
            AutoPlay = true,
            Particle = Particle.CastEffectParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 仙风云体术持续特效Actor ==========
        _ = new GameDataActorParticle(Actor.XianFengBuffEffect)
        {
            Name = "仙风云体术持续特效Actor",
            AutoPlay = true,
            Particle = Particle.XianFengBuffParticle,
            KillOnFinish = false, // 持续特效，不自动结束
            ForceOneShot = false, // 循环播放
            Offset = new Vector3(0, 0, 0), // 在角色中心位置
            Scale = 1.5f, // 放大1.5倍，让特效更显眼
        };

        // ========== 万剑诀Actor ==========
        // 万剑诀剑气Actor
        _ = new GameDataActorParticle(Actor.TenThousandSwordsEffect)
        {
            Name = "万剑诀剑气Actor",
            AutoPlay = true,
            Particle = Particle.TenThousandSwordsParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // 万剑诀击中Actor
        _ = new GameDataActorParticle(Actor.TenThousandSwordsHitEffect)
        {
            Name = "万剑诀击中Actor",
            AutoPlay = true,
            Particle = Particle.TenThousandSwordsHitParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 御剑术Actor ==========
        // 御剑术击中Actor
        _ = new GameDataActorParticle(Actor.SwordControlHitEffect)
        {
            Name = "御剑术击中Actor",
            AutoPlay = true,
            Particle = Particle.SwordControlHitParticle,
            KillOnFinish = true, // 击中后自动结束
            ForceOneShot = true, // 单次播放
        };

        // 御剑术施法Actor
        _ = new GameDataActorParticle(Actor.SwordControlCastEffect)
        {
            Name = "御剑术施法Actor",
            AutoPlay = true,
            Particle = Particle.SwordControlCastParticle,
            KillOnFinish = true, // 施法后自动结束
            ForceOneShot = true, // 单次播放
        };

        Game.Logger.LogInformation("✅ 仙剑技能Actor配置完成");
    }

    /// <summary>
    /// 初始化投射物单位配置
    /// </summary>
    private static void InitializeMissiles()
    {
        Game.Logger.LogInformation("🚀 配置仙剑投射物单位...");

        // ========== 剑气投射物单位 ==========
        _ = new GameDataUnit(Missile.SwordQiMissile)
        {
            Name = "剑气",
            Filter = [UnitFilter.Missile],
            State = [UnitState.Invulnerable],
            CollisionRadius = 16,
            AttackableRadius = 32,
            Particle = "effect/effect_new1/effect_knief/eff_knief_003/particle.effect"u8, // 剑气弹道特效
            UpdateFlags = new()
            {
                AllowMover = true,
            },
        };

        // ========== 万剑诀剑气投射物单位 ==========
        _ = new GameDataUnit(Missile.TenThousandSwordsMissile)
        {
            Name = "万剑诀剑气",
            Filter = [UnitFilter.Missile],
            State = [UnitState.Invulnerable],
            CollisionRadius = 16, // 和御剑术相同的碰撞半径
            AttackableRadius = 32,
            Particle = "effect/effect_new/effect_mofa/effect_qingzhujian_01/particle.effect"u8, // 使用和御剑术相同的特效
            UpdateFlags = new()
            {
                AllowMover = true,
            },
        };

        // ========== 御剑术飞剑投射物单位 ==========
        _ = new GameDataUnit(Missile.SwordControlMissile)
        {
            Name = "御剑术飞剑",
            Filter = [UnitFilter.Missile],
            State = [UnitState.Invulnerable],
            CollisionRadius = 16, // 飞剑碰撞半径
            AttackableRadius = 32,
            Particle = "effect/effect_new/effect_mofa/effect_qingzhujian_01/particle.effect"u8, // 使用你指定的特效作为弹道
            UpdateFlags = new()
            {
                AllowMover = true,
            },
        };

        Game.Logger.LogInformation("✅ 仙剑投射物单位配置完成");
    }
}
