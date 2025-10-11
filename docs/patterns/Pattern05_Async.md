# Pattern 5: Asynchronous Programming (WebAssembly-Friendly)

## Overview
The correct pattern for asynchronous programming in WasiCore. **CRITICAL**: AI must use `Game.Delay` instead of `Task.Delay` for WebAssembly compatibility.

## Complete Code Example

```csharp
/// <summary>
/// The correct pattern for asynchronous programming in WasiCore.
/// AI must use `Game.Delay` instead of `Task.Delay`.
/// </summary>
public class AsyncGameLogic
{
    /// <summary>
    /// Correct async delay pattern.
    /// </summary>
    public async Task DelayedAction()
    {
        Game.Logger.LogInformation("⏳ Starting delayed action...");
        
        // ✅ Correct: Use Game.Delay (WebAssembly-compatible).
        await Game.Delay(TimeSpan.FromSeconds(2));
        
        Game.Logger.LogInformation("✅ Delayed action completed");
        
        // ❌ Incorrect: Do not use Task.Delay (will not work in WebAssembly).
        // await Task.Delay(2000);
    }
    
    /// <summary>
    /// Async animation sequence pattern.
    /// </summary>
    public async Task PlayAnimationSequence(Unit unit)
    {
        Game.Logger.LogInformation("🎬 Starting animation sequence for: {UnitName}", unit.Name);
        
        // Play attack animation
        unit.PlayAnimation("attack");
        await Game.Delay(TimeSpan.FromSeconds(1));
        
        // Play hit effect
        var hitEffect = new ActorParticle(EffectLinks.HitSpark.Data, false, null);
        await Game.Delay(TimeSpan.FromMilliseconds(500));
        
        // Return to idle animation
        unit.PlayAnimation("idle");
        
        Game.Logger.LogInformation("🎭 Animation sequence completed for: {UnitName}", unit.Name);
    }
    
    /// <summary>
    /// Async ability casting pattern.
    /// </summary>
    public async Task<bool> CastSpellAsync(Unit caster, Unit target, GameDataAbility spellData)
    {
        Game.Logger.LogInformation("✨ Casting spell: {SpellName}", spellData.Name);
        
        try
        {
            // Check spell conditions
            if (!CanCastSpell(caster, spellData))
            {
                Game.Logger.LogWarning("⚠️ Cannot cast spell: conditions not met");
                return false;
            }
            
            // Pre-cast animation
            caster.PlayAnimation("cast_start");
            await Game.Delay(spellData.CastTime);
            
            // Execute spell effect
            await ExecuteSpellEffect(caster, target, spellData);
            
            // Post-cast animation
            caster.PlayAnimation("cast_end");
            await Game.Delay(TimeSpan.FromMilliseconds(200));
            
            // Return to idle state
            caster.PlayAnimation("idle");
            
            Game.Logger.LogInformation("🎯 Spell cast successfully: {SpellName}", spellData.Name);
            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error casting spell: {SpellName}", spellData.Name);
            return false;
        }
    }
}
```

## Key Rules

### ✅ ALWAYS Use Game.Delay
```csharp
// Correct - WebAssembly compatible
await Game.Delay(TimeSpan.FromSeconds(1));
await Game.Delay(TimeSpan.FromMilliseconds(500));
```

### ❌ NEVER Use Task.Delay
```csharp
// Wrong - Will NOT work in WebAssembly
await Task.Delay(1000);  // DON'T DO THIS!
```

## Common Async Patterns

### Timed Actions
```csharp
public async Task TimedPowerUp(Unit unit)
{
    // Apply power-up
    unit.AddBuff(BuffLinks.SpeedBoost.Data);
    
    // Wait for duration
    await Game.Delay(TimeSpan.FromSeconds(10));
    
    // Remove power-up
    unit.RemoveBuff(BuffLinks.SpeedBoost.Data);
}
```

### Sequential Actions
```csharp
public async Task SpawnWaves()
{
    for (int wave = 1; wave <= 5; wave++)
    {
        Game.Logger.LogInformation("🌊 Spawning wave {Wave}", wave);
        SpawnEnemies(wave * 3);
        
        // Wait between waves
        await Game.Delay(TimeSpan.FromSeconds(30));
    }
}
```

### Parallel Actions
```csharp
public async Task MultipleEffects()
{
    // Start multiple async operations
    var tasks = new List<Task>
    {
        PlaySound(),
        ShowVisualEffect(),
        ApplyDamageOverTime()
    };
    
    // Wait for all to complete
    await Task.WhenAll(tasks);
}
```

### Cancellable Operations
```csharp
private CancellationTokenSource cts;

public async Task StartRepeatingAction()
{
    cts = new CancellationTokenSource();
    
    try
    {
        while (!cts.Token.IsCancellationRequested)
        {
            DoAction();
            await Game.Delay(TimeSpan.FromSeconds(1));
        }
    }
    catch (OperationCanceledException)
    {
        Game.Logger.LogInformation("Action cancelled");
    }
}

public void StopRepeatingAction()
{
    cts?.Cancel();
}
```

## Best Practices

1. **Always use Game.Delay**: Never use Task.Delay
2. **Handle exceptions**: Wrap async code in try-catch
3. **Return Task<bool>**: For operations that can fail
4. **Use TimeSpan**: More readable than milliseconds
5. **Log async operations**: Track start and completion
6. **Consider cancellation**: For long-running operations

## Common Mistakes

```csharp
// ❌ WRONG - Using Task.Delay
await Task.Delay(1000);

// ❌ WRONG - Blocking async code
Game.Delay(TimeSpan.FromSeconds(1)).Wait();

// ❌ WRONG - Fire and forget without handling
async void DoSomething() { } // Avoid async void

// ✅ CORRECT
await Game.Delay(TimeSpan.FromSeconds(1));
async Task DoSomething() { } // Return Task
```
