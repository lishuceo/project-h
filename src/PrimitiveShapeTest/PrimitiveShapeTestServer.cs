#if SERVER
using Events;
using GameCore.BaseInterface;
using GameCore.Drawing;
using GameCore.EntitySystem;
using GameCore.EntitySystem.Data;
using GameCore.Event;
using GameCore.PlayerAndUsers;
using GameCore.SceneSystem;
using GameCore.Shape.Data;
using GameData;
using System.Numerics;
using Microsoft.Extensions.Logging;

namespace GameEntry.PrimitiveShapeTest;

/// <summary>
/// 基本形状测试游戏模式的服务端实现
/// 测试基本形状系统的所有功能：单个形状、复合形状、颜色主题、性能等
/// </summary>
internal class PrimitiveShapeTestServer : IGameClass
{
    #region Fields

    private static Trigger<EventGameStart>? gameStartTrigger;
    private static Scene? testScene;
    private static List<Unit> testUnits = new();
    private static Unit? playerUnit;
    private static readonly Vector3 basePosition = new(8000, 8000, 0); // 地图中心
    private static ShapeColorTheme currentTheme = ShapeColorTheme.Standard;
    private static int totalUnitsCreated = 0;
    private static bool isPerformanceTestRunning = false;
    private static DateTime lastOperationTime = DateTime.Now;

    // 测试位置网格
    private static readonly Vector3[] testPositions = 
    {
        new(7000, 7000, 0), new(8000, 7000, 0), new(9000, 7000, 0),
        new(7000, 8000, 0), new(8000, 8000, 0), new(9000, 8000, 0),
        new(7000, 9000, 0), new(8000, 9000, 0), new(9000, 9000, 0),
        new(6000, 6000, 0), new(10000, 6000, 0), new(6000, 10000, 0), new(10000, 10000, 0),
    };

    #endregion

