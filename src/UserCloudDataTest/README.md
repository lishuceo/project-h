# UserCloudDataTest 游戏模式

这是一个专门用于测试新包装的 `CloudData` 系统全面功能的游戏模式。

## 功能概述

这个测试模式提供了全面的新 `CloudData` API功能测试，专门针对框架重写后的用户友好包装API进行验证。

### 🔧 测试功能

#### 1. **API入口点测试**
   - **CloudDataApi.ForUser** - 测试主要API入口点
   - **CloudData.ForUser** - 测试便利别名入口点  
   - **链式API调用** - 验证流畅的方法链调用体验
   - **初始化数据** - 设置测试所需的基础数据

#### 2. **专门查询API测试**
   - **QueryUserDataAsync** - 通用用户数据批量查询
   - **QueryCurrencyAsync** - 专门的货币数据查询
   - **QueryCappedDataAsync** - 专门的上限数据查询
   - **批量用户查询** - 多用户数据同时查询优化
   - **结果验证** - 查询结果的完整性验证

#### 3. **UserData包装类测试**
   - **GetByUserId** - 按用户ID筛选数据功能
   - **GetByKey** - 按数据键筛选数据功能
   - **HasUserId** - 用户存在性检查功能
   - **数据分组** - 验证结果数据的分组和过滤能力

#### 4. **批量操作测试**
   - **单用户多操作** - 在一个事务中执行多个操作
   - **批量查询性能** - 多用户多键查询的性能表现
   - **货币批量查询** - 专门针对货币的批量查询优化

#### 5. **事务优化功能测试**
   - **操作自动合并** - 相同键的操作智能合并
   - **事务验证** - 参数和数据完整性验证
   - **自定义描述** - 事务描述和日志记录功能

#### 6. **CappedData上限数据测试**
   - **ModifyCappedData** - 上限数据修改操作
   - **QueryCappedDataAsync** - 上限数据专门查询
   - **重置选项** - 每日、每周、每月等重置策略
   - **上限验证** - 超出上限时的错误处理

#### 7. **列表项操作测试**
   - **PrepareListItem** - 列表项引用准备
   - **AddListItem** - 列表项添加操作
   - **QueryUserListItemsAsync** - 列表项查询功能
   - **FindListItemByIdAsync** - 通过全局ID精确查找列表项（包括找到和未找到测试）
   - **UpdateListItem** - 列表项更新功能
   - **DeleteListItem** - 列表项删除功能
   - **ID验证** - 生成的列表项ID正确性验证

#### 8. **错误处理改进测试**
   - **参数验证** - 无效用户ID、空键名等参数验证（空键名现在直接抛出ArgumentException）
   - **货币不足处理** - 余额不足时的错误处理
   - **空数组查询** - 空参数数组的错误处理
   - **异常捕获** - 各种异常情况的正确捕获

#### 9. **性能和便利性测试**
   - **链式调用性能** - 多操作链式调用的执行效率
   - **复杂查询性能** - 大量数据查询的性能表现
   - **并发操作测试** - 多个并发事务的处理能力
   - **数据一致性** - 操作后数据状态的一致性验证

## 🎮 如何运行

1. 启动游戏引擎
2. 游戏模式设置为 `UserCloudDataTest`
3. 游戏启动后会自动按阶段运行所有测试
4. 查看控制台日志获取详细测试结果

## 📊 测试结果

测试运行后会在控制台输出详细的测试报告，包括：

- ✅ **通过的测试用例** - 成功执行的功能测试
- ❌ **失败的测试用例** - 需要关注的问题和错误
- 📊 **测试统计信息** - 总体通过率和执行摘要
- ⏱️ **运行时间** - 各阶段和总体执行时间
- 🔍 **详细结果** - 每个测试的具体执行情况

## 🔍 测试数据

测试使用以下模拟数据：

- **测试用户**: `12345`, `67890`, `11111`
- **数据键名**: `level`, `experience`, `player_name`, `settings`
- **货币类型**: `gold`, `diamond`, `energy`
- **上限数据**: `daily_quest_attempts`, `weekly_activity`, `monthly_pvp_score`
- **列表键名**: `inventory`, `friends`
- **示例数据**: 整数、字符串、二进制数据、货币、列表项等

