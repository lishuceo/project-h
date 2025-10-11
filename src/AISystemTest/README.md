# 🤖 AI System Test Mode

AI系统测试模式是一个专门设计用于验证和演示WasiAsync框架中AI系统功能的游戏模式。

## 🎯 测试目标

这个测试模式专注于验证以下AI系统组件：

### **WaveAI系统**
- ✅ **Guard模式** - 单位守卫指定目标位置
- ✅ **Move模式** - 单位移动到目标并保持位置，包含滞后机制防震荡
- ✅ **Patrol模式** - 单位在两个位置之间巡逻
- ✅ **编队系统** - 群体单位的编队移动和保持
- ✅ **动态目标** - 实时响应移动目标的变化

### **AIThinkTree系统**
- ✅ **个体AI** - 独立单位的AI行为
- ✅ **状态管理** - 用户禁用、移动禁用、死亡禁用的多层控制
- ✅ **协调机制** - 与WaveAI系统的协调工作

## 🚀 如何启动测试

### **1. 设置游戏模式**

**方法A：修改默认测试模式（推荐）**
在 `Tests/Game/GlobalConfig.cs` 中修改：
```csharp
GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.AISystemTest;
```

**方法B：通过命令行参数**
启动时指定游戏模式：
```bash
./Game.exe --gamemode AISystemTest
```

**方法C：代码中动态设置**
```csharp
Game.GameModeLink = ScopeData.GameMode.AISystemTest;
```

### **2. 启动游戏**
启动服务端和客户端，系统将自动：
- 🌍 使用游戏模式的默认场景进行AI测试
- 👥 生成5个测试单位（1个领队 + 4个跟随）
- 🌊 初始化WaveAI系统
- 🔍 运行AIThinkTree验证器
- ▶️ 开始自动化测试序列

## 📋 测试序列

测试按以下时间表自动执行：

| 时间段 | 测试内容 | 描述 |
|--------|----------|------|
| 0-10s | 初始化 | 创建单位，设置AI系统 |
| 10-20s | Guard模式 | 测试守卫行为 |
| 20-30s | Move模式 | 测试移动行为和滞后机制 |
| 30-45s | Patrol模式 | 测试巡逻行为 |
| 45-75s | 动态目标 | 目标单位移动，AI跟随 |
| 75-95s | 编队测试 | 测试不同编队模式 |
| 95s+ | 测试完成 | 所有测试完成 |

## 🎮 测试单位布局

测试环境中会创建以下单位：

```
Player Hero (500, 1000)    →    Guard AI (1000, 1000)
                                Move AI  (1200, 1000)
                                Patrol AI(1400, 1000)
                                Individual AI (1600, 1000)
                                Target Unit (1800, 1000)
```

### **单位说明**
- **Guard AI Unit** - 加入WaveAI，测试Guard模式
- **Move AI Unit** - 加入WaveAI，测试Move模式  
- **Patrol AI Unit** - 加入WaveAI，测试Patrol模式
- **Individual AI Unit** - 独立AIThinkTree，不加入WaveAI
- **Target Unit** - 作为其他AI单位的目标

## 🖥️ 客户端界面

客户端提供实时监控界面，显示：

### **状态信息**
- 当前帧数和游戏时间
- AI系统状态
- 当前测试阶段

### **测试说明**
- 各个测试阶段的详细说明
- 实时观察指导

## 🔧 关键测试验证点

### **WaveAI功能验证**
1. **✅ 单位添加/移除** - 验证单位正确加入和离开Wave
2. **✅ 目标跟踪** - 验证单位正确跟随WaveTarget
3. **✅ 模式切换** - 验证不同模式之间的平滑切换
4. **✅ 编队保持** - 验证编队在移动中的保持
5. **✅ 滞后机制** - 验证Move模式防震荡机制

### **AIThinkTree功能验证**
1. **✅ 状态管理** - 验证多层禁用机制
2. **✅ 协调工作** - 验证与WaveAI的协调
3. **✅ 独立运行** - 验证独立AI的正常工作

### **系统集成验证**
1. **✅ 性能表现** - 验证AI系统的性能影响
2. **✅ 异常处理** - 验证错误情况的处理
3. **✅ 内存管理** - 验证资源的正确清理

## 📊 测试输出

测试过程中，系统会输出详细的日志信息：

```
🤖 AI System Test Mode - Registering triggers
🎯 Initializing AI System Test Environment
🌍 Test scene created: Scene[default]
✅ Created test unit: Guard AI Unit at (1000, 1000)
🌊 Setting up WaveAI tests
✅ Added AIThinkTree to unit: Guard AI Unit
🎯 WaveAI tests configured successfully
🚀 Starting AI test sequence
📋 Testing Guard Mode (default)
📋 Switching to Move Mode
📋 Switching to Patrol Mode
📋 Testing dynamic target movement
🎯 Moving target to (2000, 1000)
🎖️ Enabling formation system
🎖️ Changing formation vector
✅ AI test sequence completed
```

## 🛠️ 自定义测试

您可以修改测试参数来进行自定义测试：

### **修改WaveAI配置**
```csharp
// 在ScopeData.cs中修改默认WaveAI配置
var defaultWaveAI = new GameLink<GameDataWaveAI, GameDataWaveAI>("default"u8);
_ = new GameDataWaveAI(defaultWaveAI)
{
    MoveHysteresisFactor = 0.8f,    // 调整滞后系数
    MinControlDuration = 2.0f,      // 调整最小控制时间
    EnableWaveFormation = true,     // 启用编队
    WaveLeash = 300f,              // 设置群体范围
};
```

### **修改测试序列**
在 `AISystemTestServer.cs` 中的 `StartAITestSequence()` 方法中可以：
- 调整测试时间间隔
- 添加新的测试场景
- 修改测试参数

## 🎯 预期结果

成功的测试应该显示：

1. **所有AI单位正常创建** ✅
2. **WaveAI模式正确切换** ✅
3. **单位响应目标移动** ✅
4. **编队保持正确** ✅
5. **滞后机制防止震荡** ✅
6. **无异常或错误日志** ✅

## 🚨 常见问题

### **Q: AI单位不移动**
**A:** 检查以下项目：
- 单位是否有Walkable组件
- WaveAI是否正确启动思考
- 目标位置是否有效

### **Q: 客户端UI不显示**
**A:** 确认：
- 游戏模式设置正确
- 客户端正确连接到服务端
- UI初始化没有异常

### **Q: 测试序列不执行**
**A:** 检查：
- 服务端日志中的错误信息
- 单位创建是否成功
- WaveAI初始化是否正常

## 📈 扩展建议

这个测试框架可以进一步扩展：

1. **添加战斗测试** - 测试AI在战斗中的表现
2. **性能压力测试** - 测试大量AI单位的性能
3. **异常场景测试** - 测试各种异常情况的处理
4. **交互式控制** - 允许玩家手动控制测试参数

---

**🎉 开始您的AI系统测试之旅！观察AI单位如何智能地响应不同的指令和环境变化。** 