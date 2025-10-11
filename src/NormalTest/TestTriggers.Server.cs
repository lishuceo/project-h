#if SERVER
using Events;

using GameCore.Components;
using GameCore.Container;
using GameCore.Event;
using GameCore.Interface;
using GameCore.OrderSystem;
using GameCore.SceneSystem;
using GameCore.Shape;
using GameCore.VitalSystem;

using GameData.Extension;

using System.Diagnostics;
using System.Numerics;

using TriggerEncapsulation;

namespace GameEntry.NormalTest;

internal class TestTriggers : IGameClass
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public static void OnRegisterGameClass()
    {

        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.ForgetRunCaptureCallerStackTrace = true;
        EventAggregator.CaptureCallerStackTrace = true;
    }
    private static async Task<bool> GameStartAsync(object sender, EventGameStart eventArgs)
    {
        // 打日志的例子
        Game.Logger.Log(LogLevel.Warning, "Test Start!");
        Game.Logger.Log(LogLevel.Warning, "Scene string Hash C#: {Hash}", "default"u8.GetHashCode(true));
        // 获取场景的例子
        var scene = Scene.GetOrCreate(ScopeData.Scene.DefaultScene)!;
        Game.Logger.Log(LogLevel.Warning, "Scene Hash C#: {Hash}", scene.GetHashCode());
        Game.Logger.Log(LogLevel.Warning, "Scene Loaded State: {result}", scene.Loaded);
        RegionCircle regionCircle = new(new ScenePoint(3500, 3000, scene), radius: 100);
        Trigger<EventRegionEnter> triggerRegionEnter = new(async (s, d) =>
        {
            Game.Logger.Log(LogLevel.Warning, "RegionCircle: {regionCircle} enter, {unit}", d.Region, d.Unit);
            return true;
        }, true);
        triggerRegionEnter.Register(regionCircle);
        Trigger<EventRegionExit> triggerRegionExit = new(async (s, d) =>
        {
            Game.Logger.Log(LogLevel.Warning, "RegionCircle: {regionCircle} exit, {unit}", d.Region, d.Unit);
            return true;
        }, true);
        triggerRegionExit.Register(regionCircle);
        RegionRectangle regionRectangle = new(new ScenePoint(3500, 3000, scene), 200, 200);
        Trigger<EventRegionEnter> triggerRegionEnter2 = new(async (s, d) =>
        {
            Game.Logger.Log(LogLevel.Warning, "RegionRectangle: {regionRectangle} enter, {unit}", d.Region, d.Unit);
            return true;
        }, true);
        triggerRegionEnter2.Register(regionRectangle);
        Trigger<EventRegionExit> triggerRegionExit2 = new(async (s, d) =>
        {
            Game.Logger.Log(LogLevel.Warning, "RegionRectangle: {regionRectangle} exit, {unit}", d.Region, d.Unit);
            return true;
        }, true);
        triggerRegionExit2.Register(regionRectangle);

        // 地编物品的例子
        var placedItem = scene.GetPlacedItem(3);
        var item = placedItem?.TriggerGetterInstance!;
        Game.Logger.Log(LogLevel.Warning, "Placed Item #3: {placedItem}, {item}, PlacedId:{id}", placedItem, item, item.GetProperty<int>(PropertyItem.PlacedId));
        // 获取物品属性加成的例子
        var mods = (item as ItemMod)!.GetModificationManager(ItemSlotType.Equip)!.Modifications;
        foreach (var mod in mods)
        {
            Game.Logger.Log(LogLevel.Warning, "Mod: {mod}", mod);
        }
        // 获取玩家的例子
        var player1 = Player.GetById(1)!;
        Game.Logger.LogWarning("玩家1所在场景:{scene}", player1.Scene);
        // Game.Logger.LogWarning("玩家1跳转到默认场景:{result}", player1.SwitchScene(scene, true));
        Game.Logger.LogWarning("玩家1所在场景:{scene}", player1.Scene);
        var player3 = Player.GetById(3)!;
        // 获取地编单位的例子
        var unit = scene.GetPlacedUnit(1)?.TriggerGetterInstance!;
        Game.Logger.Log(LogLevel.Warning, "Player 1 Unit Created#: {unit} {name}, Position: {pos}", unit, unit.Cache.Name, unit.Position);
        // 设置玩家1的主控单位
        Game.Logger.Log(LogLevel.Warning, "Player 1 Main Unit: {unit}", player1.MainUnit);
        // Randomly create 20 ScopeData.Unit.HostTestHero units around ScenePoint (3000,3500,0) with a radius of 1000, belongs to player 3, randomly facing.
        //for (var i = 0; i < 20; i++)
        //{
        //    var newUnit = ScopeData.Unit.HostTestHero.Data?.CreateUnit(player3, new(new Vector3(3000 + IExecutionContext.Random.Next(-1000, 1000), 3500 + IExecutionContext.Random.Next(-1000, 1000), 0), scene), IExecutionContext.Random.Next(0, 360))!;
        //}
        // 动态创建一个触发器，事件为技能阶段。
        Trigger<EventEntityOrderStage> trigger = new(async (s, d) =>
        {
            Game.Logger.LogInformation("{unit} {Order} stage changed from {StagePrevious} {OrderStage} - target: {target}", d.Unit, d.Order, d.StagePrevious, d.Stage, d.Target);
            if (!d.Target.IsValid)
            {
                Game.Logger.LogInformation("{Target} is invalid", d.Target);
            }
            if (d.Order.Command.Index == CommandIndexInventory.PickUp)
            {
                var inventoryManager = d.Unit?.GetTagComponent<InventoryManager>(ComponentTagEx.InventoryManager);
                if (inventoryManager is not null)
                {
                    foreach (var inventory in inventoryManager.Inventories)
                    {
                        foreach (var item in inventory.Items)
                        {
                            Game.Logger.LogInformation("Inventory: {inventory}, Item: {item}", inventory, item);
                        }
                    }
                }
            }
            return true;
        });
        // 为触发器添加事件，注册刚创建的单位的技能阶段事件。
        trigger.Register(unit);
        // 事件注册对象填写游戏代表注册所有单位的这一事件。
        // trigger.AddEvent(Game.Instance);
        var unit2 = scene.GetPlacedUnit(2)?.TriggerGetterInstance!;

        Game.Logger.Log(LogLevel.Warning, "Player 3 Unit Created#: {unit}, Position: {pos}", unit2, unit2.Position);
        // 为单位设置血量模块
        var vital = unit2.GetTagComponent<Vital>(PropertyVital.Health)!;
        // 动态创建一个触发器，事件为单位受到伤害
        Trigger<EventEntityDamageTaken> damageTakenTrigger = new(async (s, d) =>
        {
            Game.Logger.LogInformation("{unit} Taken Damage {Damage}, Life {Life}", unit2, d.DamageInstance, vital);
            Game.Logger.Log(LogLevel.Warning, "GetProperty 1: {PROPERTY}", (unit2 as IPropertyHost).GetPropertyWithHash<double>(1));
            Game.Logger.Log(LogLevel.Warning, "GetProperty Life: {PROPERTY}", (unit2 as IPropertyHost).GetPropertyWithHash<double>(PropertyVital.Health.GetHashCode()));
            await Task.CompletedTask;
            return true;
        });
        // 注册单位3的受伤事件
        damageTakenTrigger.Register(unit2);
        // 将玩家3的主控设为单位3
        player3.MainUnit = unit2;
        // 创建测试技能并把它添加给玩家1的主控
        var spell = ScopeData.Ability.TestSpell.Data!.CreateAbility(unit);
        Game.Logger.Log(LogLevel.Warning, "Spell Created#: {spell}", spell);
        // 构造一个新的指令，由玩家1发出，施法完毕后会重复施法，命令单位对玩家3的主控单位使用TestSpell
        Command command = new()
        {
            AbilityLink = ScopeData.Ability.TestSpell,
            Target = unit2,
            Flag = CommandFlag.IsAI | CommandFlag.DoRecast,
            Player = player1
        };
        // IssueOrder，对玩家1的主控发布指令。
        Game.Logger.Log(LogLevel.Warning, "Issue order result: {result}", command.IssueOrder(unit));
        // await PerformanceTest(unit);
        Game.Logger.Log(LogLevel.Warning, "Server delay 1s start");
        _ = await Game.Delay(TimeSpan.FromSeconds(1));
        Game.Logger.Log(LogLevel.Warning, "Server delay 1s end");

        Game.Logger.Log(LogLevel.Warning, "GetProperty LifeMax 2: {PROPERTY}", unit2.GetUnitPropertyFinal(GameCore.ScopeData.UnitProperty.LifeMax));
        Game.Logger.Log(LogLevel.Warning, "GetProperty 2: {PROPERTY}", (unit2 as IPropertyHost).GetPropertyWithHash<double>(2));
        Game.Logger.Log(LogLevel.Warning, "GetProperty 1: {PROPERTY}", (unit2 as IPropertyHost).GetPropertyWithHash<double>(1));
        
        // 🧪 物品附属表现测试逻辑已移至F4按键触发，游戏开始时不自动运行
        Game.Logger.Log(LogLevel.Warning, "💡 提示：按下F4键可以手动触发物品附属表现测试");
        
        return true;
    }

    /// <summary>
    /// 测试物品附属表现行为
    /// 包括：物品自身的附属Actor表现 和 物品被持有/装备时的拥有者附属表现
    /// </summary>
    private static async Task TestItemActorBehavior(Scene scene, Player player, Unit unit)
    {
        Game.Logger.Log(LogLevel.Warning, "🧪 === 物品附属表现测试开始 ===");
        
        try
        {
            // 1. 创建测试物品并记录
            var testItemMod = await CreateTestItemWithActors(scene, player);
            
            if (testItemMod != null)
            {
                // 2. 测试物品拾取后的附属表现变化
                await TestItemPickupActorBehavior(testItemMod, player, unit);
                
                // 2.5. 将物品从普通物品栏移动到装备物品栏
                await TestMoveItemToEquipSlot(testItemMod, player, unit);
                
                // 3. 测试物品装备时的拥有者附属表现
                await TestItemEquipOwnerActors(scene, player, unit);
                
                // 4. 最后测试丢弃物品的附属表现恢复
                await TestDropItemActorBehavior(testItemMod, player, unit);
            }
            else
            {
                Game.Logger.LogWarning("⚠️ 无法创建测试物品，跳过后续测试");
            }
            
            Game.Logger.Log(LogLevel.Warning, "✅ === 物品附属表现测试完成 ===");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 物品附属表现测试失败");
        }
    }

    /// <summary>
    /// 创建带有附属表现的测试物品
    /// </summary>
    private static async Task<ItemMod?> CreateTestItemWithActors(Scene scene, Player player)
    {
        Game.Logger.Log(LogLevel.Warning, "🔍 测试1: 创建物品并观察自身附属Actor表现");
        
        // 在场景中创建一个带有附属表现的ItemMod物品
        var testPosition = new ScenePoint(new Vector3(3000, 3000, 0), scene);
        var testItemMod = ScopeData.Item.TestItemWithActors.Data?.CreateItem(testPosition, player) as ItemMod;
        
        if (testItemMod != null)
        {
            Game.Logger.Log(LogLevel.Warning, "📦 创建测试物品: {item} 位置: {pos}", testItemMod.Cache.Name, testItemMod.Position);
            
            // 注意：附属表现（Actor）仅存在于客户端中，服务端只能记录物品的基本信息
            var itemUnit = testItemMod.Unit;
            Game.Logger.Log(LogLevel.Warning, "🎭 物品单位信息: {unit} (附属表现仅在客户端可见)", itemUnit);
            Game.Logger.Log(LogLevel.Warning, "📋 物品单位状态: IsValid={valid}, HasState={hasState}", itemUnit.IsValid, itemUnit.HasState(UnitState.SuppressActor));
            
            await Game.Delay(TimeSpan.FromMilliseconds(500));
        }
        else
        {
            Game.Logger.LogWarning("⚠️ 无法创建测试物品");
        }
        
        return testItemMod;
    }

    /// <summary>
    /// 测试物品拾取后的附属表现变化
    /// </summary>
    private static async Task TestItemPickupActorBehavior(ItemMod itemMod, Player player, Unit unit)
    {
        // 等待5秒，有足够的时间观察物品的附属表现
        await Game.Delay(TimeSpan.FromMilliseconds(5000));
        Game.Logger.Log(LogLevel.Warning, "🔍 测试2: 物品拾取附属表现变化");
        
        if (itemMod != null)
        {
            Game.Logger.Log(LogLevel.Warning, "📦 使用新创建的测试物品进行拾取测试: {item}", itemMod.Cache.Name);
            
            // 记录拾取前的状态
            Game.Logger.Log(LogLevel.Warning, "📍 拾取前 - 物品位置: {pos}, Carrier: {carrier}", itemMod.Position, itemMod.Carrier);
            Game.Logger.Log(LogLevel.Warning, "📋 拾取前 - 物品单位状态: SuppressActor={suppressActor}", itemMod.Unit.HasState(UnitState.SuppressActor));
            
            // 创建拾取指令
            Command pickupCommand = new()
            {
                Index = CommandIndexInventory.PickUp,
                Type = ComponentTagEx.InventoryManager,
                Target = itemMod,
                Player = player,
            };
            
            // 执行拾取
            var pickupResult = pickupCommand.IssueOrder(unit);
            Game.Logger.Log(LogLevel.Warning, "📥 拾取指令结果: {result}", pickupResult);
            
            await Game.Delay(TimeSpan.FromMilliseconds(3000));
            
            // 记录拾取后的状态
            Game.Logger.Log(LogLevel.Warning, "📍 拾取后 - 物品位置: {pos}, Carrier: {carrier}", itemMod.Position, itemMod.Carrier);
            Game.Logger.Log(LogLevel.Warning, "🎒 拾取后 - 物品槽位: {slot}", itemMod.Slot);
            Game.Logger.Log(LogLevel.Warning, "📋 拾取后 - 物品单位状态: SuppressActor={suppressActor}", itemMod.Unit.HasState(UnitState.SuppressActor));
            
            // 拾取测试完成，不在这里丢弃物品
        }
        else
        {
            Game.Logger.LogWarning("⚠️ 传入的测试物品为空，无法进行拾取测试");
        }
    }

    /// <summary>
    /// 将物品从普通物品栏移动到装备物品栏槽位
    /// </summary>
    private static async Task TestMoveItemToEquipSlot(ItemMod itemMod, Player player, Unit unit)
    {
        Game.Logger.Log(LogLevel.Warning, "🔄 测试2.5: 将物品移动到装备槽位");
        
        // 获取物品栏管理器
        var inventoryManager = unit.GetTagComponent<InventoryManager>(ComponentTagEx.InventoryManager);
        if (inventoryManager == null)
        {
            Game.Logger.LogWarning("⚠️ 单位没有物品栏管理器");
            return;
        }
        
        // 查找装备物品栏（TestInventory6Equip）
        Inventory? equipInventory = null;
        foreach (var inventory in inventoryManager.Inventories)
        {
            if (inventory.Cache.Link == ScopeData.Inventory.TestInventory6Equip)
            {
                equipInventory = inventory;
                break;
            }
        }
        
        if (equipInventory == null)
        {
            Game.Logger.LogWarning("⚠️ 找不到装备物品栏TestInventory6Equip");
            return;
        }
        
        // 获取装备物品栏的第一个槽位
        var equipSlot = equipInventory.Slots.FirstOrDefault();
        if (equipSlot == null)
        {
            Game.Logger.LogWarning("⚠️ 装备物品栏没有可用槽位");
            return;
        }
        
        Game.Logger.Log(LogLevel.Warning, "📋 移动前 - 物品槽位: {slot}, 槽位类型: {slotType}", itemMod.Slot, itemMod.SlotType);
        Game.Logger.Log(LogLevel.Warning, "📋 移动前 - 物品启用状态: {enabled}", itemMod.IsEnabled);
        
        // 使用Swap指令将物品移动到装备槽位
        Command swapCommand = new()
        {
            Index = CommandIndexInventory.Swap,
            Type = ComponentTagEx.InventoryManager,
            Target = equipSlot,
            Item = itemMod,
            Player = player
        };
        
        var swapResult = swapCommand.IssueOrder(unit);
        Game.Logger.Log(LogLevel.Warning, "🔄 移动到装备槽指令结果: {result}", swapResult);
        
        await Game.Delay(TimeSpan.FromMilliseconds(1000));
        
        // 记录移动后的状态
        Game.Logger.Log(LogLevel.Warning, "📋 移动后 - 物品槽位: {slot}, 槽位类型: {slotType}", itemMod.Slot, itemMod.SlotType);
        Game.Logger.Log(LogLevel.Warning, "📋 移动后 - 物品启用状态: {enabled}", itemMod.IsEnabled);
        Game.Logger.Log(LogLevel.Warning, "🎭 移动后 - ActiveModificationManager: {manager}", itemMod.ActiveModificationManager);
    }

    /// <summary>
    /// 测试物品装备时的拥有者附属表现
    /// </summary>
    private static async Task TestItemEquipOwnerActors(Scene scene, Player player, Unit unit)
    {
        Game.Logger.Log(LogLevel.Warning, "🔍 测试3: 物品装备拥有者附属表现");
        
        // 获取单位的物品栏管理器
        var inventoryManager = unit.GetTagComponent<InventoryManager>(ComponentTagEx.InventoryManager);
        if (inventoryManager == null)
        {
            Game.Logger.LogWarning("⚠️ 单位没有物品栏管理器");
            return;
        }
        
        // 查找物品栏中的ItemMod
        ItemMod? equippableItem = null;
        foreach (var inventory in inventoryManager.Inventories)
        {
            foreach (var item in inventory.Items)
            {
                if (item is ItemMod itemMod && itemMod.SlotType == ItemSlotType.Equip)
                {
                    equippableItem = itemMod;
                    break;
                }
            }
            if (equippableItem != null) break;
        }
        
        if (equippableItem != null)
        {
            Game.Logger.Log(LogLevel.Warning, "⚔️ 找到可装备物品: {item}", equippableItem.Cache.Name);
            
            // 记录装备前的状态
            Game.Logger.Log(LogLevel.Warning, "📊 装备前 - 物品启用状态: {enabled}", equippableItem.IsEnabled);
            Game.Logger.Log(LogLevel.Warning, "🎭 装备前 - 物品ActiveModificationManager: {manager}", equippableItem.ActiveModificationManager);
            Game.Logger.Log(LogLevel.Warning, "📋 装备前 - 物品槽位类型: {slotType}", equippableItem.SlotType);
            
            await Game.Delay(TimeSpan.FromMilliseconds(1000));
            
            // 记录装备后的状态（如果物品满足装备条件会自动启用）
            Game.Logger.Log(LogLevel.Warning, "📊 装备后 - 物品启用状态: {enabled}", equippableItem.IsEnabled);
            Game.Logger.Log(LogLevel.Warning, "🎭 装备后 - 物品ActiveModificationManager: {manager}", equippableItem.ActiveModificationManager);
            Game.Logger.Log(LogLevel.Warning, "🔧 装备后 - 物品修饰管理器状态: Applied={applied}", equippableItem.ActiveModificationManager?.IsStateApplied ?? false);
            
            // 检查物品的主动技能
            if (equippableItem.ActiveAbility != null)
            {
                Game.Logger.Log(LogLevel.Warning, "⚡ 装备后 - 激活技能: {ability}", equippableItem.ActiveAbility.Cache.Name);
            }
            
            // 注意：HostedActors和ClientActors仅在客户端存在，服务端只能观察逻辑状态
        }
        else
        {
            Game.Logger.LogWarning("⚠️ 物品栏中没有找到可装备的ItemMod物品");
        }
    }

    /// <summary>
    /// 运行物品附属表现测试（通过按键触发）
    /// </summary>
    private static async Task RunItemActorBehaviorTest(Player player)
    {
        try
        {
            var scene = player.Scene;
            var mainUnit = player.MainUnit;
            
            if (scene == null || mainUnit == null)
            {
                Game.Logger.LogWarning("⚠️ 玩家场景或主控单位不存在，无法进行测试");
                return;
            }
            
            Game.Logger.Log(LogLevel.Warning, "🧪 === 手动触发物品附属表现测试 ===");
            await TestItemActorBehavior(scene, player, mainUnit);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 手动物品附属表现测试失败");
        }
    }

    /// <summary>
    /// 测试丢弃物品后的附属表现恢复
    /// </summary>
    private static async Task TestDropItemActorBehavior(ItemMod itemMod, Player player, Unit unit)
    {
        Game.Logger.Log(LogLevel.Warning, "🔍 测试4: 丢弃物品附属表现恢复");
        
        // 等待一段时间让用户观察装备效果
        Game.Logger.Log(LogLevel.Warning, "⏱️ 等待5秒以观察装备效果...");
        await Game.Delay(TimeSpan.FromMilliseconds(5000));
        
        if (itemMod != null && itemMod.IsValid)
        {
            // 记录丢弃前的状态
            Game.Logger.Log(LogLevel.Warning, "📍 丢弃前 - 物品位置: {pos}, Carrier: {carrier}", itemMod.Position, itemMod.Carrier);
            Game.Logger.Log(LogLevel.Warning, "🎒 丢弃前 - 物品槽位: {slot}, 槽位类型: {slotType}", itemMod.Slot, itemMod.SlotType);
            Game.Logger.Log(LogLevel.Warning, "📋 丢弃前 - 物品单位状态: SuppressActor={suppressActor}", itemMod.Unit.HasState(UnitState.SuppressActor));
            
            // 测试丢弃物品
            Game.Logger.Log(LogLevel.Warning, "🔄 开始测试物品丢弃...");
            Command dropCommand = new()
            {
                Index = CommandIndexInventory.Drop,
                Type = ComponentTagEx.InventoryManager,
                Item = itemMod,
                Player = player,
            };
            
            var dropResult = dropCommand.IssueOrder(unit);
            Game.Logger.Log(LogLevel.Warning, "📤 丢弃指令结果: {result}", dropResult);
            
            await Game.Delay(TimeSpan.FromMilliseconds(1000));
            
            // 记录丢弃后的状态
            Game.Logger.Log(LogLevel.Warning, "📍 丢弃后 - 物品位置: {pos}, Carrier: {carrier}", itemMod.Position, itemMod.Carrier);
            Game.Logger.Log(LogLevel.Warning, "🎒 丢弃后 - 物品槽位: {slot}", itemMod.Slot);
            Game.Logger.Log(LogLevel.Warning, "📋 丢弃后 - 物品单位状态: SuppressActor={suppressActor}", itemMod.Unit.HasState(UnitState.SuppressActor));
        }
        else
        {
            Game.Logger.LogWarning("⚠️ 物品无效，无法进行丢弃测试");
        }
    }

    private static async Task PerformanceTest(Unit unit)
    {
        Game.Logger.Log(LogLevel.Warning, "Performance Test Start!");
        Game.Logger.Log(LogLevel.Warning, "GetProperty a null value as double? {unitId}", unit.GetProperty<double>(PropertyUnitEx.CustomNumber));
        // Game.Logger.Log(LogLevel.Warning, "GetProperty a null value as byte[]? {unitId}", unit.GetProperty<byte[]>(PropertyUnit.UnitId));
        Stopwatch stopwatch = new();
        _ = await Game.Delay(TimeSpan.FromSeconds(1));
        unit.SetProperty(PropertyUnitEx.CustomNumber, unit.GetProperty<double>(PropertyUnitEx.CustomNumber));
        stopwatch.Start();
        for (var i = 0; i < 100000; i++)
        {
            unit.SetProperty(PropertyUnitEx.CustomNumber, unit.GetProperty<double>(PropertyUnitEx.CustomNumber) + 1.0d);
        }
        stopwatch.Stop();
        Game.Logger.Log(LogLevel.Warning, "Get then Set Property 100000 times: {time}ms, result {result}", stopwatch.Elapsed.TotalMilliseconds, unit.GetProperty<double>(PropertyUnitEx.CustomNumber));
        _ = await Game.Delay(TimeSpan.FromSeconds(1));
        stopwatch.Restart();
        for (var i = 0; i < 100000; i++)
        {
            unit.AddProperty(PropertyUnitEx.CustomNumber, 1d);
        }
        stopwatch.Stop();
        Game.Logger.Log(LogLevel.Warning, "Add Property 100000 times: {time}ms, result {result}", stopwatch.Elapsed.TotalMilliseconds, unit.GetProperty<double>(PropertyUnitEx.CustomNumber));
        Game.Logger.Log(LogLevel.Warning, "Performance Test End!");
    }

    private static void OnGameTriggerInitialization()
    {
        // 如果游戏模式不是默认模式或手柄测试模式，则不注册触发器
        if (Game.GameModeLink != GameCore.ScopeData.GameMode.Default
        && Game.GameModeLink != ScopeData.GameMode.JoyStickTest)
        {
            return;
        }
        Trigger<EventPlayerKeyDown> callTest = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("{Player} KeyDown {key}", d.Player, d.Key);
            if (d.Key == GameCore.Platform.SDL.VirtualKey.F3)
            {
                Game.Logger.LogInformation("F3 pressed, Run call test");
                await Game.InvokeTest(100_0000);
                return true;
            }
            // 🧪 F4 键触发物品附属表现测试
            if (d.Key == GameCore.Platform.SDL.VirtualKey.F4)
            {
                Game.Logger.LogInformation("F4 pressed, Run item actor behavior test");
                await RunItemActorBehaviorTest(d.Player);
                return true;
            }
            return false;
        }, true);
        callTest.Register(Game.Instance);
        // 在这里构造所有默认的触发器
        // 构造一个触发器，事件为游戏开始
        Trigger<EventGameStart> trigger = new(GameStartAsync);
        trigger.Register(Game.Instance);
        // 物品槽位变化事件
        Trigger<EventItemSlotChange> trigger2 = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("{Item} slot changed from {slotOld} to {slotNew}", d.Item, d.SlotPrevious, d.Slot);
            await Task.CompletedTask;
            return true;
        }, true);
        trigger2.Register(Game.Instance);
        Trigger<EventItemLevelChange> trigger3 = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("{Item} level changed from {levelOld} to {levelNew}", d.Item, d.LevelPrevious, d.Level);
            await Task.CompletedTask;
            return true;
        });
        trigger3.Register(Game.Instance);
        Trigger<EventItemQualityChange> trigger4 = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("{Item} quality changed from {qualityOld} to {qualityNew}", d.Item, d.QualityPrevious, d.Quality);
            await Task.CompletedTask;
            return true;
        }, true);
        trigger4.Register(Game.Instance);
        Trigger<EventSceneLoadStart> triggerLoadStart = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("Scene {scene} load start", d.Scene);
            await Task.CompletedTask;
            return true;
        });
        triggerLoadStart.Register(Game.Instance);
        Trigger<EventSceneLoadEnd> triggerLoadEnd = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("Scene {scene} load end", d.Scene);
            await Task.CompletedTask;
            return true;
        }, true);
        triggerLoadEnd.Register(Game.Instance);
        Trigger<EventPlayerKeyDown> trigger1 = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("{Player} KeyDown {key}", d.Player, d.Key);
            if (d.Key == GameCore.Platform.SDL.VirtualKey.F1)
            {
                Game.Logger.LogInformation("F1 pressed, Run performance test");
                var unit = d.Player.MainUnit;
                if (unit is not null)
                {
                    await PerformanceTest(unit);
                }
                return true;
            }
            return false;
        }, true);
        trigger1.Register(Game.Instance);
        Trigger<EventPlayerKeyDown> trigger5 = new(static async (s, d) =>
        {
            if (d.Key != GameCore.Platform.SDL.VirtualKey.F2)
            {
                return false;
            }
            Game.Logger.LogInformation("{Player} KeyDown {key}", d.Player, d.Key);
            var item = ScopeData.Scene.DefaultScene.Data?.GetPlacedItem(3)?.TriggerGetterInstance;
            if (item is not ItemMod itemMod)
            {
                Game.Logger.LogInformation("Item is null");
                return false;
            }
            itemMod.Stack += 1;
            Game.Logger.LogWarning("Item Stack: {stack}, {stack2}", itemMod.Stack, item.GetProperty<uint>(PropertyItem.Stack));
            _ = await Game.Delay(TimeSpan.FromSeconds(1));
            itemMod.Stack += 1;
            Game.Logger.LogWarning("Item Stack: {stack}, {stack2}", itemMod.Stack, item.GetProperty<uint>(PropertyItem.Stack));
            _ = await Game.Delay(TimeSpan.FromSeconds(1));
            itemMod.Stack += 1;
            Game.Logger.LogWarning("Item Stack: {stack}, {stack2}", itemMod.Stack, item.GetProperty<uint>(PropertyItem.Stack));
            return true;
        }, true);
        trigger5.Register(Game.Instance);
        
        // JoyStickTest模式专用：为玩家1的主控单位添加移动速度Buff的测试功能
        Trigger<EventPlayerKeyDown> buffTestTrigger = new(static async (s, d) =>
        {
            // 只在JoyStickTest模式下响应
            if (Game.GameModeLink != ScopeData.GameMode.JoyStickTest)
            {
                return false;
            }
            
            // 检查按键是否是B键
            if (d.Key != GameCore.Platform.SDL.VirtualKey.B)
            {
                return false;
            }
            
            Game.Logger.LogInformation("{Player} 按下B键，开始移动速度Buff测试", d.Player);
            
            // 获取玩家1
            var player1 = Player.GetById(1);
            if (player1?.MainUnit == null)
            {
                Game.Logger.LogWarning("玩家1或其主控单位未找到");
                return false;
            }
            
            var unit = player1.MainUnit;
            Game.Logger.LogInformation("目标单位: {unit}", unit.Cache.Name);
            
            // 获取并输出当前移动速度
            var currentSpeed = unit.GetUnitPropertyFinal(GameCore.ScopeData.UnitProperty.MoveSpeed);
            Game.Logger.LogInformation("🏃 当前移动速度: {speed}", currentSpeed);
            
            try
            {
                // 使用TriggerEncapsulation便利函数添加移动速度Buff
                var buff = unit.AddBuff(
                    ScopeData.Buff.SpeedDebuff,  // 使用预定义的速度Debuff
                    caster: unit,
                    stack: 1,
                    duration: TimeSpan.FromSeconds(5)
                );
                
                if (buff != null)
                {
                    Game.Logger.LogInformation("✅ 成功为单位 {unit} 添加-150移动速度Buff，持续5秒", unit.Cache.Name);
                    
                    // 获取并输出Buff附加后的移动速度
                    var newSpeed = unit.GetUnitPropertyFinal(GameCore.ScopeData.UnitProperty.MoveSpeed);
                    Game.Logger.LogInformation("🏃 Buff附加后移动速度: {speed}", newSpeed);
                    
                    // 注册Buff移除事件监听器，在Buff消失后输出移动速度
                    Trigger<GameCore.Event.EventBuffRemoved> buffRemovedTrigger = new(async (sender, eventArgs) =>
                    {
                        if (eventArgs.Buff == buff)
                        {
                            // Buff消失后获取并输出移动速度
                            var finalSpeed = unit.GetUnitPropertyFinal(GameCore.ScopeData.UnitProperty.MoveSpeed);
                            Game.Logger.LogInformation("🏃 Buff消失后移动速度: {speed}", finalSpeed);
                            await Task.CompletedTask;
                            return true;
                        }
                        return false;
                    }, keepReference: true);
                    
                    buffRemovedTrigger.Register(Game.Instance);
                }
                else
                {
                    Game.Logger.LogWarning("❌ 添加Buff失败");
                }
            }
            catch (Exception ex)
            {
                Game.Logger.LogError(ex, "添加移动速度Buff时发生错误");
            }
            
            return true;
        }, true);
        
        buffTestTrigger.Register(Game.Instance);
        
        // 伤害响应概率测试（按F6键触发）
        Trigger<EventPlayerKeyDown> damageResponseProbabilityTestTrigger = new(static async (s, d) =>
        {
            // 只在默认模式下响应F6键
            if (Game.GameModeLink != GameCore.ScopeData.GameMode.Default || d.Key != GameCore.Platform.SDL.VirtualKey.F6)
            {
                return false;
            }
            
            Game.Logger.LogInformation("{Player} 按下F6键，开始伤害响应概率测试", d.Player);
            var rnd = new Random();
            var successfulTriggers = 0;
            for(var i = 0; i < 1000; i++){
                var randomValue = rnd.NextDouble();
                if(randomValue < 0.3){
                    successfulTriggers++;
                }
            }
            Game.Logger.LogInformation("随机数结果: {result}", successfulTriggers/1000.0);
            await RunDamageResponseProbabilityTest(d.Player);
            return true;
        }, true);
        
        damageResponseProbabilityTestTrigger.Register(Game.Instance);
    }

    /// <summary>
    /// 运行伤害响应概率测试
    /// 测试GameDataResponseDamage的Chance字段是否按预期概率工作
    /// </summary>
    private static async Task RunDamageResponseProbabilityTest(Player player)
    {
        try
        {
            Game.Logger.Log(LogLevel.Warning, "🧪 === 伤害响应概率测试开始 ===");
            
            var scene = player.Scene;
            var mainUnit = player.MainUnit;
            
            if (scene == null || mainUnit == null)
            {
                Game.Logger.LogWarning("⚠️ 玩家场景或主控单位不存在，无法进行测试");
                return;
            }
            
            // 测试参数
            const double expectedProbability = 0.3; // 30%的触发概率
            const int testIterations = 100; // 100次测试
            const double toleranceRange = 0.1; // 10%的容差范围
            
            // 创建测试用的伤害响应数据
            var testResponseLink = new GameLink<GameCore.Behavior.GameDataResponse, GameCore.Behavior.GameDataResponseDamage>("TestChanceResponse");
            var testDamageResponseData = new GameCore.Behavior.GameDataResponseDamage(testResponseLink)
            {
                Modification = (_, _) => 50, // 每次触发增加50点伤害
                Chance = (_, _) => new GameCore.BaseType.Probability(expectedProbability) // 设置30%的触发概率
            };
            
            // 创建测试目标单位
            var testTargetUnit = await CreateTestTargetUnit(scene, player);
            if (testTargetUnit == null)
            {
                Game.Logger.LogWarning("⚠️ 无法创建测试目标单位");
                return;
            }
            
            // 为目标单位添加伤害响应
            var damageResponse = testDamageResponseData.CreateResponse(testTargetUnit, testTargetUnit);
            
            Game.Logger.Log(LogLevel.Warning, "🎯 测试目标: {unit} (生命值: {health})", testTargetUnit.Cache.Name, testTargetUnit.GetTagComponent<GameCore.VitalSystem.Vital>(PropertyVital.Health)?.Current);
            Game.Logger.Log(LogLevel.Warning, "📊 测试参数: 预期概率={probability:P0}, 测试次数={iterations}, 容差范围=±{tolerance:P0}", expectedProbability, testIterations, toleranceRange);
            
            // 创建伤害效果用于测试
            var testDamageLink = new GameLink<GameCore.Execution.Data.GameDataEffect, GameCore.Execution.Data.GameDataEffectDamage>("TestDamageEffect");
            var testDamageEffect = new GameCore.Execution.Data.GameDataEffectDamage(testDamageLink)
            {
                Amount = (_) => 100, // 基础伤害100
                Name = "Test Damage for Probability"
            };
            
            int successfulTriggers = 0;
            var originalHealth = testTargetUnit.GetTagComponent<GameCore.VitalSystem.Vital>(PropertyVital.Health)?.Current ?? 0;
            
            Game.Logger.Log(LogLevel.Warning, "🔄 开始执行 {iterations} 次伤害测试...", testIterations);
            
            // 执行多次伤害测试
            for (int i = 0; i < testIterations; i++)
            {
                // 重置目标单位生命值
                var healthVital = testTargetUnit.GetTagComponent<GameCore.VitalSystem.Vital>(PropertyVital.Health);
                if (healthVital != null)
                {
                    healthVital.Set(10000, GameCore.VitalSystem.Vital.Property.Current); // 设置足够高的生命值
                }
                
                // 记录伤害前的生命值
                var healthBefore = healthVital?.Current ?? 0;
                
                // 创建并执行伤害效果
                var damageEffectInstance = mainUnit.FactoryCreateChild(testDamageLink, testTargetUnit);
                if (damageEffectInstance != null)
                {
                    _ = damageEffectInstance.ResolveTarget();
                    var result = damageEffectInstance.Execute();
                    
                    if (result.IsSuccess)
                    {
                        // 检查是否触发了响应（通过检查额外伤害来判断）
                        var healthAfter = healthVital?.Current ?? 0;
                        var actualDamage = healthBefore - healthAfter;
                        
                        if (actualDamage > 100) // 如果伤害超过基础值100，说明响应被触发了
                        {
                            successfulTriggers++;
                        }
                        
                        // 每10次输出一次进度
                        if ((i + 1) % 10 == 0)
                        {
                            double currentRate = (double)successfulTriggers / (i + 1);
                            Game.Logger.LogInformation("📈 进度: {current}/{total} - 当前触发率: {rate:P1}", i + 1, testIterations, currentRate);
                        }
                    }
                }
                
                // 短暂延迟避免过于频繁的操作
                if (i % 20 == 0)
                {
                    await Game.Delay(TimeSpan.FromMilliseconds(50));
                }
            }
            
            // 计算并验证结果
            double actualTriggerRate = (double)successfulTriggers / testIterations;
            double lowerBound = expectedProbability - toleranceRange;
            double upperBound = expectedProbability + toleranceRange;
            
            bool testPassed = actualTriggerRate >= lowerBound && actualTriggerRate <= upperBound;
            
            // 输出测试结果
            Game.Logger.Log(testPassed ? LogLevel.Warning : LogLevel.Error, 
                "📊 === 伤害响应概率测试结果 ===");
            Game.Logger.Log(testPassed ? LogLevel.Warning : LogLevel.Error, 
                "🎯 预期概率: {expected:P1}", expectedProbability);
            Game.Logger.Log(testPassed ? LogLevel.Warning : LogLevel.Error, 
                "📈 实际触发率: {actual:P1}", actualTriggerRate);
            Game.Logger.Log(testPassed ? LogLevel.Warning : LogLevel.Error, 
                "🔢 成功触发次数: {triggers}/{total}", successfulTriggers, testIterations);
            Game.Logger.Log(testPassed ? LogLevel.Warning : LogLevel.Error, 
                "📏 容差范围: [{lower:P1}, {upper:P1}]", lowerBound, upperBound);
            Game.Logger.Log(testPassed ? LogLevel.Warning : LogLevel.Error, 
                "✅ 测试结果: {result}", testPassed ? "通过 ✅" : "失败 ❌");
            
            if (!testPassed)
            {
                Game.Logger.LogError("❌ 触发率 {actual:P1} 不在预期范围 [{lower:P1}, {upper:P1}] 内！", 
                    actualTriggerRate, lowerBound, upperBound);
            }
            
            // 清理测试单位
            testTargetUnit.Destroy();
            
            Game.Logger.Log(LogLevel.Warning, "🧪 === 伤害响应概率测试完成 ===");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 伤害响应概率测试失败");
        }
    }
    
    /// <summary>
    /// 创建用于测试的目标单位
    /// </summary>
    private static async Task<GameCore.EntitySystem.Unit?> CreateTestTargetUnit(GameCore.SceneSystem.Scene scene, Player player)
    {
        try
        {
            // 在主控单位附近创建测试目标
            var mainUnit = player.MainUnit;
            if (mainUnit == null) return null;
            
            var testPosition = new GameCore.SceneSystem.ScenePoint(
                mainUnit.Position.Vector3 + new Vector3(200, 0, 0), // 在主控右侧200单位处
                scene
            );
            
            // 使用默认的测试单位数据创建目标单位
            var testUnit = ScopeData.Unit.HostTestHero.Data?.CreateUnit(
                Player.GetById(3) ?? player, // 使用玩家3或当前玩家
                testPosition,
                0 // 朝向
            );
            
            if (testUnit != null)
            {
                // 设置足够的生命值用于测试
                var healthComponent = testUnit.GetTagComponent<GameCore.VitalSystem.Vital>(PropertyVital.Health);
                if (healthComponent != null)
                {
                    testUnit.GetComponent<GameCore.Components.UnitPropertyComplex>()?.SetFixed(
                        GameCore.ScopeData.UnitProperty.LifeMax, 
                        GameCore.BaseType.PropertySubType.Base, 
                        10000
                    );
                    healthComponent.Set(10000, GameCore.VitalSystem.Vital.Property.Current);
                }
                
                Game.Logger.LogInformation("🎯 创建测试目标单位: {unit} 位置: {pos}", testUnit.Cache.Name, testUnit.Position);
            }
            
            await Task.CompletedTask;
            return testUnit;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "创建测试目标单位失败");
            return null;
        }
    }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}

internal class EventEntityEnterRegion
{
}
#endif