## 🆕 新功能测试重点

### API统一性
- 验证 `CloudDataApi` 和 `CloudData` 两种入口的一致性
- 测试链式API的流畅性和可读性

### 类型安全
- `UserData<T>` 包装类的类型安全功能
- 专门查询API的返回类型验证
- 正确区分 `UserCloudDataResult<T>.IsSuccess` vs `UserCloudDataResult == Success`

### 性能优化
- 批量操作的性能提升验证
- 事务优化功能的效果测试
- 并发操作的稳定性测试

### 用户体验
- 错误处理的友好性和明确性
- API使用的便利性和直观性

### CappedData功能
- 上限数据的完整生命周期管理
- 重置机制的正确性验证
- 体力、任务次数等游戏常见场景

## 🛠️ 自定义测试

如果需要添加新的测试用例，可以在 `UserCloudDataTestMode.cs` 中添加新的测试方法：

1. 在适当的测试阶段方法中添加新的测试逻辑
2. 使用 `TestStats.LogResult()` 记录测试结果
3. 使用 `Game.Logger.LogInformation()` 记录测试过程
4. 处理异常并记录错误信息

### 测试方法规范

每个测试方法应该：
```csharp
try
{
    // 执行测试逻辑
    var result = await CloudData.ForUser(userId)...;
    
    // 记录测试结果 - 注意正确的类型检查
    // 对于 UserCloudDataResult<T> 使用 .IsSuccess
    // 对于 UserCloudDataResult 使用 == UserCloudDataResult.Success
    TestStats.LogResult("测试名称", result.IsSuccess, // 或 result == UserCloudDataResult.Success
        $"详细信息: {result}");
}
catch (Exception ex)
{
    TestStats.LogResult("测试名称", false, ex.Message);
}
```

## 📝 注意事项

- 测试需要有效的 `IUserCloudDataEngineProvider` 实现
- 云数据功能仅在服务端可用，客户端会跳过测试
- 新的API专注于类型安全和用户体验优化
- 所有测试都使用新的包装API，不再使用旧的直接Provider调用

### 重要API区别

- **方法签名**: `UpdateListItem(itemId, data)` 和 `DeleteListItem(itemId)` - 使用全局唯一ID，无需指定列表键
- **属性名称**: `IListItemRecord.ItemUUID` (不是 `Id`)
- **类型检查**: `UserCloudDataResult<T>.IsSuccess` vs `UserCloudDataResult == Success`
- **参数类型**: 需要显式转换为 `VarChar180[]`
- **键名验证**: 不允许空字符串或纯空白字符串作为key，会直接抛出 `ArgumentException`

## 🔄 与旧版本对比

### 主要改进
- **统一入口点**: `CloudDataApi` 和 `CloudData` 提供一致的访问方式
- **类型安全**: `UserData<T>` 包装类提供强类型支持
- **链式API**: 流畅的方法链调用体验
- **专门查询**: 针对不同数据类型的优化查询API
- **智能优化**: 自动事务合并和验证功能
- **错误处理**: 更友好和明确的错误信息
- **CappedData**: 完整的上限数据管理功能

### 测试覆盖
新的测试模式全面覆盖了重写后API的各个方面，确保新包装层的稳定性和易用性。通过9个阶段的系统性测试，验证了从基础操作到高级功能的完整API生态系统。

## 🚨 常见问题解决

### 编译错误
如果遇到编译错误，请检查：
1. 是否正确使用了 `.IsSuccess` (仅限泛型版本) 或 `== Success` (枚举版本)
2. 是否正确转换了字符串数组为 `VarChar180[]`
3. 是否使用了正确的方法签名 (`UpdateListItem(itemId, data)`, `DeleteListItem(itemId)`)
4. 是否使用了正确的属性名称 (`ItemUUID`)

### 运行时错误
常见的运行时问题：
1. 确保在服务端环境运行
2. 确保有有效的云数据提供者
3. 检查用户ID和键名的有效性
4. 注意上限数据的边界条件 