using Events;
using GameCore.BaseType;
using GameCore.BuffSystem;
using GameCore.BuffSystem.Data;
using GameCore.BuffSystem.Data.Enum;
using GameCore.BuffSystem.Data.Struct;
using GameCore.Event;
using GameCore.SceneSystem;
using GameCore.Localization;
using GameData;
using GameData.Extension;

namespace GameEntry.BuffTest;

/// <summary>
/// BuffTest游戏模式的主要协调类
/// 测试TriggerEncapsulation中的为单位添加Buff的便利函数
/// </summary>
/// <remarks>
/// 这个游戏模式专门用于测试TriggerEncapsulation中的Buff便利函数，包括：
/// - 使用TriggerEncapsulation的AddBuff便利函数为单位添加Buff
/// - 通过按键触发Buff添加（空格键）
/// - 客户端UI显示Buff的总时长和剩余时长
/// - 实时更新Buff状态和进度条
/// </remarks>
public class BuffTestMode : IGameClass
{
    /// <summary>
    /// 测试Buff的数据链接（共享定义）
    /// </summary>
    public static readonly GameLink<GameDataBuff, GameDataBuff> TestBuff = new("TestBuff");

    /// <summary>
    /// 测试Buff的持续时间（秒）
    /// </summary>
    private const int BUFF_DURATION_SECONDS = 10;

    /// <summary>
    /// 测试Buff的堆叠数量
    /// </summary>
    private const uint BUFF_STACK = 1;

    /// <summary>
    /// 游戏模式状态
    /// </summary>
    public static class GameState
    {
        public static bool IsInitialized { get; set; } = false;
        public static int BuffsAdded { get; set; } = 0;
        public static int BuffsRemoved { get; set; } = 0;
        public static DateTime StartTime { get; set; }
    }

    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization += OnGameDataInitialization;
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    /// <summary>
    /// 游戏数据初始化
    /// </summary>
    private static void OnGameDataInitialization()
    {
        // 只在BuffTest游戏模式下运行
        if (Game.GameModeLink != ScopeData.GameMode.BuffTest)
        {
            return;
        }

        Game.Logger.LogInformation("🧪 Initializing BuffTest Game Data...");
        CreateTestBuffData();
    }

    /// <summary>
    /// 创建测试Buff数据
    /// </summary>
    private static void CreateTestBuffData()
    {
        try
        {
            // 检查TestBuff数据是否已经存在
            if (TestBuff.Data != null)
            {
                Game.Logger.LogInformation("✅ Test buff data already exists: {buff}", TestBuff.Data.DisplayName);
                return;
            }

            // 创建测试Buff数据
            var testBuffData = new GameDataBuff(TestBuff)
            {
                DisplayName = new LocalizedString("Test Buff"),
                Description = new LocalizedString("A test buff for demonstrating TriggerEncapsulation buff utility functions"),
                Icon = null,
                SyncType = SyncType.All,
                Polarity = BuffPolarity.Positive,
                StackStart = BUFF_STACK,
                Duration = static (_) => TimeSpan.FromSeconds(BUFF_DURATION_SECONDS),
                BuffFlags = new BuffFlags
                {
                    SingleInstancePerCaster = true,
                    Channeling = false
                }
            };

            Game.Logger.LogInformation("✅ Test buff data created: {buff}", testBuffData.DisplayName);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error creating test buff data");
        }
    }

    private static void OnGameTriggerInitialization()
    {
        // 只在BuffTest游戏模式下运行
        if (Game.GameModeLink != ScopeData.GameMode.BuffTest)
        {
            return;
        }

        Game.Logger.LogInformation("🧪 Initializing BuffTest Game Mode...");
        
        try
        {
            // 设置游戏状态
            GameState.IsInitialized = true;
            GameState.StartTime = DateTime.UtcNow;
            GameState.BuffsAdded = 0;
            GameState.BuffsRemoved = 0;

            Game.Logger.LogInformation("✅ BuffTest Game Mode initialized successfully");
            Game.Logger.LogInformation("📋 Instructions:");
            Game.Logger.LogInformation("   - Press SPACE key to add a test buff to your unit");
            Game.Logger.LogInformation("   - Buff duration: 10 seconds");
            Game.Logger.LogInformation("   - UI will show buff status and remaining time");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to initialize BuffTest Game Mode");
        }
    }
}

