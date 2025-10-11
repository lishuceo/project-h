#if CLIENT || SERVER
using GameCore.ProtocolClientTransient;
using GameCore.ProtocolServerTransient;
using GameCore.EntitySystem;
using GameCore.Behavior;
using GameCore.Execution;
using GameCore.BuffSystem;
using GameCore.BuffSystem.Manager;
using GameCore.BuffSystem.Data;
using GameData;
using System;

namespace GameEntry.GameUITest;

/// <summary>
/// Buff测试命令类型枚举
/// </summary>
public enum BuffTestCommandType
{
    AddStunBuff,    // 添加眩晕buff
    ClearAllBuffs,  // 清除所有buff
    AddTestBuff,    // 添加测试buff（可自定义类型）
    AddSlowDebuff   // 🐌 添加减速buff
}

/// <summary>
/// Buff测试结果信息（只包含值类型，避免序列化问题）
/// </summary>
public struct BuffTestResultInfo
{
    public bool IsSuccess { get; init; }
    public int ResultCode { get; init; } // 结果代码：0=成功，1=单位未找到，2=无BuffManager，3=添加失败，4=清除失败
    public int BuffCount { get; init; } // 当前buff数量
    public int AffectedCount { get; init; } // 受影响的buff数量
}

/// <summary>
/// 客户端向服务端发送Buff测试命令的协议
/// </summary>
public readonly struct ProtoBuffTestCommand : IProtocolClientTransient<ProtoBuffTestCommand>
{
    public required BuffTestCommandType CommandType { get; init; }
    public required int TargetUnitSyncId { get; init; }  // 目标单位的SyncId
    public required float Duration { get; init; }   // 持续时间（不可为空）

#if SERVER
    public readonly void Handle(Player player)
    {
        try
        {
            Game.Logger.LogInformation("🩸 Received buff test command from client: {CommandType} (Player: {PlayerId}, Target: {UnitId})", 
                CommandType, player.Id, TargetUnitSyncId);

            // 查找目标单位
            var targetUnit = Entity.GetById(TargetUnitSyncId) as Unit;
            if (targetUnit == null)
            {
                Game.Logger.LogWarning("⚠️ 目标单位未找到 (SyncId: {UnitId})", TargetUnitSyncId);
                SendResultToClient(player, false, 1, 0, 0); // ResultCode 1: 单位未找到
                return;
            }

            // 获取或创建BuffManager组件
            var buffManager = targetUnit.GetComponent<BuffManager>();
            if (buffManager == null)
            {
                try
                {
                    // 使用扩展方法创建BuffManager组件
                    buffManager = targetUnit.GetOrCreateComponent<BuffManager>();
                    Game.Logger.LogInformation("✅ 为单位创建了BuffManager组件: {UnitType}", targetUnit.GetType().Name);
                }
                catch (Exception ex)
                {
                    Game.Logger.LogError(ex, "⚠️ 无法为单位创建BuffManager组件");
                    SendResultToClient(player, false, 2, 0, 0); // ResultCode 2: 无BuffManager
                    return;
                }
            }

            // 执行相应的buff操作
            switch (CommandType)
            {
                case BuffTestCommandType.AddStunBuff:
                    HandleAddStunBuff(player, targetUnit, buffManager);
                    break;

                case BuffTestCommandType.ClearAllBuffs:
                    HandleClearAllBuffs(player, targetUnit, buffManager);
                    break;

                case BuffTestCommandType.AddTestBuff:
                    HandleAddTestBuff(player, targetUnit, buffManager, "$$default_units_ts.buff.眩晕.root", Duration > 0 ? Duration : 5.0f);
                    break;

                case BuffTestCommandType.AddSlowDebuff:
                    HandleAddSlowDebuff(player, targetUnit, buffManager);
                    break;

                default:
                    Game.Logger.LogWarning("⚠️ 未知的buff测试命令: {CommandType}", CommandType);
                    SendResultToClient(player, false, 99, 0, 0); // ResultCode 99: 未知命令
                    break;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling buff test command: {CommandType}", CommandType);
            SendResultToClient(player, false, 98, 0, 0); // ResultCode 98: 处理异常
        }
    }

    private static void HandleAddStunBuff(Player player, Unit targetUnit, BuffManager buffManager)
    {
        try
        {
            // 使用正确的方式添加Buff：Effect.FactoryCreateTree -> ResolveTarget -> new Buff
            var effect = (Effect.FactoryCreateTree(ScopeData.Effect.AddStunBuff, targetUnit, targetUnit) as EffectBuffAdd)!;
            _ = effect.ResolveTarget();
            var buff = new Buff(targetUnit, ScopeData.Buff.Stun, effect);
            
            var currentBuffCount = buffManager.GetAll().Count();
            
            Game.Logger.LogInformation("✅ 成功添加眩晕Buff (持续5秒), 目标: {UnitType}", targetUnit.GetType().Name);
            SendResultToClient(player, true, 0, currentBuffCount, 1); // ResultCode 0: 成功
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 添加眩晕Buff失败: {Message}", ex.Message);
            SendResultToClient(player, false, 3, 0, 0); // ResultCode 3: 添加失败
        }
    }

    private static void HandleClearAllBuffs(Player player, Unit targetUnit, BuffManager buffManager)
    {
        try
        {
            var allBuffsBefore = buffManager.GetAll().ToList();
            int beforeCount = allBuffsBefore.Count;

            // ✅ 使用GameDataEffectBuffRemove来移除所有buff
            var effect = Effect.FactoryCreateTree(ScopeData.Effect.RemoveAllBuffs, targetUnit, targetUnit);
            _ = effect.ResolveTarget();
            effect.Execute();

            var allBuffsAfter = buffManager.GetAll().ToList();
            int afterCount = allBuffsAfter.Count;
            int removedCount = beforeCount - afterCount;
            
            Game.Logger.LogInformation("✅ 使用GameDataEffectBuffRemove成功清除 {removedCount}/{totalCount} 个Buff, 目标: {UnitType}", removedCount, beforeCount, targetUnit.GetType().Name);
            SendResultToClient(player, true, 0, afterCount, removedCount); // ResultCode 0: 成功
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 使用GameDataEffectBuffRemove清除Buff失败: {Message}", ex.Message);
            SendResultToClient(player, false, 4, 0, 0); // ResultCode 4: 清除失败
        }
    }

    private static void HandleAddTestBuff(Player player, Unit targetUnit, BuffManager buffManager, string buffLinkPath, float duration)
    {
        try
        {
            // 使用正确的方式添加Buff：Effect.FactoryCreateTree -> ResolveTarget -> new Buff
            var effect = (Effect.FactoryCreateTree(ScopeData.Effect.AddTestBuff, targetUnit, targetUnit) as EffectBuffAdd)!;
            _ = effect.ResolveTarget();
            var buff = new Buff(targetUnit, ScopeData.Buff.TestBuff, effect);
            
            var currentBuffCount = buffManager.GetAll().Count();
            
            Game.Logger.LogInformation("✅ 成功添加测试Buff (持续{duration:F1}秒), 目标: {UnitType}, Buff: {BuffLink}", duration, targetUnit.GetType().Name, buffLinkPath);
            SendResultToClient(player, true, 0, currentBuffCount, 1); // ResultCode 0: 成功
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 添加测试Buff失败: {Message}", ex.Message);
            SendResultToClient(player, false, 3, 0, 0); // ResultCode 3: 添加失败
        }
    }

    private static void HandleAddSlowDebuff(Player player, Unit targetUnit, BuffManager buffManager)
    {
        try
        {
            // 🐌 使用塔防模板的减速Buff - 需要创建一个简单的BuffAdd效果
            // 由于塔防模板的SlowDebuff需要特定的Effect，我们创建一个简单的BuffAdd效果
            var effect = (Effect.FactoryCreateTree(ScopeData.Effect.AddTestBuff, targetUnit, targetUnit) as EffectBuffAdd)!;
            _ = effect.ResolveTarget();
            
            // 🐌 直接使用塔防模板的SlowDebuff
            var buff = new Buff(targetUnit, ScopeData.Buff.SlowDebuff, effect);
            
            var currentBuffCount = buffManager.GetAll().Count();
            
            Game.Logger.LogInformation("✅ 成功添加减速Buff (持续2秒，减速50%), 目标: {UnitType}", targetUnit.GetType().Name);
            SendResultToClient(player, true, 0, currentBuffCount, 1); // ResultCode 0: 成功
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 添加减速Buff失败: {Message}", ex.Message);
            SendResultToClient(player, false, 3, 0, 0); // ResultCode 3: 添加失败
        }
    }

    private static void SendResultToClient(Player player, bool isSuccess, int resultCode, int buffCount, int affectedCount)
    {
        try
        {
            var result = new ProtoBuffTestResult
            {
                ResultInfo = new BuffTestResultInfo
                {
                    IsSuccess = isSuccess,
                    ResultCode = resultCode,
                    BuffCount = buffCount,
                    AffectedCount = affectedCount
                }
            };

            // 使用正确的服务端协议发送方法 - 需要ref参数
            result.SendTo(player, null);
            Game.Logger.LogDebug("📡 Sent buff test result to client: {IsSuccess}", isSuccess);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to send buff test result to client");
        }
    }
#endif
}

/// <summary>
/// 服务端向客户端发送Buff测试结果的协议
/// </summary>
public readonly struct ProtoBuffTestResult : IProtocolServerTransient<ProtoBuffTestResult>
{
    public required BuffTestResultInfo ResultInfo { get; init; }

#if CLIENT
    public readonly void Handle()
    {
        try
        {
            Game.Logger.LogDebug("📡 Received buff test result from server");
            BuffTestCommandSender.UpdateResultFromServer(ResultInfo);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling buff test result");
        }
    }
#endif
}

#if CLIENT
/// <summary>
/// 客户端Buff测试命令发送器
/// </summary>
public static class BuffTestCommandSender
{
    private static BuffTestResultInfo? _lastResult;

    /// <summary>
    /// 发送Buff测试命令到服务端
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="targetUnitSyncId">目标单位SyncId</param>
    /// <param name="duration">持续时间</param>
    /// <returns>是否发送成功</returns>
    public static bool SendCommand(BuffTestCommandType commandType, int targetUnitSyncId, float duration = 5.0f)
    {
        try
        {
            var command = new ProtoBuffTestCommand
            {
                CommandType = commandType,
                TargetUnitSyncId = targetUnitSyncId,
                Duration = duration
            };

            bool success = command.SendToServer();
            
            if (success)
            {
                Game.Logger.LogInformation("📡 Buff test command sent to server: {CommandType} (Target: {UnitId})", commandType, targetUnitSyncId);
            }
            else
            {
                Game.Logger.LogError("❌ Failed to send buff test command: {CommandType}", commandType);
            }

            return success;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Exception sending buff test command: {CommandType}", commandType);
            return false;
        }
    }

    /// <summary>
    /// 从服务端更新测试结果
    /// </summary>
    /// <param name="resultInfo">结果信息</param>
    internal static void UpdateResultFromServer(BuffTestResultInfo resultInfo)
    {
        _lastResult = resultInfo;
        
        // 客户端现在通过定时器自动刷新，无需手动更新UI
        
        var icon = resultInfo.IsSuccess ? "✅" : "❌";
        var message = GetResultMessage(resultInfo.ResultCode);
        Game.Logger.LogInformation("{Icon} Buff test result: {Message} (Buffs: {Count}, Affected: {Affected})", 
            icon, message, resultInfo.BuffCount, resultInfo.AffectedCount);
    }

    /// <summary>
    /// 根据结果代码获取用户友好的消息
    /// </summary>
    private static string GetResultMessage(int resultCode)
    {
        return resultCode switch
        {
            0 => "操作成功",
            1 => "目标单位未找到",
            2 => "无法创建BuffManager组件",
            3 => "添加Buff失败",
            4 => "清除Buff失败",
            98 => "处理时发生异常",
            99 => "未知命令",
            _ => $"未知错误代码: {resultCode}"
        };
    }

    /// <summary>
    /// 获取最后一次测试结果
    /// </summary>
    public static BuffTestResultInfo? GetLastResult() => _lastResult;
}
#endif

#endif
