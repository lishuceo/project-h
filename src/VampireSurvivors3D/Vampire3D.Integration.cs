// 🔧 Vampire3D 改进系统集成指南
// 这个文件提供了将新系统集成到现有Vampire3D游戏中的具体步骤

namespace GameEntry.VampireSurvivors3D;

/// <summary>
/// 集成指南 - 按此步骤将改进系统集成到现有游戏中
/// </summary>
public static class IntegrationGuide
{
    /// <summary>
    /// 步骤1: 在主游戏类中注册新系统
    /// 将此代码添加到现有的 OnRegisterGameClass 方法中
    /// </summary>
    public static void Step1_RegisterNewSystems()
    {
        /*
        在 PassiveAbilities.OnRegisterGameClass() 方法中添加：
        
        public static void OnRegisterGameClass()
        {
            Game.OnGameDataInitialization += OnGameDataInitialization;
            
            // 🆕 添加新系统注册
            GameplaySystem.OnRegisterGameClass();
        }
        */
    }

    /// <summary>
    /// 步骤2: 在客户端集成新UI系统
    /// 修改 Vampire3D.Client.cs 中的UI创建代码
    /// </summary>
    public static void Step2_IntegrateEnhancedUI()
    {
        /*
        在 VampireSurvivors3DGameStartAsync 方法中，替换现有的 CreateGameUI() 调用：
        
        private static async Task<bool> VampireSurvivors3DGameStartAsync(object sender, EventGameStart eventArgs)
        {
            Game.Logger.Log(LogLevel.Warning, "🧛 Vampire Survivors 3D Client Game Start!");

            // 🔄 替换原有UI系统
            // CreateGameUI();  // 注释掉旧的UI
            EnhancedUI.CreateEnhancedGameUI();  // 使用新的增强UI

            SetupInputHandlers();
            // ...
        }
        */
    }

    /// <summary>
    /// 步骤3: 扩展数据定义
    /// 在 ExtraScopeDataVampire3D.cs 中添加新的单位和效果定义
    /// </summary>
    public static void Step3_ExtendDataDefinitions()
    {
        /*
        在 ExtraScopeDataVampire3D.Unit 类中添加：
        
        public static class Unit
        {
            // 现有单位...
            public static readonly GameLink<GameDataUnit, GameDataUnit> VampireSurvivorHero = new("VampireSurvivorHero"u8);
            public static readonly GameLink<GameDataUnit, GameDataUnit> SmallMonster = new("SmallMonster"u8);
            
            // 🆕 新增单位
            public static readonly GameLink<GameDataUnit, GameDataUnit> ExperienceOrb = new("ExperienceOrb"u8);
            public static readonly GameLink<GameDataUnit, GameDataUnit> HealthPotion = new("HealthPotion"u8);
            public static readonly GameLink<GameDataUnit, GameDataUnit> DamageBoostItem = new("DamageBoostItem"u8);
        }
        */
    }

    /// <summary>
    /// 步骤4: 创建经验球和道具单位
    /// 在数据初始化方法中添加这些单位的创建代码
    /// </summary>
    public static void Step4_CreateNewUnitTypes()
    {
        /*
        在 OnGameDataInitialization 方法中添加：
        
        private static void OnGameDataInitialization()
        {
            // 现有代码...
            
            // 🆕 创建经验球
            _ = new GameDataUnit(Unit.ExperienceOrb)
            {
                Name = "经验球",
                Filter = [UnitFilter.Item],
                State = [UnitState.Invulnerable],
                CollisionRadius = 20,
                AttackableRadius = 30,
                Particle = "effect/experience_orb/particle.effect"u8,
                UpdateFlags = new() { AllowMover = true },
                ModelScale = 0.5f,
            };

            // 🆕 创建生命药水
            _ = new GameDataUnit(Unit.HealthPotion)
            {
                Name = "生命药水",
                Filter = [UnitFilter.Item],
                State = [UnitState.Invulnerable],
                CollisionRadius = 16,
                AttackableRadius = 24,
                Particle = "effect/health_potion/particle.effect"u8,
                ModelScale = 0.3f,
            };
        }
        */
    }

