# 🎯 AI Test Scene Configuration

## **场景概述**

为AI系统测试专门创建了一个大型场景 `AITestScene`，提供了更好的AI行为观察和测试环境。

### **场景规格**
- **名称**: AI System Test Scene
- **标识**: `ai_test_scene`
- **尺寸**: 64×256 × 64×256 = 16,384 × 16,384 单位
- **相比默认场景**: 比默认场景大4倍 (16×256 vs 64×256)

## **🗺️ 场景布局**

### **预放置单位**
| 单位ID | 类型 | 位置 | 所属玩家 | 用途 |
|--------|------|------|----------|------|
| 1 | HostTestHero | (8000, 8000, 0) | Player 1 | 主角单位，用户控制 |
| 10 | HostTestHero | (10000, 10000, 0) | Player 2 | AI目标 - 东北方向 |
| 11 | HostTestHero | (6000, 10000, 0) | Player 2 | AI目标 - 西北方向 |
| 12 | HostTestHero | (10000, 6000, 0) | Player 2 | AI目标 - 东南方向 |
| 13 | HostTestHero | (6000, 6000, 0) | Player 2 | AI目标 - 西南方向 |

### **测试区域划分**
```
(0,0)                    (8192,0)                  (16384,0)
  ┌─────────────────────────┬─────────────────────────┐
  │                         │                         │
  │    西北区域             │      东北区域           │
  │  Target 11              │    Target 10            │
  │  (6000,10000)           │   (10000,10000)         │
  │                         │                         │
  │                         │   AIThinkTree           │
  │                         │   Validation Area       │
  │                         │   (12000+, 12000+)      │
  ├─────────────────────────┼─────────────────────────┤
  │                         │                         │
  │    西南区域             │      东南区域           │
  │  Target 13              │    Target 12            │
  │  (6000,6000)            │   (10000,6000)          │
  │                         │                         │
  │  AI Test Area           │    主角区域             │
  │  (4000+, 4000+)         │   Player (8000,8000)    │
  │                         │                         │
  └─────────────────────────┴─────────────────────────┘
(0,16384)              (8192,16384)               (16384,16384)
```

## **🔧 配置细节**

### **GameData配置**
```csharp
_ = new GameDataScene(Scene.AITestScene)
{
    DefaultCamera = Camera.DefaultCamera,
    Name = "AI System Test Scene",
    HostedSceneTag = "ai_test_scene"u8,
    Size = new(64 * 256, 64 * 256),
    OnLoaded = static (scene) => Game.Logger.LogInformation("AI Test Scene {scene} loaded", scene),
    PlacedPlayerObjects = new() { /* 预放置单位配置 */ }
};
```

### **游戏模式更新**
```csharp
_ = new GameDataGameMode(GameMode.AISystemTest)
{
    Name = "AI System Test Mode",
    Gameplay = Gameplay.Default,
    PlayerSettings = PlayerSettings.Default,
    SceneList = [ Scene.AITestScene ],      // 使用新场景
    DefaultScene = Scene.AITestScene,       // 默认加载新场景
};
```

## **🎮 测试区域配置**

### **AI测试区域** (西南象限)
- **基础位置**: (4000, 4000, 0)
- **用途**: WaveAI单位创建和移动测试
- **相对偏移**:
  - Guard测试目标: +1200, +800
  - Move测试目标: +1000, +600  
  - Patrol测试目标: -800, +1200

### **验证测试区域** (东北象限)
- **基础位置**: (12000, 12000, 0)
- **用途**: AIThinkTree功能验证
- **特点**: 远离主测试区域，避免干扰

## **✅ 迁移完成状态**

### **已更新文件**
- ✅ `Tests/Game/ScopeData.cs`: 添加AITestScene定义和配置
- ✅ `Tests/Game/AISystemTest/AISystemTest.Server.cs`: 更新场景引用和基础位置
- ✅ `Tests/Game/AISystemTest/AIThinkTreeValidator.cs`: 更新验证区域位置

### **关键更改**
- ✅ **场景尺寸**: 从16×256扩展到64×256 (4倍面积)
- ✅ **预放置目标**: 4个目标单位分散在四个方向
- ✅ **测试区域**: AI测试和验证功能使用不同象限
- ✅ **编译验证**: 所有更改通过编译测试

## **🚀 使用优势**

### **改进对比**
| 方面 | 旧配置 (DefaultScene) | 新配置 (AITestScene) |
|------|---------------------|---------------------|
| 场景大小 | 16×256 (4,096²) | 64×256 (16,384²) |
| 测试空间 | 受限 | 宽敞的测试区域 |
| 目标单位 | 运行时创建 | 预放置，更稳定 |
| 区域划分 | 无 | 清晰的功能分区 |
| 观察距离 | 短距离移动 | 长距离移动，更易观察 |

### **测试体验提升**
- 🎯 **更大观察空间**: AI移动行为更容易观察
- 🗺️ **专用测试区域**: 不同功能在不同象限，避免冲突
- 📍 **预设目标单位**: 测试更稳定，减少运行时创建的复杂性
- 🔄 **更长移动距离**: AI行为模式更清晰可见

现在AI系统测试拥有了一个专门设计的大型场景，为观察和验证AI行为提供了理想的环境！🎊 