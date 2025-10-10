import { TetrominoData } from '@/types';
import { BagSystem, createTetromino, randomColor } from '@/core/Tetromino';
import { PREVIEW_SLOTS } from '@/config/constants';

/**
 * 预览槽位系统
 * 参考设计文档第7章
 */
export class PreviewSlots {
  private slots: (TetrominoData | null)[];
  private bagSystem: BagSystem;

  constructor() {
    this.bagSystem = new BagSystem();
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
    const color = randomColor();
    this.slots[slotIndex] = createTetromino(shape, color, 0);
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
   */
  useSlot(index: number): TetrominoData | null {
    const tetromino = this.getSlot(index);
    if (tetromino) {
      this.refillSlot(index);
    }
    return tetromino;
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

