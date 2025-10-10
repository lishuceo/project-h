# 创新俄罗斯方块游戏 - 完整策划文档
**文档版本**: 4.1  
**日期**: 2025年10月10日  
**目标**: 为AI开发提供完整、清晰、无歧义的游戏设计规范

---

## ⚠️ 核心技术要点

本游戏的视觉特色来自**三方向下落物理系统**：

1. 每个像素块独立下落（不是整块方块）
2. 像素块优先向正下方移动，如果被占用则尝试斜向（左下/右下）滑落
3. 这种机制使方形方块自然形成**三角形堆积**（如沙堆）
4. 高性能设计：每帧每个像素块只检查3个相邻格子

**🔴 关键实现要点（AI必读）：**

1. **消除判定必须在像素网格层面进行BFS**（第8.2节）
   - ❌ 错误：在逻辑网格（12×22）进行BFS
   - ✅ 正确：在像素网格（120×220）进行BFS
   - 原因：三角形堆积导致逻辑格子中间为空，但像素层连续

2. **消除时必须删除BFS找到的原始像素块**（第8.2.3节）
   - ❌ 错误：通过逻辑坐标重新查找像素块
   - ✅ 正确：直接删除BFS返回的像素块数组
   - 原因：像素块可能滑落到逻辑格子之外

3. **稳定性检查必须循环进行**（第8.4节）
   - ❌ 错误：消除后只检查一次稳定性
   - ✅ 正确：循环检查直到没有新的不稳定像素块
   - 原因：像素块逐层失去支撑，需要多次检查

4. **判定稳定时只计算稳定的像素块作为支撑**（第8.4.2节）
   - ❌ 错误：不稳定的像素块也算支撑
   - ✅ 正确：只有已稳定的像素块才能提供支撑
   - 原因：不稳定的像素块会移走，不是真实支撑

**请重点阅读第5章"物理系统"和第8章"消除机制"！**

---

