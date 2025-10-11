using GameCore.GameSystem.Data;
using GameEntry.ARPGTemplate;

namespace GameEntry;

public class GlobalConfig : IGameClass
{
    public static void OnRegisterGameClass()
    {
        // Register the game mod for the game system.
        // in non-testing (online) mode, the server will send game mode strings to the engine,
        // and the engine will use this to determine which game mode to use.
        GameDataGlobalConfig.AvailableGameModes = new()
        {
            // 默认游戏模式，3D场景，有主控单位，敌方单位，展示创建单位，服务端下达指令，客户端下达指令，客户端搜索和拾取物品等。
            {"", GameCore.ScopeData.GameMode.Default},
            // 3D游戏模式，使用3D场景，经典的吸血鬼幸存者游戏模式。
            // 有主控单位，敌方单位，展示被动技能的使用。怪物刷新和追击玩家机制。
            {"VampireSurvivors3D", ScopeData.GameMode.VampireSurvivors3D},
            // 2D游戏模式，使用Canvas. 但并非最佳实践。
            {"VampireSurvivors2D", ScopeData.GameMode.VampireSurvivors2D},
            // 经典的 Flappy Bird 2D游戏模式，使用Canvas. 是Canvas样例。但并非普通UI的最佳实践。
            {"FlappyBird", ScopeData.GameMode.FlappyBird},
            // 3D游戏模式，使用默认游戏模式场景，测试摇杆。
            {"JoyStickTest", ScopeData.GameMode.JoyStickTest},
            // 测试服务器和客户端的自定义消息通信。
            {"TypedMessageTest", ScopeData.GameMode.TypedMessageTest},
            // 五子棋游戏。2D游戏。在服务端运行逻辑，在客户端使用2D UI系统来展示。支持双人联机。
            {"Gomoku", ScopeData.GameMode.Gomoku},
            // 测试用户云数据。也可作为用户数据同步的参考。
            {"UserCloudDataTest", ScopeData.GameMode.UserCloudDataTest},
            // 测试AI系统。
            {"AISystemTest", ScopeData.GameMode.AISystemTest},
            // 测试优化后的UI框架和设计系统，流式扩展，AI辅助UI开发的最佳实践
            {"UIFrameworkTest", ScopeData.GameMode.UIFrameworkTest},
            // 普通的UI流式扩展测试系统
            {"ModernUITest", ScopeData.GameMode.ModernUITest},
            // 测试基本形状。
            {"PrimitiveShapeTest", ScopeData.GameMode.PrimitiveShapeTest},
            // 测试Buff系统。
            {"BuffTest", ScopeData.GameMode.BuffTest},
            // 测试脚本性能。
            {"ScriptPerformanceTest", ScopeData.GameMode.ScriptPerformanceTest},
            // 2048游戏。2D游戏。不使用Canvas，而是普通UI。
            {"Game2048", ScopeData.GameMode.Game2048},
            // 跳一跳游戏。3D游戏。纯客户端逻辑。目前非常不完善。
            {"JumpJump", ScopeData.GameMode.JumpJump},
            // 测试物理游戏。
            // {"PhysicsGame", PhysicsGameData.GameMode.PhysicsGame},
            // 测试TouchBehavior行为的UI测试模式，流式扩展，AI辅助UI开发中按钮长按功能的最佳实践
            {"TouchBehaviorTest", ScopeData.GameMode.TouchBehaviorTest},
            {"TowerDefense", ScopeData.GameMode.TowerDefense},
            {"ARPGMode", ScopeData.GameMode.ARPGMode},
            {"UIShowcaseDemo", ScopeData.GameMode.UIShowcaseDemo},
            {"XianJianQiXiaZhuan", ScopeData.GameMode.XianJianQiXiaZhuan},
            {"AVGTest", ScopeData.GameMode.AVGTest},
            {"RPGRoleTest", ScopeData.GameMode.RPGRoleTest},
        };
        // Set the default game mode for testing, this will be used when the game is in testing mode.
        // GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.TowerDefense;
        // GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.ARPGMode;
        GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.RPGRoleTest;
        // GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.XianJianQiXiaZhuan;
        // GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.FlappyBird;
        // Set the single-player test slot ID, this defines which player slot the local player will use in
        // single-player testing mode.
        GameDataGlobalConfig.SinglePlayerTestSlotId = 1;
    }
}
