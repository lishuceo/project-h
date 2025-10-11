# Vampire3D 被动技能系统

## 概述

本文档描述了为 Vampire3D 游戏实现的被动技能系统。该系统通过 GameCore 框架的 `GameDataAbility` 的 `PassivePeriod` 和 `PassivePeriodicEffect` 字段实现，使主角能够自动释放技能而不与移动指令冲突。

## 已实现的被动技能

### 1. 火球术 (FireballAura)
- **描述**: 每2.5秒自动向附近敌人发射火球
- **机制**: 
  - 搜索600单位范围内的敌人
  - 最多同时选择3个目标
  - 发射带有粒子效果的火球投射物
  - 造成150点物理伤害
- **效果组件**:
  - `FireballSearch`: 搜索附近敌人
  - `FireballLaunch`: 发射火球投射物
  - `FireballDamage`: 造成伤害
  - `FireballImpact`: 撞击效果组合

### 2. 闪电链 (LightningChain)
- **描述**: 每4秒释放闪电链，连击多个敌人
- **机制**:
  - 搜索400单位范围内的敌人
  - 最多连击5个目标
  - 造成200点物理伤害
- **效果组件**:
  - `LightningSearch`: 搜索目标
  - `LightningDamage`: 造成伤害

### 3. 治疗光环 (HealingAura)
- **描述**: 每3秒恢复生命值
- **机制**: 
  - 当前实现为日志输出（"玩家恢复了50点生命值"）
  - 未来可扩展为实际治疗效果
- **效果组件**:
  - `HealingEffect`: 治疗效果（目前为日志）

## 技术实现

### 核心类结构

```csharp
namespace GameEntry.VampireSurvivors3D;

public class PassiveAbilities : IGameClass
{
    // 技能定义
    public static class Ability { ... }
    
    // 效果定义  
    public static class Effect { ... }
    
    // 单位定义
    public static class Unit { ... }
    
    // 粒子效果定义
    public static class Particle { ... }
    
    // Actor定义
    public static class Actor { ... }
}
```

### 被动技能配置

被动技能通过以下方式配置：

```csharp
_ = new GameDataAbility(Ability.FireballAura)
{
    DisplayName = "火球术",
    Description = "每2.5秒自动向附近敌人发射火球",
    PassivePeriod = static (_) => TimeSpan.FromSeconds(2.5),
    PassivePeriodicEffect = Effect.FireballSearch,
    Flags = new() { IsPassive = true },
};
```

### 主角技能配置

在 `ExtraScopeDataVampire3D.cs` 中，主角单位被配置了这些被动技能：

```csharp
Abilities = [
    VampireSurvivors3D.PassiveAbilities.Ability.FireballAura,
    VampireSurvivors3D.PassiveAbilities.Ability.LightningChain,
    VampireSurvivors3D.PassiveAbilities.Ability.HealingAura,
],
```

## 框架优势

使用 GameCore 框架的 `PassivePeriod` 和 `PassivePeriodicEffect` 有以下优势：

1. **无冲突**: 被动技能不会与玩家的移动指令冲突
2. **自动执行**: 按指定周期自动触发，无需手动操作
3. **组件化**: 使用效果组件系统，便于扩展和维护
4. **性能优化**: 框架级别的优化，避免过度的轮询和计算

## 视觉效果

系统包含以下粒子效果：
- **火球拖尾**: `effect/eff_tongyong/huoqiu2/particle.effect`
- **火球爆炸**: `effect/eff_tongyong/huoqiu_blast/particle.effect`

## 扩展性

系统设计时考虑了扩展性，可以轻松添加新的被动技能：

1. 在 `Ability` 类中定义新技能
2. 在 `Effect` 类中定义相关效果
3. 在数据初始化中配置技能属性
4. 在主角配置中添加新技能

## 已知限制

1. **治疗效果**: 当前治疗光环仅为日志输出，需要进一步实现实际治疗逻辑
2. **效果多样性**: 目前主要实现了基础的搜索-伤害模式，可扩展更多创意效果
3. **平衡性**: 伤害数值和冷却时间需要根据游戏测试进行调整

## 下一步优化

1. **实现真实治疗**: 使用 `GameDataEffectUnitModifyVital` 实现实际生命值恢复
2. **添加更多技能**: 实现冰霜新星、buff系统等
3. **视觉效果增强**: 添加更多粒子效果和音效
4. **技能升级系统**: 允许技能随游戏进度升级
5. **技能选择机制**: 实现升级时的技能选择界面

## 使用说明

1. 确保游戏模式设置为 `VampireSurvivors3D`
2. 被动技能系统会在游戏数据初始化时自动加载
3. 主角生成时会自动获得配置的被动技能
4. 技能会按设定的周期自动触发，无需玩家干预

这个被动技能系统为 Vampire3D 游戏提供了核心的自动战斗机制，符合吸血鬼生存者类游戏的设计理念。 