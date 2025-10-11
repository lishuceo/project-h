using GameCore;
using GameCore.ActorSystem.Data;
using GameCore.SceneSystem;
using GameCore.SceneSystem.Data;
using GameCore.EntitySystem;
using GameCore.PlayerAndUsers;
using GameData;
using System.Numerics;

namespace GameEntry.TowerDefenseGame.SpawnSystem;

/// <summary>
/// 塔防游戏刷怪管理器
/// 负责基于数据配置的怪物生成逻辑
/// </summary>
public class SpawnManager
{
    /// <summary>
    /// 刷怪器运行时状态
    /// </summary>
    private class SpawnerState
    {
        public GameLink<GameDataSpawner, GameDataSpawnerBasic> SpawnerLink { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime NextSpawnTime { get; set; }
        public int SpawnedCount { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }

        public SpawnerState(GameLink<GameDataSpawner, GameDataSpawnerBasic> spawnerLink)
        {
            SpawnerLink = spawnerLink;
            StartTime = DateTime.UtcNow;
            SpawnedCount = 0;
            IsCompleted = false;
            IsActive = false;
        }
    }

    /// <summary>
    /// 波次运行时状态
    /// </summary>
    private class WaveState
    {
        public GameLink<GameDataWave, GameDataWaveBasic> WaveLink { get; set; }
        public DateTime StartTime { get; set; }
        public List<SpawnerState> SpawnerStates { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }

        public WaveState(GameLink<GameDataWave, GameDataWaveBasic> waveLink)
        {
            WaveLink = waveLink;
            SpawnerStates = new List<SpawnerState>();
            IsCompleted = false;
            IsActive = false;
        }
    }

    private readonly Scene _scene;
    private readonly Player _monsterPlayer;
    private GameLink<GameDataLevel, GameDataLevelBasic>? _currentLevel;
    private List<WaveState> _waveStates = new();
    private int _currentWaveIndex = 0;
    private bool _isRunning = false;
    private DateTime _levelStartTime;
    
    // 循环相关
    private bool _loopMode = true; // 默认启用循环模式
    private int _loopCount = 0; // 循环计数器
    private DateTime _lastLoopTime;
    private readonly TimeSpan _loopDelay = TimeSpan.FromSeconds(10); // 循环间隔10秒

    // 事件
    public event Action<int>? WaveStarted;
    public event Action<int>? WaveCompleted;
    public event Action? LevelCompleted;
    public event Action<int>? LevelLooped; // 关卡循环事件
    public event Action<Unit>? MonsterSpawned;

    public SpawnManager(Scene scene, Player monsterPlayer)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _monsterPlayer = monsterPlayer ?? throw new ArgumentNullException(nameof(monsterPlayer));
    }

    /// <summary>
    /// 是否启用循环模式
    /// </summary>
    public bool LoopMode 
    { 
        get => _loopMode; 
        set => _loopMode = value; 
    }

    /// <summary>
    /// 获取当前循环次数
    /// </summary>
    public int LoopCount => _loopCount;

    /// <summary>
    /// 启动关卡
    /// </summary>
    public bool StartLevel(GameLink<GameDataLevel, GameDataLevelBasic> levelLink)
    {
        if (_isRunning)
        {
            Game.Logger.LogWarning("SpawnManager is already running");
            return false;
        }

        var levelData = levelLink.Data;
        if (levelData == null)
        {
            Game.Logger.LogError("Level data not found: {LevelLink}", levelLink.Id ?? levelLink.HashCode.ToString());
            return false;
        }

        if (!levelData.Enabled)
        {
            Game.Logger.LogWarning("Level is disabled: {LevelName}", levelData.LevelName);
            return false;
        }

        _currentLevel = levelLink;
        _levelStartTime = DateTime.UtcNow;
        _currentWaveIndex = 0;
        _waveStates.Clear();

        // 初始化所有波次状态
        foreach (var waveLink in levelData.Waves)
        {
            var waveData = waveLink.Data;
            if (waveData != null && waveData.Enabled)
            {
                var waveState = new WaveState(waveLink);
                
                // 初始化波次中的所有刷怪器状态
                foreach (var spawnerLink in waveData.WaveData)
                {
                    var spawnerData = spawnerLink.Data;
                    if (spawnerData != null && spawnerData.Enabled)
                    {
                        var spawnerState = new SpawnerState(spawnerLink);
                        waveState.SpawnerStates.Add(spawnerState);
                    }
                }
                
                _waveStates.Add(waveState);
            }
        }

        if (_waveStates.Count == 0)
        {
            Game.Logger.LogWarning("No valid waves found in level: {LevelName}", levelData.LevelName);
            return false;
        }

        _isRunning = true;
        Game.Logger.LogInformation("🚀 Started level: {LevelName} with {WaveCount} waves", 
            levelData.LevelName, _waveStates.Count);

        return true;
    }

