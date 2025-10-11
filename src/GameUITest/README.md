# GameUITest 模块 🎮

GameUITest是一个专门用于测试游戏UI组件的模块，基于依赖库中的UI控件实现。主要包含摇杆测试和Buff列表测试功能。

## 📁 文件结构

```
GameUITest/
├── GameUITest.cs              # 主模块入口类
├── GameUITestMode.cs          # 测试模式管理类（主界面）
├── JoyStickTestExample.cs     # 摇杆测试实现
├── BuffListTestExample.cs     # Buff列表测试实现
└── README.md                  # 本文档
```

## 🎯 功能特性

### 🕹️ 摇杆测试 (JoyStickTestExample)

基于 `wasigamesystemui` 库中的 `AbilityJoyStickGroup` 组件实现：

- ✅ **自动绑定单位** - 自动绑定到当前主控单位
- ✅ **技能摇杆显示** - 显示单位的所有可执行技能
- ✅ **智能布局** - 自动布局技能按钮（环形分布）
- ✅ **快捷键支持** - 支持键盘1-6键快速释放技能
- ✅ **可配置参数** - 支持自定义摇杆大小、位置、角度等
- ✅ **实时状态** - 显示绑定状态和技能数量信息

#### 配置参数
- `MaxSkillCount`: 最大显示技能数量 (默认: 6)
- `ButtonSize`: 普通技能按钮大小 (默认: 120px)
- `AttackButtonSize`: 攻击技能按钮大小 (默认: 180px)
- `MinAroundDistance`: 环绕距离 (默认: 280px)
- `TotalAngleDelta`: 分布角度范围 (默认: 120°)

### 🩸 Buff列表测试 (BuffListTestExample)

基于 `wasigamesystemui` 库中的 `BuffBar` 组件实现：

- ✅ **自动绑定单位** - 自动绑定到当前主控单位
- ✅ **Buff实时显示** - 实时显示单位身上的Buff状态
- ✅ **分类过滤** - 支持正面/负面/无极性Buff分类
- ✅ **CD显示** - 支持Buff持续时间倒计时
- ✅ **堆叠显示** - 支持显示Buff叠加层数
- ✅ **测试按钮** - 提供模拟添加/清除Buff的按钮（演示用）

#### 显示特性
- 自动排序：正面Buff > 负面Buff > 无极性Buff
- 支持闪烁效果（即将消失时）
- 支持多实例合并显示
- 编辑器预览功能

## 🚀 使用方法

### 启动测试

1. **设置游戏模式**：确保在 `GlobalConfig.cs` 中 `TestGameMode` 设置为 `GameUITest`
   ```csharp
   GameDataGlobalConfig.TestGameMode = ScopeData.GameMode.GameUITest;
   ```

2. **运行游戏**：启动游戏后会自动进入GameUITest模式

3. **选择测试**：在主界面选择要测试的UI组件：
   - 🕹️ 摇杆测试
   - 🩸 Buff列表测试

### 测试界面操作

#### 主菜单
- **摇杆测试按钮** - 进入摇杆测试模式
- **Buff列表测试按钮** - 进入Buff列表测试模式
- **返回主菜单按钮** - 从测试模式返回主菜单

#### 摇杆测试模式
- **点击摇杆按钮** - 释放对应技能
- **拖拽摇杆** - 指定技能释放方向
- **键盘1-6键** - 快速释放对应技能
- **状态信息** - 显示绑定单位和技能信息

#### Buff列表测试模式
- **自动显示** - 单位Buff会自动显示在屏幕上方
- **测试按钮** - 点击添加/清除按钮（仅演示，需要实际游戏逻辑支持）
- **状态信息** - 显示绑定状态和使用说明

## 🔧 技术实现

### 模块架构

```
GameUITest (主入口)
├── 检查游戏模式
├── 注册UI初始化事件
└── 启动子模块
    ├── GameUITestMode (界面管理)
    ├── JoyStickTestExample (摇杆测试)
    └── BuffListTestExample (Buff测试)
```

### 依赖库组件

- **AbilityJoyStickGroup** (`wasigamesystemui.AbilitySystemUI`)
  - 技能摇杆组控件
  - 自动技能布局和绑定
  - 支持键盘快捷键
  
- **BuffBar** (`wasigamesystemui.BuffSystemUI`)
  - Buff条显示控件
  - 自动Buff管理和更新
  - 支持分类过滤和排序

### 事件处理

- **Game.OnMainUnitChanged** - 监听主控单位变化
- **Game.OnUIInitialization** - UI初始化时机
- **Button.OnClick** - 按钮点击事件

## 📋 注意事项

1. **单位依赖** ⚠️
   - 测试需要在有单位的场景中运行
   - 确保场景中有主控单位
   - 单位需要具备技能系统组件

2. **技能系统** ⚠️
   - 摇杆测试需要单位有 `AbilityManager` 组件
   - 技能需要为 `AbilityExecute` 类型才能显示

3. **Buff系统** ⚠️
   - Buff测试需要实际的Buff系统支持
   - 测试按钮仅为演示，不会实际添加Buff
   - 实际Buff需要通过游戏逻辑添加

4. **编译条件** 📝
   - 所有文件都使用 `#if CLIENT` 条件编译
   - 仅在客户端环境下编译和运行

## 🐛 故障排除

### 常见问题

1. **摇杆不显示技能**
   - 检查单位是否有 `AbilityManager` 组件
   - 检查技能是否为 `AbilityExecute` 类型
   - 查看控制台日志确认绑定状态

2. **Buff列表不显示**
   - 确认单位已正确绑定
   - 检查单位是否有实际的Buff
   - BuffBar需要实际的Buff系统支持

3. **界面不显示**
   - 确认游戏模式设置正确
   - 检查 `Game.MainUI` 是否可用
   - 查看控制台错误日志

### 调试信息

模块会在控制台输出详细的调试信息：
- `🎮 初始化摇杆测试界面...`
- `🩸 初始化Buff列表测试界面...`
- `✅ 已绑定单位: [单位名] (ID: [ID])`
- `❌ 绑定失败: [错误信息]`

## 📝 扩展说明

### 添加新的UI测试

1. 创建新的测试类（参考现有实现）
2. 在 `GameUITestMode` 中添加对应按钮
3. 在 `GameUITest.cs` 中注册新的测试类
4. 更新切换逻辑和清理方法

### 自定义配置

可以通过修改各测试类中的配置参数来自定义显示效果，如摇杆大小、布局位置、颜色等。

---

**制作者**: 🐱 AI助手  
**版本**: 1.0  
**更新时间**: 2024年12月  

主人，这个GameUITest模块现在可以很好地测试摇杆和Buff列表功能了喵～ 🎮✨
