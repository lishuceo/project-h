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
using GameCore.AISystem;
using GameCore.AISystem.Data;
using GameCore.AISystem.Data.Enum;
using GameCore.PlayerAndUsers.Enum;
using GameData;
using GameEntry.ARPGTemplate.ScopeData; // 添加ARPG ScopeData引用
using System.Numerics;

namespace GameEntry.ARPGTemplate;

/// <summary>
/// ARPG剑客游戏服务端逻辑
/// </summary>
internal class ARPGServer : IGameClass
{
    // 游戏状态
    private static bool _gameStarted = false;
    private static Scene? _currentScene = null;
    private static DateTime _gameStartTime;
    
    // 触发器
    private static Trigger<EventGameStart>? gameStartTrigger;
    private static Trigger<EventGameTick>? gameTickTrigger;

    // 动态创建的单位列表
    private static readonly List<Unit> dynamicUnits = new();

    public static void OnRegisterGameClass()
    {
        Game.Logger.LogInformation("⚔️ 注册ARPG剑客服务端模块...");
        
        // 使用触发器初始化方式
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        // 检查当前游戏模式是否为ARPG
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.ARPGMode)
        {
            return;
        }

        Game.Logger.LogInformation("⚔️ ARPG模式检测到，初始化ARPG服务端...");
        
