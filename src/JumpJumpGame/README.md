# 🦘 跳一跳小游戏 - AI友好版

## 📖 游戏简介

跳一跳是一个使用WasiCore框架基础形状拼凑功能开发的3D平台跳跃游戏。玩家控制一个角色，通过跳跃在不同平台间移动，收集收集品获得分数。

**🎯 AI开发友好特性：**
- 完整的API使用示例
- 清晰的代码结构说明
- 常见问题解决方案
- 框架特性详细解释

## 🎯 游戏特色

- **基础形状拼凑** - 使用框架的AIShapeFactory创建游戏对象
- **3D平台跳跃** - 真实的物理跳跃体验
- **动态平台生成** - 无限的游戏内容
- **收集品系统** - 增加游戏趣味性
- **现代化UI** - 清晰的游戏界面

## 🏗️ 技术实现详解

### 1. 基础形状系统 - 核心功能

#### ✅ 正确的API使用方式
```csharp
// 创建平台（棕色平面）
var platformActor = AIShapeFactory.CreatePlatform(
    new ScenePoint(x, y, 0),
    new Vector3(width / 100f, height / 100f, 1f) // 缩放平台
);

// 创建玩家角色（蓝色胶囊）
var playerActor = AIShapeFactory.CreatePlayer(
    new ScenePoint(playerX, playerY, 0),
    null, // 无作用域
    new Vector3(PLAYER_SIZE / 100f, PLAYER_SIZE / 100f, PLAYER_SIZE / 100f)
);

// 创建收集品（黄色圆锥）
var collectibleActor = AIShapeFactory.CreateCollectible(
    new ScenePoint(collectibleX, collectibleY, 0),
    null, // 无作用域
    new Vector3(COLLECTIBLE_SIZE / 100f, COLLECTIBLE_SIZE / 100f, COLLECTIBLE_SIZE / 100f)
);
```

#### 🔍 形状系统特性说明
- **智能颜色分配** - 框架自动为不同形状分配合适的颜色
- **标准化尺寸** - 所有形状基于100单位的基准尺寸
- **灵活缩放** - 通过Vector3参数精确控制形状大小
- **自动注册** - 创建的Actor自动添加到游戏场景

### 2. UI系统 - 重要概念澄清

#### ⚠️ 关键理解：Layout vs 直接属性
```csharp
// ❌ 错误理解：Layout属性在UI对象中不存在
gameCanvas.Layout = new() { ... };  // 这会导致编译错误

// ✅ 正确理解：布局属性直接属于UI对象
var panel = new Panel()
{
    Width = 800,           // 直接属性
    Height = 600,          // 直接属性
    Background = new SolidColorBrush(Color.Blue),  // 直接属性
    Parent = parentContainer  // 直接属性
};

var label = new Label()
{
    Text = "Hello World",   // 直接属性
    TextColor = Color.White, // 直接属性（不是Foreground）
    FontSize = 24,          // 直接属性
    Parent = panel          // 直接属性
};
```

#### 📚 UI组件属性对照表
| 属性类型 | 正确用法 | 错误用法 | 说明 |
|---------|---------|---------|------|
| 尺寸 | `Width`, `Height` | `Layout.Width` | 直接设置 |
| 颜色 | `TextColor`, `Background` | `Foreground` | 使用特定属性名 |
| 可见性 | `Visible` | `Visibility` | 布尔值，不是枚举 |
| 父容器 | `Parent` | `Children.Add()` | 设置父容器关系 |

### 3. 游戏框架集成 - 完整示例

#### 🎮 游戏类注册
```csharp
public class JumpJump : IGameClass, IThinker
{
    public static void OnRegisterGameClass()
    {
        Game.Logger.LogInformation("Jump Jump game registered");
        Game.OnGameTriggerInitialization += RegisterAll;
    }

    public static void RegisterAll()
    {
        // 检查游戏模式
        if (Game.GameModeLink != ScopeData.GameMode.JumpJump)
            return;

        // 使用Trigger系统注册游戏启动事件
        Trigger<EventGameStart> trigger = new(async (s, d) =>
        {
            Game.Logger.LogInformation("Jump Jump game started");
            var game = new JumpJump();
            game.Initialize();
            return true;
        });
        trigger.Register(Game.Instance);
    }
}
```

#### ⚡ 事件系统使用
```csharp
// 键盘事件监听
Trigger<EventGameKeyDown> keyDownTrigger = new(async (s, d) =>
{
    if (d.Key == GameCore.Platform.SDL.VirtualKey.Space && !isJumping && isGameRunning)
    {
        StartJump();
        return true;
    }
    return false;
});
keyDownTrigger.Register(Game.Instance);

// 鼠标事件监听
gameCanvas.OnPointerPressed += OnCanvasClicked;
```

#### 🧠 思考器系统
```csharp
public void Think(int deltaTime)
{
    // deltaTime是毫秒，需要转换为秒
    UpdateGame(deltaTime / 1000f);
}

// 启用/禁用思考器
(this as IThinker).DoesThink = true;   // 启用
(this as IThinker).DoesThink = false;  // 禁用
```

## 🎨 视觉设计系统

