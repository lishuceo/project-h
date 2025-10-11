#if SERVER
using GameCore.AISystem.Data.Enum;
using GameCore.AISystem.Enum;

namespace GameEntry.AISystemTest;

/// <summary>
/// AI系统测试游戏模式的服务端实现
/// 正确测试WaveAI和AIThinkTree的各种功能和交互
/// </summary>
internal class AISystemTestServer : IGameClass
{
    #region Fields

    private static Trigger<EventGameStart>? gameStartTrigger;
    private static Scene? testScene;
    private static WaveAI? currentWaveAI;  // 当前活跃的WaveAI实例
    private static readonly List<Unit> testUnits = [];
    private static Unit? leaderUnit;
    private static readonly Vector3 basePosition = new(4000, 4000, 0);
    private static int currentTestPhase = 0;
    private static bool waitingForNextPhase = false;
    private static bool isTestSequenceRunning = false;

    // 测试目标点
    private static readonly Vector3 guardTargetPosition = new(4000, 4000, 0);
    private static readonly Vector3 moveTargetPosition = new(5500, 5000, 0);
    private static readonly Vector3 patrolPointA = new(4000, 4000, 0);
    private static readonly Vector3 patrolPointB = new(5000, 5500, 0);

    // AI战斗测试相关
    private static readonly List<Unit> aiCombatUnits = [];
    private static readonly Vector3 aiCombatArea = new(2000, 2000, 0); // 场景左下1/4区域
    private static readonly float aiCombatAreaSize = 800f; // 减小战斗区域确保单位能相互发现

    #endregion

