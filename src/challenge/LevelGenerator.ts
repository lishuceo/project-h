/**
 * 每日挑战关卡生成器
 * 基于种子生成确定性的初始布局
 */

import { SeededRandom } from '../utils/seedRandom';
import { DailyChallengeData, PixelBlockData, StarThresholds } from '../types/challenge';
import { Color } from '../types';

export class LevelGenerator {
  /**
   * 生成每日挑战关卡
   * @param seed 随机种子（基于日期）
   * @param date 日期字符串
   * @returns 完整的挑战数据
   */
  public generate(seed: number, date: string): DailyChallengeData {
    console.log(`🎲 开始生成关卡 [日期: ${date}, 种子: ${seed}]`);
    
    const random = new SeededRandom(seed);
    
    // 根据日期决定难度（周末更难）
    const dayOfWeek = new Date(date).getUTCDay();
    const difficulty = this.getDifficulty(dayOfWeek);
    console.log(`📊 难度: ${difficulty} (${dayOfWeek === 0 || dayOfWeek === 6 ? '周末' : '工作日'})`);
    
    // 生成初始布局
    const initialLayout = this.generateLayout(random, difficulty);
    console.log(`📦 生成了 ${initialLayout.length} 个像素块`);
    
    // 计算合理的步数限制
    const maxSteps = this.calculateMaxSteps(initialLayout, difficulty);
    console.log(`🎯 步数限制: ${maxSteps} 步`);
    
    // 计算校验和
    const checksum = this.calculateChecksum(initialLayout);
    console.log(`🔐 校验和: ${checksum}`);
    
    return {
      date,
      seed,
      difficulty,
      initialLayout,
      maxSteps,
      timeLimit: undefined, // 暂不限制时间
      checksum
    };
  }
  
  /**
   * 生成初始布局
   * 策略：创建复杂的多层结构，需要策略性消除
   */
  private generateLayout(
    random: SeededRandom, 
    difficulty: 1 | 2 | 3
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];
    
    // 根据难度决定颜色数量
    const colorCount = difficulty === 1 ? 2 : difficulty === 2 ? 3 : 4;
    const colors = this.selectColors(random, colorCount);
    
    console.log(`🏗️ 生成 ${colorCount} 层结构`);
    
    // 策略：生成多层结构，需要先消除外层才能接触内层
    for (let i = 0; i < colors.length; i++) {
      const color = colors[i];
      const layer = i; // 层级：0=底层，1=第二层，2=第三层...
      
      console.log(`  生成第 ${i} 层，颜色索引 ${i}`);
      
      if (i === 0) {
        // 最底层（目标层）：左右两侧小块，需要连接
        const leftSmall = this.generateSmallPile(random, color, 'left', layer);
        const rightSmall = this.generateSmallPile(random, color, 'right', layer);
        pixels.push(...leftSmall, ...rightSmall);
        console.log(`  目标层（红色）: ${leftSmall.length + rightSmall.length}像素块`);
      } else {
        // 上层（障碍层）：完整的横跨布局
        const barrier = this.generateBarrier(random, color, layer, difficulty);
        pixels.push(...barrier);
        console.log(`  障碍层 ${i}: ${barrier.length}像素块`);
      }
    }
    
