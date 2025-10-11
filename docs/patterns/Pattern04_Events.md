# Pattern 4: Event-Driven Game Logic

## Overview
Standard pattern for using the WasiCore event system to handle game events and state changes.

## Complete Code Example

```csharp
/// <summary>
/// Standard pattern for using the WasiCore event system.
/// AI should use this pattern to handle game events and state changes.
/// </summary>
public class GameLogicSystem : IGameClass
{
    private static List<ITrigger> triggers = new();
    
    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += RegisterTriggers;
    }
    
    private static void RegisterTriggers()
    {
        // Player connected event
        var playerConnectTrigger = new Trigger<EventPlayerUserConnected>(OnPlayerConnected);
        playerConnectTrigger.Register(Game.Instance);
        triggers.Add(playerConnectTrigger);
        
        // Unit death event
        var unitDeathTrigger = new Trigger<EventUnitDeath>(OnUnitDeath);
        unitDeathTrigger.Register(Game.Instance);
        triggers.Add(unitDeathTrigger);
        
        // Item pickup event
        var itemPickupTrigger = new Trigger<EventItemPickup>(OnItemPickup);
        itemPickupTrigger.Register(Game.Instance);
        triggers.Add(itemPickupTrigger);
        
        Game.Logger.LogInformation("⚡ Game logic triggers registered: {Count}", triggers.Count);
    }
    
    // Handle player connection
    private static async Task<bool> OnPlayerConnected(object sender, EventPlayerUserConnected e)
    {
        var player = e.Player;
        Game.Logger.LogInformation("👤 Player connected: {PlayerName}", player.Name);
        
        // Create an initial hero for the player
        await CreatePlayerHero(player);
        
        // Send a welcome message
        await SendWelcomeMessage(player);
        
        return true;
    }
    
    // Handle unit death
    private static async Task<bool> OnUnitDeath(object sender, EventUnitDeath e)
    {
        var unit = e.Unit;
        Game.Logger.LogInformation("💀 Unit died: {UnitName}", unit.Name);
        
        // Drop loot
        await DropLoot(unit);
        
        // Grant experience
        await GiveExperience(unit);
        
        // Play death effect
        await PlayDeathEffect(unit);
        
        return true;
    }
    
    // Handle item pickup
    private static async Task<bool> OnItemPickup(object sender, EventItemPickup e)
    {
        var item = e.Item;
        var player = e.Player;
        
        Game.Logger.LogInformation("📦 Item picked up: {ItemName} by {PlayerName}", 
            item.Name, player.Name);
        
        // Add to player's inventory
        player.Inventory.AddItem(item);
        
        // Play pickup sound
        await PlayPickupSound(item.Position);
        
        return true;
    }
}
```

## Key Concepts

### Trigger Registration Pattern
```csharp
// 1. Create trigger with handler
var trigger = new Trigger<EventType>(HandlerMethod);

// 2. Register with game instance
trigger.Register(Game.Instance);

// 3. Store reference (optional but recommended)
triggers.Add(trigger);
```

### Event Handler Signature
```csharp
private static async Task<bool> HandlerMethod(object sender, EventType e)
{
    // Process event
    // Return true if handled successfully
    return true;
}
```

## Common Game Events

| Event | Description | Key Properties |
|-------|-------------|----------------|
| `EventPlayerUserConnected` | Player joins game | `e.Player` |
| `EventPlayerUserDisconnected` | Player leaves game | `e.Player` |
| `EventUnitDeath` | Unit dies | `e.Unit`, `e.Killer` |
| `EventUnitDamaged` | Unit takes damage | `e.Unit`, `e.Damage`, `e.Source` |
| `EventAbilityCast` | Ability is cast | `e.Ability`, `e.Caster`, `e.Target` |
| `EventItemPickup` | Item picked up | `e.Item`, `e.Player` |
| `EventGameStart` | Game begins | Game state info |
| `EventGameEnd` | Game ends | Winner info |

## UI Event Handling

```csharp
// Button click
button.OnClick += (sender, e) => 
{
    Game.Logger.LogInformation("Button clicked");
};

// Text input
inputField.OnTextChanged += (sender, e) =>
{
    Game.Logger.LogInformation("Text changed: {Text}", e.NewText);
};

// Slider value
slider.OnValueChanged += (sender, e) =>
{
    Game.Logger.LogInformation("Value: {Value}", e.NewValue);
};
```

## Custom Events

```csharp
// Define custom event
public class EventLevelComplete : IEvent
{
    public int Level { get; set; }
    public float CompletionTime { get; set; }
    public int Score { get; set; }
}

// Publish custom event
Game.Instance.PublishEvent(new EventLevelComplete
{
    Level = 1,
    CompletionTime = 120.5f,
    Score = 5000
});

// Subscribe to custom event
var trigger = new Trigger<EventLevelComplete>(OnLevelComplete);
trigger.Register(Game.Instance);
```

## Best Practices

1. **Store trigger references**: Keep a list of triggers for cleanup
2. **Use async handlers**: Return `Task<bool>` for async operations
3. **Handle exceptions**: Wrap handler logic in try-catch
4. **Log events**: Use parameterized logging for debugging
5. **Return true**: Indicate successful handling
6. **Check null values**: Validate event data before use
