# Pattern 1: Game System Initialization

## Overview
Standard pattern for registering a WasiCore game system. Use this pattern for every game system.

## Complete Code Example

```csharp
/// <summary>
/// Standard pattern for registering a WasiCore game system.
/// AI should use this pattern for every game system.
/// </summary>
public class MyGameSystem : IGameClass
{
    // System registration - called automatically by the framework.
    public static void OnRegisterGameClass()
    {
        // Register data initialization callback.
        Game.OnGameDataInitialization += OnGameDataInitialization;
        
        // Register trigger initialization callback.
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        
        // Standard logging.
        Game.Logger.LogInformation("🎮 {SystemName} system registered", nameof(MyGameSystem));
    }
    
    // Data initialization - create GameData and GameLinks.
    private static void OnGameDataInitialization()
    {
        // Create game data entries.
        new GameDataUnit(UnitLinks.MyHero)
        {
            Name = "My Hero",
            Properties = new()
            {
                { UnitProperty.LifeMax, 1000 },
                { UnitProperty.MoveSpeed, 300 }
            }
        };
        
        Game.Logger.LogInformation("📊 {SystemName} data initialized", nameof(MyGameSystem));
    }
    
    // Trigger initialization - register event handlers.
    private static void OnGameTriggerInitialization()
    {
        // Create an event trigger.
        var trigger = new Trigger<EventUnitDeath>(OnUnitDeath);
        trigger.Register(Game.Instance);
        
        Game.Logger.LogInformation("⚡ {SystemName} triggers initialized", nameof(MyGameSystem));
    }
    
    // Event handler method.
    private static async Task<bool> OnUnitDeath(object sender, EventUnitDeath e)
    {
        Game.Logger.LogInformation("💀 Unit died: {UnitName}", e.Unit.Name);
        // Handle unit death logic here.
        return true;
    }
}

/// <summary>
/// GameLink definitions - typically in a separate static class.
/// </summary>
public static class UnitLinks
{
    public static readonly GameLink<GameDataUnit> MyHero = new("MyHero"u8);
}
```

## Key Points

1. **IGameClass Interface**: All game systems must implement this
2. **OnRegisterGameClass**: Static method, called automatically by the framework
3. **Initialization Callbacks**: Register for data and trigger initialization
4. **GameLink Definitions**: Define links in a separate static class
5. **Logging**: Use parameterized logging with emojis for clarity

## Common Variations

### With Game Mode
```csharp
// Define game mode first
new GameDataGameMode(ScopeData.GameMode.MyGame)
{
    Name = "My Game Mode"
};
```

### Multiple Data Types
```csharp
private static void OnGameDataInitialization()
{
    // Units
    new GameDataUnit(UnitLinks.Hero) { Name = "Hero" };
    
    // Buildings
    new GameDataBuilding(BuildingLinks.Tower) { Name = "Tower" };
    
    // Items
    new GameDataItem(ItemLinks.Sword) { Name = "Sword" };
}
```
