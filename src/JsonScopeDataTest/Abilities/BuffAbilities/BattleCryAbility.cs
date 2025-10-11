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
using GameCore.Struct;
using GameCore.ModelAnimation.Data;
using GameData;
using static GameCore.ScopeData;

namespace GameEntry.JsonScopeDataTest.Abilities.BuffAbilities;

/// <summary>
/// 战吼技能 - 一次性施放给攻击力+20加成持续15秒
/// </summary>
public class BattleCryAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> BattleCry = new("BattleCry"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> BattleCryBuffApply = new("BattleCryBuffApply"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> BattleCryBuff = new("BattleCryBuff"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> BattleCryParticle = new("BattleCryParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> BattleCryBuffParticle = new("BattleCryBuffParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> BattleCryEffect = new("BattleCryEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> BattleCryBuffEffect = new("BattleCryBuffEffect"u8);
    #endregion

    #region 声音定义
    public static readonly GameLink<GameDataSound, GameDataSound> BattleCrySound = new("BattleCrySound"u8);
    public static readonly GameLink<GameDataActor, GameDataActorSound> BattleCrySoundActor = new("BattleCrySoundActor"u8);
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

        Game.Logger.LogInformation("📢 Initializing Battle Cry Ability...");

        // ========== 粒子效果配置 ==========
        // 施放时的特效（一次性）
        _ = new GameDataParticle(BattleCryParticle)
        {
            Asset = "effect/eff_xujian/effect_zhanshi_03/particle.effect"u8,
            AssetLayerScale = 1.0f,
            Radius = 64f,
        };

        _ = new GameDataActorParticle(BattleCryEffect)
        {
            AutoPlay = true,
            Particle = BattleCryParticle,
            KillOnFinish = true, // 播放完毕后销毁
            ForceOneShot = true, // 一次性播放
        };

        // Buff持续期间的特效
        _ = new GameDataParticle(BattleCryBuffParticle)
        {
            Asset = "effect/effect_new/effect_buff/eff_buff_02/particle.effect"u8,
            AssetLayerScale = 0.8f,
            Radius = 32f,
        };

        _ = new GameDataActorParticle(BattleCryBuffEffect)
        {
            AutoPlay = true,
            Particle = BattleCryBuffParticle,
            KillOnFinish = false, // 持续显示
            ForceOneShot = false, // 循环播放
        };

        // ========== 声音效果配置 ==========
        _ = new GameDataSound(BattleCrySound)
        {
            Asset = new Sound("sound/a2_sfx/fight/magic/sfx_magic_cast_imprisoned_01.ogg"u8),
        };

        _ = new GameDataActorSound(BattleCrySoundActor)
        {
            AutoPlay = false, // 手动触发
        };

        // ========== Buff配置 ==========
        _ = new GameDataBuff(BattleCryBuff)
        {
            Name = "战吼加成",
            Duration = static (_) => TimeSpan.FromSeconds(15.0),
            
            // 攻击力+20的固定加成
            Modifications = [
                new() 
                { 
                    Property = UnitProperty.AttackDamage,
                    SubType = PropertySubType.Base,
                    Value = static (_) => 20.0 // 固定+20攻击力
                }
            ],
            
            // Buff期间的视觉效果
            ActorArray = [BattleCryBuffEffect],
        };

        // ========== 效果配置 ==========
        _ = new GameDataEffectBuffAdd(BattleCryBuffApply)
        {
            Name = "战吼Buff施加",
            BuffLink = BattleCryBuff,
            TargetLocation = new() { Value = TargetLocation.Caster },
            ActorArray = [BattleCryEffect], // 施放时的特效
            LogExecutionFailure = true,
        };

        // ========== 冷却配置 ==========
        var battleCryCooldown = new GameLink<GameDataCooldown, GameDataCooldownActive>("BattleCryCooldown"u8);
        _ = new GameDataCooldownActive(battleCryCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(20.0),
        };

        // ========== 动画配置 ==========
        var battleCryAnim = new GameLink<GameDataAnimation, GameDataAnimationSimple>("BattleCryAnim"u8);
        _ = new GameDataAnimationSimple(battleCryAnim)
        {
            Name = "战吼动画",
            File = "anim/human/sword_anim/DaJianZhanShi/skill_03.ani"u8,
            IsLooping = false,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(BattleCry)
        {
            Name = "战吼",
            DisplayName = "战吼",
            Description = "发出震撼的战吼，提升攻击力+20，持续15秒",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.39849),
                Channel = static (_) => TimeSpan.FromSeconds(0.30957),
                Backswing = static (_) => TimeSpan.FromSeconds(0.45860),
            },
            
            Cost = new()
            {
                Cooldown = battleCryCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = BattleCryBuffApply,
            TargetType = AbilityTargetType.None,
            
            ActorArray = [BattleCrySoundActor],
            Animation = [battleCryAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Battle Cry Ability initialized successfully!");
    }
}
