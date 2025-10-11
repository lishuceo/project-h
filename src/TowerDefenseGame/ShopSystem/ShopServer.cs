#if SERVER
using GameCore;
using GameCore.PlayerAndUsers;
using GameCore.ProtocolServerTransient;
using GameEntry.TowerDefenseGame.ShopSystem;
using System;

namespace GameEntry.TowerDefenseGame.ShopSystem;

/// <summary>
/// 商店系统服务端逻辑
/// </summary>
public static class ShopServer
{
    /// <summary>
    /// 处理打开商店命令
    /// </summary>
    public static void HandleOpenShopCommand(Player player)
    {
        try
        {
            Game.Logger.LogInformation("🛒 Player {PlayerId} opened shop", player.Id);
            
            // 发送商店数据到客户端
            var shopItems = ShopManager.GetShopItems();
            var shopResult = new ProtoShopResult
            {
                IsOpen = true,
                ItemCount = shopItems.Length
            };

            shopResult.SendTo(player, null);
            Game.Logger.LogInformation("✅ Shop data sent to player {PlayerId} with {ItemCount} items", player.Id, shopItems.Length);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to handle open shop command");
        }
    }

    /// <summary>
    /// 处理购买物品命令
    /// </summary>
    public static void HandlePurchaseItemCommand(Player player, PurchaseRequest purchaseData)
    {
        try
        {
            Game.Logger.LogInformation("💰 Player {PlayerId} wants to purchase {Quantity}x {ItemId}", 
                player.Id, purchaseData.Quantity, purchaseData.ItemId);

            // 查找物品
            var shopItem = ShopManager.GetShopItem(purchaseData.ItemId);
            if (shopItem == null)
            {
                Game.Logger.LogWarning("⚠️ Item {ItemId} not found in shop", purchaseData.ItemId);
                return;
            }

            // 计算总价格  
            int totalPrice = shopItem.Value.Price * purchaseData.Quantity;
            
            // 检查玩家金币是否足够
            int currentGold = TowerDefenseServer.PlayerGold;
            if (currentGold < totalPrice)
            {
                Game.Logger.LogWarning("⚠️ Player {PlayerId} doesn't have enough gold. Need: {TotalPrice}, Has: {CurrentGold}", 
                    player.Id, totalPrice, currentGold);
                return;
            }

            // 扣除金币
            TowerDefenseServer.ModifyPlayerGold(-totalPrice);

            // 添加物品到玩家背包
            var mainUnit = player.MainUnit;
            if (mainUnit != null)
            {
                // 获取玩家的背包管理器
                var inventoryManager = GameCore.Container.ComponentExtension.GetComponent<GameCore.Container.InventoryManager>(mainUnit);
                if (inventoryManager != null)
                {
                    for (int i = 0; i < purchaseData.Quantity; i++)
                    {
                        // 根据物品ID创建对应的物品
                        var itemLink = ShopManager.GetItemLinkFromId(purchaseData.ItemId);
                        if (itemLink != null)
                        {
                            // 创建物品实例 - 需要场景位置，使用玩家位置
                            var playerPosition = new GameCore.SceneSystem.ScenePoint(new System.Numerics.Vector2(mainUnit.Position.X, mainUnit.Position.Y), mainUnit.Scene);
                            var createdItem = itemLink.Data?.CreateItem(playerPosition, player);
                            if (createdItem != null && createdItem is GameCore.ItemSystem.ItemPickable pickableItem)
                            {
                                // 尝试添加物品到背包
                                bool added = inventoryManager.Take(pickableItem);
                                if (added)
                                {
                                    Game.Logger.LogInformation("📦 Added {ItemName} to player inventory", 
                                        shopItem.Value.Name);
                                }
                                else
                                {
                                    Game.Logger.LogWarning("⚠️ Failed to add {ItemName} to inventory - inventory might be full", 
                                        shopItem.Value.Name);
                                    // 如果添加失败，应该退还金币并销毁物品
                                    TowerDefenseServer.ModifyPlayerGold(shopItem.Value.Price);
                                    createdItem.Unit?.Destroy();
                                    return;
                                }
                            }
                            else
                            {
                                Game.Logger.LogError("❌ Failed to create item instance for {ItemId}", purchaseData.ItemId);
                                return;
                            }
                        }
                        else
                        {
                            Game.Logger.LogError("❌ Failed to get item link for {ItemId}", purchaseData.ItemId);
                            return;
                        }
                    }
                }
                else
                {
                    Game.Logger.LogError("❌ Player inventory manager not found!");
                    // 退还金币
                    TowerDefenseServer.ModifyPlayerGold(totalPrice);
                    return;
                }
            }

            Game.Logger.LogInformation("✅ Purchase completed: {Quantity}x {ItemName} for {TotalPrice} gold", 
                purchaseData.Quantity, shopItem.Value.Name, totalPrice);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to handle purchase item command");
        }
    }

    /// <summary>
    /// 处理关闭商店命令
    /// </summary>
    public static void HandleCloseShopCommand(Player player)
    {
        try
        {
            Game.Logger.LogInformation("🛒 Player {PlayerId} closed shop", player.Id);
            
            var shopResult = new ProtoShopResult
            {
                IsOpen = false,
                ItemCount = 0
            };

            shopResult.SendTo(player, null);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to handle close shop command");
        }
    }
}
#endif
