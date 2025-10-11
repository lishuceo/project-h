# Pattern 6: Game Scene Creation

## Overview
Quickly create game scenes using the primitive shape system. This is an AI-friendly way to prototype game environments.

## Complete Code Example

```csharp
/// <summary>
/// Quickly create a game scene using the primitive shape system (AI-friendly).
/// </summary>
public class SceneCreationSystem : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.OnGameInitialization += CreateGameScene;
    }
    
    private static void CreateGameScene()
    {
        Game.Logger.LogInformation("🏗️ Creating game scene...");
        
        // Create the ground platform
        CreateGround();
        
        // Create player and enemies
        CreateCharacters();
        
        // Create environmental decorations
        CreateEnvironment();
        
        // Create collectibles
        CreateCollectibles();
        
        Game.Logger.LogInformation("🎨 Game scene created successfully");
    }
    
    private static void CreateGround()
    {
        // Use the AI shape factory to create the ground
        var ground = AIShapeFactory.CreatePlatform(
            new ScenePoint(0, -50, Game.CurrentScene, 0),
            new Vector3(20.0f, 1.0f, 20.0f) // width x height x depth
        );
        
        Game.Logger.LogInformation("🏔️ Ground platform created");
    }
    
    private static void CreateCharacters()
    {
        // Create the player character (blue capsule)
        var player = AIShapeFactory.CreatePlayer(
            new ScenePoint(0, 2, 0, Game.CurrentScene),
            scope: null,
            scale: new Vector3(1.0f, 1.5f, 1.0f)
        );
        
        // Create an array of enemies (red spheres)
        var enemyPositions = new List<ScenePoint>
        {
            new(5, 2, 5, Game.CurrentScene),
            new(-5, 2, 5, Game.CurrentScene),
            new(5, 2, -5, Game.CurrentScene),
            new(-5, 2, -5, Game.CurrentScene)
        };
        
        var enemies = AIShapeFactory.CreateShapes(
            PrimitiveShape.Sphere,
            enemyPositions,
            scope: null
        );
        
        Game.Logger.LogInformation("👥 Characters created: 1 player, {EnemyCount} enemies", enemies.Count);
    }
    
    private static void CreateEnvironment()
    {
        // Create a decorative tower
        var tower = AIShapeComposer.CreateCastleTower(
            new ScenePoint(10, 0, 10, Game.CurrentScene),
            scope: null,
            scale: 2.0f
        );
        
        // Create a grid of obstacles
        var obstacles = AIShapeFactory.CreateShapeGrid(
            PrimitiveShape.Cube,
            new ScenePoint(0, 1, -10, Game.CurrentScene),
            gridSize: (3, 3),
            spacing: 3.0f,
            scope: null
        );
        
        Game.Logger.LogInformation("🏰 Environment created: 1 tower, {ObstacleCount} obstacles", obstacles.Count);
    }
    
    private static void CreateCollectibles()
    {
        // Create collectibles (yellow cones)
        var collectibles = AIShapeFactory.CreateShapeGrid(
            PrimitiveShape.Cone,
            new ScenePoint(0, 1, 0, Game.CurrentScene),
            gridSize: (5, 1),
            spacing: 4.0f,
            scope: null
        );
        
        // Set collectible properties
        foreach (var collectible in collectibles)
        {
            // Add a rotation animation
            collectible.RotationSpeed = new Vector3(0, 45, 0); // 45 degrees per second
        }
        
        Game.Logger.LogInformation("💎 Collectibles created: {Count}", collectibles.Count);
    }
}
```

## AIShapeFactory Methods

### Basic Shape Creation
```csharp
// Single shape at position
var shape = AIShapeFactory.CreateShape(
    PrimitiveShape.Cube,    // Shape type
    position,               // ScenePoint
    scope                   // Optional scope
);

// Multiple shapes at positions
var shapes = AIShapeFactory.CreateShapes(
    PrimitiveShape.Sphere,
    positionList,
    scope
);
```

### Specialized Creators
```csharp
// Player character (blue capsule)
var player = AIShapeFactory.CreatePlayer(position, scope, scale);

// Enemy (red sphere)
var enemy = AIShapeFactory.CreateEnemy(position, scope, scale);

// Platform (scaled cube)
var platform = AIShapeFactory.CreatePlatform(position, size);

// Collectible (yellow cone)
var collectible = AIShapeFactory.CreateCollectible(position, scope);
```

### Grid Creation
```csharp
// Create grid of shapes
var grid = AIShapeFactory.CreateShapeGrid(
    PrimitiveShape.Cube,
    centerPosition,
    gridSize: (3, 3),    // rows, columns
    spacing: 2.0f,       // distance between shapes
    scope
);
```

## AIShapeComposer Patterns

### Complex Structures
```csharp
// Castle tower
var tower = AIShapeComposer.CreateCastleTower(position, scope, scale);

// Bridge
var bridge = AIShapeComposer.CreateBridge(startPos, endPos, width);

// Stairs
var stairs = AIShapeComposer.CreateStairs(
    bottomPosition,
    stepCount: 10,
    stepHeight: 0.5f,
    stepDepth: 1.0f
);
```

## Primitive Shape Types

```csharp
public enum PrimitiveShape
{
    Cube,       // Box shape
    Sphere,     // Ball shape
    Cylinder,   // Cylinder shape
    Cone,       // Cone shape
    Capsule,    // Capsule shape (good for characters)
    Plane       // Flat surface
}
```

## Shape Properties

```csharp
// Common properties you can set
shape.Color = Colors.Blue;
shape.RotationSpeed = new Vector3(0, 45, 0);  // degrees per second
shape.Scale = new Vector3(2, 1, 2);
shape.Position = new ScenePoint(x, y, z, scene);
```

## Best Practices

1. **Use ScenePoint**: Always specify scene in positions
2. **Group creation logic**: Separate ground, characters, environment
3. **Use shape factories**: Leverage AIShapeFactory helpers
4. **Add visual variety**: Use different colors and scales
5. **Consider physics**: Shapes can have physics components
6. **Log creation**: Track what's created for debugging

## Common Scene Patterns

### Arena Layout
```csharp
// Central platform
var arena = AIShapeFactory.CreatePlatform(
    new ScenePoint(0, 0, 0, scene),
    new Vector3(30, 1, 30)
);

// Surrounding walls
for (int i = 0; i < 4; i++)
{
    var angle = i * 90 * Mathf.Deg2Rad;
    var wallPos = new ScenePoint(
        Mathf.Cos(angle) * 15, 5, Mathf.Sin(angle) * 15, scene
    );
    AIShapeFactory.CreateShape(PrimitiveShape.Cube, wallPos)
        .Scale = new Vector3(1, 10, 30);
}
```
