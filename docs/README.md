# 项目文档索引

本目录包含项目的所有文档，按类别组织。

## 📚 文档结构

```
docs/
├── README.md                           # 本文档（文档索引）
├── AI_HANDOFF.md                       # ⭐⭐⭐⭐⭐ AI会话交接指南
├── PROJECT_CONTEXT.md                  # ⭐⭐⭐⭐⭐ 项目完整上下文
├── GameDesignDocument.md               # ⭐⭐⭐⭐⭐ 游戏设计规范
├── ARCHITECTURE.md                     # ⭐⭐⭐⭐ 技术架构说明
├── DEVELOPMENT_LOG.md                  # ⭐⭐⭐⭐ 开发历程记录
│
├── features/                           # 功能文档目录
│   ├── README.md                       # 功能文档索引
│   ├── DAILY_CHALLENGE_GUIDE.md        # 每日挑战用户指南
│   └── DAILY_CHALLENGE_IMPLEMENTATION.md # 每日挑战技术实现
│
└── engineering/                        # 工程技术文档目录
    ├── README.md                       # 工程文档索引
    └── PERFORMANCE_OPTIMIZATION.md     # 性能优化追踪
```

---

## 🎯 快速导航

### 如果你是新的 AI 会话
👉 **必读**: [AI_HANDOFF.md](./AI_HANDOFF.md) (5分钟)

### 如果你想了解项目
👉 **推荐**: [PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md) (15分钟)

### 如果你想开发新功能
👉 **必读**: [GameDesignDocument.md](./GameDesignDocument.md) (60分钟)

### 如果你想了解架构
👉 **推荐**: [ARCHITECTURE.md](./ARCHITECTURE.md) (20分钟)

---

## 📖 文档分类详解

### 🤖 核心文档（根目录）

#### [AI_HANDOFF.md](./AI_HANDOFF.md) ⭐⭐⭐⭐⭐
**用途**: AI会话交接指南
**阅读时间**: 5分钟
**适用场景**: 新AI会话启动时必读

**包含内容**:
- 项目5分钟速览
- 4个关键警告（避免踩坑）
- 已解决的bug清单
- 快速上手步骤

---

#### [PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md) ⭐⭐⭐⭐⭐
**用途**: 项目完整上下文
**阅读时间**: 15分钟
**适用场景**: 深入了解项目背景和现状

**包含内容**:
- 项目起源和目标
- 技术选型说明
- 当前进度和状态
- 关键决策记录

---

#### [GameDesignDocument.md](./GameDesignDocument.md) ⭐⭐⭐⭐⭐
**用途**: 游戏设计规范（权威文档）
**阅读时间**: 60分钟（可分章阅读）
**适用场景**: 开发新功能、修改游戏逻辑

**包含内容**:
- 完整的游戏设计规范
- 双层网格系统详解
- 物理引擎实现细节
- 消除算法说明
- 所有游戏系统的设计文档

**重点章节**:
- 第5章: 物理系统
- 第8章: 消除系统
- 第12章: 性能优化

---

#### [ARCHITECTURE.md](./ARCHITECTURE.md) ⭐⭐⭐⭐
**用途**: 技术架构说明
**阅读时间**: 20分钟
**适用场景**: 了解代码结构和架构设计

**包含内容**:
- 项目目录结构
- 核心模块说明
- 数据流图
- 架构决策

---

#### [DEVELOPMENT_LOG.md](./DEVELOPMENT_LOG.md) ⭐⭐⭐⭐
**用途**: 开发历程记录
**阅读时间**: 10分钟
**适用场景**: 了解项目演进历史

**包含内容**:
- 开发时间线
- 重要里程碑
- 遇到的问题和解决方案
- 技术决策演变

---

### 🎮 功能文档 (features/)

功能相关的设计、实现和使用文档。

#### [features/README.md](./features/README.md)
功能文档目录索引

#### [features/DAILY_CHALLENGE_GUIDE.md](./features/DAILY_CHALLENGE_GUIDE.md)
每日挑战系统用户指南

#### [features/DAILY_CHALLENGE_IMPLEMENTATION.md](./features/DAILY_CHALLENGE_IMPLEMENTATION.md)
每日挑战系统技术实现文档

---

### 🔧 工程文档 (engineering/)

技术、性能、测试、部署等工程相关文档。

#### [engineering/README.md](./engineering/README.md)
工程文档目录索引

#### [engineering/PERFORMANCE_OPTIMIZATION.md](./engineering/PERFORMANCE_OPTIMIZATION.md) ⭐⭐⭐⭐
**用途**: 性能优化追踪文档
**阅读时间**: 20分钟
**适用场景**: 性能优化、问题排查

**包含内容**:
- 性能问题分析报告（8个瓶颈）
- P1/P2/P3 优化方案（含实施进度）
- 性能测试建议
- 优化技术总结

**当前状态**:
- ✅ P1 完成度: 100% (3/3)
- ⏳ P2 完成度: 0% (0/2)
- ⏳ P3 完成度: 0% (0/3)
- 性能提升: 30-50% (P1完成后)

