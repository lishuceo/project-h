/**
 * 每日挑战关卡生成器
 * 基于种子生成确定性的初始布局
 */

import { SeededRandom } from '../utils/seedRandom';
import { DailyChallengeData, PixelBlockData, StarThresholds } from '../types/challenge';
import { PIXEL_GRID_WIDTH, PIXEL_GRID_HEIGHT } from '../config/constants';
import { Color } from '../types';

export class LevelGenerator {
  /**
   * 生成每日挑战关卡
   * @param seed 随机种子（基于日期和挑战ID）
   * @param date 日期字符串
   * @param challengeId 挑战ID（1=简单，2=中等，3=困难）
   * @returns 完整的挑战数据
   */
  public generate(seed: number, date: string, challengeId: 1 | 2 | 3 = 1): DailyChallengeData {
    console.log(`🎲 开始生成关卡 [日期: ${date}, 种子: ${seed}, 挑战ID: ${challengeId}]`);

    const random = new SeededRandom(seed);

    // 使用传入的challengeId作为难度
    const difficulty = challengeId;
    console.log(`📊 难度: ${difficulty} (挑战${challengeId})`);

    // 🎯 先选择关卡使用的颜色
    const colorCount = difficulty === 1 ? 2 : difficulty === 2 ? 3 : 4;
    const availableColors = this.selectColors(random, colorCount);

    // 生成初始布局（使用选定的颜色）
    const initialLayout = this.generateLayoutWithColors(random, difficulty, availableColors);
    console.log(`📦 生成了 ${initialLayout.length} 个像素块`);

    // 计算合理的步数限制
    const maxSteps = this.calculateMaxSteps(initialLayout, difficulty);
    console.log(`🎯 步数限制: ${maxSteps} 步`);

    // 计算校验和
    const checksum = this.calculateChecksum(initialLayout);
    console.log(`🔐 校验和: ${checksum}`);

    return {
      date,
      challengeId,
      seed,
      difficulty,
      initialLayout,
      maxSteps,
      timeLimit: undefined, // 暂不限制时间
      checksum,
      availableColors  // 🎯 返回可用颜色，确保玩家能完成关卡
    };
  }
  
  /**
   * 生成初始布局（使用指定的颜色）
   * 新策略：
   * 1. 明确的目标层（第一种颜色）- 左右两侧，需要连接才能消除
   * 2. 障碍层（其他颜色）- 阻挡目标层，需要先清除
   * 3. 镂空仅用于困难模式，且有限度使用
   */
  private generateLayoutWithColors(
    random: SeededRandom,
    difficulty: 1 | 2 | 3,
    colors: Color[]
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];

    // 🎯 随机选择整体布局风格（5种）
    const layoutStyles: Array<'classic' | 'pyramid' | 'tunnel' | 'island' | 'maze'> =
      ['classic', 'pyramid', 'tunnel', 'island', 'maze'];
    const style = random.choice(layoutStyles);
    console.log(`🎨 布局风格: ${style}`);

    switch (style) {
      case 'classic':
        // 经典：左右小块 + 中间障碍
        pixels.push(...this.generateClassicLayout(random, difficulty, colors));
        break;

      case 'pyramid':
        // 金字塔：底部左右 + 上层金字塔形障碍
        pixels.push(...this.generatePyramidLayout(random, difficulty, colors));
        break;

      case 'tunnel':
        // 隧道：左右大块包围中间通道
        pixels.push(...this.generateTunnelLayout(random, difficulty, colors));
        break;

      case 'island':
        // 岛屿：多个分离的岛，需要造桥连接
        pixels.push(...this.generateIslandLayout(random, difficulty, colors));
        break;

      case 'maze':
        // 迷宫：曲折的通道
        pixels.push(...this.generateMazeLayout(random, difficulty, colors));
        break;
    }

