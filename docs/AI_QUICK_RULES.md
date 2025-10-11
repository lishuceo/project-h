# 🤖 AI Quick Rules for WasiCore Development

> **This document contains essential rules for AI agents developing WasiCore games. These rules are designed to be included in system prompts to prevent common mistakes.**

## 🚨 Critical Rules

### 1. .NET Framework Version
- **ALWAYS use .NET 9.0** for all projects
- Ensure `<TargetFramework>net9.0</TargetFramework>` in .csproj files
- Never target older versions like net8.0 or net6.0

### 2. API Usage and Compilation Errors
When encountering API issues or compilation errors:
- **NEVER guess API names or signatures**
- **ALWAYS search in `api-client-reference/` for client-side APIs**
- **ALWAYS search in `api-server-reference/` for server-side APIs**
- Use `grep` tool to search XML files for exact API definitions

**Example workflow:**
```bash
# Error: 'Unit' does not contain a definition for 'ApplyDamage'
# Step 1: Search for damage-related APIs
grep -i "damage" api-client-reference/*.xml

# Step 2: Found TakeDamage in GameCore.xml
# Step 3: Check exact signature
grep -B2 -A5 "TakeDamage" api-client-reference/GameCore.xml

# Step 4: Use correct API
# unit.TakeDamage(damage, source) ✅
```

### 3. Reference Existing Examples
When creating game mechanics, **ALWAYS check src/ for similar examples first**:
- **NEVER create from scratch** if a similar game exists
- **ALWAYS copy and modify** existing patterns
- This reduces errors and ensures best practices

**Available examples in src/:**
- `ARPGTemplate/` - **[3D]** Complete ARPG with abilities, items, units
- `VampireSurvivors3D/` - **[3D]** Survival game with waves, upgrades
- `VampireSurvivor/` - **[2D]** 2D version of vampire survivors
- `Game2048/` - **[2D]** Grid-based puzzle game
- `Gomoku/` - **[2D]** Turn-based board game
- `FlappyBirdGame/` - **[2D]** Simple arcade game
- `UIFrameworkTest/` - **[2D/3D]** 流式UI最佳实践和示例
- `PrimitiveShapeTest/` - **[3D]** Physics and shapes
- `AISystemTest/` - **[3D]** AI behavior patterns

**Example workflow:**
```bash
# Task: Create a tower defense game
# Step 1: Check for similar mechanics
grep -r "wave" src/  # Found wave system in VampireSurvivors3D
grep -r "spawn" src/  # Found spawn patterns

# Step 2: Copy relevant files
# Step 3: Modify for your needs
```

### 4. Game Mode Definition and Configuration
When creating a new game, you MUST:

1. **Define a GameLink for your game mode:**
   ```csharp
   // In ScopeData.cs
   public static readonly GameLink<GameDataGameMode> MyNewGame = new("MyNewGame"u8);
   ```

2. **Create the GameDataGameMode:**
   ```csharp
   // In your game system's OnGameDataInitialization
   new GameDataGameMode(ScopeData.GameMode.MyNewGame)
   {
       Name = "My New Game"
   };
   ```

3. **Register in GlobalConfig.cs:**
   ```csharp
   // Add to AvailableGameModes dictionary
   GameDataGlobalConfig.AvailableGameModes = new()
   {
       // ... existing modes ...
       {"MyNewGame", ScopeData.GameMode.MyNewGame},
   };
   
   // Set as default test mode
   GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.MyNewGame;
   ```

## 📋 Reference Template

```csharp
// 1. Define GameLink in ScopeData.cs
public static readonly GameLink<GameDataGameMode> MyGame = new("MyGame"u8);

// 2. Register in game system
new GameDataGameMode(ScopeData.GameMode.MyGame) { Name = "My Game" };

// 3. Update GlobalConfig.cs
GameDataGlobalConfig.AvailableGameModes["MyGame"] = ScopeData.GameMode.MyGame;
GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.MyGame;
```

---
*This document should be included in AI system prompts for WasiCore development.*
