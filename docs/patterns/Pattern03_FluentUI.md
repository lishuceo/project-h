# Pattern 3: Fluent UI Building

## Overview
Use the fluent API to create modern, responsive game UIs. This pattern is preferred over traditional `new Control()` syntax.

## Complete Code Example

```csharp
/// <summary>
/// Use the fluent API to create a modern game UI.
/// AI should prefer this pattern over the traditional `new Control()`.
/// </summary>
using static GameUI.Control.Extensions.UI;

public void CreateGameUI()
{
    // Fluent UI construction - an AI-friendly chained syntax.
    var mainMenu = VStack(20,
        
        // Title area
        Label("🎮 My Game").FontSize(32).Bold().Center(),
        
        // Button area
        VStack(15,
            Button("🎯 Start Game")
                .Size(200, 50)
                .OnClick((s, e) => StartGame()),
                
            Button("⚙️ Settings")
                .Size(200, 50) 
                .OnClick((s, e) => ShowSettings()),
                
            Button("🚪 Exit")
                .Size(200, 50)
                .OnClick((s, e) => ExitGame())
        ).Center(),
        
        // Version info
        Label("Version 1.0").FontSize(12).Opacity(0.7f).Center()
        
    ).FillParent().Background(Colors.DarkBlue);
    
    // Add to the root UI element.
    Game.UIRoot.AddChild(mainMenu);
    
    Game.Logger.LogInformation("🎨 Main menu UI created");
}

// Event handler methods
private void StartGame()
{
    Game.Logger.LogInformation("🚀 Game starting...");
    // Game start logic
}

private void ShowSettings()
{
    Game.Logger.LogInformation("⚙️ Opening settings...");
    // Settings UI logic
}

private void ExitGame()
{
    Game.Logger.LogInformation("👋 Game exiting...");
    // Exit game logic
}
```

## Fluent API Components

### Containers
```csharp
// Vertical stack
VStack(spacing, child1, child2, ...)

// Horizontal stack
HStack(spacing, child1, child2, ...)

// Grid (not yet implemented in fluent API)
```

### Basic Controls
```csharp
// Label
Label("Text").FontSize(16).Bold().Color(Colors.White)

// Button
Button("Click Me").Size(width, height).OnClick(handler)

// Input field
InputField("placeholder").OnTextChanged(handler)

// Image
Image(imageLink).Size(width, height)
```

### Common Modifiers
```csharp
.Size(width, height)         // Set size
.Position(x, y)              // Set position
.Center()                    // Center in parent
.FillParent()               // Fill parent container
.Background(color)          // Set background color
.Opacity(0.5f)              // Set transparency
.Visible(true/false)        // Toggle visibility
.Enable(true/false)         // Toggle interactivity
```

## Best Practices

1. **Import the UI namespace**: `using static GameUI.Control.Extensions.UI;`
2. **Chain methods**: Build UI in a single expression when possible
3. **Use containers**: Structure UI with VStack/HStack
4. **Handle events inline**: Use lambda expressions for simple handlers
5. **Add to UIRoot**: Don't forget to add your UI to `Game.UIRoot`

## Common UI Patterns

### Dialog Box
```csharp
var dialog = VStack(20,
    Label("Confirm Action").FontSize(20).Bold(),
    Label("Are you sure?").FontSize(14),
    HStack(10,
        Button("Yes").Size(80, 30).OnClick((s, e) => Confirm()),
        Button("No").Size(80, 30).OnClick((s, e) => Cancel())
    )
).Size(300, 150).Center().Background(Colors.DarkGray);
```

### HUD Element
```csharp
var hud = HStack(20,
    Label("Health: 100").FontSize(16),
    Label("Score: 0").FontSize(16)
).Position(10, 10).Background(Colors.Black.WithAlpha(0.7f));
```
