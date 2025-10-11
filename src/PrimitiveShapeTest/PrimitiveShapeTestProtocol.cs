#if CLIENT || SERVER
using GameCore.ProtocolClientTransient;
using GameCore.ProtocolServerTransient;
using GameCore.Shape.Data;
using System.Numerics;

namespace GameEntry.PrimitiveShapeTest;

/// <summary>
/// 基本形状测试命令类型枚举
/// </summary>
public enum ShapeTestCommandType
{
    CreateSingleShape,      // 创建单个形状测试
    CreateCompositeShape,   // 创建复合形状测试
    CreateAllShapes,        // 创建所有形状类型展示
    CreateScenario,         // 创建特定场景（机器人、建筑等）
    ClearAllUnits,          // 清除所有测试单位
    ChangeColorTheme,       // 切换颜色主题
    TestAnimations,         // 测试动画效果
    TestPerformance         // 性能压力测试
}

/// <summary>
/// 形状测试场景类型
/// </summary>
public enum ShapeTestScenario
{
    RobotArmy,      // 机器人军队
    TowerDefense,   // 塔防建筑
    SpaceFleet,     // 太空舰队
    AnimalFarm,     // 动物农场
    GeometryMuseum  // 几何博物馆
}

/// <summary>
/// 形状测试状态信息（只包含值类型，避免序列化问题）
/// </summary>
public struct ShapeTestStatusInfo
{
    public int TotalUnitsCreated { get; init; }
    public int CurrentActiveUnits { get; init; }
    public ShapeColorTheme CurrentTheme { get; init; }
    public double LastOperationTime { get; init; }
    public bool IsPerformanceTestRunning { get; init; }
}

/// <summary>
/// 客户端向服务端发送形状测试命令的协议
/// </summary>
public readonly struct ProtoShapeTestCommand : IProtocolClientTransient<ProtoShapeTestCommand>
{
    public required ShapeTestCommandType CommandType { get; init; }
    public PrimitiveShape TargetShape { get; init; }
    public ShapeTestScenario TargetScenario { get; init; }
    public ShapeColorTheme ColorTheme { get; init; }
    public Vector3 Position { get; init; }
    public Vector3 Scale { get; init; }

#if SERVER
    public readonly void Handle(Player player)
    {
        try
        {
            Game.Logger.LogInformation("🎯 Received shape test command: {CommandType} (Player: {PlayerId})", 
                CommandType, player.Id);

            switch (CommandType)
            {
                case ShapeTestCommandType.CreateSingleShape:
                    _ = PrimitiveShapeTestServer.CreateSingleShapeTest(TargetShape, Position, Scale, ColorTheme);
                    break;
                    
                case ShapeTestCommandType.CreateCompositeShape:
                    _ = PrimitiveShapeTestServer.CreateCompositeShapeTest(Position);
                    break;
                    
                case ShapeTestCommandType.CreateAllShapes:
                    _ = PrimitiveShapeTestServer.CreateAllShapesDemo(ColorTheme);
                    break;
                    
                case ShapeTestCommandType.CreateScenario:
                    _ = PrimitiveShapeTestServer.CreateTestScenario(TargetScenario);
                    break;
                    
                case ShapeTestCommandType.ClearAllUnits:
                    _ = PrimitiveShapeTestServer.ClearAllTestUnits();
                    break;
                    
                case ShapeTestCommandType.ChangeColorTheme:
                    _ = PrimitiveShapeTestServer.ChangeColorTheme(ColorTheme);
                    break;
                    
                case ShapeTestCommandType.TestAnimations:
                    _ = PrimitiveShapeTestServer.StartAnimationTest();
                    break;
                    
                case ShapeTestCommandType.TestPerformance:
                    _ = PrimitiveShapeTestServer.StartPerformanceTest();
                    break;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling shape test command {CommandType}", CommandType);
        }
    }
#endif
}

/// <summary>
/// 服务端向客户端发送形状测试状态更新的协议
/// </summary>
public readonly struct ProtoShapeTestStatus : IProtocolServerTransient<ProtoShapeTestStatus>
{
    public required ShapeTestStatusInfo StatusInfo { get; init; }

#if CLIENT
    public readonly void Handle()
    {
        PrimitiveShapeTestClient.UpdateStatus(StatusInfo);
    }
#endif
}

/// <summary>
/// 测试类型枚举
/// </summary>
public enum TestType
{
    SingleShape,
    CompositeShape,  
    AllShapes,
    Scenario,
    Animation,
    Performance
}

/// <summary>
/// 服务端向客户端发送测试完成通知的协议
/// </summary>
public readonly struct ProtoShapeTestComplete : IProtocolServerTransient<ProtoShapeTestComplete>
{
    public required TestType TestType { get; init; }
    public required bool Success { get; init; }
    public int UnitsCreated { get; init; }
    public double ElapsedTime { get; init; }

#if CLIENT
    public readonly void Handle()
    {
        PrimitiveShapeTestClient.OnTestComplete(TestType, Success, UnitsCreated, ElapsedTime);
    }
#endif
}

#endif