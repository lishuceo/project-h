using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.ModelAnimation.Data;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.TargetingSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.BuffSystem.Data;
using GameCore.Struct;
using GameData;
using static GameCore.ScopeData;
using GameCore.Components;
using GameCore.Components.Data;

namespace GameEntry.JsonScopeDataTest.Abilities.ControlAbilities;

/// <summary>
/// 控制技能：群体减速
/// 区域减速技能，减慢敌人移动速度和攻击速度
/// </summary>
public class SlowAreaAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> SlowArea = new("SlowArea"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> SlowAreaSearch = new("SlowAreaSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> SlowAreaAddBuff = new("SlowAreaAddBuff"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> SlowBuff = new("SlowBuff"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> SlowAreaCooldown = new("SlowAreaCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> SlowAreaAnim = new("SlowAreaAnim"u8);
    #endregion

    #region 粒子和Actor定义
    // 地面效果粒子 (施法位置的特效)
    public static readonly GameLink<GameDataParticle, GameDataParticle> SlowGroundParticle = new("SlowGroundParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> SlowGroundEffect = new("SlowGroundEffect"u8);
    
    // 目标身上的特效粒子 (被减速单位身上的特效)
    public static readonly GameLink<GameDataParticle, GameDataParticle> SlowTargetParticle = new("SlowTargetParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> SlowTargetEffect = new("SlowTargetEffect"u8);
    #endregion

    #region 瞄准指示器定义
    public static readonly GameLink<GameDataTargetingIndicator, GameDataTargetingIndicator> AreaIndicator = new("p_0tja.ScopeData.GameDataTargetingIndicator.AreaIndicator.Root"u8);
    #endregion

    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        Game.Logger.LogInformation("❄️ Initializing Slow Area Ability for Test...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(SlowAreaAnim)
        {
            Name = "群体减速动画",
            File = "anim/human/barehanded_anim/hand_05/skill_018.ani"u8, // 使用和ini文件相同的动画
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        // 地面施法特效 (类似ini中的Particle_1)
        _ = new GameDataParticle(SlowGroundParticle)
        {
            Asset = "effect/samplespells/slow/slow/particle.effect"u8, // 使用ini中相同的特效
            AssetLayerScale = 1.3f, // 和ini中的Scale相同
            Radius = 128f, // 与搜索半径匹配
        };

        // 目标身上的减速特效 (类似ini中的Particle_2)
        _ = new GameDataParticle(SlowTargetParticle)
        {
            Asset = "effect/samplespells/slow/slow_target/particle.effect"u8, // 使用ini中相同的目标特效
            Radius = 64f,
        };

        // ========== Actor配置 ==========
        // 地面效果Actor
        _ = new GameDataActorParticle(SlowGroundEffect)
        {
            AutoPlay = true,
            Particle = SlowGroundParticle,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // 目标身上效果Actor (持续特效)
        _ = new GameDataActorParticle(SlowTargetEffect)
        {
            AutoPlay = true,
            Particle = SlowTargetParticle,
            KillOnFinish = false, // 不自动结束，跟随Buff持续时间
            ForceOneShot = false,
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(SlowAreaCooldown)
        {
            Time = static (context) => {
                // 基础冷却时间10秒 (和ini中一样)
                double baseCooldown = 10.0;
                return TimeSpan.FromSeconds(baseCooldown);
            },
        };

        // ========== Buff配置 ==========
        // 减速Buff (同时影响移动速度和攻击速度)
        _ = new GameDataBuff(SlowBuff)
        {
            Name = "减速效果",
            DisplayName = "减速",
            Description = "移动速度和攻击速度降低60%",
            Duration = static (_) => TimeSpan.FromSeconds(5), // 持续5秒，和ini中一样
            SyncType = SyncType.Sight,
            // 属性修改数组 - 减速效果
            Modifications = [
                new() {
                    Property = UnitProperty.MoveSpeed, // 移动速度
                    SubType = PropertySubType.Base, // 基础修改
                    Value = static (_) => -210.0 // 减少210移动速度（相当于60%减速）
                }
                // 注释掉攻击速度，因为该属性不存在
                // new() {
                //     Property = UnitProperty.AttackSpeed, // 攻击速度
                //     SubType = PropertySubType.Base,
                //     Value = static (_) => -0.6
                // }
            ],
            
            // Buff身上的持续特效
            ActorArray = [SlowTargetEffect],
        };

        // ========== 效果配置 ==========
        // 添加Buff效果
        _ = new GameDataEffectBuffAdd(SlowAreaAddBuff)
        {
            Name = "群体减速添加Buff",
            BuffLink = SlowBuff,
            LogExecutionFailure = true,
        };

        // 区域搜索效果 (类似ini中的Search_1)
        _ = new GameDataEffectSearch(SlowAreaSearch)
        {
            Name = "群体减速范围搜索",
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy], // 只搜索敌方单位
                Excluded = [UnitState.Dead, UnitState.Invulnerable] // 排除死亡和无敌单位
            }],
            Radius = static (_) => 300, // 搜索半径300，和ini中一样
            MinCount = static (_) => 1, // 最少搜索到1个单位，和ini中一样
            ActorArray = [SlowGroundEffect], // 地面特效
            Effect = SlowAreaAddBuff, // 对搜索到的单位添加减速Buff
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(SlowArea)
        {
            Name = "群体减速",
            DisplayName = "群体减速",
            Description = "在目标区域施放减速法术，使区域内敌方单位移动速度和攻击速度降低60%，持续5秒",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.66), // 类似ini中的cast_start_time
                Channel = static (_) => TimeSpan.FromSeconds(0.2),   // 类似ini中的cast_shot_time
                Backswing = static (_) => TimeSpan.FromSeconds(0.64), // 类似ini中的cast_finish_time
            },
            
            Cost = new()
            {
                Cooldown = SlowAreaCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = SlowAreaSearch, // 主效果是区域搜索
            TargetType = AbilityTargetType.Ground, // 地面目标类型
            Range = static (_) => 600, // 施法距离 (参考ini中的range)
            
            AcquireSettings = new()
            {
                TargetingFilters = [new()
                { 
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }],
            },
            
            TargetingIndicatorInfo = new()
            {
                TargetingIndicator = AreaIndicator, // 使用现有的区域指示器
                CursorRadius = static (_) => 300, // 显示影响范围，和搜索半径一致
            },
            
            Animation = [SlowAreaAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Slow Area Ability initialized successfully!");
    }
}
