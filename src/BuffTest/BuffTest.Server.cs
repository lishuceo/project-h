#if SERVER
using Events;
using GameCore.BaseType;
using GameCore.BuffSystem;
using GameCore.BuffSystem.Data;
using GameCore.BuffSystem.Data.Struct;
using GameCore.Event;
using GameCore.OrderSystem;
using GameCore.SceneSystem;
using GameCore.VitalSystem;
using GameData;
using GameData.Extension;
using TriggerEncapsulation;
using GameCore.Localization;
using GameCore.BuffSystem.Data.Enum;
using System.Numerics;
using GameCore.Platform.SDL;
using GameCore.PlayerAndUsers;
using System.Diagnostics;

namespace GameEntry.BuffTest;

/// <summary>
/// BuffTest游戏模式的服务器端实现
/// 测试TriggerEncapsulation中的为单位添加Buff的便利函数
/// </summary>
public class BuffTestServer : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.OnGameStart += OnGameStart;
    }

    private static void OnGameTriggerInitialization()
    {
        // 只在BuffTest游戏模式下运行
        if (Game.GameModeLink != ScopeData.GameMode.BuffTest)
        {
            return;
        }

        Game.Logger.LogInformation("🧪 Initializing BuffTest Server Mode...");

        // 注册按键事件处理器
        RegisterKeyEventHandlers();
    }

    private static void OnGameStart()
    {
        // 只在BuffTest游戏模式下运行
        if (Game.GameModeLink != ScopeData.GameMode.BuffTest)
        {
            return;
        }

        Game.Logger.LogInformation("🎮 BuffTest game started");

        // 获取默认场景
        var scene = Scene.GetOrCreate(ScopeData.Scene.DefaultScene);
        if (scene == null)
        {
            Game.Logger.LogError("Failed to get default scene");
            return;
        }

        // 创建测试单位
        _ = CreateTestUnits(scene);
    }

    /// <summary>
    /// 注册按键事件处理器
    /// </summary>
    private static void RegisterKeyEventHandlers()
    {
        Game.Logger.LogInformation("🔑 Registering key event handlers for BuffTest...");
        // 注册按键事件，当用户按下指定键时添加Buff
        Trigger<EventPlayerKeyDown> keyDownTrigger = new(async (sender, eventArgs) =>
        {
            // 检查是否是BuffTest游戏模式
            if (Game.GameModeLink != ScopeData.GameMode.BuffTest)
            {
                return false;
            }

            // 检查按键是否是空格键（Space）
            if (eventArgs.Key == VirtualKey.Space)
            {
                await AddBuffToPlayerUnit(eventArgs.Player);
            }

            return true;
        }, true);

        keyDownTrigger.Register(Game.Instance);
    }

    /// <summary>
    /// 为玩家单位添加Buff
    /// </summary>
    private static async Task AddBuffToPlayerUnit(Player player)
    {
        try
        {
            if (player?.MainUnit == null)
            {
                Game.Logger.LogWarning("Player or main unit not found");
                return;
            }

            var unit = player.MainUnit;
            Game.Logger.LogInformation("Adding buff to unit: {unit}", unit.Cache.Name);

            // 使用TriggerEncapsulation的便利函数添加Buff
            var buff = unit.AddBuff(
                BuffTestMode.TestBuff,
                caster: unit,
                stack: 1, // 使用固定值，因为常量已移动到BuffTestMode
                duration: TimeSpan.FromSeconds(10) // 使用固定值，因为常量已移动到BuffTestMode
            );

            if (buff != null)
            {
                Game.Logger.LogInformation("✅ Successfully added buff to unit: {unit}, Buff: {buff}, Duration: 10s", 
                    unit.Cache.Name, buff.Cache.DisplayName);
            }
            else
            {
                Game.Logger.LogWarning("❌ Failed to add buff to unit: {unit}", unit.Cache.Name);
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error adding buff to player unit");
        }
    }

    /// <summary>
    /// 创建测试单位
    /// </summary>
    private static async Task CreateTestUnits(Scene scene)
    {
        try
        {
            // 获取玩家1
            var player1 = Player.GetById(1);
            if (player1 == null)
            {
                Game.Logger.LogWarning("Player 1 not found");
                return;
            }

            // 在场景中创建测试单位
            var unitPosition = new ScenePoint(new Vector3(3500, 3000, 0), scene);
            var testUnit = ScopeData.Unit.HostTestHero.Data?.CreateUnit(player1, unitPosition, 0);

            if (testUnit != null)
            {
                // 设置为主控单位
                player1.MainUnit = testUnit;
                Game.Logger.LogInformation("✅ Test unit created: {unit} at position {position}", 
                    testUnit.Cache.Name, unitPosition);
            }
            else
            {
                Game.Logger.LogWarning("❌ Failed to create test unit");
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error creating test units");
        }
    }
}
#endif
