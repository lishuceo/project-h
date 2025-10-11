# CloudData System Quick Start

## Overview

The CloudData system is the core user data management service in the WasiCore framework. It provides a unified, type-safe, and high-performance API for handling cloud data operations. This guide will help you get started with the basics of the CloudData system.

### ⚠️ Important: `Player.Id` vs. `User.UserId`

**Before you start, you must understand this key distinction:**

-   **`Player.Id` (`int`)**: A temporary slot number (0, 1, 2, ...) for the current game session. It can change between sessions. ❌
-   **`User.UserId` (`long`)**: A globally unique and persistent identifier for a user. ✅

**CloudData operations must use either a `UserId` or a `Player` object (from which the `UserId` is automatically extracted).**

### 📝 Correct Parameter Types

```csharp
// ✅ Correct: Use a UserId parameter.
public async Task SavePlayerProgress(long userId, int level) 
{
    await CloudData.ForUser(userId).SetData("level", level).ExecuteAsync();
}

// ✅ Correct: Use a Player object parameter (recommended for game logic).
public async Task SavePlayerProgress(Player player, int level) 
{
    await CloudData.ForPlayer(player).SetData("level", level).ExecuteAsync();
}

// ❌ Incorrect: A misleading parameter name.
public async Task SavePlayerProgress(long playerId, int level)  // Looks like Player.Id!
{
    await CloudData.ForUser(playerId).SetData("level", level).ExecuteAsync();  // Confusing
}
```

## 5-Minute Quick Start

### 1. Basic Concepts

The CloudData system supports the following data types:

| Type | Purpose | Example |
|---|---|---|
| **BigInt** | Integer data | Level, experience, score |
| **VarChar255** | String data | Nickname, status |
| **Currency** | Currency data | Gold, diamonds |
| **CappedData** | Capped data (can be reset to 0 on a schedule) | Stamina, daily activity, check-in progress |
| **Blob** | Binary data | Configuration, saved games |
| **ListItem** | List data | Inventory, friends list |

### 2. Import Namespaces

```csharp
using GameCore.UserCloudData;
using static GameCore.UserCloudData.CloudData; // For simplified calls
```

### 3. Querying Data

```csharp
// Method 1: Use UserId (recommended for stored user IDs).
var result = await QueryUserDataAsync(
    userIds: [userId],
    keys: ["level", "experience", "gold"]
);

// Method 2: Use a Player object (convenience method, common in game logic).
var playerResult = await QueryPlayersDataAsync(
    players: [player],
    keys: ["level", "experience", "gold"]
);

if (playerResult.IsSuccess)
{
    var userData = playerResult.Data.First();
    var level = userData.BigIntData["level"];
    var exp = userData.BigIntData["experience"];
    var gold = userData.CurrencyData["gold"];
}
```

### 4. Basic Transaction Operations

```csharp
// Method 1: Use UserId.
var result = await ForUser(userId)
    .SetData("level", newLevel)
    .AddToData("experience", 1000)
    .AddCurrency("gold", 500)
    .WithDescription("Level up reward")
    .ExecuteAsync();

// Method 2: Use a Player object (convenience method).
var playerResult = await ForPlayer(player)
    .SetData("level", newLevel)
    .AddToData("experience", 1000)
    .AddCurrency("gold", 500)
    .WithDescription("Level up reward")
    .ExecuteAsync();
```

### 5. Error Handling

```csharp
if (result == UserCloudDataResult.Success)
{
    Game.Logger.LogInformation("Operation successful!");
}
else
{
    Game.Logger.LogError("Operation failed: {Result}", result);
}
```

## Common Use Cases

### Scenario 1: Player Login

```csharp
public async Task<PlayerLoginInfo> HandlePlayerLogin(long userId)  // Note: This is a UserId, not Player.Id.
{
    // Query basic player info.
    var playerData = await QueryUserDataAsync(
        userIds: [userId],  // userId is User.UserId
        keys: ["level", "experience", "last_login", "total_playtime"]
    );

    // Query player currency.
    var currencyData = await QueryCurrencyAsync(
        userIds: [userId],
        keys: ["gold", "diamond", "energy"]
    );

    // Update login time.
    var updateResult = await ForUser(userId)
        .SetData("last_login", DateTime.UtcNow.ToString())
        .AddToData("login_count", 1)
        .WithDescription("Player login")
        .ExecuteAsync();

    return new PlayerLoginInfo
    {
        Level = playerData.Data.First().BigIntData["level"],
        Experience = playerData.Data.First().BigIntData["experience"],
        Gold = currencyData.Data.First().CurrencyData["gold"],
        // ... other data
    };
}
```

