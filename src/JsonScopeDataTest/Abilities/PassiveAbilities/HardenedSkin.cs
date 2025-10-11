using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.Behavior;
using GameCore.Data;
using GameCore.Execution.Data;
using GameCore.ResourceType.Data;
using GameCore.Struct;
using GameData;
using static GameCore.ScopeData;

namespace GameEntry.JsonScopeDataTest.Abilities.PassiveAbilities;

/// <summary>
/// 硬化体肤 - 被动技能，直接响应物理伤害并减少12点伤害
/// </summary>
public class HardenedSkin : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbility> HardenedSkinAbility = new("HardenedSkin"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectLog> HardenedSkinLogEffect = new("HardenedSkinLogEffect"u8);
    #endregion

    #region 响应定义
    public static readonly GameLink<GameDataResponse, GameDataResponseDamage> HardenedSkinDamageResponse = new("HardenedSkinDamageResponse"u8);
    #endregion

    #region 粒子和Actor定义
    public static readonly GameLink<GameDataParticle, GameDataParticle> HardenedSkinParticle = new("HardenedSkinParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> HardenedSkinEffect = new("HardenedSkinEffect"u8);
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

        Game.Logger.LogInformation("🛡️ Initializing Hardened Skin...");

        // ========== 日志效果配置 ==========
        // 用于测试响应是否触发的日志效果
        _ = new GameDataEffectLog(HardenedSkinLogEffect)
        {
            Message = context =>
            {
                return $"🛡️ HardenedSkin damaged response triggered! Target: {context.Target}";
            },
            Level = LogLevel.Information,
            LogExecutionFailure = true,
        };

        // ========== 粒子效果配置 ==========
        // 硬化体肤持续特效
        _ = new GameDataParticle(HardenedSkinParticle)
        {
            Asset = "effect/effect_new/effect_debuff/eff_jiansu/particle.effect"u8,
            AssetLayerScale = 1.0f,
            Radius = 128f,
        };

        _ = new GameDataActorParticle(HardenedSkinEffect)
        {
            AutoPlay = true,
            Particle = HardenedSkinParticle,
            KillOnFinish = false, // 持续显示
            ForceOneShot = false, // 循环播放
            InheritRotation = false,
        };

        // ========== 物理伤害响应配置 ==========
        _ = new GameDataResponseDamage(HardenedSkinDamageResponse)
        {
            Name = "硬化体肤减少物理伤害",
            Chance = static (_, _) => 1.0,
            Location = ResponseLocation.Defender, // 受击者触发
            
            // 只响应物理伤害
            DamageType = new()
            {
                [DamageType.Physical] = true,
            },
            ResponseEffect = HardenedSkinLogEffect,
            // // 减少12点伤害
            Modification = static (_, _) => -12.0,
        };

        // ========== 被动技能配置 ==========
        _ = new GameDataAbility(HardenedSkinAbility)
        {
            Name = "硬化体肤（被动）",
            DisplayName = "硬化体肤",
            Description = "被动技能：减少受到的物理伤害12点",
            
            TargetType = AbilityTargetType.None,
            
            // // 被动技能配置
            // Flags = new()
            // {
            //     DisableWhenDead = true, // 死亡时禁用
            //     PersistDuringMorph = true, // 变形时保持
            //     Hidden = false, // 显示在技能栏
            // },
            
            // 直接绑定伤害响应到技能上！
            Responses = [HardenedSkinDamageResponse],
            
            // 持续特效
            ActorArray = [HardenedSkinEffect],
            
            SyncType = SyncType.SelfOrSight,
        };

        Game.Logger.LogInformation("✅ Hardened Skin initialized successfully!");
    }
}
