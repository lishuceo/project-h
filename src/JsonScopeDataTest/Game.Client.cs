// #if CLIENT
// using Events;
// using GameCore.BaseInterface;
// using GameCore.Event;
// using GameCore.PlayerAndUsers;
// using Microsoft.Extensions.Logging;
// using TriggerEncapsulation.Event;
// using TriggerEncapsulation.Messaging;
// using System;
// using System.Text;
// using System.Text.Json;
// using System.Numerics;
// using GameUI.TriggerEvent;
// using GameCore.OrderSystem;
// using GameUI.Device;

// namespace GameEntry;

// /// <summary>
// /// 客户端游戏逻辑类
// /// 负责处理UI交互、显示状态和客户端独有的逻辑
// /// </summary>
// public class GameClient : IGameClass
// {
//     // ==================== 事件触发器 ====================
//     private static Trigger<EventGameStart>? gameStartTrigger;

//     /// <summary>
//     /// 框架初始化时自动调用此方法注册游戏类
//     /// </summary>
//     public static void OnRegisterGameClass()
//     {
//         Game.OnGameTriggerInitialization += RegisterTriggers;
//     }

//     /// <summary>
//     /// 注册所有客户端事件触发器
//     /// </summary>
//     private static void RegisterTriggers()
//     {
//         // 游戏开始事件
//         gameStartTrigger = new(static async (s, d) =>
//         {
//             Game.Logger.LogInformation("客户端游戏开始！");
//             return true;
//         });
//         gameStartTrigger.Register(Game.Instance);
//     }
// }

// #endif
#if CLIENT
using Events;
using GameCore.BaseInterface;
using GameCore.Event;
using GameCore.PlayerAndUsers;
using Microsoft.Extensions.Logging;
using TriggerEncapsulation.Event;
using TriggerEncapsulation.Messaging;
using System;
using System.Text;
using System.Text.Json;
using System.Numerics;
using GameUI.TriggerEvent;
using GameCore.OrderSystem;
using GameUI.Device;
using GameData;
using GameCore.AbilitySystem.Data;
using GameUI.Control;
using GameUI.Control.Primitive;
using GameUI.Control.Enum;
using GameUI.Control.Advanced;
using GameUI.Control.Struct;
using GameUI.Control.Extensions;
using GameUI.Brush;
using GameUI.Struct;
using System.Drawing;
using GameUI.Enum;
using GameSystemUI.AbilitySystemUI.Advanced;
using GameSystemUI.MoveKeyBoard.Advanced;

namespace GameEntry;

/// <summary>
/// 客户端游戏逻辑类
/// 负责处理UI交互、显示状态和客户端独有的逻辑
/// </summary>
public class GameClient : IGameClass
{
    // ==================== 事件触发器 ====================
    private static Trigger<EventGameStart>? gameStartTrigger;
    private static Trigger<EventPlayerMainUnitChanged>? mainUnitChangedTrigger;
    private static Trigger<EventServerMessage>? serverMessageTrigger;
    private static Trigger<EventGameKeyDown>? keyDownTrigger;

    // ==================== UI组件 ====================
    private static Panel? gameUI;
    private static Label? statusLabel;
    private static AbilityJoyStickGroup? abilityJoyStickGroup;
    private static MoveKeyBoard? moveKeyBoard;
    private static bool isInitialized = false;