        try
        {
            SetupEventTriggers();
            Game.Logger.LogInformation("✅ ARPG服务端初始化完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ ARPG服务端初始化失败");
        }
    }

    private static void SetupEventTriggers()
    {
        // 游戏开始事件
        gameStartTrigger = new Trigger<EventGameStart>(ARPGGameStartAsync);
        gameStartTrigger.Register(Game.Instance);

        // 游戏循环事件
        gameTickTrigger = new Trigger<EventGameTick>(OnGameTick);
        gameTickTrigger.Register(Game.Instance);
        
        Game.Logger.LogInformation("⚔️ ARPG服务端事件触发器设置完成");
    }

    private static async Task<bool> ARPGGameStartAsync(object sender, EventGameStart eventArgs)
    {
        if (_gameStarted)
            return false;

        Game.Logger.LogInformation("⚔️ ARPG游戏开始！");
        
        _gameStarted = true;
        _gameStartTime = DateTime.Now;
        
        // 获取ARPG场景
        var scene = Scene.GetOrCreate(ScopeData.ARPGScopeData.Scene.ARPGScene);
        if (scene == null)
        {
            Game.Logger.LogError("❌ ARPG场景未找到");
            return false;
        }

        // 确保场景已加载
        if (!scene.Loaded)
        {
            Game.Logger.LogInformation("⏳ 等待ARPG场景加载...");
            // 等待场景加载，使用简单的等待方式
            var maxWait = 100; // 最多等待10秒
            var waitCount = 0;
            while (!scene.Loaded && waitCount < maxWait)
            {
                await Game.Delay(TimeSpan.FromMilliseconds(100));
                waitCount++;
            }
            
            if (!scene.Loaded)
            {
                Game.Logger.LogWarning("⚠️ ARPG场景加载超时");
            }
        }
        
        _currentScene = scene;
        
        if (_currentScene != null)
        {
            Game.Logger.LogInformation($"⚔️ 当前场景: {_currentScene.Name}");
            
            // 检查场景中的剑客单位
            CheckForSwordsmanUnits(_currentScene);
            
            // 动态创建AI单位
            await CreateDynamicUnits(_currentScene);
        }
        
        return true;
    }

    private static void CheckForSwordsmanUnits(Scene scene)
    {
        try
        {
            Game.Logger.LogInformation("⚔️ 检查ARPG场景中的单位...");
            Game.Logger.LogInformation($"   场景名称: {scene.Name}");
            Game.Logger.LogInformation($"   场景已加载: {scene.Loaded}");
            
            // 场景检查逻辑，无需等待
            
            // 简化检查逻辑，先看看能找到什么单位
            Game.Logger.LogInformation("✅ ARPG场景检查完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 检查ARPG场景单位时出错");
        }
    }

    /// <summary>
    /// 动态创建AI单位（狼人、史莱姆、蜘蛛）
    /// </summary>
    private static async Task CreateDynamicUnits(Scene scene)
    {
        try
        {
            Game.Logger.LogInformation("🤖 开始动态创建AI单位...");
            
            // 获取可用玩家
            var players = Player.AllPlayers.ToList();
            if (players.Count < 2)
            {
                Game.Logger.LogWarning("⚠️ 玩家数量不足，无法创建敌对单位");
                return;
            }

            // 获取敌对玩家（Player 4）和友方玩家（Player 1）
            var enemyPlayer = players.FirstOrDefault(p => p.Id == 4) ?? players.LastOrDefault();
            var friendlyPlayer = players.FirstOrDefault(p => p.Id == 1) ?? players.FirstOrDefault();
            
            if (enemyPlayer == null || friendlyPlayer == null)
            {
                Game.Logger.LogError("❌ 无法找到合适的玩家来创建单位");
                return;
            }
            
            Game.Logger.LogInformation("👥 使用玩家: 敌方=Player{EnemyId}, 友方=Player{FriendlyId}", 
                enemyPlayer.Id, friendlyPlayer.Id);

            // 创建三种敌人，每种各两只
            await CreateWerewolves(scene, enemyPlayer);
            await CreateSlimes(scene, enemyPlayer);
            await CreateSpiders(scene, enemyPlayer);
            
            Game.Logger.LogInformation("✅ 动态创建AI单位完成，总共创建了 {Count} 个单位", dynamicUnits.Count);
            Game.Logger.LogInformation("   - 狼人敌人: 2只 (圆形布局 0°, 60°)");
            Game.Logger.LogInformation("   - 史莱姆敌人: 2只 (圆形布局 120°, 180°)");
            Game.Logger.LogInformation("   - 蜘蛛敌人: 2只 (圆形布局 240°, 300°)");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 动态创建AI单位时出错");
        }
    }

    /// <summary>
    /// 创建狼人单位 - 近战强力敌人
    /// </summary>
    private static async Task CreateWerewolves(Scene scene, Player ownerPlayer)
    {
        Game.Logger.LogInformation("🐺 创建狼人单位...");
        
        // 狼人位置配置 - 圆形布局的前两个位置（0度和60度）
        var werewolfPositions = new Vector3[]
        {
            new(3800, 3000, 0), // 圆形布局位置1 (0度)
            new(3400, 3693, 0), // 圆形布局位置2 (60度)
        };

        for (int i = 0; i < werewolfPositions.Length; i++)
        {
            var position = werewolfPositions[i];
            
            // 创建狼人单位
            var werewolf = GameEntry.ARPGTemplate.ScopeData.ARPGUnits.Unit.WerewolfEnemy.Data?.CreateUnit(
                ownerPlayer,
                new ScenePoint(position, scene),
                0
            );

            if (werewolf != null)
            {
                dynamicUnits.Add(werewolf);
                
                // 🤖 添加AI - 会自动使用单位配置中的TacticalAI（MonsterAI）
                var aiThinkTree = AIThinkTree.AddDefaultAI(werewolf);
                if (aiThinkTree != null)
                {
                    Game.Logger.LogInformation("🧠 狼人 {Index} MonsterAI配置成功: {UnitName} at {Position}", 
                        i + 1, werewolf.Cache.Name, position);
                }
                else
                {
                    Game.Logger.LogWarning("⚠️ 狼人 {Index} MonsterAI配置失败", i + 1);
                }
                
                await Game.Delay(TimeSpan.FromMilliseconds(100)); // 稍微延迟避免同时创建
            }
            else
            {
                Game.Logger.LogError("❌ 无法创建狼人单位 {Index}", i + 1);
            }
        }
    }

    /// <summary>
    /// 创建史莱姆单位 - 近战弱小敌人
    /// </summary>
    private static async Task CreateSlimes(Scene scene, Player ownerPlayer)
    {
        Game.Logger.LogInformation("🟢 创建史莱姆单位...");
        
        // 史莱姆位置配置 - 圆形布局的中间两个位置（120度和180度）
        var slimePositions = new Vector3[]
        {
            new(2600, 3693, 0), // 圆形布局位置3 (120度)
            new(2200, 3000, 0), // 圆形布局位置4 (180度)
        };

        for (int i = 0; i < slimePositions.Length; i++)
        {
            var position = slimePositions[i];
            
            // 创建史莱姆单位
            var slime = GameEntry.ARPGTemplate.ScopeData.ARPGUnits.Unit.SlimeEnemy.Data?.CreateUnit(
                ownerPlayer,
                new ScenePoint(position, scene),
                0
            );

            if (slime != null)
            {
                dynamicUnits.Add(slime);
                
                // 🤖 添加AI - 会自动使用单位配置中的TacticalAI（MonsterAI）
                var aiThinkTree = AIThinkTree.AddDefaultAI(slime);
                if (aiThinkTree != null)
                {
                    Game.Logger.LogInformation("🧠 史莱姆 {Index} MonsterAI配置成功: {UnitName} at {Position}", 
                        i + 1, slime.Cache.Name, position);
                }
                else
                {
                    Game.Logger.LogWarning("⚠️ 史莱姆 {Index} MonsterAI配置失败", i + 1);
                }
                
                await Game.Delay(TimeSpan.FromMilliseconds(100)); // 稍微延迟避免同时创建
            }
            else
            {
                Game.Logger.LogError("❌ 无法创建史莱姆单位 {Index}", i + 1);
            }
        }
    }

    /// <summary>
    /// 创建蜘蛛单位 - 远程攻击敌人
    /// </summary>
    private static async Task CreateSpiders(Scene scene, Player ownerPlayer)
    {
        Game.Logger.LogInformation("🕷️ 创建蜘蛛单位...");
        
        // 蜘蛛位置配置 - 圆形布局的最后两个位置（240度和300度）
        var spiderPositions = new Vector3[]
        {
            new(2600, 2307, 0), // 圆形布局位置5 (240度)
            new(3400, 2307, 0), // 圆形布局位置6 (300度)
        };

        for (int i = 0; i < spiderPositions.Length; i++)
        {
            var position = spiderPositions[i];
            
            // 创建蜘蛛单位
            var spider = GameEntry.ARPGTemplate.ScopeData.ARPGUnits.Unit.SpiderEnemy.Data?.CreateUnit(
                ownerPlayer,
                new ScenePoint(position, scene),
                0
            );

            if (spider != null)
            {
                dynamicUnits.Add(spider);
                
                // 🤖 添加AI - 会自动使用单位配置中的TacticalAI（MonsterAI）
                var aiThinkTree = AIThinkTree.AddDefaultAI(spider);
                if (aiThinkTree != null)
                {
                    Game.Logger.LogInformation("🧠 蜘蛛 {Index} MonsterAI配置成功: {UnitName} at {Position}", 
                        i + 1, spider.Cache.Name, position);
                }
                else
                {
                    Game.Logger.LogWarning("⚠️ 蜘蛛 {Index} MonsterAI配置失败", i + 1);
                }
                
                await Game.Delay(TimeSpan.FromMilliseconds(100)); // 稍微延迟避免同时创建
            }
            else
            {
                Game.Logger.LogError("❌ 无法创建蜘蛛单位 {Index}", i + 1);
            }
        }
    }



    private static async Task<bool> OnGameTick(object sender, EventGameTick e)
    {
        if (!_gameStarted || _currentScene == null)
            return false;

        // 这里可以添加定时的游戏逻辑，比如AI更新、状态检查等
        // 为了性能考虑，目前保持简单
        await Task.CompletedTask; // 避免async警告
        return true;
    }

    /// <summary>
    /// 获取游戏运行时间
    /// </summary>
    public static TimeSpan GetGameTime()
    {
        return _gameStarted ? DateTime.Now - _gameStartTime : TimeSpan.Zero;
    }

    /// <summary>
    /// 检查游戏是否已开始
    /// </summary>
    public static bool IsGameStarted()
    {
        return _gameStarted;
    }

    /// <summary>
    /// 获取当前场景
    /// </summary>
    public static Scene? GetCurrentScene()
    {
        return _currentScene;
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public static void Cleanup()
    {
        Game.Logger.LogInformation("🧹 清理ARPG服务端资源...");
        
        try
        {
            // 清理动态创建的单位
            CleanupDynamicUnits();
            
            // 取消事件注册
            gameStartTrigger?.Unregister(Game.Instance);
            gameTickTrigger?.Unregister(Game.Instance);
            
            gameStartTrigger = null;
            gameTickTrigger = null;
            
            _gameStarted = false;
            _currentScene = null;
            
            Game.Logger.LogInformation("✅ ARPG服务端资源清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 清理ARPG服务端资源时出错");
        }
    }

    /// <summary>
    /// 清理动态创建的单位
    /// </summary>
    private static void CleanupDynamicUnits()
    {
        try
        {
            Game.Logger.LogInformation("🗑️ 清理动态创建的AI单位...");
            
            foreach (var unit in dynamicUnits)
            {
                try
                {
                    if (unit.IsValid)
                    {
                        unit.Destroy();
                    }
                }
                catch (Exception ex)
                {
                    Game.Logger.LogWarning("清理单位时出错: {Error}", ex.Message);
                }
            }
            
            dynamicUnits.Clear();
            Game.Logger.LogInformation("✅ 动态单位清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 清理动态单位时出错");
        }
    }
}
#endif