    /// <summary>
    /// 停止关卡
    /// </summary>
    public void StopLevel()
    {
        _isRunning = false;
        _currentLevel = null;
        _waveStates.Clear();
        _currentWaveIndex = 0;
        
        Game.Logger.LogInformation("🛑 Spawn manager stopped");
    }

    /// <summary>
    /// 更新刷怪逻辑（在游戏主循环中调用）
    /// </summary>
    public void Update()
    {
        if (!_isRunning || _currentLevel?.Data == null)
            return;

        var now = DateTime.UtcNow;

        // 检查是否需要启动下一个波次
        if (_currentWaveIndex < _waveStates.Count)
        {
            var currentWave = _waveStates[_currentWaveIndex];
            
            if (!currentWave.IsActive)
            {
                // 检查是否到了启动时间
                var waveData = currentWave.WaveLink.Data;
                if (waveData != null)
                {
                    var shouldStart = (_currentWaveIndex == 0) ? 
                        (now - _levelStartTime).TotalSeconds >= waveData.WaveDelay :
                        _waveStates[_currentWaveIndex - 1].IsCompleted;

                    if (shouldStart)
                    {
                        StartWave(currentWave, now);
                    }
                }
            }
            else
            {
                // 更新当前波次
                UpdateWave(currentWave, now);
            }
        }
        else if (_waveStates.All(w => w.IsCompleted))
        {
            // 所有波次完成
            if (_loopMode && _loopCount > 0)
            {
                // 循环模式且已经有过循环，检查循环间隔
                if (now - _lastLoopTime >= _loopDelay)
                {
                    CompleteLLevel();
                }
            }
            else
            {
                // 第一次完成或非循环模式
                CompleteLLevel();
            }
        }
    }

    private void StartWave(WaveState waveState, DateTime now)
    {
        waveState.IsActive = true;
        waveState.StartTime = now;

        var waveData = waveState.WaveLink.Data;
        if (waveData != null)
        {
            // 启动波次中的所有刷怪器
            foreach (var spawnerState in waveState.SpawnerStates)
            {
                var spawnerData = spawnerState.SpawnerLink.Data;
                if (spawnerData != null)
                {
                    spawnerState.IsActive = true;
                    spawnerState.StartTime = now;
                    spawnerState.NextSpawnTime = now.AddSeconds(spawnerData.Delay);
                }
            }

            Game.Logger.LogInformation("🌊 Wave {WaveNumber} started: {WaveName} with {SpawnerCount} spawners", 
                _currentWaveIndex + 1, waveData.WaveName, waveState.SpawnerStates.Count);
            
            WaveStarted?.Invoke(_currentWaveIndex + 1);
        }
    }

    private void UpdateWave(WaveState waveState, DateTime now)
    {
        bool allSpawnersCompleted = true;

        foreach (var spawnerState in waveState.SpawnerStates)
        {
            if (!spawnerState.IsCompleted)
            {
                allSpawnersCompleted = false;
                UpdateSpawner(spawnerState, now);
            }
        }

        if (allSpawnersCompleted && !waveState.IsCompleted)
        {
            waveState.IsCompleted = true;
            
            Game.Logger.LogInformation("✅ Wave {WaveNumber} completed", _currentWaveIndex + 1);
            WaveCompleted?.Invoke(_currentWaveIndex + 1);
            
            _currentWaveIndex++;
        }
    }

    private void UpdateSpawner(SpawnerState spawnerState, DateTime now)
    {
        var spawnerData = spawnerState.SpawnerLink.Data;
        if (spawnerData == null) return;

        // 检查是否到了生成时间
        if (now >= spawnerState.NextSpawnTime && spawnerState.SpawnedCount < spawnerData.Times)
        {
            // 生成怪物
            SpawnMonsters(spawnerData);
            
            spawnerState.SpawnedCount++;
            
            // 设置下次生成时间
            if (spawnerState.SpawnedCount < spawnerData.Times)
            {
                spawnerState.NextSpawnTime = now.AddSeconds(spawnerData.Pulse);
            }
            else
            {
                spawnerState.IsCompleted = true;
            }
        }
    }

    private void SpawnMonsters(GameDataSpawner spawnerData)
    {
        var spawnPosition = GetSpawnPosition(spawnerData.LineEx);
        if (!spawnPosition.HasValue)
        {
            Game.Logger.LogError("❌ Cannot get spawn position for line: {Line}", spawnerData.LineEx);
            return;
        }

        for (int i = 0; i < spawnerData.Number; i++)
        {
            var monster = CreateMonster(spawnerData, spawnPosition.Value);
            if (monster != null)
            {
                SetupMonsterPath(monster, spawnerData.LineEx);
                MonsterSpawned?.Invoke(monster);
            }
        }
    }

