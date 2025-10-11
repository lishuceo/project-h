using GameCore;
using System.Collections.Concurrent;

namespace GameEntry.TowerDefenseGame.ResourceSystem;

/// <summary>
/// 玩家资源管理器 - 管理玩家的金币等资源
/// </summary>
public static class PlayerResourceManager
{
    // 玩家金币存储 (PlayerId -> GoldAmount)
    private static readonly ConcurrentDictionary<int, int> playerGold = new();

    /// <summary>
    /// 初始化玩家资源
    /// </summary>
    public static void InitializePlayerResources(int playerId, int initialGold = 100)
    {
        playerGold[playerId] = initialGold;
        Game.Logger.LogInformation("💰 Player {playerId} initialized with {gold} gold", playerId, initialGold);
    }

    /// <summary>
    /// 获取玩家金币数量
    /// </summary>
    public static int GetPlayerGold(int playerId)
    {
        return playerGold.GetValueOrDefault(playerId, 0);
    }

    /// <summary>
    /// 检查玩家是否有足够的金币
    /// </summary>
    public static bool HasEnoughGold(int playerId, int amount)
    {
        return GetPlayerGold(playerId) >= amount;
    }

    /// <summary>
    /// 扣除玩家金币
    /// </summary>
    public static bool DeductGold(int playerId, int amount)
    {
        if (!HasEnoughGold(playerId, amount))
        {
            Game.Logger.LogWarning("⚠️ Player {playerId} doesn't have enough gold: has {current}, needs {amount}", 
                playerId, GetPlayerGold(playerId), amount);
            return false;
        }

        playerGold[playerId] = GetPlayerGold(playerId) - amount;
        Game.Logger.LogInformation("💸 Deducted {amount} gold from player {playerId}, remaining: {remaining}", 
            amount, playerId, GetPlayerGold(playerId));
        return true;
    }

    /// <summary>
    /// 给玩家增加金币
    /// </summary>
    public static void AddGold(int playerId, int amount)
    {
        playerGold[playerId] = GetPlayerGold(playerId) + amount;
        Game.Logger.LogInformation("💰 Added {amount} gold to player {playerId}, total: {total}", 
            amount, playerId, GetPlayerGold(playerId));
    }

    /// <summary>
    /// 设置玩家金币数量
    /// </summary>
    public static void SetPlayerGold(int playerId, int amount)
    {
        playerGold[playerId] = amount;
        Game.Logger.LogInformation("💰 Set player {playerId} gold to {amount}", playerId, amount);
    }

    /// <summary>
    /// 获取所有玩家的资源信息
    /// </summary>
    public static Dictionary<int, int> GetAllPlayerResources()
    {
        return new Dictionary<int, int>(playerGold);
    }
}

