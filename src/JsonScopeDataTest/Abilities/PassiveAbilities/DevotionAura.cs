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
/// 虔诚光环 - 被动技能，定期为周围友军提供护甲+2的加成
/// </summary>
public class DevotionAura : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbility> DevotionAuraAbility = new("DevotionAura"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> DevotionAuraSearch = new("DevotionAuraSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> DevotionAuraBuffApply = new("DevotionAuraBuffApply"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> DevotionAuraBuff = new("DevotionAuraBuff"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> DevotionAuraParticle = new("DevotionAuraParticle"u8);
    public static readonly GameLink<GameDataParticle, GameDataParticle> DevotionBuffParticle = new("DevotionBuffParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> DevotionAuraEffect = new("DevotionAuraEffect"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> DevotionBuffEffect = new("DevotionBuffEffect"u8);
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

        Game.Logger.LogInformation("🛡️ Initializing Devotion Aura...");

        // ========== 粒子效果配置 ==========
        // 光环持续特效（施法者身上）
        _ = new GameDataParticle(DevotionAuraParticle)
        {
            Asset = "effect/samplespells/devotion/devotionaura/particle.effect"u8,
            AssetLayerScale = 1.0f,
            Radius = 64f,
        };

        _ = new GameDataActorParticle(DevotionAuraEffect)
        {
            AutoPlay = true,
            Particle = DevotionAuraParticle,
            KillOnFinish = false, // 持续显示
            ForceOneShot = false, // 循环播放
        };

        // Buff效果（受益者身上）
        _ = new GameDataParticle(DevotionBuffParticle)
        {
            Asset = "effect/samplespells/devotion/devotionaura_target/particle.effect"u8,
            AssetLayerScale = 1.0f,
            Radius = 32f,
        };

        _ = new GameDataActorParticle(DevotionBuffEffect)
        {
            AutoPlay = true,
            Particle = DevotionBuffParticle,
            KillOnFinish = false,
            ForceOneShot = false,
            Offset = new Vector3 { Z = 360f },
        };

        // ========== Buff配置 ==========
        _ = new GameDataBuff(DevotionAuraBuff)
        {
            Name = "虔诚光环",
            Duration = static (_) => TimeSpan.FromSeconds(2.0), // 短持续时间，需要光环不断刷新
            
            // 护甲+2
            Modifications = [
                new() 
                { 
                    Property = UnitProperty.Armor,
                    SubType = PropertySubType.Base,
                    Value = static (_) => 2.0 // +2护甲
                }
            ],
            
            // Buff期间的视觉效果
            ActorArray = [DevotionBuffEffect],
            
            // Buff设置
            InstanceMax = 1,
        };

        // ========== 搜索效果配置 ==========
        _ = new GameDataEffectSearch(DevotionAuraSearch)
        {
            Name = "虔诚光环搜索",
            SearchFilters = [new() {
                Required = [UnitFilter.Unit, UnitRelationship.Alliance], // 搜索单位类型的盟友 - 参考INI中SearchFilter = '盟友,单位;死亡'
                Excluded = [UnitState.Dead] // 排除死亡单位
            },
            new() {
                Required = [UnitFilter.Unit, UnitRelationship.Self], // 搜索单位类型的盟友 - 参考INI中SearchFilter = '盟友,单位;死亡'
                Excluded = [UnitState.Dead] // 排除死亡单位
            }],
            Radius = static (_) => 450.0, // 搜索半径450
            Effect = DevotionAuraBuffApply,
        };

        // ========== Buff施加效果配置 ==========
        _ = new GameDataEffectBuffAdd(DevotionAuraBuffApply)
        {
            Name = "虔诚光环Buff施加",
            BuffLink = DevotionAuraBuff,
            LogExecutionFailure = true,
        };

        // ========== 被动技能配置 ==========
        _ = new GameDataAbility(DevotionAuraAbility)
        {
            Name = "虔诚光环",
            DisplayName = "虔诚光环",
            Description = "被动光环：为周围450范围内的友军提供护甲+2的加成",
            
            TargetType = AbilityTargetType.None,
            
            // 被动技能配置
            Flags = new()
            {
                DisableWhenDead = true, // 死亡时禁用
                PersistDuringMorph = true, // 变形时保持
            },
            
            // 定期触发搜索效果
            PassivePeriod = static (_) => TimeSpan.FromSeconds(1.0), // 每秒触发一次
            PassivePeriodicEffect = DevotionAuraSearch,
            
            // 光环持续特效
            ActorArray = [DevotionAuraEffect],
        };

        Game.Logger.LogInformation("✅ Devotion Aura initialized successfully!");
    }
}
