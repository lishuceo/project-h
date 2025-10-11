using Events;
using GameCore.BaseInterface;
using GameCore.Event;
using TriggerEncapsulation.Event;

#if SERVER
using EngineInterface.BaseType;
using EngineInterface.BaseInterface;
using EngineInterface.Enum;
using GameCore.UserCloudData;
using GameCore.PlayerAndUsers;
using System.Text;
using static GameCore.UserCloudData.CloudDataApi;
#endif

namespace GameEntry.UserCloudDataTest;

/// <summary>
/// 全新的CloudData系统功能测试游戏模式
/// </summary>
/// <remarks>
/// 这个游戏模式专门用于测试新包装的CloudData系统的各种功能，包括：
/// - 新的CloudDataApi和CloudData入口点
/// - ForUser vs ForPlayer便利方法
/// - 专门的查询API (QueryCurrency, QueryCappedData等)
/// - UserData&lt;T&gt; 结果包装类
/// - 批量操作和事务优化
/// - CappedData上限数据操作
/// - 改进的错误处理和参数验证
/// - 列表项操作和PrepareListItem
/// </remarks>
public class UserCloudDataTestMode : IGameClass
{
    /// <summary>
    /// 测试状态统计
    /// </summary>
    public static class TestStats
    {
        public static bool IsInitialized { get; set; } = false;
        public static int TestsCompleted { get; set; } = 0;
        public static int TestsPassed { get; set; } = 0;
        public static int TestsFailed { get; set; } = 0;
        public static DateTime StartTime { get; set; }
        public static readonly List<string> TestResults = [];
        
        public static void Reset()
        {
            IsInitialized = false;
            TestsCompleted = 0;
            TestsPassed = 0;
            TestsFailed = 0;
            StartTime = DateTime.Now;
            TestResults.Clear();
        }
        
        public static void LogResult(string testName, bool passed, string? details = null)
        {
            TestsCompleted++;
            if (passed)
            {
                TestsPassed++;
                Game.Logger.LogInformation("✅ {TestName} - PASSED {Details}", testName, details ?? "");
                TestResults.Add($"✅ {testName} - PASSED {details ?? ""}");
            }
            else
            {
                TestsFailed++;
                Game.Logger.LogError("❌ {TestName} - FAILED {Details}", testName, details ?? "");
                TestResults.Add($"❌ {testName} - FAILED {details ?? ""}");
            }
        }
        
        public static void PrintSummary()
        {
            var elapsed = DateTime.Now - StartTime;
            Game.Logger.LogInformation("🧪 UserCloudData测试阶段完成");
            Game.Logger.LogInformation("⏱️ 运行时间: {Elapsed:mm\\:ss\\.fff}", elapsed);
            Game.Logger.LogInformation("📊 测试统计: {Passed}/{Total} 通过, {Failed} 失败", TestsPassed, TestsCompleted, TestsFailed);
            
            if (TestResults.Count > 0)
            {
                Game.Logger.LogInformation("📋 详细结果:");
                foreach (var result in TestResults)
                {
                    Game.Logger.LogInformation("   {Result}", result);
                }
            }
        }
    }

#if SERVER
    /// <summary>
    /// 测试用户数据
    /// </summary>
    public static class TestData
    {
        public const long TestUserId1 = 100;
        public const long TestUserId2 = 101;
        public const long TestUserId3 = 102;
        public static readonly long[] TestUserIds = [TestUserId1, TestUserId2, TestUserId3];
        
        // 测试键名
        public const string LevelKey = "level";
        public const string ExperienceKey = "experience";
        public const string PlayerNameKey = "player_name";
        public const string SettingsKey = "settings";
        
        // 货币键名
        public const string GoldKey = "gold";
        public const string DiamondKey = "diamond";
        public const string EnergyKey = "energy";
        
        // 上限数据键名
        public const string DailyQuestKey = "daily_quest_attempts";
        public const string WeeklyActivityKey = "weekly_activity";
        public const string MonthlyPvPKey = "monthly_pvp_score";
        
        // 列表键名
        public const string InventoryKey = "inventory";
        public const string FriendsKey = "friends";
        public const string WarehouseKey = "warehouse";
        
        // 名称键名
        public const string GuildNameKey = "guild_names";
        public const string PlayerNicknameKey = "player_nicknames";
        
        // 测试常量
        public const long InitialGold = 1000L;
        public const long InitialDiamonds = 100L;
        public const long InitialEnergy = 50L;
        
        public static readonly byte[] SampleItemData = Encoding.UTF8.GetBytes("{\"type\":\"sword\",\"level\":5,\"attack\":100}");
        public static readonly byte[] UpdatedItemData = Encoding.UTF8.GetBytes("{\"type\":\"sword\",\"level\":10,\"attack\":200}");
        public static readonly byte[] SampleItemData2 = Encoding.UTF8.GetBytes("{\"type\":\"potion\",\"heal\":50}");
    }
#endif

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        // 只在UserCloudDataTest游戏模式下运行
        if (Game.GameModeLink != ScopeData.GameMode.UserCloudDataTest)
        {
            return;
        }

#if SERVER
        Game.Logger.LogInformation("🎮 UserCloudDataTest模式已激活 (服务端)");
        Game.Logger.LogInformation("🚀 使用新的CloudData包装API进行全面测试");
        
        // 初始化测试环境
        InitializeTestEnvironment();
        
        // 注册游戏开始事件
        var gameStartTrigger = new Trigger<EventGameStart>(OnGameStart, keepReference: true);
        gameStartTrigger.Register(Game.Instance);
        
        TestStats.IsInitialized = true;
        Game.Logger.LogInformation("🔧 UserCloudDataTest初始化完成");
#else
        Game.Logger.LogInformation("🎮 UserCloudDataTest模式已激活 (客户端)");
        Game.Logger.LogInformation("ℹ️ 云数据功能仅在服务端可用，客户端跳过测试");
        
