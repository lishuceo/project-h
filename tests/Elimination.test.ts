import { describe, it, expect, beforeEach } from 'vitest';
import { Grid } from '@/core/Grid';
import { EliminationSystem } from '@/gameplay/Elimination';
import { PixelBlock, Color } from '@/types';

describe('EliminationSystem', () => {
  let grid: Grid;
  let eliminationSystem: EliminationSystem;

  beforeEach(() => {
    grid = new Grid();
    eliminationSystem = new EliminationSystem(grid);
  });

  describe('消除检测', () => {
    it('空网格不应产生消除', () => {
      const clusters = eliminationSystem.checkElimination();
      expect(clusters).toHaveLength(0);
    });

    it('不连通的像素不应产生消除', () => {
      // 在左边放一个红色像素
      const leftPixel: PixelBlock = {
        x: 0,
        y: 10,
        color: Color.Red,
        isActive: false,
      };
      grid.setPixel(0, 10, leftPixel);

      // 在右边放一个红色像素（但不连通）
      const rightPixel: PixelBlock = {
        x: 119,
        y: 10,
        color: Color.Red,
        isActive: false,
      };
      grid.setPixel(119, 10, rightPixel);

      const clusters = eliminationSystem.checkElimination();
      expect(clusters).toHaveLength(0);
    });

    it('连通且触及两边的集群应该被检测到', () => {
      // 创建一条从左到右的水平线
      for (let x = 0; x < 120; x++) {
        const pixel: PixelBlock = {
          x,
          y: 50,
          color: Color.Red,
          isActive: false,
        };
        grid.setPixel(x, 50, pixel);
      }

      const clusters = eliminationSystem.checkElimination();
      expect(clusters.length).toBeGreaterThan(0);
      expect(clusters[0].cluster.color).toBe(Color.Red);
    });

    it('不同颜色的集群应该分开检测', () => {
      // 创建红色线（从左到右）
      for (let x = 0; x < 120; x++) {
        const pixel: PixelBlock = {
          x,
          y: 30,
          color: Color.Red,
          isActive: false,
        };
        grid.setPixel(x, 30, pixel);
      }

      // 创建蓝色线（从左到右）
      for (let x = 0; x < 120; x++) {
        const pixel: PixelBlock = {
          x,
          y: 60,
          color: Color.Blue,
          isActive: false,
        };
        grid.setPixel(x, 60, pixel);
      }

      const clusters = eliminationSystem.checkElimination();
      expect(clusters.length).toBeGreaterThanOrEqual(2);
    });
  });

  describe('执行消除', () => {
    it('应该能正确移除被消除的像素', () => {
      // 创建一条水平线
      for (let x = 0; x < 120; x++) {
        const pixel: PixelBlock = {
          x,
          y: 50,
          color: Color.Red,
          isActive: false,
        };
        grid.setPixel(x, 50, pixel);
      }

      // 检测消除
      const clusters = eliminationSystem.checkElimination();
      expect(clusters.length).toBeGreaterThan(0);

      // 执行消除
      clusters.forEach(({ pixels }) => {
        pixels.forEach(pixel => {
          grid.setPixel(pixel.x, pixel.y, null);
        });
      });

      // 验证像素已被清除
      expect(grid.getPixel(60, 50)).toBeNull();
    });
  });
});
