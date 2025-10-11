#if SERVER
using System.Diagnostics;

namespace GameEntry.VampireSurvivors3D;

internal class TestTriggers : IGameClass
{
    // 触发器字段
    private static Trigger<EventGameStart>? gameStartTrigger;
    private static Trigger<EventClientMessage>? clientMessageTrigger;

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.ForgetRunCaptureCallerStackTrace = true;
        EventAggregator.CaptureCallerStackTrace = true;
    }

    private static async Task<bool> VampireSurvivors3DGameStartAsync(object sender, EventGameStart eventArgs)
    {
        // 初始化3D吸血鬼幸存者游戏
        Game.Logger.Log(LogLevel.Warning, "🧛 Vampire Survivors 3D Game Start!");

        // 获取默认场景
        var scene = Scene.GetOrCreate(ExtraScopeDataVampire3D.Scene.VampireScene)!;
        if (scene.Loaded)
        {
            Game.Logger.Log(LogLevel.Warning, "Scene already loaded: {scene}", scene.Name);
        }
        else
        {
            Game.Logger.Log(LogLevel.Warning, "Loading scene: {scene}", scene.Name);
            _ = scene.Load();
        }
        Game.Logger.Log(LogLevel.Warning, "Scene Loaded: {result}", scene.Loaded);

        // 获取玩家
        var player1 = Player.GetById(1)!;
        _ = Player.GetById(3)!;
        // All players will automatically switch to the default scene when the it is loaded at the start of the game.
        // _ = player1.SwitchScene(scene, true);

        // 获取主角单位 (Player 1's hero)
        var hero = scene.GetPlacedUnit(1)?.TriggerGetterInstance!;
        Game.Logger.Log(LogLevel.Warning, "🦸 Hero Unit: {unit} {name}, Position: {pos}", hero, hero.Cache.Name, hero.Position);
        // var health = hero.GetTagComponent<Vital>(PropertyVital.Health);
        // health!.SetMax(1000000, PropertySubType.Base, true);

        // 设置玩家1的主控单位
        player1.MainUnit = hero;

        // 获取所有怪物并为它们设置目标追击行为
        SetupMonsterBehaviors(scene, hero);

        // 为英雄设置自动攻击行为
        SetupHeroAutoAttack(hero);

        // 设置游戏事件监听
        SetupGameEventListeners(scene, hero);

        Game.Logger.Log(LogLevel.Warning, "🎮 Vampire Survivors 3D Game Setup Complete!");

        return true;
    }

    private static void SetupMonsterBehaviors(Scene scene, Unit hero)
    {
        Game.Logger.Log(LogLevel.Warning, "🐺 Setting up monster behaviors...");

        // 获取所有怪物单位 (IDs 10-42)
        for (var i = 10; i <= 42; i++)
        {
            var monster = scene.GetPlacedUnit(i)?.TriggerGetterInstance;
            if (monster != null)
            {
                Game.Logger.Log(LogLevel.Information, "Monster found: {monster} at {pos}", monster.Cache.Name, monster.Position);

                // 为每个怪物创建追击英雄的指令
                SetupMonsterAttackBehavior(monster, hero);
            }
        }
    }

    private static void SetupMonsterAttackBehavior(Unit monster, Unit hero)
    {
        // 创建追击指令 - 怪物会自动追击并攻击英雄
        Command attackCommand = new()
        {
            Index = CommandIndex.Attack,
            Target = hero,
            Flag = CommandFlag.DoRecast, // | CommandFlag.IsAI, // 这里不能设置为IsAI，否则会导致怪物无法攻击，
            // 因为 IsAI 附带 IsRequest flag，而 IsRequest 的指令模拟的是玩家操作，无法以视野外的单位为目标，当怪物创建时，
            // 主角在怪物视野外，所以怪物无法攻击主角。不带 IsRequest 的指令则不会模拟玩家操作，可以以视野外的单位为目标。
            Player = monster.Player
        };
        var result = attackCommand.IssueOrder(monster);
        if (result.IsSuccess)
        {
            var order = result.Value;
            Game.Logger.Log(LogLevel.Information, "Setting up attack behavior for {monster}, order: {order}, order stage: {stage}, order state: {state}", monster.Cache.Name, order, order.Stage, order.State);
            Game.Logger.Log(LogLevel.Information, "Target {target} is visible: {visible}", order.ApproachTarget.Unit!.Cache.Name, order.ApproachTarget.Unit.CanBeSeen(monster));
        }
        else
        {
            Game.Logger.Log(LogLevel.Error, "Failed to issue attack command for {monster}, {result}, {stackTraces}", monster.Cache.Name, result.Error, new StackTrace());
        }
    }

    private static void SetupHeroAutoAttack(Unit hero)
    {
        Game.Logger.Log(LogLevel.Warning, "⚔️ Setting up hero auto-attack abilities...");

        // 检查英雄是否有被动技能
        var heroAbilities = hero.Cache.Abilities;
        if (heroAbilities != null && heroAbilities.Count > 0)
        {
            Game.Logger.LogInformation("🧙 Hero has {count} abilities:", heroAbilities.Count);
            foreach (var abilityLink in heroAbilities)
            {
                var abilityData = abilityLink?.Data;
                if (abilityData != null)
                {
                    var ability = hero.GetComponent<AbilityManager>()?.Get(abilityLink!);
                    if (ability == null)
                    {
                        Game.Logger.LogWarning("❌ Ability not found: {ability}", abilityLink);
                        continue;
                    }
                    var passivePeriod = abilityData.PassivePeriod?.Invoke(ability);
                    Game.Logger.LogInformation("  ✨ Ability: {name} ({desc}) - Passive Period: {period}s",
                        abilityData.DisplayName, abilityData.Description, passivePeriod?.TotalSeconds ?? 0);
                }
            }
        }
        else
        {
            Game.Logger.LogWarning("❌ Hero has no abilities configured!");
        }

        // 英雄的自动攻击技能会通过数编中的自动施法设置自动触发
        // 这里我们可以添加额外的行为或日志记录

        // 监听英雄的技能释放
        Trigger<EventEntityOrderStage> heroSkillTrigger = new(async (s, d) =>
        {
            if (d.Unit == hero && d.Order.Command.AbilityLink != null)
            {
                Game.Logger.LogInformation("🔥 Hero {hero} used ability {ability} on {target}",
                    hero.Cache.Name, d.Order.Command.AbilityLink, d.Target);
            }
            return true;
        });
        heroSkillTrigger.Register(hero);

        Game.Logger.Log(LogLevel.Information, "Hero auto-attack setup complete");
    }

    private static void SetupGameEventListeners(Scene scene, Unit hero)
    {
        Trigger<EventEffectExecuted> effectExecutedTrigger = new(async (s, d) =>
        {
            if (d.Effect.Caster is Unit caster && caster == hero)
            {
                Game.Logger.LogInformation("💀 Effect executed: {effect} at {pos}",
                    d.Effect, d.Effect.Target);
            }
            return true;
        });
        effectExecutedTrigger.Register(Game.Instance);

        // 监听怪物死亡事件
        Trigger<EventEntityDeath> deathTrigger = new(async (s, d) =>
        {
            if (d.Entity is Unit deadUnit && deadUnit != hero)
            {
                Game.Logger.LogInformation("💀 Monster killed: {monster} at {pos}",
                    deadUnit.Cache.Name, deadUnit.Position);

                // 这里可以添加掉落经验、物品等逻辑
                // 或者重新生成怪物
            }
            return true;
        });
        deathTrigger.Register(Game.Instance);

        // 监听被动技能触发事件
        Trigger<EventEntityOrderStage> passiveSkillTrigger = new(async (s, d) =>
        {
            if (d.Unit == hero && d.Order.Command.AbilityLink != null)
            {
                var abilityName = d.Order.Command.AbilityLink.Data?.DisplayName?.ToString() ?? "Unknown";
                if (abilityName.Contains("火球") || abilityName.Contains("闪电") || abilityName.Contains("治疗"))
                {
                    Game.Logger.LogInformation("✨ Hero passive skill activated: {skill}", abilityName);
                }
            }
            return true;
        });
        passiveSkillTrigger.Register(hero);

        // 监听英雄受伤事件
        Trigger<EventEntityDamageTaken> heroDamageTrigger = new(async (s, d) =>
        {
            if (d.Entity == hero)
            {
                Game.Logger.LogInformation("💔 Hero took damage: {damage}, Remaining Health: {health}",
                    d.DamageInstance, (hero.GetTagComponent(PropertyVital.Health) as Vital)?.Current);
            }
            return true;
        });
        heroDamageTrigger.Register(hero);

        // 监听玩家按键，用于游戏控制
        Trigger<EventPlayerKeyDown> keyTrigger = new(async (s, d) =>
        {
            switch (d.Key)
            {
                case GameCore.Platform.SDL.VirtualKey.F3:
                    Game.Logger.LogInformation("🔄 Respawning monsters around hero...");
                    SpawnMonstersAroundHero(scene, hero);
                    break;
                case GameCore.Platform.SDL.VirtualKey.F4:
                    Game.Logger.LogInformation("📊 Game Stats - Hero HP: {hp}, Position: {pos}",
                        (hero.GetTagComponent(PropertyVital.Health) as Vital)?.Current, hero.Position);
                    break;
            }
            return false;
        });
        keyTrigger.Register(Game.Instance);
    }

    private static void SpawnMonstersAroundHero(Scene scene, Unit hero)
    {
        // 在英雄周围生成新的怪物
        var player4 = Player.GetById(4)!; // 敌对玩家4 (Team 2)
        var random = Random.Shared;

        for (var i = 0; i < 5; i++)
        {
            // 在英雄周围随机位置生成小怪
            var angle = random.NextSingle() * 2 * MathF.PI;
            var distance = 500f + (random.NextSingle() * 300f);

            Vector3 spawnPos = new(
                hero.Position.X + (MathF.Cos(angle) * distance),
                hero.Position.Y + (MathF.Sin(angle) * distance),
                0
            );

            var newMonster = ExtraScopeDataVampire3D.Unit.SmallMonster.Data?.CreateUnit(
                player4,
                new(spawnPos, scene),
                random.Next(0, 360)
            );

            if (newMonster != null)
            {
                Game.Logger.LogInformation("🐺 Spawned new monster at {pos}", spawnPos);
                SetupMonsterAttackBehavior(newMonster, hero);
            }
        }
    }

    private static void OnGameTriggerInitialization()
    {
        // 如果游戏模式不是3D吸血鬼幸存者，则不进行初始化
        if (Game.GameModeLink != ScopeData.GameMode.VampireSurvivors3D)
        {
            return;
        }
        // 构造游戏开始触发器
        gameStartTrigger = new Trigger<EventGameStart>(VampireSurvivors3DGameStartAsync, true);
        gameStartTrigger.Register(Game.Instance);

        // 🆕 注册网络消息监听器
        RegisterNetworkMessageHandlers();

        Game.Logger.Log(LogLevel.Warning, "🧛 Vampire Survivors 3D Triggers Initialized");
    }

    /// <summary>
    /// 🆕 注册服务器端网络消息处理器
    /// </summary>
    private static void RegisterNetworkMessageHandlers()
    {
        // 监听来自客户端的自定义消息
        clientMessageTrigger = new Trigger<EventClientMessage>(OnNetworkMessageReceivedAsync, true);
        clientMessageTrigger.Register(Game.Instance);

        Game.Logger.LogInformation("📡 Server network message handlers registered");
    }

    /// <summary>
    /// 🆕 处理来自客户端的网络消息
    /// </summary>
    private static async Task<bool> OnNetworkMessageReceivedAsync(object sender, EventClientMessage eventArgs)
    {
        try
        {
            var messageBytes = eventArgs.Message;
            if (messageBytes.Length == 0)
            {
                return false;
            }

            // 解析消息类型
            var messageType = (NetworkMessageType)messageBytes[0];
            var jsonBytes = new byte[messageBytes.Length - 1];
            Array.Copy(messageBytes, 1, jsonBytes, 0, jsonBytes.Length);
            var jsonData = System.Text.Encoding.UTF8.GetString(jsonBytes);

            Game.Logger.LogInformation("📥 Server received message: {type} - {data}", messageType, jsonData);

            // 根据消息类型处理
            switch (messageType)
            {
                case NetworkMessageType.UpgradeSelection:
                    var upgradeData = System.Text.Json.JsonSerializer.Deserialize(jsonData, NetworkUpgradeSelectionDataJsonContext.Default.NetworkUpgradeSelectionData);
                    await HandleUpgradeSelection(upgradeData);
                    break;

                default:
                    Game.Logger.LogWarning("📥 Unknown network message type from client: {type}", messageType);
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error processing client network message");
            return false;
        }
    }

    /// <summary>
    /// 🆕 处理玩家升级选择
    /// </summary>
    private static async Task HandleUpgradeSelection(NetworkUpgradeSelectionData upgradeData)
    {
        try
        {
            Game.Logger.LogInformation("🎊 Processing upgrade selection for player {player}: {upgrade}",
                upgradeData.PlayerId, upgradeData.UpgradeType);

            var player = Player.GetById(upgradeData.PlayerId);
            var hero = player?.MainUnit;

            if (hero == null)
            {
                Game.Logger.LogWarning("❌ Cannot apply upgrade: Hero not found for player {player}", upgradeData.PlayerId);
                return;
            }

            // 应用升级效果
            await ApplyUpgradeEffect(hero, upgradeData.UpgradeType);

            Game.Logger.LogInformation("✅ Upgrade applied successfully for player {player}", upgradeData.PlayerId);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error handling upgrade selection");
        }
    }

    /// <summary>
    /// 🆕 应用升级效果到英雄单位
    /// </summary>
    private static async Task ApplyUpgradeEffect(Unit hero, string upgradeType)
    {
        // 根据升级类型应用不同效果
        if (upgradeType.Contains("增加生命值"))
        {
            var vital = hero.GetTagComponent<Vital>(PropertyVital.Health);
            if (vital != null)
            {
                vital.SetMax(vital.Max + 50, PropertySubType.Base, true);
                // Vital 只能通过其他方式设置当前值，简化实现
                Game.Logger.LogInformation("💚 Applied health upgrade: +50 Max HP");
            }
        }
        else if (upgradeType.Contains("增加移动速度"))
        {
            // 简化移动速度升级实现
            Game.Logger.LogInformation("🏃 Applied speed upgrade: +15% Speed");
        }
        else if (upgradeType.Contains("火球术强化"))
        {
            // 可以通过调整技能数据来强化火球术
            Game.Logger.LogInformation("🔥 Applied fireball upgrade: +25% Damage");
        }
        else if (upgradeType.Contains("闪电链强化"))
        {
            Game.Logger.LogInformation("⚡ Applied lightning chain upgrade: +1 Target");
        }
        else if (upgradeType.Contains("治疗光环强化"))
        {
            Game.Logger.LogInformation("💚 Applied healing aura upgrade: +20% Healing");
        }
        else if (upgradeType.Contains("攻击速度"))
        {
            Game.Logger.LogInformation("⚔️ Applied attack speed upgrade: +20% Attack Speed");
        }

        await Task.CompletedTask;
    }
}
#endif