import { describe, it, expect, beforeEach } from 'vitest';
import { Grid } from '@/core/Grid';
import { PixelBlock, Color } from '@/types';

describe('Grid', () => {
  let grid: Grid;

  beforeEach(() => {
    grid = new Grid();
  });

  describe('基本操作', () => {
    it('应该正确初始化空网格', () => {
      const pixel = grid.getPixel(0, 0);
      expect(pixel).toBeNull();
    });

    it('应该能设置和获取像素块', () => {
      const testPixel: PixelBlock = {
        x: 5,
        y: 10,
        color: Color.Red,
        isActive: true,
      };

      grid.setPixel(5, 10, testPixel);
      const retrieved = grid.getPixel(5, 10);

      expect(retrieved).toEqual(testPixel);
      expect(retrieved?.color).toBe(Color.Red);
    });

    it('应该能清除像素块', () => {
      const testPixel: PixelBlock = {
        x: 3,
        y: 7,
        color: Color.Blue,
        isActive: true,
      };

      grid.setPixel(3, 7, testPixel);
      expect(grid.getPixel(3, 7)).not.toBeNull();

      grid.setPixel(3, 7, null);
      expect(grid.getPixel(3, 7)).toBeNull();
    });
  });

  describe('边界检查', () => {
    it('应该对无效位置返回null', () => {
      expect(grid.getPixel(-1, 0)).toBeNull();
      expect(grid.getPixel(0, -1)).toBeNull();
      expect(grid.getPixel(1000, 1000)).toBeNull();
    });

    it('应该验证位置有效性', () => {
      expect(grid.isValidPixelPosition(0, 0)).toBe(true);
      expect(grid.isValidPixelPosition(-1, 0)).toBe(false);
      expect(grid.isValidPixelPosition(0, -1)).toBe(false);
    });
  });

  describe('逻辑网格转换', () => {
    it('应该能构建逻辑网格', () => {
      const logicalGrid = grid.buildLogicalGrid();
      expect(logicalGrid).toBeDefined();
      expect(logicalGrid.length).toBeGreaterThan(0);
    });

    it('空格子应该被检测为空', () => {
      expect(grid.isLogicalCellEmpty(0, 0)).toBe(true);
    });

    it('有像素的格子应该不为空', () => {
      const testPixel: PixelBlock = {
        x: 0,
        y: 0,
        color: Color.Red,
        isActive: false,
      };
      grid.setPixel(0, 0, testPixel);
      expect(grid.isLogicalCellEmpty(0, 0)).toBe(false);
    });
  });

  describe('活跃像素管理', () => {
    it('应该能获取所有活跃像素', () => {
      const pixel1: PixelBlock = { x: 0, y: 0, color: Color.Red, isActive: true };
      const pixel2: PixelBlock = { x: 1, y: 1, color: Color.Blue, isActive: true };

      grid.setPixel(0, 0, pixel1);
      grid.setPixel(1, 1, pixel2);

      const activePixels = grid.getAllPixels();
      expect(activePixels.length).toBe(2);
    });

    it('应该能获取像素总数', () => {
      const pixel: PixelBlock = { x: 5, y: 5, color: Color.Red, isActive: true };
      grid.setPixel(5, 5, pixel);

      expect(grid.getTotalPixelCount()).toBe(1);
    });
  });
});
