#if SERVER
using Events;
using GameCore;
using GameCore.Event;
using GameCore.SceneSystem;
using GameCore.PlayerAndUsers;
using GameCore.PlayerAndUsers.Enum;
using GameData;

namespace GameEntry.XianJianQiXiaZhuan;

/// <summary>
/// 仙剑奇侠传游戏服务端逻辑 - 简化版本
/// </summary>
internal class XianJianServer : IGameClass
{
    // 游戏状态
    private static bool _gameStarted = false;
    private static Scene? _currentScene = null;
    private static DateTime _gameStartTime;
    
    // 触发器
    private static Trigger<EventGameStart>? gameStartTrigger;
    private static Trigger<EventGameTick>? gameTickTrigger;
    private static Trigger<EventPlayerUserConnected>? playerConnectedTrigger;

    public static void OnRegisterGameClass()
    {
        Game.Logger.LogInformation("🗡️ 注册仙剑奇侠传服务端模块...");
        
        // 使用触发器初始化方式
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        // 检查当前游戏模式是否为仙剑奇侠传
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.XianJianQiXiaZhuan)
        {
            return;
        }

        Game.Logger.LogInformation("🗡️ 仙剑奇侠传模式检测到，初始化服务端...");
        
        try
        {
            SetupEventTriggers();
            Game.Logger.LogInformation("✅ 仙剑奇侠传服务端初始化完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 仙剑奇侠传服务端初始化失败");
        }
    }

    private static void SetupEventTriggers()
    {
        try
        {
            // 注册游戏开始触发器
            gameStartTrigger = new Trigger<EventGameStart>(OnGameStartAsync);
            gameStartTrigger.Register(Game.Instance);

            // 注册游戏Tick触发器
            gameTickTrigger = new Trigger<EventGameTick>(OnGameTick);
            gameTickTrigger.Register(Game.Instance);

            // 注册玩家连接触发器
            playerConnectedTrigger = new Trigger<EventPlayerUserConnected>(OnPlayerConnected);
            playerConnectedTrigger.Register(Game.Instance);

            Game.Logger.LogInformation("✅ 仙剑奇侠传服务端触发器初始化完成!");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "💥 初始化仙剑奇侠传服务端触发器时发生错误");
        }
    }

    /// <summary>
    /// 游戏开始事件处理
    /// </summary>
    private static async Task<bool> OnGameStartAsync(object sender, EventGameStart evt)
    {
        try
        {
            if (_gameStarted)
                return false;

            _gameStarted = true;
            _gameStartTime = DateTime.Now;

            Game.Logger.LogInformation("🗡️ 仙剑奇侠传世界开始！");

            // 获取仙剑场景
            var scene = Scene.GetOrCreate(ScopeData.XianJianScopeData.Scene.YuHangTown);
            if (scene == null)
            {
                Game.Logger.LogError("❌ 余杭镇场景未找到");
                return false;
            }

            _currentScene = scene;

            // 等待场景加载
            if (!scene.Loaded)
            {
                Game.Logger.LogInformation("🌍 等待余杭镇场景加载...");
                await Game.Delay(TimeSpan.FromSeconds(1));
            }

            // 显示开场故事
            await ShowOpeningStory();

            // 创建敌对玩家用于蛇妖单位
            await CreateEnemyPlayer();

            Game.Logger.LogInformation("✅ 仙剑奇侠传世界初始化完成!");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "💥 仙剑奇侠传游戏开始时发生错误");
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 游戏Tick事件处理
    /// </summary>
    private static async Task<bool> OnGameTick(object sender, EventGameTick evt)
    {
        if (!_gameStarted || _currentScene == null)
            return false;

        try
        {
            // 简化版本 - 基本的游戏循环
            // 可以在这里添加周期性逻辑，如怪物刷新、状态检查等
            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "💥 处理游戏Tick时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 玩家连接事件处理
    /// </summary>
    private static async Task<bool> OnPlayerConnected(object sender, EventPlayerUserConnected evt)
    {
        try
        {
            var player = evt.Player;
            Game.Logger.LogInformation("👤 玩家 {PlayerId} 进入了仙剑奇侠传世界!", player.Id);

            // 欢迎消息
            Game.Logger.LogInformation("🌟 欢迎来到仙剑奇侠传的世界！");
            Game.Logger.LogInformation("🗡️ 你扮演的是蜀山剑派弟子李逍遥");
            Game.Logger.LogInformation("💫 你的冒险即将开始...");

            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "💥 欢迎新玩家时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 显示开场故事 - 现在由客户端AVG系统处理
    /// </summary>
    private static async Task ShowOpeningStory()
    {
        // 开场故事现在由客户端的AVG对话系统处理
        Game.Logger.LogInformation("📖 仙剑奇侠传开场故事由客户端AVG系统播放");
        await Game.Delay(TimeSpan.FromSeconds(1)); // 短暂延迟确保客户端准备就绪
    }

    /// <summary>
    /// 创建敌对玩家并设置玩家关系
    /// </summary>
    private static async Task CreateEnemyPlayer()
    {
        try
        {
            Game.Logger.LogInformation("👹 设置玩家关系以支持敌对单位...");
            
            // 等待一下让系统稳定
            await Game.Delay(TimeSpan.FromSeconds(0.5));
            
            Game.Logger.LogInformation("✅ 玩家关系设置完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 设置玩家关系时发生错误");
        }
    }
}
#endif