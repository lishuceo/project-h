using GameCore.Container;
using GameCore.Container.Data;
using GameCore.Data;
using GameCore.SceneSystem;
using GameCore.SceneSystem.Data;
using GameCore.SceneSystem.Data.Struct;
using GameData;
using System.Numerics;
using static GameCore.ScopeData;
using GameCore.Struct;
using GameData.Extension;
using GameCore.EntitySystem;
using GameCore.AbilitySystem.Data;
using GameCore.AISystem.Data;
using GameUI.CameraSystem.Data;
using GameCore.TargetingSystem.Data;
using GameCore.ActorSystem.Data;
using GameCore.ActorSystem.Data.Enum;
using GameCore.Behavior;
using GameUI.Brush;
using System;

namespace GameEntry.JsonScopeDataTest;

/// <summary>
/// ARPG AI系统独立测试 🤖
/// 从ARPGScopeData中抽离的AI系统，用于测试AI行为树逻辑
/// </summary>
public class ARPGAISystem : IGameClass
{
    #region AI系统定义
    public static class AI
    {
        // 战斗测试AI - 独立战斗逻辑
        public static readonly GameLink<GameDataAIThinkTree, GameDataAIThinkTree> CombatTest = new("TestCombatAI"u8);
        
        // 怪物AI - 标准怪物行为逻辑
        public static readonly GameLink<GameDataAIThinkTree, GameDataAIThinkTree> MonsterAI = new("TestMonsterAI"u8);
        
        // Boss AI - 高级Boss行为逻辑
        public static readonly GameLink<GameDataAIThinkTree, GameDataAIThinkTree> BossAI = new("TestBossAI"u8);
        
        // 跟随AI - NPC跟随玩家行为
        public static readonly GameLink<GameDataAIThinkTree, GameDataAIThinkTree> FollowAI = new("TestFollowAI"u8);
        
        // 巡逻AI - 守卫巡逻行为
        public static readonly GameLink<GameDataAIThinkTree, GameDataAIThinkTree> PatrolAI = new("TestPatrolAI"u8);
    }
    #endregion

    public static void OnRegisterGameClass()
    {
        // 先移除可能已存在的订阅，避免重复注册
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        Game.Logger.LogInformation("🤖 Initializing ARPG AI System Test...");

        InitializeAI();

        Game.Logger.LogInformation("✅ ARPG AI System Test initialized successfully!");
    }

