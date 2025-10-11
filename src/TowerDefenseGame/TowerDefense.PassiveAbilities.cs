using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.BuffSystem.Data;
using GameCore.Components;
using GameCore.Components.Data;
using GameCore.CooldownSystem.Data;
using GameCore.EntitySystem;
using GameCore.EntitySystem.Data.Enum;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.Execution.Data.Struct;
using GameCore.ResourceType.Data;
using GameCore.Struct;
using GameCore.VitalSystem;

using GameData;

using static GameCore.ScopeData;

namespace GameEntry.TowerDefenseGame;

/// <summary>
/// 塔防被动技能系统 - 定义四种塔的自动攻击技能
/// </summary>
public class PassiveAbilities : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    // 技能定义
    public static class Ability
    {
        // 塔防攻击技能 - 被动版本 (自动攻击)
        public static readonly GameLink<GameDataAbility, GameDataAbility> SlowProjectilePassive = new("TowerDefense_SlowProjectilePassive"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbility> AuraSlowPassive = new("TowerDefense_AuraSlowPassive"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbility> AOEDamagePassive = new("TowerDefense_AOEDamagePassive"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbility> PenetrateAttackPassive = new("TowerDefense_PenetrateAttackPassive"u8);
    }

    // 效果定义
    public static class Effect
    {
        // 🎯 1. 减速塔被动技能效果链 - 简化版本使用 GameDataEffectLaunchMissile
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> SlowProjectilePassiveDamage = new("TowerDefense_SlowProjectilePassiveDamage"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> SlowProjectilePassiveBuffAdd = new("TowerDefense_SlowProjectilePassiveBuffAdd"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> SlowProjectilePassiveCompleteEffect = new("TowerDefense_SlowProjectilePassiveCompleteEffect"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> SlowProjectilePassiveLaunchMissile = new("TowerDefense_SlowProjectilePassiveLaunchMissile"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSearch> SlowProjectilePassiveSearch = new("TowerDefense_SlowProjectilePassiveSearch"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> SlowTowerPassiveWithLog = new("TowerDefense_SlowTowerPassiveWithLog"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectLog> SlowProjectilePassiveImpactLog = new("TowerDefense_SlowProjectilePassiveImpactLog"u8);

        // 🌪️ 2. 光环减速塔被动技能效果链
        public static readonly GameLink<GameDataEffect, GameDataEffectBuffAdd> AuraSlowPassiveBuffAdd = new("TowerDefense_AuraSlowPassiveBuffAdd"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSearch> AuraSlowPassiveSearch = new("TowerDefense_AuraSlowPassiveSearch"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> AuraSlowTowerPassiveWithLog = new("TowerDefense_AuraSlowTowerPassiveWithLog"u8);


        // 💥 3. 群伤塔被动技能效果链
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> AOEProjectilePassiveImpact = new("TowerDefense_AOEProjectilePassiveImpact"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> AOEProjectilePassiveLaunch = new("TowerDefense_AOEProjectilePassiveLaunch"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSearch> AOEDamagePassiveSearch = new("TowerDefense_AOEDamagePassiveSearch"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> AOETowerPassiveWithLog = new("TowerDefense_AOETowerPassiveWithLog"u8);
        // 新增：AOE撞击时的范围搜索和完整效果
        public static readonly GameLink<GameDataEffect, GameDataEffectSearch> AOEImpactAreaSearch = new("TowerDefense_AOEImpactAreaSearch"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> AOEProjectileCompleteEffect = new("TowerDefense_AOEProjectileCompleteEffect"u8);

        // ⚡ 4. 穿透塔被动技能效果链
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> PenetrateAttackPassiveDamage = new("TowerDefense_PenetrateAttackPassiveDamage"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSearch> PenetrateAttackPassiveSearch = new("TowerDefense_PenetrateAttackPassiveSearch"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> PenetrateTowerPassiveWithLog = new("TowerDefense_PenetrateTowerPassiveWithLog"u8);
        // 新增：穿透塔的单体搜索和直线穿透效果
        public static readonly GameLink<GameDataEffect, GameDataEffectSearch> PenetrateTargetSearch = new("TowerDefense_PenetrateTargetSearch"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> PenetrateLineSearch = new("TowerDefense_PenetrateLineSearch"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSearch> PenetrateLineSearchActual = new("TowerDefense_PenetrateLineSearchActual"u8);
        // 穿透搜索日志效果
        public static readonly GameLink<GameDataEffect, GameDataEffectLog> PenetrateSearchLog = new("TowerDefense_PenetrateSearchLog"u8);

        // 🎨 减速Buff特效效果已简化 - 只使用持续性特效
    }

    // 单位定义
    public static class Unit
    {
        public static readonly GameLink<GameDataUnit, GameDataUnit> SlowProjectileMissile = new("TowerDefense_SlowProjectileMissile"u8);
        public static readonly GameLink<GameDataUnit, GameDataUnit> AOEProjectileMissile = new("TowerDefense_AOEProjectileMissile"u8);
    }

    // Mover定义
    public static class Mover
    {
        public static readonly GameLink<GameDataMover, GameDataMoverTarget> SlowProjectileMoverTarget = new("TowerDefense_SlowProjectileMoverTarget"u8);
    }

    // Buff定义
    public static class Buff
    {
        public static readonly GameLink<GameDataBuff, GameDataBuff> SlowDebuff = new("TowerDefense_SlowDebuff"u8);
    }

    // Actor定义 - 用于特效
    public static class Actor
    {
        // 持续性减速特效 - 附着在目标身上，buff消失时自动移除
        public static readonly GameLink<GameDataActor, GameDataActorParticle> SlowDebuffEffect = new("TowerDefense_SlowDebuffEffect"u8);
        // 光环减速搜索特效 - 搜索时显示的AOE范围特效
        public static readonly GameLink<GameDataActor, GameDataActorParticle> AuraSlowSearchEffect = new("TowerDefense_AuraSlowSearchEffect"u8);
        // AOE撞击特效 - 火环爆炸效果
        public static readonly GameLink<GameDataActor, GameDataActorParticle> AOEImpactEffect = new("TowerDefense_AOEImpactEffect"u8);
        // 穿透攻击特效 - 直线剑气效果
        public static readonly GameLink<GameDataActor, GameDataActorParticle> PenetrateLineEffect = new("TowerDefense_PenetrateLineEffect"u8);
    }

    // 粒子效果定义
    public static class Particle
    {
        // 持续性减速粒子效果 - buff存在期间持续显示，buff消失时自动移除
        public static readonly GameLink<GameDataParticle, GameDataParticle> SlowDebuffParticle = new("TowerDefense_SlowDebuffParticle"u8);
        // 光环减速搜索特效 - 搜索时显示的AOE范围特效
        public static readonly GameLink<GameDataParticle, GameDataParticle> AuraSlowSearchEffect = new("TowerDefense_AuraSlowSearchEffect"u8);
        // AOE撞击特效 - 火环爆炸效果
        public static readonly GameLink<GameDataParticle, GameDataParticle> AOEImpactEffect = new("TowerDefense_AOEImpactEffect"u8);
        // 穿透攻击特效 - 直线剑气效果
        public static readonly GameLink<GameDataParticle, GameDataParticle> PenetrateLineEffect = new("TowerDefense_PenetrateLineEffect"u8);
    }

    private static void OnGameDataInitialization()
    {
        // 在塔防模式和GameUITest模式下初始化（GameUITest需要测试SlowDebuff）
        if (Game.GameModeLink != ScopeData.GameMode.TowerDefense && 
            Game.GameModeLink != ScopeData.GameMode.GameUITest)
        {
            return;
        }

        var modeName = Game.GameModeLink == ScopeData.GameMode.TowerDefense ? "塔防模式" : "GameUITest模式";
        Game.Logger.LogInformation("🏰 Initializing Tower Defense Passive Abilities System for {modeName}...", modeName);

        CreateParticleEffects();
        CreateActors();
        CreateMovers();
        CreateMissileUnits();
        CreateBuffs();
        CreateEffects();

        Game.Logger.LogInformation("✅ Tower Defense Passive Abilities System initialized successfully for {modeName}!", modeName);
    }

    private static void CreateParticleEffects()
    {
        // 🎨 持续性减速特效粒子 - 蓝色冰霜效果，buff消失时自动移除
        _ = new GameDataParticle(Particle.SlowDebuffParticle)
        {
            Asset = "effect/effect_new/effect_debuff/eff_jiansu/particle.effect"u8,
        };

        // 🌪️ 光环减速搜索特效粒子 - AOE范围指示特效，四倍大小
        _ = new GameDataParticle(Particle.AuraSlowSearchEffect)
        {
            Asset = "effect/effect_new1/effect_mofa/eff_yuanzhu_aoe/particle.effect"u8,
        };

        // 💥 AOE撞击特效粒子 - 火环爆炸效果，2倍大小
        _ = new GameDataParticle(Particle.AOEImpactEffect)
        {
            Asset = "effect/effect_new/effect_hit/ps_firering_hit_eff/particle.effect"u8,
            Radius = 128 // 2倍大小 (64 * 2 = 128)
        };

        // ⚡ 穿透攻击特效粒子 - 直线剑气效果
        _ = new GameDataParticle(Particle.PenetrateLineEffect)
        {
            Asset = "effect/eff_xujian/effect_emiti_003/particle.effect"u8,
            Radius = 64
        };
    }

    private static void CreateActors()
    {
        // 🎨 持续性减速特效Actor - 循环播放直到Buff移除
        _ = new GameDataActorParticle(Actor.SlowDebuffEffect)
        {
            AutoPlay = true,
            Particle = Particle.SlowDebuffParticle,
            KillOnFinish = false, // 不要在播放完成后自动结束
            ForceOneShot = false, // 允许循环播放
        };

        // 🌪️ 光环减速搜索特效Actor - 搜索时的瞬时AOE特效
        _ = new GameDataActorParticle(Actor.AuraSlowSearchEffect)
        {
            AutoPlay = true,
            Particle = Particle.AuraSlowSearchEffect,
            KillOnFinish = true, // 播放完成后自动销毁
            ForceOneShot = true, // 一次性播放
            Scale = 4.0f // 四倍大小
        };

        // 💥 AOE撞击特效Actor - 炮弹撞击时的火环爆炸特效
        _ = new GameDataActorParticle(Actor.AOEImpactEffect)
        {
            AutoPlay = true,
            Particle = Particle.AOEImpactEffect,
            KillOnFinish = true, // 播放完成后自动销毁
            ForceOneShot = true, // 一次性播放
            Scale = 2.0f // 2倍大小
        };

        // ⚡ 穿透攻击特效Actor - 直线剑气穿透特效
        _ = new GameDataActorParticle(Actor.PenetrateLineEffect)
        {
            AutoPlay = true,
            Particle = Particle.PenetrateLineEffect,
            KillOnFinish = true, // 播放完成后自动销毁
            ForceOneShot = true, // 一次性播放
            Scale = 1.0f // 正常大小
        };
    }

    private static void CreateMovers()
    {
        // 🎯 其他移动器可以在这里添加
        // 减速导弹现在使用 GameDataEffectLaunchMissile，不再需要单独的移动器配置
    }

    private static void CreateMissileUnits()
    {
        // 减速投射物
        _ = new GameDataUnit(Unit.SlowProjectileMissile)
        {
            Name = "减速弹",
            Filter = [UnitFilter.Missile],
            State = [UnitState.Invulnerable],
            CollisionRadius = 16,
            AttackableRadius = 32,
            Particle = "effect/eff_tongyong/huoqiu2/particle.effect"u8,
            UpdateFlags = new()
            {
                AllowMover = true,
            },
        };

        // 群伤炮弹
        _ = new GameDataUnit(Unit.AOEProjectileMissile)
        {
            Name = "群伤炮弹",
            Filter = [UnitFilter.Missile],
            State = [UnitState.Invulnerable],
            CollisionRadius = 16,
            AttackableRadius = 32,
            Particle = "effect/eff_tongyong/huoqiu2/particle.effect"u8,
            UpdateFlags = new()
            {
                AllowMover = true,
            },
            // 🎯 追踪配置 - 实现导弹追踪效果 (这些属性需要通过其他方式配置)
        };
    }

    private static void CreateBuffs()
    {
        // 🎯 减速Buff - 基础实现（移动速度减少50%，持续1秒）
        _ = new GameDataBuff(Buff.SlowDebuff)
        {
            Name = "减速效果",
            Description = "移动速度降低50%，持续1秒",
            Duration = (_) => TimeSpan.FromSeconds(1), // 持续1秒
            Icon = "image/buff/buff_3.png",
            Polarity = GameCore.BuffSystem.Data.Enum.BuffPolarity.Negative, // 负面效果
            
            // 🔢 叠加配置 - 限制只能有一个实例
            StackStart = 1, // 初始层数为1
            StackMax = static (_) => 1, // 最大层数为1（不能叠加）
            InstanceMax = 1, // 最大实例数为1（不同施法者也不能叠加）
            
            // 🎨 视觉特效配置 - 持续性减速特效附着在目标身上
            ActorArray = [Actor.SlowDebuffEffect],
            // 🐌 属性修改 - 减少50%移动速度
            Modifications = [
                new()
                {
                    Property = UnitProperty.MoveSpeed,
                    SubType = PropertySubTypeTowerDefense.Multiplier,
                    Value = static (_) => -50 // 乘数设置为-50，即减速50%
                }
            ],
            SyncType = EngineInterface.BaseType.SyncType.Sight
        };
    }

    private static void CreateEffects()
    {
        CreateSlowTowerEffects();
        CreateAuraSlowTowerEffects();
        CreateAOETowerEffects();
        CreatePenetrateTowerEffects();
        CreateLogWrapperEffects();
    }

    private static void CreateSlowTowerEffects()
    {
        // 🎯 1. 减速塔被动技能效果链
        
        // 减速投射物伤害效果
        _ = new GameDataEffectDamage(Effect.SlowProjectilePassiveDamage)
        {
            // 🎯 使用塔的真实攻击力属性，而不是固定数值
            Amount = (context) => 
            {
                // 从效果上下文获取施法者（塔）并转换为Unit类型
                if (context.Caster is GameCore.EntitySystem.Unit tower)
                {
                    // 获取塔的最终攻击力
                    var finalAttack = tower.GetUnitPropertyFinal(UnitProperty.AttackDamage);
                    if (finalAttack.HasValue)
                    {
                        return (float)finalAttack.Value;
                    }
                }
                return 60f; // 默认值作为后备方案
            },
            Type = DamageType.Physical,
            LogExecutionFailure = true,
        };

        // 减速Buff添加效果
        _ = new GameDataEffectBuffAdd(Effect.SlowProjectilePassiveBuffAdd)
        {
            BuffLink = Buff.SlowDebuff,
            LogExecutionFailure = true,
        };

        // 🎯 减速导弹CompleteEffect - 包含伤害+减速+特效的完整效果集合
        _ = new GameDataEffectSet(Effect.SlowProjectilePassiveCompleteEffect)
        {
            Effects = [
                new() { Link = Effect.SlowProjectilePassiveImpactLog }, // 🔍 撞击日志 - 查看攻击目标是否正确
                new() { Link = Effect.SlowProjectilePassiveBuffAdd }, // Buff本身包含属性修改和特效
                new() { Link = Effect.SlowProjectilePassiveDamage },
            ],
        };

        // 🚀 减速导弹发射效果 - 使用GameDataEffectLaunchMissile简化实现
        _ = new GameDataEffectLaunchMissile(Effect.SlowProjectilePassiveLaunchMissile)
        {
            Method = EffectLaunchMissileMethod.CreateMissile,
            Missile = Unit.SlowProjectileMissile,
            LaunchHeight = static (_) => 300,
            TargetHeight = static (_) => 50,
            Speed = static (_) => 500f,
            CompleteEffect = Effect.SlowProjectilePassiveCompleteEffect, // 🎯 使用CompleteEffect实现伤害+减速+特效
            LogExecutionFailure = true,
        };

        // 撞击日志效果 - 用于打印撞击目标信息
        _ = new GameDataEffectLog(Effect.SlowProjectilePassiveImpactLog)
        {
            Message = context => 
            {
                return $"💥 减速导弹撞击目标喵～ " +
                       $"撞击目标: {context.Target?.ToString() ?? "null"}, " +
                       $"导弹单位: {context.Caster?.ToString() ?? "null"}";
            },
            Level = Microsoft.Extensions.Logging.LogLevel.Information,
        };

        // 减速塔被动搜索效果
        _ = new GameDataEffectSearch(Effect.SlowProjectilePassiveSearch)
        {
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Radius = static (_) => 1000, // 搜索范围
            MaxCount = static (_) => 1, // 单体攻击
            Method = SearchMethod.Circle,
            Effect = Effect.SlowProjectilePassiveLaunchMissile, // 🚀 使用新的LaunchMissile效果
            LogExecutionFailure = true,
        };

        // 🎯 1. 减速塔被动技能 - 每2秒自动释放
        _ = new GameDataAbility(Ability.SlowProjectilePassive)
        {
            DisplayName = "自动减速射击",
            Description = "每2秒自动向最近敌人发射减速弹",
            PassivePeriod = static (_) => TimeSpan.FromSeconds(2.0),
            PassivePeriodicEffect = Effect.SlowTowerPassiveWithLog,
            // ActorArray = [Actor.SlowDebuffEffect],
        };
    }

    private static void CreateAuraSlowTowerEffects()
    {
        // 🌪️ 2. 光环减速塔被动技能效果链
        
        // 光环减速Buff添加效果
        _ = new GameDataEffectBuffAdd(Effect.AuraSlowPassiveBuffAdd)
        {
            BuffLink = Buff.SlowDebuff, // 复用减速Buff（包含属性修改和特效）
            LogExecutionFailure = true,
        };

        // 光环减速被动搜索效果 - 修改为600范围，添加搜索特效
        _ = new GameDataEffectSearch(Effect.AuraSlowPassiveSearch)
        {
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Radius = static (_) => 600, // 修改光环范围为600
            MaxCount = static (_) => 5, // 最多影响5个敌人
            Method = SearchMethod.Circle,
            Effect = Effect.AuraSlowPassiveBuffAdd, // 直接使用Buff添加效果
            LogExecutionFailure = true,
            // 🎨 添加搜索特效 - 在施法者位置播放AOE范围指示
            ActorArray = [Actor.AuraSlowSearchEffect],
        };

        // 🌪️ 2. 光环减速被动技能 - 每1秒自动释放
        _ = new GameDataAbility(Ability.AuraSlowPassive)
        {
            DisplayName = "自动光环减速",
            Description = "每1秒自动对周围600范围敌人施加减速效果",
            PassivePeriod = static (_) => TimeSpan.FromSeconds(1),
            PassivePeriodicEffect = Effect.AuraSlowTowerPassiveWithLog,
        };
    }

    private static void CreateAOETowerEffects()
    {
        // 💥 3. 群伤塔被动技能效果链 - 重新设计：单体瞄准 + AOE撞击
        
        // AOE撞击范围搜索效果 - 300范围内搜索敌人并造成伤害
        _ = new GameDataEffectSearch(Effect.AOEImpactAreaSearch)
        {
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Radius = static (_) => 300, // AOE撞击范围300
            MaxCount = static (_) => 8, // 最多影响8个敌人
            Method = SearchMethod.Circle,
            Effect = Effect.AOEProjectilePassiveImpact, // 对每个目标造成伤害
            LogExecutionFailure = true,
            // 🔥 添加火环爆炸特效 - 在撞击点播放
            ActorArray = [Actor.AOEImpactEffect],
        };

        // 群伤炮弹撞击效果 - 单体伤害
        _ = new GameDataEffectDamage(Effect.AOEProjectilePassiveImpact)
        {
            // 🎯 使用塔的真实攻击力属性，而不是固定数值
            Amount = (context) => 
            {
                // 从效果上下文获取施法者（塔）并转换为Unit类型
                if (context.Caster is GameCore.EntitySystem.Unit tower)
                {
                    // 获取塔的最终攻击力
                    var finalAttack = tower.GetUnitPropertyFinal(UnitProperty.AttackDamage);
                    if (finalAttack.HasValue)
                    {
                        return (float)finalAttack.Value;
                    }
                }
                return 80f; // 默认值作为后备方案
            },
            Type = DamageType.Physical,
            LogExecutionFailure = true,
        };

        // AOE炮弹完整撞击效果 - 包含范围搜索
        _ = new GameDataEffectSet(Effect.AOEProjectileCompleteEffect)
        {
            Effects = [
                new() { Link = Effect.AOEImpactAreaSearch } // 撞击时搜索300范围并造成AOE伤害
            ],
        };

        // 群伤炮弹发射效果 - 修改为单体瞄准
        _ = new GameDataEffectLaunchMissile(Effect.AOEProjectilePassiveLaunch)
        {
            Method = GameCore.Struct.EffectLaunchMissileMethod.CreateMissile,
            Missile = Unit.AOEProjectileMissile,
            LaunchHeight = static (_) => 300,
            TargetHeight = static (_) => 50,
            Speed = static (_) => 1000, // 稍快的炮弹速度
            CompleteEffect = Effect.AOEProjectileCompleteEffect, // 🎯 撞击时执行AOE搜索效果
            LogExecutionFailure = true,
        };

        // 群伤塔被动搜索效果 - 修改为单体搜索
        _ = new GameDataEffectSearch(Effect.AOEDamagePassiveSearch)
        {
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Radius = static (_) => 800, // 搜索范围
            MaxCount = static (_) => 1, // 🎯 修改为单体攻击
            Method = SearchMethod.Circle,
            Effect = Effect.AOEProjectilePassiveLaunch, // 发射单发炮弹
            LogExecutionFailure = true,
        };

        // 💥 3. 群体伤害被动技能 - 每3秒自动释放单发AOE炮弹
        _ = new GameDataAbility(Ability.AOEDamagePassive)
        {
            DisplayName = "自动AOE炮击",
            Description = "每3秒自动向最近敌人发射AOE炮弹，撞击时造成300范围伤害",
            PassivePeriod = static (_) => TimeSpan.FromSeconds(3.0),
            PassivePeriodicEffect = Effect.AOETowerPassiveWithLog,
        };
    }

    private static void CreatePenetrateTowerEffects()
    {
        // ⚡ 4. 穿透塔被动技能效果链 - 重新设计：单体瞄准 + 直线穿透
        
        // 穿透攻击被动伤害效果
        _ = new GameDataEffectDamage(Effect.PenetrateAttackPassiveDamage)
        {
            // 🎯 使用塔的真实攻击力属性，而不是固定数值
            Amount = (context) => 
            {
                // 从效果上下文获取施法者（塔）并转换为Unit类型
                if (context.Caster is GameCore.EntitySystem.Unit tower)
                {
                    // 获取塔的最终攻击力
                    var finalAttack = tower.GetUnitPropertyFinal(UnitProperty.AttackDamage);
                    if (finalAttack.HasValue)
                    {
                        return (float)finalAttack.Value;
                    }
                }
                return 100f; // 默认值作为后备方案
            },
            Type = DamageType.Physical,
            LogExecutionFailure = true,
        };

        // 🔍 穿透搜索日志效果 - 显示target和caster信息
        _ = new GameDataEffectLog(Effect.PenetrateSearchLog)
        {
            Message = context => 
            {
                var targetInfo = context.Target?.ToString() ?? "null";
                var casterInfo = context.Caster?.ToString() ?? "null";
                var targetPos = context.Target?.Position.ToString() ?? "null";
                var casterPos = context.Caster?.Position.ToString() ?? "null";
                
                return $"🗡️ 穿透塔直线搜索喵～ " +
                       $"Target: {targetInfo} at {targetPos}, " +
                       $"Caster: {casterInfo} at {casterPos}";
            },
            Level = Microsoft.Extensions.Logging.LogLevel.Information,
        };

        // 实际的直线穿透搜索效果 - 从施法者朝向目标的直线搜索
        _ = new GameDataEffectSearch(Effect.PenetrateLineSearchActual)
        {
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Height = static (_) => 2000,
            Width = static (_) => 150,   // 穿透宽度150
            MaxCount = static (_) => 8, // 最多穿透8个敌人
            Method = SearchMethod.Rectangle, // 矩形/直线搜索
            Effect = Effect.PenetrateAttackPassiveDamage, // 对每个敌人造成伤害
            LogExecutionFailure = true,
            
            // 🎯 配置搜索起始点和方向
            TargetLocation = new() { Value = GameCore.BaseType.TargetLocation.Caster }, // 从施法者位置开始
            Facing = new() // 朝向配置
            {
                Method = GameCore.Struct.EffectAngleMethod.AngleBetweenTwoPoints, // 两点间角度
                Location = new() { Value = GameCore.BaseType.TargetLocation.Caster }, // 起始点：施法者
                OtherLocation = new() { Value = GameCore.BaseType.TargetLocation.Target }, // 终点：目标敌人
            },
            
            // ⚡ 添加直线剑气特效 - 在施法者位置播放
            ActorArray = [Actor.PenetrateLineEffect],
        };

        // 直线穿透搜索效果 - 从施法者朝向目标的直线搜索（带日志包装）
        _ = new GameDataEffectSet(Effect.PenetrateLineSearch)
        {
            Effects = [
                new() { Link = Effect.PenetrateSearchLog }, // 🔍 先打印日志信息
                new() { Link = Effect.PenetrateLineSearchActual } // 然后执行真正的搜索
            ],
        };

        // 穿透塔单体目标搜索 - 先找到一个目标，然后朝向它进行直线穿透
        _ = new GameDataEffectSearch(Effect.PenetrateTargetSearch)
        {
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Radius = static (_) => 800, // 搜索范围
            MaxCount = static (_) => 1, // 🎯 搜索单个目标
            Method = SearchMethod.Circle,
            Effect = Effect.PenetrateLineSearch, // 找到目标后执行直线穿透
            LogExecutionFailure = true,
        };

        // 保留旧的搜索效果定义（向后兼容）
        _ = new GameDataEffectSearch(Effect.PenetrateAttackPassiveSearch)
        {
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Radius = static (_) => 800, // 搜索范围
            MaxCount = static (_) => 1, // 单体搜索
            Method = SearchMethod.Circle,
            Effect = Effect.PenetrateLineSearch, // 使用新的直线搜索
            LogExecutionFailure = true,
        };

        // ⚡ 4. 穿透攻击被动技能 - 每4秒自动释放穿透攻击
        _ = new GameDataAbility(Ability.PenetrateAttackPassive)
        {
            DisplayName = "自动穿透攻击",
            Description = "每4秒自动锁定敌人，释放直线穿透攻击",
            PassivePeriod = static (_) => TimeSpan.FromSeconds(4.0),
            PassivePeriodicEffect = Effect.PenetrateTowerPassiveWithLog,
        };
    }

    private static void CreateLogWrapperEffects()
    {
        // 🔊 带日志的包装效果 - 用于调试和监控
        
        // 减速塔日志包装效果
        _ = new GameDataEffectSet(Effect.SlowTowerPassiveWithLog)
        {
            Effects = [
                new() { Link = Effect.SlowProjectilePassiveSearch }
            ],
        };

        // 光环减速塔日志包装效果 - 搜索时自动播放特效
        _ = new GameDataEffectSet(Effect.AuraSlowTowerPassiveWithLog)
        {
            Effects = [
                new() { Link = Effect.AuraSlowPassiveSearch } // 执行搜索、播放特效并施加Buff
            ],
        };

        // 群伤塔日志包装效果
        _ = new GameDataEffectSet(Effect.AOETowerPassiveWithLog)
        {
            Effects = [
                new() { Link = Effect.AOEDamagePassiveSearch }
            ],
        };

        // 穿透塔日志包装效果
        _ = new GameDataEffectSet(Effect.PenetrateTowerPassiveWithLog)
        {
            Effects = [
                new() { Link = Effect.PenetrateAttackPassiveSearch }
            ],
        };
    }
}
