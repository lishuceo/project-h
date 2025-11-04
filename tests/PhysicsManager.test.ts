import { describe, it, expect, beforeEach } from 'vitest';
import { PhysicsManager } from '@/core/PhysicsManager';
import { Grid } from '@/core/Grid';
import { PixelBlock, Color } from '@/types';

describe('PhysicsManager', () => {
  let grid: Grid;
  let physics: PhysicsManager;

  beforeEach(() => {
    grid = new Grid();
    physics = new PhysicsManager(grid);
  });

  describe('初始化', () => {
    it('应该正确初始化', () => {
      expect(physics).toBeDefined();
      expect(physics.allStable).toBe(true);
      expect(physics.activeCount).toBe(0);
    });
  });

  describe('添加像素块', () => {
    it('应该能添加单个像素块', () => {
      const pixel: PixelBlock = {
        x: 5,
        y: 5,
        color: Color.Red,
        isActive: true,
        isStable: true,
      };

      physics.addPixel(pixel);

      expect(physics.activeCount).toBe(1);
      expect(physics.allStable).toBe(false);
      expect(pixel.isStable).toBe(false);
    });

    it('应该能添加多个像素块', () => {
      const pixels: PixelBlock[] = [
        { x: 0, y: 0, color: Color.Red, isActive: true, isStable: true },
        { x: 1, y: 1, color: Color.Blue, isActive: true, isStable: true },
        { x: 2, y: 2, color: Color.Green, isActive: true, isStable: true },
      ];

      physics.addPixels(pixels);

      expect(physics.activeCount).toBe(3);
      expect(physics.allStable).toBe(false);
    });
  });

  describe('垂直下落', () => {
    it('像素块应该向下落', () => {
      const pixel: PixelBlock = {
        x: 60,
        y: 10,
        color: Color.Red,
        isActive: true,
        isStable: false,
      };

      grid.setPixel(60, 10, pixel);
      physics.addPixel(pixel);

      const initialY = pixel.y;

      // 多次更新，让像素块下落
      for (let i = 0; i < 5; i++) {
        physics.update();
      }

      expect(pixel.y).toBeGreaterThan(initialY);
    });

    it('像素块应该在到达底部时稳定', () => {
      const pixel: PixelBlock = {
        x: 60,
        y: 219,
        color: Color.Red,
        isActive: true,
        isStable: false,
      };

      grid.setPixel(60, 219, pixel);
      physics.addPixel(pixel);

      physics.update();

      expect(pixel.isStable).toBe(true);
      expect(physics.allStable).toBe(true);
    });

    it('像素块应该在碰到其他像素块时稳定', () => {
      // 放置底部像素块（中心）
      const bottomPixel: PixelBlock = {
        x: 60,
        y: 119,
        color: Color.Blue,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(60, 119, bottomPixel);

      // 放置左右两侧的像素块，防止斜向滑落
      const leftPixel: PixelBlock = {
        x: 59,
        y: 119,
        color: Color.Green,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(59, 119, leftPixel);

      const rightPixel: PixelBlock = {
        x: 61,
        y: 119,
        color: Color.Yellow,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(61, 119, rightPixel);

      // 放置顶部像素块
      const topPixel: PixelBlock = {
        x: 60,
        y: 50,
        color: Color.Red,
        isActive: true,
        isStable: false,
      };
      grid.setPixel(60, 50, topPixel);
      physics.addPixel(topPixel);

      const initialY = topPixel.y;

      // 多次更新物理直到稳定
      let iterations = 0;
      while (!physics.allStable && iterations < 200) {
        physics.update();
        iterations++;
      }

      // 顶部像素块应该下落并稳定在底部像素块上方
      expect(topPixel.isStable).toBe(true);
      expect(topPixel.y).toBeGreaterThan(initialY);
      expect(topPixel.y).toBe(118);
    });
  });

  describe('斜向滑落', () => {
    it('像素块应该能斜向滑落', () => {
      // 创建一个阻挡物
      const obstacle: PixelBlock = {
        x: 60,
        y: 100,
        color: Color.Blue,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(60, 100, obstacle);

      // 在阻挡物正上方放置像素块
      const pixel: PixelBlock = {
        x: 60,
        y: 98,
        color: Color.Red,
        isActive: true,
        isStable: false,
      };
      grid.setPixel(60, 98, pixel);
      physics.addPixel(pixel);

      const initialX = pixel.x;

      // 第一次更新：下落到99
      physics.update();
      expect(pixel.y).toBe(99);

      // 第二次更新：应该斜向滑落（左或右）
      physics.update();
      expect(pixel.x).not.toBe(initialX);
    });

    it('像素块应该能堆叠成三角形', () => {
      // 底部中心
      const bottom: PixelBlock = {
        x: 60,
        y: 119,
        color: Color.Blue,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(60, 119, bottom);

      // 底部左右，防止斜向滑落
      const bottomLeft: PixelBlock = {
        x: 59,
        y: 119,
        color: Color.Cyan,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(59, 119, bottomLeft);

      const bottomRight: PixelBlock = {
        x: 61,
        y: 119,
        color: Color.Magenta,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(61, 119, bottomRight);

      // 第二层左右
      const left: PixelBlock = {
        x: 59,
        y: 118,
        color: Color.Red,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(59, 118, left);

      const right: PixelBlock = {
        x: 61,
        y: 118,
        color: Color.Green,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(61, 118, right);

      // 顶部像素块应该能稳定在上面
      const top: PixelBlock = {
        x: 60,
        y: 50,
        color: Color.Yellow,
        isActive: true,
        isStable: false,
      };
      grid.setPixel(60, 50, top);
      physics.addPixel(top);

      const initialY = top.y;

      // 多次更新直到稳定
      let iterations = 0;
      while (!physics.allStable && iterations < 200) {
        physics.update();
        iterations++;
      }

      // 验证像素块最终稳定且位置合理
      // 应该稳定在底部的上方中间位置 (y=118)
      expect(top.isStable).toBe(true);
      expect(top.y).toBeGreaterThan(initialY);
      expect(top.y).toBe(118);
      expect(top.x).toBe(60);
    });
  });

  describe('稳定性检查', () => {
    it('allStable 应该正确反映状态', () => {
      expect(physics.allStable).toBe(true);

      const pixel: PixelBlock = {
        x: 60,
        y: 10,
        color: Color.Red,
        isActive: true,
        isStable: false,
      };
      grid.setPixel(60, 10, pixel);
      physics.addPixel(pixel);

      expect(physics.allStable).toBe(false);

      // 让像素块落到底部
      while (!physics.allStable) {
        physics.update();
      }

      expect(physics.allStable).toBe(true);
    });

    it('activeCount 应该正确统计', () => {
      expect(physics.activeCount).toBe(0);

      const pixels: PixelBlock[] = [];
      for (let i = 0; i < 5; i++) {
        const pixel: PixelBlock = {
          x: i * 10,
          y: 10,
          color: Color.Red,
          isActive: true,
          isStable: false,
        };
        grid.setPixel(pixel.x, pixel.y, pixel);
        pixels.push(pixel);
      }

      physics.addPixels(pixels);
      expect(physics.activeCount).toBe(5);
    });
  });

  describe('重新检查稳定性', () => {
    it('消除后应该能重新检查稳定性', () => {
      // 创建一个悬空结构：底部有支撑，中间空缺
      const bottom: PixelBlock = {
        x: 60,
        y: 100,
        color: Color.Blue,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(60, 100, bottom);

      const top: PixelBlock = {
        x: 60,
        y: 98,
        color: Color.Red,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(60, 98, top);

      // 移除底部支撑
      grid.setPixel(60, 100, null);

      // 重新检查稳定性
      physics.recheckStability();

      // 顶部像素块应该被标记为不稳定
      expect(physics.activeCount).toBeGreaterThan(0);
    });

    it('底部和紧邻上方的像素块应该保持稳定', () => {
      // 在底部创建一行稳定的像素块
      const bottomRow: PixelBlock[] = [];
      for (let x = 58; x <= 62; x++) {
        const pixel: PixelBlock = {
          x,
          y: 219,
          color: Color.Blue,
          isActive: false,
          isStable: true,
        };
        grid.setPixel(x, 219, pixel);
        bottomRow.push(pixel);
      }

      // 在上面创建另一行稳定的像素块
      const topRow: PixelBlock[] = [];
      for (let x = 58; x <= 62; x++) {
        const pixel: PixelBlock = {
          x,
          y: 218,
          color: Color.Red,
          isActive: false,
          isStable: true,
        };
        grid.setPixel(x, 218, pixel);
        topRow.push(pixel);
      }

      // 重新检查稳定性
      physics.recheckStability();

      // 底部像素块应该保持稳定（在底边）
      bottomRow.forEach(pixel => {
        expect(pixel.isStable).toBe(true);
      });

      // 顶部像素块应该保持稳定（被完全支撑）
      topRow.forEach(pixel => {
        expect(pixel.isStable).toBe(true);
      });

      expect(physics.activeCount).toBe(0);
    });
  });

  describe('markAsUnstable', () => {
    it('应该能手动标记像素块为不稳定', () => {
      const pixel: PixelBlock = {
        x: 60,
        y: 100,
        color: Color.Red,
        isActive: false,
        isStable: true,
      };
      grid.setPixel(60, 100, pixel);

      expect(physics.activeCount).toBe(0);

      physics.markAsUnstable(pixel);

      expect(pixel.isStable).toBe(false);
      expect(physics.activeCount).toBe(1);
    });

    it('已经不稳定的像素块不应该重复添加', () => {
      const pixel: PixelBlock = {
        x: 60,
        y: 100,
        color: Color.Red,
        isActive: false,
        isStable: false,
      };
      grid.setPixel(60, 100, pixel);

      physics.markAsUnstable(pixel);
      physics.markAsUnstable(pixel);

      expect(physics.activeCount).toBe(0);
    });
  });

  describe('清空', () => {
    it('clear 应该清空活跃像素块集合', () => {
      const pixels: PixelBlock[] = [];
      for (let i = 0; i < 3; i++) {
        const pixel: PixelBlock = {
          x: i * 10,
          y: 10,
          color: Color.Red,
          isActive: true,
          isStable: false,
        };
        grid.setPixel(pixel.x, pixel.y, pixel);
        pixels.push(pixel);
      }

      physics.addPixels(pixels);
      expect(physics.activeCount).toBe(3);

      physics.clear();
      expect(physics.activeCount).toBe(0);
      expect(physics.allStable).toBe(true);
    });
  });

  describe('批量下落测试', () => {
    it('多个像素块应该能同时下落', () => {
      const pixels: PixelBlock[] = [];
      for (let i = 0; i < 10; i++) {
        const pixel: PixelBlock = {
          x: i * 10,
          y: 50,
          color: Color.Red,
          isActive: true,
          isStable: false,
        };
        grid.setPixel(pixel.x, pixel.y, pixel);
        pixels.push(pixel);
        physics.addPixel(pixel);
      }

      // 更新一次
      physics.update();

      // 所有像素块都应该下落
      pixels.forEach(pixel => {
        expect(pixel.y).toBeGreaterThan(50);
      });
    });

    it('大量像素块应该能正确处理', () => {
      const pixels: PixelBlock[] = [];
      for (let x = 0; x < 120; x += 10) {
        for (let y = 0; y < 100; y += 10) {
          const pixel: PixelBlock = {
            x,
            y,
            color: Color.Red,
            isActive: true,
            isStable: false,
          };
          grid.setPixel(x, y, pixel);
          pixels.push(pixel);
        }
      }

      physics.addPixels(pixels);
      expect(physics.activeCount).toBeGreaterThan(0);

      // 多次更新直到稳定
      let iterations = 0;
      while (!physics.allStable && iterations < 300) {
        physics.update();
        iterations++;
      }

      expect(physics.allStable).toBe(true);
    });
  });

  describe('性能测试 - 桶排序优化', () => {
    it('update() 在大量像素块时应该高效（< 20ms）', () => {
      // 创建 500 个活跃像素块
      const pixels: PixelBlock[] = [];
      for (let i = 0; i < 500; i++) {
        const pixel: PixelBlock = {
          x: (i % 110),
          y: Math.floor(i / 110) * 2, // 分散在不同 y 层
          color: Color.Red,
          isActive: true,
          isStable: false,
          updatedThisFrame: false,
          groupId: 1,
        };
        grid.setPixel(pixel.x, pixel.y, pixel);
        pixels.push(pixel);
      }

      physics.addPixels(pixels);

      // 性能测试：单次 update 调用
      const startTime = performance.now();
      physics.update();
      const updateTime = performance.now() - startTime;

      console.log(`500 个活跃像素块的 update() 耗时: ${updateTime.toFixed(2)}ms`);
      expect(updateTime).toBeLessThan(20); // 应该 < 20ms（桶排序优化后）
    });

    it('recheckStability() 在大量像素块时应该高效（< 30ms）', () => {
      // 创建 1000 个稳定像素块
      const pixels: PixelBlock[] = [];
      for (let i = 0; i < 1000; i++) {
        const pixel: PixelBlock = {
          x: (i % 110),
          y: 100 + Math.floor(i / 110),
          color: Color.Red,
          isActive: false,
          isStable: true,
          updatedThisFrame: false,
          groupId: 1,
        };
        grid.setPixel(pixel.x, pixel.y, pixel);
        pixels.push(pixel);
      }

      // 性能测试：recheckStability 调用
      const startTime = performance.now();
      physics.recheckStability();
      const recheckTime = performance.now() - startTime;

      console.log(`1000 个像素块的 recheckStability() 耗时: ${recheckTime.toFixed(2)}ms`);
      expect(recheckTime).toBeLessThan(30); // 应该 < 30ms（桶排序优化后）
    });

    it('桶排序应该产生与普通排序相同的更新顺序', () => {
      // 创建多个像素块在不同 y 坐标（使用合法范围）
      const pixels: PixelBlock[] = [
        { x: 50, y: 50, color: Color.Red, isActive: true, isStable: false, updatedThisFrame: false, groupId: 1 },
        { x: 51, y: 80, color: Color.Blue, isActive: true, isStable: false, updatedThisFrame: false, groupId: 1 },
        { x: 52, y: 25, color: Color.Green, isActive: true, isStable: false, updatedThisFrame: false, groupId: 1 },
        { x: 53, y: 100, color: Color.Yellow, isActive: true, isStable: false, updatedThisFrame: false, groupId: 1 },
      ];

      pixels.forEach(pixel => {
        grid.setPixel(pixel.x, pixel.y, pixel);
        physics.addPixel(pixel);
      });

      // 记录初始 y 坐标
      const initialYs = pixels.map(p => p.y);

      // 更新一次
      physics.update();

      // y 坐标大的像素块应该先被处理（先下落）
      // 验证：y=100 的像素块应该已经移动
      const pixel100 = pixels.find(p => initialYs[pixels.indexOf(p)] === 100);
      expect(pixel100).toBeDefined();
      expect(pixel100!.y).toBeGreaterThan(100);

      // 验证所有像素块都下落了
      pixels.forEach((pixel, index) => {
        expect(pixel.y).toBeGreaterThan(initialYs[index]);
      });
    });

    it('桶结构应该正确维护', () => {
      const pixel: PixelBlock = {
        x: 50,
        y: 100,
        color: Color.Red,
        isActive: true,
        isStable: false,
        updatedThisFrame: false,
        groupId: 1,
      };

      grid.setPixel(50, 100, pixel);
      physics.addPixel(pixel);

      // 验证桶中有该像素块
      expect(physics.activeCount).toBe(1);

      // 更新后，像素块移动到新的 y 坐标
      const oldY = pixel.y;
      physics.update();
      const newY = pixel.y;

      // 验证像素块已移动且仍在活跃集合中
      expect(newY).toBeGreaterThan(oldY);
      if (!pixel.isStable) {
        expect(physics.activeCount).toBe(1);
      }
    });
  });
});
