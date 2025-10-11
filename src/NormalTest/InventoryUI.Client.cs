#if CLIENT
using GameCore.BaseType;
using GameCore.Components;
using GameCore.Container;
using GameCore.EntitySystem;
using GameCore.OrderSystem;

using GameUI.Brush;
using GameUI.Control;
using GameUI.Control.Primitive;
using GameUI.Enum;
using GameUI.Struct;

using Microsoft.Extensions.Logging;

using System.Drawing;

using Player = GameCore.PlayerAndUsers.Player;

namespace GameEntry;

internal class InventoryUI
{
    private Panel _inventoryPanel = null!;
    private readonly Player _player; 
    private const int SlotSize = 50; // 物品栏格子的大小
    private const int SlotMargin = 10; // 物品栏格子之间的间距
    public InventoryUI(Player player)
    {
        _player = player;
        InitializeUI();
    }

    private void InitializeUI()
    {
        _inventoryPanel = new Panel
        {
            Width = 400,
            Height = 600,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.Gray)
        };

        PopulateInventory();
        _ = _inventoryPanel.AddToParent(UIRoot.Instance);
    }

    private void PopulateInventory()
    {
        var inventoryManager = _player.MainUnit?.GetComponent<InventoryManager>();
        if (inventoryManager == null)
        {
            return;
        }

        var slotIndex = 0;
        var itemsPerRow = (int)((_inventoryPanel.Width - SlotMargin) / (SlotSize + SlotMargin)); // 动态计算每行显示的物品栏格子数量

        foreach (var inventory in inventoryManager.Inventories)
        {
            foreach (var slot in inventory.Slots)
            {
                var itemButton = new Button
                {
                    Width = SlotSize,
                    Height = SlotSize,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(
                        SlotMargin + slotIndex % itemsPerRow * (SlotSize + SlotMargin),
                        SlotMargin + slotIndex / itemsPerRow * (SlotSize + SlotMargin),
                        0, 0),
                    Background = new SolidColorBrush(Color.LightBlue),
                    Children = [
                        new Label
                        {
                            Text = slot.Item?.ToString() ?? "Empty",
                            TextColor = new SolidColorBrush(Color.Black),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    ]
                };

                itemButton.OnPointerClicked += (s, e) => UseItem(slot.Item);
                _ = itemButton.AddToParent(_inventoryPanel);
                slotIndex++;
            }
        }
    }

    private void UseItem(Item? item)
    {
        if (item == null)
        {
            return;
        }

        var command = new Command
        {
            Index = CommandIndexInventory.Use,
            Item = item,
            Target = _player.MainUnit,
            Type = ComponentTagEx.InventoryManager,
            Flag = CommandFlag.Queued
        };
        if (_player.MainUnit is null)
        {
            return;
        }

        var result = command.IssueOrder(_player.MainUnit);
        if (!result.IsSuccess)
        {
            Game.Logger.LogError("Failed to use item: {result}", result);
        }
        else
        {
            Game.Logger.LogWarning("Item used successfully");
        }
    }
}
#endif