#if CLIENT || SERVER
using GameCore.ProtocolClientTransient;
using GameCore.ProtocolServerTransient;

namespace GameEntry.AISystemTest;

/// <summary>
/// AI测试命令类型枚举
/// </summary>
public enum AITestCommandType
{
    StartTest,      // 开始测试序列
    StartValidation, // 开始验证
    Reset,          // 重置测试状态
    NextPhase,      // 推进到下一阶段
    StartCombat,    // 开始AI战斗测试
    StopCombat      // 停止AI战斗测试
}

/// <summary>
/// AI测试状态信息（只包含值类型，避免序列化问题）
/// </summary>
public struct AITestStatusInfo
{
    public int CurrentPhase { get; init; }
    public bool IsTestRunning { get; init; }
    public bool IsWaitingForNextPhase { get; init; }
    public int UnitCount { get; init; }
    public double ElapsedSeconds { get; init; } // 测试运行时间，客户端用于生成描述
}

/// <summary>
/// 客户端向服务端发送AI测试命令的协议
/// </summary>
public readonly struct ProtoAITestCommand : IProtocolClientTransient<ProtoAITestCommand>
{
    public required AITestCommandType CommandType { get; init; }

#if SERVER
    public readonly void Handle(Player player)
    {
        try
        {
            Game.Logger.LogInformation("🎮 Received AI test command from client: {CommandType} (Player: {PlayerId})", 
                CommandType, player.Id);

            switch (CommandType)
            {
                case AITestCommandType.StartTest:
                    _ = AISystemTestServer.StartAITestSequence();
                    Game.Logger.LogInformation("✅ AI test sequence started by client request");
                    break;

                case AITestCommandType.StartValidation:
                    _ = AISystemTestServer.StartAIValidation();
                    Game.Logger.LogInformation("✅ AI validation started by client request");
                    break;

                case AITestCommandType.Reset:
                    AISystemTestServer.ResetTestState();
                    Game.Logger.LogInformation("✅ AI test state reset by client request");
                    break;

                case AITestCommandType.NextPhase:
                    AISystemTestServer.TriggerNextPhase();
                    Game.Logger.LogInformation("✅ Next phase triggered by client request");
                    break;

                case AITestCommandType.StartCombat:
                    _ = AISystemTestServer.StartAICombatTestCommand();
                    Game.Logger.LogInformation("✅ AI combat test started by client request");
                    break;

                case AITestCommandType.StopCombat:
                    _ = AISystemTestServer.StopAICombatTestCommand();
                    Game.Logger.LogInformation("✅ AI combat test stopped by client request");
                    break;

                default:
                    Game.Logger.LogWarning("⚠️ Unknown AI test command type: {CommandType}", CommandType);
                    break;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling AI test command: {CommandType}", CommandType);
        }
    }
#endif
}

/// <summary>
/// 服务端向客户端发送AI测试状态更新的协议
/// </summary>
public readonly struct ProtoAITestStatusUpdate : IProtocolServerTransient<ProtoAITestStatusUpdate>
{
    public required AITestStatusInfo StatusInfo { get; init; }

#if CLIENT
    public readonly void Handle()
    {
        try
        {
            Game.Logger.LogDebug("📡 Received AI test status update from server");
            AISystemTestClient.UpdateStatusFromServer(StatusInfo);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling AI test status update");
        }
    }
#endif
}

#if CLIENT
/// <summary>
/// 客户端AI测试命令发送器
/// </summary>
public static class AITestCommandSender
{
    /// <summary>
    /// 发送AI测试命令到服务端
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <returns>是否发送成功</returns>
    public static bool SendCommand(AITestCommandType commandType)
    {
        try
        {
            var command = new ProtoAITestCommand
            {
                CommandType = commandType
            };

            bool success = command.SendToServer();
            
            if (success)
            {
                Game.Logger.LogInformation("📡 AI test command sent to server: {CommandType}", commandType);
            }
            else
            {
                Game.Logger.LogError("❌ Failed to send AI test command: {CommandType}", commandType);
            }

            return success;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Exception sending AI test command: {CommandType}", commandType);
            return false;
        }
    }
}
#endif
#endif 