    #region IGameClass Implementation

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.Logger.LogInformation("🎯 Primitive Shape Test Server registered");
    }

    #endregion

    #region Initialization

    private static void OnGameTriggerInitialization()
    {
        if (Game.GameModeLink != ScopeData.GameMode.PrimitiveShapeTest)
        {
            Game.Logger.LogInformation("🚫 Not PrimitiveShapeTest mode - skipping trigger registration");
            return;
        }
        gameStartTrigger = new Trigger<EventGameStart>(OnGameStartAsync, true);
        gameStartTrigger.Register(Game.Instance);
        Game.Logger.LogInformation("🎯 Primitive Shape Test triggers initialized");
    }

    private static Task<bool> OnGameStartAsync(object sender, EventGameStart eventArgs)
    {
        Game.Logger.LogInformation("🎯 Primitive Shape Test Server Started!");
        
        testScene = Scene.GetOrCreate(ScopeData.Scene.PrimitiveShapeTestScene);
        if (testScene == null)
        {
            Game.Logger.LogError("❌ Failed to create Primitive Shape Test Scene");
            return Task.FromResult(false);
        }

        Game.Logger.LogInformation("🎯 Primitive Shape Test Scene initialized: {scene}", testScene);
        
        // 获取玩家单位
        playerUnit = testScene.GetPlacedUnit(1)?.TriggerGetterInstance;
        if (playerUnit != null)
        {
            Game.Logger.LogInformation("🎮 Player unit found: {unit}", playerUnit);
        }
        
        SendStatusUpdate("Primitive Shape Test System Ready! 🎯");
        return Task.FromResult(true);
    }

    #endregion

    #region Public Test Methods

    /// <summary>
    /// 创建单个形状测试
    /// </summary>
    public static Task CreateSingleShapeTest(PrimitiveShape shape, Vector3 position, Vector3 scale, ShapeColorTheme theme)
    {
        try
        {
            lastOperationTime = DateTime.Now;
            Game.Logger.LogInformation("🎯 Creating single shape test: {shape} at {position}", shape, position);

            if (testScene == null)
            {
                Game.Logger.LogError("❌ Test scene not initialized");
                return Task.CompletedTask;
            }

            // 根据形状类型选择预注册的单位
            var unitLink = shape switch
            {
                PrimitiveShape.Sphere => ScopeData.Unit.ShapeTestSphere,
                PrimitiveShape.Cube => ScopeData.Unit.ShapeTestCube,
                PrimitiveShape.Cylinder => ScopeData.Unit.ShapeTestCylinder,
                PrimitiveShape.Plane => ScopeData.Unit.ShapeTestPlane,
                PrimitiveShape.Cone => ScopeData.Unit.ShapeTestCone,
                PrimitiveShape.Capsule => ScopeData.Unit.ShapeTestCapsule,
                PrimitiveShape.Pyramid => ScopeData.Unit.ShapeTestPyramid,
                _ => ScopeData.Unit.ShapeTestSphere // 默认使用球体
            };

            // 创建单位
            var testPlayer = Player.GetById(2) ?? throw new InvalidOperationException("Test player not found");
            var unit = unitLink.Data?.CreateUnit(
                testPlayer, 
                new ScenePoint(position, testScene!), 
                0);

            if (unit != null)
            {
                testUnits.Add(unit);
                totalUnitsCreated++;
                SendStatusUpdate($"Created {shape} shape at {position}");
                Game.Logger.LogInformation("✅ Successfully created {shape} unit at {position}", shape, position);
            }
            else
            {
                Game.Logger.LogError("❌ Failed to create unit for {shape}", shape);
                SendStatusUpdate($"Failed to create {shape}");
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to create single shape test");
            SendStatusUpdate($"Failed to create {shape}: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 创建复合形状测试
    /// </summary>
    public static Task CreateCompositeShapeTest(Vector3 position)
    {
        try
        {
            lastOperationTime = DateTime.Now;
            Game.Logger.LogInformation("🎯 Creating composite shape test at {position}", position);

            if (testScene == null)
            {
                Game.Logger.LogError("❌ Test scene not initialized");
                return Task.CompletedTask;
            }

            // 使用预注册的复合机器人单位
            var testPlayer = Player.GetById(2) ?? throw new InvalidOperationException("Test player not found");
            var unit = ScopeData.Unit.CompositeRobot.Data?.CreateUnit(
                testPlayer, 
                new ScenePoint(position, testScene!), 
                0);

            if (unit != null)
            {
                testUnits.Add(unit);
                totalUnitsCreated++;
                SendStatusUpdate($"Created robot composite shape at {position}");
                Game.Logger.LogInformation("✅ Successfully created composite robot unit at {position}", position);
            }
            else
            {
                Game.Logger.LogError("❌ Failed to create composite robot unit");
                SendStatusUpdate("Failed to create composite robot");
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to create composite shape test");
            SendStatusUpdate($"Failed to create composite shape: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 创建所有形状类型展示
    /// </summary>
    public static async Task CreateAllShapesDemo(ShapeColorTheme theme)
    {
        try
        {
            lastOperationTime = DateTime.Now;
            currentTheme = theme;
            Game.Logger.LogInformation("🎯 Creating all shapes demo with theme: {theme}", theme);

            var shapes = System.Enum.GetValues<PrimitiveShape>();
            var startPosition = new Vector3(6000, 6000, 0);
            
            for (int i = 0; i < shapes.Length && i < testPositions.Length; i++)
            {
                await CreateSingleShapeTest(shapes[i], testPositions[i], Vector3.One, theme);
                await Game.Delay(TimeSpan.FromMilliseconds(100)); // 延迟100ms避免创建过快
            }

            SendStatusUpdate($"Created all {shapes.Length} shape types with {theme} theme");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to create all shapes demo");
            SendStatusUpdate($"Failed to create shapes demo: {ex.Message}");
        }
    }

    /// <summary>
    /// 创建测试场景
    /// </summary>
    public static async Task CreateTestScenario(ShapeTestScenario scenario)
    {
        try
        {
            lastOperationTime = DateTime.Now;
            Game.Logger.LogInformation("🎯 Creating test scenario: {scenario}", scenario);

            switch (scenario)
            {
                case ShapeTestScenario.RobotArmy:
                    await CreateRobotArmy();
                    break;
                case ShapeTestScenario.TowerDefense:
                    await CreateTowerDefense();
                    break;
                case ShapeTestScenario.SpaceFleet:
                    await CreateSpaceFleet();
                    break;
                case ShapeTestScenario.AnimalFarm:
                    await CreateAnimalFarm();
                    break;
                case ShapeTestScenario.GeometryMuseum:
                    await CreateGeometryMuseum();
                    break;
            }

            var completeMessage = new ProtoShapeTestComplete
            {
                TestType = TestType.Scenario,
                Success = true,
                UnitsCreated = testUnits.Count,
                ElapsedTime = (DateTime.Now - lastOperationTime).TotalSeconds
            };
            Player.BroadcastClientMessage(ref completeMessage);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to create test scenario");
            SendStatusUpdate($"Failed to create {scenario}: {ex.Message}");
        }
    }

    /// <summary>
    /// 清除所有测试单位
    /// </summary>
    public static Task ClearAllTestUnits()
    {
        try
        {
            lastOperationTime = DateTime.Now;
            Game.Logger.LogInformation("🗑️ Clearing all test units...");

            var unitCount = testUnits.Count;
            foreach (var unit in testUnits.ToList())
            {
                try
                {
                    unit.Destroy();
                }
                catch (Exception ex)
                {
                    Game.Logger.LogWarning(ex, "⚠️ Failed to destroy unit: {unit}", unit);
                }
            }

            testUnits.Clear();
            
            SendStatusUpdate($"Cleared {unitCount} test units");
            Game.Logger.LogInformation("✅ Successfully cleared {count} test units", unitCount);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to clear test units");
            SendStatusUpdate($"Failed to clear units: {ex.Message}");
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 切换颜色主题
    /// </summary>
    public static async Task ChangeColorTheme(ShapeColorTheme theme)
    {
        try
        {
            lastOperationTime = DateTime.Now;
            currentTheme = theme;
            Game.Logger.LogInformation("🎨 Changing color theme to: {theme}", theme);

            // 重新创建所有现有单位以应用新主题
            if (testUnits.Count > 0)
            {
                var unitPositions = testUnits.Select(u => u.Position.Vector3).ToList();
                await ClearAllTestUnits();
                
                for (int i = 0; i < unitPositions.Count && i < testPositions.Length; i++)
                {
                    var shape = (PrimitiveShape)(i % System.Enum.GetValues<PrimitiveShape>().Length);
                    await CreateSingleShapeTest(shape, unitPositions[i], Vector3.One, theme);
                }
            }

            SendStatusUpdate($"Changed color theme to {theme}");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to change color theme");
            SendStatusUpdate($"Failed to change theme: {ex.Message}");
        }
    }

    /// <summary>
    /// 开始动画测试
    /// </summary>
    public static async Task StartAnimationTest()
    {
        try
        {
            lastOperationTime = DateTime.Now;
            Game.Logger.LogInformation("🎬 Starting animation test...");

            // 创建一些单位进行动画测试
            var centerPosition = new Vector3(8000, 8000, 0);
            var radius = 500f;
            
            for (int i = 0; i < 6; i++)
            {
                var angle = i * Math.PI * 2 / 6;
                var position = centerPosition + new Vector3(
                    (float)(Math.Cos(angle) * radius),
                    (float)(Math.Sin(angle) * radius),
                    0);
                
                var shape = (PrimitiveShape)(i % System.Enum.GetValues<PrimitiveShape>().Length);
                await CreateSingleShapeTest(shape, position, Vector3.One, currentTheme);
            }

            SendStatusUpdate("Animation test setup completed");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to start animation test");
            SendStatusUpdate($"Failed to start animation test: {ex.Message}");
        }
    }

    /// <summary>
    /// 开始性能测试
    /// </summary>
    public static async Task StartPerformanceTest()
    {
        try
        {
            isPerformanceTestRunning = true;
            lastOperationTime = DateTime.Now;
            Game.Logger.LogInformation("⚡ Starting performance test...");

            var startTime = DateTime.Now;
            var targetCount = 100; // 创建100个单位
            var shapes = System.Enum.GetValues<PrimitiveShape>();
            
            for (int i = 0; i < targetCount; i++)
            {
                var position = new Vector3(
                    6000 + (i % 10) * 200,
                    6000 + (i / 10) * 200,
                    0);
                
                var shape = shapes[i % shapes.Length];
                await CreateSingleShapeTest(shape, position, Vector3.One, currentTheme);
                
                // 每10个单位发送一次进度更新
                if (i % 10 == 0)
                {
                    SendStatusUpdate($"Performance test: {i}/{targetCount} units created");
                }
            }

            var elapsedTime = (DateTime.Now - startTime).TotalSeconds;
            isPerformanceTestRunning = false;

            var completeMessage = new ProtoShapeTestComplete
            {
                TestType = TestType.Performance,
                Success = true,
                UnitsCreated = targetCount,
                ElapsedTime = elapsedTime
            };
            Player.BroadcastClientMessage(ref completeMessage);
        }
        catch (Exception ex)
        {
            isPerformanceTestRunning = false;
            Game.Logger.LogError(ex, "❌ Failed to complete performance test");
            SendStatusUpdate($"Performance test failed: {ex.Message}");
        }
    }

    #endregion

    #region Private Scenario Methods

    private static async Task CreateRobotArmy()
    {
        Game.Logger.LogInformation("🤖 Creating robot army scenario...");
        
        var formation = new Vector3[]
        {
            new(7500, 7500, 0), new(8000, 7500, 0), new(8500, 7500, 0),
            new(7500, 8000, 0), new(8500, 8000, 0),
            new(7500, 8500, 0), new(8000, 8500, 0), new(8500, 8500, 0),
        };

        foreach (var position in formation)
        {
            await CreateCompositeShapeTest(position);
            await Game.Delay(TimeSpan.FromMilliseconds(200));
        }
    }

    private static async Task CreateTowerDefense()
    {
        Game.Logger.LogInformation("🏰 Creating tower defense scenario...");
        
        // 创建塔楼
        var towerPositions = new Vector3[]
        {
            new(7000, 7000, 0), new(9000, 7000, 0),
            new(7000, 9000, 0), new(9000, 9000, 0),
        };

        foreach (var position in towerPositions)
        {
            await CreateTowerUnit(position);
            await Game.Delay(TimeSpan.FromMilliseconds(150));
        }
    }

    private static async Task CreateSpaceFleet()
    {
        Game.Logger.LogInformation("🚀 Creating space fleet scenario...");
        
        var fleetPositions = new Vector3[]
        {
            new(7000, 8000, 100), new(7500, 7800, 80), new(8000, 8200, 120),
            new(8500, 7900, 90), new(9000, 8100, 110),
        };

        foreach (var position in fleetPositions)
        {
            await CreateSpaceshipUnit(position);
            await Game.Delay(TimeSpan.FromMilliseconds(200));
        }
    }

    private static async Task CreateAnimalFarm()
    {
        Game.Logger.LogInformation("🐄 Creating animal farm scenario...");
        
        var animalPositions = new Vector3[]
        {
            new(7200, 7200, 0), new(7400, 7400, 0), new(7600, 7200, 0),
            new(7800, 7600, 0), new(8000, 7400, 0), new(8200, 7800, 0),
        };

        var animalShapes = new PrimitiveShape[]
        {
            PrimitiveShape.Capsule, PrimitiveShape.Sphere, PrimitiveShape.Cylinder,
            PrimitiveShape.Capsule, PrimitiveShape.Sphere, PrimitiveShape.Cylinder,
        };

        for (int i = 0; i < animalPositions.Length; i++)
        {
            await CreateSingleShapeTest(animalShapes[i], animalPositions[i], 
                new Vector3(0.8f, 0.8f, 1.2f), ShapeColorTheme.Natural);
            await Game.Delay(TimeSpan.FromMilliseconds(150));
        }
    }

    private static async Task CreateGeometryMuseum()
    {
        Game.Logger.LogInformation("🏛️ Creating geometry museum scenario...");
        
        var shapes = System.Enum.GetValues<PrimitiveShape>();
        var radius = 800f;
        var centerPosition = new Vector3(8000, 8000, 0);
        
        for (int i = 0; i < shapes.Length; i++)
        {
            var angle = i * Math.PI * 2 / shapes.Length;
            var position = centerPosition + new Vector3(
                (float)(Math.Cos(angle) * radius),
                (float)(Math.Sin(angle) * radius),
                0);
            
            await CreateSingleShapeTest(shapes[i], position, new Vector3(1.5f, 1.5f, 1.5f), ShapeColorTheme.Educational);
            await Game.Delay(TimeSpan.FromMilliseconds(200));
        }
    }

    private static Task CreateTowerUnit(Vector3 position)
    {
        var testPlayer = Player.GetById(2) ?? throw new InvalidOperationException("Test player not found");
        
        // 使用专门的城堡塔楼配置
        var unit = ScopeData.Unit.CastleTower.Data?.CreateUnit(
            testPlayer, 
            new ScenePoint(position, testScene!), 
            0);

        if (unit != null)
        {
            testUnits.Add(unit);
            totalUnitsCreated++;
            Game.Logger.LogInformation("🏰 Created castle tower unit at {position}", position);
        }
        
        return Task.CompletedTask;
    }

    private static Task CreateSpaceshipUnit(Vector3 position)
    {
        var testPlayer = Player.GetById(2) ?? throw new InvalidOperationException("Test player not found");
        
        // 使用专门的太空船配置
        var unit = ScopeData.Unit.Spaceship.Data?.CreateUnit(
            testPlayer, 
            new ScenePoint(position, testScene!), 
            0);

        if (unit != null)
        {
            testUnits.Add(unit);
            totalUnitsCreated++;
            Game.Logger.LogInformation("🚀 Created spaceship unit at {position}", position);
        }
        
        return Task.CompletedTask;
    }

    #endregion

    #region Status Updates

    private static void SendStatusUpdate(string message)
    {
        try
        {
            var statusInfo = new ShapeTestStatusInfo
            {
                TotalUnitsCreated = totalUnitsCreated,
                CurrentActiveUnits = testUnits.Count,
                CurrentTheme = currentTheme,
                LastOperationTime = (DateTime.Now - lastOperationTime).TotalSeconds,
                IsPerformanceTestRunning = isPerformanceTestRunning
            };

            var statusUpdate = new ProtoShapeTestStatus
            {
                StatusInfo = statusInfo
            };
            Player.BroadcastClientMessage(ref statusUpdate);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to send status update");
        }
    }

    #endregion
}

#endif
