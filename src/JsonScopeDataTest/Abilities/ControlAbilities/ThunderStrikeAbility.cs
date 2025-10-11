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
/// 控制技能：雷霆一击
/// 单体目标技能，在目标位置产生AOE雷电伤害和减速效果
/// </summary>
public class ThunderStrikeAbility : IGameClass
{
    #region 技能定义
    public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> ThunderStrike = new("ThunderStrike"u8);
    #endregion

    #region 效果定义
    public static readonly GameLink<GameDataEffect, GameDataEffectSearch> ThunderStrikeSearch = new("ThunderStrikeSearch"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectSet> ThunderStrikeSet = new("ThunderStrikeSet"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectDamage> ThunderStrikeDamage = new("ThunderStrikeDamage"u8);
    public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> ThunderStrikeAddBuff = new("ThunderStrikeAddBuff"u8);
    #endregion

    #region Buff定义
    public static readonly GameLink<GameDataBuff, GameDataBuff> ThunderDebuff = new("ThunderDebuff"u8);
    #endregion

    #region 冷却定义
    public static readonly GameLink<GameDataCooldown, GameDataCooldownActive> ThunderStrikeCooldown = new("ThunderStrikeCooldown"u8);
    #endregion

    #region 动画定义
    public static readonly GameLink<GameDataAnimation, GameDataAnimationSimple> ThunderStrikeAnim = new("ThunderStrikeAnim"u8);
    #endregion

    #region 粒子和Actor定义
    // 雷电冲击波特效 (类似ini中的Model_1)
    public static readonly GameLink<GameDataParticle, GameDataParticle> ThunderClapParticle = new("ThunderClapParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ThunderClapEffect = new("ThunderClapEffect"u8);
    
    // 雷电撞击特效 (类似ini中的Model_2)
    public static readonly GameLink<GameDataParticle, GameDataParticle> ThunderHitParticle = new("ThunderHitParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ThunderHitEffect = new("ThunderHitEffect"u8);
    
    // AOE范围特效 (类似ini中的Particle_2)
    public static readonly GameLink<GameDataParticle, GameDataParticle> ThunderAOEParticle = new("ThunderAOEParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ThunderAOEEffect = new("ThunderAOEEffect"u8);
    
    // 目标身上的雷电减速特效 (buff持续特效)
    public static readonly GameLink<GameDataParticle, GameDataParticle> ThunderBuffParticle = new("ThunderBuffParticle"u8);
    public static readonly GameLink<GameDataActor, GameDataActorParticle> ThunderBuffEffect = new("ThunderBuffEffect"u8);
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
        Game.Logger.LogInformation("⚡ Initializing Thunder Strike Ability for Test...");

        // ========== 动画配置 ==========
        _ = new GameDataAnimationSimple(ThunderStrikeAnim)
        {
            Name = "雷霆一击动画",
            File = "anim/human/barehanded_anim/hand_05/skill_09.ani"u8, // 使用和ini文件相同的动画
            IsLooping = false,
        };

        // ========== 粒子效果配置 ==========
        // 雷电冲击波特效 (主要特效)
        _ = new GameDataParticle(ThunderClapParticle)
        {
            Asset = "effect/SampleSpells/Thunder/ThunderClap/particle.effect"u8, // 使用ini中的特效
            Radius = 128f,
        };

        // 雷电撞击特效 (伤害特效)
        _ = new GameDataParticle(ThunderHitParticle)
        {
            Asset = "effect/SampleSpells/Thunder/ThunderClap_Hit/particle.effect"u8, // 使用ini中的特效
            Radius = 96f,
        };

        // AOE范围特效 (范围显示)
        _ = new GameDataParticle(ThunderAOEParticle)
        {
            Asset = "effect/SampleSpells/Thunder/thunderclap_aoe/particle.effect"u8, // 使用ini中的特效
            AssetLayerScale = 2f, // 和ini中的Scale一致
            Radius = 256f, // 与搜索半径匹配
        };

        // 雷电减速buff特效 (目标身上的持续特效)
        _ = new GameDataParticle(ThunderBuffParticle)
        {
            Asset = "effect/SampleSpells/Thunder/ThunderClap/particle.effect"u8, // 重用雷电特效作为buff特效
            Radius = 64f,
        };

        // ========== Actor配置 ==========
        // 雷电冲击波Actor (绑定到身体socket)
        _ = new GameDataActorParticle(ThunderClapEffect)
        {
            AutoPlay = true,
            Particle = ThunderClapParticle,
            KillOnFinish = false, // 持续特效，和ini中一样
            ForceOneShot = false,
            Socket = "socket_body"u8, // 绑定到身体位置，和ini中一样
        };

        // 雷电撞击Actor (伤害时的特效)
        _ = new GameDataActorParticle(ThunderHitEffect)
        {
            AutoPlay = true,
            Particle = ThunderHitParticle,
            KillOnFinish = true,
            ForceOneShot = true, // 一次性特效
        };

        // AOE范围Actor (搜索时的范围特效)
        _ = new GameDataActorParticle(ThunderAOEEffect)
        {
            AutoPlay = true,
            Particle = ThunderAOEParticle,
            KillOnFinish = true,
            ForceOneShot = true, // 一次性范围特效
        };

        // 雷电buff特效Actor (持续在目标身上)
        _ = new GameDataActorParticle(ThunderBuffEffect)
        {
            AutoPlay = true,
            Particle = ThunderBuffParticle,
            KillOnFinish = false, // 跟随buff持续时间
            ForceOneShot = false,
        };

        // ========== 冷却配置 ==========
        _ = new GameDataCooldownActive(ThunderStrikeCooldown)
        {
            Time = static (context) => {
                // 基础冷却时间10秒 (和ini中一样)
                double baseCooldown = 10.0;
                return TimeSpan.FromSeconds(baseCooldown);
            },
        };

        // ========== Buff配置 ==========
        // 雷电减速Buff (修改为减速效果，而不是ini中的加速效果)
        _ = new GameDataBuff(ThunderDebuff)
        {
            Name = "雷电减速",
            DisplayName = "雷电减速",
            Description = "被雷电麻痹，移动速度降低50%",
            Duration = static (_) => TimeSpan.FromSeconds(2), // 持续2秒，和ini中一样
            SyncType = SyncType.Sight, // 注意SyncType设置
            
            // 属性修改 - 减速效果
            Modifications = [
                new() {
                    Property = UnitProperty.MoveSpeed, // 移动速度
                    SubType = PropertySubType.Base, // 基础修改
                    Value = static (_) => -175.0 // 减少175移动速度（相当于50%减速）
                }
            ],
            
            // Buff身上的持续雷电特效
            ActorArray = [ThunderBuffEffect],
        };

        // ========== 效果配置 ==========
        // 雷电伤害效果
        _ = new GameDataEffectDamage(ThunderStrikeDamage)
        {
            Name = "雷霆一击伤害",
            Amount = static (_) => 60, // 基础伤害60，和ini中一样
            Type = DamageType.Magical, // 魔法伤害
            LogExecutionFailure = true,
            ActorArray = [ThunderHitEffect], // 伤害时的撞击特效
        };

        // 添加Buff效果
        _ = new GameDataEffectBuffAdd(ThunderStrikeAddBuff)
        {
            Name = "雷霆一击添加减速",
            BuffLink = ThunderDebuff,
            LogExecutionFailure = true,
        };

        // 组合效果Set (同时造成伤害和添加buff)
        _ = new GameDataEffectSet(ThunderStrikeSet)
        {
            Name = "雷霆一击组合效果",
            Effects = [
                new() { Link = ThunderStrikeDamage }, // 造成伤害
                new() { Link = ThunderStrikeAddBuff }, // 添加减速buff
            ],
            LogExecutionFailure = true,
        };

        // AOE搜索效果 (类似ini中的Search_1)
        _ = new GameDataEffectSearch(ThunderStrikeSearch)
        {
            Name = "雷霆一击范围搜索",
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy], // 只搜索敌方单位
                Excluded = [UnitState.Dead, UnitState.Invulnerable] // 排除死亡和无敌单位
            }],
            Radius = static (_) => 256, // 搜索半径256，和ini中一样
            ActorArray = [ThunderAOEEffect], // AOE范围特效
            Effect = ThunderStrikeSet, // 对搜索到的单位执行组合效果
            LogExecutionFailure = true,
        };

        // ========== 技能配置 ==========
        _ = new GameDataAbilityExecute(ThunderStrike)
        {
            Name = "雷霆一击",
            DisplayName = "雷霆一击",
            Description = "对目标发动雷电攻击，在目标位置产生雷电冲击波，对范围内敌人造成60点魔法伤害并减速2秒",
            
            Time = new()
            {
                Preswing = static (_) => TimeSpan.FromSeconds(0.26), // 类似ini中的cast_start_time
                Channel = static (_) => TimeSpan.FromSeconds(0.11),   // 类似ini中的cast_shot_time
                Backswing = static (_) => TimeSpan.FromSeconds(0.50), // 类似ini中的cast_finish_time
            },
            
            Cost = new()
            {
                Cooldown = ThunderStrikeCooldown
            },
            
            AbilityActiveFlags = new() { AllowEnqueueInCooldown = true },
            AbilityExecuteFlags = new() { },
            Effect = ThunderStrikeSearch, // 主效果是范围搜索
            TargetType = AbilityTargetType.Unit, // 单体目标技能，和ini中target_type=1一致
            Range = static (_) => 256, // 施法距离，和ini中一样
            
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
                TargetingIndicator = AreaIndicator, // 使用区域指示器，显示AOE范围
                CursorRadius = static (_) => 256, // 显示影响范围
            },
            
            Animation = [ThunderStrikeAnim],
            LogExecutionFailure = true,
        };

        Game.Logger.LogInformation("✅ Thunder Strike Ability initialized successfully!");
    }
}
