import {
  LOGICAL_GRID_WIDTH,
  LOGICAL_GRID_HEIGHT,
  PIXEL_GRID_WIDTH,
  PIXEL_GRID_HEIGHT,
  CELL_TO_PIXEL_RATIO,
} from '@/config/constants';
import { PixelBlock, Color } from '@/types';

/**
 * 网格系统 - 管理逻辑层和物理层的双层网格
 * 参考设计文档第3章
 */
export class Grid {
  // 像素网格 (120×220) - 用于物理和渲染
  private pixelGrid: (PixelBlock | null)[][];
  
  // 逻辑网格 (12×22) - 用于规则计算（缓存）
  private logicalGrid: (Color | null)[][];

  constructor() {
    // 初始化像素网格
    this.pixelGrid = Array(PIXEL_GRID_HEIGHT)
      .fill(null)
      .map(() => Array(PIXEL_GRID_WIDTH).fill(null));

    // 初始化逻辑网格
    this.logicalGrid = Array(LOGICAL_GRID_HEIGHT)
      .fill(null)
      .map(() => Array(LOGICAL_GRID_WIDTH).fill(null));
  }

  /**
   * 获取指定位置的像素块
   */
  getPixel(x: number, y: number): PixelBlock | null {
    if (!this.isValidPixelPosition(x, y)) {
      return null;
    }
    return this.pixelGrid[y][x];
  }

  /**
   * 设置指定位置的像素块
   */
  setPixel(x: number, y: number, pixel: PixelBlock | null): void {
    if (!this.isValidPixelPosition(x, y)) {
      return;
    }
    this.pixelGrid[y][x] = pixel;
  }

  /**
   * 检查像素位置是否合法
   */
  isValidPixelPosition(x: number, y: number): boolean {
    return x >= 0 && x < PIXEL_GRID_WIDTH && y >= 0 && y < PIXEL_GRID_HEIGHT;
  }

  /**
   * 检查逻辑位置是否合法
   */
  isValidLogicalPosition(x: number, y: number): boolean {
    return x >= 0 && x < LOGICAL_GRID_WIDTH && y >= 0 && y < LOGICAL_GRID_HEIGHT;
  }

  /**
   * 逻辑坐标转像素坐标
   */
  logicalToPixel(logicalX: number, logicalY: number): { x: number; y: number } {
    return {
      x: logicalX * CELL_TO_PIXEL_RATIO,
      y: logicalY * CELL_TO_PIXEL_RATIO,
    };
  }

  /**
   * 像素坐标转逻辑坐标
   */
  pixelToLogical(pixelX: number, pixelY: number): { x: number; y: number } {
    return {
      x: Math.floor(pixelX / CELL_TO_PIXEL_RATIO),
      y: Math.floor(pixelY / CELL_TO_PIXEL_RATIO),
    };
  }

  /**
   * 构建逻辑网格（从像素网格采样）
   * 参考设计文档8.2.1节
   */
  buildLogicalGrid(): (Color | null)[][] {
    for (let logicalY = 0; logicalY < LOGICAL_GRID_HEIGHT; logicalY++) {
      for (let logicalX = 0; logicalX < LOGICAL_GRID_WIDTH; logicalX++) {
        this.logicalGrid[logicalY][logicalX] = this.sampleCellColor(
          logicalX,
          logicalY
        );
      }
    }
    return this.logicalGrid;
  }

  /**
   * 采样逻辑格子的颜色（返回该格子中占比最高的颜色）
   * 参考设计文档8.2.1节
   */
  private sampleCellColor(logicalX: number, logicalY: number): Color | null {
    const startX = logicalX * CELL_TO_PIXEL_RATIO;
    const startY = logicalY * CELL_TO_PIXEL_RATIO;

    const colorCount = new Map<Color, number>();
    let totalPixels = 0;

    for (let py = 0; py < CELL_TO_PIXEL_RATIO; py++) {
      for (let px = 0; px < CELL_TO_PIXEL_RATIO; px++) {
        const pixel = this.pixelGrid[startY + py]?.[startX + px];
        if (pixel) {
          colorCount.set(pixel.color, (colorCount.get(pixel.color) || 0) + 1);
          totalPixels++;
        }
      }
    }

    if (totalPixels === 0) {
      return null;
    }

    // 返回占比最高的颜色
    let dominantColor: Color | null = null;
    let maxCount = 0;
    colorCount.forEach((count, color) => {
      if (count > maxCount) {
        maxCount = count;
        dominantColor = color;
      }
    });

    return dominantColor;
  }

  /**
   * 获取所有像素块（用于遍历）
   */
  getAllPixels(): PixelBlock[] {
    const pixels: PixelBlock[] = [];
    for (let y = 0; y < PIXEL_GRID_HEIGHT; y++) {
      for (let x = 0; x < PIXEL_GRID_WIDTH; x++) {
        const pixel = this.pixelGrid[y][x];
        if (pixel) {
          pixels.push(pixel);
        }
      }
    }
    return pixels;
  }

  /**
   * 清空网格
   */
  clear(): void {
    for (let y = 0; y < PIXEL_GRID_HEIGHT; y++) {
      for (let x = 0; x < PIXEL_GRID_WIDTH; x++) {
        this.pixelGrid[y][x] = null;
      }
    }
  }

  /**
   * 获取逻辑网格（缓存）
   */
  getLogicalGrid(): (Color | null)[][] {
    return this.logicalGrid;
  }

  /**
   * 检查逻辑格子区域是否为空（用于放置验证）
   */
  isLogicalCellEmpty(logicalX: number, logicalY: number): boolean {
    if (!this.isValidLogicalPosition(logicalX, logicalY)) {
      return false;
    }

    const startX = logicalX * CELL_TO_PIXEL_RATIO;
    const startY = logicalY * CELL_TO_PIXEL_RATIO;

    // 检查该逻辑格子对应的10×10像素块区域
    for (let py = 0; py < CELL_TO_PIXEL_RATIO; py++) {
      for (let px = 0; px < CELL_TO_PIXEL_RATIO; px++) {
        if (this.pixelGrid[startY + py][startX + px] !== null) {
          return false;
        }
      }
    }
    return true;
  }
}

