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
/// 魔法侦测 - 被动技能，定期侦测周围的敌方隐身单位，使其显形
/// </summary>
public class MagicDetection : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbility> MagicDetectionAbility = new("MagicDetection"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> MagicDetectionSearch = new("MagicDetectionSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> MagicDetectionBuffApply = new("MagicDetectionBuffApply"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> MagicDetectionBuff = new("MagicDetectionBuff"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> MagicDetectionParticle = new("MagicDetectionParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> MagicDetectionEffect = new("MagicDetectionEffect"u8);
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

        Game.Logger.LogInformation("👁️ Initializing Magic Detection...");

        // ========== 粒子效果配置 ==========
        // 侦测范围指示特效（施法者身上）
        _ = new GameDataParticle(MagicDetectionParticle)
        {
            Asset = "effect/samplespells/magic  sentry/magic  sentry/particle.effect"u8,
            AssetLayerScale = 1.0f,
            Radius = 64f,
        };

        _ = new GameDataActorParticle(MagicDetectionEffect)
        {
            AutoPlay = true,
            Particle = MagicDetectionParticle,
            KillOnFinish = false, // 持续显示
            ForceOneShot = false, // 循环播放
            Offset = new Vector3 { Z = 200f },
        };

        // ========== Buff配置（显形效果）==========
        _ = new GameDataBuff(MagicDetectionBuff)
        {
            Name = "魔法侦测显形",
            Duration = static (_) => TimeSpan.FromSeconds(0.125), // 短暂显形
            RemoveStates = [GameCore.BaseType.UnitState.InvisibleToEnemy],
        };

        // ========== 搜索效果配置 ==========
        _ = new GameDataEffectSearch(MagicDetectionSearch)
        {
            Name = "魔法侦测搜索",
            Radius = static (_) => 200.0, // 搜索半径200
            Effect = MagicDetectionBuffApply,
        };

        // ========== Buff施加效果配置 ==========
        _ = new GameDataEffectBuffAdd(MagicDetectionBuffApply)
        {
            Name = "魔法侦测Buff施加",
            BuffLink = MagicDetectionBuff,
            LogExecutionFailure = true,
        };

        // ========== 被动技能配置 ==========
        _ = new GameDataAbility(MagicDetectionAbility)
        {
            Name = "魔法侦测（被动）",
            DisplayName = "魔法侦测",
            Description = "被动技能：持续侦测周围200范围内的敌方隐身单位，使其短暂显形",
            
            TargetType = AbilityTargetType.None,
            
            // 被动技能配置
            Flags = new()
            {
                DisableWhenDead = true, // 死亡时禁用
                PersistDuringMorph = true, // 变形时保持
            },
            
            // 定期触发搜索效果
            PassivePeriod = static (_) => TimeSpan.FromSeconds(0.5), // 每0.5秒触发一次
            PassivePeriodicEffect = MagicDetectionSearch,
            
            // 侦测指示特效
            ActorArray = [MagicDetectionEffect],
        };

        Game.Logger.LogInformation("✅ Magic Detection initialized successfully!");
    }
}
