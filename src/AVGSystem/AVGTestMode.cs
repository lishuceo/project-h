using GameCore;
using GameEntry.AVGSystem.Data;
using GameEntry.AVGSystem.Engine;

namespace GameEntry.AVGSystem;

/// <summary>
/// AVG系统测试模式 - 用于测试对话系统功能
/// </summary>
public class AVGTestMode : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.Logger.LogInformation("📖 注册AVG系统测试模式...");
        
        // 检查是否在AVG测试模式下
        Game.OnGameDataInitialization -= CheckAndInitialize;
        Game.OnGameDataInitialization += CheckAndInitialize;
    }

    private static void CheckAndInitialize()
    {
        // 检查当前游戏模式是否为AVG测试模式
        if (Game.GameModeLink == GameEntry.ScopeData.GameMode.AVGTest)
        {
            Game.Logger.LogInformation("📖 AVG测试模式检测到，初始化AVG系统...");
            
            // 注册UI初始化
            Game.OnGameUIInitialization -= InitializeAVGTest;
            Game.OnGameUIInitialization += InitializeAVGTest;
        }
    }

    private static async void InitializeAVGTest()
    {
        try
        {
            Game.Logger.LogInformation("📖 欢迎来到AVG系统测试模式！");
            
            // 注册剧本数据
            XianJianScripts.RegisterAllScripts();
            
            // 等待2秒确保系统就绪
            await Game.Delay(TimeSpan.FromSeconds(2));
            
#if CLIENT
            // 使用数据驱动的剧本系统
            await AVGScriptEngine.PlayScript("AVG_Test");
            
            // 测试完成后可以播放其他剧本
            var continueChoice = await AVGFullScreen.ShowChoice("想要体验其他剧情吗？", new[]
            {
                "仙剑开场剧情",
                "角色相遇剧情",
                "黑神话悟空故事",
                "背景图片展示测试",
                "退出测试"
            });
            
            switch (continueChoice)
            {
                case 0:
                    await AVGScriptEngine.PlayScript("XianJian_Opening");
                    break;
                case 1:
                    await AVGScriptEngine.PlayScript("XianJian_CharacterMeeting");
                    break;
                case 2:
                    await AVGScriptEngine.PlayScript("BlackWukong_Story");
                    break;
                case 3:
                    await AVGScriptEngine.PlayScript("Background_Test");
                    break;
                case 4:
                    await AVGFullScreen.ShowDialog("AVG系统", "感谢使用AVG系统！");
                    break;
            }
#endif
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ AVG系统测试失败");
        }
    }

    // 旧的硬编码测试方法已移除，现在使用数据驱动的剧本系统
}

#if CLIENT
/// <summary>
/// AVG系统客户端测试
/// </summary>
public class AVGTestClient : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.Logger.LogInformation("📖 注册AVG系统客户端测试...");
        
        // 检查是否在AVG测试模式下
        Game.OnGameDataInitialization -= CheckAndInitialize;
        Game.OnGameDataInitialization += CheckAndInitialize;
    }

    private static void CheckAndInitialize()
    {
        // 检查当前游戏模式是否为AVG测试模式
        if (Game.GameModeLink == GameEntry.ScopeData.GameMode.AVGTest)
        {
            Game.Logger.LogInformation("📖 AVG客户端测试模式检测到...");
            
            // 客户端特定的初始化（如果需要）
        }
    }
}
#endif
