# AVG系统 API 文档

## 🎯 **快速开始**

```csharp
// 1. 注册剧本数据
XianJianScripts.RegisterAllScripts();

// 2. 播放剧本
await AVGScriptEngine.PlayScript("BlackWukong_Story");
```

## 📚 **核心API**

### **AVGScriptEngine** - 剧本执行引擎

```csharp
// 注册剧本
public static void RegisterScript(AVGScript script)

// 播放剧本
public static async Task PlayScript(string scriptId)

// 检查状态
public static bool IsExecuting { get; }

// 清理资源
public static void Cleanup()
```

### **AVGFullScreen** - 直接对话API（可选）

```csharp
// 显示单句对话
public static async Task ShowDialog(string speaker, string content)

// 显示选择分支
public static async Task<int> ShowChoice(string title, string[] choices)

// 设置背景
public static void SetBackground(string imagePath)

// 显示角色
public static void ShowCharacter(string name, string imagePath, CharacterPosition position)
```

## 🏗️ **数据结构**

### **AVGScript** - 剧本

```csharp
public class AVGScript
{
    public string Id { get; set; }           // 剧本唯一ID
    public string Name { get; set; }         // 剧本名称
    public string StartNodeId { get; set; }  // 起始节点ID
    public List<AVGNode> Nodes { get; set; } // 节点列表
}
```

### **DialogNode** - 对话节点

```csharp
public class DialogNode : AVGNode
{
    public string Speaker { get; set; }                    // 说话人
    public string Content { get; set; }                    // 对话内容
    public string? Background { get; set; }                // 背景图片路径（可选）
    public List<CharacterDisplay> Characters { get; set; } // 角色立绘（可选）
    public string? NextNodeId { get; set; }                // 下一节点ID，null=结束
}
```

### **ChoiceNode** - 选择节点

```csharp
public class ChoiceNode : AVGNode
{
    public string Title { get; set; }                // 选择标题
    public List<ChoiceOption> Options { get; set; }  // 选择选项
}

public class ChoiceOption
{
    public string Text { get; set; }       // 选项文本
    public string NextNodeId { get; set; } // 选择后跳转的节点ID
}
```

### **CharacterDisplay** - 角色显示

```csharp
public class CharacterDisplay
{
    public string Name { get; set; }              // 角色名称
    public string ImagePath { get; set; }         // 角色图片路径
    public CharacterPosition Position { get; set; } // 显示位置：Left/Center/Right
    public bool Visible { get; set; } = true;     // 是否显示
}
```

### **BackgroundNode** - 背景设置节点

```csharp
public class BackgroundNode : AVGNode
{
    public string ImagePath { get; set; }  // 背景图片路径
    public string? NextNodeId { get; set; } // 下一节点ID
}
```

## 📝 **剧本创建示例**

### **简单对话**

```csharp
var script = new AVGScript
{
    Id = "MyStory",
    Name = "我的故事",
    StartNodeId = "start",
    Nodes = new List<AVGNode>
    {
        new DialogNode
        {
            Id = "start",
            Speaker = "主角",
            Content = "这是一个简单的对话。",
            NextNodeId = null // 结束
        }
    }
};
```

### **复杂分支剧本**

```csharp
var script = new AVGScript
{
    Id = "ComplexStory", 
    StartNodeId = "scene1",
    Nodes = new List<AVGNode>
    {
        // 设置背景
        new BackgroundNode
        {
            Id = "bg_set",
            ImagePath = "image/AVGSystem/Resources/bg1.png",
            NextNodeId = "scene1"
        },
        
        // 对话
        new DialogNode
        {
            Id = "scene1",
            Speaker = "角色A",
            Content = "你好，欢迎来到我的世界。",
            Characters = new List<CharacterDisplay>
            {
                new CharacterDisplay
                {
                    Name = "角色A",
                    ImagePath = "image/AVGSystem/Resources/Characters/CharacterA/avatar.png",
                    Position = CharacterPosition.Center
                }
            },
            NextNodeId = "choice1"
        },
        
        // 选择分支
        new ChoiceNode
        {
            Id = "choice1",
            Title = "你想做什么？",
            Options = new List<ChoiceOption>
            {
                new ChoiceOption { Text = "探索世界", NextNodeId = "explore" },
                new ChoiceOption { Text = "休息一下", NextNodeId = "rest" },
                new ChoiceOption { Text = "离开这里", NextNodeId = "leave" }
            }
        },
        
        // 分支结果
        new DialogNode { Id = "explore", Speaker = "角色A", Content = "很好的选择！", NextNodeId = null },
        new DialogNode { Id = "rest", Speaker = "角色A", Content = "休息也很重要。", NextNodeId = null },
        new DialogNode { Id = "leave", Speaker = "角色A", Content = "再见！", NextNodeId = null }
    }
};
```

## 🎮 **使用流程**

### **1. 创建剧本数据**
- 继承 `AVGNode` 创建节点
- 组装成 `AVGScript` 剧本

### **2. 注册剧本**
```csharp
AVGScriptEngine.RegisterScript(myScript);
```

### **3. 播放剧本**
```csharp
await AVGScriptEngine.PlayScript("MyStory");
```

## 📁 **资源路径规范**

```
image/AVGSystem/Resources/
├── bg1.png, bg2.png, bg3.png, bg4.png    # 背景图片
└── Characters/                            # 角色头像
    ├── CharacterName/
    │   └── avatar.png                     # 角色头像 (120x120px)
    └── ...
```

## ⚙️ **配置说明**

### **启用AVG测试模式**
在 `GlobalConfig.cs` 中设置：
```csharp
GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.AVGTest;
```

### **节点ID命名建议**
- 使用语义化ID：`opening_001`, `choice_battle`, `ending_good`
- 同一剧本内ID唯一
- 建议使用前缀区分不同剧本

## 🎨 **UI特性**

- **全屏16:9背景**：自动适配屏幕，无变形
- **底部30%对话框**：半透明背景，清晰文字
- **右侧25%选择面板**：现代化选择界面
- **多角色立绘**：支持左中右三位置同时显示

## ⚡ **性能说明**

- 剧本数据在内存中缓存
- UI组件复用，无需重复创建
- 支持客户端和服务端编译
- 异步执行，不阻塞游戏主线程

---

**完整示例请参考**: `GameEntry/AVGSystem/Data/XianJianScripts.cs`