### Scenario 2: Completing a Quest

```csharp
public async Task<bool> CompleteQuest(long userId, QuestInfo quest)  // Note: This is a UserId.
{
    var builder = ForUser(userId);  // userId is User.UserId, not Player.Id

    // Add experience and gold rewards.
    builder.AddCurrency("experience", quest.ExpReward)
           .AddCurrency("gold", quest.GoldReward);

    // Add item rewards.
    if (quest.ItemRewards.Any())
    {
        var itemRefs = builder.PrepareListItems("inventory", quest.ItemRewards);
        builder.AddListItems(itemRefs);
    }

    // Update quest status.
    builder.SetData($"quest_{quest.Id}_completed", true)
           .SetData($"quest_{quest.Id}_complete_time", DateTime.UtcNow.ToString());

    var result = await builder
        .WithDescription($"Completed quest: {quest.Name}")
        .ExecuteAsync();

    return result == UserCloudDataResult.Success;
}
```

### Scenario 3: Purchasing from a Shop

```csharp
public async Task<PurchaseResult> PurchaseItem(long userId, int itemId, int cost)  // UserId parameter
{
    // Query the player's current gold.
    var currentGold = await QueryCurrencyAsync(
        userIds: [userId],  // userId is User.UserId
        keys: ["gold"]
    );

    if (!currentGold.IsSuccess)
        return PurchaseResult.NetworkError;

    var goldAmount = currentGold.Data.First().CurrencyData["gold"];
    if (goldAmount < cost)
        return PurchaseResult.InsufficientFunds;

    // Execute the purchase.
    var builder = ForUser(userId);  // Use UserId
    var itemRef = builder.PrepareListItem("inventory", CreateItemData(itemId));

    var result = await builder
        .CostCurrency("gold", cost)        // Deduct gold
        .AddListItem(itemRef)              // Add item
        .SetData("last_purchase", DateTime.UtcNow.ToString())
        .WithDescription($"Purchased item {itemId}")
        .ExecuteAsync();

    if (result == UserCloudDataResult.Success)
    {
        return new PurchaseResult
        {
            Success = true,
            ItemId = itemRef.Id  // Return the generated item ID
        };
    }

    return PurchaseResult.TransactionFailed;
}
```

### Scenario 4: Bulk User Operations (Daily Rewards)

```csharp
public async Task DistributeDailyRewards(long[] userIds)  // An array of UserIds
{
    var tasks = userIds.Select(async userId =>
    {
        return await ForUser(userId)  // Use UserId
            .AddCurrency("gold", 100)
            .AddCurrency("energy", 20)
            .SetData("last_daily_reward", DateTime.UtcNow.ToString())
            .WithDescription("Daily login reward")
            .ExecuteAsync();
    });

    var results = await Task.WhenAll(tasks);
    
    // Count successes and failures.
    var successCount = results.Count(r => r == UserCloudDataResult.Success);
    Game.Logger.LogInformation("Daily rewards distributed: {SuccessCount}/{TotalCount} successful", successCount, results.Length);
}
```

### Scenario 5: Stamina System Management

💡 **There are multiple ways to implement a stamina system**:
-   **Scheduled Reset**: Full stamina recovery at a set time (e.g., midnight) → Use `CappedData` reset mechanism.
-   **Linear Recovery**: Recover 1 stamina per minute → Use `CappedData` + `LastUpdateTime`.
-   **Complex Logic**: VIP acceleration, item-based recovery → Use normal data + currency data.

