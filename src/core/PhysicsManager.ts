import { PixelBlock } from '@/types';
import { Grid } from './Grid';
import { PIXEL_GRID_HEIGHT } from '@/config/constants';

/**
 * 物理管理器 - 实现三方向下落物理系统
 * 参考设计文档第5章
 */
export class PhysicsManager {
  private grid: Grid;
  private activePixels: Set<PixelBlock>;

  constructor(grid: Grid) {
    this.grid = grid;
    this.activePixels = new Set();
  }

  /**
   * 添加像素块到活跃集合
   */
  addPixel(pixel: PixelBlock): void {
    this.activePixels.add(pixel);
    pixel.isStable = false;
  }

  /**
   * 添加多个像素块
   */
  addPixels(pixels: PixelBlock[]): void {
    pixels.forEach((pixel) => this.addPixel(pixel));
  }

  /**
   * 更新所有活跃像素块
   * 参考设计文档5.4.2节
   */
  update(): void {
    if (this.activePixels.size === 0) {
      return; // 没有活跃像素块，直接返回
    }

    // 清除上一帧的更新标记
    this.activePixels.forEach((pixel) => {
      pixel.updatedThisFrame = false;
    });

    // 将活跃像素块转为数组并按Y坐标排序（从下往上）
    const pixelsToUpdate = Array.from(this.activePixels).sort(
      (a, b) => b.y - a.y
    );

    let movedCount = 0;
    let stabilizedCount = 0;

    // 从下往上更新
    for (const pixel of pixelsToUpdate) {
      // 跳过已稳定或已更新的像素块
      if (pixel.isStable || pixel.updatedThisFrame) {
        continue;
      }

      const oldY = pixel.y;
      
      // 更新该像素块
      this.updatePixelPhysics(pixel);

      // 标记为已更新
      pixel.updatedThisFrame = true;

      // 统计
      if (pixel.y !== oldY) {
        movedCount++;
      }

      // 如果已稳定，从活跃集合中移除
      if (pixel.isStable) {
        this.activePixels.delete(pixel);
        stabilizedCount++;
      }
    }

    // 只在有显著变化时输出日志
    if (movedCount > 100 || stabilizedCount > 50 || this.activePixels.size % 100 === 0) {
      console.log(`物理更新: ${movedCount}个移动, ${stabilizedCount}个稳定, 剩余活跃: ${this.activePixels.size}`);
    }
  }

  /**
   * 三方向下落算法
   * 参考设计文档5.2.2节
   */
  private updatePixelPhysics(pixel: PixelBlock): void {
    const x = pixel.x;
    const y = pixel.y;

    // === 优先级1: 尝试正下方 ===
    if (this.canMoveTo(x, y + 1)) {
      this.movePixelTo(pixel, x, y + 1);
      return; // 移动成功，继续下落
    }

    // === 正下方被占用，尝试斜向滑落 ===
    // 随机选择左右优先顺序（避免系统性偏向）
    const leftFirst = Math.random() > 0.5;
    const directions = leftFirst ? [-1, 1] : [1, -1];

    for (const dir of directions) {
      // 优先级2/3: 尝试斜下方 (左下或右下)
      const newX = x + dir;
      const newY = y + 1;

      if (this.canMoveTo(newX, newY)) {
        this.movePixelTo(pixel, newX, newY);
        return; // 斜向滑落成功
      }
    }

    // === 三个方向都无法移动 → 稳定 ===
    pixel.isStable = true;
  }

  /**
   * 检查目标位置是否可以移动到
   */
  private canMoveTo(x: number, y: number): boolean {
    // 边界检查
    if (!this.grid.isValidPixelPosition(x, y)) {
      return false;
    }

    // 检查目标位置是否为空
    return this.grid.getPixel(x, y) === null;
  }

  /**
   * 移动像素块到新位置
   */
  private movePixelTo(pixel: PixelBlock, newX: number, newY: number): void {
    // 从旧位置移除
    this.grid.setPixel(pixel.x, pixel.y, null);

    // 更新坐标
    pixel.x = newX;
    pixel.y = newY;

    // 放置到新位置
    this.grid.setPixel(newX, newY, pixel);
  }

