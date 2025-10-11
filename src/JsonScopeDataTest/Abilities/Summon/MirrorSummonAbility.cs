using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.BuffSystem.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.Components.Data;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.ModelAnimation.Data;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameData;
using static GameCore.ScopeData;
using Microsoft.Extensions.Logging;
using GameCore.Struct;

namespace GameEntry.JsonScopeDataTest.Abilities.Summon;

/// <summary>
/// 镜像召唤技能 - 在目标位置创建镜像单位
/// 简洁的效果节点实现方式
/// </summary>
public class MirrorSummonAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> MirrorSummon = new("MirrorSummon"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectCreateUnit> CreateMirrorUnit = new("CreateMirrorUnit"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> AddMirrorBuff = new("AddMirrorBuff"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectUnitRemove> RemoveMirrorUnit = new("RemoveMirrorUnit"u8);
    #endregion

    // 注意：不再需要自定义镜像单位，因为我们动态复制施法者单位类型

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> MirrorIllusionBuff = new("MirrorIllusionBuff"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> MirrorSummonCooldown = new("MirrorSummonCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> MirrorSummonAnim = new("MirrorSummonAnim"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> SummonParticle = new("MirrorSummonParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> SummonEffect = new("MirrorSummonEffect"u8);
    #endregion

    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        Game.Logger.LogInformation("🪞 Initializing Mirror Summon Ability for JsonScopeDataTest...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(MirrorSummonAnim)
        {
            Name = "镜像召唤动画",
            File = "anim/human/barehanded_anim/hand_07/skill_01.ani"u8,
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        _ = new GameDataParticle(SummonParticle)
        {
            Asset = "effect/effect_new1/effect_guanghuan/eff_boss_guanghuan/particle.effect"u8,
            AssetLayerScale = 1.2f,
            Radius = 96f,
        };

        _ = new GameDataActorParticle(SummonEffect)
        {
            AutoPlay = true,
            Particle = SummonParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(MirrorSummonCooldown)
        {
            Time = static (_) => TimeSpan.FromSeconds(8.0), // 8秒冷却
        };

        // ========== Buff配置 ==========
        _ = new GameDataBuff(MirrorIllusionBuff)
        {
            Name = "镜像幻象",
            Duration = static (_) => TimeSpan.FromSeconds(30), // 30秒持续时间
            ExpireEffect = RemoveMirrorUnit, // Buff到期时删除镜像单位
        };

        // 镜像单位配置：动态复制施法者，无需预定义镜像单位类型

        // ========== 效果配置 ==========
        
        // 创建镜像单位 - 复制施法者的单位类型
        _ = new GameDataEffectCreateUnit(CreateMirrorUnit)
        {
            Name = "创建镜像单位",
            UnitPicker = UnitPicker.Location, // 使用位置方式动态获取单位类型
            SpawnTypeUnit = new TargetLocationExpression { Value = TargetLocation.Caster }, // 从施法者获取单位类型
            TargetLocation = new TargetLocationExpression { Value = TargetLocation.MainTarget }, // 在目标位置创建
            SpawnOwner = new EffectOwnerExpression { Value = EffectOwner.TargetLocation }, // 镜像归属于施法者
            SpawnCount = static (_) => 1, // 创建1个镜像
            SpawnOffset = null, // 精确位置创建，不使用偏移
            LogExecutionFailure = true,
            SpawnEffect = AddMirrorBuff, // 创建后添加镜像Buff
        };

        // 给镜像添加幻象Buff
        _ = new GameDataEffectBuffAdd(AddMirrorBuff)
        {
            Name = "添加镜像Buff",
            BuffLink = MirrorIllusionBuff,
            LogExecutionFailure = true,
        };

        // 移除镜像单位（用于Buff到期时） - 直接移除，无伤害
        _ = new GameDataEffectUnitRemove(RemoveMirrorUnit)
        {
            Name = "移除镜像单位",
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(MirrorSummon)
        {
            Name = "镜像",
            DisplayName = "镜像",
            Description = "在目标位置召唤一个镜像单位，镜像会持续30秒",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.2),
                Channel = static (_) => TimeSpan.FromSeconds(0.3),
                Backswing = static (_) => TimeSpan.FromSeconds(0.5),
            },
            
            Cost = new()
            {
                Cooldown = MirrorSummonCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = CreateMirrorUnit,
            TargetType = AbilityTargetType.Ground, // 地面目标
            Range = static (_) => 1000, // 1000范围
            
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = new GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator>("$p_0tja.ScopeData.GameDataTargetingIndicator.AreaIndicator.Root"u8),
                CursorRadius = static (_) => 64,
                VectorLineWidth = static (_) => 48,
                VectorHighlightLimit = static (_) => 1
            },
            
            Animation = [MirrorSummonAnim],
            ActorArray = [SummonEffect], // 召唤特效
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Mirror Summon Ability initialized successfully! 喵~");
    }
}
