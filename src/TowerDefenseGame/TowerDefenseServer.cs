#if SERVER
using Events;
using GameCore;
using GameCore.Event;
using GameCore.SceneSystem;
using GameCore.Components;
using GameCore.Container;
using GameCore.ProtocolServerTransient;
using GameCore.EntitySystem;
using GameCore.PlayerAndUsers;
using GameEntry.TowerDefenseGame.SpawnSystem;
using GameData;
using System.Numerics;

namespace GameEntry.TowerDefenseGame;

/// <summary>
/// 塔防游戏服务端逻辑
/// </summary>
internal class TowerDefenseServer : IGameClass
{
    // 游戏状态
    private static bool _gameStarted = false;
    private static Scene? _currentScene = null;
    private static DateTime _gameStartTime;
    private static DateTime _lastMonsterSpawnTime;
    private static DateTime _lastGameStateUpdateTime;
    private static int _monstersSpawned = 0;
    private static int _currentWave = 0;
    
    // 玩家资源
    private static int _playerHealth = 20; // 玩家血量，默认20点
    private static int _playerGold = 10;   // 玩家金币，默认10个
    
    // 怪物管理
    private static List<Unit> _spawnedMonsters = new();
    
    // 建筑管理
    private static List<Unit> _placedBuildings = new();
    private static Dictionary<Unit, ImprovedPathFollower> _monsterPathFollowers = new();
    
    // 新的刷怪管理系统
    private static SpawnManager? _spawnManager = null;
    
    // 游戏状态更新配置
    private static readonly TimeSpan GAME_STATE_UPDATE_INTERVAL = TimeSpan.FromSeconds(1); // 每秒更新游戏状态
    
    // 触发器
    private static Trigger<EventGameStart>? gameStartTrigger;
    private static Trigger<EventGameTick>? gameTickTrigger;
    private static Trigger<EventEntityDeath>? entityDeathTrigger;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        // 如果游戏模式不是塔防，则不注册
        if (Game.GameModeLink != ScopeData.GameMode.TowerDefense)
        {
            return;
        }

        Game.Logger.LogInformation("🏰 Tower Defense Server - registering triggers...");
        
        // 注册游戏开始触发器
        gameStartTrigger = new Trigger<EventGameStart>(TowerDefenseGameStartAsync);
        gameStartTrigger.Register(Game.Instance);
        
        // 注册游戏Tick触发器用于定时器功能
        gameTickTrigger = new Trigger<EventGameTick>(OnGameTick);
        gameTickTrigger.Register(Game.Instance);
        
        // 注册实体死亡触发器监听怪物死亡
        entityDeathTrigger = new Trigger<EventEntityDeath>(OnEntityDeath);
        entityDeathTrigger.Register(Game.Instance);
        