---

## 📝 推荐阅读路径

### 路径 1: 快速上手 (20分钟)
适合新AI会话或快速了解项目

1. [AI_HANDOFF.md](./AI_HANDOFF.md) - 5分钟
2. [PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md) - 15分钟
3. 运行游戏测试

### 路径 2: 功能开发 (90分钟)
适合开发新功能

1. [AI_HANDOFF.md](./AI_HANDOFF.md) - 5分钟
2. [PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md) - 15分钟
3. [GameDesignDocument.md](./GameDesignDocument.md) - 60分钟（重点章节）
4. [ARCHITECTURE.md](./ARCHITECTURE.md) - 10分钟（相关模块）

### 路径 3: 性能优化 (40分钟)
适合性能问题排查和优化

1. [AI_HANDOFF.md](./AI_HANDOFF.md) - 5分钟
2. [engineering/PERFORMANCE_OPTIMIZATION.md](./engineering/PERFORMANCE_OPTIMIZATION.md) - 20分钟
3. [GameDesignDocument.md](./GameDesignDocument.md) 第12章 - 10分钟
4. 相关源代码阅读 - 5分钟

### 路径 4: 完整理解 (2小时)
适合深入理解整个项目

1. [AI_HANDOFF.md](./AI_HANDOFF.md)
2. [PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md)
3. [GameDesignDocument.md](./GameDesignDocument.md)
4. [ARCHITECTURE.md](./ARCHITECTURE.md)
5. [DEVELOPMENT_LOG.md](./DEVELOPMENT_LOG.md)
6. 功能文档（按需）
7. 工程文档（按需）
8. 源代码阅读

---

## 🔍 按主题查找文档

### 主题: 网格系统
- [GameDesignDocument.md](./GameDesignDocument.md) 第3章
- [ARCHITECTURE.md](./ARCHITECTURE.md) Grid模块

### 主题: 物理引擎
- [GameDesignDocument.md](./GameDesignDocument.md) 第5章
- [ARCHITECTURE.md](./ARCHITECTURE.md) PhysicsManager模块
- [engineering/PERFORMANCE_OPTIMIZATION.md](./engineering/PERFORMANCE_OPTIMIZATION.md) P2-5

### 主题: 消除系统
- [GameDesignDocument.md](./GameDesignDocument.md) 第8章
- [engineering/PERFORMANCE_OPTIMIZATION.md](./engineering/PERFORMANCE_OPTIMIZATION.md) P1-3

### 主题: 渲染系统
- [GameDesignDocument.md](./GameDesignDocument.md) 第5.5节
- [engineering/PERFORMANCE_OPTIMIZATION.md](./engineering/PERFORMANCE_OPTIMIZATION.md) P1-1

### 主题: 每日挑战
- [features/DAILY_CHALLENGE_GUIDE.md](./features/DAILY_CHALLENGE_GUIDE.md)
- [features/DAILY_CHALLENGE_IMPLEMENTATION.md](./features/DAILY_CHALLENGE_IMPLEMENTATION.md)

### 主题: 性能优化
- [engineering/PERFORMANCE_OPTIMIZATION.md](./engineering/PERFORMANCE_OPTIMIZATION.md)
- [GameDesignDocument.md](./GameDesignDocument.md) 第12章

---

## 📊 文档状态

| 文档 | 状态 | 最后更新 |
|------|------|---------|
| AI_HANDOFF.md | ✅ 完整 | 2025-10-13 |
| PROJECT_CONTEXT.md | ✅ 完整 | 2025-10-13 |
| GameDesignDocument.md | ✅ 完整 | 2025-10-13 |
| ARCHITECTURE.md | ✅ 完整 | 2025-10-13 |
| DEVELOPMENT_LOG.md | ✅ 完整 | 2025-10-13 |
| features/DAILY_CHALLENGE_* | ✅ 完整 | 2025-10-13 |
| engineering/PERFORMANCE_OPTIMIZATION.md | ✅ 完整 | 2025-11-03 |

---

## 📋 文档维护规范

### 新增文档时
1. 确定文档类别（核心/功能/工程）
2. 放置在对应目录下
3. 更新对应目录的 README.md
4. 更新本文档的索引
5. 更新项目根目录的 README.md

### 文档命名规范
- 使用大写字母和下划线: `FEATURE_NAME.md`
- 功能文档: `FEATURE_NAME_GUIDE.md` (用户)、`FEATURE_NAME_IMPLEMENTATION.md` (技术)
- 工程文档: 描述性命名，如 `PERFORMANCE_OPTIMIZATION.md`

### 文档格式规范
- 使用 Markdown 格式
- 包含清晰的标题层级
- 添加目录（如果文档较长）
- 使用代码块展示代码
- 添加适当的 emoji 增强可读性

---

## 🔗 相关链接

- [项目 README](../README.md)
- [源代码](../src/)
- [游戏配置](../src/config/constants.ts)
