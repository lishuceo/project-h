#if CLIENT || SERVER
using GameCore.ProtocolClientTransient;
using GameCore.ProtocolServerTransient;
using GameCore;
using GameCore.PlayerAndUsers;
using GameCore.ResourceType;
using System;
using System.Linq;

namespace GameEntry.TowerDefenseGame.ShopSystem;

/// <summary>
/// 商店命令类型枚举
/// </summary>
public enum ShopCommandType
{
    OpenShop,       // 打开商店
    PurchaseItem,   // 购买物品
    CloseShop       // 关闭商店
}

/// <summary>
/// 商店物品信息
/// </summary>
public struct ShopItemInfo
{
    public int ItemId { get; init; } // 使用数字ID而不是字符串
    public required string Name { get; init; }
    public required string Description { get; init; }
    public int Price { get; init; }
    public required Icon Icon { get; init; }
}

/// <summary>
/// 购买请求信息
/// </summary>
public struct PurchaseRequest
{
    public int ItemId { get; init; } // 使用数字ID而不是字符串
    public int Quantity { get; init; }
}

/// <summary>
/// 客户端向服务端发送商店命令的协议
/// </summary>
public readonly struct ProtoShopCommand : IProtocolClientTransient<ProtoShopCommand>
{
    public required ShopCommandType CommandType { get; init; }
    public required int PlayerId { get; init; }
    public required PurchaseRequest PurchaseData { get; init; } // 购买数据，仅在购买物品时使用

#if SERVER
    public readonly void Handle(Player player)
    {
        try
        {
            Game.Logger.LogInformation("🛒 Received shop command from client: {CommandType} (Player: {PlayerId})", 
                CommandType, player.Id);

            switch (CommandType)
            {
                case ShopCommandType.OpenShop:
                    ShopServer.HandleOpenShopCommand(player);
                    break;

                case ShopCommandType.PurchaseItem:
                    ShopServer.HandlePurchaseItemCommand(player, PurchaseData);
                    break;

                case ShopCommandType.CloseShop:
                    ShopServer.HandleCloseShopCommand(player);
                    break;

                default:
                    Game.Logger.LogWarning("⚠️ Unknown shop command: {CommandType}", CommandType);
                    break;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling shop command: {CommandType}", CommandType);
        }
    }
#endif
}

/// <summary>
/// 服务端向客户端发送商店数据的协议
/// </summary>
public readonly struct ProtoShopResult : IProtocolServerTransient<ProtoShopResult>
{
    public required bool IsOpen { get; init; }
    public required int ItemCount { get; init; } // 简化：只发送物品数量，客户端自己获取物品列表

#if CLIENT
    public readonly void Handle()
    {
        try
        {
            Game.Logger.LogDebug("🛒 Received shop data from server");
            // 客户端直接从商店管理器获取物品列表
            var items = IsOpen ? ShopManager.GetShopItems() : Array.Empty<ShopItemInfo>();
            ShopClient.UpdateShopFromServer(IsOpen, items);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling shop result");
        }
    }
#endif
}

#if CLIENT
/// <summary>
/// 客户端商店命令发送器
/// </summary>
public static class ShopCommandSender
{
    /// <summary>
    /// 发送打开商店命令
    /// </summary>
    public static bool SendOpenShopCommand(int playerId = 1)
    {
        var command = new ProtoShopCommand
        {
            CommandType = ShopCommandType.OpenShop,
            PlayerId = playerId,
            PurchaseData = new PurchaseRequest { ItemId = 0, Quantity = 0 }
        };

        bool success = command.SendToServer();
        Game.Logger.LogInformation("🛒 Open shop command sent: {Success}", success);
        return success;
    }

    /// <summary>
    /// 发送购买物品命令
    /// </summary>
    public static bool SendPurchaseItemCommand(int itemId, int quantity = 1, int playerId = 1)
    {
        var command = new ProtoShopCommand
        {
            CommandType = ShopCommandType.PurchaseItem,
            PlayerId = playerId,
            PurchaseData = new PurchaseRequest
            {
                ItemId = itemId,
                Quantity = quantity
            }
        };

        bool success = command.SendToServer();
        Game.Logger.LogInformation("💰 Purchase command sent: {ItemId} x{Quantity}, Success: {Success}", itemId, quantity, success);
        return success;
    }

    /// <summary>
    /// 发送关闭商店命令
    /// </summary>
    public static bool SendCloseShopCommand(int playerId = 1)
    {
        var command = new ProtoShopCommand
        {
            CommandType = ShopCommandType.CloseShop,
            PlayerId = playerId,
            PurchaseData = new PurchaseRequest { ItemId = 0, Quantity = 0 }
        };

        bool success = command.SendToServer();
        Game.Logger.LogInformation("🛒 Close shop command sent: {Success}", success);
        return success;
    }
}
#endif

#endif