    /// <summary>
    /// 初始化ARPG AI系统
    /// </summary>
    private static void InitializeAI()
    {
        Game.Logger.LogInformation("🤖 Configuring ARPG AI System...");

        // ========== 创建共享的怪物战斗行为树 ==========
        var monsterCombatBehavior = new GameLink<GameDataAINode, GameDataAINodeSequence>("TestMonsterCombatBehavior"u8);
        
        // 子节点1：扫描敌人节点
        var monsterScanEnemies = new GameLink<GameDataAINode, GameDataAINodeValidateScan>("TestMonsterScanEnemies"u8);
        _ = new GameDataAINodeValidateScan(monsterScanEnemies)
        {
            Name = "测试怪物扫描敌人",
            // 不忽略牵引限制，怪物有活动范围限制
            IgnoreLeash = false,
        };
        
        // 子节点2：对扫描目标施法 - 使用怪物的攻击技能
        var monsterCastAtTarget = new GameLink<GameDataAINode, GameDataAINodeValidateCast>("TestMonsterCastAtTarget"u8);
        _ = new GameDataAINodeValidateCast(monsterCastAtTarget)
        {
            Name = "测试怪物对目标施法",
            DoRecast = true, // 允许重复施法攻击
        };
        
        // 怪物行为树：序列节点（扫描敌人 -> 攻击目标）
        _ = new GameDataAINodeSequence(monsterCombatBehavior)
        {
            Name = "测试怪物战斗行为树",
            // 🐺 怪物AI行为：先扫描敌人，然后攻击目标
            Nodes = [monsterScanEnemies, monsterCastAtTarget],
        };

        // ========== 战斗测试AI配置 ==========
        _ = new GameDataAIThinkTree(AI.CombatTest)
        {
            Name = "测试战斗AI",
            ScanFilters = [new() {
                Required=[UnitRelationship.Enemy, UnitFilter.Unit, UnitRelationship.Visible],
                Excluded=[UnitFilter.Item, UnitState.Invulnerable, UnitState.Dead]
            }],
            CombatBehaviorTree = monsterCombatBehavior, // 使用相同的行为树
        };

        // ========== 怪物AI配置 ==========
        _ = new GameDataAIThinkTree(AI.MonsterAI)
        {
            Name = "测试怪物AI",
            // 🎯 扫描过滤器：寻找可见的敌方单位，排除物品、无敌、死亡状态
            ScanFilters = [new() {
                Required=[UnitRelationship.Enemy, UnitFilter.Unit, UnitRelationship.Visible],
                Excluded=[UnitFilter.Item, UnitState.Invulnerable, UnitState.Dead]
            }],
            // 🎯 优先级排序：优先攻击英雄，然后是普通单位
            ScanSorts = [UnitFilter.Hero, UnitFilter.Unit],
            // 🤖 使用怪物专用的战斗行为树
            CombatBehaviorTree = monsterCombatBehavior,
        };

        // ========== Boss AI配置 ==========
        _ = new GameDataAIThinkTree(AI.BossAI)
        {
            Name = "测试Boss AI",
            // 高级Boss行为：技能释放、阶段变换、特殊攻击模式
            ScanFilters = [new() {
                Required=[UnitRelationship.Enemy, UnitFilter.Unit, UnitRelationship.Visible],
                Excluded=[UnitFilter.Item, UnitState.Invulnerable, UnitState.Dead]
            }],
            ScanSorts = [UnitFilter.Hero, UnitFilter.Unit],
            CombatBehaviorTree = monsterCombatBehavior, // 暂时也使用基础行为树
        };

        // ========== 跟随AI配置 ==========
        _ = new GameDataAIThinkTree(AI.FollowAI)
        {
            Name = "测试跟随AI",
            // NPC跟随玩家：保持距离、协助战斗、避开障碍
            // 这里需要不同的行为树配置，暂时为空
        };

        // ========== 巡逻AI配置 ==========
        _ = new GameDataAIThinkTree(AI.PatrolAI)
        {
            Name = "测试巡逻AI",
            // 守卫巡逻：定点巡逻、警戒、发现入侵者后报警
            // 这里需要不同的行为树配置，暂时为空
        };

        Game.Logger.LogInformation("✅ ARPG AI System configured successfully!");
        Game.Logger.LogInformation("   - 战斗测试AI: 独立的战斗逻辑系统");
        Game.Logger.LogInformation("   - 怪物AI: 标准怪物行为AI，优先攻击英雄");
        Game.Logger.LogInformation("   - Boss AI: 高级Boss行为AI");
        Game.Logger.LogInformation("   - 跟随AI: NPC跟随玩家AI (待扩展)");
        Game.Logger.LogInformation("   - 巡逻AI: 守卫巡逻行为AI (待扩展)");
        Game.Logger.LogInformation("   🤖 关键特性：CombatTest和MonsterAI共享同一个行为树！");
        Game.Logger.LogInformation("   🎯 区别：不同的扫描过滤器和优先级排序");
    }

    /// <summary>
    /// 获取AI配置信息的实用方法
    /// </summary>
    public static void LogAIInfo()
    {
        Game.Logger.LogInformation("📊 AI系统配置信息:");
        // Game.Logger.LogInformation($"   - CombatTest AI ID: {AI.CombatTest.ID}");
        // Game.Logger.LogInformation($"   - Monster AI ID: {AI.MonsterAI.ID}");
        // Game.Logger.LogInformation($"   - Boss AI ID: {AI.BossAI.ID}");
        // Game.Logger.LogInformation($"   - Follow AI ID: {AI.FollowAI.ID}");
        // Game.Logger.LogInformation($"   - Patrol AI ID: {AI.PatrolAI.ID}");
    }

    /// <summary>
    /// 创建测试单位的辅助方法
    /// </summary>
    public static void CreateTestUnitsWithAI()
    {
        Game.Logger.LogInformation("🧪 创建AI测试单位...");
        
        // 这里可以添加创建测试单位的逻辑
        // 使用不同的AI配置来测试行为差异
        
        Game.Logger.LogInformation("   💡 提示：你可以在这里添加单位创建逻辑");
        Game.Logger.LogInformation("   💡 提示：可以测试CombatTest AI vs MonsterAI的行为差异");
    }
}
