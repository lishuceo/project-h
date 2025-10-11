#if CLIENT || SERVER
using GameCore.ProtocolClientTransient;
using GameCore.ProtocolServerTransient;
using GameCore;
using GameCore.PlayerAndUsers;
using GameEntry.TowerDefenseGame.ShopSystem;
using System;

namespace GameEntry.TowerDefenseGame;

/// <summary>
/// 塔防游戏命令类型枚举
/// </summary>
public enum TowerDefenseCommandType
{
    StartGame,      // 开始游戏
    StopGame,       // 停止游戏
    PauseGame,      // 暂停游戏
    ResumeGame      // 恢复游戏
}

/// <summary>
/// 塔防游戏状态枚举
/// </summary>
public enum TowerDefenseGameState
{
    Idle,           // 空闲状态（未开始）
    Playing,        // 游戏进行中
    Paused,         // 游戏暂停
    GameOver        // 游戏结束
}

/// <summary>
/// 塔防游戏状态信息
/// </summary>
public struct TowerDefenseGameInfo
{
    public TowerDefenseGameState GameState { get; init; }
    public bool IsSuccess { get; init; }
    public int ResultCode { get; init; }  // 0=成功, 1=游戏已开始, 2=游戏未开始, 3=操作失败
    public int MonstersSpawned { get; init; }  // 已生成怪物数量
    public int WaveNumber { get; init; }       // 当前波数
    public float ElapsedTime { get; init; }    // 游戏时间
    public int PlayerHealth { get; init; }     // 玩家当前血量
    public int PlayerGold { get; init; }       // 玩家当前金币
}