        // 注册游戏开始事件，但只是简单记录
        var gameStartTrigger = new Trigger<EventGameStart>(OnGameStartClient, keepReference: true);
        gameStartTrigger.Register(Game.Instance);
#endif
    }

#if SERVER
    private static void InitializeTestEnvironment()
    {        
        // 重置测试统计
        TestStats.Reset();
        
        Game.Logger.LogInformation("🧪 测试环境初始化完成");
    }

    private static async Task<bool> OnGameStart(object sender, EventGameStart eventArgs)
    {
        Game.Logger.LogInformation("🚀 UserCloudDataTest开始运行全面测试...");
        
        try
        {
            await StartComprehensiveTests();
            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "💥 测试运行中发生异常");
            TestStats.LogResult("异常处理", false, ex.Message);
            TestStats.PrintSummary();
            return false;
        }
        finally
        {
            TestStats.PrintSummary();
        }
    }

    private static async Task StartComprehensiveTests()
    {
        Game.Logger.LogInformation("📝 开始执行CloudData系统全面功能测试...");
        
        TestStats.Reset();
        TestStats.IsInitialized = true;
        
        // 1. API入口点测试
        Game.Logger.LogInformation("🏁 第一阶段: API入口点测试");
        await TestApiEntryPoints();
        await Game.Delay(TimeSpan.FromSeconds(1));
        
        // 2. 新查询API测试  
        Game.Logger.LogInformation("🔍 第二阶段: 专门查询API测试");
        await TestSpecializedQueryApis();
        await Game.Delay(TimeSpan.FromSeconds(1));
        
        // 3. UserData包装类测试
        Game.Logger.LogInformation("📦 第三阶段: UserData包装类测试");
        await TestUserDataWrapper();
        await Game.Delay(TimeSpan.FromSeconds(1));
        
        // 4. 批量操作测试
        Game.Logger.LogInformation("🔄 第四阶段: 批量操作测试");
        await TestBatchOperations();
        await Game.Delay(TimeSpan.FromSeconds(1));
        
        // 5. 事务优化测试
        Game.Logger.LogInformation("⚡ 第五阶段: 事务优化功能测试");
        await TestTransactionOptimization();
        await Game.Delay(TimeSpan.FromSeconds(1));
        
        // 6. CappedData测试
        Game.Logger.LogInformation("📊 第六阶段: 上限数据功能测试");
        await TestCappedDataOperations();
        await Game.Delay(TimeSpan.FromSeconds(1));
        
        // 7. 列表项操作测试
        Game.Logger.LogInformation("📋 第七阶段: 列表项操作测试");
        await TestListItemOperations();
        await Game.Delay(TimeSpan.FromSeconds(1));
        
        // 8. 名称管理操作测试
        Game.Logger.LogInformation("🏷️ 第八阶段: 名称管理操作测试");
        await TestNameOperations();
        await Game.Delay(TimeSpan.FromSeconds(1));
        
        // 9. 用户名查询测试
        Game.Logger.LogInformation("👤 第九阶段: 用户名查询测试");
        await TestUserNameQuery();
        await Game.Delay(TimeSpan.FromSeconds(1));
        
        // 10. 错误处理改进测试
        Game.Logger.LogInformation("🛡️ 第十阶段: 错误处理改进测试");
        await TestImprovedErrorHandling();
        await Game.Delay(TimeSpan.FromSeconds(1));
        
        // 11. 性能和便利性测试
        Game.Logger.LogInformation("🏎️ 第十一阶段: 性能和便利性测试");
        await TestPerformanceAndConvenience();
        
        // 输出测试摘要
        PrintTestSummary();
    }

    private static void PrintTestSummary()
    {
        TestStats.PrintSummary();
        
        // 添加明显的测试结束标识
        Game.Logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Game.Logger.LogInformation("🏁 USERCLOUDDATA测试已全部完成");
        Game.Logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        
        // 输出测试统计信息
        var successRate = TestStats.TestsCompleted > 0 
            ? (double)TestStats.TestsPassed / TestStats.TestsCompleted * 100 
            : 0;
            
        Game.Logger.LogInformation("📈 测试统计总览:");
        Game.Logger.LogInformation("   ✅ 成功: {Passed} 项", TestStats.TestsPassed);
        Game.Logger.LogInformation("   ❌ 失败: {Failed} 项", TestStats.TestsFailed);
        Game.Logger.LogInformation("   📊 总计: {Total} 项", TestStats.TestsCompleted);
        Game.Logger.LogInformation("   🎯 成功率: {Rate:F1}%", successRate);
        
        // 根据成功率输出结论
        if (successRate >= 90)
        {
            Game.Logger.LogInformation("🎉 测试结果: 优秀！新CloudData包装API运行良好");
        }
        else if (successRate >= 80)
        {
            Game.Logger.LogInformation("✅ 测试结果: 良好！核心功能正常，部分功能需要改进");
        }
        else if (successRate >= 70)
        {
            Game.Logger.LogInformation("⚠️ 测试结果: 合格！主要功能正常，存在一些问题需要解决");
        }
        else
        {
            Game.Logger.LogInformation("❌ 测试结果: 需要改进！发现多个问题，建议检查实现");
        }
        
        Game.Logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Game.Logger.LogInformation("🔚 测试报告结束");
        Game.Logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    private static async Task TestApiEntryPoints()
    {
        Game.Logger.LogInformation("💾 测试CloudDataApi vs CloudData入口点...");

        try
        {
            // 测试CloudDataApi.ForUser
            var cloudDataApiResult = await CloudDataApi.ForUser(TestData.TestUserId1)
                .SetData(TestData.LevelKey, 10L)
                .SetData(TestData.PlayerNameKey, "TestPlayer_CloudDataApi")
                .WithDescription("CloudDataApi入口点测试")
                .ExecuteAsync();
            
            TestStats.LogResult("CloudDataApi.ForUser", cloudDataApiResult == UserCloudDataResult.Success, 
                $"结果: {cloudDataApiResult}");

            // 测试CloudData.ForUser (别名)
            var cloudDataResult = await CloudData.ForUser(TestData.TestUserId1)
                .SetData(TestData.ExperienceKey, 500L)
                .SetData(TestData.SettingsKey, "{\"sound\":true,\"music\":false}")
                .WithDescription("CloudData别名入口点测试")
                .ExecuteAsync();
            
            TestStats.LogResult("CloudData.ForUser别名", cloudDataResult == UserCloudDataResult.Success, 
                $"结果: {cloudDataResult}");

            // 测试CloudDataApi.ForUser(User user) - 新的重载方法
            var testUser = new User(TestData.TestUserId1);
            var cloudDataApiUserResult = await CloudDataApi.ForUser(testUser)
                .SetData("user_test_key", "TestValue_CloudDataApi_User")
                .WithDescription("CloudDataApi.ForUser(User)重载测试")
                .ExecuteAsync();
            
            TestStats.LogResult("CloudDataApi.ForUser(User)", cloudDataApiUserResult == UserCloudDataResult.Success, 
                $"结果: {cloudDataApiUserResult}");

            // 测试CloudData.ForUser(User user) - 新的重载方法
            var cloudDataUserResult = await CloudData.ForUser(testUser)
                .SetData("user_alias_test_key", "TestValue_CloudData_User")
                .WithDescription("CloudData.ForUser(User)重载测试")
                .ExecuteAsync();
            
            TestStats.LogResult("CloudData.ForUser(User)别名", cloudDataUserResult == UserCloudDataResult.Success, 
                $"结果: {cloudDataUserResult}");

            // 初始化货币，为后续测试做准备
            var initCurrencyResult = await CloudData.ForUser(TestData.TestUserId1)
                .AddCurrency(TestData.GoldKey, TestData.InitialGold)
                .AddCurrency(TestData.DiamondKey, TestData.InitialDiamonds)
                .AddCurrency(TestData.EnergyKey, TestData.InitialEnergy)
                .WithDescription("初始化测试货币")
                .ExecuteAsync();
            
            TestStats.LogResult("初始化测试货币", initCurrencyResult == UserCloudDataResult.Success, 
                $"结果: {initCurrencyResult}");
        }
        catch (Exception ex)
        {
            TestStats.LogResult("API入口点测试", false, ex.Message);
        }
    }

    private static async Task TestSpecializedQueryApis()
    {
        Game.Logger.LogInformation("🔍 测试专门的查询API...");

        try
        {
            // 测试QueryUserDataAsync
            var userDataKeys = new VarChar180[] { TestData.LevelKey, TestData.PlayerNameKey, TestData.ExperienceKey, TestData.SettingsKey };
            var userDataResult = await CloudData.QueryUserDataAsync(
                userIds: [TestData.TestUserId1],
                keys: userDataKeys
            );
            
            TestStats.LogResult("QueryUserDataAsync", userDataResult.IsSuccess, 
                $"查询到{userDataResult.Data?.Count() ?? 0}个用户数据");

            // 测试QueryCurrencyAsync 
            var currencyKeys = new VarChar180[] { TestData.GoldKey, TestData.DiamondKey, TestData.EnergyKey };
            var currencyResult = await CloudData.QueryCurrencyAsync(
                userIds: [TestData.TestUserId1],
                keys: currencyKeys
            );
            
            var currencySuccess = currencyResult.IsSuccess;
            var currencyDetails = "";
            if (currencySuccess)
            {
                var userData = currencyResult.Data?.GetByUserId(TestData.TestUserId1);
                if (userData != null)
                {
                    var gold = userData.FirstOrDefault(c => c.Key == TestData.GoldKey)?.Value ?? 0;
                    currencyDetails = $"查询到金币: {gold}";
                }
            }
            
            TestStats.LogResult("QueryCurrencyAsync", currencySuccess, currencyDetails);

            // 批量用户查询测试
            var batchKeys = new VarChar180[] { TestData.LevelKey, TestData.PlayerNameKey };
            var batchResult = await CloudData.QueryUserDataAsync(
                userIds: TestData.TestUserIds,
                keys: batchKeys
            );
            
            TestStats.LogResult("批量用户查询", batchResult.IsSuccess, 
                $"查询{TestData.TestUserIds.Length}个用户，返回{batchResult.Data?.Count() ?? 0}条记录");
        }
        catch (Exception ex)
        {
            TestStats.LogResult("专门查询API测试", false, ex.Message);
        }
    }

    private static async Task TestUserDataWrapper()
    {
        Game.Logger.LogInformation("📦 测试UserData包装类功能...");

        try
        {
            // 准备多个用户的数据
            await CloudData.ForUser(TestData.TestUserId2)
                .SetData(TestData.LevelKey, 15L)
                .SetData(TestData.PlayerNameKey, "TestPlayer2")
                .AddCurrency(TestData.GoldKey, 2000L)
                .WithDescription("准备UserData包装测试数据")
                .ExecuteAsync();

            // 查询并测试UserData功能
            var queryKeys = new VarChar180[] { TestData.GoldKey, TestData.DiamondKey };
            var queryResult = await CloudData.QueryCurrencyAsync(
                userIds: [TestData.TestUserId1, TestData.TestUserId2],
                keys: queryKeys
            );

            if (queryResult.IsSuccess)
            {
                var userData = queryResult.Data;
                
                // 测试GetByUserId
                var user1Data = userData.GetByUserId(TestData.TestUserId1);
                var user1HasData = user1Data != null && user1Data.Any();
                TestStats.LogResult("UserData.GetByUserId", user1HasData, 
                    $"用户1数据条数: {user1Data?.Count() ?? 0}");

                // 测试GetByKey
                var goldData = userData.GetByKey(TestData.GoldKey);
                var goldHasData = goldData != null && goldData.Any();
                TestStats.LogResult("UserData.GetByKey", goldHasData, 
                    $"金币数据条数: {goldData?.Count() ?? 0}");

                // 测试HasUserId
                var hasUser1 = userData.HasUserId(TestData.TestUserId1);
                var hasUser999 = userData.HasUserId(999999L);
                TestStats.LogResult("UserData.HasUserId", hasUser1 && !hasUser999, 
                    $"存在用户1: {hasUser1}, 存在不存在用户: {hasUser999}");
            }
            else
            {
                TestStats.LogResult("UserData包装类测试", false, "查询失败");
            }
        }
        catch (Exception ex)
        {
            TestStats.LogResult("UserData包装类测试", false, ex.Message);
        }
    }

    private static async Task TestBatchOperations()
    {
        Game.Logger.LogInformation("🔄 测试批量操作功能...");

        try
        {
            // 测试批量用户事务
            var batchResult = await CloudData.ForUser(TestData.TestUserId3)
                .SetData(TestData.LevelKey, 20L)
                .AddCurrency(TestData.GoldKey, 500L)
                .SetData(TestData.PlayerNameKey, "BatchTestPlayer")
                .WithDescription("批量操作测试")
                .ExecuteAsync();
            
            TestStats.LogResult("单用户多操作批量", batchResult == UserCloudDataResult.Success, 
                $"结果: {batchResult}");

            // 测试批量查询优化
            var startTime = DateTime.UtcNow;
            var batchQueryKeys = new VarChar180[] { TestData.LevelKey, TestData.PlayerNameKey, TestData.ExperienceKey };
            var batchQueryResult = await CloudData.QueryUserDataAsync(
                userIds: TestData.TestUserIds,
                keys: batchQueryKeys
            );
            var queryTime = DateTime.UtcNow - startTime;
            
            TestStats.LogResult("批量查询性能", batchQueryResult.IsSuccess, 
                $"查询{TestData.TestUserIds.Length}用户耗时: {queryTime.TotalMilliseconds:F1}ms");

            // 测试货币批量查询
            var currencyBatchKeys = new VarChar180[] { TestData.GoldKey, TestData.DiamondKey, TestData.EnergyKey };
            var currencyBatchResult = await CloudData.QueryCurrencyAsync(
                userIds: TestData.TestUserIds,
                keys: currencyBatchKeys
            );
            
            TestStats.LogResult("批量货币查询", currencyBatchResult.IsSuccess, 
                $"查询{TestData.TestUserIds.Length}用户的{currencyBatchKeys.Length}种货币");
        }
        catch (Exception ex)
        {
            TestStats.LogResult("批量操作测试", false, ex.Message);
        }
    }

    private static async Task TestTransactionOptimization()
    {
        Game.Logger.LogInformation("⚡ 测试事务优化功能...");

        try
        {
            // 测试操作合并优化
            var optimizedResult = await CloudData.ForUser(TestData.TestUserId1)
                .AddCurrency(TestData.GoldKey, 100L)   // 这些操作应该被合并
                .AddCurrency(TestData.GoldKey, 50L)    
                .AddCurrency(TestData.GoldKey, -30L)   // 最终结果: +120
                .SetData(TestData.LevelKey, 11L)       // 这些操作应该被优化
                .SetData(TestData.LevelKey, 12L)       // 最终结果: 12
                .WithOptimization(true)                // 启用优化
                .WithDescription("事务优化测试")
                .ExecuteAsync();
            
            TestStats.LogResult("事务自动优化", optimizedResult == UserCloudDataResult.Success, 
                $"结果: {optimizedResult}");

            // 测试验证功能
            var validationResult = await CloudData.ForUser(TestData.TestUserId1)
                .SetData(TestData.ExperienceKey, 1500L)
                .WithValidation(true)                  // 启用验证
                .WithDescription("验证功能测试")
                .ExecuteAsync();
            
            TestStats.LogResult("事务验证功能", validationResult == UserCloudDataResult.Success, 
                $"结果: {validationResult}");

            // 测试描述功能
            var descriptiveResult = await CloudData.ForUser(TestData.TestUserId1)
                .AddCurrency(TestData.DiamondKey, 10L)
                .WithDescription("自定义事务描述测试")
                .ExecuteAsync();
            
            TestStats.LogResult("事务描述功能", descriptiveResult == UserCloudDataResult.Success, 
                $"结果: {descriptiveResult}");
        }
        catch (Exception ex)
        {
            TestStats.LogResult("事务优化测试", false, ex.Message);
        }
    }

    private static async Task TestCappedDataOperations()
    {
        Game.Logger.LogInformation("📊 测试上限数据功能...");

        try
        {
            // 测试ModifyCappedData操作
            var cappedResult = await CloudData.ForUser(TestData.TestUserId1)
                .ModifyCappedData(TestData.DailyQuestKey, 1, 10, UserDataResetOption.Daily())
                .ModifyCappedData(TestData.WeeklyActivityKey, 5, 100, UserDataResetOption.Weekly())
                .WithDescription("上限数据操作测试")
                .ExecuteAsync();
            
            TestStats.LogResult("ModifyCappedData操作", cappedResult == UserCloudDataResult.Success, 
                $"结果: {cappedResult}");

            // 测试QueryCappedDataAsync
            var cappedQueryKeys = new VarChar180[] { TestData.DailyQuestKey, TestData.WeeklyActivityKey, TestData.MonthlyPvPKey };
            var cappedQueryResult = await CloudData.QueryCappedDataAsync(
                userIds: [TestData.TestUserId1],
                keys: cappedQueryKeys
            );
            
            var cappedQuerySuccess = cappedQueryResult.IsSuccess;
            var cappedDetails = "";
            if (cappedQuerySuccess)
            {
                var userData = cappedQueryResult.Data?.GetByUserId(TestData.TestUserId1);
                if (userData != null)
                {
                    var dailyQuest = userData.FirstOrDefault(c => c.Key == TestData.DailyQuestKey);
                    if (dailyQuest != null)
                    {
                        cappedDetails = $"每日任务: {dailyQuest.Value}/{dailyQuest.Cap}";
                    }
                }
            }
            
            TestStats.LogResult("QueryCappedDataAsync", cappedQuerySuccess, cappedDetails);

            // 测试上限达到情况
            var cappedLimitResult = await CloudData.ForUser(TestData.TestUserId1)
                .ModifyCappedData(TestData.DailyQuestKey, 15, 10, UserDataResetOption.Daily()) // 超出上限
                .WithDescription("测试上限限制")
                .ExecuteAsync();
            
            // 这应该失败，因为超出了上限，并返回CapExceeded错误
            var isCapExceeded = cappedLimitResult == UserCloudDataResult.CapExceeded;
            TestStats.LogResult("上限数据限制测试", isCapExceeded, 
                $"超出上限操作结果（应该返回CapExceeded）: {cappedLimitResult}");

            // 测试ResetCappedData操作
            var resetResult = await CloudData.ForUser(TestData.TestUserId1)
                .ResetCappedData(TestData.DailyQuestKey)
                .WithDescription("重置上限数据测试")
                .ExecuteAsync();
            
            TestStats.LogResult("ResetCappedData操作", resetResult == UserCloudDataResult.Success, 
                $"重置结果: {resetResult}");

            // 验证重置后的值
            var resetQueryResult = await CloudData.QueryCappedDataAsync(
                userIds: [TestData.TestUserId1],
                keys: [TestData.DailyQuestKey]
            );
            
            if (resetQueryResult.IsSuccess)
            {
                var userData = resetQueryResult.Data?.GetByUserId(TestData.TestUserId1);
                var resetedData = userData?.FirstOrDefault(c => c.Key == TestData.DailyQuestKey);
                var resetSuccess = resetedData?.Value == 0;
                TestStats.LogResult("重置数据验证", resetSuccess, 
                    $"重置后值: {resetedData?.Value ?? -1} (应该为0)");
            }
        }
        catch (Exception ex)
        {
            TestStats.LogResult("上限数据测试", false, ex.Message);
        }
    }

    private static async Task TestListItemOperations()
    {
        Game.Logger.LogInformation("📋 测试列表项操作...");

        try
        {
            // 测试PrepareListItem和AddListItem
            var builder = CloudData.ForUser(TestData.TestUserId1);
            var itemRef1 = builder.PrepareListItem(TestData.InventoryKey, TestData.SampleItemData);
            var itemRef2 = builder.PrepareListItem(TestData.InventoryKey, TestData.SampleItemData2);

            var addItemResult = await builder
                .AddListItem(itemRef1)
                .AddListItem(itemRef2)
                .WithDescription("添加列表项测试")
                .ExecuteAsync();
            
            var item1Id = itemRef1.Id;
            var item2Id = itemRef2.Id;
            TestStats.LogResult("PrepareListItem和AddListItem", addItemResult == UserCloudDataResult.Success, 
                $"添加2个物品，ID: {item1Id}, {item2Id}");

            // 测试QueryUserListItemsAsync
            var listQueryResult = await CloudData.QueryUserListItemsAsync(
                userId: TestData.TestUserId1,
                key: TestData.InventoryKey,
                maxCount: 50
            );
            
            var listSuccess = listQueryResult.IsSuccess;
            var listDetails = "";
            if (listSuccess)
            {
                var items = listQueryResult.Data;
                listDetails = $"查询到{items?.Count()}个物品";
                
                // 验证新添加的物品是否存在
                var hasItem1 = items?.Any(item => item.ItemUUID == item1Id);
                var hasItem2 = items?.Any(item => item.ItemUUID == item2Id);
                listDetails += $", 包含物品1: {hasItem1}, 包含物品2: {hasItem2}";
            }
            
            TestStats.LogResult("QueryUserListItemsAsync", listSuccess, listDetails);

            // 测试FindListItemByIdAsync（通过全局ID查找）
            if (item1Id > 0)
            {
                var findResult = await CloudData.FindListItemByIdAsync(item1Id);
                
                if (findResult.IsSuccess)
                {
                    var foundStatus = findResult.Data;
                    if (foundStatus.IsFound)
                    {
                        var foundItem = foundStatus.Record;
                        var itemMatches = foundItem.ItemUUID == item1Id && 
                                        foundItem.UserId == TestData.TestUserId1 && 
                                        foundItem.ListKey == TestData.InventoryKey;
                        TestStats.LogResult("FindListItemByIdAsync找到项目", itemMatches, 
                            $"找到物品{item1Id}，用户: {foundItem.UserId}, 列表: {foundItem.ListKey}");
                    }
                    else
                    {
                        TestStats.LogResult("FindListItemByIdAsync找到项目", false, 
                            "查询成功但项目未找到");
                    }
                }
                else
                {
                    TestStats.LogResult("FindListItemByIdAsync找到项目", false, 
                        $"查询失败: {findResult.Result}");
                }
            }

            // 测试FindListItemByIdAsync未找到的情况
            var notFoundResult = await CloudData.FindListItemByIdAsync(999999999L); // 不存在的ID
            
            if (notFoundResult.IsSuccess)
            {
                var notFoundStatus = notFoundResult.Data;
                var correctlyNotFound = !notFoundStatus.IsFound;
                TestStats.LogResult("FindListItemByIdAsync未找到项目", correctlyNotFound, 
                    $"不存在ID查询结果正确: 未找到 = {!notFoundStatus.IsFound}");
            }
            else
            {
                TestStats.LogResult("FindListItemByIdAsync未找到项目", false, 
                    $"查询失败: {notFoundResult.Result}");
            }

            // 测试UpdateListItem（使用全局唯一ID）
            if (item1Id > 0)
            {
                var updateResult = await CloudData.ForUser(TestData.TestUserId1)
                    .UpdateListItem(item1Id, TestData.UpdatedItemData)
                    .WithDescription("更新列表项测试")
                    .ExecuteAsync();
                
                TestStats.LogResult("UpdateListItem", updateResult == UserCloudDataResult.Success, 
                    $"更新物品{item1Id}结果: {updateResult}");
            }

            // 测试MoveListItem（将物品从背包移动到仓库）
            if (item1Id > 0)
            {
                var moveResult = await CloudData.ForUser(TestData.TestUserId1)
                    .MoveListItem(item1Id, TestData.WarehouseKey)
                    .WithDescription("移动列表项测试")
                    .ExecuteAsync();
                
                TestStats.LogResult("MoveListItem", moveResult == UserCloudDataResult.Success, 
                    $"移动物品{item1Id}到仓库结果: {moveResult}");

                // 验证物品确实被移动了
                var warehouseQueryResult = await CloudData.QueryUserListItemsAsync(
                    userId: TestData.TestUserId1,
                    key: TestData.WarehouseKey,
                    maxCount: 50
                );
                
                if (warehouseQueryResult.IsSuccess)
                {
                    var warehouseItems = warehouseQueryResult.Data;
                    var itemMoved = warehouseItems?.Any(item => item.ItemUUID == item1Id) ?? false;
                    TestStats.LogResult("MoveListItem验证", itemMoved, 
                        $"物品{item1Id}在仓库中: {itemMoved}");
                }
            }

            // 测试DeleteListItem（使用全局唯一ID）
            if (item2Id > 0)
            {
                var deleteResult = await CloudData.ForUser(TestData.TestUserId1)
                    .DeleteListItem(item2Id)
                    .WithDescription("删除列表项测试")
                    .ExecuteAsync();
                
                TestStats.LogResult("DeleteListItem", deleteResult == UserCloudDataResult.Success, 
                    $"删除物品{item2Id}结果: {deleteResult}");
            }
        }
        catch (Exception ex)
        {
            TestStats.LogResult("列表项操作测试", false, ex.Message);
        }
    }

    private static async Task TestNameOperations()
    {
        Game.Logger.LogInformation("🏷️ 测试名称管理操作...");

        try
        {
            // 测试ClaimName操作
            var guildName = "AwesomeGuild123";
            var playerNickname = "CoolPlayer456";
            
            var claimResult = await CloudData.ForUser(TestData.TestUserId1)
                .ClaimName(TestData.GuildNameKey, guildName, "测试公会名称")
                .ClaimName(TestData.PlayerNicknameKey, playerNickname, "测试玩家昵称")
                .WithDescription("声明名称测试")
                .ExecuteAsync();
            
            TestStats.LogResult("ClaimName操作", claimResult == UserCloudDataResult.Success, 
                $"声明公会名称和玩家昵称结果: {claimResult}");

            // 测试名称重复声明（应该失败）
            var duplicateResult = await CloudData.ForUser(TestData.TestUserId2)
                .ClaimName(TestData.GuildNameKey, guildName, "重复公会名称")
                .WithDescription("重复名称测试")
                .ExecuteAsync();
            
            TestStats.LogResult("重复名称验证", duplicateResult != UserCloudDataResult.Success, 
                $"重复声明结果（应该失败）: {duplicateResult}");

            // 测试DeleteName操作
            var deleteNameResult = await CloudData.ForUser(TestData.TestUserId1)
                .DeleteName(TestData.PlayerNicknameKey, playerNickname)
                .WithDescription("删除名称测试")
                .ExecuteAsync();
            
            TestStats.LogResult("DeleteName操作", deleteNameResult == UserCloudDataResult.Success, 
                $"删除玩家昵称结果: {deleteNameResult}");

            // 验证删除后可以重新声明
            var reclaimResult = await CloudData.ForUser(TestData.TestUserId2)
                .ClaimName(TestData.PlayerNicknameKey, playerNickname, "重新声明昵称")
                .WithDescription("重新声明测试")
                .ExecuteAsync();
            
            TestStats.LogResult("删除后重新声明", reclaimResult == UserCloudDataResult.Success, 
                $"删除后重新声明结果: {reclaimResult}");

            // 清理测试数据
            await CloudData.ForUser(TestData.TestUserId1)
                .DeleteName(TestData.GuildNameKey, guildName)
                .WithDescription("清理测试数据")
                .ExecuteAsync();
                
            await CloudData.ForUser(TestData.TestUserId2)
                .DeleteName(TestData.PlayerNicknameKey, playerNickname)
                .WithDescription("清理测试数据")
                .ExecuteAsync();
        }
        catch (Exception ex)
        {
            TestStats.LogResult("名称管理操作测试", false, ex.Message);
        }
    }

    private static async Task TestUserNameQuery()
    {
        Game.Logger.LogInformation("👤 测试平台用户名查询功能...");

        try
        {
            // QueryUserNameBatchAsync 查询的是平台用户名，不是用户自定义数据
            // 这些是用户在平台注册时使用的用户名，由平台系统管理
            
            // 测试QueryUserNameBatchAsync - 使用CloudDataApi
            var userNameQueryResult = await CloudDataApi.QueryUserNameBatchAsync(TestData.TestUserIds);
            
            var userNameQuerySuccess = userNameQueryResult.IsSuccess;
            var userNameDetails = "";
            if (userNameQueryResult.IsSuccess)
            {
                var userNames = userNameQueryResult.Data;
                var foundUser1 = userNames.GetUserName(TestData.TestUserId1);
                var foundUser2 = userNames.GetUserName(TestData.TestUserId2);
                var foundUser3 = userNames.GetUserName(TestData.TestUserId3);
                
                userNameDetails = $"查询到平台用户名 - 用户1: {foundUser1 ?? "null"}, 用户2: {foundUser2 ?? "null"}, 用户3: {foundUser3 ?? "null"}";
                
                // 平台用户名可能为空（如果用户没有设置平台用户名）
                var hasAnyUserName = !string.IsNullOrEmpty(foundUser1) || !string.IsNullOrEmpty(foundUser2) || !string.IsNullOrEmpty(foundUser3);
                TestStats.LogResult("平台用户名查询结果", hasAnyUserName, userNameDetails);
            }
            
            TestStats.LogResult("QueryUserNameBatchAsync", userNameQuerySuccess, userNameDetails);

            // 测试QueryUserNameBatchAsync - 使用CloudData别名
            var userNameQueryAliasResult = await CloudData.QueryUserNameBatchAsync(TestData.TestUserIds);
            
            var userNameAliasSuccess = userNameQueryAliasResult.IsSuccess;
            var userNameAliasDetails = "";
            if (userNameQueryAliasResult.IsSuccess)
            {
                var userNames = userNameQueryAliasResult.Data;
                var recordCount = userNames.Records.Count();
                userNameAliasDetails = $"别名查询成功，返回{recordCount}条记录";
            }
            
            TestStats.LogResult("CloudData.QueryUserNameBatchAsync别名", userNameAliasSuccess, userNameAliasDetails);

            // 测试单个用户查询
            var singleUserQueryResult = await CloudData.QueryUserNameBatchAsync([TestData.TestUserId1]);
            
            var singleUserSuccess = singleUserQueryResult.IsSuccess;
            var singleUserDetails = "";
            if (singleUserQueryResult.IsSuccess)
            {
                var userNames = singleUserQueryResult.Data;
                var foundUserName = userNames.GetUserName(TestData.TestUserId1);
                singleUserDetails = $"单用户平台用户名查询结果: {foundUserName ?? "null"}";
            }
            
            TestStats.LogResult("单用户平台用户名查询", singleUserSuccess, singleUserDetails);

            // 测试空用户数组查询（应该失败）
            var emptyUserQueryResult = await CloudData.QueryUserNameBatchAsync([]);
            
            TestStats.LogResult("空用户数组查询", !emptyUserQueryResult.IsSuccess, 
                $"空数组查询结果（应该失败）: {emptyUserQueryResult.Result}");

            // 测试不存在的用户查询
            var nonExistentUserQueryResult = await CloudData.QueryUserNameBatchAsync([999999L]);
            
            var nonExistentSuccess = nonExistentUserQueryResult.IsSuccess;
            var nonExistentDetails = "";
            if (nonExistentUserQueryResult.IsSuccess)
            {
                var userNames = nonExistentUserQueryResult.Data;
                var foundUserName = userNames.GetUserName(999999L);
                nonExistentDetails = $"不存在用户查询结果: {foundUserName ?? "null"}";
            }
            
            TestStats.LogResult("不存在用户平台用户名查询", nonExistentSuccess, nonExistentDetails);

            // 测试混合用户查询（存在和不存在）
            var mixedUserIds = new long[] { TestData.TestUserId1, 999999L, TestData.TestUserId2 };
            var mixedQueryResult = await CloudData.QueryUserNameBatchAsync(mixedUserIds);
            
            var mixedSuccess = mixedQueryResult.IsSuccess;
            var mixedDetails = "";
            if (mixedQueryResult.IsSuccess)
            {
                var userNames = mixedQueryResult.Data;
                var foundUser1 = userNames.GetUserName(TestData.TestUserId1);
                var foundNonExistent = userNames.GetUserName(999999L);
                var foundUser2 = userNames.GetUserName(TestData.TestUserId2);
                
                mixedDetails = $"混合查询 - 用户1: {foundUser1 ?? "null"}, 不存在用户: {foundNonExistent ?? "null"}, 用户2: {foundUser2 ?? "null"}";
            }
            
            TestStats.LogResult("混合用户平台用户名查询", mixedSuccess, mixedDetails);

            // 测试性能 - 批量查询多个用户
            var startTime = DateTime.UtcNow;
            var performanceQueryResult = await CloudData.QueryUserNameBatchAsync(TestData.TestUserIds);
            var queryTime = DateTime.UtcNow - startTime;
            
            TestStats.LogResult("平台用户名查询性能", performanceQueryResult.IsSuccess, 
                $"查询{TestData.TestUserIds.Length}个用户耗时: {queryTime.TotalMilliseconds:F1}ms");
        }
        catch (Exception ex)
        {
            TestStats.LogResult("用户名查询测试", false, ex.Message);
        }
    }

    private static async Task TestImprovedErrorHandling()
    {
        Game.Logger.LogInformation("🛡️ 测试改进的错误处理...");

        try
        {
            // 测试参数验证
            try
            {
                var invalidUserResult = await CloudData.ForUser(-1L) // 无效用户ID
                    .SetData(TestData.LevelKey, 1L)
                    .ExecuteAsync();
                TestStats.LogResult("无效用户ID验证", false, "应该抛出异常但没有");
            }
            catch (ArgumentException)
            {
                TestStats.LogResult("无效用户ID验证", true, "正确捕获了ArgumentException");
            }

            // 测试空键名验证
            try
            {
                var emptyKeyResult = await CloudData.ForUser(TestData.TestUserId1)
                    .SetData("", 123L) // 空键名
                    .ExecuteAsync();
                TestStats.LogResult("空键名验证", false, "应该抛出异常但没有");
            }
            catch (ArgumentException)
            {
                TestStats.LogResult("空键名验证", true, "正确捕获了ArgumentException");
            }
            catch (Exception ex)
            {
                TestStats.LogResult("空键名验证", false, $"捕获了意外异常: {ex.GetType().Name}");
            }

            // 测试货币不足错误
            var insufficientResult = await CloudData.ForUser(TestData.TestUserId1)
                .CostCurrency(TestData.GoldKey, 999999L) // 超过余额
                .WithDescription("余额不足测试")
                .ExecuteAsync();
            
            TestStats.LogResult("货币不足处理", insufficientResult != UserCloudDataResult.Success, 
                $"预期失败: {insufficientResult}");

            // 测试空查询数组
            var emptyKeys = new VarChar180[] { };
            var emptyQueryResult = await CloudData.QueryUserDataAsync(
                userIds: [TestData.TestUserId1],
                keys: emptyKeys
            );
            
            TestStats.LogResult("空键数组查询", !emptyQueryResult.IsSuccess, 
                $"预期失败: {emptyQueryResult}");

            // 测试空用户数组查询
            var emptyUserQueryResult = await CloudData.QueryUserDataAsync(
                userIds: [],
                keys: new VarChar180[] { TestData.LevelKey }
            );
            
            TestStats.LogResult("空用户数组查询", !emptyUserQueryResult.IsSuccess, 
                $"预期失败: {emptyUserQueryResult}");
        }
        catch (Exception ex)
        {
            TestStats.LogResult("错误处理改进测试", false, ex.Message);
        }
    }

    private static async Task TestPerformanceAndConvenience()
    {
        Game.Logger.LogInformation("🏎️ 测试性能和便利性功能...");

        try
        {
            // 测试链式调用的便利性
            var startTime = DateTime.UtcNow;
            var chainResult = await CloudData.ForUser(TestData.TestUserId1)
                .SetData(TestData.LevelKey, 25L)
                .SetData(TestData.ExperienceKey, 2500L)
                .AddCurrency(TestData.GoldKey, 100L)
                .AddCurrency(TestData.DiamondKey, 5L)
                .CostCurrency(TestData.EnergyKey, 10L)
                .SetData(TestData.PlayerNameKey, "ChainTestPlayer")
                .WithDescription("链式调用性能测试")
                .ExecuteAsync();
            var chainTime = DateTime.UtcNow - startTime;
            
            TestStats.LogResult("链式调用便利性", chainResult == UserCloudDataResult.Success, 
                $"6个操作耗时: {chainTime.TotalMilliseconds:F1}ms");

            // 测试复杂查询性能
            startTime = DateTime.UtcNow;
            var complexKeys = new VarChar180[] { TestData.LevelKey, TestData.PlayerNameKey, TestData.ExperienceKey, TestData.SettingsKey };
            var complexQueryResult = await CloudData.QueryUserDataAsync(
                userIds: TestData.TestUserIds,
                keys: complexKeys
            );
            var complexQueryTime = DateTime.UtcNow - startTime;
            
            TestStats.LogResult("复杂查询性能", complexQueryResult.IsSuccess, 
                $"查询{TestData.TestUserIds.Length}用户{complexKeys.Length}键耗时: {complexQueryTime.TotalMilliseconds:F1}ms");

            // 测试并发操作
            var tasks = new List<Task<UserCloudDataResult>>();
            for (int i = 0; i < 5; i++)
            {
                var task = CloudData.ForUser(TestData.TestUserId1)
                    .AddCurrency(TestData.GoldKey, 1L)
                    .WithDescription($"并发测试 {i}")
                    .ExecuteAsync();
                tasks.Add(task);
            }

            startTime = DateTime.UtcNow;
            var concurrentResults = await Task.WhenAll(tasks);
            var concurrentTime = DateTime.UtcNow - startTime;
            
            var successCount = concurrentResults.Count(r => r == UserCloudDataResult.Success);
            TestStats.LogResult("并发操作测试", successCount >= 3, // 允许部分失败
                $"5个并发操作，{successCount}个成功，耗时: {concurrentTime.TotalMilliseconds:F1}ms");

            // 测试数据一致性验证
            var currencyKeys = new VarChar180[] { TestData.GoldKey };
            var finalQuery = await CloudData.QueryCurrencyAsync(
                userIds: [TestData.TestUserId1],
                keys: currencyKeys
            );
            
            if (finalQuery.IsSuccess)
            {
                var goldData = finalQuery.Data.GetByUserId(TestData.TestUserId1);
                var currentGold = goldData?.FirstOrDefault(c => c.Key == TestData.GoldKey)?.Value ?? 0;
                TestStats.LogResult("数据一致性验证", currentGold > TestData.InitialGold, 
                    $"最终金币数量: {currentGold} (初始: {TestData.InitialGold})");
            }
        }
        catch (Exception ex)
        {
            TestStats.LogResult("性能和便利性测试", false, ex.Message);
        }
    }
#else
    private static Task<bool> OnGameStartClient(object sender, EventGameStart eventArgs)
    {
        Game.Logger.LogInformation("🎮 UserCloudDataTest (客户端模式)");
        Game.Logger.LogInformation("ℹ️ 云数据功能仅在服务端可用");
        Game.Logger.LogInformation("📝 如需测试云数据功能，请在服务端运行此游戏模式");
        
        TestStats.Reset();
        TestStats.LogResult("客户端模式检查", true, "云数据功能在客户端不可用，这是正常的");
        TestStats.PrintSummary();
        
        return Task.FromResult(true);
    }
#endif
} 