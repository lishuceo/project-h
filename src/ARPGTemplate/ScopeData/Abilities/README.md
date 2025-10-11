# ARPG 技能系统架构

## 📁 文件结构

```
Abilities/
├── README.md                    # 本文档
├── Swordsman/                   # 剑客职业技能
│   ├── SwordSlashAbility.cs     # 挥剑 - 近战范围物理伤害
│   ├── DashAbility.cs           # 冲刺 - 辅助位移技能
│   └── CrushingBlowAbility.cs   # 痛击 - 近战单体减益破防
├── Gunner/                      # 🔮 枪手职业技能 (未来扩展)
├── Mage/                        # 🔮 法师职业技能 (未来扩展)
└── Warrior/                     # 🔮 战士职业技能 (未来扩展)
```

## 🎯 设计原则

### 1. 职业分离
每个职业的技能独立成文件夹，便于：
- 独立开发和维护
- 减少文件冲突
- 清晰的职业界限

### 2. 技能独立
每个技能独立成文件，包含：
- 技能定义
- 效果配置
- 粒子特效
- 动画配置
- 冷却和消耗

### 3. 统一入口
`ARPGAbilities.cs` 作为统一入口：
- 对外提供技能引用
- 协调各职业技能注册
- 保持向后兼容

## 🚀 扩展指南

### 添加新职业
1. 在 `Abilities/` 下创建新职业文件夹
2. 为该职业创建技能文件
3. 在 `ARPGAbilities.cs` 中添加技能引用
4. 在 `ARPGUnits.cs` 中为职业单位分配技能

### 添加新技能
1. 在对应职业文件夹下创建新技能文件
2. 实现 `IGameClass` 接口
3. 定义所有相关的GameLink引用
4. 在 `OnGameDataInitialization` 中初始化配置
5. 在 `ARPGAbilities.cs` 中添加引用

## 📊 当前实现

### 剑客技能 (Swordsman)
- ⚔️ **挥剑** - 近战范围物理伤害，带刀光和受击特效
- 🏃‍♂️ **冲刺** - 快速位移，提高机动性
- 💥 **痛击** - 单体高伤害+破甲debuff，带特效

### 特效系统
每个攻击技能都包含：
- **刀光特效**: 技能释放时在施法者身上播放
- **受击特效**: 造成伤害时在目标身上播放
- **持续特效**: Buff/Debuff的视觉表现

## 🎮 技能引用方式

```csharp
// 在其他地方引用技能（保持原有接口）
var swordSlash = ARPGAbilities.Ability.SwordSlash;
var dash = ARPGAbilities.Ability.Dash;
var crushingBlow = ARPGAbilities.Ability.CrushingBlow;

// 单位技能配置（ARPGUnits.cs中）
Abilities = [
    ARPGAbilities.Ability.SwordSlash,
    ARPGAbilities.Ability.Dash,
    ARPGAbilities.Ability.CrushingBlow,
],
```

## 🔧 技术细节

- **命名空间**: `GameEntry.ARPGTemplate.ScopeData.Abilities.[职业名]`
- **GameLink**: 每个技能使用唯一的GameLink标识符
- **初始化**: 通过 `OnRegisterGameClass()` 自动注册
- **条件初始化**: 只在 ARPG 模式下初始化技能数据

这种架构为后续添加更多职业和技能提供了良好的扩展性！