    console.log(`✅ 总共生成 ${pixels.length} 个像素块`);
    return pixels;
  }
  
  /**
   * 生成一个色块堆（左侧或右侧）- 已弃用，保留用于参考
   */
  private generatePile(
    random: SeededRandom,
    color: Color,
    side: 'left' | 'right',
    difficulty: number,
    layerIndex: number
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];
    
    // 确定基础位置
    const baseX = side === 'left' ? 0 : 100;
    
    // 根据层级调整Y坐标（避免重叠）
    const baseY = 140 - layerIndex * 40;
    
    // 根据难度决定堆积大小
    const widthRange = difficulty === 1 ? [30, 40] : difficulty === 2 ? [25, 35] : [20, 30];
    const heightRange = difficulty === 1 ? [30, 40] : difficulty === 2 ? [25, 35] : [20, 30];
    
    const width = random.nextInt(widthRange[0], widthRange[1]);
    const height = random.nextInt(heightRange[0], heightRange[1]);
    
    // 生成矩形堆积（简化版，实际可以更复杂）
    for (let dy = 0; dy < height; dy++) {
      for (let dx = 0; dx < width; dx++) {
        const x = side === 'left' ? (baseX + dx) : (baseX + 20 - width + dx);
        const y = baseY - dy;
        
        // 边界检查（网格高度150）
        if (x >= 0 && x < 120 && y >= 0 && y < 150) {
          // 生成完整的矩形，不要镂空
          pixels.push({ x, y, color });
        }
      }
    }
    
    return pixels;
  }
  
  /**
   * 生成小的堆积（用于目标层，需要玩家连接）
   */
  private generateSmallPile(
    random: SeededRandom,
    color: Color,
    side: 'left' | 'right',
    layer: number
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];
    const baseY = 145 - layer * 15; // 底部（网格高度150，从145开始）
    
    // 小块：宽10-20，高20-30
    const width = random.nextInt(10, 20);
    const height = random.nextInt(20, 30);
    
    const baseX = side === 'left' ? 0 : (120 - width);
    
    for (let dy = 0; dy < height; dy++) {
      for (let dx = 0; dx < width; dx++) {
        const x = baseX + dx;
        const y = baseY - dy;
        
        if (x >= 0 && x < 120 && y >= 0 && y < 150) {
          pixels.push({ x, y, color });
        }
      }
    }
    
    console.log(`  ${side === 'left' ? '左' : '右'}侧小块: ${pixels.length}像素块 (baseY=${baseY})`);
    return pixels;
  }
  
  /**
   * 生成障碍层（横跨整个宽度）
   */
  private generateBarrier(
    random: SeededRandom,
    color: Color,
    layer: number,
    difficulty: number
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];
    
    // 障碍层的Y位置（从底层往上叠加）
    const baseY = 145 - layer * 20; // 每层间隔20像素
    const height = random.nextInt(8, 15); // 障碍层高度
    
    // 横跨的宽度（留一些间隙）
    const gapLeft = random.nextInt(15, 25);
    const gapRight = random.nextInt(15, 25);
    const startX = gapLeft;
    const endX = 120 - gapRight;
    
    for (let dy = 0; dy < height; dy++) {
      for (let x = startX; x < endX; x++) {
        const y = baseY - dy;
        
        if (y >= 0 && y < 150) {
          // 添加一些随机镂空，制造不规则形状
          if (random.boolean(0.9)) {
            pixels.push({ x, y, color });
          }
        }
      }
    }
    
    console.log(`  第${layer}层障碍: ${pixels.length}像素块 (baseY=${baseY})`);
    return pixels;
  }
  
  /**
   * 生成桥梁（中间的零散像素块）- 已弃用
   */
  private generateBridge(
    random: SeededRandom, 
    color: Color,
    difficulty: number
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];
    const count = random.nextInt(30, 60);
    
    for (let i = 0; i < count; i++) {
      const x = random.nextInt(45, 75);  // 中间区域
      const y = random.nextInt(100, 145); // 适配网格高度150
      pixels.push({ x, y, color });
    }
    
    return pixels;
  }
  
  /**
   * 根据星期几决定难度
   */
  private getDifficulty(dayOfWeek: number): 1 | 2 | 3 {
    // 周一周二：简单
    if (dayOfWeek === 1 || dayOfWeek === 2) return 1;
    // 周三周四周五：中等
    if (dayOfWeek >= 3 && dayOfWeek <= 5) return 2;
    // 周六周日：困难
    return 3;
  }
  
  /**
   * 计算合理的步数限制
   */
  private calculateMaxSteps(layout: PixelBlockData[], difficulty: number): number {
    // 估算：每100个像素块（约1个逻辑格子）需要0.2步来清除
    const estimatedSteps = Math.ceil(layout.length / 100 * 0.2);
    
    // 根据难度调整宽松度
    const multiplier = difficulty === 1 ? 3.0 : difficulty === 2 ? 2.5 : 2.0;
    const maxSteps = Math.ceil(estimatedSteps * multiplier);
    
    // 至少10步
    return Math.max(10, maxSteps);
  }
  
  /**
   * 选择颜色（只使用明亮的颜色）
   */
  private selectColors(random: SeededRandom, count: number): Color[] {
    // 只使用明亮易辨识的颜色
    const brightColors = [
      Color.RED,
      Color.BLUE,
      Color.GREEN,
      Color.YELLOW
    ];
    
    // 打乱并选择前n个
    const shuffled = random.shuffle(brightColors);
    const selected = shuffled.slice(0, count);
    
    // 调试日志
    console.log(`🎨 选择的颜色:`, selected.map(c => {
      if (c === Color.RED) return '红色';
      if (c === Color.BLUE) return '蓝色';
      if (c === Color.GREEN) return '绿色';
      if (c === Color.YELLOW) return '黄色';
      return '未知';
    }));
    
    return selected;
  }
  
  /**
   * 计算校验和（用于验证关卡一致性）
   */
  private calculateChecksum(layout: PixelBlockData[]): string {
    let hash = 0;
    
    // 对每个像素块计算哈希
    for (const pixel of layout) {
      hash = ((hash << 5) - hash) + pixel.x;
      hash = ((hash << 5) - hash) + pixel.y;
      hash = ((hash << 5) - hash) + pixel.color;
      hash = hash & hash; // 转换为32位整数
    }
    
    // 转换为16进制字符串
    return Math.abs(hash).toString(16).padStart(8, '0');
  }
  
  /**
   * 获取星级阈值
   */
  public getStarThresholds(difficulty: 1 | 2 | 3): StarThresholds {
    if (difficulty === 1) {
      return {
        time3star: 60,
        time2star: 120,
        steps3star: 10,
        steps2star: 15
      };
    } else if (difficulty === 2) {
      return {
        time3star: 90,
        time2star: 150,
        steps3star: 15,
        steps2star: 25
      };
    } else {
      return {
        time3star: 120,
        time2star: 200,
        steps3star: 20,
        steps2star: 35
      };
    }
  }
}

