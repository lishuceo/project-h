# AVG系统设计文档

## 🎯 **系统目标**

AVG系统旨在实现两个核心目标：
1. **制作基本的AVG游戏** - 提供完整的AVG游戏开发框架
2. **提供简单的对话系统API** - 供其他游戏模式轻松集成对话功能

## 🏗️ **系统架构**

### **组件层次结构**
```
GameEntry/AVGSystem/
├── ScopeData/              # C#剧本定义
│   ├── AVGScripts.cs       # 主要剧本集合
│   ├── AVGCharacters.cs    # 角色定义
│   ├── AVGScenes.cs        # 场景定义
│   └── AVGDialogs.cs       # 对话库
├── Resources/              # 资源文件
│   ├── Scripts/            # 剧本文件
│   │   ├── XianJian/       # 仙剑奇侠传剧本
│   │   │   ├── Chapter1.avg
│   │   │   ├── Chapter2.avg
│   │   │   └── ...
│   │   └── Common/         # 通用对话
│   │       ├── Tutorial.avg
│   │       └── NPCDialogs.avg
│   ├── Characters/         # 角色立绘
│   │   ├── LiXiaoYao/
│   │   │   ├── normal.png
│   │   │   ├── happy.png
│   │   │   ├── sad.png
│   │   │   └── angry.png
│   │   ├── ZhaoLingEr/
│   │   └── LinYueRu/
│   ├── Backgrounds/        # 背景图片
│   │   ├── YuHangTown.jpg
│   │   ├── ShuShanMountain.jpg
│   │   └── ...
│   ├── Audio/              # 音效和BGM
│   │   ├── BGM/
│   │   └── SFX/
│   └── UI/                # UI素材
│       ├── DialogBox.png
│       ├── ChoiceButton.png
│       └── ...
├── Core/                   # 核心引擎
│   ├── ScriptEngine.cs     # 脚本解析和执行引擎
│   ├── DialogManager.cs    # 对话管理器
│   ├── VariableManager.cs  # 变量和条件系统
│   └── ResourceManager.cs  # 资源加载管理
├── UI/                     # UI组件
│   ├── FullScreenAVGUI.cs  # 全屏AVG模式UI
│   ├── DialogBoxUI.cs      # 对话框UI组件
│   ├── CharacterDisplay.cs # 角色立绘显示
│   └── ChoiceButtons.cs    # 选择分支按钮
├── Data/                   # 数据定义
│   ├── AVGScript.cs        # 剧本数据结构
│   ├── Character.cs        # 角色定义
│   └── Scene.cs           # 场景定义
└── API/                   # 外部调用接口
    ├── SimpleDialog.cs     # 简单对话API
    └── AVGGameMode.cs      # 完整AVG游戏模式
```

## 📝 **剧本定义风格**

### **混合模式设计**

#### **1. 声明式配置（简单对话）**
适用于：NPC对话、商店交互、简单任务对话

```csharp
public static class NPCDialogs
{
    public static readonly AVGDialog JiangZiYaIntro = new()
    {
        Speaker = "姜子牙",
        Lines = [
            "年轻人，你愿意学习仙术吗？",
            "我观你骨骼精奇，是个练武的好苗子。"
        ],
        Choices = [
            new() { Text = "请教我仙术！", Action = LearnMagic },
            new() { Text = "让我再想想", Action = RefuseMagic }
        ],
        Condition = () => Player.MainUnit.Level >= 1
    };
    
    public static readonly AVGDialog ShopKeeper = new()
    {
        Speaker = "店小二",
        Lines = [ "客官，需要什么？" ],
        Choices = [
            new() { Text = "买装备", Action = () => Shop.Open() },
            new() { Text = "离开", Action = () => Dialog.Close() }
        ]
    };
}
```

#### **2. 流式API（复杂剧情）**
适用于：主线剧情、复杂分支、多角色对话

