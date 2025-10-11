# GameEntry 项目说明

## 吸血鬼幸存者游戏 (Vampire Survivors)
当前项目包含一个完整的吸血鬼幸存者生存游戏！

### 主要文件
- `VampireSurvivors.cs` - 主游戏逻辑和界面系统
- `GameClasses.cs` - 游戏对象类定义 (Player, Enemy, Projectile等)
- `VAMPIRE_SURVIVORS_README.md` - 详细的游戏说明和玩法指南

### 游戏特色
- **经典生存玩法** - 控制角色在敌人群中生存，越久越好
- **自动攻击系统** - 角色自动向四个方向发射攻击
- **升级系统** - 击败敌人获得经验，升级选择强化效果
- **流畅的界面** - 使用GameUI库制作的现代游戏界面
- **摄像机跟随** - 平滑的摄像机跟随玩家移动

### 运行要求
- 需要在 `Client-Debug`、`Client-Release` 或 `Client-Wasm` 配置下编译运行
- 依赖 GameUI 库进行界面渲染
- 使用GameCore引擎的事件系统和组件架构

### 控制方式
- **WASD** 或 **方向键** - 移动角色
- **ESC** - 暂停/恢复游戏  
- **鼠标点击** - 选择升级选项

## 已删除的示例文件
以下原始测试示例文件已被删除，为新游戏让路：
- ~~`FlappyBird.cs`~~ - Flappy Bird游戏示例
- ~~`TestTriggers.Client.cs`~~ - 客户端触发器测试示例
- ~~`TestTriggers.Server.cs`~~ - 服务器端触发器测试示例
- ~~`ScopeData.cs`~~ - 游戏数据定义示例
- ~~`InventoryUI.Client.cs`~~ - 物品栏UI示例
- ~~`TestUnitProperty.cs`~~ - 单位属性测试示例

## 开发说明
游戏采用模块化设计，可以轻松扩展：
- 新的敌人类型 (FastEnemy, TankEnemy等已预定义)
- 新的武器系统 (基础框架已实现)
- 新的升级选项 (在GenerateUpgradeOptions方法中添加)
- 更多游戏机制 (道具系统、技能树等)

详细的游戏玩法和技术说明请参考 `VAMPIRE_SURVIVORS_README.md` 文件。 