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
using GameCore.VitalSystem;

using GameData;
using GameData.Extension;

using GameUI.Brush;
using GameUI.Control;
using GameUI.Control.Enum;
using GameUI.Control.Primitive;
using GameUI.Device;
using GameUI.Enum;
using GameUI.TriggerEvent;

using System.Drawing;
using System.Numerics;

namespace GameEntry.VampireSurvivors3D;

internal class TestTriggers : IGameClass
{
    private static Label? healthLabel;
    private static Label? killCountLabel;
    private static Label? timeLabel;
    private static Label? statusLabel;
    private static Panel? gameUI;

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.ForgetRunCaptureCallerStackTrace = true;
        EventAggregator.CaptureCallerStackTrace = true;
    }

    private static void OnGameTriggerInitialization()
    {
        // 如果游戏模式不是3D吸血鬼幸存者，则不进行初始化
        if (Game.GameModeLink != ScopeData.GameMode.VampireSurvivors3D)
        {
            return;
        }
        // 游戏开始触发器
        Trigger<EventGameStart> trigger = new(VampireSurvivors3DGameStartAsync, keepReference: true);
        trigger.Register(Game.Instance);

        // 主单位变化触发器
        Trigger<EventPlayerMainUnitChanged> mainUnitTrigger = new(OnMainUnitChangedAsync, keepReference: true);
        var player1 = Player.GetById(1)!;
        mainUnitTrigger.Register(player1);

        Game.Logger.Log(LogLevel.Warning, "🧛 Vampire Survivors 3D Client Triggers Initialized");
    }

    private static async Task<bool> VampireSurvivors3DGameStartAsync(object sender, EventGameStart eventArgs)
    {
        Game.Logger.Log(LogLevel.Warning, "🧛 Vampire Survivors 3D Client Game Start!");

        // 创建游戏UI
        // CreateGameUI();
        EnhancedUI.CreateEnhancedGameUI();

        // 设置输入处理
        SetupInputHandlers();

        await Task.CompletedTask;
        return true;
    }

    private static void CreateGameUI()
    {
        // 创建主UI面板
        gameUI = new Panel()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            WidthStretchRatio = 1.0f,
            HeightStretchRatio = 1.0f,
        };

        // 血量显示
        healthLabel = new Label()
        {
            Text = "Health: ---/---",
            FontSize = 20,
            TextColor = new SolidColorBrush(Color.Red),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new(10, 10, 0, 0),
            Parent = gameUI
        };

        // 击杀数显示
        killCountLabel = new Label()
        {
            Text = "Kills: 0",
            FontSize = 18,
            TextColor = new SolidColorBrush(Color.Orange),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new(0, 10, 10, 0),
            Parent = gameUI
        };

        // 游戏时间显示
        timeLabel = new Label()
        {
            Text = "Time: 0:00",
            FontSize = 18,
            TextColor = new SolidColorBrush(Color.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new(0, 10, 0, 0),
            Parent = gameUI
        };

        // 状态信息显示
        statusLabel = new Label()
        {
            Text = "🧛 Vampire Survivors 3D",
            FontSize = 16,
            TextColor = new SolidColorBrush(Color.Yellow),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new(10, 0, 0, 10),
            Parent = gameUI
        };

        // 创建帮助信息
        var helpLabel = new Label()
        {
            Text = "WASD: Move | F3: Spawn Monsters | F4: Show Stats | ESC: Pause",
            FontSize = 14,
            TextColor = new SolidColorBrush(Color.LightGray),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new(0, 0, 0, 10),
            Parent = gameUI
        };

        // 添加到视觉树
        _ = gameUI.AddToVisualTree();

        Game.Logger.LogInformation("🎮 Game UI created successfully");
    }

    private static void SetupInputHandlers()
    {
        // 键盘输入处理
        Trigger<EventGameKeyDown> keyDownTrigger = new(OnKeyDownAsync, true);
        keyDownTrigger.Register(Game.Instance);

        // 鼠标输入处理
        Trigger<EventGamePointerButtonDown> mouseDownTrigger = new(OnMouseDownAsync, true);
        mouseDownTrigger.Register(Game.Instance);

        Game.Logger.LogInformation("⌨️ Input handlers setup complete");
    }

    private static async Task<bool> OnKeyDownAsync(object sender, EventGameKeyDown eventArgs)
    {
        var localPlayer = Player.LocalPlayer;
        var mainUnit = localPlayer?.MainUnit;

        switch (eventArgs.Key)
        {
            case GameCore.Platform.SDL.VirtualKey.W:
            case GameCore.Platform.SDL.VirtualKey.Up:
                MoveUnit(mainUnit, new Vector2(0, -1));
                break;
            case GameCore.Platform.SDL.VirtualKey.S:
            case GameCore.Platform.SDL.VirtualKey.Down:
                MoveUnit(mainUnit, new Vector2(0, 1));
                break;
            case GameCore.Platform.SDL.VirtualKey.A:
            case GameCore.Platform.SDL.VirtualKey.Left:
                MoveUnit(mainUnit, new Vector2(-1, 0));
                break;
            case GameCore.Platform.SDL.VirtualKey.D:
            case GameCore.Platform.SDL.VirtualKey.Right:
                MoveUnit(mainUnit, new Vector2(1, 0));
                break;
            case GameCore.Platform.SDL.VirtualKey.Escape:
                statusLabel!.Text = "⏸️ Game Paused - Press ESC to resume";
                Game.Logger.LogInformation("Game paused");
                break;
            case GameCore.Platform.SDL.VirtualKey.F5:
                // 重置游戏视角到英雄位置
                if (mainUnit != null)
                {
                    var camera = DeviceInfo.PrimaryViewport.Camera;
                    if (camera != null)
                    {
                        camera.SetPosition(mainUnit.Position.Vector3, TimeSpan.Zero);
                        Game.Logger.LogInformation("📹 Camera reset to hero position");
                    }
                }
                break;
        }

        await Task.CompletedTask;
        return false; // 允许其他触发器处理
    }

    private static async Task<bool> OnMouseDownAsync(object sender, EventGamePointerButtonDown eventArgs)
    {
        // 处理鼠标点击 - 可以用于手动攻击指定目标
        var localPlayer = Player.LocalPlayer;
        var mainUnit = localPlayer?.MainUnit;

        if (mainUnit != null && eventArgs.PointerPosition.HasValue)
        {
            // 将屏幕坐标转换为世界坐标
            var result = DeviceInfo.PrimaryViewport.RaycastTerrainOrWorldPanelXY(eventArgs.PointerPosition.Value);
            if (result.IsHit)
            {
                var targetPos = new ScenePoint(result.Position, localPlayer!.Scene);
                
                // 发出移动指令
                Command moveCommand = new()
                {
                    Index = CommandIndex.Move,
                    Target = targetPos,
                    Type = ComponentTag.Walkable
                };
                
                var result2 = moveCommand.IssueOrder(mainUnit);
                if (result2.IsSuccess)
                {
                    Game.Logger.LogInformation("🎯 Moving to: {pos}", targetPos.Vector3);
                }
            }
        }

        await Task.CompletedTask;
        return false;
    }

    private static void MoveUnit(Unit? unit, Vector2 direction)
    {
        if (unit == null) return;

        var angle = Angle.FromVector2(direction);
        Command command = new()
        {
            Index = CommandIndex.VectorMove,
            Target = angle,
            Type = ComponentTag.Walkable
        };
        
        var result = command.IssueOrder(unit);
        if (result.IsSuccess)
        {
            Game.Logger.LogInformation("🚶 Moving in direction: {direction}", direction);
        }
    }

    private static async Task<bool> OnMainUnitChangedAsync(object sender, EventPlayerMainUnitChanged eventArgs)
    {
        Game.Logger.Log(LogLevel.Warning, "🦸 Main unit changed to: {unit}", eventArgs.Unit);

        var mainUnit = eventArgs.Unit;
        if (mainUnit == null) return true;

        // 等待帧末确保单位完全初始化
        await Game.Delay(TimeSpan.FromSeconds(0));

        // 检查主角的被动技能配置
        var heroAbilities = mainUnit.Cache.Abilities;
        if (heroAbilities != null && heroAbilities.Count > 0)
        {
            Game.Logger.LogInformation("🧙 Hero has {count} abilities configured:", heroAbilities.Count);
            foreach (var ability in heroAbilities)
            {
                if (ability?.Data is not null)
                {
                    Game.Logger.LogInformation("  ✨ {name}: {desc}", 
                        ability.Data.DisplayName, ability.Data.Description);
                }
            }
        }
        else
        {
            Game.Logger.LogWarning("❌ Hero has no abilities configured! Passive skills won't work.");
        }

        // 设置摄像机跟随英雄
        var camera = DeviceInfo.PrimaryViewport.Camera;
        if (camera != null)
        {
            camera.SetPosition(mainUnit.Position.Vector3, TimeSpan.Zero);
            camera.FollowTarget = mainUnit;
            Game.Logger.LogInformation("📹 Camera set to follow hero");
        }

        // 设置UI更新
        ClientUIManager.SetupClientUIUpdates(mainUnit);

        // 设置战斗事件监听
        SetupCombatEventListeners(mainUnit);

        Game.Logger.LogInformation("🎮 Hero setup complete: {hero} at {pos}", 
            mainUnit.Cache.Name, mainUnit.Position);

        return true;
    }

    private static void SetupUIUpdates(Unit hero)
    {
        // 创建UI更新定时器
        var aTimer = new GameCore.Timers.Timer(TimeSpan.FromMilliseconds(100))
        {
            AutoReset = true
        };
        aTimer.Elapsed += (_, __) => UpdateGameUI(hero);
        aTimer.Start();

        Game.Logger.LogInformation("📊 UI update timer created");
    }

    private static void UpdateGameUI(Unit hero)
    {
        if (healthLabel == null || killCountLabel == null || timeLabel == null) return;

        // 更新血量显示
        var vital = hero.GetTagComponent<Vital>(PropertyVital.Health);
        if (vital != null)
        {
            healthLabel.Text = $"Health: {vital.Current:F0}/{vital.Max:F0}";
        }

        // 更新游戏时间 (简单计算)
        // 保留HH:MM:SS
        timeLabel.Text = $"Time: {Game.ElapsedTime:hh\\:mm\\:ss}";

        // 更新状态
        statusLabel!.Text = $"🧛 Position: ({hero.Position.X:F0}, {hero.Position.Y:F0})";
    }

    private static void SetupCombatEventListeners(Unit hero)
    {
        // 监听英雄技能使用
        //Trigger<EventEntityOrderStage> skillTrigger = new(async (s, d) =>
        //{
        //    if (d.Unit == hero && d.Order.Command.AbilityLink != null)
        //    {
        //        statusLabel!.Text = $"🔥 Used: {d.Order.Command.AbilityLink}";
        //        Game.Logger.LogInformation("Hero used ability: {ability}", d.Order.Command.AbilityLink);
        //    }
        //    return true;
        //});
        //skillTrigger.Register(hero);

        // 监听英雄受伤
        Trigger<EventDamageNotification> damageTrigger = new(async (s, d) =>
        {
            if (d.Target == hero)
            {
                statusLabel!.Text = $"💔 Took {d.DamageInstance} damage!";
                Game.Logger.LogInformation("Hero took damage: {damage}", d.DamageInstance);
            }
            await Task.CompletedTask;
            return true;
        });
        damageTrigger.Register(hero);

        Game.Logger.LogInformation("⚔️ Combat event listeners setup complete");
    }
}
#endif 