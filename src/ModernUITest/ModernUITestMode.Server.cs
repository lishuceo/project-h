#if SERVER
using GameCore.GameSystem;

namespace GameEntry.ModernUITest;

/// <summary>
/// Modern UI Test Mode - 服务端初始化
/// </summary>
/// <remarks>
/// 负责在服务端初始化UI属性系统，确保客户端-服务端通信正常工作
/// </remarks>
public class ModernUITestServerMode : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.Logger.LogInformation("Modern UI Test Server Mode registered");
    }

    private static void OnGameTriggerInitialization()
    {
        // 只在ModernUITest模式下初始化
        if (Game.GameModeLink != ScopeData.GameMode.ModernUITest)
        {
            return;
        }

        // 初始化TypedMessageHandler系统
        TriggerEncapsulation.Messaging.TypedMessageHandler.Initialize();
        
        // 初始化UI属性系统
        TriggerEncapsulation.UIProperty.UIPropertyUsageExample.Initialize();
        
        Game.Logger.LogInformation("Modern UI Test server systems initialized");
    }
}
#endif
