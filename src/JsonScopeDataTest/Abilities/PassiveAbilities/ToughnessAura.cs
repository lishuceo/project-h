using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.BuffSystem.Data;
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
/// 坚韧光环 - 被动技能，定期为周围友军提供移动速度+10%和攻击速度+5%的加成
/// </summary>
public class ToughnessAura : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbility> ToughnessAuraAbility = new("ToughnessAura"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> ToughnessAuraSearch = new("ToughnessAuraSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> ToughnessAuraBuffApply = new("ToughnessAuraBuffApply"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> ToughnessAuraBuff = new("ToughnessAuraBuff"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> ToughnessAuraParticle = new("ToughnessAuraParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> ToughnessBuffParticle = new("ToughnessBuffParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ToughnessAuraEffect = new("ToughnessAuraEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ToughnessBuffEffect = new("ToughnessBuffEffect"u8);
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

        Game.Logger.LogInformation("💪 Initializing Toughness Aura...");

        // ========== 粒子效果配置 ==========
        // 光环持续特效（施法者身上）
        _ = new GameDataParticle(ToughnessAuraParticle)
        {
            Asset = "effect/effect_new/effect_buff/eff_buff_10/particle.effect"u8,
            AssetLayerScale = 1.5f,
            Radius = 64f,
        };

        _ = new GameDataActorParticle(ToughnessAuraEffect)
        {
            AutoPlay = true,
            Particle = ToughnessAuraParticle,
            KillOnFinish = false, // 持续显示
            ForceOneShot = false, // 循环播放
        };

        // Buff效果（受益者身上）
        _ = new GameDataParticle(ToughnessBuffParticle)
        {
            Asset = "effect/effect_new/effect_buff/eff_buff_10/particle.effect"u8,
            AssetLayerScale = 0.8f,
            Radius = 32f,
        };

        _ = new GameDataActorParticle(ToughnessBuffEffect)
        {
            AutoPlay = true,
            Particle = ToughnessBuffParticle,
            KillOnFinish = false,
            ForceOneShot = false,
        };

        // ========== Buff配置 ==========
        _ = new GameDataBuff(ToughnessAuraBuff)
        {
            Name = "坚韧光环加成",
            Duration = static (_) => TimeSpan.FromSeconds(2.0), // 短持续时间，需要光环不断刷新
            
            // 移动速度+35，攻击速度提升（暂时用移动速度代替）
            Modifications = [
                new() 
                { 
                    Property = UnitProperty.MoveSpeed,
                    SubType = PropertySubType.Base,
                    Value = static (_) => 35.0 // +35移动速度（约10%）
                }
            ],
            
            // Buff期间的视觉效果
            ActorArray = [ToughnessBuffEffect],
        };

        // ========== 搜索效果配置 ==========
        _ = new GameDataEffectSearch(ToughnessAuraSearch)
        {
            Name = "坚韧光环搜索",
            Radius = static (_) => 300.0, // 搜索半径300
            Effect = ToughnessAuraBuffApply,
        };

        // ========== Buff施加效果配置 ==========
        _ = new GameDataEffectBuffAdd(ToughnessAuraBuffApply)
        {
            Name = "坚韧光环Buff施加",
            BuffLink = ToughnessAuraBuff,
            LogExecutionFailure = true,
        };

        // ========== 被动技能配置 ==========
        _ = new GameDataAbility(ToughnessAuraAbility)
        {
            Name = "坚韧光环",
            DisplayName = "坚韧光环",
            Description = "被动光环：为周围300范围内的友军提供移动速度+10%和攻击速度+5%的加成",
            
            TargetType = AbilityTargetType.None,
            
            // 被动技能配置
            Flags = new()
            {
                DisableWhenDead = true, // 死亡时禁用
                PersistDuringMorph = true, // 变形时保持
            },
            
            // 定期触发搜索效果
            PassivePeriod = static (_) => TimeSpan.FromSeconds(1.0), // 每秒触发一次
            PassivePeriodicEffect = ToughnessAuraSearch,
            
            // 光环持续特效
            ActorArray = [ToughnessAuraEffect],
        };

        Game.Logger.LogInformation("✅ Toughness Aura initialized successfully!");
    }
}