  /**
   * 检查是否所有像素块都已稳定
   */
  get allStable(): boolean {
    return this.activePixels.size === 0;
  }

  /**
   * 获取活跃像素块数量
   */
  get activeCount(): number {
    return this.activePixels.size;
  }

  /**
   * 标记某个像素块为不稳定（用于消除后重新触发重力）
   */
  markAsUnstable(pixel: PixelBlock): void {
    if (pixel && !pixel.isStable) {
      return;
    }
    
    if (pixel) {
      pixel.isStable = false;
      this.activePixels.add(pixel);
    }
  }

  /**
   * 重新检查所有像素块的稳定性（消除后调用）
   * 参考设计文档8.4节
   * 
   * 重要：增量检查，只标记真正失去支撑的像素块
   */
  recheckStability(): void {
    const allPixels = this.grid.getAllPixels();
    
    console.log(`重新检查 ${allPixels.length} 个像素块的稳定性`);
    
    let newUnstableCount = 0;
    let alreadyActive = 0;
    let stableCount = 0;
    
    // 从下往上检查每个像素块
    allPixels.sort((a, b) => b.y - a.y);
    
    allPixels.forEach((pixel) => {
      // 跳过已经在活跃集合中的像素块
      if (this.activePixels.has(pixel)) {
        alreadyActive++;
        return;
      }
      
      // 只检查已稳定的像素块
      if (!pixel.isStable) {
        return;
      }
      
      // 检查下方是否真的为空（不包括不稳定的像素块）
      const hasRealSupport = this.hasRealSupportBelow(pixel.x, pixel.y);
      
      if (!hasRealSupport) {
        // 失去真实支撑，标记为不稳定
        pixel.isStable = false;
        this.activePixels.add(pixel);
        newUnstableCount++;
      } else {
        stableCount++;
      }
    });
    
    console.log(`重新检查结果: 新增不稳定=${newUnstableCount}, 已活跃=${alreadyActive}, 稳定=${stableCount}, 总活跃=${this.activePixels.size}`);
  }
  
  /**
   * 检查像素块是否真的稳定（三个方向都无法移动）
   * 根据三方向下落规则，只要有一个方向可以移动就不稳定
   */
  private hasRealSupportBelow(x: number, y: number): boolean {
    // 已经在底部
    if (y >= PIXEL_GRID_HEIGHT - 1) {
      return true;
    }
    
    // 检查正下方
    const canMoveDown = this.canMoveToStableOnly(x, y + 1);
    if (canMoveDown) {
      return false; // 可以向下移动，不稳定
    }
    
    // 检查左下方
    const canMoveLeftDown = this.canMoveToStableOnly(x - 1, y + 1);
    if (canMoveLeftDown) {
      return false; // 可以左斜下移动，不稳定
    }
    
    // 检查右下方
    const canMoveRightDown = this.canMoveToStableOnly(x + 1, y + 1);
    if (canMoveRightDown) {
      return false; // 可以右斜下移动，不稳定
    }
    
    // 三个方向都被占用，稳定
    return true;
  }
  
  /**
   * 检查目标位置是否可以移动到（只考虑稳定的像素块作为障碍）
   */
  private canMoveToStableOnly(x: number, y: number): boolean {
    // 边界检查
    if (!this.grid.isValidPixelPosition(x, y)) {
      return false;
    }
    
    // 检查目标位置
    const pixel = this.grid.getPixel(x, y);
    
    // 如果位置为空，可以移动
    if (!pixel) {
      return true;
    }
    
    // 如果有不稳定的像素块，认为可以移动（它会让开）
    if (!pixel.isStable) {
      return true;
    }
    
    // 有稳定的像素块，不能移动
    return false;
  }

  /**
   * 清空活跃像素块集合
   */
  clear(): void {
    this.activePixels.clear();
  }
}