    private Unit? CreateMonster(GameDataSpawner spawnerData, Vector3 position)
    {
        if (spawnerData.Monster == null || spawnerData.Monster.Value.Data == null)
        {
            Game.Logger.LogError("Monster data not found in spawner");
            return null;
        }

        try
        {
            var monster = spawnerData.Monster.Value.Data.CreateUnit(
                _monsterPlayer,
                new ScenePoint(position, _scene),
                0);

            if (monster != null)
            {
                Game.Logger.LogDebug("👹 Monster spawned: {MonsterName} at {Position}", 
                    monster.Cache.Name, position);
            }

            return monster;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to create monster from spawner");
            return null;
        }
    }

    private Vector3? GetSpawnPosition(string? pathName)
    {
        if (string.IsNullOrEmpty(pathName))
        {
            pathName = "玩家1前进路线"; // 默认路径
        }

        try
        {
            var path = PathSystem.GetPath(pathName);
            if (path != null && path.Count > 0)
            {
                return path.GetPoint(0).Position;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to get spawn position from path: {PathName}", pathName);
        }

        return null;
    }

    private void SetupMonsterPath(Unit monster, string? pathName)
    {
        if (string.IsNullOrEmpty(pathName))
        {
            pathName = "玩家1前进路线"; // 默认路径
        }

        try
        {
            var path = PathSystem.GetPath(pathName);
            if (path != null)
            {
                var pathFollower = new ImprovedPathFollower(monster, path);
                // 这里可以添加到全局的路径跟随管理器中
                // 或者直接启动路径跟随
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to setup monster path: {PathName}", pathName);
        }
    }

    private void CompleteLLevel()
    {
        var levelData = _currentLevel?.Data;
        
        if (_loopMode)
        {
            // 循环模式：重新开始关卡
            _loopCount++;
            _lastLoopTime = DateTime.UtcNow;
            
            Game.Logger.LogInformation("🔄 Level loop {LoopCount} completed: {LevelName}, starting next loop...", 
                _loopCount, levelData?.LevelName);
            
            // 重置所有波次状态
            ResetAllWaves();
            _currentWaveIndex = 0;
            
            // 触发循环事件
            LevelLooped?.Invoke(_loopCount);
        }
        else
        {
            // 非循环模式：正常结束
            _isRunning = false;
            Game.Logger.LogInformation("🏆 Level completed: {LevelName}", levelData?.LevelName);
            LevelCompleted?.Invoke();
        }
    }

    /// <summary>
    /// 重置所有波次状态以便重新开始
    /// </summary>
    private void ResetAllWaves()
    {
        foreach (var waveState in _waveStates)
        {
            waveState.IsActive = false;
            waveState.IsCompleted = false;
            
            // 重置波次中的所有刷怪器状态
            foreach (var spawnerState in waveState.SpawnerStates)
            {
                spawnerState.IsActive = false;
                spawnerState.IsCompleted = false;
                spawnerState.SpawnedCount = 0;
                spawnerState.StartTime = DateTime.UtcNow;
                
                var spawnerData = spawnerState.SpawnerLink.Data;
                if (spawnerData != null)
                {
                    spawnerState.NextSpawnTime = DateTime.UtcNow.AddSeconds(spawnerData.Delay);
                }
            }
        }
    }

    /// <summary>
    /// 获取当前关卡进度信息
    /// </summary>
    public (int currentWave, int totalWaves, bool isCompleted) GetProgress()
    {
        // 在循环模式下，关卡永不完成
        bool isCompleted = _loopMode ? false : (!_isRunning && _currentWaveIndex >= _waveStates.Count);
        return (_currentWaveIndex + 1, _waveStates.Count, isCompleted);
    }

    /// <summary>
    /// 获取当前波次状态
    /// </summary>
    public string GetCurrentWaveInfo()
    {
        if (_currentWaveIndex < _waveStates.Count)
        {
            var currentWave = _waveStates[_currentWaveIndex];
            var waveData = currentWave.WaveLink.Data;
            var loopInfo = _loopMode && _loopCount > 0 ? $" (Loop {_loopCount})" : "";
            return $"Wave {_currentWaveIndex + 1}/{_waveStates.Count}{loopInfo}: {waveData?.WaveName ?? "Unknown"}";
        }
        
        if (_loopMode)
        {
            return $"Waiting for next loop... (Loop {_loopCount + 1})";
        }
        
        return "All waves completed";
    }

    /// <summary>
    /// 停止循环模式并结束关卡
    /// </summary>
    public void StopLoop()
    {
        _loopMode = false;
        Game.Logger.LogInformation("🛑 Loop mode disabled, level will complete after current wave");
    }
}
