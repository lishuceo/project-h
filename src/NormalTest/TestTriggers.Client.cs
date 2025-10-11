#if CLIENT
using Events;

using GameCore.AbilitySystem;
using GameCore.AbilitySystem.Manager;
using GameCore.BaseType;
using GameCore.Components;
using GameCore.Container;
using GameCore.Event;
using GameCore.OrderSystem;
using GameCore.SceneSystem;
using GameCore.TargetingSystem;

using GameData;
using GameData.Extension;

using GameUI.Brush;
using GameUI.Control;
using GameUI.Control.Enum;
using GameUI.Control.Extensions;
using GameUI.Control.Primitive;
using static GameUI.Control.Extensions.UI;
using GameUI.Device;
using GameUI.Enum;
using GameUI.TriggerEvent;

using System.Drawing;
using System.Numerics;

namespace GameEntry;
internal class TestTriggers : IGameClass
{
    private static void TestJoyStick()
    {
        // 创建手柄测试触发器（仅在手柄测试模式下调用）
        Trigger<EventGameStart> trigger = new(static async (s, d) =>
        {
            var normalJoystick = GameUI.Examples.JoystickExamples.CreateNormalJoystick();
            var floatJoystick = GameUI.Examples.JoystickExamples.CreateFloatJoystick();
            var dynamicJoystick = GameUI.Examples.JoystickExamples.CreateDynamicJoystick();
            var draw = DeviceInfo.PrimaryViewport.CreateDebugDraw();
            var scenePoint = new ScenePoint(new Vector3(3500, 3000, 0), Player.LocalPlayer.Scene);
            var groundHeight = scenePoint.GetGroundHeight();
            draw?.DrawCircle(scenePoint.Vector3 + new Vector3(0, 0, groundHeight + 10), new(), 100, Color.Red, true);
            Panel panelFullScreen = UI.Panel()
                .Stretch()  // 拉伸到全宽全高
                .GrowRatio(1, 1);  // 占满可用空间

            // Create a panel to hold the joysticks for layout
            Panel panel = new()
            {
                Width = 800,
                Height = 300,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                FlowOrientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalContentAlignment.Center,
                HorizontalContentAlignment = HorizontalContentAlignment.Center,
            };
            // Add joysticks to the panel
            _ = normalJoystick.AddToParent(panel);

            _ = floatJoystick.AddToParent(panel);
            _ = dynamicJoystick.AddToParent(panelFullScreen);

            // Add the panel to the visual tree
            _ = panelFullScreen.AddToVisualTree();
            _ = panel.AddToVisualTree();


            await Game.Delay(TimeSpan.FromSeconds(1));
            Game.Logger.LogInformation("Panel FullScreen position: {pos}, size:{size}", panelFullScreen.Position, panelFullScreen.ActualSize);
            Game.Logger.LogInformation("Normal joystick created: {joystick}, position: {pos}, size: {size}, visible: {}", normalJoystick, normalJoystick.ScreenPosition, normalJoystick.ActualSize, normalJoystick.IsActuallyVisible);
            Game.Logger.LogInformation("Floating joystick created: {joystick}, position: {pos}, size: {size}, visible: {}", floatJoystick, floatJoystick.ScreenPosition, floatJoystick.ActualSize, floatJoystick.IsActuallyVisible);
            Game.Logger.LogInformation("Dynamic joystick created: {joystick}, position: {pos}, size: {size}, visible: {}", dynamicJoystick, dynamicJoystick.ScreenPosition, dynamicJoystick.ActualSize, dynamicJoystick.IsActuallyVisible);
            Game.Logger.LogInformation("Joystick setup completed successfully.");
            normalJoystick.ValueChanged += Joystick_ValueChanged;
            normalJoystick.DragEnded += Joystick_DragEnded;
            floatJoystick.ValueChanged += Joystick_ValueChanged;
            floatJoystick.Deactivated += Joystick_DragEnded;
            dynamicJoystick.ValueChanged += Joystick_ValueChanged;
            dynamicJoystick.Deactivated += Joystick_DragEnded;
            return true;
        }, keepReference: true);
        trigger.Register(Game.Instance);
    }