## 📋 目录
1. [游戏概述](#1-游戏概述)
2. [核心概念](#2-核心概念)
3. [游戏世界：网格系统](#3-游戏世界网格系统)
4. [方块系统](#4-方块系统)
5. [物理系统：重力与下落](#5-物理系统重力与下落)
6. [核心玩法：拖放机制](#6-核心玩法拖放机制)
7. [预览与选择系统](#7-预览与选择系统)
8. [消除机制](#8-消除机制)
9. [计分系统](#9-计分系统)
10. [游戏流程与状态机](#10-游戏流程与状态机)
11. [游戏结束条件](#11-游戏结束条件)
12. [UI与视觉反馈](#12-ui与视觉反馈)
13. [关键参数表](#13-关键参数表)
14. [开发实现建议](#14-开发实现建议)

---

## 1. 游戏概述

### 1.1 核心玩法
这是一款创新的俄罗斯方块游戏，融合了：
- **Block Blast式的拖放机制**：玩家可以自由拖动方块到任意位置
- **沙盒物理引擎**：每个方块由数百个独立的"像素块"组成，每个像素块都可以独立下落
- **横向连接消除**：当同色方块连接游戏区域的左右边界时，触发消除

### 1.2 游戏目标
- 策略性地放置方块，形成左右连通的同色集群进行消除
- 获得尽可能高的分数
- 保持游戏区域有足够的空间继续放置方块

### 1.3 核心特色
- ✨ **像素级物理模拟**：每个小颗粒独立受重力影响
- ✨ **自由拖放**：无传统俄罗斯方块的下落限制
- ✨ **连锁反应**：消除后可能触发连续的连锁消除
- ✨ **无时间压力**：纯策略游戏，玩家可以充分思考

---

## 2. 核心概念

### 2.1 双层系统架构

本游戏采用**逻辑层**和**物理层**分离的架构：

#### 逻辑层 (Logical Layer)
- 用于：方块形状定义、消除判定、计分计算
- 单位：逻辑格子 (Logical Cell)
- 特点：简化的网格系统，便于规则计算

#### 物理层 (Pixel Layer)  
- 用于：渲染、动画、物理模拟（重力、碰撞）
- 单位：像素块 (Pixel Block)
- 特点：精细的物理表现，创造沙盒质感

### 2.2 核心对应比例

```
1 个逻辑格子 = 10×10 个像素块
```

**示例**：
- 一个标准的T形俄罗斯方块（4个逻辑格子）= 400个像素块
- 一个2×2的正方形方块（4个逻辑格子）= 400个像素块

### 2.3 关键术语

| 术语 | 定义 |
|------|------|
| **逻辑格子** | 游戏规则层的基本单元（1格） |
| **像素块** | 渲染和物理层的基本单元（1个小方块） |
| **方块/Tetromino** | 由4个逻辑格子组成的标准俄罗斯方块形状 |
| **集群/Cluster** | 相连通的同色像素块集合 |
| **连通** | 像素块之间上下左右相邻（不包括对角） |
| **落定** | 像素块下落后达到稳定状态（下方有支撑或到达底部） |

---

## 3. 游戏世界：网格系统

### 3.1 逻辑网格 (Logical Grid)

```
尺寸: 12 (宽) × 22 (高) 逻辑格子
```

- **宽度**: 12 逻辑格子（列索引：0-11）
- **可见高度**: 22 逻辑格子（行索引：0-21，从上到下）
- **作用**: 
  - 定义方块形状
  - 判定消除条件
  - 计算分数
  - 检测游戏结束

**重要：游戏区域是统一的整体，不分左中右区域。**

### 3.2 像素网格 (Pixel Grid)

```
尺寸: 120 (宽) × 220 (高) 像素块
```

根据10:1的比例计算：
- **宽度**: 12 × 10 = 120 像素块
- **高度**: 22 × 10 = 220 像素块
- **作用**:
  - 渲染所有视觉元素
  - 执行物理模拟（重力、碰撞）
  - 播放动画效果
  - 实现粒子化消除

### 3.3 坐标系统

#### 逻辑坐标
```
(0, 0) 位于左上角
列 (Column/X): 0-11 (左→右)
行 (Row/Y): 0-21 (上→下)
```

#### 像素坐标
```
(0, 0) 位于左上角
X: 0-119 (左→右)
Y: 0-219 (上→下)
```

#### 坐标转换公式
```
逻辑坐标 → 像素坐标:
pixelX = logicalX × 10
pixelY = logicalY × 10

像素坐标 → 逻辑坐标:
logicalX = floor(pixelX / 10)
logicalY = floor(pixelY / 10)
```

### 3.4 边界定义

游戏区域的关键边界：
- **左边界**: 列 0 (像素块 X = 0-9)
- **右边界**: 列 11 (像素块 X = 110-119)
- **顶部边界**: 行 0 (像素块 Y = 0-9)
- **底部边界**: 行 21 (像素块 Y = 210-219)

**消除判定的关键**：同色集群必须同时触及左边界（列0）和右边界（列11）

---

## 4. 方块系统

### 4.1 标准方块形状 (7-Bag System)

游戏使用标准的7种俄罗斯方块形状：

| 形状 | 名称 | 逻辑格子数 | 像素块数 | 旋转状态 |
|------|------|-----------|----------|----------|
| I | 长条 | 4 | 400 | 2种 |
| O | 正方形 | 4 | 400 | 1种 |
| T | T形 | 4 | 400 | 4种 |
| L | L形 | 4 | 400 | 4种 |
| J | 反L形 | 4 | 400 | 4种 |
| S | S形 | 4 | 400 | 2种 |
| Z | Z形 | 4 | 400 | 2种 |

### 4.2 颜色系统

方块颜色池（4种颜色）：
1. 🔴 红色 (Red)
2. 🔵 蓝色 (Blue)
3. 🟢 绿色 (Green)
4. 🟡 黄色 (Yellow)


**颜色分配规则**：
- 每个方块的颜色从4种颜色中随机选择
- 一个方块内的所有像素块颜色相同
- 形状和颜色独立随机

### 4.3 方块生成规则 (7-Bag)

为了保证公平性和可预测性，采用7-Bag随机系统：

```python
# 伪代码
bag = [I, O, T, L, J, S, Z]  # 创建一个袋子
shuffle(bag)                  # 打乱顺序
for shape in bag:
    color = random_choice([红, 蓝, 绿, 黄, 紫, 白])
    generate_tetromino(shape, color)
# 当袋子用完，重新填充并打乱
```

**保证**：在连续7个方块中，每种形状恰好出现1次

---

## 5. 物理系统：重力与下落

### 5.1 核心原则

**每个像素块都是独立的物理实体**

关键特性：
- ✅ 每个像素块独立检测碰撞
- ✅ 每个像素块独立下落
- ✅ 方块不作为整体移动（放置后立即分解为像素块）
- ✅ 形成自然的堆积效果（如沙堆、三角形堆积）

### 5.2 重力模拟算法（三方向下落规则）

**核心设计理念**：
- 高性能：每帧只检查3个相邻格子，无需复杂物理计算
- 山形堆积：通过侧向滑落自然形成三角形堆积效果
- 网格对齐：像素块始终在整数坐标上，无需浮点计算

#### 5.2.1 像素块数据结构

```javascript
class PixelBlock {
    x: number;              // X坐标 (0-119)
    y: number;              // Y坐标 (0-219)
    color: Color;           // 颜色
    isStable: boolean;      // 是否已稳定（落定）
    groupId: number;        // 所属方块组ID
}
```

**注意**：无需速度属性，每帧移动固定距离（1格）

#### 5.2.2 三方向下落算法

这是形成山形堆积的关键算法！

```javascript
function updatePixelPhysics(pixel) {
    if (pixel.isStable) return;  // 已稳定的不再更新
    
    const x = pixel.x;
    const y = pixel.y;
    
    // === 优先级1: 尝试正下方 ===
    if (canMoveTo(x, y + 1)) {
        movePixelTo(pixel, x, y + 1);
        return;  // 移动成功，继续下落
    }
    
    // === 正下方被占用，尝试斜向滑落 ===
    // 随机选择左右优先顺序（避免系统性偏向）
    const leftFirst = Math.random() > 0.5;
    const directions = leftFirst ? [-1, 1] : [1, -1];
    
    for (const dir of directions) {
        // 优先级2/3: 尝试斜下方 (左下或右下)
        const newX = x + dir;
        const newY = y + 1;
        
        if (canMoveTo(newX, newY)) {
            movePixelTo(pixel, newX, newY);
            return;  // 斜向滑落成功
        }
    }
    
    // === 三个方向都无法移动 → 稳定 ===
    pixel.isStable = true;
}

function canMoveTo(x, y) {
    // 边界检查
    if (x < 0 || x >= 120) return false;
    if (y < 0 || y >= 220) return false;
    
    // 检查目标位置是否为空
    return pixelGrid[y][x] === null;
}

function movePixelTo(pixel, newX, newY) {
    // 从旧位置移除
    pixelGrid[pixel.y][pixel.x] = null;
    
    // 更新坐标
    pixel.x = newX;
    pixel.y = newY;
    
    // 放置到新位置
    pixelGrid[newY][newX] = pixel;
}
```

### 5.3 山形堆积的形成原理

#### 5.3.1 为什么能形成三角形？

从您提供的截图可以看出，16×16像素块（2×2逻辑格子）的方块下落后形成了**规则的三角形堆积**。这是三方向下落规则的自然结果！

**关键机制**：
- ✅ **中心收敛**：中间的像素块正下方被占用，无法继续下落，堆积在高处
- ✅ **边缘扩散**：边缘的像素块可以斜向滑落到更低位置，形成斜坡
- ✅ **自平衡**：系统自动趋向于最稳定的三角形结构

#### 5.3.2 详细模拟过程（16×16方块）

让我们逐帧模拟一个16×16像素块的方形方块如何形成三角形：

**初始状态（T=0）**：方块放置在空中
```
■■■■■■■■■■■■■■■■
■■■■■■■■■■■■■■■■
■■■■■■■■■■■■■■■■
...（16行）
──────地面──────
```

**T=1帧**：所有像素块尝试下落
```
结果：全部向下移动1格
■■■■■■■■■■■■■■■■
■■■■■■■■■■■■■■■■
■■■■■■■■■■■■■■■■
...
```

**T=N帧**：底部像素块触地
```
(空中继续下落的像素块)
■■■■■■■■■■■■■■■■  ← 底层触地，标记为稳定
══════════════════════  地面
```

**T=N+1帧**：次底层像素块的命运分叉
```
分析：
- 中间8列：正下方有像素块 → 无法下落 → 尝试斜下 → 斜下也被占用 → 稳定
- 左边4列：正下方有像素块 → 尝试左下 → 左下为空！→ 滑落
- 右边4列：正下方有像素块 → 尝试右下 → 右下为空！→ 滑落

结果：
    ■■■■■■■■        ← 中间稳定
  ■■          ■■    ← 边缘滑落到两侧
■■■■■■■■■■■■■■■■  ← 底层
```

**继续迭代...**
```
T=N+5:
      ■■■■          ← 顶部
    ■■■■■■■■        ← 中层
  ■■■■■■■■■■■■    ← 次层
■■■■■■■■■■■■■■■■  ← 底层

最终形成完美的三角形！
```

#### 5.3.3 数学分析

对于一个宽度为W的方形方块，最终形成的三角形高度为：

```
最终高度 ≈ W（在平坦地面上）
底宽 = W
顶宽 ≈ 1-2像素块

坡度 ≈ 45°（因为每下降1格，横向扩散最多1格）
```

**16×16方块的理论堆积**：
- 底层：16像素宽
- 每上升1层，宽度减少约2像素（左右各1）
- 最终高度：约14-16层
- 形成近似等腰三角形

### 5.4 更新顺序与同步性

#### 5.4.1 关键问题：更新顺序重要吗？

**答案：重要！** 必须采用特定的更新顺序，避免"穿透"或"双重移动"。

**错误示例**（从上往下更新）：
```
初始：
  ■ (像素A，即将下落)
  □ (空)
  ■ (像素B，即将下落)
  □ (空)
地面

从上往下更新：
1. 更新像素A → 移动到下方空格
2. 更新像素B → 移动到下方空格
3. 再次循环到像素A（现在在新位置）→ 又移动了一次！

结果：像素A在一帧内移动了2次（BUG！）
```

#### 5.4.2 正确的更新算法

**解决方案：从下往上更新 + 标记已更新**

```javascript
function updateAllPixels() {
    // 清除上一帧的更新标记
    for (let y = 0; y < 220; y++) {
        for (let x = 0; x < 120; x++) {
            const pixel = pixelGrid[y][x];
            if (pixel) {
                pixel.updatedThisFrame = false;
            }
        }
    }
    
    // 从下往上、从左往右更新
    for (let y = 219; y >= 0; y--) {  // 从底部开始
        for (let x = 0; x < 120; x++) {
            const pixel = pixelGrid[y][x];
            
            // 跳过空格、已稳定、已更新的像素块
            if (!pixel || pixel.isStable || pixel.updatedThisFrame) {
                continue;
            }
            
            // 更新该像素块
            updatePixelPhysics(pixel);
            
            // 标记为已更新（即使移动到新位置，本帧也不再更新）
            pixel.updatedThisFrame = true;
        }
    }
}
```

**为什么从下往上？**
- 保证下方的像素块先稳定
- 上方的像素块基于准确的下方状态做决策
- 避免"连锁移动"Bug

#### 5.4.3 完整的游戏循环

```javascript
function gameLoop() {
    requestAnimationFrame(gameLoop);
    
    // 1. 更新所有活跃像素块的物理状态
    updateAllPixels();
    
    // 2. 检查是否所有像素块都已稳定
    const allStable = checkAllPixelsStable();
    
    if (allStable) {
        // 3. 全部稳定后，进行消除检测
        const eliminationOccurred = performEliminationCheck();
        
        if (eliminationOccurred) {
            // 4. 消除触发了，播放动画
            playEliminationAnimation();
            // 注意：动画完成后会重新触发重力，回到步骤1
        } else {
            // 5. 无消除，检查游戏结束
            checkGameOver();
            // 6. 返回空闲状态，等待玩家操作
            gameState = GameState.IDLE;
        }
    }
    
    // 7. 渲染
    render();
}

function checkAllPixelsStable() {
    for (let y = 0; y < 220; y++) {
        for (let x = 0; x < 120; x++) {
            const pixel = pixelGrid[y][x];
            if (pixel && !pixel.isStable) {
                return false;  // 发现未稳定的像素块
            }
        }
    }
    return true;  // 全部稳定
}
```

### 5.5 性能优化策略

#### 5.5.1 活跃像素块追踪

不需要每帧扫描整个120×220网格，只追踪活跃（未稳定）的像素块：

```javascript
class PhysicsManager {
    activePixels = new Set();  // 活跃像素块集合
    
    addPixel(pixel) {
        this.activePixels.add(pixel);
        pixel.isStable = false;
    }
    
    update() {
        const pixelsToUpdate = Array.from(this.activePixels);
        
        // 按Y坐标排序（从下往上）
        pixelsToUpdate.sort((a, b) => b.y - a.y);
        
        for (const pixel of pixelsToUpdate) {
            if (pixel.isStable) {
                this.activePixels.delete(pixel);  // 移除已稳定的
                continue;
            }
            
            updatePixelPhysics(pixel);
        }
    }
    
    get allStable() {
        return this.activePixels.size === 0;
    }
}
```

**优化效果**：
- 空闲时：0次循环（无活跃像素块）
- 放置16×16方块：最多256次循环
- 大规模消除后：可能数千次循环，但远少于26,400次（120×220）

#### 5.5.2 脏区域标记

```javascript
class DirtyRegion {
    minX = Infinity;
    maxX = -Infinity;
    minY = Infinity;
    maxY = -Infinity;
    
    markDirty(x, y) {
        this.minX = Math.min(this.minX, x);
        this.maxX = Math.max(this.maxX, x);
        this.minY = Math.min(this.minY, y);
        this.maxY = Math.max(this.maxY, y);
    }
    
    contains(x, y) {
        return x >= this.minX && x <= this.maxX &&
               y >= this.minY && y <= this.maxY;
    }
}
```

只更新和渲染变化的区域，大幅提升性能。

#### 5.5.3 分层渲染

```javascript
// 已稳定的像素块 → 缓存为静态纹理
const stableLayer = createRenderTexture();

// 活跃的像素块 → 动态渲染
const activeLayer = createRenderTexture();

function render() {
    // 只在稳定层变化时重新渲染
    if (stableLayerDirty) {
        renderStablePixels(stableLayer);
        stableLayerDirty = false;
    }
    
    // 每帧渲染活跃层
    renderActivePixels(activeLayer);
    
    // 合成
    compositeToScreen([stableLayer, activeLayer]);
}
```

### 5.6 边缘情况处理

#### 5.6.1 边界滑落

像素块在游戏区域边缘时的行为：

```javascript
// 左边界 (x=0)
if (x === 0) {
    // 只能尝试：正下、右下
    // 不能尝试左下（越界）
}

// 右边界 (x=119)
if (x === 119) {
    // 只能尝试：正下、左下
    // 不能尝试右下（越界）
}
```

这会在边界形成更陡峭的堆积。

#### 5.6.2 极限堆积

当游戏区域接近满时（密度>80%）：

```javascript
// 可选：限制活跃像素块数量
if (activePixels.size > 10000) {
    // 强制稳定一部分像素块
    forceLowerLayersStable();
}
```

#### 5.6.3 同时放置多个方块

玩家在物理模拟期间放置新方块时：

```javascript
function placeNewTetromino(tetromino, x, y) {
    const newPixels = createPixelsFromTetromino(tetromino, x, y);
    
    // 为新方块分配独立的groupId
    const groupId = generateGroupId();
    
    for (const pixel of newPixels) {
        pixel.groupId = groupId;
        physicsManager.addPixel(pixel);
    }
    
    // 不会干扰正在下落的其他方块组
}
```

---

## 6. 核心玩法：拖放机制

### 6.1 拖放流程

```
1. 玩家点击预览槽位中的方块
2. 方块跟随手指/鼠标移动（拖动状态）
3. 实时检测当前位置是否可放置
4. 玩家松手（尝试放置）
5. 验证放置合法性
   ├─ 合法 → 放置成功 → 触发物理下落
   └─ 非法 → 拒绝放置 → 方块返回预览槽位
```

### 6.2 拖动状态

当玩家拖动方块时：

**视觉反馈**：
- 方块显示为**半透明**或**高亮**状态
- 方块跟随指针移动
- 实时显示网格吸附预览

**碰撞检测**：
- 实时检测方块与已有像素块的重叠
- 重叠部分以**红色高亮**或**警告色**显示
- 非重叠部分显示为正常颜色或绿色提示

### 6.3 放置验证规则

#### 6.3.1 合法放置条件

方块可以放置，当且仅当：
1. ✅ 方块完全在游戏区域内（列0-11，行0-21）
2. ✅ 方块的所有逻辑格子位置都没有已存在的像素块（**完全无重叠**）

#### 6.3.2 放置位置转换

```javascript
function snapToGrid(screenX, screenY) {
    // 将屏幕坐标转换为逻辑网格坐标
    const logicalX = Math.floor(screenX / CELL_SIZE);
    const logicalY = Math.floor(screenY / CELL_SIZE);
    
    // 吸附到最近的网格位置
    return { x: logicalX, y: logicalY };
}

function canPlaceTetromino(tetromino, gridX, gridY) {
    // 检查方块的每个逻辑格子
    for (const cell of tetromino.cells) {
        const checkX = gridX + cell.x;
        const checkY = gridY + cell.y;
        
        // 越界检查
        if (checkX < 0 || checkX >= 12 || checkY < 0 || checkY >= 22) {
            return false;
        }
        
        // 重叠检查（检查对应的10x10像素块区域）
        for (let py = 0; py < 10; py++) {
            for (let px = 0; px < 10; px++) {
                const pixelX = checkX * 10 + px;
                const pixelY = checkY * 10 + py;
                if (pixelGrid[pixelY][pixelX] !== null) {
                    return false;  // 发现重叠
                }
            }
        }
    }
    return true;  // 可以放置
}
```

#### 6.3.3 放置执行

```javascript
function placeTetromino(tetromino, gridX, gridY) {
    // 将方块的每个逻辑格子转换为像素块
    for (const cell of tetromino.cells) {
        const startX = (gridX + cell.x) * 10;
        const startY = (gridY + cell.y) * 10;
        
        // 创建10x10的像素块
        for (let py = 0; py < 10; py++) {
            for (let px = 0; px < 10; px++) {
                const pixel = new PixelBlock({
                    x: startX + px,
                    y: startY + py,
                    color: tetromino.color,
                    velocityY: 0,
                    isStable: false  // 初始为不稳定状态
                });
                pixelGrid[startY + py][startX + px] = pixel;
            }
        }
    }
    
    // 触发物理更新
    startPhysicsSimulation();
}
```

### 6.4 特殊规则

#### 空中放置
- ✅ **允许**：玩家可以将方块放置在空中（下方无支撑）
- 放置后，方块的像素块会立即受重力影响开始下落
- 这是核心玩法的关键特性，允许玩家进行策略性的"空投"

#### 拖动限制
- 拖动过程中方块不受重力影响
- 方块始终保持形状完整（不会提前分解）
- 只有在放置（松手）后，方块才分解为独立像素块

---

## 7. 预览与选择系统

### 7.1 预览槽位

**固定槽位数**：3个

```
┌─────────┬─────────┬─────────┐
│  槽位1  │  槽位2  │  槽位3  │
│  [🔴T]  │  [🔵I]  │  [🟢L]  │
└─────────┴─────────┴─────────┘
```

### 7.2 选择规则

- **自由选择**：玩家可以任意选择3个槽位中的任一方块使用
- **无顺序限制**：不需要按照从左到右或任何特定顺序使用
- **点击选中**：玩家点击某个槽位，该方块进入拖动状态

### 7.3 补充机制

#### 立即补充
```
使用方块的流程：
1. 玩家选择槽位1的方块
2. 成功放置后（或取消放置）
3. 槽位1立即生成新的随机方块
4. 槽位2和槽位3保持不变
```

**不是**3个都用完后才整体刷新

#### 补充时机
- ✅ 方块成功放置 → 立即补充
- ✅ 方块放置失败返回 → 不补充（方块还在槽位中）

### 7.4 生成规则

每个新生成的方块：
1. 形状：从7-Bag系统中抽取
2. 颜色：从4种颜色中随机选择
3. 初始状态：未旋转的标准形态

```javascript
function refillSlot(slotIndex) {
    const shape = bagSystem.getNextShape();  // 7-Bag
    const color = randomColor();             // 4色随机
    const tetromino = createTetromino(shape, color);
    slots[slotIndex] = tetromino;
}
```

---

## 8. 消除机制

这是游戏的核心机制，决定了玩家的策略和得分。

### 8.1 消除触发条件

消除判定在以下时机触发：
```
方块放置 → 像素块下落 → 全部落定 → 消除检测
```

**关键**：必须等待所有像素块完全稳定（`isStable = true`）后，才进行消除检测

### 8.2 消除判定算法

**⚠️ 重要修正：必须在像素网格层面进行BFS！**

由于三方向下落物理会导致像素块形成三角形堆积，**逻辑网格中间可能是空的**，但视觉上是连续的。因此必须在像素网格层面进行连通性判定。

#### 8.2.1 像素网格BFS搜索

直接在120×220的像素网格上进行BFS：

```javascript
function findConnectedPixelClusters() {
    const visited = new Set(); // 使用"x,y"字符串作为key
    const clusters = [];
    
    // 遍历整个像素网格
    for (let y = 0; y < 220; y++) {
        for (let x = 0; x < 120; x++) {
            const pixel = pixelGrid[y][x];
            const key = `${x},${y}`;
            
            if (pixel && !visited.has(key)) {
                // 发现新的未访问像素块，进行BFS
                const cluster = bfsPixelSearch(x, y, pixel.color, visited);
                clusters.push(cluster);
            }
        }
    }
    
    return clusters;
}

function bfsPixelSearch(startX, startY, color, visited) {
    const queue = [{x: startX, y: startY}];
    const cluster = {
        color: color,
        pixels: [],  // 保存像素块引用
        touchesLeft: false,
        touchesRight: false
    };
    
    visited.add(`${startX},${startY}`);
    
    while (queue.length > 0) {
        const {x, y} = queue.shift();
        const pixel = pixelGrid[y][x];
        
        if (pixel) {
            cluster.pixels.push(pixel);
            
            // 检查是否触及边界（像素网格边界）
            if (x === 0) cluster.touchesLeft = true;
            if (x === 119) cluster.touchesRight = true;
            
            // 检查四个方向的相邻像素块
            const neighbors = [
                {x: x-1, y: y},  // 左
                {x: x+1, y: y},  // 右
                {x: x, y: y-1},  // 上
                {x: x, y: y+1}   // 下
            ];
            
            for (const {x: nx, y: ny} of neighbors) {
                const key = `${nx},${ny}`;
                
                // 边界检查
                if (nx < 0 || nx >= 120 || ny < 0 || ny >= 220) continue;
                
                // 已访问检查
                if (visited.has(key)) continue;
                
                // 颜色匹配检查
                const neighborPixel = pixelGrid[ny][nx];
                if (neighborPixel && neighborPixel.color === color) {
                    visited.add(key);
                    queue.push({x: nx, y: ny});
                }
            }
        }
    }
    
    return cluster;
}
```

**为什么必须在像素层面？**

```
三角形堆积示例：

视觉上（像素层）：     逻辑层：
■■■■■■■■■■■■    ■■■□□□□□■■■
  ■■■■■■■■■■        ■■□□□□□■■
    ■■■■■■■■            ■■□□■■
      ■■■■                  ■■

像素层连续 ✅           逻辑层断开 ❌
```

#### 8.2.2 边界触及判定

```javascript
function checkElimination() {
    const pixelClusters = findConnectedPixelClusters();
    
    // 筛选同时触及左右边界的集群
    const eliminationClusters = pixelClusters.filter(
        cluster => cluster.touchesLeft && cluster.touchesRight
    );
    
    return eliminationClusters;
}
```

**关键**：边界判定也在像素层面（X=0 和 X=119）

#### 8.2.3 消除执行

**重要**：消除时必须删除BFS找到的原始像素块，而不是通过逻辑坐标重新查找！

```javascript
function eliminateCluster(pixelCluster) {
    console.log(`开始删除 ${pixelCluster.pixels.length} 个像素块`);
    
    // 直接删除BFS找到的所有像素块
    pixelCluster.pixels.forEach(pixel => {
        pixelGrid[pixel.y][pixel.x] = null;
    });
    
    console.log(`删除完成`);
}
```

**错误做法**：❌ 通过逻辑格子坐标重新查找像素块
```javascript
// 错误！可能漏掉滑落到斜角的像素块
for (const cell of cluster.cells) {
    const startX = cell.x * 10;
    const startY = cell.y * 10;
    for (let py = 0; py < 10; py++) {
        for (let px = 0; px < 10; px++) {
            pixelGrid[startY + py][startX + px] = null;
        }
    }
}
```

### 8.3 消除执行

#### 8.3.1 消除动画

```javascript
function eliminateCluster(cluster) {
    // 1. 标记要消除的像素块
    const pixelsToEliminate = [];
    
    for (const cell of cluster.cells) {
        const startX = cell.x * 10;
        const startY = cell.y * 10;
        
        // 收集该逻辑格子对应的所有像素块
        for (let py = 0; py < 10; py++) {
            for (let px = 0; px < 10; px++) {
                const pixel = pixelGrid[startY + py][startX + px];
                if (pixel && pixel.color === cluster.color) {
                    pixelsToEliminate.push(pixel);
                }
            }
        }
    }
    
    // 2. 播放粒子化消除动画
    playParticleAnimation(pixelsToEliminate);
    
    // 3. 1秒后移除这些像素块
    setTimeout(() => {
        for (const pixel of pixelsToEliminate) {
            pixelGrid[pixel.y][pixel.x] = null;
        }
        
        // 4. 触发重力结算
        triggerGravity();
    }, 1000);  // 动画持续1秒
}
```

#### 8.3.2 粒子化动画

```javascript
function playParticleAnimation(pixels) {
    for (const pixel of pixels) {
        // 为每个像素块创建粒子效果
        createParticle({
            x: pixel.x,
            y: pixel.y,
            color: pixel.color,
            velocityX: random(-50, 50),
            velocityY: random(-100, 0),
            lifetime: 1000,  // 1秒
            fadeOut: true
        });
    }
}
```

### 8.4 连锁消除机制与稳定性重新检查

**核心规则**：消除后触发的重力下落可能形成新的连接，从而触发新的消除

**⚠️ 关键难点：消除后的稳定性检查**

#### 8.4.1 消除后重力处理的挑战

消除像素块后，上方的像素块失去支撑，但有一个微妙的问题：

```
消除前：               消除后：              第一次检查：          实际应该：
  C                     C                    C（稳定）            C（不稳定）
  B                     B                    B（稳定）            B（不稳定）
  A                     A                    A（不稳定） ✓        A（不稳定）
■■■ 消除这层  →    □□□ 空               □□□ 空             □□□ 空
```

**问题**：A下落后，B和C才失去支撑，但第一次检查时B和C看起来有支撑（因为A还在）。

#### 8.4.2 正确的稳定性检查算法

**方案1：增量检查（推荐）**

只检查已稳定的像素块，判断它们下方是否**只有稳定的像素块**作为支撑：

```javascript
function recheckStability() {
    const allPixels = getAllPixels();
    let newUnstableCount = 0;
    
    allPixels.forEach(pixel => {
        // 跳过已经不稳定的像素块
        if (!pixel.isStable || activePixels.has(pixel)) {
            return;
        }
        
        // 检查三个方向是否有真实支撑（只计算稳定的像素块）
        const hasSupport = hasStableSupportBelow(pixel.x, pixel.y);
        
        if (!hasSupport) {
            pixel.isStable = false;
            activePixels.add(pixel);
            newUnstableCount++;
        }
    });
    
    return newUnstableCount;
}

function hasStableSupportBelow(x, y) {
    // 检查正下方、左下方、右下方
    // 只要有一个方向为空或有不稳定像素块，就认为没有真实支撑
    const canMoveDown = isEmptyOrUnstable(x, y+1);
    const canMoveLeftDown = isEmptyOrUnstable(x-1, y+1);
    const canMoveRightDown = isEmptyOrUnstable(x+1, y+1);
    
    // 三个方向都必须被稳定的像素块占据
    return !canMoveDown && !canMoveLeftDown && !canMoveRightDown;
}

function isEmptyOrUnstable(x, y) {
    if (越界) return false;
    const pixel = pixelGrid[y][x];
    return pixel === null || !pixel.isStable;
}
```

**方案2：循环检查**

每次物理稳定后，自动再次检查：

```javascript
function gameLoop() {
    updatePhysics();
    
    if (allPixelsStable()) {
        // 再次检查是否有新的不稳定像素块
        const newUnstable = recheckStability();
        
        if (newUnstable > 0) {
            // 发现新的不稳定像素块，继续物理模拟
            continue;
        } else {
            // 真正全部稳定，进入消除检测
            checkElimination();
        }
    }
}
```

#### 8.4.3 连锁消除流程

```javascript
function handleEliminationChain() {
    let chainCount = 0;
    let totalScore = 0;
    
    while (true) {
        // 1. 检查消除
        const eliminationClusters = checkElimination();
        
        if (eliminationClusters.length === 0) {
            break;  // 没有可消除的集群，结束连锁
        }
        
        chainCount++;
        
        // 2. 执行消除（直接删除像素块）
        for (const cluster of eliminationClusters) {
            const score = calculateScore(cluster.pixels.length);
            totalScore += score * chainCount;  // 连锁加成
            eliminatePixels(cluster.pixels);
        }
        
        // 3. 重新检查稳定性
        recheckStability();
        
        // 4. 等待所有像素块落定
        await waitForAllPixelsStable();
        
        // 5. 继续检查（可能触发新的连锁）
    }
    
    // 显示总得分
    showScore(totalScore, chainCount);
}
```

**连锁加成**：
- 第1次消除：基础分数 × 1
- 第2次消除：基础分数 × 2
- 第3次消除：基础分数 × 3
- ...

### 8.5 消除示例

#### 示例1：简单消除
```
初始状态（俯视图，R=红色，B=蓝色，.=空）：
列: 0  1  2  3  4  5  6  7  8  9 10 11
   [R][R][R][R][R][R][R][R][R][R][R][R]  ← 红色横跨左右边界
   [B][B][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]
   
动作：玩家在列0-11的空隙处放置红色方块，填补中间
结果：红色集群连接列0和列11 → 消除整行红色
```

#### 示例2：复杂连通
```
初始状态：
列: 0  1  2  3  4  5  6  7  8  9 10 11
   [R][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][R]
   [R][R][ ][ ][ ][ ][ ][ ][ ][ ][R][R]
   [R][R][R][ ][ ][ ][ ][ ][ ][R][R][R]
   
动作：玩家放置红色方块，连接中间断开的部分
结果：所有红色形成一个连通集群，触及列0和列11 → 消除
```

#### 示例3：文字描述（用户提供）
> "玩家把红色方形方块放在中间位置，方块掉落后，红色像素块连接了左边的游戏区域边界和右边的游戏区域边界，于是所有连通的红色像素块都被消除，得分+XXX，上方的其他颜色方块因为失去支撑继续下落。"

---

## 9. 计分系统

### 9.1 计分基础

**计分单位**：消除的逻辑格子数量 (N)

根据用户反馈，消除1056个逻辑格子得373分，推导公式：

### 9.2 计分公式

```javascript
function calculateScore(cellsEliminated) {
    const N = cellsEliminated;
    
    // 简化公式（根据实际数据调整）
    // 1056格 ≈ 373分，推测公式可能是线性或平方根关系
    
    // 方案A：平方根公式
    const score = Math.floor(10 * Math.sqrt(N));
    
    // 方案B：分段线性公式
    // const score = Math.floor(N * 0.35);
    
    return score;
}
```

**注**：需要根据实际游戏测试调整公式，确保得分曲线合理

### 9.3 连锁加成

```javascript
function calculateChainScore(baseScore, chainLevel) {
    return baseScore * chainLevel;
}
```

示例：
- 第1次消除100格 → 基础分100 × 1 = 100分
- 连锁第2次消除50格 → 基础分50 × 2 = 100分
- 连锁第3次消除30格 → 基础分30 × 3 = 90分
- 总分 = 290分

### 9.4 特殊加成（可选）

未来可扩展的加成机制：
- **巨大消除加成**：一次消除超过1000格 → 额外+50%
- **完美清屏加成**：消除后游戏区域完全清空 → 额外+200分
- **连续消除奖励**：连续N次放置都触发消除 → 额外倍数

---

## 10. 游戏流程与状态机

### 10.1 游戏状态

```javascript
enum GameState {
    READY,              // 准备状态（游戏开始前）
    IDLE,               // 空闲状态（等待玩家操作）
    DRAGGING,           // 拖动状态（玩家正在拖动方块）
    PLACING,            // 放置状态（验证并放置方块）
    PHYSICS_RUNNING,    // 物理模拟状态（像素块下落中）
    CHECKING_ELIMINATION, // 消除检测状态
    ELIMINATING,        // 消除动画播放中
    GAME_OVER           // 游戏结束
}
```

### 10.2 状态转换流程图

```
[READY] 
   ↓ (游戏开始)
[IDLE] ←──────────────────┐
   ↓ (点击方块)            │
[DRAGGING]                │
   ↓ (松手)                │
[PLACING]                 │
   ├─ 失败 → 返回 IDLE     │
   └─ 成功 ↓               │
[PHYSICS_RUNNING]         │
   ↓ (全部落定)            │
[CHECKING_ELIMINATION]    │
   ├─ 无消除 → 返回 IDLE ──┘
   └─ 有消除 ↓
[ELIMINATING]
   ↓ (动画完成)
[PHYSICS_RUNNING] (重力结算)
   ↓ (全部落定)
[CHECKING_ELIMINATION]
   └─ (循环，直到无消除)
   ↓
检查游戏结束条件
   ├─ 未结束 → [IDLE]
   └─ 结束 → [GAME_OVER]
```

### 10.3 关键时序

#### 时序1：正常放置流程
```
T=0    玩家松手
T=0    验证放置 (瞬间)
T=0    放置成功，创建像素块
T=0    开始物理模拟
T=0.5  部分像素块落定
T=1.2  所有像素块落定
T=1.2  消除检测
T=1.2  发现连接，开始消除动画
T=2.2  动画完成，移除像素块
T=2.2  开始新一轮重力结算
T=3.0  全部落定
T=3.0  消除检测（无消除）
T=3.0  返回IDLE，玩家可以继续操作
```

#### 时序2：玩家可操作性
**关键设计**：玩家在物理模拟和消除动画期间**可以继续放置**新方块

```javascript
function canPlayerPlaceBlock() {
    // 允许玩家操作的状态
    return gameState === GameState.IDLE ||
           gameState === GameState.PHYSICS_RUNNING ||
           gameState === GameState.ELIMINATING;
}
```

**但注意**：
- 在`PHYSICS_RUNNING`或`ELIMINATING`期间放置的新方块，不参与当前的消除检测
- 新方块的消除检测要等到它自己完全落定后才进行

### 10.4 并发处理

游戏中可能同时存在多个"活跃方块组"（不同时间放置的方块）：

```javascript
class ActiveBlockGroup {
    id: number;
    pixels: PixelBlock[];
    placedTime: number;
    isFullyStable: boolean;
    hasBeenChecked: boolean;
}

// 管理多个活跃组
const activeGroups = [];

function onBlockPlaced(tetromino, position) {
    const group = new ActiveBlockGroup(tetromino, position);
    activeGroups.push(group);
}

function updatePhysics() {
    for (const group of activeGroups) {
        if (!group.isFullyStable) {
            // 更新该组的物理状态
            updateGroupPhysics(group);
            
            // 检查是否全部落定
            if (allPixelsStable(group)) {
                group.isFullyStable = true;
                group.hasBeenChecked = false;
            }
        }
    }
}

function checkEliminationQueue() {
    // 按放置时间顺序检查消除
    for (const group of activeGroups) {
        if (group.isFullyStable && !group.hasBeenChecked) {
            performEliminationCheck(group);
            group.hasBeenChecked = true;
        }
    }
}
```

---

## 11. 游戏结束条件

### 11.1 结束判定

游戏结束的唯一条件：
```
预览槽位中的3个方块都无法放置到游戏区域的任何位置
```

### 11.2 判定算法

```javascript
function checkGameOver() {
    const slot1 = previewSlots[0];
    const slot2 = previewSlots[1];
    const slot3 = previewSlots[2];
    
    const canPlace1 = canPlaceAnywhere(slot1);
    const canPlace2 = canPlaceAnywhere(slot2);
    const canPlace3 = canPlaceAnywhere(slot3);
    
    if (!canPlace1 && !canPlace2 && !canPlace3) {
        gameState = GameState.GAME_OVER;
        showGameOverScreen();
    }
}

function canPlaceAnywhere(tetromino) {
    // 遍历所有可能的位置
    for (let y = 0; y < 22; y++) {
        for (let x = 0; x < 12; x++) {
            if (canPlaceTetromino(tetromino, x, y)) {
                return true;  // 找到至少一个可放置位置
            }
        }
    }
    return false;  // 无处可放
}
```

### 11.3 判定时机

在以下时刻触发游戏结束检测：
1. 玩家尝试放置方块但失败时
2. 方块成功放置并完成所有消除后

**优化**：只在玩家操作后检测，不需要每帧检测

### 11.4 边缘情况

**Q**: 如果玩家在物理模拟期间放置方块失败，游戏区域仍在变化，是否应该等待？  
**A**: 不等待。游戏结束判定基于当前帧的游戏区域状态。

---

## 12. UI与视觉反馈

### 12.1 游戏界面布局

```
┌─────────────────────────────────┐
│  上滑锁定 2.0x 播放              │ ← 顶部控制栏
├─────────────────────────────────┤
│           进度条                 │ ← 可选：进度/目标显示
├─────────────────────────────────┤
│                                 │
│                                 │
│       游戏区域 (12×22)          │
│                                 │
│         [像素块堆积]             │
│                                 │
│                                 │
├─────────────────────────────────┤
│  ┌────┐  ┌────┐  ┌────┐        │
│  │ T  │  │ I  │  │ L  │        │ ← 预览槽位 (3个)
│  └────┘  └────┘  └────┘        │
├─────────────────────────────────┤
│  [道具] [商店] [暂停]           │ ← 底部功能按钮
└─────────────────────────────────┘
```

### 12.2 视觉反馈清单

#### 拖动反馈
- ✅ 方块跟随指针移动
- ✅ 方块显示为半透明（透明度60-80%）
- ✅ 网格吸附预览（虚线边框显示放置位置）
- ✅ 碰撞检测：
  - 绿色/正常色：可放置
  - 红色/警告色：不可放置（有重叠）
- ✅ 重叠部分高亮显示

#### 放置反馈
- ✅ 放置成功：
  - 方块瞬间变为不透明
  - 播放"放置"音效
  - 开始下落动画
- ✅ 放置失败：
  - 方块震动动画
  - 播放"错误"音效
  - 方块返回预览槽位

#### 物理反馈
- ✅ 像素块独立下落（逐帧动画）
- ✅ 落地时的轻微弹跳效果（可选）
- ✅ 堆积时的沙堆效果

#### 消除反馈
- ✅ 消除前：
  - 连接的集群闪烁或高亮（0.2秒）
  - 显示即将消除的范围
- ✅ 消除中：
  - 粒子化效果（像素块炸开、飞散、淡出）
  - 持续1秒
  - 播放"消除"音效
- ✅ 消除后：
  - 显示得分文字（弹出动画）
  - 连锁时显示"Chain x2!"、"Chain x3!"
  - 总分累加动画

### 12.3 分数显示

```
┌─────────────────┐
│  得分: 12,580   │ ← 实时更新
│  连锁: x3       │ ← 连锁倍数
│  +373           │ ← 本次得分（淡出）
└─────────────────┘
```

### 12.4 预览槽位UI

```
┌─────────┐ ┌─────────┐ ┌─────────┐
│  [🔴T]  │ │  [🔵I]  │ │  [🟢L]  │
│  可用   │ │  可用   │ │  不可用 │ ← 状态指示
└─────────┘ └─────────┘ └─────────┘
    ↑           ↑           ↑
  可点击      可点击      灰色禁用
```

如果某个方块无法放置到任何位置，该槽位显示为灰色或有禁用标记。

---

## 13. 关键参数表

| 参数名 | 值 | 描述 |
|--------|---|------|
| `LOGICAL_GRID_WIDTH` | 12 | 逻辑网格宽度（列数） |
| `LOGICAL_GRID_HEIGHT` | 22 | 逻辑网格高度（行数） |
| `CELL_TO_PIXEL_RATIO` | 10 | 1个逻辑格子 = 10×10像素块 |
| `PIXEL_GRID_WIDTH` | 120 | 像素网格宽度 |
| `PIXEL_GRID_HEIGHT` | 220 | 像素网格高度 |
| `PREVIEW_SLOTS` | 3 | 预览槽位数量 |
| `TETROMINO_SHAPES` | 7 | 标准俄罗斯方块形状数 (I,O,T,L,J,S,Z) |
| `COLOR_POOL` | 6 | 颜色种类数（红蓝绿黄紫白） |
| `GRAVITY` | 980 | 重力加速度（像素块/秒²） |
| `MAX_FALL_SPEED` | 500 | 最大下落速度（像素块/秒） |
| `PHYSICS_TIME_STEP` | 1/60 | 物理更新频率（60 FPS） |
| `ELIMINATION_ANIM_DURATION` | 1000ms | 消除动画持续时间 |
| `PARTICLE_LIFETIME` | 1000ms | 粒子生命周期 |
| `CHAIN_MULTIPLIER` | x链数 | 连锁得分倍数 |

---

## 14. 开发实现建议

### 14.1 技术栈建议

#### 前端（Web）
- **游戏引擎**: Phaser.js / PixiJS / Three.js
- **物理引擎**: Matter.js（用于像素块物理）或自定义简化物理
- **UI框架**: React / Vue（用于UI层）
- **状态管理**: Redux / Zustand

#### 移动端（原生）
- **iOS**: SpriteKit / Metal
- **Android**: LibGDX / Unity2D
- **跨平台**: Flutter + Flame / React Native

### 14.2 数据结构建议

```javascript
// 像素网格（物理层）
const pixelGrid = Array(220).fill(null).map(() => 
    Array(120).fill(null)
);

// 逻辑网格（规则层）
const logicalGrid = Array(22).fill(null).map(() => 
    Array(12).fill(null)
);

// 像素块对象
class PixelBlock {
    x: number;              // X坐标 (0-119)
    y: number;              // Y坐标 (0-219)
    color: Color;           // 颜色枚举
    velocityY: number;      // Y方向速度
    isStable: boolean;      // 是否已落定
    groupId: number;        // 所属方块组ID
}

// 方块对象
class Tetromino {
    shape: ShapeType;       // I,O,T,L,J,S,Z
    color: Color;           // 颜色
    rotation: number;       // 旋转状态 (0,1,2,3)
    cells: Array<{x, y}>;   // 相对坐标
}

// 活跃方块组
class ActiveBlockGroup {
    id: number;
    pixels: PixelBlock[];
    placedTime: number;
    isFullyStable: boolean;
    hasBeenChecked: boolean;
}
```

### 14.3 性能优化建议

#### 像素块管理
- 使用**对象池**重用PixelBlock对象，避免频繁创建销毁
- 使用**空间分区**（四叉树/网格）加速碰撞检测
- 已落定的像素块不再参与物理更新

#### 渲染优化
- 使用**Sprite批处理**减少Draw Call
- 已稳定区域使用**静态纹理缓存**
- 只渲染可见区域（视口裁剪）

#### 消除检测优化
- 不是每帧检测，只在像素块全部落定后检测
- 使用**增量BFS**，只从新放置的方块位置开始搜索
- 缓存连通性结果

### 14.4 调试工具建议

开发时应该提供的调试功能：
- ✅ 显示逻辑网格线
- ✅ 显示像素块ID和状态
- ✅ 显示集群边界
- ✅ 慢动作播放（0.5x, 0.1x）
- ✅ 单步执行物理更新
- ✅ 手动触发消除检测
- ✅ 显示性能统计（FPS, 像素块数量）

### 14.5 测试用例

#### 基础功能测试
1. 方块拖放基本操作
2. 物理下落与堆积
3. 简单横向连接消除
4. 预览槽位补充机制

#### 边缘情况测试
1. 方块放置到游戏区域边界
2. 方块放置到顶部（游戏结束检测）
3. 极大的连通集群消除（1000+格子）
4. 多次连锁反应（5+连锁）
5. 快速连续放置多个方块（并发处理）
6. 消除动画期间放置新方块

#### 性能测试
1. 游戏区域填满大量像素块（密度80%+）
2. 大规模消除（消除50%+的游戏区域）
3. 长时间游戏（30分钟+）
4. 低端设备性能测试

---

## 附录A：快速参考流程图

### 完整游戏循环
```
┌─ 游戏开始 ─┐
│            │
│ 生成3个预览方块
│            │
└────┬───────┘
     ↓
┌────┴────────────────────────────┐
│     等待玩家操作 (IDLE)          │
└────┬────────────────────────────┘
     ↓ 玩家选择并拖动方块
┌────┴────────────────────────────┐
│    拖动中 (DRAGGING)             │
│  - 实时碰撞检测                  │
│  - 显示放置预览                  │
└────┬────────────────────────────┘
     ↓ 玩家松手
┌────┴────────────────────────────┐
│    验证放置 (PLACING)            │
├──────┬──────────────────────────┤
│ 失败 │ 成功                     │
│  ↓   │  ↓                       │
│ 震动 │ 创建像素块                │
│ 返回 │ 立即补充预览槽位          │
└──┬───┴───┬──────────────────────┘
   │       ↓
   │  ┌────┴─────────────────────┐
   │  │ 物理模拟 (PHYSICS)        │
   │  │  - 像素块逐个下落         │
   │  │  - 重力加速              │
   │  │  - 碰撞检测              │
   │  └────┬─────────────────────┘
   │       ↓ 全部落定
   │  ┌────┴─────────────────────┐
   │  │ 消除检测                  │
   │  ├─────┬────────────────────┤
   │  │无消除│有消除              │
   │  │  ↓   │  ↓                 │
   │  │ 检查 │ 播放消除动画(1秒)  │
   │  │游戏  │  ↓                 │
   │  │结束  │ 移除像素块         │
   │  │  ?   │  ↓                 │
   │  │      │ 重力结算           │
   │  │      │  ↓                 │
   │  │      │ 返回消除检测(连锁) │
   │  └──┬───┴────────────────────┘
   │     ↓
   │  ┌──┴──────────────────────┐
   │  │ 检查游戏结束条件        │
   │  ├──────┬─────────────────┤
   │  │未结束│结束             │
   │  │  ↓   │  ↓              │
   └──┴──┘   └→ GAME OVER      │
      ↑                         │
      └─────────────────────────┘
```

---

## 附录B：消除机制详细示例

### 示例：完整游戏流程

**初始状态**（游戏开始，游戏区域为空）
```
列: 0  1  2  3  4  5  6  7  8  9 10 11
   [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]
   [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]
   ...（全空）
```

**第1步**：玩家放置红色I形方块（横向）于底部
```
列: 0  1  2  3  4  5  6  7  8  9 10 11
   [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]
   ...
   [R][R][R][R][ ][ ][ ][ ][ ][ ][ ][ ] ← 底部
```
- 无法触及右边界 → 不消除

**第2步**：玩家放置蓝色L形方块于右侧
```
列: 0  1  2  3  4  5  6  7  8  9 10 11
   [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]
   ...
   [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][B][ ]
   [R][R][R][R][ ][ ][ ][ ][ ][ ][B][B][B]
```
- 红色未连接右边界 → 不消除
- 蓝色未连接左边界 → 不消除

**第3步**：玩家放置红色T形方块，连接左右
```
列: 0  1  2  3  4  5  6  7  8  9 10 11
   [ ][ ][ ][ ][ ][ ][ ][ ][R][ ][ ][ ]
   [R][R][R][R][R][R][R][R][R][R][ ][B]
   [R][R][R][R][ ][ ][ ][ ][ ][ ][B][B][B]
```
- 红色连通且触及列0和列11 → **消除触发！**
- BFS搜索到所有红色格子（约11个逻辑格子 = 1100个像素块）
- 播放粒子动画1秒
- 移除所有红色像素块

**第4步**：消除后，蓝色像素块失去支撑，下落
```
消除后：
列: 0  1  2  3  4  5  6  7  8  9 10 11
   [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]
   [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][ ]
   [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][B][ ]
   [ ][ ][ ][ ][ ][ ][ ][ ][ ][ ][B][B][B]
```
- 蓝色像素块下落到底部
- 检查消除：蓝色未连接左边界 → 不消除
- 返回IDLE状态

**继续游戏...**

---

## 附录C：常见问题解答 (FAQ)

### Q1: 为什么要分逻辑层和物理层？
**A**: 逻辑层简化规则计算（如消除判定），物理层提供细腻的视觉效果。这种分离使代码更清晰，性能更好。

### Q2: 像素块下落时，是否会"侧向滑动"？
**A**: **会！** 这是游戏的核心机制。像素块在正下方被占用时，会尝试斜向（左下或右下）滑落。这正是形成三角形堆积的关键。详见第5章"物理系统"。

### Q3: 消除动画期间，新放置的方块会受影响吗？
**A**: 不会。新放置的方块独立进行物理模拟，不参与正在进行的消除检测。

### Q4: 如果玩家疯狂快速放置方块，会有问题吗？
**A**: 不会。每个方块组独立追踪，系统按放置顺序依次检测消除。

### Q5: 方块可以旋转吗？
**A**: 当前设计中未提及旋转功能。如需添加，可在拖动期间添加旋转按钮。

### Q6: 是否支持撤销操作？
**A**: 当前设计中未包含，但可作为未来功能扩展。

---

## 附录D：AI开发常见陷阱（必读）

### 陷阱1：在逻辑网格层面进行消除判定 ❌

**错误做法**：
```javascript
// 在12×22的逻辑网格上进行BFS
const logicalGrid = buildLogicalGrid();
const clusters = findClustersOnLogicalGrid(logicalGrid);
```

**问题**：三角形堆积导致逻辑格子中间为空，视觉连续的区域会被判定为多个断开的集群。

**正确做法**：✅
```javascript
// 在120×220的像素网格上进行BFS
const pixelClusters = findClustersOnPixelGrid();
// 直接使用像素块引用
```

**参考章节**：第8.2节

---

### 陷阱2：通过逻辑坐标删除像素块 ❌

**错误做法**：
```javascript
// BFS返回逻辑格子坐标
const cluster = {cells: [{x: 5, y: 10}, ...]};

// 尝试通过逻辑坐标删除像素块
for (const cell of cluster.cells) {
    const startX = cell.x * 10;
    const startY = cell.y * 10;
    for (let py = 0; py < 10; py++) {
        for (let px = 0; px < 10; px++) {
            pixelGrid[startY + py][startX + px] = null;
        }
    }
}
```

**问题**：像素块可能滑落到逻辑格子范围之外，导致部分像素块没被删除（悬空残留）。

**正确做法**：✅
```javascript
// BFS返回像素块引用数组
const cluster = {pixels: [pixel1, pixel2, ...]};

// 直接删除BFS找到的像素块
cluster.pixels.forEach(pixel => {
    pixelGrid[pixel.y][pixel.x] = null;
});
```

**参考章节**：第8.2.3节

---

### 陷阱3：消除后只检查一次稳定性 ❌

**错误做法**：
```javascript
// 消除像素块
eliminatePixels(cluster.pixels);

// 只检查一次
recheckStability();

// 立即进入消除检测
checkElimination();
```

**问题**：第一批像素块下落后，它们上方的像素块才失去支撑，导致悬空残留。

**正确做法**：✅
```javascript
// 消除像素块
eliminatePixels(cluster.pixels);

// 循环检查
while (true) {
    recheckStability();
    
    // 等待物理稳定
    await waitForPhysicsStable();
    
    // 再次检查
    const newUnstable = recheckStability();
    
    if (newUnstable === 0) {
        break; // 真正全部稳定
    }
}

// 进入消除检测
checkElimination();
```

**参考章节**：第8.4.2节

---

### 陷阱4：错误的稳定性判定 ❌

**错误做法**：
```javascript
function hasSupport(pixel) {
    // 任何一个方向有像素块就认为稳定
    const below = getPixel(x, y+1);
    const leftBelow = getPixel(x-1, y+1);
    const rightBelow = getPixel(x+1, y+1);
    
    return below || leftBelow || rightBelow; // 错误！
}
```

**问题**：斜角的像素块虽然一侧有支撑，但另一侧为空，应该滑落！

**正确做法**：✅
```javascript
function hasSupport(pixel) {
    // 三个方向都必须被稳定的像素块占据
    const canMoveDown = isEmpty(x, y+1);
    const canMoveLeftDown = isEmpty(x-1, y+1);
    const canMoveRightDown = isEmpty(x+1, y+1);
    
    // 只要有一个方向可以移动，就不稳定
    return !canMoveDown && !canMoveLeftDown && !canMoveRightDown;
}

function isEmpty(x, y) {
    const pixel = getPixel(x, y);
    // 空位置或不稳定的像素块都认为可以移动
    return pixel === null || !pixel.isStable;
}
```

**参考章节**：第5.2节、第8.4.2节

---

### 陷阱5：不稳定像素块也算作支撑 ❌

**错误做法**：
```javascript
function canMoveTo(x, y) {
    const pixel = getPixel(x, y);
    return pixel === null; // 只检查是否为空
}
```

**问题**：下方有不稳定的像素块时，上方像素块会被错误地认为"无法移动"，但实际下方像素块会让开。

**正确做法**：✅
```javascript
function canMoveToStableOnly(x, y) {
    const pixel = getPixel(x, y);
    
    // 空位置，可以移动
    if (pixel === null) return true;
    
    // 不稳定的像素块，认为可以移动（它会让开）
    if (!pixel.isStable) return true;
    
    // 稳定的像素块，不能移动
    return false;
}
```

**参考章节**：第8.4.2节

---

### 陷阱6：级联稳定性检查导致无限增长 ❌

**错误做法**：
```javascript
function movePixelTo(pixel, newX, newY) {
    // 移动像素块
    pixel.x = newX;
    pixel.y = newY;
    
    // 检查上方的像素块
    checkPixelAbove(pixel.x, pixel.y); // 危险！
}

function checkPixelAbove(x, y) {
    const above = getPixel(x, y-1);
    if (above && above.isStable) {
        // 标记为不稳定
        above.isStable = false;
        activePixels.add(above); // 可能重复添加
    }
}
```

**问题**：每次移动都检查上方，导致活跃像素块数量爆炸性增长（从100增长到1000+）。

**正确做法**：✅
```javascript
function movePixelTo(pixel, newX, newY) {
    // 只移动，不做级联检查
    pixel.x = newX;
    pixel.y = newY;
}

// 只在消除后统一检查所有像素块
function afterElimination() {
    recheckStability(); // 一次性检查所有
}
```

**参考章节**：第5.4.2节

---

## 附录E：实现检查清单

开发AI应该验证以下关键点：

### 消除系统检查清单

- [ ] BFS在像素网格（120×220）层面进行
- [ ] BFS返回像素块引用数组，不是逻辑坐标
- [ ] 边界触及判定使用像素坐标（X=0 和 X=119）
- [ ] 消除时直接删除BFS返回的像素块
- [ ] 验证：消除前后像素块总数应该减少

### 稳定性检查清单

- [ ] 消除后调用`recheckStability()`
- [ ] 物理稳定后自动再次检查稳定性
- [ ] 发现新的不稳定像素块时继续物理模拟
- [ ] 稳定性判定只计算稳定的像素块作为支撑
- [ ] 验证：悬空的像素块会自动下落，无残留

### 性能检查清单

- [ ] 活跃像素块数量不会失控增长
- [ ] 不在移动时进行级联检查
- [ ] 使用Set追踪活跃像素块
- [ ] 从下往上更新避免双重移动
- [ ] 验证：大规模消除后帧率稳定

---

## 附录F：常见错误日志识别

通过控制台日志快速识别问题：

### 问题1：消除后像素块总数不变

**日志特征**：
```
消除前像素块总数: 2400
开始删除 1600 个像素块
实际删除了 1600 个像素块
消除后像素块总数: 2400  ← 应该是800！
```

**原因**：通过逻辑坐标删除，漏掉了滑落到斜角的像素块

**解决方案**：使用像素层BFS，直接删除BFS返回的像素块引用

---

### 问题2：活跃像素块数量爆炸

**日志特征**：
```
物理更新: 150个移动, 剩余活跃: 227  ← 从150增加到227！
物理更新: 223个移动, 剩余活跃: 297  ← 继续增长！
物理更新: 284个移动, 剩余活跃: 351
```

**原因**：在`movePixelTo`中进行级联检查，重复添加像素块

**解决方案**：移除级联检查，只在消除后统一检查

---

### 问题3：标记为不稳定但不移动

**日志特征**：
```
重新检查结果: 新增不稳定=79
物理更新: 0个移动, 8个稳定, 剩余活跃: 0  ← 79个不稳定但0个移动！
```

**原因**：下方有不稳定的像素块，被错误地当作障碍物

**解决方案**：稳定性判定只计算稳定的像素块作为支撑

---

### 问题4：视觉连续的区域未触发消除

**日志特征**：
```
找到 2 个连通集群
集群0: 颜色=16776960, 格子数=4, 触及左=true, 触及右=false
集群1: 颜色=16776960, 格子数=9, 触及左=false, 触及右=true
可消除集群数: 0  ← 明明是同色且视觉连续！
```

**原因**：在逻辑网格层面BFS，三角形堆积导致断连

**解决方案**：改为在像素网格层面BFS

---

### 问题5：悬空残留的像素块

**症状**：消除后有少量像素块悬浮在空中

**调试方法**：
```javascript
// 按G键查看状态
总像素块: 1600
稳定: 1600  ← 全部标记为稳定
不稳定: 0
活跃集合: 0

// 按R键手动检查
重新检查结果: 新增不稳定=79  ← 发现悬空像素块！
```

**原因**：稳定性检查不完整或逻辑错误

**解决方案**：
1. 实现循环检查机制
2. 修正稳定性判定逻辑（只计算稳定像素块作为支撑）

---

## 附录G：验收测试用例

### 测试1：基础消除
1. 放置方块形成横跨左右边界的连接
2. 验证：触发消除，像素块被删除
3. 验证：总数减少 = 删除数量
4. 验证：无悬空残留

### 测试2：三角形堆积消除
1. 放置多个方形方块，形成三角形堆积
2. 放置方块连接左右两侧的斜坡
3. 验证：虽然逻辑层断开，但像素层连续，应触发消除
4. 验证：BFS正确识别像素层连通性

### 测试3：消除后稳定性
1. 消除底层方块
2. 验证：上方所有像素块自动下落
3. 验证：无悬空残留
4. 验证：最终形成紧密堆积

### 测试4：性能测试
1. 连续放置大量方块
2. 触发大规模消除（1000+像素块）
3. 验证：活跃像素块数量不会失控增长
4. 验证：帧率保持稳定（30+ FPS）

---

## 文档变更记录

| 版本 | 日期 | 变更内容 |
|------|------|----------|
| 1.0 | 2025-10-09 | 初始版本 |
| 2.0 | 2025-10-09 | 添加像素块概念 |
| 3.0 | 2025-10-09 | 定义逻辑/物理双层系统 |
| 4.0 | 2025-10-09 | 完整重写，移除墙体概念，明确消除机制 |
| 4.1 | 2025-10-09 | 完整物理系统设计 |
| **5.0** | **2025-10-10** | **重大修正：基于实际开发经验** |
|  |  | - 修正消除判定：必须在像素网格层面BFS |
|  |  | - 修正消除执行：直接删除像素块引用 |
|  |  | - 添加稳定性循环检查机制 |
|  |  | - 添加AI开发常见陷阱（附录D） |
|  |  | - 添加错误日志识别（附录F） |
|  |  | - 添加验收测试用例（附录G） |

---

## 附录H：物理系统速查表

### 核心算法总结

**三方向下落规则（伪代码）**：
```
for each 像素块 from 下往上:
    if 正下方为空:
        移动到正下方
    else if 左下方为空 or 右下方为空:
        随机选择一个空的斜下方移动
    else:
        标记为稳定
```

### 关键参数

| 参数 | 值 | 说明 |
|------|---|------|
| 更新频率 | 60 FPS | 每帧更新一次物理 |
| 移动速度 | 1像素块/帧 | 每帧最多移动1格 |
| 更新顺序 | 从下往上 | Y=219→0 |
| 随机方向 | 50/50 | 左下和右下等概率 |
| 堆积角度 | ~45° | 自然形成 |

### 性能基准

| 场景 | 活跃像素块数 | 预期帧率 |
|------|-------------|---------|
| 空闲 | 0 | 60 FPS |
| 放置单个方块 | ~400 | 60 FPS |
| 大规模消除后 | 1000-3000 | 60 FPS |
| 极限密度(80%) | 5000-8000 | 30-60 FPS |

### 调试检查清单

开发时需要验证的关键行为：

- [ ] 方形方块能形成三角形堆积
- [ ] 像素块不会在一帧内移动超过1格
- [ ] 边界处理正确（不越界）
- [ ] 从下往上更新（无穿透Bug）
- [ ] 消除后重力正确触发
- [ ] 多方块并发放置互不干扰
- [ ] 稳定后不再参与物理更新

### 常见Bug及解决方案

| Bug描述 | 原因 | 解决方案 |
|---------|------|---------|
| 像素块穿透地面 | 边界检查缺失 | 添加 y >= 220 检查 |
| 堆积不对称 | 左右方向固定优先级 | 随机选择左/右 |
| 性能卡顿 | 全网格扫描 | 使用活跃像素块追踪 |
| 双重移动 | 更新顺序错误 | 从下往上更新+标记 |
| 山形不成形 | 缺少斜向滑落 | 实现三方向下落 |

### 视觉效果建议

为了增强物理感：

1. **下落动画**：线性插值，使移动平滑
   ```javascript
   pixel.renderY = lerp(pixel.prevY, pixel.y, deltaTime / frameTime);
   ```

2. **落地粒子**：像素块稳定时播放微小的尘埃效果

3. **堆积音效**：根据稳定的像素块数量调整音量和音调

4. **震动反馈**（移动端）：大块方块落地时轻微震动

---

## 附录E：实现路线图

### Phase 1: 基础框架（1-2周）
- [ ] 网格系统（逻辑层 + 物理层）
- [ ] 方块生成（7-Bag系统）
- [ ] 基础渲染（像素块绘制）
- [ ] 简单的拖放输入

### Phase 2: 物理系统（2-3周）
- [ ] 三方向下落算法
- [ ] 更新顺序优化
- [ ] 稳定性检测
- [ ] 山形堆积验证
- [ ] 性能优化（活跃追踪）

### Phase 3: 消除机制（2周）
- [ ] BFS连通搜索
- [ ] 边界触及判定
- [ ] 粒子化消除动画
- [ ] 连锁反应系统

### Phase 4: 游戏流程（1周）
- [ ] 预览槽位系统
- [ ] 计分系统
- [ ] 游戏结束判定
- [ ] 状态机实现

### Phase 5: UI与体验（1-2周）
- [ ] 完整UI界面
- [ ] 音效和音乐
- [ ] 动画和特效
- [ ] 教程和引导

### Phase 6: 测试与优化（1-2周）
- [ ] 边缘情况测试
- [ ] 性能优化
- [ ] 平衡性调整
- [ ] Bug修复

**总计**: 约8-12周开发周期（MVP可在4-6周完成）

---

**文档版本**: 5.0  
**最后更新**: 2025年10月10日  
**更新内容**: 基于实际开发经验的重大修正，添加AI开发常见陷阱和解决方案

---

**文档结束**

此文档为AI开发提供了完整的游戏设计规范。

**🔴 特别提醒**：本文档已根据实际开发经验进行重大修正，特别是：
- 第8.2节：消除判定必须在像素网格层面BFS
- 第8.4节：稳定性检查必须循环进行
- 附录D：总结了所有常见陷阱和解决方案

这些修正是确保游戏正确运行的**关键**。如果按照旧版本（逻辑网格BFS）实现，会导致：
1. 视觉连续的区域无法消除
2. 消除后有悬空残留
3. 活跃像素块数量失控

**强烈建议AI在开发前仔细阅读"核心技术要点"和"附录D：AI开发常见陷阱"！**

