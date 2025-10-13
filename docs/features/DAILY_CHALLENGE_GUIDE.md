# 每日挑战功能说明

## 📋 功能概述

每日挑战是一个新增的游戏模式，每天提供一个固定的初始关卡，全球所有玩家在同一天玩的是完全相同的关卡，确保排名的公平性。

## 🎯 玩法说明

### 目标
- **清空所有像素块**：将初始布局中的所有像素块消除

### 评分维度
1. **⏱️ 用时**：完成挑战的时间（越快越好）
2. **🚶 步数**：使用的步数（越少越好）
3. **⭐ 星级**：根据时间和步数综合评定（1-3星）

### 难度设置
- **周一周二**：⭐ 简单
- **周三周四周五**：⭐⭐ 中等
- **周六周日**：⭐⭐⭐ 困难

## 🔐 全球统一机制

### 如何保证公平？

1. **UTC日期**：使用UTC时间而非本地时间，确保全球同一天
2. **种子随机数**：相同日期 → 相同种子 → 完全相同的关卡
3. **确定性算法**：所有随机生成都使用种子随机数生成器
4. **校验和验证**：每个关卡都有唯一的校验和，用于验证一致性

### 示例

```
2025年10月13日

中国玩家（UTC+8）：
- 本地时间：20:00
- UTC时间：12:00
- 种子：20251013
- 关卡：[完全相同的初始布局]

美国玩家（UTC-8）：
- 本地时间：04:00
- UTC时间：12:00
- 种子：20251013
- 关卡：[完全相同的初始布局] ✅
```

## 🎮 游戏界面

### 挑战信息栏
```
📅 2025-10-13    难度: ⭐⭐
⏱️ 01:23        🚶 步数: 5/20    📦 剩余: 8格
```

### 控制按钮
- **← 返回菜单**：退出挑战，返回主菜单
- **🔄 重新开始**：重新开始今日挑战

## 📊 星级评定标准

### 简单难度（⭐）
- **3星**：≤ 60秒 且 ≤ 10步
- **2星**：≤ 120秒 且 ≤ 15步
- **1星**：完成即可

### 中等难度（⭐⭐）
- **3星**：≤ 90秒 且 ≤ 15步
- **2星**：≤ 150秒 且 ≤ 25步
- **1星**：完成即可

### 困难难度（⭐⭐⭐）
- **3星**：≤ 120秒 且 ≤ 20步
- **2星**：≤ 200秒 且 ≤ 35步
- **1星**：完成即可

## 💾 本地记录

系统会自动保存你的挑战记录：
- ✅ 最佳时间
- ✅ 最少步数
- ✅ 最高分数
- ✅ 最高星级
- ✅ 尝试次数

### 查看记录

```javascript
// 在浏览器控制台中
const manager = ChallengeManager.getInstance();

// 查看今日记录
const today = manager.getTodayRecord();
console.log(today);

// 查看历史记录
const history = manager.getRecords(30); // 最近30天
console.log(history);

// 查看统计
const stats = manager.getStats();
console.log(stats);
```

## 🏆 排行榜（待实现）

当SDK集成后，将支持：
- 📈 全球每日排行榜
- 🎯 好友排名
- 🏅 个人最佳记录展示

## 🛠️ 技术实现

### 核心模块

```
src/
├── challenge/
│   ├── ChallengeManager.ts    # 挑战管理器（单例）
│   ├── LevelGenerator.ts      # 关卡生成器
│   ├── Timer.ts               # 计时器
│   └── types.ts               # 类型定义
├── utils/
│   └── seedRandom.ts          # 种子随机数生成器
└── scenes/
    └── DailyChallengeScene.ts # 每日挑战场景
```

### 种子生成算法

```typescript
// 日期转种子
'2025-10-13' → 20251013

// 种子随机数生成器（LCG算法）
X(n+1) = (a * X(n) + c) mod m
```

### 关卡生成策略

1. 根据星期几决定难度
2. 选择N种颜色（简单2种，中等3种，困难4种）
3. 为每种颜色生成左右两侧的堆积
4. 中等及以上难度添加"桥梁"像素块
5. 计算合理的步数限制
6. 生成校验和

## 📝 开发指南

### 添加新功能

#### 自定义难度

```typescript
// src/challenge/LevelGenerator.ts

private getDifficulty(dayOfWeek: number): 1 | 2 | 3 {
  // 修改难度规则
  if (dayOfWeek === 0) return 3; // 周日困难
  if (dayOfWeek === 6) return 3; // 周六困难
  return 1; // 其他天简单
}
```

#### 调整星级阈值

```typescript
// src/challenge/LevelGenerator.ts

public getStarThresholds(difficulty: 1 | 2 | 3): StarThresholds {
  if (difficulty === 1) {
    return {
      time3star: 45,  // 改为45秒
      time2star: 90,
      steps3star: 8,  // 改为8步
      steps2star: 12
    };
  }
  // ...
}
```

#### 修改关卡生成算法

```typescript
// src/challenge/LevelGenerator.ts

private generateLayout(...) {
  // 自定义关卡生成逻辑
}
```

### 测试

```bash
# 启动开发服务器
npm run dev

# 访问游戏
http://localhost:3001

# 点击 "⭐ 每日挑战" 按钮
```

### 调试

```javascript
// 浏览器控制台

// 获取今日挑战数据
const manager = ChallengeManager.getInstance();
const challenge = manager.getTodayChallenge();
console.log(challenge);

// 查看种子和校验和
console.log('种子:', challenge.seed);
console.log('校验和:', challenge.checksum);

// 清除所有记录（慎用）
manager.clearAllRecords();
```

## 🐛 常见问题

### Q: 为什么今天的关卡和昨天不一样？
A: 每天的关卡都是根据日期生成的，每天的种子不同，所以关卡不同。

### Q: 时区不同会影响吗？
A: 不会。系统使用UTC时间，确保全球同一天。

### Q: 如何验证关卡一致性？
A: 每个关卡都有唯一的校验和，可以通过校验和验证是否是同一关卡。

### Q: 可以提前玩明天的关卡吗？
A: 不可以。系统会自动检测当前UTC日期，只能玩今天的关卡。

### Q: 记录保存在哪里？
A: 本地记录保存在浏览器的localStorage中。未来集成SDK后会同步到服务器。

## 🔮 未来计划

- [ ] SDK集成（全球排行榜）
- [ ] 好友对战
- [ ] 周挑战
- [ ] 成就系统
- [ ] 回放功能
- [ ] 自定义挑战（玩家创建并分享）

## 📚 相关文档

- [AI_HANDOFF.md](AI_HANDOFF.md) - AI会话交接
- [ARCHITECTURE.md](ARCHITECTURE.md) - 技术架构
- [GameDesignDocument.md](GameDesignDocument.md) - 游戏设计

---

**祝你在每日挑战中取得好成绩！** 🎉