        Game.Logger.LogInformation("✅ Tower Defense Server triggers registered");
    }



    private static async Task<bool> TowerDefenseGameStartAsync(object sender, EventGameStart eventArgs)
    {
        Game.Logger.LogInformation("🚀 Tower Defense Game Start!");
        
        // 获取塔防场景
        var scene = Scene.GetOrCreate(ScopeData.Scene.TowerDefenseScene);
        if (scene == null)
        {
            Game.Logger.LogError("❌ Tower Defense scene not found");
            return false;
        }

        // 确保场景已加载
        if (!scene.Loaded)
        {
            Game.Logger.LogInformation("🏰 Loading Tower Defense scene...");
            _ = scene.Load();
        }

        _currentScene = scene;
        Game.Logger.LogInformation("🏰 Current scene: {scene} (Loaded: {loaded})", scene.Name, scene.Loaded);

        // 获取玩家
        var player1 = Player.GetById(1);
        if (player1 == null)
        {
            Game.Logger.LogError("❌ Player 1 not found");
            return false;
        }

        // 查找场景中预置的塔防英雄单位（参考TestHero做法）
        var hero = player1.MainUnit;
        
        if (hero != null)
        {
            Game.Logger.LogInformation("🏰 Tower Defense Hero found in scene: {unit} {name}, Position: {pos}", 
                hero, hero.Cache.Name, hero.Position);
            
            Game.Logger.LogInformation("🛠️ 塔防英雄已装备四种建造技能：减速塔、光环塔、爆炸塔、穿透塔");
        }
        else
        {
            Game.Logger.LogError("❌ Tower Defense Hero not found in scene - please check scene configuration");
            return false;
        }

        Game.Logger.LogInformation("✅ Tower Defense Game Setup Complete!");
        return true;
    }

    /// <summary>
    /// 根据SyncId查找建筑单位
    /// </summary>
    public static Unit? FindBuildingById(int buildingId)
    {
        try
        {
            return _placedBuildings.FirstOrDefault(b => b.SyncId == buildingId);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error finding building by SyncId: {BuildingId}", buildingId);
            return null;
        }
    }

    /// <summary>
    /// 注册建筑到塔防系统（由BuildingEffect调用）
    /// </summary>
    public static void RegisterBuilding(Unit building)
    {
        if (building == null) return;
        
        try
        {
            _placedBuildings.Add(building);
            Game.Logger.LogInformation("🏗️ 建筑已注册到塔防系统: {Building}, 总建筑数: {Count}, 建筑SyncId: {SyncId}", 
                                     building.Cache.Name, _placedBuildings.Count, building.SyncId);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 注册建筑失败");
        }
    }



    /// <summary>
    /// 游戏Tick事件处理，用于替代Timer
    /// </summary>
    private static Task<bool> OnGameTick(object sender, EventGameTick eventArgs)
    {
        if (!_gameStarted)
        {
            return Task.FromResult(true);
        }

        var now = DateTime.UtcNow;

        // 更新新的刷怪管理系统
        _spawnManager?.Update();

        // 检查游戏状态更新时间
        if (now - _lastGameStateUpdateTime >= GAME_STATE_UPDATE_INTERVAL)
        {
            SendGameStateToAllPlayers();
            _lastGameStateUpdateTime = now;
        }

        // 更新所有怪物的路径跟随状态
        UpdateMonsterPathFollowing();

        return Task.FromResult(true);
    }
    
    /// <summary>
    /// 处理实体死亡事件 - 当怪物被击杀时给予金币奖励
    /// </summary>
    private static Task<bool> OnEntityDeath(object sender, EventEntityDeath eventArgs)
    {
        try
        {
            // 检查游戏是否正在进行
            if (!_gameStarted)
            {
                return Task.FromResult(true);
            }
            
            // 检查死亡的是否为怪物单位
            if (eventArgs.Entity is Unit deadUnit && _spawnedMonsters.Contains(deadUnit))
            {
                // 只有当怪物是被其他单位击杀时才给予奖励（不是到达终点死亡）
                if (eventArgs.DeathType != DeathType.Destroy && eventArgs.KillerUnit != null)
                {
                    // 给玩家增加1金币
                    _playerGold += 1;
                    
                    Game.Logger.LogInformation("💰 Monster killed! Player earned 1 gold. Current gold: {gold}", _playerGold);
                    
                    // 从怪物列表中移除（如果还在的话）
                    _spawnedMonsters.Remove(deadUnit);
                    
                    // 清理路径跟随器
                    if (_monsterPathFollowers.ContainsKey(deadUnit))
                    {
                        _monsterPathFollowers[deadUnit].Stop();
                        _monsterPathFollowers.Remove(deadUnit);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling entity death event");
        }
        
        return Task.FromResult(true);
    }

    /// <summary>
    /// 启动塔防游戏
    /// </summary>
    public static void StartGame()
    {
        if (_gameStarted)
        {
            Game.Logger.LogWarning("⚠️ Game already started");
            return;
        }

        // 首先初始化场景
        Game.Logger.LogInformation("🎯 Initializing scene...");
        if (!InitializeScene())
        {
            Game.Logger.LogError("❌ Failed to initialize scene, aborting game start");
            return;
        }

        _gameStarted = true;
        _gameStartTime = DateTime.UtcNow;
        _lastMonsterSpawnTime = DateTime.UtcNow;
        _lastGameStateUpdateTime = DateTime.UtcNow;
        _monstersSpawned = 0;
        _currentWave = 1;
        _playerHealth = 20; // 重置玩家血量
        _playerGold = 10;   // 重置玩家金币
        
        Game.Logger.LogInformation("🚀 Tower Defense game started! Using EventGameTick for timing.");
    }

    /// <summary>
    /// 初始化塔防场景
    /// </summary>
    private static bool InitializeScene()
    {
        Game.Logger.LogInformation("🚀 Tower Defense Scene Initialize!");
        
        // 获取塔防场景
        var scene = Scene.GetOrCreate(ScopeData.Scene.TowerDefenseScene);
        if (scene == null)
        {
            Game.Logger.LogError("❌ Tower Defense scene not found");
            return false;
        }

        // 确保场景已加载
        if (!scene.Loaded)
        {
            Game.Logger.LogInformation("🏰 Loading Tower Defense scene...");
            _ = scene.Load();
        }

        _currentScene = scene;
        Game.Logger.LogInformation("🏰 Current scene: {scene} (Loaded: {loaded})", scene.Name, scene.Loaded);

        // 获取玩家
        var player1 = Player.GetById(1);
        if (player1 == null)
        {
            Game.Logger.LogError("❌ Player 1 not found");
            return false;
        }

        // 查找场景中预置的塔防英雄单位（参考TestHero做法）
        var hero = player1.MainUnit;
        
        if (hero != null)
        {
            Game.Logger.LogInformation("🏰 Tower Defense Hero found in scene: {unit} {name}, Position: {pos}", 
                hero, hero.Cache.Name, hero.Position);
            
            Game.Logger.LogInformation("🛠️ 塔防英雄已装备四种建造技能：减速塔、光环塔、爆炸塔、穿透塔");
        }
        else
        {
            Game.Logger.LogError("❌ Tower Defense Hero not found in scene - please check scene configuration");
            return false;
        }

        // 初始化刷怪管理器
        InitializeSpawnManager(scene);

        Game.Logger.LogInformation("✅ Tower Defense Scene Setup Complete!");
        return true;
     }

    /// <summary>
    /// 初始化刷怪管理器
    /// </summary>
    private static void InitializeSpawnManager(Scene scene)
    {
        try
        {
            // 获取敌方玩家用于创建怪物单位 (参考吸血鬼3D配置：Player 4 = Team 2)
            var monsterPlayer = Player.GetById(4);
            if (monsterPlayer == null)
            {
                Game.Logger.LogError("❌ Monster player (ID:4) not found - cannot initialize spawn manager");
                return;
            }

            // 创建刷怪管理器
            _spawnManager = new SpawnManager(scene, monsterPlayer);
            
            // 订阅事件
            _spawnManager.MonsterSpawned += OnMonsterSpawned;
            _spawnManager.WaveStarted += OnWaveStarted;
            _spawnManager.WaveCompleted += OnWaveCompleted;
            _spawnManager.LevelCompleted += OnLevelCompleted;
            _spawnManager.LevelLooped += OnLevelLooped;

            // 启动默认关卡
            var defaultLevel = ScopeData.Level.DefaultLevel;
            var defaultLevelData = defaultLevel.Data;
            if (defaultLevelData != null)
            {
                if (_spawnManager.StartLevel(defaultLevel))
                {
                    Game.Logger.LogInformation("🏁 Started default level: {LevelName}", defaultLevelData.LevelName);
                }
                else
                {
                    Game.Logger.LogError("❌ Failed to start default level");
                }
            }
            else
            {
                Game.Logger.LogWarning("⚠️ No default level configured, spawn system will not start");
            }

            Game.Logger.LogInformation("✅ Spawn manager initialized successfully");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to initialize spawn manager");
        }
    }

    /// <summary>
    /// 处理怪物生成事件
    /// </summary>
    private static void OnMonsterSpawned(Unit monster)
    {
        try
        {
            // 将怪物添加到管理列表
            _spawnedMonsters.Add(monster);
            _monstersSpawned++;
            
            // 为怪物创建路径跟随器（使用现有的设置方法）
            SetupMonsterPathFollowing(monster);
            
            Game.Logger.LogDebug("👹 Monster spawned via SpawnManager: {MonsterName} (Total: {Total})", 
                monster.Cache.Name, _monstersSpawned);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling monster spawned event");
        }
    }

    /// <summary>
    /// 处理波次开始事件
    /// </summary>
    private static void OnWaveStarted(int waveNumber)
    {
        _currentWave = waveNumber;
        Game.Logger.LogInformation("🌊 Wave {WaveNumber} started via SpawnManager", waveNumber);
    }

    /// <summary>
    /// 处理波次完成事件
    /// </summary>
    private static void OnWaveCompleted(int waveNumber)
    {
        Game.Logger.LogInformation("✅ Wave {WaveNumber} completed via SpawnManager", waveNumber);
    }

    /// <summary>
    /// 处理关卡完成事件
    /// </summary>
    private static void OnLevelCompleted()
    {
        Game.Logger.LogInformation("🏆 Level completed via SpawnManager! All waves finished.");
        // 这里可以添加关卡完成的逻辑，比如显示胜利界面等
    }

    /// <summary>
    /// 处理关卡循环事件
    /// </summary>
    private static void OnLevelLooped(int loopCount)
    {
        Game.Logger.LogInformation("🔄 Level loop {LoopCount} completed! Starting next wave cycle...", loopCount);
        // 可以在这里添加循环相关的逻辑，比如：
        // - 增加怪物强度
        // - 给予玩家额外奖励
        // - 调整游戏难度等
    }

    /// <summary>
    /// 🎯 获取怪物生成位置 - 动态从路径系统获取第一个点
    /// </summary>
    private static Vector3? GetMonsterSpawnPosition()
    {
        try
        {
            var path = PathSystem.GetPath("玩家1前进路线");
            if (path != null && path.Count > 0)
            {
                var firstPoint = path.GetPoint(0);
                return firstPoint.Position;
            }
            else
            {
                Game.Logger.LogError("❌ 无法获取怪物生成位置：路径不存在或为空");
                return null;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 获取怪物生成位置时发生异常");
            return null;
        }
    }

    /// <summary>
    /// 生成一个怪物
    /// </summary>
    private static void SpawnMonster()
    {
        if (_currentScene == null)
        {
            Game.Logger.LogWarning("⚠️ Cannot spawn monster: no current scene");
            return;
        }

        if (!_currentScene.Loaded)
        {
            Game.Logger.LogWarning("⚠️ Cannot spawn monster: scene not loaded yet");
            return;
        }

        // 🎯 获取动态生成位置
        var spawnPosition = GetMonsterSpawnPosition();
        if (!spawnPosition.HasValue)
        {
            Game.Logger.LogError("❌ 无法获取怪物生成位置，跳过本次生成");
            return;
        }

        // Game.Logger.LogInformation("👹 Spawning monster #{number} at position {position} (Wave {wave})", 
            // _monstersSpawned + 1, spawnPosition.Value, _currentWave);

        try
        {
            // 获取敌方玩家用于创建怪物单位 (参考吸血鬼3D配置：Player 4 = Team 2)
            var monsterPlayer = Player.GetById(4);
            if (monsterPlayer == null)
            {
                Game.Logger.LogError("❌ Monster player (ID:4) not found");
                return;
            }

            // 创建怪物单位（使用专门的塔防怪物模板，移动速度较慢）
            var monster = ScopeData.Unit.TDMonster.Data?.CreateUnit(
                monsterPlayer,
                new ScenePoint(spawnPosition.Value, _currentScene),
                0);

            if (monster != null)
            {
                // 将怪物添加到管理列表
                _spawnedMonsters.Add(monster);
                _monstersSpawned++;
                
                // 🗺️ 为怪物创建路径跟随器
                SetupMonsterPathFollowing(monster);
                
                // Game.Logger.LogInformation("✅ Monster #{number} created successfully at {position}! (Wave {wave})", 
                    // _monstersSpawned, spawnPosition.Value, _currentWave);
                
                // 每10个怪物进入下一波
                if (_monstersSpawned % 10 == 0)
                {
                    _currentWave++;
                    // Game.Logger.LogInformation("🌊 Wave {wave} started! Total monsters spawned: {total}", _currentWave, _monstersSpawned);
                }
            }
            else
            {
                Game.Logger.LogError("❌ Failed to create monster unit");
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Exception while spawning monster");
        }
    }





    /// <summary>
    /// 向所有玩家发送游戏状态更新
    /// </summary>
    private static void SendGameStateToAllPlayers()
    {
        try
        {
            // 从SpawnManager获取进度信息（如果可用）
            var (currentWave, totalWaves, isCompleted) = _spawnManager?.GetProgress() ?? (_currentWave, 0, false);
            
            var gameInfo = new TowerDefenseGameInfo
            {
                GameState = isCompleted ? TowerDefenseGameState.GameOver : TowerDefenseGameState.Playing,
                IsSuccess = true,
                ResultCode = 0,
                MonstersSpawned = _monstersSpawned,
                WaveNumber = currentWave,
                ElapsedTime = ElapsedTime,
                PlayerHealth = _playerHealth,
                PlayerGold = _playerGold
            };

            var result = new ProtoTowerDefenseResult
            {
                GameInfo = gameInfo
            };

            // 向所有在线玩家发送游戏状态更新
            // TODO: 这里需要找到正确的广播方法
            // result.SendToAllPlayers();
            
            // 暂时只发送给Player 1
            var player1 = Player.GetById(1);
            if (player1 != null)
            {
                result.SendTo(player1, null);
            }

            // Game.Logger.LogDebug("📡 Game state update sent: Wave {wave}, Monsters {monsters}, Time {time:F1}s", 
            //     _currentWave, _monstersSpawned, ElapsedTime);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to send game state to players");
        }
    }



    /// <summary>
    /// 停止游戏
    /// </summary>
    public static void StopGame()
    {
        if (!_gameStarted)
        {
            return;
        }

        _gameStarted = false;
        
        // 清理所有生成的怪物
        ClearAllMonsters();
        
        Game.Logger.LogInformation("🛑 Tower Defense game stopped after {time:F1}s, {monsters} monsters spawned, wave {wave}", 
            ElapsedTime, _monstersSpawned, _currentWave);
    }

    /// <summary>
    /// 🗺️ 为怪物设置路径跟随
    /// </summary>
    private static void SetupMonsterPathFollowing(Unit monster)
    {
        try
        {
            // 获取预定义的路径
            var path = PathSystem.GetPath("玩家1前进路线");
            if (path == null)
            {
                Game.Logger.LogError("❌ Failed to find path: 玩家1前进路线");
                return;
            }

            // 🆕 创建改进版路径跟随器 - 使用底层Command系统获得更好的控制
            var pathFollower = new ImprovedPathFollower(monster, path, 100f); // 100单位的到达阈值，确保能到达
            
            // 设置事件处理
            pathFollower.OnPointReached += OnMonsterReachedPoint;
            pathFollower.OnPathCompleted += OnMonsterPathCompleted;
            
            // 存储路径跟随器
            _monsterPathFollowers[monster] = pathFollower;
            
            // 开始跟随路径
            if (pathFollower.StartFollowing())
            {
                // Game.Logger.LogInformation("🚶‍♂️ Monster {monster} started following path", monster.Cache?.Name ?? "Unknown");
            }
            else
            {
                Game.Logger.LogError("❌ Failed to start path following for monster {monster}", monster.Cache?.Name ?? "Unknown");
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to setup path following for monster");
        }
    }

    /// <summary>
    /// 🔄 更新怪物路径跟随状态
    /// </summary>
    private static void UpdateMonsterPathFollowing()
    {
        try
        {
            // 更新所有活跃的路径跟随器
            var followersToRemove = new List<Unit>();
            
            foreach (var kvp in _monsterPathFollowers.ToList())
            {
                var monster = kvp.Key;
                var pathFollower = kvp.Value;
                
                // 检查怪物是否还存在 (这种情况理论上不应该发生，但为安全起见检查)
                if (monster == null)
                {
                    // 跳过null值，不添加到移除列表
                    continue;
                }
                
                // 检查怪物是否已从活跃列表中移除
                if (!_spawnedMonsters.Contains(monster))
                {
                    followersToRemove.Add(monster);
                    continue;
                }
                
                // 更新路径跟随状态
                pathFollower.Update();
            }
            
            // 清理已销毁怪物的路径跟随器
            foreach (var monster in followersToRemove)
            {
                if (_monsterPathFollowers.TryGetValue(monster, out var follower))
                {
                    follower.Stop();
                    _monsterPathFollowers.Remove(monster);
                }
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to update monster path following");
        }
    }

    /// <summary>
    /// 📍 怪物到达路径点事件处理
    /// </summary>
    private static void OnMonsterReachedPoint(ImprovedPathFollower follower, int pointIndex)
    {
        // Game.Logger.LogDebug("📍 Monster reached point {point} - {status}", pointIndex, follower.GetOrderStatus());
    }

    /// <summary>
    /// 🏁 怪物完成路径事件处理
    /// </summary>
    private static void OnMonsterPathCompleted(ImprovedPathFollower follower)
    {
        Game.Logger.LogInformation("🏁 Monster completed path and reached the end! Progress: {progress:P}", follower.GetProgress());
        
        try
        {
            // 扣除玩家生命值
            _playerHealth = Math.Max(0, _playerHealth - 1);
            Game.Logger.LogInformation("💔 玩家失去1点血量！当前血量: {health}/20", _playerHealth);
            
            // 获取怪物单位并删除
            var monster = follower.Unit;
            if (monster != null && _spawnedMonsters.Contains(monster))
            {
                _spawnedMonsters.Remove(monster);
                monster.Destroy();
                Game.Logger.LogInformation("🗑️ 怪物已删除: {monster}", monster.Cache?.Name ?? "Unknown");
            }
            
            // 清理路径跟随器
            if (monster != null && _monsterPathFollowers.ContainsKey(monster))
            {
                _monsterPathFollowers.Remove(monster);
            }
            
            // 检查游戏结束条件
            if (_playerHealth <= 0)
            {
                Game.Logger.LogInformation("💀 玩家血量为0，游戏结束！");
                // TODO: 触发游戏结束逻辑
                StopGame();
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 处理怪物到达终点时发生错误");
        }
    }

    /// <summary>
    /// 清理所有生成的怪物
    /// </summary>
    private static void ClearAllMonsters()
    {
        try
        {
            Game.Logger.LogInformation("🗑️ Clearing {count} spawned monsters...", _spawnedMonsters.Count);
            
            foreach (var monster in _spawnedMonsters.ToList())
            {
                try
                {
                    // 清理路径跟随器
                    if (_monsterPathFollowers.TryGetValue(monster, out var pathFollower))
                    {
                        pathFollower.Stop();
                        _monsterPathFollowers.Remove(monster);
                    }
                    
                    monster?.Destroy();
                }
                catch (Exception ex)
                {
                    Game.Logger.LogWarning(ex, "⚠️ Failed to destroy monster: {monster}", monster);
                }
            }
            
            _spawnedMonsters.Clear();
            _monsterPathFollowers.Clear();
            Game.Logger.LogInformation("✅ All monsters and path followers cleared");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to clear monsters");
        }
    }

    /// <summary>
    /// 获取当前游戏状态
    /// </summary>
    public static bool IsGameStarted => _gameStarted;

    /// <summary>
    /// 获取已生成的怪物数量
    /// </summary>
    public static int MonstersSpawned => _monstersSpawned;

    /// <summary>
    /// 获取当前波数
    /// </summary>
    public static int WaveNumber => _currentWave;

    /// <summary>
    /// 获取游戏已运行时间（秒）
    /// </summary>
    public static float ElapsedTime => _gameStarted ? (float)(DateTime.Now - _gameStartTime).TotalSeconds : 0f;

    /// <summary>
    /// 获取玩家当前血量
    /// </summary>
    public static int PlayerHealth => _playerHealth;

    /// <summary>
    /// 获取玩家当前金币
    /// </summary>
    public static int PlayerGold => _playerGold;

    /// <summary>
    /// 修改玩家血量（用于调试或特殊情况）
    /// </summary>
    public static void ModifyPlayerHealth(int amount)
    {
        _playerHealth = Math.Max(0, _playerHealth + amount);
        Game.Logger.LogInformation("🩺 玩家血量变化: {amount:+0;-0}, 当前血量: {health}/20", amount, _playerHealth);
    }

    /// <summary>
    /// 修改玩家金币（用于建造消耗等）
    /// </summary>
    public static void ModifyPlayerGold(int amount)
    {
        _playerGold = Math.Max(0, _playerGold + amount);
        Game.Logger.LogInformation("💰 玩家金币变化: {amount:+0;-0}, 当前金币: {gold}", amount, _playerGold);
    }
}
#endif

