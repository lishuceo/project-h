import Phaser from 'phaser';
import { Grid } from '@/core/Grid';
import { PixelBlock } from '@/types';
import {
  PIXEL_SIZE,
  PIXEL_GRID_WIDTH,
  PIXEL_GRID_HEIGHT,
  GAME_AREA_OFFSET_X,
  GAME_AREA_OFFSET_Y,
} from '@/config/constants';

/**
 * 像素块渲染器
 * 参考设计文档第5.5节和第12章
 */
export class PixelRenderer {
  private scene: Phaser.Scene;
  private grid: Grid;
  private graphics: Phaser.GameObjects.Graphics;
  private pixelSprites: Map<PixelBlock, Phaser.GameObjects.Rectangle>;

  constructor(scene: Phaser.Scene, grid: Grid) {
    this.scene = scene;
    this.grid = grid;
    this.graphics = scene.add.graphics();
    this.pixelSprites = new Map();
  }

  /**
   * 渲染网格线（极弱化，只用于辅助定位）
   */
  renderGrid(): void {
    // 注意：不要在这里 clear()，否则会清除边框
    // 使用极细、极淡的线条（1px、20-25%透明度）
    this.graphics.lineStyle(1, 0x2a3344, 0.2);

    // 绘制垂直线
    for (let x = 0; x <= PIXEL_GRID_WIDTH; x += 10) {
      const screenX = GAME_AREA_OFFSET_X + x * PIXEL_SIZE;
      this.graphics.lineBetween(
        screenX,
        GAME_AREA_OFFSET_Y,
        screenX,
        GAME_AREA_OFFSET_Y + PIXEL_GRID_HEIGHT * PIXEL_SIZE
      );
    }

    // 绘制水平线
    for (let y = 0; y <= PIXEL_GRID_HEIGHT; y += 10) {
      const screenY = GAME_AREA_OFFSET_Y + y * PIXEL_SIZE;
      this.graphics.lineBetween(
        GAME_AREA_OFFSET_X,
        screenY,
        GAME_AREA_OFFSET_X + PIXEL_GRID_WIDTH * PIXEL_SIZE,
        screenY
      );
    }
  }

  /**
   * 渲染游戏区域边框（深色背景 + 简洁边框）
   */
  renderBorder(): void {
    // 先清除之前的绘制
    this.graphics.clear();

    const x = GAME_AREA_OFFSET_X;
    const y = GAME_AREA_OFFSET_Y;
    const width = PIXEL_GRID_WIDTH * PIXEL_SIZE;
    const height = PIXEL_GRID_HEIGHT * PIXEL_SIZE;
    const borderRadius = 6; // 小圆角

    // 1. 深色背景层（用于凸显方块）
    const bgLayer = this.scene.add.graphics();
    bgLayer.fillStyle(0x1a1f2e, 1); // 深蓝灰色背景
    bgLayer.fillRoundedRect(x, y, width, height, borderRadius);
    bgLayer.setDepth(-2);

    // 2. 简洁的白色边框
    const borderLayer = this.scene.add.graphics();
    borderLayer.lineStyle(2, 0xffffff, 0.3); // 2px白色半透明边框
    borderLayer.strokeRoundedRect(x, y, width, height, borderRadius);
    borderLayer.setDepth(-1);
  }

  /**
   * 渲染所有像素块
   */
  renderPixels(): void {
    const allPixels = this.grid.getAllPixels();

    // 移除已不存在的像素块的精灵
    this.pixelSprites.forEach((sprite, pixel) => {
      if (!allPixels.includes(pixel)) {
        sprite.destroy();
        this.pixelSprites.delete(pixel);
      }
    });

    // 渲染所有像素块
    allPixels.forEach((pixel) => {
      this.renderPixel(pixel);
    });
  }

  /**
   * 渲染单个像素块（霓虹风格 + 微弱发光）
   */
  private renderPixel(pixel: PixelBlock): void {
    let sprite = this.pixelSprites.get(pixel);

    if (!sprite) {
      // 创建新的矩形精灵
      const screenX = GAME_AREA_OFFSET_X + pixel.x * PIXEL_SIZE;
      const screenY = GAME_AREA_OFFSET_Y + pixel.y * PIXEL_SIZE;

      sprite = this.scene.add.rectangle(
        screenX + PIXEL_SIZE / 2,
        screenY + PIXEL_SIZE / 2,
        PIXEL_SIZE - 1,
        PIXEL_SIZE - 1,
        pixel.color
      );

      this.pixelSprites.set(pixel, sprite);
    } else {
      // 更新位置
      const screenX = GAME_AREA_OFFSET_X + pixel.x * PIXEL_SIZE;
      const screenY = GAME_AREA_OFFSET_Y + pixel.y * PIXEL_SIZE;

      sprite.setPosition(screenX + PIXEL_SIZE / 2, screenY + PIXEL_SIZE / 2);
      sprite.setFillStyle(pixel.color);
    }

    // 霓虹风格：深色边框 + 微弱内发光
    sprite.setStrokeStyle(1, 0x000000, 0.4);
  }

  /**
   * 清除所有渲染
   */
  clear(): void {
    this.graphics.clear();
    this.pixelSprites.forEach((sprite) => sprite.destroy());
    this.pixelSprites.clear();
  }

  /**
   * 销毁渲染器
   */
  destroy(): void {
    this.clear();
    this.graphics.destroy();
  }

  /**
   * 获取屏幕坐标（从网格坐标转换）
   */
  toScreenPosition(gridX: number, gridY: number): { x: number; y: number } {
    return {
      x: GAME_AREA_OFFSET_X + gridX * PIXEL_SIZE,
      y: GAME_AREA_OFFSET_Y + gridY * PIXEL_SIZE,
    };
  }

  /**
   * 从屏幕坐标转换到网格坐标
   */
  toGridPosition(screenX: number, screenY: number): { x: number; y: number } {
    return {
      x: Math.floor((screenX - GAME_AREA_OFFSET_X) / PIXEL_SIZE),
      y: Math.floor((screenY - GAME_AREA_OFFSET_Y) / PIXEL_SIZE),
    };
  }
}

