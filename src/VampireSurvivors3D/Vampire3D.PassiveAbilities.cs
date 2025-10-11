using GameCore.AbilitySystem.Data;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.CooldownSystem.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.Execution.Data;
using GameCore.Execution.Data.Enum;
using GameCore.ResourceType.Data;
using GameCore.Struct;
using GameCore.VitalSystem;

using GameData;

using static GameCore.ScopeData;

namespace GameEntry.VampireSurvivors3D;

/// <summary>
/// Vampire3D被动技能系统 - 定义各种自动释放的技能效果
/// </summary>
public class PassiveAbilities : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization += OnGameDataInitialization;
        
        // 🆕 注册新系统
#if SERVER
        GameplaySystem.OnRegisterGameClass();
        NetworkSyncTest.OnRegisterGameClass(); // 网络同步测试
#endif
#if CLIENT
        NetworkClientSync.OnRegisterGameClass();
#endif
    }

    // 技能定义
    public static class Ability
    {
        public static readonly GameLink<GameDataAbility, GameDataAbility> FireballAura = new("Vampire3D_FireballAura"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbility> LightningChain = new("Vampire3D_LightningChain"u8);
        public static readonly GameLink<GameDataAbility, GameDataAbility> HealingAura = new("Vampire3D_HealingAura"u8);
    }

    // 效果定义
    public static class Effect
    {
        // 火球术相关效果
        public static readonly GameLink<GameDataEffect, GameDataEffectSearch> FireballSearch = new("Vampire3D_FireballSearch"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectLaunchMissile> FireballLaunch = new("Vampire3D_FireballLaunch"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> FireballDamage = new("Vampire3D_FireballDamage"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectSet> FireballImpact = new("Vampire3D_FireballImpact"u8);

        // 闪电链相关效果
        public static readonly GameLink<GameDataEffect, GameDataEffectSearch> LightningSearch = new("Vampire3D_LightningSearch"u8);
        public static readonly GameLink<GameDataEffect, GameDataEffectDamage> LightningDamage = new("Vampire3D_LightningDamage"u8);

        // 治疗效果
        public static readonly GameLink<GameDataEffect, GameDataEffectUnitModifyVital> HealingEffect = new("Vampire3D_HealingEffect"u8);
    }

    // 单位定义
    public static class Unit
    {
        public static readonly GameLink<GameDataUnit, GameDataUnit> FireballMissile = new("Vampire3D_FireballMissile"u8);
    }

    // 粒子效果定义
    public static class Particle
    {
        public static readonly GameLink<GameDataParticle, GameDataParticle> FireballExplosion = new("Vampire3D_FireballExplosion"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> LightningChainBeam = new("Vampire3D_LightningChainBeam"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> LightningChainImpact = new("Vampire3D_LightningChainImpact"u8);
        public static readonly GameLink<GameDataParticle, GameDataParticle> HealingImpact = new("Vampire3D_HealingImpact"u8);
    }

    // Actor定义
    public static class Actor
    {
        public static readonly GameLink<GameDataActor, GameDataActorParticle> FireballExplosion = new("Vampire3D_FireballExplosion"u8);
        public static readonly GameLink<GameDataActor, GameDataActorBeam> LightningChainBeam = new("Vampire3D_LightningChainBeam"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> LightningChainImpact = new("Vampire3D_LightningChainImpact"u8);
        public static readonly GameLink<GameDataActor, GameDataActorParticle> HealingImpact = new("Vampire3D_HealingImpact"u8);
    }

    private static void OnGameDataInitialization()
    {
        // 只在Vampire3D模式下初始化
        if (Game.GameModeLink != ScopeData.GameMode.VampireSurvivors3D)
        {
            return;
        }

        Game.Logger.LogInformation("🧙 Initializing Vampire3D Passive Abilities System...");

        CreateParticleEffects();
        CreateActors();
        CreateMissileUnits();
        CreateEffects();
        CreateAbilities();

        Game.Logger.LogInformation("✨ Vampire3D Passive Abilities System initialized successfully!");
    }

    private static void CreateParticleEffects()
    {
        // 火球爆炸效果
        _ = new GameDataParticle(Particle.FireballExplosion)
        {
            Asset = "effect/eff_tongyong/huoqiu_blast/particle.effect"u8,
            Radius = 128
        };

        // 闪电链光束效果
        _ = new GameDataParticle(Particle.LightningChainBeam)
        {
            Asset = "effect/eff_xujian/effect_light_000/particle.effect"u8,
            Radius = 128
        };

        // 闪电链命中效果
        _ = new GameDataParticle(Particle.LightningChainImpact)
        {
            Asset = "effect/effect_new1/effect_hit/eff_hit_02/particle.effect"u8,
            Radius = 128
        };

        // 治疗效果
        _ = new GameDataParticle(Particle.HealingImpact)
        {
            Asset = "effect/samplespells/healing/healingaura_heal/particle.effect"u8,
            Radius = 128
        };
    }

    private static void CreateActors()
    {
        // 火球爆炸Actor
        _ = new GameDataActorParticle(Actor.FireballExplosion)
        {
            AutoPlay = true,
            Particle = Particle.FireballExplosion,
            KillOnFinish = true,
            ForceOneShot = true,
        };

        // 闪电链Actor
        _ = new GameDataActorBeam(Actor.LightningChainBeam)
        {
            AutoPlay = true,
            Beam = Particle.LightningChainBeam,
            Duration = TimeSpan.FromSeconds(0.5),
            LaunchSocket = "socket_magic_weapon"u8,
            ImpactSocket = "socket_hit"u8,
        };

        // 闪电链命中Actor
        _ = new GameDataActorParticle(Actor.LightningChainImpact)
        {
            AutoPlay = true,
            Particle = Particle.LightningChainImpact,
            KillOnFinish = true,
            ForceOneShot = true,
            Scale = 1,
        };

        // 治疗效果
        _ = new GameDataActorParticle(Actor.HealingImpact)
        {
            AutoPlay = true,
            Particle = Particle.HealingImpact,
            KillOnFinish = true,
            ForceOneShot = true,
        };
    }

    private static void CreateMissileUnits()
    {
        // 火球投射物
        _ = new GameDataUnit(Unit.FireballMissile)
        {
            Name = "火球",
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
    }

    private static void CreateEffects()
    {
        CreateFireballEffects();
        CreateLightningEffects();
        CreateHealingEffects();
    }

    private static void CreateFireballEffects()
    {
        // 火球伤害效果
        _ = new GameDataEffectDamage(Effect.FireballDamage)
        {
            Amount = static (_) => 150, // 火球伤害
            Type = DamageType.Physical, // 使用物理伤害
            LogExecutionFailure = true,
        };

        // 火球撞击效果组合
        _ = new GameDataEffectSet(Effect.FireballImpact)
        {
            Effects = [
                new() { Link = Effect.FireballDamage }
            ],
            SetFlags = new() { PreferUnique = true },
        };

        // 火球发射效果
        _ = new GameDataEffectLaunchMissile(Effect.FireballLaunch)
        {
            Method = EffectLaunchMissileMethod.CreateMissile,
            Missile = Unit.FireballMissile,
            DoImpactEntity = true,
            DoStaticCollision = true,
            ImpactMaxCount = static (_) => 1,
            ImpactSearchRadius = static (_) => 32,
            Speed = static (_) => 800,
            ImpactEffect = Effect.FireballImpact,
            ImpactSearchFilter = [
                new() {
                    Required = [UnitRelationship.Enemy],
                    Excluded = [UnitState.Dead, UnitState.Invulnerable]
                }
            ],
            ImpactActors = [Actor.FireballExplosion],
            LaunchHeight = (_) => 80,
        };

        // 火球搜索效果
        _ = new GameDataEffectSearch(Effect.FireballSearch)
        {
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Radius = static (_) => 600, // 搜索范围
            MaxCount = static (_) => 3, // 最多选择3个目标
            Method = SearchMethod.Circle,
            Effect = Effect.FireballLaunch,
            LogExecutionFailure = true,
        };
    }

    private static void CreateLightningEffects()
    {
        // 闪电伤害效果
        _ = new GameDataEffectDamage(Effect.LightningDamage)
        {
            Amount = static (_) => 200, // 闪电伤害
            Type = DamageType.Physical,
            LogExecutionFailure = true,
            ActorArray = [Actor.LightningChainBeam, Actor.LightningChainImpact],
        };

        // 闪电链搜索效果
        _ = new GameDataEffectSearch(Effect.LightningSearch)
        {
            SearchFilters = [new() {
                Required = [UnitRelationship.Enemy],
                Excluded = [UnitState.Dead, UnitState.Invulnerable]
            }],
            Radius = static (_) => 400, // 闪电链范围
            MaxCount = static (_) => 5, // 最多连击5个目标
            Method = SearchMethod.Circle,
            Effect = Effect.LightningDamage,
            LogExecutionFailure = true,
        };
    }

    private static void CreateHealingEffects()
    {
        // 真正的治疗效果
        _ = new GameDataEffectUnitModifyVital(Effect.HealingEffect)
        {
            Modification = [
                new() {
                    Property = PropertyVital.Health,
                    Value = static (_) => 50, // 治疗50点生命值
                }
            ],
            Operation = PropertyModificationOperation.Add,
            ActorArray = [Actor.HealingImpact],
            LogExecutionFailure = true,
        };
    }

    private static void CreateAbilities()
    {
        Game.Logger.LogInformation("🔥 Creating passive abilities...");

        // 火球光环 - 每2.5秒释放一次
        _ = new GameDataAbility(Ability.FireballAura)
        {
            DisplayName = "火球术",
            Description = "每2.5秒自动向附近敌人发射火球",
            PassivePeriod = static (_) => TimeSpan.FromSeconds(2.5),
            PassivePeriodicEffect = Effect.FireballSearch,
        };
        Game.Logger.LogInformation("🔥 Created FireballAura: {ability}", Ability.FireballAura);

        // 闪电链 - 每4秒释放一次
        _ = new GameDataAbility(Ability.LightningChain)
        {
            DisplayName = "闪电链",
            Description = "每4秒释放闪电链，连击多个敌人",
            PassivePeriod = static (_) => TimeSpan.FromSeconds(4.0),
            PassivePeriodicEffect = Effect.LightningSearch,
        };
        Game.Logger.LogInformation("⚡ Created LightningChain: {ability}", Ability.LightningChain);

        // 治疗光环 - 每3秒恢复生命值
        _ = new GameDataAbility(Ability.HealingAura)
        {
            DisplayName = "治疗光环",
            Description = "每3秒恢复生命值",
            PassivePeriod = static (_) => TimeSpan.FromSeconds(3.0),
            PassivePeriodicEffect = Effect.HealingEffect,
        };
        Game.Logger.LogInformation("💚 Created HealingAura: {ability}", Ability.HealingAura);

        Game.Logger.LogInformation("✅ All passive abilities created successfully!");
    }


}