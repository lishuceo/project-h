# Pattern 8: Physics System

## Overview
Complete pattern for using the WasiCore physics system. **CRITICAL**: Physics code is CLIENT-ONLY and must be wrapped in `#if CLIENT`.

## Complete Code Example

```csharp
/// <summary>
/// Complete pattern for using the WasiCore physics system.
/// AI should use this pattern for physics-based gameplay.
/// 
/// IMPORTANT API NOTES:
/// - PhysicsActor has ApplyForce() method, but RigidBody component also has physics methods
/// - Use RigidBody component methods for detailed physics control (as shown in PhysicsSystem.md)
/// - ScriptComponent methods (OnStart, OnUpdate, OnTriggerEnter) are from engine layer
/// - Always pass PhysicsActor (not Node) to DestroyImmediately()
/// </summary>
#if CLIENT
using System;
using System.Numerics;
using GameCorePhysics.Actor;
using EngineInterface.Urho3DInterface;
using static GameCore.SceneSystem;

public class PhysicsGameSystem : IGameClass
{
    private static GameCore.SceneSystem.Scene physicsScene;
    private static List<PhysicsActor> physicsActors = new();
    
    public static void OnRegisterGameClass()
    {
        Game.OnGameDataInitialization += OnGameDataInitialization;
        Game.OnGameInitialization += OnGameInitialization;
    }
    
    private static void OnGameDataInitialization()
    {
        // Define physics scene data
        new GameDataScene(PhysicsSceneLinks.PhysicsScene)
        {
            Name = "Physics Test Scene",
            SceneType = SceneType.Game
        };
        
        Game.Logger.LogInformation("⚛️ Physics system data initialized");
    }
    
    private static void OnGameInitialization()
    {
        // Get the physics scene
        physicsScene = GameCore.SceneSystem.Scene.Get(PhysicsSceneLinks.PhysicsScene.Data);
        
        // Create the game environment
        CreatePhysicsEnvironment();
        
        // Setup the main unit physics
        SetupMainUnitPhysics();
        
        Game.Logger.LogInformation("🎮 Physics game initialized");
    }
    
    /// <summary>
    /// Create the physics-based game environment.
    /// </summary>
    private static void CreatePhysicsEnvironment()
    {
        // Create ground platform
        var ground = new PhysicsActor(
            Player.LocalPlayer,
            PhysicsActor.GetPrimitiveLink(PrimitiveShape.Cube),
            physicsScene,
            new Vector3(0, -100, 0), // Below ground level
            Vector3.Zero
        );
        
        // Scale the ground to make it a platform
        var groundNode = ground.GetNode();
        groundNode.localScale = new Vector3(40, 1, 40);
        
        physicsActors.Add(ground);
        
        // Create floating platforms
        for (int i = 0; i < 3; i++)
        {
            var platform = new PhysicsActor(
                Player.LocalPlayer,
                PhysicsActor.GetPrimitiveLink(PrimitiveShape.Cube),
                physicsScene,
                new Vector3(1000 * (i + 1), 200 * (i + 1), 0),
                Vector3.Zero
            );
            
            var platformNode = platform.GetNode();
            platformNode.localScale = new Vector3(10, 1, 10);
            
            physicsActors.Add(platform);
        }
        
        // Create collectible items with physics
        CreateCollectibles();
        
        Game.Logger.LogInformation("🏗️ Physics environment created: {Count} objects", physicsActors.Count);
    }
}

/// <summary>
/// Player movement component using physics.
/// </summary>
public class PlayerMovementComponent : ScriptComponent
{
    private RigidBody rigidBody;
    private float moveSpeed = 500f;
    private float jumpForce = 300f;
    private bool isGrounded = false;
    
    public override void OnStart()
    {
        rigidBody = node.GetComponent<RigidBody>();
        Game.Logger.LogInformation("🏃 Player movement component started");
    }
    
    public override void OnFixedUpdate(float timeStep)
    {
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        // Get current velocity (from RigidBody component)
        var velocity = rigidBody.GetLinearVelocity();
        
        // Basic WASD movement (example - would need actual input handling)
        var moveDirection = Vector3.Zero;
        // TODO: Implement actual input detection here
        // Example: if (Input.IsKeyDown(Key.W)) moveDirection.X += 1;
        
        // Apply movement force
        if (moveDirection != Vector3.Zero)
        {
            moveDirection = Vector3.Normalize(moveDirection);
            rigidBody.ApplyForce(moveDirection * moveSpeed);
        }
        
        // Jump logic (example - would need actual input)
        if (isGrounded && /* Input.IsKeyPressed(Key.Space) */ false)
        {
            // Apply jump force when grounded
            rigidBody.ApplyForce(new Vector3(0, jumpForce, 0));
        }
    }
    
    public override void OnTriggerEnter(Node otherNode)
    {
        // Check if we hit the ground
        isGrounded = true;
        Game.Logger.LogInformation("👟 Player grounded");
    }
    
    public override void OnTriggerExit(Node otherNode)
    {
        // Left the ground
        isGrounded = false;
        Game.Logger.LogInformation("🚀 Player airborne");
    }
}
#endif
```

