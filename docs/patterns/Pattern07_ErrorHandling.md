# Pattern 7: Error Handling and Debugging

## Overview
Standard error handling patterns for WasiCore projects. AI should include appropriate error handling in all critical operations.

## Complete Code Example

```csharp
/// <summary>
/// Standard error handling patterns for a WasiCore project.
/// AI should include appropriate error handling in all critical operations.
/// </summary>
public class ErrorHandlingPatterns
{
    /// <summary>
    /// Safe pattern for accessing game data.
    /// </summary>
    public Unit CreateUnitSafely(GameLink<GameDataUnit> unitLink, Player player, ScenePoint position)
    {
        try
        {
            // Validate parameters
            if (unitLink == null)
            {
                Game.Logger.LogError("❌ Unit link is null");
                return null;
            }
            
            if (player == null)
            {
                Game.Logger.LogError("❌ Player is null for unit creation");
                return null;
            }
            
            // Get game data
            var unitData = unitLink.Data;
            if (unitData == null)
            {
                Game.Logger.LogError("❌ Unit data not found for link: {LinkId}", unitLink);
                return null;
            }
            
            // Create the unit
            var unit = unitData.CreateUnit(player, position);
            if (unit == null)
            {
                Game.Logger.LogError("❌ Failed to create unit from data: {DataName}", unitData.Name);
                return null;
            }
            
            Game.Logger.LogInformation("✅ Unit created successfully: {UnitName}", unit.Name);
            return unit;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "💥 Exception in CreateUnitSafely for link: {LinkId}", unitLink);
            return null;
        }
    }
    
    /// <summary>
    /// Safe pattern for UI operations.
    /// </summary>
    public bool CreateUISafely()
    {
        try
        {
            var mainPanel = UI.VStack(20,
                UI.Label("Game Title").FontSize(24),
                UI.Button("Start Game").OnClick((s, e) => StartGameSafely())
            );
            
            if (Game.UIRoot == null)
            {
                Game.Logger.LogError("❌ UI Root is null, cannot add panel");
                return false;
            }
            
            Game.UIRoot.AddChild(mainPanel);
            Game.Logger.LogInformation("✅ UI created successfully");
            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "💥 Exception in CreateUISafely");
            return false;
        }
    }
    
    private void StartGameSafely()
    {
        try
        {
            Game.Logger.LogInformation("🎮 Starting game...");
            // Game start logic
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "💥 Exception starting game");
        }
    }
    
    /// <summary>
    /// Pattern for outputting debug information.
    /// </summary>
    public void DebugGameState()
    {
        try
        {
            Game.Logger.LogInformation("🔍 Debug: Game State Check");
            Game.Logger.LogInformation("🔍 Current Scene: {SceneName}", Game.CurrentScene?.Name ?? "None");
            Game.Logger.LogInformation("🔍 Active Players: {PlayerCount}", Player.GetAllPlayers().Count());
            Game.Logger.LogInformation("🔍 UI Root Children: {ChildCount}", Game.UIRoot?.Children?.Count ?? 0);
            
            // More detailed debug info
            foreach (var player in Player.GetAllPlayers())
            {
                Game.Logger.LogInformation("🔍 Player: {PlayerName}, Units: {UnitCount}", 
                    player.Name, player.Units.Count);
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "💥 Exception in DebugGameState");
        }
    }
}
```

## Error Handling Principles

### 1. Validate Inputs
```csharp
if (parameter == null)
{
    Game.Logger.LogError("❌ Parameter is null");
    return defaultValue;
}
```

### 2. Check Data Availability
```csharp
var data = link.Data;
if (data == null)
{
    Game.Logger.LogError("❌ Data not found for link: {LinkId}", link);
    return null;
}
```

### 3. Wrap in Try-Catch
```csharp
try
{
    // Risky operation
}
catch (Exception ex)
{
    Game.Logger.LogError(ex, "💥 Exception in {Method}", nameof(Method));
    // Handle or return safe value
}
```

### 4. Return Safe Values
```csharp
public Unit CreateUnit(...)
{
    try
    {
        // Creation logic
        return unit;
    }
    catch
    {
        return null; // Safe default
    }
}
```

## Logging Best Practices

### Use Appropriate Log Levels
```csharp
// Information - Normal flow
Game.Logger.LogInformation("✅ Operation successful: {Detail}", detail);

// Warning - Potential issues
Game.Logger.LogWarning("⚠️ Unexpected condition: {Condition}", condition);

// Error - Failures
Game.Logger.LogError("❌ Operation failed: {Reason}", reason);

// Error with exception
Game.Logger.LogError(ex, "💥 Exception in {Operation}", operation);
```

### Use Structured Logging
```csharp
// Good - Parameterized
Game.Logger.LogInformation("Player {PlayerId} scored {Points} points", id, points);

// Bad - String concatenation
Game.Logger.LogInformation("Player " + id + " scored " + points + " points");
```

### Use Emojis for Clarity
```csharp
"✅" // Success
"❌" // Error
"⚠️" // Warning
"💥" // Exception
"🔍" // Debug
"🎮" // Game events
"👤" // Player events
"💀" // Death/destruction
```

## Common Error Scenarios

### API Not Found / Compilation Errors
```csharp
// ❌ WRONG - Guessing API names
unit.ApplyDamage(100);  // Method might not exist

// ✅ CORRECT - Check API documentation first
// Use grep to find correct API:
// grep -i "damage" api-client-reference/*.xml
// Found: TakeDamage(float damage, Unit source)
unit.TakeDamage(100, attacker);

// When unsure about API:
// 1. Search XML docs: grep -i "keyword" api-*-reference/*.xml
// 2. Check return types and parameters
// 3. Verify if it's client or server API
```

### Null Reference Prevention
```csharp
// Chain null checks
var unitName = player?.MainUnit?.Name ?? "Unknown";

// Guard clauses
if (player?.MainUnit == null)
{
    Game.Logger.LogError("❌ Player has no main unit");
    return;
}
```

### Collection Safety
```csharp
// Check before accessing
if (units.Count > index)
{
    var unit = units[index];
}

// Use LINQ safely
var firstUnit = units.FirstOrDefault();
if (firstUnit != null)
{
    // Use firstUnit
}
```

### Async Error Handling
```csharp
public async Task<bool> AsyncOperation()
{
    try
    {
        await Game.Delay(TimeSpan.FromSeconds(1));
        return true;
    }
    catch (Exception ex)
    {
        Game.Logger.LogError(ex, "💥 Async operation failed");
        return false;
    }
}
```

## Debug Helpers

### State Inspection
```csharp
public void DumpEntityInfo(Entity entity)
{
    Game.Logger.LogInformation("=== Entity Debug Info ===");
    Game.Logger.LogInformation("ID: {Id}, Name: {Name}", entity.Id, entity.Name);
    Game.Logger.LogInformation("Position: {Position}", entity.Position);
    Game.Logger.LogInformation("Components: {Components}", 
        string.Join(", ", entity.Components.Select(c => c.GetType().Name)));
}
```

### Performance Monitoring
```csharp
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
// Operation to measure
stopwatch.Stop();
Game.Logger.LogInformation("⏱️ Operation took {Ms}ms", stopwatch.ElapsedMilliseconds);
```

## Best Practices Summary

1. **Always validate inputs** - Check for null, ranges, etc.
2. **Use try-catch wisely** - Around external calls and risky operations
3. **Log appropriately** - Use correct levels and structured logging
4. **Return safe defaults** - null, false, empty collections
5. **Fail gracefully** - Don't crash the game
6. **Be descriptive** - Clear error messages help debugging