/// <summary>
/// 客户端向服务端发送塔防游戏命令的协议
/// </summary>
public readonly struct ProtoTowerDefenseCommand : IProtocolClientTransient<ProtoTowerDefenseCommand>
{
    public required TowerDefenseCommandType CommandType { get; init; }
    public required int PlayerId { get; init; }

#if SERVER
    public readonly void Handle(Player player)
    {
        try
        {
            Game.Logger.LogInformation("🏰 Received tower defense command from client: {CommandType} (Player: {PlayerId})", 
                CommandType, player.Id);

            switch (CommandType)
            {
                case TowerDefenseCommandType.StartGame:
                    HandleStartGameCommand(player);
                    break;

                case TowerDefenseCommandType.StopGame:
                    HandleStopGameCommand(player);
                    break;

                case TowerDefenseCommandType.PauseGame:
                    HandlePauseGameCommand(player);
                    break;

                case TowerDefenseCommandType.ResumeGame:
                    HandleResumeGameCommand(player);
                    break;

                default:
                    Game.Logger.LogWarning("⚠️ Unknown tower defense command: {CommandType}", CommandType);
                    SendResultToClient(player, TowerDefenseGameState.Idle, false, 99, 0, 0, 0, 0, 0);
                    break;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling tower defense command: {CommandType}", CommandType);
            SendResultToClient(player, TowerDefenseGameState.Idle, false, 98, 0, 0, 0, 0, 0);
        }
    }

    private static void HandleStartGameCommand(Player player)
    {
        try
        {
            if (TowerDefenseServer.IsGameStarted)
            {
                Game.Logger.LogWarning("⚠️ Game is already started!");
                SendResultToClient(player, TowerDefenseGameState.Playing, false, 1, 
                    TowerDefenseServer.MonstersSpawned, TowerDefenseServer.WaveNumber, TowerDefenseServer.ElapsedTime,
                    TowerDefenseServer.PlayerHealth, TowerDefenseServer.PlayerGold);
                return;
            }

            // 启动游戏
            TowerDefenseServer.StartGame();

            Game.Logger.LogInformation("✅ Tower Defense game started successfully!");
            SendResultToClient(player, TowerDefenseGameState.Playing, true, 0, 0, 1, 0, 
                TowerDefenseServer.PlayerHealth, TowerDefenseServer.PlayerGold);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to start tower defense game");
            SendResultToClient(player, TowerDefenseGameState.Idle, false, 3, 0, 0, 0, 0, 0);
        }
    }

    private static void HandleStopGameCommand(Player player)
    {
        try
        {
            if (!TowerDefenseServer.IsGameStarted)
            {
                Game.Logger.LogWarning("⚠️ Game is not started!");
                SendResultToClient(player, TowerDefenseGameState.Idle, false, 2, 0, 0, 0, 0, 0);
                return;
            }

            // 停止游戏
            TowerDefenseServer.StopGame();

            Game.Logger.LogInformation("✅ Tower Defense game stopped successfully!");
            SendResultToClient(player, TowerDefenseGameState.Idle, true, 0, 0, 0, 0, 0, 0);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to stop tower defense game");
            SendResultToClient(player, TowerDefenseGameState.Idle, false, 3, 0, 0, 0, 0, 0);
        }
    }

    private static void HandlePauseGameCommand(Player player)
    {
        try
        {
            if (!TowerDefenseServer.IsGameStarted)
            {
                Game.Logger.LogWarning("⚠️ Cannot pause: Game is not started!");
                SendResultToClient(player, TowerDefenseGameState.Idle, false, 2, 0, 0, 0, 0, 0);
                return;
            }

            // TODO: 实现暂停逻辑
            Game.Logger.LogInformation("⏸️ Tower Defense game paused!");
            SendResultToClient(player, TowerDefenseGameState.Paused, true, 0, 
                TowerDefenseServer.MonstersSpawned, TowerDefenseServer.WaveNumber, TowerDefenseServer.ElapsedTime,
                TowerDefenseServer.PlayerHealth, TowerDefenseServer.PlayerGold);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to pause tower defense game");
            SendResultToClient(player, TowerDefenseGameState.Playing, false, 3, 
                TowerDefenseServer.MonstersSpawned, TowerDefenseServer.WaveNumber, TowerDefenseServer.ElapsedTime,
                TowerDefenseServer.PlayerHealth, TowerDefenseServer.PlayerGold);
        }
    }

    private static void HandleResumeGameCommand(Player player)
    {
        try
        {
            // TODO: 实现恢复逻辑
            Game.Logger.LogInformation("▶️ Tower Defense game resumed!");
            SendResultToClient(player, TowerDefenseGameState.Playing, true, 0, 
                TowerDefenseServer.MonstersSpawned, TowerDefenseServer.WaveNumber, TowerDefenseServer.ElapsedTime,
                TowerDefenseServer.PlayerHealth, TowerDefenseServer.PlayerGold);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to resume tower defense game");
            SendResultToClient(player, TowerDefenseGameState.Paused, false, 3, 
                TowerDefenseServer.MonstersSpawned, TowerDefenseServer.WaveNumber, TowerDefenseServer.ElapsedTime,
                TowerDefenseServer.PlayerHealth, TowerDefenseServer.PlayerGold);
        }
    }



    private static void SendResultToClient(Player player, TowerDefenseGameState gameState, bool isSuccess, 
        int resultCode, int monstersSpawned, int waveNumber, float elapsedTime, int playerHealth, int playerGold)
    {
        try
        {
            var result = new ProtoTowerDefenseResult
            {
                GameInfo = new TowerDefenseGameInfo
                {
                    GameState = gameState,
                    IsSuccess = isSuccess,
                    ResultCode = resultCode,
                    MonstersSpawned = monstersSpawned,
                    WaveNumber = waveNumber,
                    ElapsedTime = elapsedTime,
                    PlayerHealth = playerHealth,
                    PlayerGold = playerGold
                }
            };

            result.SendTo(player, null);
            Game.Logger.LogDebug("📡 Sent tower defense result to client: {IsSuccess} ({GameState})", isSuccess, gameState);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to send tower defense result to client");
        }
    }


#endif
}

/// <summary>
/// 服务端向客户端发送塔防游戏结果的协议
/// </summary>
public readonly struct ProtoTowerDefenseResult : IProtocolServerTransient<ProtoTowerDefenseResult>
{
    public required TowerDefenseGameInfo GameInfo { get; init; }

#if CLIENT
    public readonly void Handle()
    {
        try
        {
            Game.Logger.LogDebug("📡 Received tower defense result from server");
            TowerDefenseCommandSender.UpdateGameStateFromServer(GameInfo);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling tower defense result");
        }
    }
#endif
}



#if CLIENT
/// <summary>
/// 客户端塔防游戏命令发送器
/// </summary>
public static class TowerDefenseCommandSender
{
    private static TowerDefenseGameInfo? _lastGameInfo;

    /// <summary>
    /// 发送塔防游戏命令到服务端
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="playerId">玩家ID</param>
    /// <returns>是否发送成功</returns>
    public static bool SendCommand(TowerDefenseCommandType commandType, int playerId = 1)
    {
        try
        {
            var command = new ProtoTowerDefenseCommand
            {
                CommandType = commandType,
                PlayerId = playerId
            };

            bool success = command.SendToServer();
            
            if (success)
            {
                Game.Logger.LogInformation("📡 Tower defense command sent to server: {CommandType} (Player: {PlayerId})", 
                    commandType, playerId);
            }
            else
            {
                Game.Logger.LogError("❌ Failed to send tower defense command: {CommandType}", commandType);
            }

            return success;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Exception sending tower defense command: {CommandType}", commandType);
            return false;
        }
    }

    /// <summary>
    /// 从服务端更新游戏状态
    /// </summary>
    /// <param name="gameInfo">游戏状态信息</param>
    internal static void UpdateGameStateFromServer(TowerDefenseGameInfo gameInfo)
    {
        _lastGameInfo = gameInfo;
        
        // 更新客户端UI
        TowerDefenseClient.UpdateGameStateUI(gameInfo);
        
        var icon = gameInfo.IsSuccess ? "✅" : "❌";
        var message = GetResultMessage(gameInfo.ResultCode);
        Game.Logger.LogInformation("{Icon} Tower defense result: {Message} (State: {GameState}, Wave: {Wave}, Monsters: {Monsters}, Health: {Health}, Gold: {Gold})", 
            icon, message, gameInfo.GameState, gameInfo.WaveNumber, gameInfo.MonstersSpawned, gameInfo.PlayerHealth, gameInfo.PlayerGold);
    }

    /// <summary>
    /// 根据结果代码获取用户友好的消息
    /// </summary>
    private static string GetResultMessage(int resultCode)
    {
        return resultCode switch
        {
            0 => "操作成功",
            1 => "游戏已经开始",
            2 => "游戏尚未开始",
            3 => "操作失败",
            98 => "处理时发生异常",
            99 => "未知命令",
            _ => $"未知错误代码: {resultCode}"
        };
    }

    /// <summary>
    /// 获取最后一次游戏状态
    /// </summary>
    public static TowerDefenseGameInfo? GetLastGameInfo() => _lastGameInfo;
}
#endif

#endif
