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
   * 渲染网格线（调试用）
   */
  renderGrid(): void {
    this.graphics.clear();
    this.graphics.lineStyle(1, 0x444444, 0.3);

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
   * 渲染游戏区域边框
   */
  renderBorder(): void {
    this.graphics.lineStyle(3, 0xffffff, 1);
    this.graphics.strokeRect(
      GAME_AREA_OFFSET_X,
      GAME_AREA_OFFSET_Y,
      PIXEL_GRID_WIDTH * PIXEL_SIZE,
      PIXEL_GRID_HEIGHT * PIXEL_SIZE
    );
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
   * 渲染单个像素块
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

    // 添加微弱的边框效果
    sprite.setStrokeStyle(1, 0x000000, 0.3);
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

