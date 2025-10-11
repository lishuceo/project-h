#if CLIENT || SERVER
using GameCore.ProtocolClientTransient;
using GameCore.ProtocolServerTransient;
using GameCore;
using GameCore.PlayerAndUsers;
using GameCore.EntitySystem;
using GameCore.Leveling;
using GameEntry.TowerDefenseGame.TowerUpgradeSystem;

namespace GameEntry.TowerDefenseGame.TowerUISystem;

/// <summary>
/// 塔协议命令类型枚举
/// </summary>
public enum TowerProtocolCommandType
{
    Upgrade = 1,    // 升级塔
    Demolish = 2    // 拆除塔
}

/// <summary>
/// 塔协议命令数据结构
/// </summary>
public struct TowerProtocolCommandData
{
    public TowerProtocolCommandType CommandType { get; init; }
    public int TowerId { get; init; }           // 塔的唯一标识
    public TowerType TowerType { get; init; }   // 塔的类型
    public int CurrentLevel { get; init; }      // 当前等级
    public int Price { get; init; }             // 升级价格或回收金额
}

/// <summary>
/// 塔命令结果数据
/// </summary>
public struct TowerProtocolCommandResult
{
    public TowerProtocolCommandType CommandType { get; init; }
    public bool IsSuccess { get; init; }
    public int ResultCode { get; init; }        // 结果代码：0=成功, 1=金币不足, 2=等级已满, 3=塔不存在
    public int NewLevel { get; init; }          // 升级后的等级
    public int NewGoldAmount { get; init; }     // 操作后的金币数量
}

