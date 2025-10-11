# 🏗️ Entity-Component-Data Pattern Guide

This document details the relationship and usage patterns between Entities, Components, and GameData in the WasiCore framework, helping developers to correctly understand and use this core design pattern.

## 📋 Table of Contents

- [🎯 Core Concepts](#core-concepts)
- [🏭 Factory Pattern Design](#factory-pattern-design)
- [🔧 Inheritance and Extension](#inheritance-and-extension)
- [💡 Practical Guide](#practical-guide)
- [⚖️ Design Analysis](#design-analysis)
- [🚀 Best Practices](#best-practices)

## 🎯 Core Concepts

### Three-Layer Architecture

The WasiCore framework uses a three-layer **Entity-Component-Data** architecture:

```
GameData -> Entity/Component -> Game Logic
    ↓ Factory   ↓ Runtime         ↓ Business
    Pattern     Instance          Logic
  Config &    Concrete          Actual
  Templates   Objects           Functionality
```

### 🎮 Entity System

#### Entity
- **Unit**: Interactive objects like characters, NPCs, and buildings.
- **Item**: Objects like equipment, consumables, and props.
- **Ability**: Abilities like spells, skills, and passive effects.

#### GameData
- **GameDataUnit**: Data definition and configuration template for units.
- **GameDataItem**: Data definition and configuration template for items.
- **GameDataAbility**: Data definition and configuration template for abilities.

#### Relationship
Each entity type has a corresponding `GameData` type that acts as a **factory**, responsible for creating and configuring entity instances.

## 🏭 Factory Pattern Design

### Factory Method Pattern

The framework's `GameData` classes implement the **Factory Method pattern**, where each `GameData` type has a corresponding creation method:

```csharp
// GameDataUnit acts as a factory for Unit
public virtual Unit? CreateUnit(Player player, ScenePoint scenePoint, Angle facing, 
                               IExecutionContext? creationContext = null, bool useDefaultAI = false)
{
    // Create a Unit instance
    ScopeScript.LastCreatedUnit = new Unit(Link, player, scenePoint, facing) 
    { 
        CreationContext = creationContext 
    };
    
    if (useDefaultAI)
    {
        AIThinkTree.AddDefaultAI(ScopeScript.LastCreatedUnit);
    }
    
    return ScopeScript.LastCreatedUnit;
}
```

### Other Factory Examples

```csharp
// GameDataItem as a factory for Item
public abstract class GameDataItem
{
    protected abstract Item CreateItem(Unit unit);
    
    public Item CreateItem(ScenePoint scene, Player? player = null)
    {
        return CreateItem(CreateItemUnit(scene, player));
    }
}

// GameDataAbility as a factory for Ability
public partial class GameDataAbility
{
    public virtual Ability CreateAbility(Unit owner, Item? item = null);
}
```

## 🔧 Inheritance and Extension

### ⚠️ Important Principle

> **When you need to inherit from the `Unit` class, you must also inherit from `GameDataUnit` and override its `CreateUnit` method.**

This is because `GameDataUnit.CreateUnit` directly constructs the base `Unit` class:

```csharp
// Inside GameDataUnit.CreateUnit
ScopeScript.LastCreatedUnit = new Unit(Link, player, scenePoint, facing);
```

### Correct Inheritance Pattern

#### 1. Inherit from the `Unit` Class

```csharp
// Create a custom unit class
public class Hero : Unit
{
    public Hero(IGameLink<GameDataUnit> link, Player player, ScenePoint scenePoint, Angle facing) 
        : base(link, player, scenePoint, facing)
    {
        // Hero-specific initialization logic
        InitializeHeroFeatures();
    }
    
    // Hero-specific properties
    public int HeroLevel { get; set; }
    public List<Skill> UltimateSkills { get; set; } = new();
    
    // Hero-specific methods
    public void LevelUp()
    {
        HeroLevel++;
        UpdateHeroStats();
    }
    
    private void InitializeHeroFeatures()
    {
        // Initialize hero-specific features
    }
}
```

#### 2. Inherit from the `GameDataUnit` Class

```csharp
// Create a corresponding GameData class
public class GameDataHero : GameDataUnit
{
    // Hero-specific data configuration
    public int BaseLevel { get; set; } = 1;
    public List<IGameLink<GameDataAbility>> UltimateAbilities { get; set; } = new();
    public HeroType HeroType { get; set; }
    
    // Override the CreateUnit method
    public override Unit? CreateUnit(Player player, ScenePoint scenePoint, Angle facing, 
                                   IExecutionContext? creationContext = null, bool useDefaultAI = false)
    {
        try
        {
            // Create a Hero instance instead of a Unit
            var hero = new Hero(Link, player, scenePoint, facing) 
            { 
                CreationContext = creationContext,
                HeroLevel = BaseLevel
            };
            
            // Add hero-specific initialization logic
            InitializeHeroSpecificFeatures(hero);
            
            if (useDefaultAI)
            {
                AIThinkTree.AddDefaultAI(hero);
            }
            
            ScopeScript.LastCreatedUnit = hero;
            return hero;
        }
        catch (Exception e)
        {
            Game.Logger.LogError(e, "Failed to create hero {hero} at {scenePoint}", this, scenePoint);
            return null;
        }
    }
    
    private void InitializeHeroSpecificFeatures(Hero hero)
    {
        // Add hero-specific abilities
        foreach (var abilityLink in UltimateAbilities)
        {
            if (abilityLink?.Data != null)
            {
                var ability = abilityLink.Data.CreateAbility(hero);
                hero.UltimateSkills.Add(ability);
            }
        }
    }
}
```

### Inheritance Patterns for Other Components

#### Custom Item Class

```csharp
// Inherit from Item
public class Equipment : Item
{
    public Equipment(Unit host, IGameLink<GameDataItem> link) : base(host, link)
    {
        InitializeEquipmentFeatures();
    }
    
    public EquipmentSlot Slot { get; set; }
    public Dictionary<PropertyType, float> StatBonuses { get; set; } = new();
}

// Inherit from GameDataItem
public class GameDataEquipment : GameDataItem
{
    public EquipmentSlot RequiredSlot { get; set; }
    public Dictionary<PropertyType, float> BaseStats { get; set; } = new();
    
    protected override Item CreateItem(Unit unit)
    {
        return new Equipment(unit, Link)
        {
            Slot = RequiredSlot,
            StatBonuses = BaseStats
        };
    }
}
```

#### Custom Ability Class

```csharp
// Inherit from Ability
public class PassiveAbility : Ability
{
    public PassiveAbility(Unit owner, IGameLink<GameDataAbility> link, Item? item = null) 
        : base(owner, link, item)
    {
        // Passive-specific initialization
        IsPassive = true;
        AutoActivate = true;
    }
    
    public bool IsActive { get; set; }
    public TimeSpan Duration { get; set; }
}

// Inherit from GameDataAbility
public class GameDataPassiveAbility : GameDataAbility
{
    public TimeSpan PassiveDuration { get; set; }
    public bool AutoTrigger { get; set; } = true;
    
    public override Ability CreateAbility(Unit owner, Item? item = null)
    {
        return new PassiveAbility(owner, Link, item)
        {
            Duration = PassiveDuration
        };
    }
}
```

## 💡 Practical Guide

### Standard Entity Creation Workflow

#### 1. Define GameData

```csharp
// Create GameData in your initialization code
new GameDataHero(ScopeData.Unit.TestHero)
{
    Name = "Test Hero",
    BaseLevel = 1,
    HeroType = HeroType.Warrior,
    AttackableRadius = 50,
    Properties = new()
    {
        { ScopeData.UnitProperty.LifeMax, 1500 },
        { ScopeData.UnitProperty.ManaMax, 800 },
        { ScopeData.UnitProperty.AttackDamage, 120 },
        { ScopeData.UnitProperty.MoveSpeed, 350 }
    },
    UltimateAbilities = 
    {
        ScopeData.Ability.HeroicStrike,
        ScopeData.Ability.BattleRoar
    }
};
```

#### 2. Use the Factory Method to Create Instances

```csharp
// Create a hero instance via GameData
var heroData = ScopeData.Unit.TestHero.Data as GameDataHero;
var hero = heroData?.CreateUnit(player, spawnPoint, facing) as Hero;

if (hero != null)
{
    // Hero created successfully, perform further actions
    hero.LevelUp();
    Console.WriteLine($"Created a level {hero.HeroLevel} hero: {hero.Cache.Name}");
}
```

### Batch Creation and Management

```csharp
public class UnitFactory
{
    /// <summary>
    /// Generic method for creating units.
    /// </summary>
    public static T? CreateUnit<T>(IGameLink<GameDataUnit> link, Player player, ScenePoint position) 
        where T : Unit
    {
        var unitData = link.Data;
        if (unitData == null)
        {
            Game.Logger.LogError("Could not find unit data: {LinkName}", link.FriendlyName);
            return null;
        }
        
        var unit = unitData.CreateUnit(player, position, new Angle(0));
        return unit as T;
    }
    
    /// <summary>
    /// Creates a hero.
    /// </summary>
    public static Hero? CreateHero(IGameLink<GameDataUnit> heroLink, Player player, ScenePoint position)
    {
        return CreateUnit<Hero>(heroLink, player, position);
    }
    
    /// <summary>
    /// Creates a batch of units.
    /// </summary>
    public static List<Unit> CreateUnits(IEnumerable<IGameLink<GameDataUnit>> unitLinks, 
                                        Player player, ScenePoint basePosition)
    {
        var units = new List<Unit>();
        var offset = Vector3.Zero;
        
        foreach (var link in unitLinks)
        {
            var unit = CreateUnit<Unit>(link, player, basePosition + offset);
            if (unit != null)
            {
                units.Add(unit);
                offset += new Vector3(100, 0, 0); // Place them apart
            }
        }
        
        return units;
    }
}
```

## ⚖️ Design Analysis

### 💪 Advantages

#### 1. **Separation of Concerns**
- **GameData**: Manages data configuration and instance creation.
- **Entity/Component**: Manages runtime logic and state.
- **Game Logic**: Manages business processes and interactions.

#### 2. **High Extensibility**
- Easily extend new entity types through inheritance.
- `GameData` supports hot-reloading for dynamic configuration changes.
- The factory pattern supports complex creation logic.

#### 3. **Type Safety**
- Generic design ensures compile-time type checking.
- Strongly-typed `GameLink` prevents runtime errors.
- Clear inheritance relationships ensure type consistency.

#### 4. **Data-Driven**
- Decouples configuration from code, making balancing easier.
- Supports visual editing with a data editor.
- Different behaviors can be achieved through data configuration.

### 🚨 Potential Issues

#### 1. **Inheritance Coupling**
- Requires inheriting from both the entity and `GameData` classes.
- Violates the "composition over inheritance" principle.
- Increases system complexity.

#### 2. **Factory Method Limitations**
- Each `GameData` can only create one type of entity.
- Difficult to support polymorphic creation.
- Extending new types requires modifying existing code.

#### 3. **Type Casting Risks**
- Requires explicit type casting.
- Overhead of runtime type checks.
- Potential for type mismatch errors.

### 🛠️ Improvement Suggestions

#### 1. **Introduce a Generic Factory**

```csharp
// Improved generic factory design
public abstract class GameDataUnit<T> : GameDataUnit where T : Unit
{
    public abstract T CreateUnitTyped(Player player, ScenePoint scenePoint, Angle facing, 
                                     IExecutionContext? creationContext = null, bool useDefaultAI = false);
    
    public override Unit? CreateUnit(Player player, ScenePoint scenePoint, Angle facing, 
                                   IExecutionContext? creationContext = null, bool useDefaultAI = false)
    {
        return CreateUnitTyped(player, scenePoint, facing, creationContext, useDefaultAI);
    }
}

// Example usage
public class GameDataHero : GameDataUnit<Hero>
{
    public override Hero CreateUnitTyped(Player player, ScenePoint scenePoint, Angle facing, 
                                        IExecutionContext? creationContext = null, bool useDefaultAI = false)
    {
        return new Hero(Link, player, scenePoint, facing) { CreationContext = creationContext };
    }
}
```

#### 2. **Use Composition Over Inheritance**

```csharp
// Use composition instead of inheritance
public class UnitTypeConfig
{
    public string UnitTypeName { get; set; }
    public Type UnitType { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class GameDataUnit
{
    public UnitTypeConfig TypeConfig { get; set; }
    
    public virtual Unit? CreateUnit(Player player, ScenePoint scenePoint, Angle facing)
    {
        if (TypeConfig?.UnitType == null)
            return null;
            
        // Use reflection or dependency injection to create an instance
        var unit = Activator.CreateInstance(TypeConfig.UnitType, Link, player, scenePoint, facing) as Unit;
        
        // Apply configuration properties
        ApplyConfiguration(unit, TypeConfig.Properties);
        
        return unit;
    }
}
```

#### 3. **Dependency Injection Pattern**

```csharp
// Use dependency injection and factory registration
public interface IUnitFactory
{
    Unit CreateUnit(IGameLink<GameDataUnit> link, Player player, ScenePoint position, Angle facing);
}

public class UnitFactoryRegistry
{
    private readonly Dictionary<Type, IUnitFactory> _factories = new();
    
    public void RegisterFactory<T>(IUnitFactory factory) where T : GameDataUnit
    {
        _factories[typeof(T)] = factory;
    }
    
    public Unit? CreateUnit(GameDataUnit data, Player player, ScenePoint position, Angle facing)
    {
        if (_factories.TryGetValue(data.GetType(), out var factory))
        {
            return factory.CreateUnit(data.Link, player, position, facing);
        }
        
        // Fallback to the default creation method
        return new Unit(data.Link, player, position, facing);
    }
}
```

## 🚀 Best Practices

### 1. **Naming Conventions**
- Entity classes: `Unit` → `Hero`, `Monster`, `Building`
- `GameData` classes: `GameDataUnit` → `GameDataHero`, `GameDataMonster`, `GameDataBuilding`
- Maintain a consistent naming correspondence.

### 2. **Control Inheritance Hierarchy**
- Limit inheritance depth to avoid deep chains.
- Prefer composition for extending functionality.
- Consider using interfaces to define behavior contracts.

### 3. **Error Handling**
- Add complete error handling to factory methods.
- Provide meaningful error messages and logs.
- Ensure graceful degradation on creation failure.

### 4. **Performance Optimization**
- Cache frequently used `GameData` instances.
- Use object pools for frequently created entities.
- Consider lazy initialization for non-critical components.

### 5. **Unit Testing**
- Write unit tests for each custom factory method.
- Test the correctness of inheritance relationships.
- Verify the safety of type casting.

---

## Summary

The **Entity-Component-Data pattern** in the WasiCore framework is a powerful design that provides a flexible entity creation mechanism through `GameData` factories. While the current design has some coupling issues, it can effectively support the development of complex game systems with proper use and improvements.

When using this pattern, developers need to remember the principle of **inheriting from both entity and `GameData` classes** and pay attention to type safety and error handling. By following best practices, you can build maintainable and extensible game systems.
