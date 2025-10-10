import { PixelBlock } from '@/types';
import { Grid } from './Grid';

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

    if (movedCount > 0 || stabilizedCount > 0) {
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
    const oldX = pixel.x;
    const oldY = pixel.y;
    
    // 从旧位置移除
    this.grid.setPixel(oldX, oldY, null);

    // 更新坐标
    pixel.x = newX;
    pixel.y = newY;

    // 放置到新位置
    this.grid.setPixel(newX, newY, pixel);
    
    // 检查旧位置上方的像素块是否失去支撑
    this.checkPixelAbove(oldX, oldY);
  }
  
  /**
   * 检查指定位置上方的像素块是否失去支撑
   */
  private checkPixelAbove(x: number, y: number): void {
    // 检查正上方的像素块
    if (y > 0) {
      const abovePixel = this.grid.getPixel(x, y - 1);
      if (abovePixel && abovePixel.isStable) {
        // 检查该像素块是否还有支撑
        const canMoveDown = this.canMoveTo(abovePixel.x, abovePixel.y + 1);
        const canMoveLeftDown = this.canMoveTo(abovePixel.x - 1, abovePixel.y + 1);
        const canMoveRightDown = this.canMoveTo(abovePixel.x + 1, abovePixel.y + 1);
        
        if (canMoveDown || canMoveLeftDown || canMoveRightDown) {
          // 失去支撑，标记为不稳定
          abovePixel.isStable = false;
          this.activePixels.add(abovePixel);
        }
      }
    }
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
   * 重要：需要按照三方向下落规则检查
   * 注意：这个方法会被多次调用，直到所有像素块都稳定
   */
  recheckStability(): void {
    const allPixels = this.grid.getAllPixels();
    
    console.log(`重新检查 ${allPixels.length} 个像素块的稳定性`);
    
    let unstableCount = 0;
    let alreadyActiveCount = 0;
    
    allPixels.forEach((pixel) => {
      // 检查三个方向：正下、左下、右下
      const canMoveDown = this.canMoveTo(pixel.x, pixel.y + 1);
      const canMoveLeftDown = this.canMoveTo(pixel.x - 1, pixel.y + 1);
      const canMoveRightDown = this.canMoveTo(pixel.x + 1, pixel.y + 1);
      
      // 如果任何一个方向可以移动，标记为不稳定
      if (canMoveDown || canMoveLeftDown || canMoveRightDown) {
        if (this.activePixels.has(pixel)) {
          alreadyActiveCount++;
        } else {
          pixel.isStable = false;
          this.activePixels.add(pixel);
          unstableCount++;
        }
      }
    });
    
    console.log(`新增 ${unstableCount} 个不稳定像素块（已活跃: ${alreadyActiveCount}），总活跃: ${this.activePixels.size}`);
  }

  /**
   * 清空活跃像素块集合
   */
  clear(): void {
    this.activePixels.clear();
  }
}

