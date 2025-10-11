# Pattern 2: Data-Driven Object Creation

## Overview
The complete process for creating game objects using the data-driven architecture. Follow this sequence: Link → Data → Object.

## Complete Code Example

```csharp
/// <summary>
/// The complete process for creating a game unit.
/// AI should follow this sequence: Link → Data → Object.
/// </summary>
public void CreateGameUnit()
{
    // Step 1: Get the GameLink (compile-time type safety).
    var heroLink = UnitLinks.MyHero;
    
    // Step 2: Get the GameData (can be null at runtime).
    var heroData = heroLink.Data;
    if (heroData == null)
    {
        Game.Logger.LogError("❌ Hero data not found for link: {LinkId}", heroLink);
        return;
    }
    
    // Step 3: Create the game object from the data.
    var player = Player.GetById(1);
    var position = new ScenePoint(100, 100, Game.CurrentScene, 0);
    var hero = heroData.CreateUnit(player, position);
    
    // Step 4: Validate the result.
    if (hero != null)
    {
        Game.Logger.LogInformation("✅ Created hero: {HeroName} at {Position}", 
            hero.Name, position);
    }
}
```

## Key Concepts

### The Data-Driven Trinity
```
Define GameLink → Create GameData → Create GameObject
    ↓                  ↓                  ↓
Compile-time      Runtime data      Actual game
type checking     configuration     object
```

## Common Object Creation APIs

### Units
```csharp
var unit = unitData.CreateUnit(player, position);
```

### Buildings
```csharp
var building = buildingData.CreateBuilding(player, position);
```

### Items
```csharp
var item = itemData.CreateItem(position);
```

### Effects
```csharp
var effect = new ActorParticle(effectLink.Data, false, scope);
```

### Models
```csharp
var model = new ActorModel(modelLink.Data, false, scope);
```

## Error Handling Pattern

```csharp
public Unit CreateUnitSafely(GameLink<GameDataUnit> unitLink, Player player, ScenePoint position)
{
    // Validate link
    if (unitLink == null)
    {
        Game.Logger.LogError("❌ Unit link is null");
        return null;
    }
    
    // Get data
    var unitData = unitLink.Data;
    if (unitData == null)
    {
        Game.Logger.LogError("❌ Unit data not found for link: {LinkId}", unitLink);
        return null;
    }
    
    // Create unit
    var unit = unitData.CreateUnit(player, position);
    if (unit == null)
    {
        Game.Logger.LogError("❌ Failed to create unit from data: {DataName}", unitData.Name);
        return null;
    }
    
    Game.Logger.LogInformation("✅ Unit created: {UnitName}", unit.Name);
    return unit;
}
```

## Important Rules

1. **Never use `new GameData()`** - Always reference via Link
2. **Always check if Data is null** - Data might not be registered
3. **Use ScenePoint for positions** - Don't hardcode coordinates
4. **Validate creation results** - Creation can fail
