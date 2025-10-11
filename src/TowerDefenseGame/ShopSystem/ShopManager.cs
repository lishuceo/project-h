#if SERVER || CLIENT
using GameCore;
using System.Linq;
using GameData;

namespace GameEntry.TowerDefenseGame.ShopSystem;

/// <summary>
/// 塔防商店管理器 - 使用数据驱动配置
/// </summary>
public static class ShopManager
{
    /// <summary>
    /// 获取商店物品列表 - 从数编表动态获取
    /// </summary>
    public static ShopItemInfo[] GetShopItems()
    {
        return GameDataCategory<GameDataShopItem>.Catalog
            .Where(item => item.IsAvailable)
            .Select(item => new ShopItemInfo
            {
                ItemId = item.ItemId,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Icon = item.Icon
            })
            .ToArray();
    }

    /// <summary>
    /// 根据物品ID获取商店物品信息
    /// </summary>
    public static ShopItemInfo? GetShopItem(int itemId)
    {
        var shopData = GetShopDataByItemId(itemId);
        if (shopData == null) return null;

        return new ShopItemInfo
        {
            ItemId = shopData.ItemId,
            Name = shopData.Name,
            Description = shopData.Description,
            Price = shopData.Price,
            Icon = shopData.Icon
        };
    }

    /// <summary>
    /// 根据物品ID获取商店数据
    /// </summary>
    public static GameDataShopItem? GetShopDataByItemId(int itemId)
    {
        return GameDataCategory<GameDataShopItem>.Catalog
            .FirstOrDefault(item => item.ItemId == itemId);
    }

    /// <summary>
    /// 根据物品ID获取物品链接 - 从数编表获取
    /// </summary>
    public static GameData.IGameLink<GameCore.ItemSystem.Data.GameDataItem>? GetItemLinkFromId(int itemId)
    {
        var shopData = GetShopDataByItemId(itemId);
        if (shopData is GameDataShopItemTower towerData)
        {
            return towerData.ItemLink;
        }
        return null;
    }

    /// <summary>
    /// 根据物品ID获取建造技能链接
    /// </summary>
    public static GameData.IGameLink<GameCore.AbilitySystem.Data.GameDataAbility>? GetBuildAbilityFromId(int itemId)
    {
        var shopData = GetShopDataByItemId(itemId);
        if (shopData is GameDataShopItemTower towerData)
        {
            return towerData.BuildAbility;
        }
        return null;
    }

    /// <summary>
    /// 根据物品ID获取塔类型
    /// </summary>
    public static TowerType? GetTowerTypeFromId(int itemId)
    {
        var shopData = GetShopDataByItemId(itemId);
        if (shopData is GameDataShopItemTower towerData)
        {
            return towerData.TowerType;
        }
        return null;
    }
}
#endif