## Key Physics Concepts

### PhysicsActor Creation
```csharp
// Create physics object with shape
var physicsActor = new PhysicsActor(
    player,                                      // Owner player
    PhysicsActor.GetPrimitiveLink(shape),       // Shape link
    scene,                                       // Physics scene
    position,                                    // Initial position
    rotation                                     // Initial rotation
);

// Create physics for existing unit
var unitPhysics = new PhysicsActor(unit);
```

### Node and Components
```csharp
// Get the node (required for components)
var node = physicsActor.GetNode();

// Add/Get components
var rigidBody = node.GetComponent<RigidBody>();
var collider = node.GetComponent<Collider>();
var customScript = node.CreateComponent<MyComponent>();
```

### RigidBody Physics Control
```csharp
// Forces and velocity
rigidBody.ApplyForce(new Vector3(0, 100, 0));
rigidBody.SetLinearVelocity(new Vector3(0, 0, 10));
rigidBody.SetAngularVelocity(new Vector3(0, 45, 0));

// Physics properties
rigidBody.SetMass(10.0f);
rigidBody.SetFriction(0.8f);
rigidBody.SetRestitution(0.5f);  // Bounciness
rigidBody.SetUseGravity(true);
rigidBody.SetKinematic(false);

// Constraints
rigidBody.SetLinearFactor(new Vector3(1, 1, 0));  // Lock Z movement
rigidBody.SetAngularFactor(new Vector3(0, 1, 0)); // Only Y rotation
```

### Collision Layers
```csharp
// Set collision layer (what layer this object is on)
rigidBody.SetCollisionLayer(1u);

// Set collision mask (what layers this object collides with)
rigidBody.SetCollisionMask(0xFFFFFFFF); // Collide with everything

// Common layer setup
const uint LAYER_GROUND = 1u;
const uint LAYER_PLAYER = 2u;
const uint LAYER_ENEMY = 4u;
const uint LAYER_PICKUP = 8u;
```

### ScriptComponent Pattern
```csharp
public class MyPhysicsComponent : ScriptComponent
{
    // Called once when component starts
    public override void OnStart()
    {
        // Initialize component
    }
    
    // Called every frame
    public override void OnUpdate(float timeStep)
    {
        // Visual updates
    }
    
    // Called at fixed physics rate
    public override void OnFixedUpdate(float timeStep)
    {
        // Physics updates
    }
    
    // Collision events
    public override void OnTriggerEnter(Node otherNode)
    {
        // Entered trigger zone
    }
    
    public override void OnTriggerExit(Node otherNode)
    {
        // Left trigger zone
    }
    
    public override void OnCollision(Node otherNode, Vector3 contact)
    {
        // Physical collision occurred
    }
}
```

## Important Rules

### 1. Always Use #if CLIENT
```csharp
#if CLIENT
// All physics code here
#endif
```

### 2. Store PhysicsActor References
```csharp
// CORRECT - Store PhysicsActor for destruction
private PhysicsActor physicsActor;

// Pass to DestroyImmediately
PhysicsActor.DestroyImmediately(physicsActor);
```

### 3. Physics Update Order
```csharp
// Visual/logic updates
public override void OnUpdate(float timeStep) { }

// Physics updates ONLY
public override void OnFixedUpdate(float timeStep) { }
```

### 4. Null Check Components
```csharp
var rigidBody = node.GetComponent<RigidBody>();
if (rigidBody != null)
{
    // Use rigidBody
}
```

## Common Physics Patterns

### Projectile
```csharp
public void FireProjectile(Vector3 origin, Vector3 direction)
{
    var projectile = new PhysicsActor(
        Player.LocalPlayer,
        PhysicsActor.GetPrimitiveLink(PrimitiveShape.Sphere),
        physicsScene,
        origin,
        Vector3.Zero
    );
    
    var node = projectile.GetNode();
    node.localScale = new Vector3(0.2f, 0.2f, 0.2f);
    
    var rb = node.GetComponent<RigidBody>();
    rb.SetLinearVelocity(direction * 50);
    rb.SetUseGravity(false);
}
```

### Trigger Zone
```csharp
public void CreateTriggerZone(Vector3 position, float radius)
{
    var trigger = new PhysicsActor(
        Player.LocalPlayer,
        PhysicsActor.GetPrimitiveLink(PrimitiveShape.Sphere),
        physicsScene,
        position,
        Vector3.Zero
    );
    
    var node = trigger.GetNode();
    node.localScale = Vector3.One * radius;
    
    var rb = node.GetComponent<RigidBody>();
    rb.SetTrigger(true);  // Make it a trigger, not solid
}
```

## Best Practices

1. **Wrap in #if CLIENT** - Physics is client-only
2. **Use OnFixedUpdate** - For physics operations
3. **Store actor references** - For proper cleanup
4. **Check components** - They might be null
5. **Use collision layers** - For selective collisions
6. **Log physics events** - For debugging