```csharp
public async Task<EnergySystemResult> ManagePlayerEnergy(long userId, EnergyAction action, int amount = 0)  // UserId parameter
{
    switch (action)
    {
        case EnergyAction.Consume:
            // Consume stamina: Increase the consumed amount. Resets to 0 daily at midnight (full stamina recovery). Cap is 100.
            var consumeResult = await ForUser(userId)  // Use UserId
                .ModifyCappedData("energy_consumed", amount, 100, UserDataResetOption.Daily())
                .WithDescription($"Consumed stamina +{amount}")
                .ExecuteAsync();
            return EnergySystemResult.FromCloudResult(consumeResult);

        case EnergyAction.Query:
            // Query stamina status.
            var queryResult = await QueryCappedDataAsync([userId], ["energy_consumed"]);
            if (queryResult.IsSuccess)
            {
                var userData = queryResult.Data.First();
                var energyConsumed = userData.CappedData["energy_consumed"].Value;
                var maxEnergy = userData.CappedData["energy_consumed"].Cap;
                var remainingEnergy = maxEnergy - energyConsumed;  // Calculate remaining stamina.
                return new EnergySystemResult { Success = true, RemainingEnergy = (int)remainingEnergy };
            }
            return EnergySystemResult.FromCloudResult(queryResult);

        case EnergyAction.UpgradeCapacity:
            // Increase stamina capacity (VIP perk).
            var upgradeResult = await ForUser(userId)
                .ModifyCappedData("energy_consumed", 0, 100 + amount)  // New capacity
                .WithDescription($"Stamina capacity increased to {100 + amount}")
                .ExecuteAsync();
            return EnergySystemResult.FromCloudResult(upgradeResult);

        default:
            // Query current stamina status.
            var defaultQueryResult = await QueryCappedDataAsync(
                userIds: [userId],  // Use UserId
                keys: ["energy_consumed"]
            );
            
            if (defaultQueryResult.IsSuccess && defaultQueryResult.Data.Any())
            {
                var energyData = defaultQueryResult.Data.First();
                var energyConsumed = energyData.CappedData["energy_consumed"];
                var remainingEnergy = energyConsumed.Cap - energyConsumed.Value;  // Remaining stamina
                return new EnergySystemResult
                {
                    Success = true,
                    CurrentEnergy = remainingEnergy,  // Remaining stamina
                    MaxEnergy = energyConsumed.Cap,
                    NextResetTime = energyConsumed.NextResetTime
                };
            }
            return EnergySystemResult.QueryFailed;
    }
}

public enum EnergyAction
{
    Query,      // Query current status
    Restore,    // Restore stamina
    Consume,    // Consume stamina
    UpgradeCapacity  // Increase capacity
}
```

### Scenario 6: Using Player Objects Correctly in Game Logic

```csharp
// ✅ Recommended: Use Player objects in game event handlers.
public class GameEventHandler 
{
    // Save data when a player levels up.
    public async Task OnPlayerLevelUp(Player player, int newLevel)
    {
        // Use the Player convenience method, which automatically extracts the User.UserId.
        var result = await ForPlayer(player)
            .SetData("level", newLevel)
            .AddCurrency("skill_points", 1)
            .WithDescription($"Leveled up to {newLevel}")
            .ExecuteAsync();
    }
    
    // When a player gets an item.
    public async Task OnPlayerGetItem(Player player, ItemData item)
    {
        var builder = ForPlayer(player);  // Recommended: Use the Player object directly.
        var itemRef = builder.PrepareListItem("inventory", item);
        
        await builder
            .AddListItem(itemRef)
            .SetData("last_item_time", DateTime.UtcNow.ToString())
            .ExecuteAsync();
    }
    
    // When a UserId is passed from an external system.
    public async Task LoadUserProfile(long userId)  // Clearly indicate this is a UserId.
    {
        var userData = await QueryUserDataAsync(
            userIds: [userId],  // A UserId is needed here.
            keys: ["level", "experience", "last_login"]
        );
        // Process data...
    }
}

// ✅ Correct way to handle bulk player operations.
public async Task DistributeEventRewards(Player[] activePlayers)
{
    // Use an array of Player objects, which automatically filters out AI players.
    var result = await ForPlayers(activePlayers)
        .ForAllUsers(builder => builder
            .AddCurrency("event_token", 10)
            .SetData("last_event_reward", DateTime.UtcNow.ToString())
        )
        .ExecuteAllAsync();
}
```

## Advanced Features

### 1. Transaction Optimization

```csharp
// The system automatically merges operations on the same key.
var result = await ForUser(userId)  // Use UserId
    .AddCurrency("gold", 100)
    .AddCurrency("gold", 50)      // Automatically merged to +150
    .AddCurrency("gold", -20)     // Final result: +130
    .SetData("level", 10)
    .SetData("level", 11)         // Automatically optimized to the last value
    .WithOptimization(true)       // Enabled by default
    .ExecuteAsync();
```

### 2. Advanced List Item Operations

```csharp
var builder = ForUser(userId);

// Prepare multiple items.
var lootItems = GenerateLootRewards();
var itemRefs = builder.PrepareListItems("inventory", lootItems);

// Use the new item's ID within the transaction.
var firstItemId = itemRefs[0].Id;  // Get the ID before ExecuteAsync is called.

var result = await builder
    .AddListItems(itemRefs)
    .SetData("last_loot_item_id", firstItemId)  // Record the newest item's ID.
    .ExecuteAsync();
```

### 3. Error Handling Best Practices

