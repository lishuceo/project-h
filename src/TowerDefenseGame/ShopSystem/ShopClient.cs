#if CLIENT
using GameCore;
using GameUI.Control.Primitive;
using GameUI.Control.Enum;
using GameUI.Control.Extensions;
using GameUI.Brush;
using GameUI.Struct;
using GameUI.Enum;
using System.Drawing;
using System;
using GameEntry.TowerDefenseGame.ShopSystem;

namespace GameEntry.TowerDefenseGame.ShopSystem;

/// <summary>
/// 商店系统客户端UI逻辑
/// </summary>
public static class ShopClient
{
    // UI控件
    private static Panel? shopPanel;
    private static bool isShopVisible = false;

    /// <summary>
    /// 更新商店UI
    /// </summary>
    public static void UpdateShopFromServer(bool isOpen, ShopItemInfo[] items)
    {
        try
        {
            isShopVisible = isOpen;

            if (isOpen)
            {
                // 显示商店界面
                ShowShop(items);
            }
            else
            {
                // 隐藏商店界面
                HideShop();
            }

            Game.Logger.LogInformation("🛒 Shop UI updated: Open={IsOpen}, Items={ItemCount}", isOpen, items.Length);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error updating shop UI");
        }
    }

    /// <summary>
    /// 显示商店界面
    /// </summary>
    private static void ShowShop(ShopItemInfo[] items)
    {
        try
        {
            // 创建商店面板
            shopPanel = new Panel()
            {
                Width = 600,
                Height = 400,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0)), // 半透明黑色背景
            };

            // 创建商店标题
            var shopTitle = new Label()
            {
                Text = "🛒 塔防商店",
                FontSize = 24,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 0, 0),
            };

            shopTitle.Parent = shopPanel;

            // 为每个商店物品创建UI
            for (int i = 0; i < items.Length && i < 4; i++) // 最多显示4个物品
            {
                var item = items[i];
                CreateShopItemUI(item, i, shopPanel);
            }

            // 添加商店面板到根节点
            shopPanel.AddToRoot();

            Game.Logger.LogInformation("✅ Shop panel created with {ItemCount} items", items.Length);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error showing shop");
        }
    }

    /// <summary>
    /// 创建单个商店物品的UI
    /// </summary>
    private static void CreateShopItemUI(ShopItemInfo item, int index, Panel parent)
    {
        try
        {
            // 计算位置
            int x = (index % 2) * 280 + 20; // 两列布局
            int y = (index / 2) * 100 + 60; // 每行间距100

            // 创建物品按钮
            var itemButton = new Button()
            {
                Width = 260,
                Height = 80,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(x, y, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(180, 34, 139, 34)),
            };

            // 创建物品信息标签
            var itemLabel = new Label()
            {
                Text = $"{item.Name}\n💰 {item.Price} 金币",
                FontSize = 14,
                TextColor = new SolidColorBrush(Color.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            itemLabel.Parent = itemButton;
            itemButton.Parent = parent;

            // 绑定点击事件
            itemButton = itemButton.Click(() => OnShopItemClicked(item));

            Game.Logger.LogDebug("Created shop item UI: {ItemName} at position ({X}, {Y})", item.Name, x, y);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error creating shop item UI for {ItemName}", item.Name);
        }
    }

    /// <summary>
    /// 处理商店物品点击
    /// </summary>
    private static void OnShopItemClicked(ShopItemInfo item)
    {
        try
        {
            Game.Logger.LogInformation("🛒 Player clicked on shop item: {ItemName} (Price: {Price})", item.Name, item.Price);

            // 发送购买命令到服务端
            bool success = ShopCommandSender.SendPurchaseItemCommand(item.ItemId, 1, 1);
            
            if (success)
            {
                Game.Logger.LogInformation("✅ Purchase command sent for {ItemName}", item.Name);
            }
            else
            {
                Game.Logger.LogError("❌ Failed to send purchase command for {ItemName}", item.Name);
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error handling shop item click: {ItemName}", item.Name);
        }
    }

    /// <summary>
    /// 隐藏商店界面
    /// </summary>
    private static void HideShop()
    {
        try
        {
            if (shopPanel != null)
            {
                // 移除商店面板
                shopPanel.Parent = null;
                shopPanel = null;
                Game.Logger.LogInformation("✅ Shop panel hidden");
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error hiding shop");
        }
    }

    /// <summary>
    /// 获取商店状态
    /// </summary>
    public static bool IsShopVisible => isShopVisible;
}
#endif
