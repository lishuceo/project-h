using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.BuffSystem.Data;
using GameCore.Behavior;
using GameCore.Data;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.ResourceType.Data;
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.Struct;
using GameData;
using static GameCore.ScopeData;

namespace GameEntry.JsonScopeDataTest.Abilities.PassiveAbilities;

/// <summary>
/// 反噬（被动）- 被动技能，当受到物理伤害时，对攻击者造成反击伤害并减少其魔法值
/// </summary>
public class Feedback : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbility> FeedbackAbility = new("Feedback"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> FeedbackCounterAttack = new("FeedbackCounterAttack"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> FeedbackDebuffApply = new("FeedbackDebuffApply"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> FeedbackDamage = new("FeedbackDamage"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> FeedbackDebuff = new("FeedbackDebuff"u8);
    #endregion

    #region 响应定义
    public static readonly GameLink<GameDataResponse, GameDataResponseDamage> FeedbackDamageResponse = new("FeedbackDamageResponse"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> FeedbackCounterParticle = new("FeedbackCounterParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> FeedbackCounterEffect = new("FeedbackCounterEffect"u8);
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

        Game.Logger.LogInformation("⚡ Initializing Feedback...");

        // ========== 粒子效果配置 ==========
        // 反击特效
        _ = new GameDataParticle(FeedbackCounterParticle)
        {
            Asset = "effect/eff_tongyong/ps_shouji_1/particle.effect"u8,
            AssetLayerScale = 1.0f,
            Radius = 64f,
        };

        _ = new GameDataActorParticle(FeedbackCounterEffect)
        {
            AutoPlay = true,
            Particle = FeedbackCounterParticle,
            KillOnFinish = true, // 播放完毕后销毁
            ForceOneShot = true, // 一次性播放
        };

        // ========== 反击伤害效果配置 ==========
        _ = new GameDataEffectDamage(FeedbackDamage)
        {
            Name = "反噬伤害",
            Amount = static (_) => 20.0, // 固定20点伤害
        };

        // ========== 魔法值减少Buff配置 ==========
        _ = new GameDataBuff(FeedbackDebuff)
        {
            Name = "反噬魔法消耗",
            Duration = static (_) => TimeSpan.FromSeconds(30.0), // 持续30秒
            
            // 攻击力-20（代替魔法值减少）
            Modifications = [
                new() 
                { 
                    Property = UnitProperty.AttackDamage,
                    SubType = PropertySubType.Base,
                    Value = static (_) => -20.0 // -20攻击力
                }
            ],
        };

        // ========== 反击效果集合配置 ==========
        _ = new GameDataEffectSet(FeedbackCounterAttack)
        {
            Name = "反噬反击",
            ActorArray = [FeedbackCounterEffect],
            Effects = [
                new() { Link = FeedbackDebuffApply },
                new() { Link = FeedbackDamage }
            ],
        };

        // ========== Debuff施加效果配置 ==========
        _ = new GameDataEffectBuffAdd(FeedbackDebuffApply)
        {
            Name = "反噬Debuff施加",
            BuffLink = FeedbackDebuff,
            LogExecutionFailure = true,
        };

        // ========== 伤害响应配置 ==========
        _ = new GameDataResponseDamage(FeedbackDamageResponse)
        {
            Name = "反噬伤害响应",
            Location = ResponseLocation.Defender, // 受击者触发
            
            // 只响应物理伤害
            DamageType = new()
            {
                [DamageType.Physical] = true,
            },
            
            ResponseEffect = FeedbackCounterAttack,
        };

        // ========== 被动技能配置 ==========
        _ = new GameDataAbility(FeedbackAbility)
        {
            Name = "反噬（被动）",
            DisplayName = "反噬",
            Description = "被动技能：受到物理伤害时，对攻击者造成20点魔法伤害并减少其20点攻击力，持续30秒",
            
            TargetType = AbilityTargetType.None,
            
            // 被动技能配置
            Flags = new()
            {
                DisableWhenDead = true, // 死亡时禁用
                PersistDuringMorph = true, // 变形时保持
                Hidden = false, // 显示在技能栏
            },
            
            // 直接绑定伤害响应到技能上！
            Responses = [FeedbackDamageResponse],
            
            SyncType = SyncType.SelfOrSight,
        };

        Game.Logger.LogInformation("✅ Feedback initialized successfully!");
    }
}
