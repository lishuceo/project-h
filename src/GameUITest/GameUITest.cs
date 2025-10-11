#if CLIENT
using GameCore.GameSystem.Data;
using GameCore;

namespace GameEntry.GameUITest;

/// <summary>
/// GameUITest模块主入口类
/// 负责统一管理所有UI测试功能
/// </summary>
public class GameUITest : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.Logger?.LogInformation("🎮 注册GameUITest模块...");

        // 检查是否在GameUITest模式下
        Game.OnGameDataInitialization += CheckAndInitialize;
    }

    /// <summary>
    /// 检查游戏模式并初始化
    /// </summary>
    private static void CheckAndInitialize()
    {
        // 检查当前是否为GameUITest模式
        if (Game.GameModeLink == ScopeData.GameMode.GameUITest)
        {
            Game.Logger?.LogInformation("✅ 检测到GameUITest模式，初始化UI测试模块...");
            
            // 注册UI初始化事件
            Game.OnGameUIInitialization += InitializeGameUITestModule;
        }
    }

    /// <summary>
    /// 初始化GameUITest模块
    /// </summary>
    private static void InitializeGameUITestModule()
    {
        try
        {
            Game.Logger?.LogInformation("🚀 启动GameUITest模块...");

            // 注册子测试类
            JoyStickTestExample.OnRegisterGameClass();
            BuffListTestExample.OnRegisterGameClass();
            
            // 启动主测试界面
            GameUITestMode.OnRegisterGameClass();

            Game.Logger?.LogInformation("✅ GameUITest模块启动成功喵！");
        }
        catch (Exception ex)
        {
            Game.Logger?.LogError(ex, "❌ GameUITest模块启动失败: {message}", ex.Message);
        }
    }
}
#endif
