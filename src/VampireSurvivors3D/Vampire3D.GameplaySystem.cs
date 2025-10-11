#if SERVER
using Events;

using GameCore.Components;
using GameCore.Container;
using GameCore.Event;
using GameCore.VitalSystem;

using GameData;

using System.Numerics;

namespace GameEntry.VampireSurvivors3D;

/// <summary>
/// Vampire3D 游戏玩法系统 - 管理游戏核心逻辑
/// </summary>
public class GameplaySystem : IGameClass
{
    private static GameplaySystem? instance;
    private static readonly object lockObject = new();
    
    // 游戏状态
    private static int currentWave = 1;
    private static int enemiesKilled = 0;
    private static float gameTime = 0f;
    private static bool gameActive = false;
    
    // 配置参数
    private static readonly float WAVE_DURATION = 30f; // 每波持续30秒
    private static readonly int BASE_ENEMIES_PER_WAVE = 5;
    private static readonly float ENEMY_HEALTH_SCALING = 1.2f;
    private static readonly float ENEMY_DAMAGE_SCALING = 1.1f;

    public static GameplaySystem Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance ??= new GameplaySystem();
                }
            }
            return instance;
        }
    }

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        if (Game.GameModeLink != ScopeData.GameMode.VampireSurvivors3D)
        {
            return;
        }

        // 初始化游戏系统
        _ = Instance;
        
        // 注册游戏事件
        RegisterGameEvents();

        Game.Logger.LogInformation("🎮 Vampire3D Gameplay System initialized");
    }

    private static void RegisterGameEvents()
    {
        // 游戏开始事件
        Trigger<EventGameStart> gameStartTrigger = new(OnGameStartAsync, keepReference: true);
        gameStartTrigger.Register(Game.Instance);

        // 敌人死亡事件
        Trigger<EventEntityDeath> enemyDeathTrigger = new(OnEnemyDeathAsync, keepReference: true);
        enemyDeathTrigger.Register(Game.Instance);

        // 游戏时间更新（每秒）
        var gameTimer = new GameCore.Timers.Timer(TimeSpan.FromSeconds(1))
        {
            AutoReset = true
        };
        gameTimer.Elapsed += (_, __) => UpdateGameTime();
        gameTimer.Start();
    }

    private static async Task<bool> OnGameStartAsync(object sender, EventGameStart eventArgs)
    {
        gameActive = true;
        gameTime = 0f;
        currentWave = 1;
        enemiesKilled = 0;

        Game.Logger.LogInformation("🎯 Starting Vampire3D gameplay - Wave {wave}", currentWave);
        
        // 🆕 启动网络状态同步
        NetworkServerSync.Initialize();
        
        // 开始第一波
        await StartWave(currentWave);
        
        return true;
    }

    private static async Task<bool> OnEnemyDeathAsync(object sender, EventEntityDeath eventArgs)
    {
        if (eventArgs.Entity is Unit deadUnit && IsEnemy(deadUnit))
        {
            enemiesKilled++;
            
            // 掉落经验球
            await SpawnExperienceOrb(deadUnit.Position);
            
            // 检查是否随机掉落道具
            if (Random.Shared.NextSingle() < 0.1f) // 10% 概率掉落道具
            {
                await SpawnRandomItem(deadUnit.Position);
            }

            Game.Logger.LogInformation("💀 Enemy killed! Total kills: {kills}", enemiesKilled);
        }
        
        return true;
    }

    private static void UpdateGameTime()
    {
        if (!gameActive) return;

        gameTime += 1f;

        // 检查是否需要开始新波次
        if (gameTime % WAVE_DURATION == 0)
        {
            currentWave++;
            _ = StartWave(currentWave);
        }

        // 广播游戏状态更新
        BroadcastGameStats();
    }

    private static async Task StartWave(int waveNumber)
    {
        Game.Logger.LogInformation("🌊 Starting Wave {wave}", waveNumber);

        var scene = GameCore.SceneSystem.Scene.GetOrCreate(ExtraScopeDataVampire3D.Scene.VampireScene);
        if (scene == null) return;

        var player1 = Player.GetById(1);
        var hero = player1?.MainUnit;
        if (hero == null) return;

        // 计算这一波的敌人数量和强度
        int enemyCount = BASE_ENEMIES_PER_WAVE + (waveNumber - 1) * 2;
        float healthMultiplier = MathF.Pow(ENEMY_HEALTH_SCALING, waveNumber - 1);
        float damageMultiplier = MathF.Pow(ENEMY_DAMAGE_SCALING, waveNumber - 1);

        // 生成敌人
        for (int i = 0; i < enemyCount; i++)
        {
            await SpawnEnemy(scene, hero, healthMultiplier, damageMultiplier);
        }

        Game.Logger.LogInformation("👹 Spawned {count} enemies for wave {wave}", enemyCount, waveNumber);
    }

    private static async Task SpawnEnemy(GameCore.SceneSystem.Scene scene, Unit hero, float healthMult, float damageMult)
    {
        var enemyPlayer = Player.GetById(4)!; // 敌对玩家
        var random = Random.Shared;

        // 在英雄周围随机位置生成敌人
        float angle = random.NextSingle() * 2 * MathF.PI;
        float distance = 800f + random.NextSingle() * 400f;

        Vector3 spawnPos = new(
            hero.Position.X + MathF.Cos(angle) * distance,
            hero.Position.Y + MathF.Sin(angle) * distance,
            0
        );

        // 根据波次随机选择敌人类型
        var enemyType = GetRandomEnemyType(currentWave);
        var newEnemy = enemyType.Data?.CreateUnit(
            enemyPlayer,
            new(spawnPos, scene),
            random.Next(0, 360)
        );

        if (newEnemy != null)
        {
            // 应用属性倍数
            var healthVital = newEnemy.GetTagComponent<Vital>(PropertyVital.Health);
            if (healthVital != null)
            {
                var newMaxHealth = healthVital.Max * healthMult;
                healthVital.SetMax(newMaxHealth, PropertySubType.Base, true);
                healthVital.Set(newMaxHealth);
            }

            // 设置敌人行为
            SetupEnemyBehavior(newEnemy, hero);

            Game.Logger.LogInformation("👹 Spawned {enemy} at {pos} with {health} HP", 
                newEnemy.Cache.Name, spawnPos, healthVital?.Max ?? 0);
        }

        await Task.CompletedTask;
    }

    private static GameLink<GameCore.EntitySystem.Data.GameDataUnit, GameCore.EntitySystem.Data.GameDataUnit> GetRandomEnemyType(int wave)
    {
        var random = Random.Shared;
        
        // 根据波次增加高级敌人的概率
        if (wave >= 5 && random.NextSingle() < 0.3f)
        {
            return ExtraScopeDataVampire3D.Unit.LargeMonster;
        }
        else if (wave >= 3 && random.NextSingle() < 0.4f)
        {
            return ExtraScopeDataVampire3D.Unit.MediumMonster;
        }
        else
        {
            return ExtraScopeDataVampire3D.Unit.SmallMonster;
        }
    }

    private static void SetupEnemyBehavior(Unit enemy, Unit hero)
    {
        // 创建追击指令
        GameCore.OrderSystem.Command attackCommand = new()
        {
            Index = GameCore.OrderSystem.CommandIndex.Attack,
            Target = hero,
            Flag = GameCore.OrderSystem.CommandFlag.DoRecast,
            Player = enemy.Player
        };
        
        var result = attackCommand.IssueOrder(enemy);
        if (!result.IsSuccess)
        {
            Game.Logger.LogWarning("Failed to set enemy behavior for {enemy}", enemy.Cache.Name);
        }
    }

    private static async Task SpawnExperienceOrb(GameCore.SceneSystem.ScenePoint position)
    {
        // 创建经验球实体
        // 这里需要实现经验球的单位定义和生成逻辑
        Game.Logger.LogInformation("✨ Experience orb spawned at {pos}", position);
        await Task.CompletedTask;
    }

    private static async Task SpawnRandomItem(GameCore.SceneSystem.ScenePoint position)
    {
        // 随机掉落道具
        var itemTypes = new[] { "HealthPotion", "DamageBoost", "SpeedBoost" };
        var itemType = itemTypes[Random.Shared.Next(itemTypes.Length)];
        
        Game.Logger.LogInformation("🎁 Item dropped: {item} at {pos}", itemType, position);
        await Task.CompletedTask;
    }

    private static bool IsEnemy(Unit unit)
    {
        // 检查单位是否为敌人（团队2）
        return unit.Player.Team == Team.GetById(2);
    }

    private static void BroadcastGameStats()
    {
        // 广播游戏统计信息到客户端
        var stats = new GameStats
        {
            Wave = currentWave,
            EnemiesKilled = enemiesKilled,
            GameTime = TimeSpan.FromSeconds(gameTime)
        };

        Game.Logger.LogInformation("📊 Game Stats - Wave: {wave}, Kills: {kills}, Time: {time}", 
            stats.Wave, stats.EnemiesKilled, stats.GameTime.ToString(@"mm\:ss"));
    }

    /// <summary>
    /// 游戏统计信息结构
    /// </summary>
    public struct GameStats
    {
        public int Wave { get; set; }
        public int EnemiesKilled { get; set; }
        public TimeSpan GameTime { get; set; }
        public int Level { get; set; }
        public float Experience { get; set; }
        public float ExperienceRequired { get; set; }
    }

    /// <summary>
    /// 获取当前游戏统计数据
    /// </summary>
    public static GameStats GetCurrentStats()
    {
        return new GameStats
        {
            Wave = currentWave,
            EnemiesKilled = enemiesKilled,
            GameTime = TimeSpan.FromSeconds(gameTime),
            Level = ExperienceSystem.GetPlayerLevel(1),  // 假设玩家ID为1
            Experience = ExperienceSystem.GetPlayerExperience(1),
            ExperienceRequired = ExperienceSystem.GetExperienceRequired(ExperienceSystem.GetPlayerLevel(1))
        };
    }

    /// <summary>
    /// 经验值系统
    /// </summary>
    public static class ExperienceSystem
    {
        private static readonly Dictionary<int, int> playerExperience = new();
        private static readonly Dictionary<int, int> playerLevel = new();

        public static void AddExperience(int playerId, int amount)
        {
            if (!playerExperience.ContainsKey(playerId))
            {
                playerExperience[playerId] = 0;
                playerLevel[playerId] = 1;
            }

            playerExperience[playerId] += amount;
            
            // 检查升级
            int currentLevel = playerLevel[playerId];
            int expRequired = GetExperienceRequired(currentLevel);
            
            if (playerExperience[playerId] >= expRequired)
            {
                LevelUp(playerId);
            }
        }

        public static void LevelUp(int playerId)
        {
            playerLevel[playerId]++;
            int newLevel = playerLevel[playerId];
            
            Game.Logger.LogInformation("🎊 Player {player} leveled up to level {level}!", playerId, newLevel);
            
            // 🆕 向客户端广播升级事件
            NetworkServerSync.BroadcastLevelUpOverNetwork(playerId, newLevel, playerExperience[playerId]);
            
            // 触发升级选择事件
            _ = ShowUpgradeOptions(playerId);
        }

        public static int GetExperienceRequired(int level)
        {
            // 每级所需经验递增
            return level * 100;
        }

        public static int GetPlayerLevel(int playerId)
        {
            return playerLevel.ContainsKey(playerId) ? playerLevel[playerId] : 1;
        }

        public static float GetPlayerExperience(int playerId)
        {
            return playerExperience.ContainsKey(playerId) ? playerExperience[playerId] : 0;
        }

        private static async Task ShowUpgradeOptions(int playerId)
        {
            // 显示升级选项（需要与客户端UI配合）
            Game.Logger.LogInformation("Showing upgrade options for player {player}", playerId);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// 道具掉落系统
    /// </summary>
    public static class LootSystem
    {
        public static async Task<bool> RollForLoot(GameCore.SceneSystem.ScenePoint position, float dropChance = 0.1f)
        {
            if (Random.Shared.NextSingle() < dropChance)
            {
                await SpawnRandomLoot(position);
                return true;
            }
            return false;
        }

        private static async Task SpawnRandomLoot(GameCore.SceneSystem.ScenePoint position)
        {
            var lootTypes = new[]
            {
                "HealthPotion",
                "ManaPotion", 
                "DamageScroll",
                "SpeedBoots",
                "GoldCoin"
            };

            var lootType = lootTypes[Random.Shared.Next(lootTypes.Length)];
            Game.Logger.LogInformation("💎 Loot spawned: {loot} at {pos}", lootType, position);
            
            await Task.CompletedTask;
        }
    }
}
#endif 