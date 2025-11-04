import { describe, it, expect, beforeEach } from 'vitest';
import {
  TETROMINO_SHAPES,
  COLOR_POOL,
  BagSystem,
  SeededBagSystem,
  randomColor,
  createTetromino,
  getTetrominoBounds,
} from '@/core/Tetromino';
import { ShapeType, Color } from '@/types';
import { SeededRandom } from '@/utils/seedRandom';

describe('Tetromino', () => {
  describe('方块形状定义', () => {
    it('所有形状都应该定义', () => {
      expect(TETROMINO_SHAPES[ShapeType.I]).toBeDefined();
      expect(TETROMINO_SHAPES[ShapeType.O]).toBeDefined();
      expect(TETROMINO_SHAPES[ShapeType.T]).toBeDefined();
      expect(TETROMINO_SHAPES[ShapeType.L]).toBeDefined();
      expect(TETROMINO_SHAPES[ShapeType.J]).toBeDefined();
      expect(TETROMINO_SHAPES[ShapeType.S]).toBeDefined();
      expect(TETROMINO_SHAPES[ShapeType.Z]).toBeDefined();
    });

    it('I形应该有2种旋转状态', () => {
      expect(TETROMINO_SHAPES[ShapeType.I]).toHaveLength(2);
      expect(TETROMINO_SHAPES[ShapeType.I][0]).toHaveLength(4);
      expect(TETROMINO_SHAPES[ShapeType.I][1]).toHaveLength(4);
    });

    it('O形应该有1种旋转状态（正方形）', () => {
      expect(TETROMINO_SHAPES[ShapeType.O]).toHaveLength(1);
      expect(TETROMINO_SHAPES[ShapeType.O][0]).toHaveLength(4);
    });

    it('T形应该有4种旋转状态', () => {
      expect(TETROMINO_SHAPES[ShapeType.T]).toHaveLength(4);
      TETROMINO_SHAPES[ShapeType.T].forEach(rotation => {
        expect(rotation).toHaveLength(4);
      });
    });

    it('所有形状都应该包含4个格子', () => {
      Object.values(TETROMINO_SHAPES).forEach(rotations => {
        rotations.forEach(rotation => {
          expect(rotation).toHaveLength(4);
        });
      });
    });
  });

  describe('颜色池', () => {
    it('应该包含4种颜色', () => {
      expect(COLOR_POOL).toHaveLength(4);
    });

    it('应该包含红黄绿蓝', () => {
      expect(COLOR_POOL).toContain(Color.RED);
      expect(COLOR_POOL).toContain(Color.YELLOW);
      expect(COLOR_POOL).toContain(Color.GREEN);
      expect(COLOR_POOL).toContain(Color.BLUE);
    });

    it('randomColor应该返回有效颜色', () => {
      const color = randomColor();
      expect(COLOR_POOL).toContain(color);
    });
  });

  describe('BagSystem', () => {
    let bagSystem: BagSystem;

    beforeEach(() => {
      bagSystem = new BagSystem();
    });

    it('应该能生成7个不同的形状', () => {
      const shapes = new Set<ShapeType>();
      for (let i = 0; i < 7; i++) {
        shapes.add(bagSystem.getNextShape());
      }
      expect(shapes.size).toBe(7);
    });

    it('每7个形状应该包含所有类型', () => {
      const shapes: ShapeType[] = [];
      for (let i = 0; i < 7; i++) {
        shapes.push(bagSystem.getNextShape());
      }

      expect(shapes).toContain(ShapeType.I);
      expect(shapes).toContain(ShapeType.O);
      expect(shapes).toContain(ShapeType.T);
      expect(shapes).toContain(ShapeType.L);
      expect(shapes).toContain(ShapeType.J);
      expect(shapes).toContain(ShapeType.S);
      expect(shapes).toContain(ShapeType.Z);
    });

    it('应该能连续生成多个袋子', () => {
      const shapes: ShapeType[] = [];
      for (let i = 0; i < 21; i++) {
        shapes.push(bagSystem.getNextShape());
      }
      expect(shapes).toHaveLength(21);
    });
  });

  describe('SeededBagSystem', () => {
    it('相同种子应该产生相同的形状序列', () => {
      const random1 = new SeededRandom(12345);
      const random2 = new SeededRandom(12345);
      const bag1 = new SeededBagSystem(random1);
      const bag2 = new SeededBagSystem(random2);

      const sequence1: ShapeType[] = [];
      const sequence2: ShapeType[] = [];

      for (let i = 0; i < 14; i++) {
        sequence1.push(bag1.getNextShape());
        sequence2.push(bag2.getNextShape());
      }

      expect(sequence1).toEqual(sequence2);
    });

    it('不同种子应该产生不同的形状序列', () => {
      const random1 = new SeededRandom(12345);
      const random2 = new SeededRandom(54321);
      const bag1 = new SeededBagSystem(random1);
      const bag2 = new SeededBagSystem(random2);

      const sequence1: ShapeType[] = [];
      const sequence2: ShapeType[] = [];

      for (let i = 0; i < 7; i++) {
        sequence1.push(bag1.getNextShape());
        sequence2.push(bag2.getNextShape());
      }

      expect(sequence1).not.toEqual(sequence2);
    });
  });

  describe('createTetromino', () => {
    it('应该创建有效的方块数据', () => {
      const tetromino = createTetromino(ShapeType.I, Color.RED, 0);

      expect(tetromino.shape).toBe(ShapeType.I);
      expect(tetromino.color).toBe(Color.RED);
      expect(tetromino.rotation).toBe(0);
      expect(tetromino.cells).toHaveLength(4);
    });

    it('应该正确处理旋转', () => {
      const tetromino0 = createTetromino(ShapeType.T, Color.BLUE, 0);
      const tetromino1 = createTetromino(ShapeType.T, Color.BLUE, 1);

      expect(tetromino0.cells).not.toEqual(tetromino1.cells);
    });

    it('旋转索引超出范围应该循环', () => {
      const tetromino0 = createTetromino(ShapeType.T, Color.GREEN, 0);
      const tetromino4 = createTetromino(ShapeType.T, Color.GREEN, 4);

      expect(tetromino0.cells).toEqual(tetromino4.cells);
    });
  });

  describe('getTetrominoBounds', () => {
    it('I形水平状态应该有正确的边界', () => {
      const tetromino = createTetromino(ShapeType.I, Color.RED, 0);
      const bounds = getTetrominoBounds(tetromino);

      expect(bounds.minX).toBe(0);
      expect(bounds.maxX).toBe(3);
      expect(bounds.minY).toBe(0);
      expect(bounds.maxY).toBe(0);
    });

    it('I形垂直状态应该有正确的边界', () => {
      const tetromino = createTetromino(ShapeType.I, Color.RED, 1);
      const bounds = getTetrominoBounds(tetromino);

      expect(bounds.minX).toBe(0);
      expect(bounds.maxX).toBe(0);
      expect(bounds.minY).toBe(0);
      expect(bounds.maxY).toBe(3);
    });

    it('O形应该是2x2的正方形', () => {
      const tetromino = createTetromino(ShapeType.O, Color.YELLOW, 0);
      const bounds = getTetrominoBounds(tetromino);

      expect(bounds.maxX - bounds.minX).toBe(1);
      expect(bounds.maxY - bounds.minY).toBe(1);
    });

    it('T形应该有合理的边界', () => {
      const tetromino = createTetromino(ShapeType.T, Color.BLUE, 0);
      const bounds = getTetrominoBounds(tetromino);

      expect(bounds.minX).toBeGreaterThanOrEqual(0);
      expect(bounds.maxX).toBeLessThanOrEqual(3);
      expect(bounds.minY).toBeGreaterThanOrEqual(0);
      expect(bounds.maxY).toBeLessThanOrEqual(3);
    });
  });
});
