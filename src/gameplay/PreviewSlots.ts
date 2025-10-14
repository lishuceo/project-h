import { TetrominoData, Color } from '@/types';
import { BagSystem, SeededBagSystem, createTetromino, randomColor } from '@/core/Tetromino';
import { SeededRandom } from '@/utils/seedRandom';
import { PREVIEW_SLOTS } from '@/config/constants';

/**
 * 预览槽位系统
 * 参考设计文档第7章
 * 
 * 支持两种模式：
 * 1. 普通模式（无种子）：用于普通游戏，完全随机
 * 2. 种子模式（有种子）：用于每日挑战，确定性生成
 */
export class PreviewSlots {
  private slots: (TetrominoData | null)[];
  private bagSystem: BagSystem | SeededBagSystem;
  private random: SeededRandom | null = null;  // 种子随机数生成器（仅种子模式使用）
  private isSeeded: boolean = false;           // 是否为种子模式
  private availableColors: Color[] | null = null;  // 🎯 可用颜色列表（每日挑战使用）

  /**
   * @param seed 可选的随机种子，用于每日挑战（相同种子 → 相同方块序列）
   * @param availableColors 可选的颜色列表，限制方块只使用这些颜色（确保关卡可解）
   */
  constructor(seed?: number, availableColors?: Color[]) {
    if (seed !== undefined) {
      // 🎯 种子模式：用于每日挑战
      this.isSeeded = true;
      this.random = new SeededRandom(seed + 1000);  // +1000偏移，避免与关卡生成冲突
      this.bagSystem = new SeededBagSystem(this.random);
      this.availableColors = availableColors || null;
      console.log(`🎲 创建种子化PreviewSlots [种子: ${seed + 1000}]`);
      if (availableColors) {
        console.log(`🎨 限制颜色:`, availableColors);
      }
    } else {
      // 普通模式：用于普通游戏
      this.bagSystem = new BagSystem();
      console.log(`🎲 创建随机PreviewSlots`);
    }
    
    this.slots = Array(PREVIEW_SLOTS).fill(null);
    this.fillAllSlots();
  }

  /**
   * 填充所有槽位
   */
  private fillAllSlots(): void {
    for (let i = 0; i < PREVIEW_SLOTS; i++) {
      this.refillSlot(i);
    }
  }

  /**
   * 重新填充指定槽位
   * 参考设计文档7.4节
   */
  private refillSlot(slotIndex: number): void {
    const shape = this.bagSystem.getNextShape();
    const color = this.isSeeded ? this.getSeededColor() : randomColor();
    this.slots[slotIndex] = createTetromino(shape, color, 0);
  }
  
  /**
   * 基于种子生成颜色（确定性）
   */
  private getSeededColor(): Color {
    if (!this.random) {
      return randomColor();  // fallback
    }
    
    // 🎯 如果指定了可用颜色列表，只从中选择（确保关卡可解）
    if (this.availableColors && this.availableColors.length > 0) {
      return this.random.choice(this.availableColors);
    }
    
    // 否则使用默认的4种明亮颜色
    const brightColors = [
      Color.RED,
      Color.BLUE,
      Color.GREEN,
      Color.YELLOW
    ];
    
    return this.random.choice(brightColors);
  }

  /**
   * 获取指定槽位的方块
   */
  getSlot(index: number): TetrominoData | null {
    if (index < 0 || index >= PREVIEW_SLOTS) {
      return null;
    }
    return this.slots[index];
  }

  /**
   * 设置指定槽位的方块（用于放置失败时恢复）
   */
  setSlot(index: number, tetromino: TetrominoData | null): void {
    if (index >= 0 && index < PREVIEW_SLOTS) {
      this.slots[index] = tetromino;
    }
  }

  /**
   * 使用指定槽位的方块（使用后立即补充）
   * 参考设计文档7.3节
   * @deprecated 使用 getSlot() 和 refillSlot() 分开调用以避免刷方块bug
   */
  useSlot(index: number): TetrominoData | null {
    const tetromino = this.getSlot(index);
    if (tetromino) {
      this.refillSlot(index);
    }
    return tetromino;
  }

  /**
   * 补充指定槽位（放置成功后调用）
   */
  refillSlotAfterPlace(index: number): void {
    this.refillSlot(index);
  }

  /**
   * 获取所有槽位
   */
  getAllSlots(): (TetrominoData | null)[] {
    return [...this.slots];
  }

  /**
   * 检查是否有任何槽位的方块可以放置
   * 参考设计文档第11章游戏结束判定
   */
  hasAnyPlaceableBlock(canPlaceChecker: (tetromino: TetrominoData) => boolean): boolean {
    for (const slot of this.slots) {
      if (slot && canPlaceChecker(slot)) {
        return true;
      }
    }
    return false;
  }
}

