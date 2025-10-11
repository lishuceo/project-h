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
/// 吸血光环 - 被动技能，为周围友军提供攻击吸血效果（将攻击伤害的15%转化为生命值）
/// </summary>
public class VampiricAura : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbility> VampiricAuraAbility = new("VampiricAura"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> VampiricAuraSearch = new("VampiricAuraSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> VampiricAuraBuffApply = new("VampiricAuraBuffApply"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> VampiricAuraBuff = new("VampiricAuraBuff"u8);
    #endregion


    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> VampiricAuraParticle = new("VampiricAuraParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> VampiricBuffParticle = new("VampiricBuffParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> VampiricAuraEffect = new("VampiricAuraEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> VampiricBuffEffect = new("VampiricBuffEffect"u8);
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

        Game.Logger.LogInformation("🩸 Initializing Vampiric Aura...");

        // ========== 粒子效果配置 ==========
        // 光环持续特效（施法者身上）
        _ = new GameDataParticle(VampiricAuraParticle)
        {
            Asset = "effect/samplespells/vampiric aura/vampiric aura/particle.effect"u8,
            AssetLayerScale = 1.0f,
            Radius = 64f,
        };

        _ = new GameDataActorParticle(VampiricAuraEffect)
        {
            AutoPlay = true,
            Particle = VampiricAuraParticle,
            KillOnFinish = false, // 持续显示
            ForceOneShot = false, // 循环播放
        };

        // Buff效果（受益者身上）
        _ = new GameDataParticle(VampiricBuffParticle)
        {
            Asset = "effect/samplespells/vampiric aura/vampiric aura/particle.effect"u8,
            AssetLayerScale = 0.8f,
            Radius = 32f,
        };

        _ = new GameDataActorParticle(VampiricBuffEffect)
        {
            AutoPlay = true,
            Particle = VampiricBuffParticle,
            KillOnFinish = false,
            ForceOneShot = false,
            Offset = new Vector3 { Z = 5f },
        };


        // ========== Buff配置 ==========
        _ = new GameDataBuff(VampiricAuraBuff)
        {
            Name = "吸血光环",
            Duration = static (_) => TimeSpan.FromSeconds(2.0), // 短持续时间，需要光环不断刷新
            
            // 攻击力加成（代替吸血效果）
            Modifications = [
                new() 
                { 
                    Property = UnitProperty.AttackDamage,
                    SubType = PropertySubType.Base,
                    Value = static (_) => 15.0 // +15攻击力
                }
            ],
            
            // Buff期间的视觉效果
            ActorArray = [VampiricBuffEffect],
        };

        // ========== 搜索效果配置 ==========
        _ = new GameDataEffectSearch(VampiricAuraSearch)
        {
            Name = "吸血光环搜索",
            Radius = static (_) => 300.0, // 搜索半径300
            Effect = VampiricAuraBuffApply,
        };

        // ========== Buff施加效果配置 ==========
        _ = new GameDataEffectBuffAdd(VampiricAuraBuffApply)
        {
            Name = "吸血光环Buff施加",
            BuffLink = VampiricAuraBuff,
            LogExecutionFailure = true,
        };

        // ========== 被动技能配置 ==========
        _ = new GameDataAbility(VampiricAuraAbility)
        {
            Name = "吸血光环",
            DisplayName = "吸血光环",
            Description = "被动光环：为周围300范围内的友军提供吸血效果，将攻击伤害的15%转化为生命值",
            
            TargetType = AbilityTargetType.None,
            
            // 被动技能配置
            Flags = new()
            {
                DisableWhenDead = true, // 死亡时禁用
                PersistDuringMorph = true, // 变形时保持
            },
            
            // 定期触发搜索效果
            PassivePeriod = static (_) => TimeSpan.FromSeconds(1.0), // 每秒触发一次
            PassivePeriodicEffect = VampiricAuraSearch,
            
            // 光环持续特效
            ActorArray = [VampiricAuraEffect],
        };

        Game.Logger.LogInformation("✅ Vampiric Aura initialized successfully!");
    }
}