    /// <summary>
    /// 框架初始化时自动调用此方法注册游戏类
    /// </summary>
    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += RegisterTriggers;
    }

    /// <summary>
    /// 注册所有客户端事件触发器
    /// </summary>
    private static void RegisterTriggers()
    {
        // 只有在JsonScopeDataTest游戏模式下才注册触发器
        if (Game.GameModeLink != ScopeData.GameMode.JsonScopeDataTest)
        {
            Game.Logger.LogInformation("当前游戏模式不是JsonScopeDataTest，跳过客户端逻辑初始化{GameModeLink}", Game.GameModeLink);
            return;
        }

        Game.Logger.LogInformation("✅ 游戏模式匹配JsonScopeDataTest，开始注册客户端触发器");

        // 游戏开始事件
        gameStartTrigger = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("客户端游戏开始！");

            // 初始化客户端UI
            InitializeClientUI();

            // 向服务器发送客户端准备就绪消息
            await SendMessageToServer("ClientReady", new { Message = "客户端已准备就绪" });

            return true;
        });
        gameStartTrigger.Register(Game.Instance);

        // 主控单位改变事件
        mainUnitChangedTrigger = new(static async (s, d) =>
        {
            var player = d.Player;
            var newUnit = d.Unit;

            Game.Logger.LogInformation("玩家 {PlayerId} 的主控单位改变为 {Unit}", player?.Id, newUnit);

            if (player == Player.LocalPlayer && newUnit != null)
            {
                Game.Logger.LogInformation("本地玩家获得主控单位哦");
                // 延迟初始化UI组件，等待主单位完全就绪
                DelayedInitializeGameSystemUI();
            }

            return true;
        });
        mainUnitChangedTrigger.Register(Player.LocalPlayer!);

        // 服务器消息处理
        serverMessageTrigger = new(OnServerMessageReceived);
        serverMessageTrigger.Register(Game.Instance);

        // 键盘输入处理 - 仅保留技能释放
        keyDownTrigger = new(OnKeyDownAsync);
        keyDownTrigger.Register(Game.Instance);
    }

    /// <summary>
    /// 初始化客户端UI界面
    /// </summary>
    private static void InitializeClientUI()
    {
        try
        {
            // 创建主面板
            gameUI = new Panel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                WidthStretchRatio = 1.0f,
                HeightStretchRatio = 1.0f,
            };

            // 状态标签
            statusLabel = new Label
            {
                Text = "Json数编测试",
                FontSize = 18,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 20, 0, 0),
            };

            // 设置父子关系
            statusLabel.Parent = gameUI;

            // 将游戏UI添加到根视图
            gameUI.AddToRoot();

            Game.Logger.LogInformation("✅ 客户端UI初始化完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 客户端UI初始化失败");
        }
    }

    /// <summary>
    /// 延迟初始化游戏系统UI，等待玩家和主单位就绪
    /// </summary>
    private static async void DelayedInitializeGameSystemUI()
    {
        try
        {
            Game.Logger.LogInformation("⚔️ 开始等待玩家和主单位就绪...");

            // 最多等待10秒，每500ms检查一次
            for (int i = 0; i < 20; i++)
            {
                var localPlayer = Player.LocalPlayer;
                if (localPlayer?.MainUnit != null)
                {
                    Game.Logger.LogInformation("✅ 玩家和主单位已就绪，开始初始化游戏系统UI");
                    InitializeGameSystemUI();
                    return;
                }

                Game.Logger.LogDebug("⏳ 等待玩家和主单位就绪... ({attempt}/20)", i + 1);
                await Game.Delay(TimeSpan.FromMilliseconds(500));
            }

            Game.Logger.LogWarning("⚠️ 等待玩家和主单位就绪超时，跳过游戏系统UI初始化");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 延迟初始化游戏系统UI时发生错误");
        }
    }

    /// <summary>
    /// 初始化游戏系统UI（技能摇杆和移动键盘）
    /// </summary>
    private static async void InitializeGameSystemUI()
    {
        try
        {
            var localPlayer = Player.LocalPlayer;
            if (localPlayer?.MainUnit == null)
            {
                Game.Logger.LogWarning("⚠️ 无法初始化游戏系统UI：本地玩家或主单位为空");
                return;
            }

            var mainUnit = localPlayer.MainUnit;

            // 等待一段时间确保单位完全加载
            await Game.Delay(TimeSpan.FromSeconds(1));

            // 初始化技能摇杆组
            try
            {
                if (abilityJoyStickGroup == null)
                {
                    abilityJoyStickGroup = new AbilityJoyStickGroup()
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(0, 0, 100, 120),
                        ZIndex = 900,
                        BindUnit = mainUnit
                    };
                    abilityJoyStickGroup.AddToRoot();
                    Game.Logger.LogInformation("✅ 技能摇杆已创建");
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogError("❌ 初始化技能摇杆时发生错误: {error}", ex.Message);
            }

            // 初始化移动键盘
            try
            {
                if (moveKeyBoard == null)
                {
                    moveKeyBoard = new MoveKeyBoard()
                    {
                        ZIndex = 850,
                        BindUnit = mainUnit
                    };
                    moveKeyBoard.AddToRoot();
                    Game.Logger.LogInformation("✅ 移动键盘已创建");
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogError("❌ 初始化移动键盘时发生错误: {error}", ex.Message);
            }

            isInitialized = true;
            Game.Logger.LogInformation("🎉 游戏系统UI初始化完成！");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 初始化游戏系统UI时发生错误");
        }
    }

    /// <summary>
    /// 处理来自服务器的消息
    /// </summary>
    private static async Task<bool> OnServerMessageReceived(object sender, EventServerMessage eventArgs)
    {
        try
        {
            var json = Encoding.UTF8.GetString(eventArgs.Message);
            var messageData = JsonSerializer.Deserialize<ServerMessage>(json);

            if (messageData?.Type == null) return false;

            Game.Logger.LogInformation("收到服务器消息: {MessageType} - {Content}",
                messageData.Type, messageData.Content);

            // 根据消息类型处理不同的服务器消息
            switch (messageData.Type)
            {
                case "Welcome":
                    Game.Logger.LogInformation("服务器欢迎消息: {Content}", messageData.Content);
                    break;

                case "GameState":
                    // 处理游戏状态更新
                    break;

                case "Notification":
                    // 处理通知消息
                    Game.Logger.LogInformation("服务器通知: {Content}", messageData.Content);
                    break;

                default:
                    Game.Logger.LogWarning("未知的服务器消息类型: {MessageType}", messageData.Type);
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "处理服务器消息时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 键盘输入处理 - 仅保留技能释放
    /// </summary>
    private static async Task<bool> OnKeyDownAsync(object sender, EventGameKeyDown eventArgs)
    {
        var localPlayer = Player.LocalPlayer;
        var mainUnit = localPlayer?.MainUnit;

        if (mainUnit == null)
        {
            return false;
        }

        if (eventArgs.IsRepeat)
        {
            return false;
        }

        switch (eventArgs.Key)
        {
            case GameCore.Platform.SDL.VirtualKey.F:
                FireBallSpell(mainUnit);
                break;
        }

        await Task.CompletedTask;
        return false; // 允许其他触发器处理
    }

    /// <summary>
    /// 火球术技能释放
    /// </summary>
    private static void FireBallSpell(Unit unit)
    {
        Command command = new()
        {
            Index = CommandIndex.Execute,
            Target = unit.Facing,
            Type = ComponentTagEx.AbilityManager,
            Flag = CommandFlag.None,
            // AbilityLink = GameEntry.ScopeData.Ability.FireBallSpell,
        };

        var result = command.IssueOrder(unit);
        if (result.IsSuccess)
        {
            Game.Logger.LogInformation("✅ 火球术释放成功");
        }
        else
        {
            Game.Logger.LogWarning("❌ 火球术释放失败: {result}", result);
        }
    }

    /// <summary>
    /// 向服务器发送消息
    /// </summary>
    private static async Task SendMessageToServer(string messageType, object data)
    {
        try
        {
            var clientMessage = new ClientMessage
            {
                Type = messageType,
                Data = data,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(clientMessage);
            var message = new ProtoCustomMessage
            {
                Message = Encoding.UTF8.GetBytes(json)
            };

            if (message.SendToServer())
            {
                Game.Logger.LogInformation("向服务器发送消息成功: {MessageType}", messageType);
            }
            else
            {
                Game.Logger.LogWarning("向服务器发送消息失败: {MessageType}", messageType);
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "发送消息到服务器时发生错误");
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public static void Cleanup()
    {
        Game.Logger.LogInformation("🧹 清理客户端资源...");

        try
        {
            // 清理事件触发器
            gameStartTrigger?.Destroy();
            mainUnitChangedTrigger?.Destroy();
            serverMessageTrigger?.Destroy();
            keyDownTrigger?.Destroy();

            gameStartTrigger = null;
            mainUnitChangedTrigger = null;
            serverMessageTrigger = null;
            keyDownTrigger = null;

            // 清理UI组件
            if (gameUI != null)
            {
                gameUI.RemoveFromParent();
                gameUI = null;
            }

            if (abilityJoyStickGroup != null)
            {
                abilityJoyStickGroup.RemoveFromParent();
                abilityJoyStickGroup = null;
            }

            if (moveKeyBoard != null)
            {
                moveKeyBoard.RemoveFromParent();
                moveKeyBoard = null;
            }

            statusLabel = null;
            isInitialized = false;

            Game.Logger.LogInformation("✅ 客户端资源清理完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 清理客户端资源时出错");
        }
    }
}

/// <summary>
/// 客户端消息数据结构
/// </summary>
public class ClientMessage
{
    public string Type { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 服务器消息数据结构
/// </summary>
public class ServerMessage
{
    public string? Type { get; set; }
    public string? Content { get; set; }
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; }
}

#endif