### 形状配置详解
```csharp
// 平台 - 棕色平面，可缩放
var platform = AIShapeFactory.CreatePlatform(
    position: new ScenePoint(x, y, 0),
    scale: new Vector3(width / 100f, height / 100f, 1f)
);

// 玩家 - 蓝色胶囊，中等大小
var player = AIShapeFactory.CreatePlayer(
    position: new ScenePoint(playerX, playerY, 0),
    scope: null,  // 无作用域
    scale: new Vector3(PLAYER_SIZE / 100f, PLAYER_SIZE / 100f, PLAYER_SIZE / 100f)
);

// 收集品 - 黄色圆锥，小尺寸
var collectible = AIShapeFactory.CreateCollectible(
    position: new ScenePoint(collectibleX, collectibleY, 0),
    scope: null,  // 无作用域
    scale: new Vector3(COLLECTIBLE_SIZE / 100f, COLLECTIBLE_SIZE / 100f, COLLECTIBLE_SIZE / 100f)
);
```

### 颜色主题系统
```csharp
// 使用智能颜色系统
AIShapeFactory.DefaultColorMode = ShapeColorMode.SmartDefaults;
AIShapeFactory.DefaultColorTheme = ShapeColorTheme.Standard;

// 自定义颜色主题
var customActor = AIShapeFactory.CreateShape(
    PrimitiveShape.Cube,
    new ScenePoint(x, y, 0),
    ShapeColorMode.SmartDefaults,
    ShapeColorTheme.Gaming  // 游戏主题
);
```

## 🚀 运行和调试指南

### 1. 编译项目
```bash
# 编译服务端调试版本
dotnet build WasiAsync.sln -c Server-Debug

# 编译客户端调试版本  
dotnet build WasiAsync.sln -c Client-Debug
```

### 2. 启动游戏
游戏已设置为默认测试模式，启动后会自动进入跳一跳游戏。

### 3. 切换游戏模式
```csharp
// 在 Tests/Game/GlobalConfig.cs 中修改
GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.JumpJump;    // 跳一跳
GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.FlappyBird;  // 小鸟
GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.Game2048;    // 2048
```

## 🔧 自定义和扩展

### 游戏参数调整
```csharp
// 游戏常量 - 可根据需要调整
private const float GRAVITY = 2000f;           // 重力加速度
private const float JUMP_FORCE = 800f;         // 跳跃力度
private const float MAX_JUMP_TIME = 0.3f;      // 最大跳跃时间
private const float PLATFORM_SPACING = 300f;   // 平台间距
private const float PLATFORM_WIDTH = 200f;     // 平台宽度
private const float PLATFORM_HEIGHT = 50f;     // 平台高度
private const float PLAYER_SIZE = 60f;         // 玩家大小
private const float COLLECTIBLE_SIZE = 40f;    // 收集品大小
```

### 形状系统扩展
```csharp
// 创建复合形状
var robot = AIShapeComposer.CreateRobotCharacter(
    position: new ScenePoint(x, y, 0),
    scope: null,
    scale: 1.5f
);

// 批量创建形状
var enemies = AIShapeFactory.CreateShapes(
    PrimitiveShape.Sphere,
    enemyPositions,
    scope: null
);

// 网格布局创建
var collectibles = AIShapeFactory.CreateShapeGrid(
    PrimitiveShape.Cone,
    centerPos,
    gridSize: (5, 3),
    spacing: 120f
);
```

## 🐛 常见问题解决

### 1. 编译错误：Layout属性不存在
```csharp
// ❌ 错误
gameCanvas.Layout = new() { Width = 800, Height = 600 };

// ✅ 正确
gameCanvas.Width = 800;
gameCanvas.Height = 600;
```

### 2. 编译错误：Visibility属性不存在
```csharp
// ❌ 错误
gameOverLabel.Visibility = Visibility.Collapsed;

// ✅ 正确
gameOverLabel.Visible = false;
```

### 3. 编译错误：ScenePoint参数类型不匹配
```csharp
// ❌ 错误：第三个参数应该是Scene类型
new ScenePoint(x, y, 0)

// ✅ 正确：使用Scene参数或默认场景
new ScenePoint(x, y, 0, Game.CurrentScene)
```

### 4. 运行时错误：Game.RemoveActor不存在
```csharp
// ❌ 错误
Game.RemoveActor(actor);

// ✅ 正确：使用Actor的Dispose方法
actor.Dispose();
// 或者
Game.RemoveControl(actor); // 如果是UI控件
```

## 📁 完整文件结构

```
Tests/Game/JumpJumpGame/
├── JumpJump.cs          # 主游戏逻辑
└── README.md            # 游戏说明文档（本文件）
```

## 🎯 扩展建议

### 1. 增加游戏元素
- 移动平台
- 特殊收集品（双倍分数、生命值等）
- 障碍物
- 背景装饰

### 2. 增强视觉效果
- 粒子效果
- 动画系统
- 光照效果
- 后处理效果

### 3. 游戏机制扩展
- 多种跳跃模式
- 技能系统
- 关卡系统
- 成就系统

## 📝 开发日志

- **v1.0.0** - 基础游戏功能实现
  - 基础跳跃机制
  - 平台生成系统
  - 收集品系统
  - 基础UI界面
- **v1.1.0** - 文档优化和API澄清
  - 修正UI系统使用说明
  - 添加常见问题解决方案
  - 完善代码示例

## 🤝 贡献指南

欢迎提交Issue和Pull Request来改进这个游戏！

**特别欢迎：**
- API使用示例的改进
- 常见问题的解决方案
- 代码结构的优化建议
- 文档的完善和修正

## 📄 许可证

本项目遵循WasiCore框架的许可证条款。

## 🔗 相关资源

- [WasiCore框架概述](../FRAMEWORK_OVERVIEW.md)
- [基础形状系统总结](../../SHAPE_SYSTEM_SUMMARY.md)
- [用户实现指南](../../USER_IMPLEMENTATION_GUIDE.md)
- [FlappyBird游戏示例](../FlappyBirdGame/FlappyBird.cs) - 参考实现