    console.log(`✅ 总共生成 ${pixels.length} 个像素块`);
    return pixels;
  }

  // ========== 5种布局风格 ==========

  /**
   * 风格1: 经典布局 - 左右小块 + 中间障碍
   */
  private generateClassicLayout(
    random: SeededRandom,
    difficulty: 1 | 2 | 3,
    colors: Color[]
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];
    const targetColor = colors[0];

    // 目标层：左右两侧
    const leftTarget = this.generateSolidPile(random, targetColor, 'left', 0);
    const rightTarget = this.generateSolidPile(random, targetColor, 'right', 0);
    pixels.push(...leftTarget, ...rightTarget);

    // 障碍层
    if (colors.length >= 2) {
      const modes: Array<'horizontal' | 'vertical' | 'cross' | 'scattered'> =
        ['horizontal', 'vertical', 'cross', 'scattered'];
      const barrierMode = random.choice(modes);
      const obstacle = this.generateObstacle(random, colors.slice(1), barrierMode, difficulty);
      pixels.push(...obstacle);
    }

    return pixels;
  }

  /**
   * 风格2: 金字塔布局
   */
  private generatePyramidLayout(
    random: SeededRandom,
    difficulty: 1 | 2 | 3,
    colors: Color[]
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];
    const targetColor = colors[0];
    const hollowRate = difficulty === 3 ? 0.15 : 0;

    // 目标：左右底部
    const leftSize = random.nextInt(18, 28);
    const rightSize = random.nextInt(18, 28);

    for (let i = 0; i < leftSize; i++) {
      for (let j = 0; j < leftSize; j++) {
        const y = PIXEL_GRID_HEIGHT - 1 - i;
        const x = j;
        if (y >= 0 && x < PIXEL_GRID_WIDTH) {
          pixels.push({ x, y, color: targetColor });
        }
      }
    }

    for (let i = 0; i < rightSize; i++) {
      for (let j = 0; j < rightSize; j++) {
        const y = PIXEL_GRID_HEIGHT - 1 - i;
        const x = PIXEL_GRID_WIDTH - 1 - j;
        if (y >= 0 && x >= 0) {
          pixels.push({ x, y, color: targetColor });
        }
      }
    }

    // 金字塔形障碍（中间）
    if (colors.length >= 2) {
      const pyramidColor = colors[1];
      const pyramidHeight = random.nextInt(40, 60);
      const centerX = Math.floor(PIXEL_GRID_WIDTH / 2);

      for (let h = 0; h < pyramidHeight; h++) {
        const width = Math.floor(pyramidHeight - h);
        for (let w = 0; w < width; w++) {
          const x = centerX - Math.floor(width / 2) + w;
          const y = PIXEL_GRID_HEIGHT - 10 - h;

          if (x >= 0 && x < PIXEL_GRID_WIDTH && y >= 0 && y < PIXEL_GRID_HEIGHT) {
            if (random.boolean(1 - hollowRate)) {
              pixels.push({ x, y, color: pyramidColor });
            }
          }
        }
      }

      // 额外障碍层
      if (colors.length >= 3) {
        const extraColor = colors[2];
        const extraBlocks = this.generateObstacle(random, [extraColor], 'scattered', difficulty);
        pixels.push(...extraBlocks);
      }
    }

    return pixels;
  }

  /**
   * 风格3: 隧道布局
   */
  private generateTunnelLayout(
    random: SeededRandom,
    difficulty: 1 | 2 | 3,
    colors: Color[]
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];
    const targetColor = colors[0];
    const hollowRate = difficulty === 3 ? 0.15 : 0;

    // 左侧墙壁（目标色）
    const leftWidth = random.nextInt(20, 30);
    const leftHeight = random.nextInt(50, 70);
    for (let y = PIXEL_GRID_HEIGHT - 1; y > PIXEL_GRID_HEIGHT - leftHeight && y >= 0; y--) {
      for (let x = 0; x < leftWidth; x++) {
        pixels.push({ x, y, color: targetColor });
      }
    }

    // 右侧墙壁（目标色）
    const rightWidth = random.nextInt(20, 30);
    const rightHeight = random.nextInt(50, 70);
    for (let y = PIXEL_GRID_HEIGHT - 1; y > PIXEL_GRID_HEIGHT - rightHeight && y >= 0; y--) {
      for (let x = PIXEL_GRID_WIDTH - rightWidth; x < PIXEL_GRID_WIDTH; x++) {
        pixels.push({ x, y, color: targetColor });
      }
    }

    // 中间通道的障碍
    if (colors.length >= 2) {
      for (let i = 1; i < colors.length; i++) {
        const color = colors[i];
        const blockCount = random.nextInt(2, 4);

        for (let b = 0; b < blockCount; b++) {
          const width = random.nextInt(15, 25);
          const height = random.nextInt(20, 30);
          const x = random.nextInt(leftWidth + 5, PIXEL_GRID_WIDTH - rightWidth - width - 5);
          const y = random.nextInt(PIXEL_GRID_HEIGHT - 60, PIXEL_GRID_HEIGHT - 10);

          for (let dy = 0; dy < height; dy++) {
            for (let dx = 0; dx < width; dx++) {
              const px = x + dx;
              const py = y - dy;

              if (px >= 0 && px < PIXEL_GRID_WIDTH && py >= 0 && py < PIXEL_GRID_HEIGHT) {
                if (random.boolean(1 - hollowRate)) {
                  pixels.push({ x: px, y: py, color });
                }
              }
            }
          }
        }
      }
    }

    return pixels;
  }

  /**
   * 风格4: 岛屿布局
   */
  private generateIslandLayout(
    random: SeededRandom,
    difficulty: 1 | 2 | 3,
    colors: Color[]
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];
    const targetColor = colors[0];
    const hollowRate = difficulty === 3 ? 0.15 : 0;

    // 创建3-5个岛屿，第一个和最后一个是目标色
    const islandCount = random.nextInt(4, 6);
    const islandWidth = Math.floor(PIXEL_GRID_WIDTH / islandCount);

    for (let i = 0; i < islandCount; i++) {
      // 第一个和最后一个岛用目标色（需要连接）
      const color = (i === 0 || i === islandCount - 1) ? targetColor : colors[(i % (colors.length - 1)) + 1];

      const x = i * islandWidth + random.nextInt(5, 10);
      const width = random.nextInt(15, 22);
      const height = random.nextInt(30, 45);
      const baseY = PIXEL_GRID_HEIGHT - random.nextInt(5, 15);

      for (let dy = 0; dy < height; dy++) {
        for (let dx = 0; dx < width; dx++) {
          const px = x + dx;
          const py = baseY - dy;

          if (px >= 0 && px < PIXEL_GRID_WIDTH && py >= 0 && py < PIXEL_GRID_HEIGHT) {
            if (random.boolean(1 - hollowRate)) {
              pixels.push({ x: px, y: py, color });
            }
          }
        }
      }
    }

    return pixels;
  }

  /**
   * 风格5: 迷宫布局
   */
  private generateMazeLayout(
    random: SeededRandom,
    difficulty: 1 | 2 | 3,
    colors: Color[]
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];
    const targetColor = colors[0];
    const hollowRate = difficulty === 3 ? 0.15 : 0;

    // 左下角和右下角目标
    const cornerSize = random.nextInt(20, 30);
    for (let y = PIXEL_GRID_HEIGHT - 1; y > PIXEL_GRID_HEIGHT - cornerSize && y >= 0; y--) {
      for (let x = 0; x < cornerSize; x++) {
        pixels.push({ x, y, color: targetColor });
      }
      for (let x = PIXEL_GRID_WIDTH - cornerSize; x < PIXEL_GRID_WIDTH; x++) {
        pixels.push({ x, y, color: targetColor });
      }
    }

    // 迷宫墙壁（障碍色）
    if (colors.length >= 2) {
      const wallCount = random.nextInt(3, 5);

      for (let w = 0; w < wallCount; w++) {
        const color = colors[(w % (colors.length - 1)) + 1];
        const isVertical = random.boolean(0.5);

        if (isVertical) {
          // 纵向墙
          const x = random.nextInt(25, PIXEL_GRID_WIDTH - 25);
          const width = random.nextInt(8, 15);
          const height = random.nextInt(30, 50);
          const baseY = PIXEL_GRID_HEIGHT - random.nextInt(10, 20);

          for (let dy = 0; dy < height; dy++) {
            for (let dx = 0; dx < width; dx++) {
              const px = x + dx;
              const py = baseY - dy;

              if (px >= 0 && px < PIXEL_GRID_WIDTH && py >= 0 && py < PIXEL_GRID_HEIGHT) {
                if (random.boolean(1 - hollowRate)) {
                  pixels.push({ x: px, y: py, color });
                }
              }
            }
          }
        } else {
          // 横向墙
          const y = random.nextInt(PIXEL_GRID_HEIGHT - 60, PIXEL_GRID_HEIGHT - 20);
          const x1 = random.nextInt(20, 40);
          const x2 = random.nextInt(PIXEL_GRID_WIDTH - 40, PIXEL_GRID_WIDTH - 20);
          const height = random.nextInt(10, 15);

          for (let dy = 0; dy < height; dy++) {
            for (let x = x1; x < x2; x++) {
              const py = y - dy;
              if (py >= 0 && py < PIXEL_GRID_HEIGHT) {
                if (random.boolean(1 - hollowRate)) {
                  pixels.push({ x, y: py, color });
                }
              }
            }
          }
        }
      }
    }

    return pixels;
  }
  
  /**
   * 生成实心堆积（无镂空，确保不产生细碎残留）
   */
  private generateSolidPile(
    random: SeededRandom,
    color: Color,
    side: 'left' | 'right',
    layer: number
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];
    const baseY = PIXEL_GRID_HEIGHT - 8 - layer * 18; // 从底部开始

    // 根据难度调整大小
    const width = random.nextInt(15, 25);  // 扩大范围
    const height = random.nextInt(25, 40); // 扩大范围

    const baseX = side === 'left' ? 0 : (PIXEL_GRID_WIDTH - width);

    // 100%实心，不镂空
    for (let dy = 0; dy < height; dy++) {
      for (let dx = 0; dx < width; dx++) {
        const x = baseX + dx;
        const y = baseY - dy;

        if (x >= 0 && x < PIXEL_GRID_WIDTH && y >= 0 && y < PIXEL_GRID_HEIGHT) {
          pixels.push({ x, y, color });
        }
      }
    }

    return pixels;
  }

  /**
   * 生成障碍物（根据模式和难度）
   */
  private generateObstacle(
    random: SeededRandom,
    obstacleColors: Color[],
    mode: 'horizontal' | 'vertical' | 'cross' | 'scattered',
    difficulty: 1 | 2 | 3
  ): PixelBlockData[] {
    const pixels: PixelBlockData[] = [];

    // 只有困难模式才使用镂空，且镂空率有限
    const hollowRate = difficulty === 3 ? 0.15 : 0; // 困难模式15%镂空，其他完全实心

    switch (mode) {
      case 'horizontal':
        // 横向障碍条（1-2层）
        obstacleColors.forEach((color, i) => {
          const y = PIXEL_GRID_HEIGHT - 25 - i * 20;
          const x1 = random.nextInt(15, 25);
          const x2 = PIXEL_GRID_WIDTH - random.nextInt(15, 25);
          const height = random.nextInt(10, 18);

          for (let dy = 0; dy < height; dy++) {
            for (let x = x1; x < x2; x++) {
              const py = y - dy;
              if (py >= 0 && py < PIXEL_GRID_HEIGHT) {
                if (random.boolean(1 - hollowRate)) { // 大部分实心
                  pixels.push({ x, y: py, color });
                }
              }
            }
          }
        });
        break;

      case 'vertical':
        // 纵向障碍柱（1-2根）
        obstacleColors.forEach((color, i) => {
          const centerX = PIXEL_GRID_WIDTH / 2;
          const offset = (i - obstacleColors.length / 2) * 25;
          const x = Math.floor(centerX + offset);
          const width = random.nextInt(12, 20);
          const height = random.nextInt(35, 55);
          const baseY = PIXEL_GRID_HEIGHT - 10;

          for (let dy = 0; dy < height; dy++) {
            for (let dx = 0; dx < width; dx++) {
              const px = x + dx - Math.floor(width / 2);
              const py = baseY - dy;

              if (px >= 0 && px < PIXEL_GRID_WIDTH && py >= 0 && py < PIXEL_GRID_HEIGHT) {
                if (random.boolean(1 - hollowRate)) {
                  pixels.push({ x: px, y: py, color });
                }
              }
            }
          }
        });
        break;

      case 'cross':
        // 十字形障碍（适合多颜色）
        obstacleColors.forEach((color, i) => {
          if (i === 0) {
            // 横向条
            const y = PIXEL_GRID_HEIGHT / 2;
            const height = 15;
            for (let dy = 0; dy < height; dy++) {
              for (let x = 20; x < PIXEL_GRID_WIDTH - 20; x++) {
                const py = Math.floor(y - dy);
                if (py >= 0 && py < PIXEL_GRID_HEIGHT) {
                  if (random.boolean(1 - hollowRate)) {
                    pixels.push({ x, y: py, color });
                  }
                }
              }
            }
          } else {
            // 纵向条
            const centerX = PIXEL_GRID_WIDTH / 2;
            const width = 15;
            const height = 50;
            const baseY = PIXEL_GRID_HEIGHT - 15;

            for (let dy = 0; dy < height; dy++) {
              for (let dx = 0; dx < width; dx++) {
                const px = Math.floor(centerX + dx - width / 2);
                const py = baseY - dy;

                if (px >= 0 && px < PIXEL_GRID_WIDTH && py >= 0 && py < PIXEL_GRID_HEIGHT) {
                  if (random.boolean(1 - hollowRate)) {
                    pixels.push({ x: px, y: py, color });
                  }
                }
              }
            }
          }
        });
        break;

      case 'scattered':
        // 分散的大块（2-4个整块，完全实心）
        const blockCount = obstacleColors.length + 1;
        for (let i = 0; i < blockCount; i++) {
          const color = obstacleColors[i % obstacleColors.length];
          const width = random.nextInt(18, 28);
          const height = random.nextInt(20, 35);
          const x = random.nextInt(15, PIXEL_GRID_WIDTH - width - 15);
          const y = random.nextInt(PIXEL_GRID_HEIGHT - 55, PIXEL_GRID_HEIGHT - 15);

          for (let dy = 0; dy < height; dy++) {
            for (let dx = 0; dx < width; dx++) {
              const px = x + dx;
              const py = y - dy;

              if (px >= 0 && px < PIXEL_GRID_WIDTH && py >= 0 && py < PIXEL_GRID_HEIGHT) {
                if (random.boolean(1 - hollowRate)) {
                  pixels.push({ x: px, y: py, color });
                }
              }
            }
          }
        }
        break;
    }

    return pixels;
  }

  
  
  /**
   * 计算合理的步数限制
   */
  private calculateMaxSteps(layout: PixelBlockData[], difficulty: number): number {
    // 估算：每100个像素块（约1个逻辑格子）需要0.2步来清除
    const estimatedSteps = Math.ceil(layout.length / 100 * 0.2);

    // 根据难度调整宽松度（翻倍后更宽松，避免卡死用户）
    const multiplier = difficulty === 1 ? 6.0 : difficulty === 2 ? 5.0 : 4.0;
    const maxSteps = Math.ceil(estimatedSteps * multiplier);

    // 至少20步（从10翻倍到20）
    return Math.max(20, maxSteps);
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
      if (c === Color.RED) return '霓虹红';
      if (c === Color.BLUE) return '霓虹蓝';
      if (c === Color.GREEN) return '霓虹绿';
      if (c === Color.YELLOW) return '霓虹黄';
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