```csharp
public class XianJianChapter1 : AVGScript
{
    public override async Task Execute()
    {
        // 场景设置
        await Scene.SetBackground("余杭镇.jpg")
                  .PlayBGM("peaceful.mp3")
                  .FadeIn();

        // 变量系统
        var playerName = Var<string>("player_name", "李逍遥");
        var affectionLingyue = Var<int>("affection_lingyue", 0);
        var hasLearnedMagic = Var<bool>("learned_magic", false);

        // 角色登场
        await Character.Show("姜子牙", Position.Center, Expression.Wise);
        
        // 对话流程
        await Dialog.Say("姜子牙", "年轻人，你叫什么名字？");
        
        playerName.Value = await Dialog.Input("请输入你的名字", "李逍遥");
        
        await Dialog.Say("姜子牙", $"原来是{playerName.Value}啊...")
                   .WithExpression("姜子牙", Expression.Thoughtful);

        // 复杂分支逻辑
        var choice = await Dialog.Choice("选择你的道路", [
            "请教我仙术！",
            "我只想做个普通人",
            "我想先考虑考虑"
        ]);

        switch (choice)
        {
            case 0: // 学习仙术
                await LearnMagicBranch(playerName, affectionLingyue, hasLearnedMagic);
                break;
            case 1: // 拒绝
                await RefuseMagicBranch();
                break;
            case 2: // 考虑
                await ConsiderBranch();
                break;
        }
    }

    private async Task LearnMagicBranch(AVGVariable<string> name, AVGVariable<int> affection, AVGVariable<bool> learned)
    {
        affection.Value += 10;
        learned.Value = true;
        
        await Dialog.Say("姜子牙", "好！我就传授你御剑术！")
                   .WithExpression("姜子牙", Expression.Happy);
        
        // 特效和游戏系统交互
        await Effect.PlayParticle("学习光效", Position.Center);
        await GameSystem.GiveSkill(Player.MainUnit, "御剑术");
        
        // 条件判断
        if (affection.Value > 5)
        {
            await Character.Show("林月如", Position.Right, Expression.Excited);
            await Dialog.Say("林月如", $"哇，{name.Value}好厉害！");
        }
        
        // 嵌套选择
        var nextChoice = await Dialog.Choice("接下来做什么？", [
            "继续修炼",
            "去冒险",
            "休息一下"
        ]);
        
        await HandleNextAction(nextChoice);
    }
}
```

## 🎨 **两种视觉模式**

### **模式1：全屏AVG模式**
- **适用场景**：主线剧情、重要对话、开场介绍
- **特点**：
  - 全屏背景图片
  - 角色立绘（支持左/中/右位置）
  - 底部对话框
  - 沉浸式体验

### **模式2：对话框模式**
- **适用场景**：NPC对话、任务对话、快速交互
- **特点**：
  - 透明/半透明背景
  - 简洁对话框
  - 可选角色头像
  - 不影响游戏画面

## 🔧 **API设计**

### **简单对话API（供其他模式使用）**
```csharp
// 基础对话
await AVG.ShowDialog("姜子牙", "年轻人，你愿意学习仙术吗？");

// 带选择的对话
var choice = await AVG.ShowChoice("选择", new[] { "是的", "不了" });
if (choice == 0) { /* 学习仙术 */ }

// 带变量的对话
AVG.SetVariable("player_level", 5);
await AVG.ShowDialog("NPC", "你已经是{player_level}级了！");

// 快速NPC对话
await AVG.ShowNPCDialog(NPCDialogs.ShopKeeper);
```

### **完整AVG模式API**
```csharp
// 执行完整剧本
await AVG.ExecuteScript<XianJianChapter1>();

// 场景控制
await AVG.Scene.SetBackground("背景.jpg");
await AVG.Character.Show("角色名", Position.Left, Expression.Happy);

// 复杂对话流程
await AVG.Dialog.StartConversation()
                .Say("角色A", "对话内容")
                .Choice("选择", ["选项1", "选项2"])
                .Branch(0, () => HandleChoice1())
                .Execute();
```

## 🎯 **变量和条件系统**

### **变量类型支持**
```csharp
public static class GameVariables
{
    // 基础类型
    public static AVGVariable<int> PlayerLevel = new("player_level", 1);
    public static AVGVariable<string> PlayerName = new("player_name", "李逍遥");
    public static AVGVariable<bool> HasMetLingyue = new("met_lingyue", false);
    
    // 复杂类型
    public static AVGVariable<List<string>> Inventory = new("inventory", new());
    public static AVGVariable<Dictionary<string, int>> Relationships = new("relationships", new());
}
```

### **条件判断系统**
```csharp
// 内置条件函数
public static class Conditions
{
    public static bool PlayerLevelAtLeast(int level) => GameVariables.PlayerLevel.Value >= level;
    public static bool HasItem(string itemName) => GameVariables.Inventory.Value.Contains(itemName);
    public static bool RelationshipLevel(string character, int minLevel) => 
        GameVariables.Relationships.Value.GetValueOrDefault(character, 0) >= minLevel;
}
```

## 🎮 **技术特性**

### **框架集成**
- **类型安全**：利用C#强类型系统，编译时检查错误
- **智能提示**：IDE自动完成和语法检查
- **调试支持**：可以设置断点调试剧本逻辑
- **性能优化**：编译时优化，运行时高效

### **扩展性设计**
- **插件化角色系统**：新角色只需添加配置
- **模块化剧本**：剧本可以相互引用和组合
- **可扩展条件系统**：轻松添加新的条件判断
- **自定义UI风格**：支持不同游戏的UI风格定制