    private static void Joystick_DragEnded(object? sender, EventArgs e)
    {
        Game.Logger.LogInformation("Joystick value reset to zero");
        Command command = new()
        {
            Index = CommandIndex.VectorMoveStop,
            Type = ComponentTag.Walkable
        };
        var mainUnit = Player.LocalPlayer.MainUnit;
        if (mainUnit is null)
        {
            Game.Logger.LogError("Main unit is null, cannot issue stop command");
            return;
        }
        _ = command.IssueOrder(mainUnit);
    }

    private static void Joystick_ValueChanged(object? sender, GameUI.Control.Advanced.JoystickValueChangedEventArgs e)
    {
        var vector = e.NewValue;
        if (vector.Length() > 0.1f)
        {
            var angle = Angle.FromVector2(vector);
            Game.Logger.LogInformation("Joystick value changed: {vector}", vector);
            Command command = new()
            {
                Index = CommandIndex.VectorMove,
                Target = angle,
                Type = ComponentTag.Walkable
            };
            var mainUnit = Player.LocalPlayer.MainUnit;
            if (mainUnit is null)
            {
                Game.Logger.LogError("Main unit is null, cannot issue move command");
                return;
            }
            _ = command.IssueOrder(mainUnit);
        }
    }

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += Game_OnGameTriggerInitialization;
        Game.OnGameTriggerInitialization += OnJoyStickTestModeCheck;
        Game.ForgetRunCaptureCallerStackTrace = true;
        EventAggregator.CaptureCallerStackTrace = true;
    }

    private static void OnJoyStickTestModeCheck()
    {
        // 只在手柄测试模式下注册摇杆测试
        if (Game.GameModeLink == ScopeData.GameMode.JoyStickTest)
        {
            TestJoyStick();
        }
    }

    private static TargetingIndicator? TargetingIndicator { get; set; }
    private static Button? TargetingButton { get; set; }

    private static TargetingIndicator? StartTargeting(AbilityExecute ability)
    {
        if (TargetingIndicator is not null)
        {
            TargetingIndicator.Destroy();
            TargetingIndicator = null;
            return null;
        }
        TargetingIndicator = TargetingIndicator.CreateFromAbility(ability);
        if (TargetingIndicator is null)
        {
            Game.Logger.LogError("Targeting indicator is null");
            return null;
        }
        Game.Logger.LogInformation("Targeting indicator: {targetingIndicator}", TargetingIndicator);
        DeviceInfo.PrimaryInputManager.OnPointerButtonMove += UpdateTargetingCursor;
        return TargetingIndicator;
    }

    private static void UpdateTargetingCursor(EventGamePointerButtonMove info)
    {
        if (TargetingIndicator is not null)
        {
            var pos = info.PointerPosition;

            if (pos.HasValue)
            {
                var result = DeviceInfo.PrimaryViewport.RaycastTerrainOrWorldPanelXY(pos.Value);
                if (!result.IsHit)
                {
                    return;
                }
                var scenePoint = new ScenePoint(result.Position, Player.LocalPlayer.Scene);
                TargetingIndicator.UpdateCursorTarget(scenePoint);
            }
        }
    }

    private static void StopTargeting()
    {
        if (TargetingIndicator is not null)
        {
            TargetingIndicator.Destroy();
            TargetingIndicator = null;
            DeviceInfo.PrimaryInputManager.OnPointerButtonMove -= UpdateTargetingCursor;
        }
    }

    private static void ToggleTargeting(AbilityExecute ability)
    {
        if (TargetingIndicator is not null)
        {
            StopTargeting();
        }
        else
        {
            _ = StartTargeting(ability);
        }
    }

    private static void Game_OnGameTriggerInitialization()
    {
        // 如果游戏模式不是默认模式，则不注册触发器
        if (Game.GameModeLink != GameCore.ScopeData.GameMode.Default)
        {
            return;
        }
        // MouseMoveTrigger.IsEnabled = false;
        Trigger<EventGameStart> trigger = new(static async (s, d) =>
        {
            Panel panel = new()
            {
                Width = 400,
                Height = 400,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                FlowOrientation = Orientation.Vertical,
                VerticalContentAlignment = VerticalContentAlignment.Top,
                HorizontalContentAlignment = HorizontalContentAlignment.Left,
            };
            Button button = new()
            {
                Width = 200,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(Color.AliceBlue),
                Children = [
                    new Label()
                    {
                        Text = "Hello World!",
                        TextColor = new SolidColorBrush(Color.Black),
                    }]
            };
            _ = button.AddToParent(panel);
            TargetingButton = button;
            // add a Input underneath the TargetingButton
            Input input = new()
            {
                Width = 200,
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(Color.BlanchedAlmond),
                TextColor = new SolidColorBrush(Color.Black),
                // Set routed events to all, except for mouse clicks
                RoutedEvents = RoutedEvents.All & ~RoutedEvents.PointerClicked & ~RoutedEvents.PointerDoubleClicked,
            };
            _ = input.AddToParent(panel);
            // TODO: Input需要拦截按键事件，避免触发游戏的按键事件。
            _ = panel.AddToVisualTree();
            Canvas canvas = new()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Color.LightGray),
            };
            _ = canvas.AddToVisualTree();
            var sample = new GameUI.Examples.DrawPathExample(canvas);
            // 绘制一些路径示例
            sample.DrawBasicShapes();
            sample.DrawComplexPath();
            sample.DrawBezierCurves();
            sample.DrawCompositePath();
            sample.DrawPathWithArcs();
            sample.DrawTransformedPath();


            input.OnPointerClicked += (s, d) =>
            {
                Game.Logger.LogInformation("input clicked: {button}, button {}", button, d.PointerButtons);
                return;
            };
            panel.OnPointerClicked += (s, d) =>
            {
                Game.Logger.LogInformation("Panel clicked: {panel}, button {}", panel, d.PointerButtons);
                return;
            };
            Trigger<EventGamePointerButtonDown> trigger = new(async (s, d) =>
            {
                Game.Logger.Log(LogLevel.Warning, "触发器鼠标按下事件: {button}", d.PointerButtons);
                await Task.CompletedTask;
                return true;
            }, keepReference: true);
            trigger.Register(Game.Instance);
            button.OnPointerClicked += (s, d) =>
            {
                Game.Logger.Log(LogLevel.Warning, "Button clicked");
                return;
            };
            button.OnPointerDoubleClicked += (s, d) =>
            {
                Game.Logger.Log(LogLevel.Warning, "Button double clicked");
                return;
            };
            button.OnPointerPressed += (s, d) =>
            {
                Game.Logger.Log(LogLevel.Warning, "Button pressed");
                return;
            };
            button.OnPointerReleased += (s, d) =>
            {
                Game.Logger.Log(LogLevel.Warning, "Button released");
                return;
            };
            button.OnPointerEntered += (s, d) =>
            {
                Game.Logger.Log(LogLevel.Warning, "Button entered");
                return;
            };
            button.OnPointerExited += (s, d) =>
            {
                Game.Logger.Log(LogLevel.Warning, "Button exited");
                return;
            };

            // 打日志的样例。
            Game.Logger.Log(LogLevel.Warning, "触发器游戏开始!");
            Game.Logger.Log(LogLevel.Warning, "ActorScopePersist Hash:{hash}", "$p_0tja.unit.ActorScope.root"u8.GetHashCode(true));
            Game.Logger.LogInformation("Player 0 is neutral: {isNeutral}", Player.DefaultPlayer.IsNeutral);
            // Player.DefaultPlayer是中立玩家，一般是玩家0
            // C#事件：主控变化，中立状态变化。
            Player.DefaultPlayer.OnMainUnitChanged += (unit) => Game.Logger.LogInformation("Player 0 main unit changed to {unit}", unit);
            Player.DefaultPlayer.OnNeutralChanged += DefaultPlayer_OnNeutralChanged;
            Game.Logger.LogInformation("Player 0 is neutral: {isNeutral}", Player.DefaultPlayer.IsNeutral);
            // C#事件：队伍变化。
            Player.DefaultPlayer.OnTeamChanged += (team) => Game.Logger.LogInformation("Player 0 team changed to {team}", team);
            var catalog = GameDataCategory<GameDataUnit>.Catalog;
            foreach (var item in catalog)
            {
                Game.Logger.LogInformation("Unit name {unitName} link: {isNeutral}, Hash{hashCode}, HashLocal{hashLocal}", item.Name, item.Link.FriendlyName, item.Link.HashCode, item.Link.HashCodeLocal);
            }

            Label errorLabel = new()
            {
                TextColor = new SolidColorBrush(Color.Red),
                FontSize = 20,
            };
            _ = errorLabel.AddToParent(UIRoot.Instance);

            // 添加测试功能提示
            Label testHintLabel = new()
            {
                Text = "按键测试: F1=性能测试 | F2=物品堆叠 | F3=调用测试 | F4=物品表现测试 | F6=伤害响应概率测试",
                FontSize = 14,
                TextColor = new SolidColorBrush(Color.LightBlue),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new(0, 0, 0, 50),
            };
            _ = testHintLabel.AddToParent(UIRoot.Instance);
            // 触发器事件：服务器返回的指令结果
            Trigger<EventGameCmdResultNotify> triggerCmdResult = new(async (s, d) =>
            {
                errorLabel.Text = d.CmdResult.ToString();
                await Task.CompletedTask;
                return true;
            }, keepReference: true);
            triggerCmdResult.Register(Game.Instance);

            // 只是用来消除警告
            await Task.CompletedTask;
            return true;
        }, keepReference: true);
        trigger.Register(Game.Instance);
        Trigger<EventPlayerMainUnitChanged> trigger1 = new(static async (s, d) =>
        {
            Game.Logger.Log(LogLevel.Warning, "{}触发器主单位改变!, 新单位{}", d.Player, d.Unit);
            // 触发器事件：主控单位变化可能发生在游戏开始前，为保证逻辑，等待到帧末。
            _ = await Game.Delay(TimeSpan.FromSeconds(0));
            Game.Logger.LogWarning("Creating actor {}", ScopeData.Actor.TestActorModelScript.HashCodeLocal);
            var actor = ScopeData.Actor.TestActorModelScript.Data?.CreateActor(null, false) ?? throw new InvalidOperationException("Failed to create actor {ScopeData.Actor.TestActorModel.Data}");
            Game.Logger.LogInformation("Actor created: {actor}", actor);
            // 获取玩家1
            var player1 = Player.GetById(1) ?? throw new InvalidOperationException("Failed to get player 1");
            Game.Logger.LogInformation("Player 1: {player1}", player1.Id);
            Game.Logger.LogInformation("Player 1: Team {team}", player1.Team);
            Game.Logger.LogInformation("Player 1: TeamId {team}", player1.Team.Id);
            // Player.LocalPlayer: 客户端本地玩家
            var localPlayer = Player.LocalPlayer;
            if (localPlayer != player1)
            {
                Game.Logger.LogWarning("Local player is not player 1");
            }
            Game.Logger.LogInformation("Local player: {localPlayer}[{Id}]", localPlayer, localPlayer?.Id);
            var mainUnit = localPlayer!.MainUnit;
            if (d.Unit != mainUnit)
            {
                throw new InvalidOperationException("Main unit changed event unit does not match main unit");
            }
            Game.Logger.LogInformation("Main unit: {mainUnit}", mainUnit);
            var mainUnitPlacedUnit = mainUnit?.PlacedUnit;
            Game.Logger.LogInformation("Main unit placed unit: {mainUnitPlacedId}", mainUnitPlacedUnit);
            var mainPos = mainUnit?.Position ?? throw new InvalidOperationException("Failed to get main unit position");
            Game.Logger.LogInformation("Main unit position: {mainPos}", mainPos);
            var testActor = ScopeData.Actor.LineCenterActor.Data?.CreateActor(null, false);
            testActor!.Position = mainPos;

            var draw = DeviceInfo.PrimaryViewport.CreateDebugDraw();
            // draw?.DrawCircle(mainPos.Vector3, new(), 800, Color.Red, false);
            // draw?.DrawCircle(mainPos.Vector3, new(), 256, Color.Red, false);
            draw?.DrawRectangle(mainPos.Vector3 + new Vector3(0, 0, 1), 128, 64, new(), Color.Red, false);
            // draw a circle around (3500, 3000), with radius 100
            var scenePoint = new ScenePoint(new Vector3(3500, 3000, 0), Player.LocalPlayer.Scene);
            var groundHeight = scenePoint.GetGroundHeight();
            draw?.DrawCircle(scenePoint.Vector3 + new Vector3(0, 0, groundHeight + 10), new(), 100, Color.Red, true);
            actor.Position = mainPos;
            Game.Logger.LogInformation("Actor position: {actorPos}", actor.Position);
            var abilityManager = mainUnit.GetComponent<AbilityManager>();
            if (abilityManager is null)
            {
                Game.Logger.LogError("Ability manager is null");
                return false;
            }
            var ability = abilityManager.GetOne<AbilityExecute>((ability) => ability.Cache == ScopeData.Ability.TestSpell.Data);
            if (ability is null)
            {
                Game.Logger.LogError("Ability is null");
                return false;
            }
            Game.Logger.LogInformation("Ability: {ability}", ability);
            if (TargetingButton is null)
            {
                Game.Logger.LogError("Targeting button is null");
                return false;
            }
            TargetingButton.OnPointerClicked += (s, d) =>
            {
                Game.Logger.LogInformation("Targeting button clicked");
                ToggleTargeting(ability);
            };
            Game.Logger.LogInformation("Client delay 1s start, current Tick: {}", Game.TickCurrentFrame);
            _ = await Game.Delay(TimeSpan.FromSeconds(1));
            Game.Logger.LogInformation("Client delay 1s end, current Tick: {}", Game.TickCurrentFrame);
            // 获得本地玩家的当前场景
            var scene = localPlayer.Scene;
            // 搜索主控周围1000范围，寻找物品，即服务端所创建的那个物品。
            var items = scene.SearchCircle(mainPos, 1000, (e) => e.GetComponent<Item>());
            if (items is null || !items.Any())
            {
                Game.Logger.LogError("Failed to find item with 1000 of main unit");
                var allUnits = scene.SearchCircle(mainPos, 1000);
                Game.Logger.LogInformation("All units in 1000 of main unit: {units}", allUnits?.Count() ?? 0);
                return true;
            }
            var item = items.First();
            Game.Logger.LogInformation("Item found: {item}", item);
            Game.Logger.LogInformation("Item placed id: {id}", item.GetProperty<int>(PropertyItem.PlacedId));
            var itemPlaced = scene.GetPlacedItem(3);
            Game.Logger.LogInformation("Item: {item}, PlacedItem: {itemPlaced}", item, itemPlaced);
            if (item != itemPlaced?.TriggerGetterInstance)
            {
                Game.Logger.LogError("Item and PlacedItem do not match, item: {item}, PlacedItem: {itemPlaced}", item, itemPlaced);
                return true;
            }
            // 获取物品属性加成的例子
            var mods = (item as ItemMod)!.GetModificationManager(ItemSlotType.Equip)!.Modifications;
            Game.Logger.LogInformation("Item mods: {mods}", mods.Count);
            foreach (var mod in mods)
            {
                Game.Logger.Log(LogLevel.Warning, "Mod: {mod}", mod);
            }

            // 构造指令，拾取指令，目标为找到的物品。
            Command command = new()
            {
                Index = CommandIndexInventory.PickUp,
                Target = item,
                Type = ComponentTagEx.InventoryManager,
                Flag = CommandFlag.Queued
            };

            var result = command.IssueOrder(mainUnit);
            if (!result.IsSuccess)
            {
                Game.Logger.LogError("Failed to issue pick up command: {result}", result);
                return true;
            }
           
            Game.Logger.LogWarning("Pick up command issued");
            Trigger<EventGameKeyDown> trigger3 = new(async (s, d) =>
            {
                if (d.Key != GameCore.Platform.SDL.VirtualKey.C)
                {
                    return false;
                }
                var unit2 = Player.LocalPlayer.Scene.GetPlacedUnit(2)?.TriggerGetterInstance;
                Game.Logger.LogWarning("Unit2 is alive: {unit} {alive}", unit2, unit2?.IsAlive);
                // _ = mainUnit.AnimateHighlight(Color.Red, Color.Blue, TimeSpan.FromSeconds(0.5), true, RepeatBehavior.Forever);
                _ = ScopeData.Actor.PreTargetingHighlight.Data?.CreateActor(mainUnit);
                Game.Logger.LogInformation("Key down: {key}", d.Key);
                _ = ScopeData.Actor.PreTargetingCircle.Data?.CreateActor(mainUnit, false);
                Game.Logger.LogInformation("Key down: {key}", d.Key);
                // 构造指令，丢弃指令，丢弃找到的物品。
                command = new()
                {
                    Index = CommandIndexInventory.Drop,
                    Item = item,
                    Type = ComponentTagEx.InventoryManager,
                    Flag = CommandFlag.Queued
                };
                var result = command.IssueOrder(mainUnit);
                if (!result.IsSuccess)
                {
                    Game.Logger.LogError("Failed to issue drop command: {result}", result);
                    return true;
                }
                Game.Logger.LogWarning("Drop command issued");
                _ = await Game.Delay(TimeSpan.FromSeconds(1));
                // 再次拾取物品，测试拾取指令的队列指令。
                command = new()
                {
                    Index = CommandIndexInventory.PickUp,
                    Target = item,
                    Type = ComponentTagEx.InventoryManager,
                    Flag = CommandFlag.Queued
                };
                result = command.IssueOrder(mainUnit);
                if (!result.IsSuccess)
                {
                    Game.Logger.LogError("Failed to issue pick up command again: {result}", result);
                    return true;
                }
                Game.Logger.LogWarning("Pick up command issued again");
                return true;
            });
            trigger3.Register(Game.Instance);
            return true;
        }, keepReference: true);
        var player1 = Player.GetById(1) ?? throw new InvalidOperationException("Failed to get player 1");
        trigger1.Register(player1);

        Trigger<EventItemSlotChange> trigger2 = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("{Item} slot changed from {slotOld} to {slotNew}", d.Item, d.SlotPrevious, d.Slot);
            await Task.CompletedTask;
            return true;
        }, keepReference: true);
        trigger2.Register(Game.Instance);
        Trigger<EventItemLevelChange> trigger3 = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("{Item} level changed from {levelOld} to {levelNew}", d.Item, d.LevelPrevious, d.Level);
            await Task.CompletedTask;
            return true;
        }, keepReference: true);
        trigger3.Register(Game.Instance);
        Trigger<EventItemQualityChange> trigger4 = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("{Item} quality changed from {qualityOld} to {qualityNew}", d.Item, d.QualityPrevious, d.Quality);
            await Task.CompletedTask;
            return true;
        }, keepReference: true);
        trigger4.Register(Game.Instance);
        Trigger<EventItemStackChange> trigger5 = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("{Item} stack changed from {stackOld} to {stackNew}", d.Item, d.StackPrevious, d.Stack);
            await Task.CompletedTask;
            return true;
        }, keepReference: true);
        trigger5.Register(Game.Instance);
        Trigger<EventSceneLoadStart> triggerLoadStart = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("Scene {scene} load start", d.Scene);
            await Task.CompletedTask;
            return true;
        }, keepReference: true);
        triggerLoadStart.Register(Game.Instance);
        Trigger<EventSceneLoadEnd> triggerLoadEnd = new(static async (s, d) =>
        {
            Game.Logger.LogInformation("Scene {scene} load end", d.Scene);
            await Task.CompletedTask;
            return true;
        }, keepReference: true);
        triggerLoadEnd.Register(Game.Instance);
    }

    private static void DefaultPlayer_OnNeutralChanged(bool obj)
    {
        Game.Logger.LogInformation("Player 0 neutral changed to {neutral}", obj);
    }
}
#endif