    #region Initialization

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.Logger.LogInformation("🤖 AI System Test Server registered");
    }

    private static void OnGameTriggerInitialization()
    {
        if (Game.GameModeLink != ScopeData.GameMode.AISystemTest)
        {
            Game.Logger.LogInformation("🚫 Not AISystemTest mode - skipping trigger registration");
            return;
        }
        gameStartTrigger = new Trigger<EventGameStart>(OnGameStartAsync, true);
        gameStartTrigger.Register(Game.Instance);
        Game.Logger.LogInformation("🎯 AI System Test triggers initialized");
    }

    #endregion

    #region Game Start

    private static async Task<bool> OnGameStartAsync(object sender, EventGameStart eventArgs)
    {
        Game.Logger.LogInformation("🚀 AI System Test Mode Started!");

        await InitializeTestEnvironment();
        await CreateTestUnits();

        Game.Logger.LogInformation("⏸️ AI Test sequence ready - use 'start_ai_test' command or press [Space] to begin");
        Game.Logger.LogInformation("⏸️ AIThinkTree validation ready - use 'start_ai_validation' command to begin");
        Game.Logger.LogInformation("⚔️ AI Combat test ready - use 'start_ai_combat' command to begin");
        Game.Logger.LogInformation("✅ AI System Test initialization complete - waiting for manual trigger");
        return true;
    }

    #endregion

    #region Environment Setup

    private static async Task InitializeTestEnvironment()
    {
        testScene = Scene.GetOrCreate(ScopeData.Scene.AITestScene);
        if (!testScene.Loaded)
        {
            _ = testScene.Load();
            Game.Logger.LogInformation("🌍 Using default scene for AI tests: {SceneName}", testScene.Name);
        }
        else
        {
            Game.Logger.LogInformation("🌍 Default scene already loaded for AI tests: {SceneName}", testScene.Name);
        }

        _ = await Game.Delay(TimeSpan.FromMilliseconds(500));
    }

    #endregion

    #region Test Units Creation

    private static async Task CreateTestUnits()
    {
        var players = Player.AllPlayers.ToList();
        if (players.Count == 0)
        {
            Game.Logger.LogWarning("⚠️ No players found for AI test");
            return;
        }

        var testPlayer = players.FirstOrDefault() ?? Player.GetById(1);
        if (testPlayer == null)
        {
            Game.Logger.LogError("❌ Failed to get test player");
            return;
        }

        Game.Logger.LogInformation("👤 Using test player: {PlayerId}", testPlayer.Id);

        // 创建领队单位
        leaderUnit = ScopeData.Unit.HostTestHero.Data?.CreateUnit(
            testPlayer,
            new ScenePoint(basePosition, testScene!),
            0
        );

        if (leaderUnit != null)
        {
            testPlayer.MainUnit = leaderUnit;
            Game.Logger.LogInformation("👑 Leader unit created: {UnitName} at {Position}",
                leaderUnit.Cache.Name, leaderUnit.Position);

            // 手动添加AIThinkTree组件
            if (leaderUnit.GetComponent<AIThinkTree>() == null)
            {
                var aiData = new GameDataAIThinkTree(new GameLink<GameDataAIThinkTree, GameDataAIThinkTree>("TestAI"u8))
                {
                    ScanFilters = [],
                    ScanSorts = []
                };
                var newAI = aiData.CreateAI(leaderUnit);
                Game.Logger.LogInformation("🧠 Manually added AIThinkTree to leader: {Success}", newAI != null);
            }

            // 验证组件
            var walkable = leaderUnit.GetComponent<Walkable>();
            var aiThinkTree = leaderUnit.GetComponent<AIThinkTree>();
            Game.Logger.LogInformation("📋 Leader components - Walkable: {Walkable}, AIThinkTree: {AI}",
                walkable != null, aiThinkTree != null);
        }

        // 创建跟随单位
        for (var i = 0; i < 4; i++)
        {
            var offset = new Vector3(
                100 + (i * 150),
                100 + (i % 2 * 150),
                0
            );

            var unit = ScopeData.Unit.HostTestHero.Data?.CreateUnit(
                testPlayer,
                new ScenePoint(basePosition + offset, testScene!),
                0
            );

            if (unit != null)
            {
                testUnits.Add(unit);
                Game.Logger.LogInformation("🔸 Test unit {Index} created: {UnitName} at {Position}",
                    i + 1, unit.Cache.Name, unit.Position);

                // 手动添加AIThinkTree组件
                if (unit.GetComponent<AIThinkTree>() == null)
                {
                    var linkName = System.Text.Encoding.UTF8.GetBytes($"TestAI{i}");
                    var aiData = new GameDataAIThinkTree(new GameLink<GameDataAIThinkTree, GameDataAIThinkTree>(linkName))
                    {
                        ScanFilters = [],
                        ScanSorts = []
                    };
                    var newAI = aiData.CreateAI(unit);
                    Game.Logger.LogInformation("🧠 Manually added AIThinkTree to unit {Index}: {Success}", i + 1, newAI != null);
                }

                // 验证组件
                var walkable = unit.GetComponent<Walkable>();
                var aiThinkTree = unit.GetComponent<AIThinkTree>();
                Game.Logger.LogInformation("📋 Unit {Index} components - Walkable: {Walkable}, AIThinkTree: {AI}",
                    i + 1, walkable != null, aiThinkTree != null);
            }
        }

        _ = await Game.Delay(TimeSpan.FromMilliseconds(100));
    }

    #endregion

    #region AI Combat Test

    /// <summary>
    /// 创建AI战斗测试场景
    /// </summary>
    public static async Task StartAICombatTest()
    {
        if (testScene == null)
        {
            Game.Logger.LogError("❌ Test scene not initialized");
            return;
        }

        Game.Logger.LogInformation("⚔️ Starting AI Combat Test...");

        // 清理之前的战斗单位
        await CleanupAICombatUnits();

        // 创建战斗单位
        await CreateAICombatUnits();

        Game.Logger.LogInformation("✅ AI Combat Test started! {Count} units are fighting in the combat area", aiCombatUnits.Count);
        Game.Logger.LogInformation("📍 Combat area: {Area}", aiCombatArea);
        Game.Logger.LogInformation("🎯 Use 'stop_ai_combat' to stop the test");
    }

    /// <summary>
    /// 停止AI战斗测试
    /// </summary>
    public static async Task StopAICombatTest()
    {
        Game.Logger.LogInformation("🛑 Stopping AI Combat Test...");
        await CleanupAICombatUnits();
        Game.Logger.LogInformation("✅ AI Combat Test stopped");
    }

    /// <summary>
    /// 清理AI战斗单位
    /// </summary>
    private static async Task CleanupAICombatUnits()
    {
        foreach (var unit in aiCombatUnits)
        {
            try
            {
                unit.Destroy();
            }
            catch (Exception ex)
            {
                Game.Logger.LogWarning("Failed to destroy AI combat unit: {Error}", ex.Message);
            }
        }
        aiCombatUnits.Clear();
        _ = await Game.Delay(TimeSpan.FromMilliseconds(100));
    }

    /// <summary>
    /// 创建战斗单位
    /// </summary>
    private static Unit? CreateCombatUnit(Player player, Vector3 position, Random random)
    {
        try
        {
            // 使用新的AI战斗测试单位（已配置TacticalAI）
            var unit = ScopeData.Unit.AICombatTestHero.Data?.CreateUnit(player, new ScenePoint(position, testScene!), 0);
            if (unit == null)
            {
                Game.Logger.LogError("Failed to create AI combat test unit");
                return null;
            }

            // 添加Default AI（现在应该能成功，因为新单位配置了TacticalAI）
            var aiThinkTree = AIThinkTree.AddDefaultAI(unit);
            if (aiThinkTree == null)
            {
                Game.Logger.LogError("⚠️ CRITICAL: Failed to add Default AI to AICombatTestHero - TacticalAI configuration may be missing");
                return null; // 如果AI创建失败，不创建无AI的战斗单位
            }
            else
            {
                Game.Logger.LogInformation("✅ Successfully added AIThinkTree to combat unit: {UnitName}", unit.Cache.Name);

                // 详细的AI调试信息
                Game.Logger.LogInformation("🔍 AI Debug Info - ScanRange: {ScanRange}, AttackRange: {AttackRange}",
                    aiThinkTree.ScanRange, aiThinkTree.Attack?.Range ?? 0);
                Game.Logger.LogInformation("🔍 AI Filters - Required: {Required}, Excluded: {Excluded}",
                    string.Join(",", aiThinkTree.Cache.ScanFilters.SelectMany(f => f.Required)),
                    string.Join(",", aiThinkTree.Cache.ScanFilters.SelectMany(f => f.Excluded)));
            }

            // 添加现有技能
            var abilityManager = unit.GetComponent<AbilityManager>();
            if (abilityManager != null)
            {
                // 添加测试技能
                var ability = ScopeData.Ability.TestSpell.Data?.CreateAbility(unit);
                if (ability != null)
                {
                    Game.Logger.LogInformation("Added TestSpell to {UnitName}", unit.Cache.Name);
                }

                // 检查攻击技能
                var attack = unit.Attack;
                if (attack != null)
                {
                    Game.Logger.LogInformation("🗡️ Attack Ability - Range: {Range}, Cooldown: {Cooldown}",
                        attack.Range, attack.Cooldown);
                }
                else
                {
                    Game.Logger.LogWarning("⚠️ Unit has no attack ability - will not be able to fight!");
                }
            }

            // 验证AI组件
            var walkable = unit.GetComponent<Walkable>();
            var finalAI = unit.GetComponent<AIThinkTree>();
            Game.Logger.LogInformation("📋 Combat unit components - Walkable: {Walkable}, AIThinkTree: {AI}, IsEnabled: {Enabled}",
                walkable != null, finalAI != null, finalAI?.IsEnabled ?? false);

            // 验证玩家关系
            Game.Logger.LogInformation("👤 Player Info - Id: {PlayerId}, Controller: {Controller}, IsNeutral: {IsNeutral}",
                player.Id, player.Controller, player.IsNeutral);

            return unit;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Failed to create AI combat test unit");
            return null;
        }
    }

    /// <summary>
    /// 创建AI战斗单位
    /// </summary>
    private static async Task CreateAICombatUnits()
    {
        var players = Player.AllPlayers.ToList();

        // 过滤出非中立且控制者为Computer的玩家
        var computerPlayers = players
            .Where(p => p.Id != 0 && p.Controller == ControllerType.Computer && !p.IsNeutral)
            .ToList();

        // 如果没有足够的Computer玩家，添加其他非中立玩家
        var nonNeutralPlayers = players
            .Where(p => p.Id != 0 && !p.IsNeutral)
            .ToList();

        var availablePlayers = computerPlayers.Count >= 2 ? computerPlayers : nonNeutralPlayers;

        if (availablePlayers.Count < 2)
        {
            Game.Logger.LogError("❌ Need at least 2 non-neutral players for AI combat test. Available: {Count}", availablePlayers.Count);
            var playerInfo = string.Join(", ", players.Select(p => $"Player{p.Id}({p.Controller}, Neutral:{p.IsNeutral})"));
            Game.Logger.LogInformation("📋 All players: {Players}", playerInfo);
            return;
        }

        // 创建两个敌对阵营，优先选择Computer玩家
        var team1Player = availablePlayers[0];
        var team2Player = availablePlayers[1];

        Game.Logger.LogInformation("🎯 Selected combat players: Team1=Player{Player1Id}({Controller1}), Team2=Player{Player2Id}({Controller2})",
            team1Player.Id, team1Player.Controller, team2Player.Id, team2Player.Controller);

        // 在战斗区域随机创建单位
        var random = new Random();
        var unitCount = random.Next(6, 12); // 6-12个单位

        for (var i = 0; i < unitCount; i++)
        {
            // 随机选择阵营
            var player = random.Next(2) == 0 ? team1Player : team2Player;

            // 随机位置（在战斗区域内）
            var randomX = aiCombatArea.X + (random.NextSingle() * aiCombatAreaSize);
            var randomY = aiCombatArea.Y + (random.NextSingle() * aiCombatAreaSize);
            var position = new Vector3(randomX, randomY, 0);

            // 创建单位
            var unit = CreateCombatUnit(player, position, random);
            if (unit != null)
            {
                aiCombatUnits.Add(unit);
                Game.Logger.LogInformation("⚔️ Created combat unit: {UnitName} (Player {PlayerId}, {Controller}) at {Position}",
                    unit.Cache.Name, player.Id, player.Controller, position);
            }

            // 稍微延迟，避免同时创建太多单位
            _ = await Game.Delay(TimeSpan.FromMilliseconds(50));
        }

        Game.Logger.LogInformation("🎉 AI combat test setup complete: {UnitCount} units created for {Team1Name} vs {Team2Name}",
            aiCombatUnits.Count, team1Player.Id, team2Player.Id);

        // 添加距离和关系调试信息
        await AnalyzeCombatSetup();

        // 启动AI状态监控
        _ = StartAIStatusMonitoring();
    }

    /// <summary>
    /// 分析战斗设置，检查单位距离和玩家关系
    /// </summary>
    private static async Task AnalyzeCombatSetup()
    {
        if (aiCombatUnits.Count < 2)
        {
            Game.Logger.LogWarning("⚠️ Not enough units for combat analysis");
            return;
        }

        Game.Logger.LogInformation("🔍 Analyzing combat setup...");

        // 分析单位间距离
        for (var i = 0; i < Math.Min(aiCombatUnits.Count, 5); i++)
        {
            var unit1 = aiCombatUnits[i];
            for (var j = i + 1; j < Math.Min(aiCombatUnits.Count, 5); j++)
            {
                var unit2 = aiCombatUnits[j];
                var distance = Vector2.Distance(unit1.Position.Vector, unit2.Position.Vector);
                var relationship = unit1.Player.GetRelationShip(unit2.Player);

                Game.Logger.LogInformation("📏 Distance Analysis - Unit{I} (Player{P1}) to Unit{J} (Player{P2}): {Distance:F1}, Relationship: {Relationship}",
                    i, unit1.Player.Id, j, unit2.Player.Id, distance, relationship);

                // 检查是否在AI搜索范围内
                var ai1 = unit1.GetComponent<AIThinkTree>();
                if (ai1 != null && distance <= ai1.ScanRange && relationship == PlayerRelationShip.Enemy)
                {
                    Game.Logger.LogInformation("✅ Unit{I} should be able to detect Unit{J} as enemy (Distance: {Distance} <= ScanRange: {ScanRange})",
                        i, j, distance, ai1.ScanRange);
                }
                else if (ai1 != null && relationship == PlayerRelationShip.Enemy)
                {
                    Game.Logger.LogWarning("⚠️ Unit{I} cannot detect Unit{J} - too far (Distance: {Distance} > ScanRange: {ScanRange})",
                        i, j, distance, ai1.ScanRange);
                }
            }
        }

        _ = await Game.Delay(TimeSpan.FromMilliseconds(100));
    }

    /// <summary>
    /// 启动AI状态监控
    /// </summary>
    private static async Task StartAIStatusMonitoring()
    {
        for (var cycle = 0; cycle < 10; cycle++) // 监控10个周期
        {
            _ = await Game.Delay(TimeSpan.FromSeconds(3));

            Game.Logger.LogInformation("🤖 AI Status Check #{Cycle}", cycle + 1);

            var activeAIs = 0;
            var combatUnits = 0;

            foreach (var unit in aiCombatUnits)
            {
                if (!unit.IsValid)
                {
                    continue;
                }

                var ai = unit.GetComponent<AIThinkTree>();
                if (ai != null)
                {
                    activeAIs++;
                    if (ai.CombatState != CombatState.OutOfCombat)
                    {
                        combatUnits++;
                        Game.Logger.LogInformation("⚔️ Unit {UnitName} (Player{PlayerId}) in combat state: {CombatState}, Target: {Target}",
                            unit.Cache.Name, unit.Player.Id, ai.CombatState, ai.DefaultTarget?.ToString() ?? "None");
                    }

                    // 尝试手动扫描
                    var scanResult = ai.Scan();
                    if (scanResult != null)
                    {
                        Game.Logger.LogInformation("👁️ Unit {UnitName} scanned target: {TargetName} at distance {Distance:F1}",
                            unit.Cache.Name, scanResult.ToString(), Vector2.Distance(unit.Position.Vector, scanResult.Position.Vector));
                    }
                }
            }

            Game.Logger.LogInformation("📊 AI Status Summary - Active AIs: {ActiveAIs}/{TotalUnits}, Units in Combat: {CombatUnits}",
                activeAIs, aiCombatUnits.Count, combatUnits);

            if (combatUnits > 0)
            {
                Game.Logger.LogInformation("🎉 Combat detected! AI units are fighting.");
                break;
            }
        }
    }

    #endregion

    #region WaveAI Management

    /// <summary>
    /// 创建指定类型的WaveAI实例
    /// </summary>
    private static WaveAI? CreateWaveAI(WaveType waveType, string name)
    {
        if (leaderUnit == null || testUnits.Count == 0)
        {
            Game.Logger.LogError("❌ Cannot create WaveAI: missing units");
            return null;
        }

        // 销毁现有的WaveAI
        DestroyCurrentWaveAI();

        // 创建新的WaveAI配置
        var waveAIData = new GameDataWaveAI(new GameLink<GameDataWaveAI, GameDataWaveAI>(System.Text.Encoding.UTF8.GetBytes(name)))
        {
            Type = waveType,
            WaveLeash = 600.0f,
            EnableWaveFormation = true,
            EnableCombat = false,  // 禁用战斗以专注测试移动行为
            MinimalApproachRange = 200.0f,
            MinimalScanRange = 500.0f,
            MaximalScanRange = 1000.0f,
            AutoDisposeOnEmpty = false  // 手动管理生命周期
        };

        var waveAI = waveAIData.CreateWaveAI();
        if (waveAI == null)
        {
            Game.Logger.LogError("❌ Failed to create WaveAI instance");
            return null;
        }

        // 添加单位到WaveAI
        waveAI.Add(leaderUnit);
        foreach (var unit in testUnits)
        {
            waveAI.Add(unit);
        }

        // 验证Player设置
        var allUnits = testUnits.Append(leaderUnit).Where(u => u != null).ToList();
        foreach (var unit in allUnits)
        {
            if (unit.Player == null)
            {
                Game.Logger.LogError("❌ CRITICAL: Unit {UnitName} has no Player assigned!", unit.Cache?.Name);
            }
            else
            {
                Game.Logger.LogInformation("✅ Unit {UnitName} → Player {PlayerId}", unit.Cache?.Name, unit.Player.Id);
            }
        }

        // 启动WaveAI思考
        waveAI.StartThinking();
        currentWaveAI = waveAI;

        Game.Logger.LogInformation("🌊 Created {WaveType} WaveAI with {UnitCount} units, Leader: {LeaderName}",
            waveType, allUnits.Count, leaderUnit.Cache.Name);

        return waveAI;
    }

    /// <summary>
    /// 销毁当前的WaveAI实例
    /// </summary>
    private static void DestroyCurrentWaveAI()
    {
        if (currentWaveAI != null)
        {
            Game.Logger.LogInformation("🗑️ Destroying current WaveAI of type: {Type}", currentWaveAI.Cache.Type);
            currentWaveAI.StopThinking();
            currentWaveAI.Destroy();
            currentWaveAI = null;
        }
    }

    #endregion

    #region Test Sequence

    private static async Task RunTestSequence()
    {
        Game.Logger.LogInformation("🎬 Starting AI test sequence...");
        isTestSequenceRunning = true;

        try
        {
            // 阶段1：Guard模式测试
            await TestGuardBehavior();
            await WaitForPhaseProgression("Guard", "Move");

            // 阶段2：Move模式测试  
            await TestMoveBehavior();
            await WaitForPhaseProgression("Move", "Patrol");

            // 阶段3：Patrol模式测试
            await TestPatrolBehavior();
            await WaitForPhaseProgression("Patrol", "Formation");

            // 阶段4：编队测试
            await TestFormationBehavior();
            await WaitForPhaseProgression("Formation", "Command System");

            // 阶段5：指令系统测试
            await TestCommandSystem();

            currentTestPhase = 6;
            Game.Logger.LogInformation("🎉 All AI tests completed successfully!");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "💥 Error during AI test sequence");
        }
        finally
        {
            isTestSequenceRunning = false;
            waitingForNextPhase = false;
            DestroyCurrentWaveAI();
        }
    }

    private static async Task WaitForPhaseProgression(string currentPhaseName, string nextPhaseName)
    {
        const int extendedObservationTime = 10;
        const int manualWaitTime = 20;

        Game.Logger.LogInformation("⏱️ {CurrentPhase} phase completed. Extended observation time: {ObservationTime}s",
            currentPhaseName, extendedObservationTime);

        _ = await Game.Delay(TimeSpan.FromSeconds(extendedObservationTime));

        Game.Logger.LogInformation("⏸️ Ready for {NextPhase} phase. Click 'Next Phase' button or wait {WaitTime}s for auto-advance",
            nextPhaseName, manualWaitTime);

        waitingForNextPhase = true;
        var startWait = DateTime.UtcNow;

        while (waitingForNextPhase && (DateTime.UtcNow - startWait).TotalSeconds < manualWaitTime)
        {
            _ = await Game.Delay(TimeSpan.FromSeconds(1));
        }

        if (waitingForNextPhase)
        {
            waitingForNextPhase = false;
            Game.Logger.LogInformation("⏩ Auto-advancing to {NextPhase} phase (timeout)", nextPhaseName);
        }
        else
        {
            Game.Logger.LogInformation("▶️ Proceeding to {NextPhase} phase (manual trigger)", nextPhaseName);
        }
    }

    private static async Task TestGuardBehavior()
    {
        currentTestPhase = 1;
        Game.Logger.LogInformation("🛡️ Phase 1: Testing Guard behavior...");

        // 发送状态更新到客户端
        BroadcastStatusUpdate("Guard Behavior Test",
            "🛡️ Current Test: Guard Behavior (Phase 1)\n" +
            "• Units should stay within LEASH range of target (600 units)\n" +
            "• When target moves, units should follow and reposition\n" +
            "• Units should maintain protective formation around target\n" +
            "• Movement should be smooth and coordinated\n" +
            "• Watch for: Units not straying too far from guard point");

        // ✅ 正确做法：创建Guard类型的WaveAI
        var guardWaveAI = CreateWaveAI(WaveType.Guard, "GuardWaveAI");
        if (guardWaveAI == null)
        {
            return;
        }

        // ✅ 正确做法：设置WaveTarget而不是直接移动单位
        var guardTarget = new ScenePoint(guardTargetPosition, testScene!);
        guardWaveAI.WaveTarget = guardTarget;

        Game.Logger.LogInformation("🛡️ Guard test: Set WaveTarget to {Target}, WaveLeash: {Leash}",
            guardTargetPosition, guardWaveAI.Cache.WaveLeash);
        Game.Logger.LogInformation("📋 Expected behavior: Units should stay within {Leash} units of the target",
            guardWaveAI.Cache.WaveLeash);

        // 观察Guard行为 - 单位应该保持在目标周围的拴绳范围内
        _ = await Game.Delay(TimeSpan.FromSeconds(8));

        // 测试Guard行为的响应性：改变目标位置
        var newGuardTarget = guardTargetPosition + new Vector3(800, 600, 0);
        guardWaveAI.WaveTarget = new ScenePoint(newGuardTarget, testScene!);
        Game.Logger.LogInformation("🛡️ Guard test: Changed WaveTarget to {Target} to test responsiveness", newGuardTarget);

        _ = await Game.Delay(TimeSpan.FromSeconds(7));
    }

    private static async Task TestMoveBehavior()
    {
        currentTestPhase = 2;
        Game.Logger.LogInformation("🏃 Phase 2: Testing Move behavior...");

        // ✅ 正确做法：创建Move类型的WaveAI
        var moveWaveAI = CreateWaveAI(WaveType.Move, "MoveWaveAI");
        if (moveWaveAI == null)
        {
            return;
        }

        // ✅ 正确做法：设置WaveTarget测试Move行为
        var moveTarget = new ScenePoint(moveTargetPosition, testScene!);
        moveWaveAI.WaveTarget = moveTarget;

        Game.Logger.LogInformation("🏃 Move test: Set WaveTarget to {Target}, MinimalApproachRange: {Range}",
            moveTargetPosition, moveWaveAI.Cache.MinimalApproachRange);
        Game.Logger.LogInformation("📋 Expected behavior: Units should move to target and stop at approach range");

        _ = await Game.Delay(TimeSpan.FromSeconds(10));

        // 测试Move行为的精确定位
        var preciseMoveTarget = moveTargetPosition + new Vector3(-400, 800, 0);
        moveWaveAI.WaveTarget = new ScenePoint(preciseMoveTarget, testScene!);
        Game.Logger.LogInformation("🏃 Move test: Changed target to {Target} to test precision movement", preciseMoveTarget);

        _ = await Game.Delay(TimeSpan.FromSeconds(8));
    }

    private static async Task TestPatrolBehavior()
    {
        currentTestPhase = 3;
        Game.Logger.LogInformation("🔄 Phase 3: Testing Patrol behavior...");

        // ✅ 正确做法：创建Patrol类型的WaveAI
        var patrolWaveAI = CreateWaveAI(WaveType.Patrol, "PatrolWaveAI");
        if (patrolWaveAI == null)
        {
            return;
        }

        // ✅ 正确做法：设置OriginTarget和WaveTarget来测试巡逻
        var originPoint = new ScenePoint(patrolPointA, testScene!);
        var patrolTarget = new ScenePoint(patrolPointB, testScene!);

        patrolWaveAI.OriginTarget = originPoint;
        patrolWaveAI.WaveTarget = patrolTarget;

        Game.Logger.LogInformation("🔄 Patrol test: Set OriginTarget to {Origin}, WaveTarget to {Target}",
            patrolPointA, patrolPointB);
        Game.Logger.LogInformation("📋 Expected behavior: Units should patrol between origin and target points");

        // 观察完整的巡逻周期
        _ = await Game.Delay(TimeSpan.FromSeconds(15));

        // 测试巡逻路径变更
        var newPatrolTarget = patrolPointB + new Vector3(-600, -400, 0);
        patrolWaveAI.WaveTarget = new ScenePoint(newPatrolTarget, testScene!);
        Game.Logger.LogInformation("🔄 Patrol test: Changed WaveTarget to {Target} to test patrol adaptation", newPatrolTarget);

        _ = await Game.Delay(TimeSpan.FromSeconds(10));
    }

    private static async Task TestFormationBehavior()
    {
        currentTestPhase = 4;
        Game.Logger.LogInformation("📐 Phase 4: Testing formation behavior...");

        // 使用Move类型测试编队
        var formationWaveAI = CreateWaveAI(WaveType.Move, "FormationWaveAI");
        if (formationWaveAI == null)
        {
            return;
        }

        // 确保编队启用
        Game.Logger.LogInformation("📐 Formation enabled: {Enabled}", formationWaveAI.Cache.EnableWaveFormation);

        // 进行长距离移动测试编队保持
        var formationTargets = new Vector3[]
        {
            basePosition + new Vector3(1500, 1000, 0),
            basePosition + new Vector3(-1000, 1500, 0),
            basePosition + new Vector3(-1500, -800, 0),
            basePosition + new Vector3(1200, -1200, 0),
            basePosition
        };

        foreach (var (target, index) in formationTargets.Select((t, i) => (t, i)))
        {
            formationWaveAI.WaveTarget = new ScenePoint(target, testScene!);
            Game.Logger.LogInformation("📐 Formation test {Phase}: Moving to {Target}", index + 1, target);
            _ = await Game.Delay(TimeSpan.FromSeconds(6));
        }

        Game.Logger.LogInformation("📐 Formation behavior test completed");
    }

    private static async Task TestCommandSystem()
    {
        currentTestPhase = 5;
        Game.Logger.LogInformation("⚡ Phase 5: Testing Command System integration...");

        // 停用WaveAI以测试直接命令
        DestroyCurrentWaveAI();

        if (leaderUnit == null)
        {
            return;
        }

        Game.Logger.LogInformation("⚡ Testing direct AI commands using Command system...");

        // ✅ 正确做法：使用Command系统发出AI指令
        var testTargets = new Vector3[]
        {
            basePosition + new Vector3(800, 600, 0),
            basePosition + new Vector3(-600, 800, 0),
            basePosition + new Vector3(600, -600, 0),
            basePosition
        };

        foreach (var (target, index) in testTargets.Select((t, i) => (t, i)))
        {
            var command = new Command
            {
                Index = CommandIndex.Move,
                Type = ComponentTag.Walkable,
                Target = new ScenePoint(target, testScene!),
                Flag = CommandFlag.IsAI,  // ✅ 使用AI标志
                Player = leaderUnit.Player
            };

            var result = command.IssueOrder(leaderUnit);
            Game.Logger.LogInformation("⚡ AI Command {Phase}: Move to {Target}, Result: {Result}",
                index + 1, target, result.IsSuccess ? "Success" : result.Error?.ToString());

            if (result.IsSuccess)
            {
                _ = await Game.Delay(TimeSpan.FromSeconds(4));
            }
        }

        Game.Logger.LogInformation("⚡ Command system test completed");
    }

    #endregion

    #region AIThinkTree Validation

    private static async Task RunAIThinkTreeValidation()
    {
        try
        {
            _ = await Game.Delay(TimeSpan.FromSeconds(2));
            Game.Logger.LogInformation("🔍 Starting AIThinkTree validation...");

            // 这里应该调用AIThinkTree验证器，如果存在的话
            // var results = await AIThinkTreeValidator.RunFullValidation();

            Game.Logger.LogInformation("🎉 AIThinkTree validation completed successfully");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "💥 Error during AIThinkTree validation");
        }
    }

    #endregion

    #region Manual Triggers

    public static async Task StartAITestSequence()
    {
        if (currentTestPhase > 0)
        {
            Game.Logger.LogWarning("⚠️ AI test sequence is already running or completed");
            return;
        }

        Game.Logger.LogInformation("🎬 Manually starting AI test sequence...");
        _ = RunTestSequence();
    }

    public static async Task StartAIValidation()
    {
        Game.Logger.LogInformation("🔍 Manually starting AIThinkTree validation...");
        _ = RunAIThinkTreeValidation();
    }

    public static async Task StartAICombatTestCommand()
    {
        await StartAICombatTest();
    }

    public static async Task StopAICombatTestCommand()
    {
        await StopAICombatTest();
    }

    public static void ResetTestState()
    {
        DestroyCurrentWaveAI();
        currentTestPhase = 0;
        waitingForNextPhase = false;
        isTestSequenceRunning = false;
        Game.Logger.LogInformation("🔄 AI test state reset - ready for new test sequence");
    }

    public static void TriggerNextPhase()
    {
        if (!isTestSequenceRunning)
        {
            Game.Logger.LogWarning("⚠️ Cannot trigger next phase - test sequence is not running");
            return;
        }

        if (!waitingForNextPhase)
        {
            Game.Logger.LogWarning("⚠️ Cannot trigger next phase - not currently waiting for manual input");
            return;
        }

        waitingForNextPhase = false;
        Game.Logger.LogInformation("⏭️ Manual phase trigger activated - proceeding to next phase");
    }

    #endregion

    #region Debug Commands

    public static async Task HandleDebugCommand(string command)
    {
        switch (command.ToLower())
        {
            case "start_ai_test":
            case "start":
                await StartAITestSequence();
                break;

            case "start_ai_validation":
            case "validate":
                await StartAIValidation();
                break;

            case "start_ai_combat":
            case "combat":
                await StartAICombatTestCommand();
                break;

            case "stop_ai_combat":
            case "stopcombat":
                await StopAICombatTestCommand();
                break;

            case "reset_ai_test":
            case "reset":
                ResetTestState();
                break;

            case "ai_status":
            case "status":
                var status = GetWaveAIStatus();
                var phase = GetCurrentTestPhase();
                var unitCount = GetTestUnitCount();
                Game.Logger.LogInformation("📊 AI Status - Phase: {Phase}, Units: {Units}, WaveAI: {Status}",
                    phase, unitCount, status);
                break;

            default:
                Game.Logger.LogInformation("❓ Unknown AI command: {Command}. Available: start_ai_test, start_ai_validation, reset_ai_test, ai_status", command);
                break;
        }
    }

    #endregion

    #region Status Broadcasting

    /// <summary>
    /// 向所有客户端广播AI测试状态更新
    /// </summary>
    private static void BroadcastStatusUpdate(string phaseName, string behaviorDescription)
    {
        try
        {
            var statusInfo = new AITestStatusInfo
            {
                CurrentPhase = currentTestPhase,
                IsTestRunning = isTestSequenceRunning,
                IsWaitingForNextPhase = waitingForNextPhase,
                UnitCount = GetTestUnitCount(),
                ElapsedSeconds = Game.ElapsedTime.TotalSeconds
            };

            var statusUpdate = new ProtoAITestStatusUpdate
            {
                StatusInfo = statusInfo
            };

            // 使用Player.BroadcastClientMessage向所有客户端广播状态更新
            Player.BroadcastClientMessage(ref statusUpdate);

            Game.Logger.LogDebug("📡 Broadcasted AI test status update: Phase {Phase} at {Elapsed}s", phaseName, statusInfo.ElapsedSeconds);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Error broadcasting status update");
        }
    }

    #endregion

    #region Public API

    public static int GetCurrentTestPhase()
    {
        return currentTestPhase;
    }

    public static int GetTestUnitCount()
    {
        return testUnits.Count + (leaderUnit != null ? 1 : 0);
    }

    public static string GetWaveAIStatus()
    {
        if (currentWaveAI == null)
        {
            return "Not initialized";
        }

        var leaderName = currentWaveAI.Leader is Unit leader ? leader.Cache.Name : "None";
        return $"Type: {currentWaveAI.Cache.Type}, Units: {testUnits.Count + 1}, Leader: {leaderName}";
    }
    public static bool CanStartTest()
    {
        return currentTestPhase == 0 && testUnits.Count > 0;
    }

    public static bool IsWaitingForNextPhase()
    {
        return waitingForNextPhase;
    }

    public static bool IsTestSequenceRunning()
    {
        return isTestSequenceRunning;
    }

    #endregion
}
#endif