### **与现有框架的兼容性**
- **事件系统集成**：AVG事件可以触发游戏系统事件
- **资源管理统一**：使用现有的资源加载机制
- **UI系统复用**：基于现有的GameUI组件构建
- **网络同步支持**：对话进度可以在多人游戏中同步

## 🚀 **开发路线图**

### **第一阶段：基础对话系统**
- [ ] 基础对话框UI组件
- [ ] 简单文本显示和打字机效果
- [ ] 基础Say/Choice API
- [ ] 声明式对话配置系统

### **第二阶段：变量和逻辑系统**
- [ ] 变量管理器实现
- [ ] 条件判断系统
- [ ] 分支逻辑处理
- [ ] 流式API基础框架

### **第三阶段：视觉增强**
- [ ] 角色立绘显示系统
- [ ] 背景图片管理
- [ ] 两种UI模式实现
- [ ] 表情和动画系统

### **第四阶段：完整AVG模式**
- [ ] 剧本加载和执行系统
- [ ] 完整的AVG游戏模式
- [ ] 存档系统预留接口
- [ ] 音效和BGM集成

### **第五阶段：高级特性**
- [ ] 复杂剧本编辑工具
- [ ] 剧本调试系统
- [ ] 性能优化
- [ ] 多语言支持

## 📚 **技术规范**

### **命名约定**
- **类名**：`AVGScript`, `DialogManager`, `CharacterDisplay`
- **接口**：`IAVGScript`, `IDialogProvider`
- **枚举**：`CharacterPosition`, `DialogType`, `ExpressionType`
- **常量**：`AVGConstants.DefaultDialogSpeed`

### **文件组织**
- **一个剧本一个类**：`XianJianChapter1.cs`
- **角色定义集中管理**：`Characters.cs`
- **通用对话分类存放**：`NPCDialogs.cs`, `TutorialDialogs.cs`

### **性能考虑**
- **资源预加载**：常用角色立绘和背景
- **内存管理**：及时释放大型资源
- **异步加载**：避免阻塞游戏主线程

## 🎨 **UI设计规范**

### **全屏AVG模式**
- **背景**：1920x1080标准分辨率
- **角色立绘**：建议800x1200像素
- **对话框**：底部1/4屏幕区域
- **字体大小**：36px正文，40px角色名

### **对话框模式**
- **对话框**：半透明背景，圆角设计
- **角色头像**：64x64像素小头像
- **字体大小**：32px正文，36px角色名
- **位置**：屏幕下方1/3区域

## 🔧 **实现示例**

### **声明式对话示例**
```csharp
public static readonly AVGDialog WelcomeDialog = new()
{
    Speaker = "系统",
    Lines = [ "欢迎来到仙剑奇侠传！" ],
    Background = "title_bg.jpg",
    BGM = "main_theme.mp3"
};
```

### **流式API示例**
```csharp
public async Task ExecuteComplexStory()
{
    await Scene.SetBackground("余杭镇.jpg")
              .Character.Show("李逍遥", Position.Left)
              .Character.Show("赵灵儿", Position.Right)
              .Dialog.Say("李逍遥", "灵儿，我们一起去冒险吧！")
              .Dialog.Say("赵灵儿", "好的，逍遥哥哥！")
              .Choice("接下来去哪里？", [
                  "蜀山派" => () => GoToShuShan(),
                  "锁妖塔" => () => GoToTower(),
                  "先休息" => () => Rest()
              ])
              .Execute();
}
```

## 🎯 **核心特性**

### **类型安全**
- 编译时检查剧本语法错误
- 强类型变量系统
- 智能代码提示和自动完成

### **高度可扩展**
- 插件化角色系统
- 可自定义UI风格
- 灵活的条件判断系统

### **性能优化**
- 资源预加载和缓存
- 异步执行避免卡顿
- 内存管理优化

### **易于使用**
- 简单的API供其他模式调用
- 丰富的内置功能
- 详细的文档和示例

## 🚀 **下一步计划**

1. **确认设计方案**：与团队讨论确认架构设计
2. **创建基础框架**：实现核心接口和基础类
3. **开发简单对话系统**：先实现最基本的对话功能
4. **逐步扩展功能**：按照路线图逐步添加功能
5. **集成测试**：在仙剑奇侠传中测试对话系统

---

## 📋 **待讨论问题**

1. **角色表情系统**：需要支持多少种表情？如何命名？
2. **音效集成深度**：角色语音、对话音效、背景音乐切换的精细程度？
3. **存档系统接口**：需要保存哪些状态？如何与现有存档系统集成？
4. **多人游戏支持**：对话进度是否需要在多人游戏中同步？
5. **本地化支持**：是否需要考虑多语言？

这个设计文档将作为AVG系统开发的指导文档，随着开发进展会持续更新和完善。