/// <summary>
/// 客户端向服务端发送塔命令的协议
/// </summary>
public readonly struct ProtoTowerCommand : IProtocolClientTransient<ProtoTowerCommand>
{
    public required TowerProtocolCommandData CommandData { get; init; }
    public required int PlayerId { get; init; }

#if SERVER
    public readonly void Handle(Player player)
    {
        try
        {
            Game.Logger.LogInformation("🏰 Received tower command from client: {CommandType} (Player: {PlayerId})", 
                CommandData.CommandType, player.Id);

            // 根据塔ID查找塔单位
            var tower = FindTowerById(CommandData.TowerId);
            if (tower == null)
            {
                Game.Logger.LogError("❌ Tower not found: ID {TowerId}", CommandData.TowerId);
                SendResultToClient(player, (TowerCommandType)(int)CommandData.CommandType, false, "塔不存在", 0, 0);
                return;
            }

            // 处理不同类型的命令
            TowerProtocolCommandResult result = CommandData.CommandType switch
            {
                TowerProtocolCommandType.Upgrade => HandleUpgradeCommand(player, tower, CommandData),
                TowerProtocolCommandType.Demolish => HandleDemolishCommand(player, tower, CommandData),
                _ => new TowerProtocolCommandResult
                {
                    CommandType = CommandData.CommandType,
                    IsSuccess = false,
                    ResultCode = 99,
                    NewLevel = 0,
                    NewGoldAmount = 0
                }
            };

            // 发送结果到客户端
            SendResultToClient(player, (TowerCommandType)(int)result.CommandType, result.IsSuccess, "success", 
                result.NewLevel, result.NewGoldAmount);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling tower command: {CommandType}", CommandData.CommandType);
            SendResultToClient(player, (TowerCommandType)(int)CommandData.CommandType, false, "服务器处理异常", 0, 0);
        }
    }

    /// <summary>
    /// 处理塔升级命令
    /// </summary>
    private static TowerProtocolCommandResult HandleUpgradeCommand(Player player, Unit tower, TowerProtocolCommandData commandData)
    {
        try
        {
            var commandResult = TowerCommandHandler.HandleUpgradeCommand(
                new GameCore.OrderSystem.Command { Player = player, Target = tower }, tower);
                
            if (commandResult.IsSuccess)
            {
                var newLevel = TowerLevelHelper.GetTowerLevel(tower);
                var newGoldAmount = ResourceSystem.PlayerResourceManager.GetPlayerGold(player.Id);
                
                Game.Logger.LogInformation("✅ Tower upgraded successfully to level {NewLevel}", newLevel);
                    
                return new TowerProtocolCommandResult
                {
                    CommandType = TowerProtocolCommandType.Upgrade,
                    IsSuccess = true,
                    ResultCode = 0,
                    NewLevel = newLevel,
                    NewGoldAmount = newGoldAmount
                };
            }
            else
            {
                return new TowerProtocolCommandResult
                {
                    CommandType = TowerProtocolCommandType.Upgrade,
                    IsSuccess = false,
                    ResultCode = 1,
                    NewLevel = commandData.CurrentLevel,
                    NewGoldAmount = ResourceSystem.PlayerResourceManager.GetPlayerGold(player.Id)
                };
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error processing tower upgrade");
            return new TowerProtocolCommandResult
            {
                CommandType = TowerProtocolCommandType.Upgrade,
                IsSuccess = false,
                ResultCode = 98,
                NewLevel = commandData.CurrentLevel,
                NewGoldAmount = ResourceSystem.PlayerResourceManager.GetPlayerGold(player.Id)
            };
        }
    }

    /// <summary>
    /// 处理塔拆除命令
    /// </summary>
    private static TowerProtocolCommandResult HandleDemolishCommand(Player player, Unit tower, TowerProtocolCommandData commandData)
    {
        try
        {
            var commandResult = TowerCommandHandler.HandleDemolishCommand(
                new GameCore.OrderSystem.Command { Player = player, Target = tower }, tower);
                
            if (commandResult.IsSuccess)
            {
                var newGoldAmount = ResourceSystem.PlayerResourceManager.GetPlayerGold(player.Id);
                
                Game.Logger.LogInformation("✅ Tower demolished successfully, refunded {Price} gold", 
                    commandData.Price);
                    
                return new TowerProtocolCommandResult
                {
                    CommandType = TowerProtocolCommandType.Demolish,
                    IsSuccess = true,
                    ResultCode = 0,
                    NewLevel = 0,
                    NewGoldAmount = newGoldAmount
                };
            }
            else
            {
                return new TowerProtocolCommandResult
                {
                    CommandType = TowerProtocolCommandType.Demolish,
                    IsSuccess = false,
                    ResultCode = 1,
                    NewLevel = commandData.CurrentLevel,
                    NewGoldAmount = ResourceSystem.PlayerResourceManager.GetPlayerGold(player.Id)
                };
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error processing tower demolish");
            return new TowerProtocolCommandResult
            {
                CommandType = TowerProtocolCommandType.Demolish,
                IsSuccess = false,
                ResultCode = 98,
                NewLevel = commandData.CurrentLevel,
                NewGoldAmount = ResourceSystem.PlayerResourceManager.GetPlayerGold(player.Id)
            };
        }
    }

    /// <summary>
    /// 根据ID查找塔单位
    /// </summary>
    private static Unit? FindTowerById(int towerId)
    {
        try
        {
            // 通过TowerDefenseServer获取已注册的建筑
            return TowerDefenseServer.FindBuildingById(towerId);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error finding tower by ID: {TowerId}", towerId);
            return null;
        }
    }

    /// <summary>
    /// 发送结果到客户端
    /// </summary>
    private static void SendResultToClient(Player player, TowerCommandType commandType, bool isSuccess, 
        string message, int newLevel, int newGoldAmount)
    {
        try
        {
            var resultData = new TowerProtocolCommandResult
            {
                CommandType = (TowerProtocolCommandType)(int)commandType,
                IsSuccess = isSuccess,
                ResultCode = isSuccess ? 0 : 1,
                NewLevel = newLevel,
                NewGoldAmount = newGoldAmount
            };
            var result = new ProtoTowerProtocolCommandResult
            {
                Result = resultData
            };

            result.SendTo(player, null);
            Game.Logger.LogDebug("📡 Sent tower command result to client: {IsSuccess} ({CommandType})", 
                isSuccess, commandType);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to send tower command result to client");
        }
    }
#endif
}

/// <summary>
/// 服务端向客户端发送塔命令结果的协议
/// </summary>
public readonly struct ProtoTowerProtocolCommandResult : IProtocolServerTransient<ProtoTowerProtocolCommandResult>
{
    public required TowerProtocolCommandResult Result { get; init; }

#if CLIENT
    public readonly void Handle()
    {
        try
        {
            Game.Logger.LogInformation("📡 Received tower command result from server: {IsSuccess} - Code: {ResultCode}", 
                Result.IsSuccess, Result.ResultCode);
                
            // 更新客户端UI
            TowerProtocolCommandResultHandler.HandleResult(Result);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling tower command result");
        }
    }
#endif
}

#if CLIENT
/// <summary>
/// 客户端塔命令发送器（新版本）
/// </summary>
public static class TowerCommandSender
{
    /// <summary>
    /// 发送塔升级命令
    /// </summary>
    public static bool SendUpgradeCommand(Unit tower, int upgradePrice)
    {
        return SendTowerCommand(tower, TowerCommandType.Upgrade, upgradePrice);
    }

    /// <summary>
    /// 发送塔拆除命令
    /// </summary>
    public static bool SendDemolishCommand(Unit tower, int refundAmount)
    {
        return SendTowerCommand(tower, TowerCommandType.Demolish, refundAmount);
    }

    /// <summary>
    /// 通用塔命令发送方法
    /// </summary>
    private static bool SendTowerCommand(Unit tower, TowerCommandType commandType, int price)
    {
        try
        {
            if (tower == null)
            {
                Game.Logger.LogError("❌ Cannot send tower command: tower is null");
                return false;
            }

            var localPlayer = Player.LocalPlayer;
            if (localPlayer == null)
            {
                Game.Logger.LogError("❌ Cannot send tower command: local player is null");
                return false;
            }

            var commandData = new TowerProtocolCommandData
            {
                CommandType = commandType == TowerCommandType.Upgrade ? TowerProtocolCommandType.Upgrade : TowerProtocolCommandType.Demolish,
                TowerId = tower.SyncId,
                TowerType = GetTowerTypeFromUnit(tower),
                CurrentLevel = TowerLevelHelper.GetTowerLevel(tower),
                Price = price,

            };

            var command = new ProtoTowerCommand
            {
                CommandData = commandData,
                PlayerId = localPlayer.Id
            };

            bool success = command.SendToServer();
            
            if (success)
            {
                            Game.Logger.LogInformation("✅ Tower command sent to server: {CommandType} (Price: {Price})", 
                commandType, price);
            }
            else
            {
                Game.Logger.LogError("❌ Failed to send tower command to server");
            }

            return success;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error sending tower command");
            return false;
        }
    }

    /// <summary>
    /// 从单位获取塔类型
    /// </summary>
    private static TowerType GetTowerTypeFromUnit(Unit unit)
    {
        var unitName = unit.Cache?.Name ?? "";
        return unitName switch
        {
            "单体减速塔" => TowerType.SlowTower,
            "光环减速塔" => TowerType.AuraSlowTower,
            "群体伤害塔" => TowerType.AOETower,
            "向量穿透塔" => TowerType.PenetrateTower,
            _ => TowerType.SlowTower
        };
    }
}

/// <summary>
/// 客户端塔命令结果处理器
/// </summary>
public static class TowerProtocolCommandResultHandler
{
    /// <summary>
    /// 处理服务器返回的塔命令结果
    /// </summary>
    public static void HandleResult(TowerProtocolCommandResult result)
    {
        try
        {
            // 更新金币显示
            if (result.NewGoldAmount > 0)
            {
                TowerDefenseClient.UpdateGoldDisplay(result.NewGoldAmount);
            }

            // 显示操作结果消息
            var statusText = result.IsSuccess 
                ? GetSuccessMessage(result.CommandType, result.ResultCode) 
                : GetErrorMessage(result.ResultCode);
            
            TowerDefenseClient.UpdateStatusText(statusText);

            // 根据命令类型执行特定操作
            switch (result.CommandType)
            {
                case TowerProtocolCommandType.Upgrade:
                    if (result.IsSuccess)
                    {
                        // 刷新塔UI显示新等级
                        TowerUIVisual.RefreshSelectedTowerUI();
                    }
                    break;

                case TowerProtocolCommandType.Demolish:
                    if (result.IsSuccess)
                    {
                        // 隐藏塔UI（因为塔已被拆除）
                        // TowerUIVisual.HideTowerUI();
                        Game.Logger.LogInformation("🏗️ Tower demolished, UI will be hidden");
                    }
                    break;
            }

            Game.Logger.LogInformation("🎯 Tower command result processed: {CommandType} - {IsSuccess}", 
                result.CommandType, result.IsSuccess);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error processing tower command result");
        }
    }

    /// <summary>
    /// 获取成功消息
    /// </summary>
    private static string GetSuccessMessage(TowerProtocolCommandType commandType, int resultCode)
    {
        return commandType switch
        {
            TowerProtocolCommandType.Upgrade => "✅ 塔升级成功！",
            TowerProtocolCommandType.Demolish => "✅ 塔拆除成功！",
            _ => "✅ 操作成功！"
        };
    }

    /// <summary>
    /// 获取错误消息
    /// </summary>
    private static string GetErrorMessage(int resultCode)
    {
        return resultCode switch
        {
            1 => "❌ 金币不足",
            2 => "❌ 塔已达到最高等级",
            3 => "❌ 塔不存在",
            98 => "❌ 服务器处理异常",
            99 => "❌ 未知命令类型",
            _ => $"❌ 错误代码: {resultCode}"
        };
    }
}
#endif

/// <summary>
/// 获取塔的当前等级 - 共享方法
/// </summary>
internal static class TowerLevelHelper
{
    /// <summary>
    /// 获取塔的当前等级
    /// </summary>
    public static int GetTowerLevel(Unit tower)
    {
        try
        {
            var level = tower.GetProperty<int>(PropertyUnit.Level);
            return level;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 获取塔等级失败: {towerName}", tower.Cache?.Name ?? "Unknown");
            return 1; // 默认等级
        }
    }
}

#endif