    /// <summary>
    /// 步骤5: 实现经验值拾取逻辑
    /// 在服务器端添加经验球拾取检测
    /// </summary>
    public static void Step5_ImplementExperienceCollection()
    {
        /*
        在 Vampire3D.Server.cs 的 SetupGameEventListeners 方法中添加：
        
        // 🆕 经验值拾取检测
        var expCollectionTimer = new GameCore.Timers.Timer(TimeSpan.FromMilliseconds(100))
        {
            AutoReset = true
        };
        expCollectionTimer.Elapsed += (_, __) => CheckExperienceCollection(hero);
        expCollectionTimer.Start();
        
        private static void CheckExperienceCollection(Unit hero)
        {
            var scene = hero.Scene;
            var nearbyOrbs = scene.GetUnitsInRange(hero.Position, 50f)
                .Where(u => u.Cache.Name == "经验球");
                
            foreach (var orb in nearbyOrbs)
            {
                // 给玩家添加经验
                GameplaySystem.ExperienceSystem.AddExperience(hero.Player.Id, 25);
                
                // 销毁经验球
                orb.Destroy();
                
                Game.Logger.LogInformation("✨ Player collected experience orb!");
            }
        }
        */
    }

    /// <summary>
    /// 步骤6: 连接UI更新
    /// 确保游戏数据变化时UI能够正确更新
    /// </summary>
    public static void Step6_ConnectUIUpdates()
    {
        /*
        在客户端的 SetupUIUpdates 方法中：
        
        private static void SetupUIUpdates(Unit hero)
        {
            var aTimer = new GameCore.Timers.Timer(TimeSpan.FromMilliseconds(100))
            {
                AutoReset = true
            };
            aTimer.Elapsed += (_, __) => 
            {
                UpdateGameUI(hero);  // 现有UI更新
                
                // 🆕 更新增强UI
                var stats = GameplaySystem.GetCurrentStats();
                EnhancedUI.UpdateGameStats(
                    health: (int)(hero.GetTagComponent<Vital>(PropertyVital.Health)?.Current ?? 0),
                    maxHealth: (int)(hero.GetTagComponent<Vital>(PropertyVital.Health)?.Max ?? 0),
                    level: stats.Level,
                    experience: stats.Experience,
                    maxExp: stats.ExperienceRequired,
                    kills: stats.EnemiesKilled,
                    gameTime: stats.GameTime
                );
            };
            aTimer.Start();
        }
        */
    }

    /// <summary>
    /// 步骤7: 测试和调试
    /// 确保所有系统正常工作的测试步骤
    /// </summary>
    public static void Step7_TestingChecklist()
    {
        /*
        测试清单：
        
        1. ✅ 启动游戏，确认新UI界面显示正常
        2. ✅ 检查被动技能是否正常触发（火球、闪电、治疗）
        3. ✅ 击杀敌人后确认经验球生成
        4. ✅ 拾取经验球确认经验值增加
        5. ✅ 达到升级条件时确认升级界面显示
        6. ✅ 选择升级选项后确认效果应用
        7. ✅ 确认波次系统正常工作（30秒一波）
        8. ✅ 确认敌人强度随波次递增
        9. ✅ 检查所有UI元素实时更新
        10. ✅ 测试游戏性能（大量敌人时的帧率）

        调试命令：
        - F3: 手动生成怪物
        - F4: 显示游戏统计
        - F5: 重置相机位置
        */
    }
}

/// <summary>
/// 常见问题解决方案
/// </summary>
public static class TroubleshootingGuide
{
    /*
    常见问题及解决方案：

    ❌ 问题1: 新UI不显示
    ✅ 解决: 确保在客户端调用了 EnhancedUI.CreateEnhancedGameUI()

    ❌ 问题2: 经验球生成但无法拾取
    ✅ 解决: 检查碰撞检测逻辑，确保经验球有正确的Filter和CollisionRadius

    ❌ 问题3: 升级界面不出现
    ✅ 解决: 确认经验值系统正确计算升级条件，检查事件触发

    ❌ 问题4: 治疗技能不生效
    ✅ 解决: 确认GameDataEffectUnitModifyVital的配置正确

    ❌ 问题5: 波次系统不工作
    ✅ 解决: 检查GameplaySystem是否正确注册，timer是否启动

    ❌ 问题6: 性能问题
    ✅ 解决: 实施对象池，减少频繁创建销毁对象
    */
} 