```csharp
public async Task<GameResult> SafeExecuteTransaction(
    long userId, 
    Func<TransactionBuilder, TransactionBuilder> buildTransaction)
{
    try
    {
        var builder = ForUser(userId);
        builder = buildTransaction(builder);
        
        var result = await builder.ExecuteAsync();
        
        if (result == UserCloudDataResult.Success)
        {
            return GameResult.Success;
        }

        // Return different results based on the error type.
        return result switch
        {
            UserCloudDataResult.InsufficientFunds => GameResult.InsufficientResources,
            UserCloudDataResult.FailedToSend => GameResult.NetworkError,
            UserCloudDataResult.TransactionCommitEmpty => GameResult.InvalidOperation,
            _ => GameResult.UnknownError
        };
    }
    catch (ArgumentException)
    {
        return GameResult.InvalidArguments;
    }
    catch (InvalidOperationException)
    {
        return GameResult.InvalidOperation;
    }
}

// Example usage
var result = await SafeExecuteTransaction(userId, builder =>
    builder.AddCurrency("gold", 100)
           .SetData("level", newLevel)
);
```

## Performance Best Practices

### 1. Batch Query Optimization

```csharp
// ✅ Recommended: Query multiple users at once.
var usersData = await QueryUserDataAsync(
    userIds: allUserIds,
    keys: ["level", "experience"]
);

// ❌ Avoid: Looping single queries.
foreach (var userId in allUserIds)  // Note: This is a UserId.
{
    var userData = await QueryUserDataAsync([userId], keys);
}
```

### 2. Transaction Merging

```csharp
// ✅ Recommended: Complete all related operations in one transaction.
var result = await ForUser(userId)  // Use UserId
    .CostCurrency("energy", 10)      // Cost
    .AddCurrency("experience", 100)  // Reward
    .SetData("last_action", DateTime.UtcNow.ToString())
    .WithDescription("Battle summary")
    .ExecuteAsync();

// ❌ Avoid: Splitting into multiple transactions.
await ForUser(userId).CostCurrency("energy", 10).ExecuteAsync();
await ForUser(userId).AddCurrency("experience", 100).ExecuteAsync();
```

### 3. Limiting Query Scope

```csharp
// Use maxCount to limit large lists.
var recentItems = await QueryUserListItemsAsync(
    userId: userId,
    key: "inventory",
    maxCount: 50  // Only get the 50 most recent items.
);

// No limit needed for small lists.
var friends = await QueryUserListItemsAsync(
    userId: userId,
    key: "friends"  // Friends lists are usually small.
);
```

## Debugging and Troubleshooting

### 1. Enable Detailed Logging

```csharp
var result = await ForUser(userId)
    .AddCurrency("gold", amount)
    .WithDescription($"Operation details - Player:{userId}, Amount:{amount}, Time:{DateTime.UtcNow}")
    .ExecuteAsync();
```

### 2. Inspect Transaction Content

```csharp
var builder = ForUser(userId)
    .SetData("level", 10)
    .AddCurrency("gold", 100);

// Debug: Check the operations that will be executed.
var operations = builder.Build();
foreach (var op in operations)
{
    Game.Logger.LogDebug("Operation: {Type}, Key: {Key}, Value: {Value}", op.Type, op.Key, op.Value);
}

var result = await ExecuteTransactionAsync(operations, "Debug transaction");
```

### 3. Handle Timeouts

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var result = await ForUser(userId)
        .SetData("data", value)
        .ExecuteAsync()
        .WaitAsync(cts.Token);
}
catch (OperationCanceledException)
{
    Game.Logger.LogWarning("Operation timed out. Please check your network connection.");
}
```

## Common Errors and Solutions

| Error | Cause | Solution |
|---|---|---|
| `ArgumentException: UserId must be positive` | Invalid user ID | Ensure user ID is > 0 |
| `ArgumentException: Key cannot be empty` | Empty key name | Check for a valid key name |
| `UserCloudDataResult.InsufficientCurrency` | Not enough currency | Query current currency before deducting |
| `UserCloudDataResult.CapExceeded` | Capped data value exceeded the limit | Check the increment value in `ModifyCappedData` |
| `UserCloudDataResult.TransactionCommitEmpty`| Empty transaction | Ensure the transaction has valid operations |
| `UserCloudDataResult.FailedToSend` | Network issue | Check network connection; consider retrying |

## Next Steps

-   Read the [full CloudData system documentation](../systems/CloudDataSystem.md).
-   Learn about [asynchronous programming best practices](../best-practices/AsyncProgramming.md).
-   See the [framework testing guide](Testing.md) to learn how to test CloudData operations.
