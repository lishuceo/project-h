import { describe, it, expect } from 'vitest';
import { SeededRandom } from '@/utils/seedRandom';

describe('SeededRandom', () => {
  describe('基本功能', () => {
    it('应该能创建实例', () => {
      const random = new SeededRandom(12345);
      expect(random).toBeDefined();
    });

    it('应该生成0到1之间的随机数', () => {
      const random = new SeededRandom(12345);
      for (let i = 0; i < 100; i++) {
        const value = random.next();
        expect(value).toBeGreaterThanOrEqual(0);
        expect(value).toBeLessThan(1);
      }
    });

    it('应该能获取当前种子状态', () => {
      const random = new SeededRandom(12345);
      const seed = random.getSeed();
      expect(seed).toBeGreaterThan(0);
    });
  });

  describe('确定性测试', () => {
    it('相同种子应该产生相同的随机序列', () => {
      const random1 = new SeededRandom(12345);
      const random2 = new SeededRandom(12345);

      const sequence1: number[] = [];
      const sequence2: number[] = [];

      for (let i = 0; i < 10; i++) {
        sequence1.push(random1.next());
        sequence2.push(random2.next());
      }

      expect(sequence1).toEqual(sequence2);
    });

    it('不同种子应该产生不同的随机序列', () => {
      const random1 = new SeededRandom(12345);
      const random2 = new SeededRandom(54321);

      const sequence1: number[] = [];
      const sequence2: number[] = [];

      for (let i = 0; i < 10; i++) {
        sequence1.push(random1.next());
        sequence2.push(random2.next());
      }

      expect(sequence1).not.toEqual(sequence2);
    });

    it('重置种子应该重新开始序列', () => {
      const random = new SeededRandom(12345);
      const first = random.next();

      random.reset(12345);
      const second = random.next();

      expect(first).toBe(second);
    });
  });

  describe('nextInt', () => {
    it('应该生成指定范围内的整数', () => {
      const random = new SeededRandom(12345);
      for (let i = 0; i < 100; i++) {
        const value = random.nextInt(0, 10);
        expect(value).toBeGreaterThanOrEqual(0);
        expect(value).toBeLessThanOrEqual(10);
        expect(Number.isInteger(value)).toBe(true);
      }
    });

    it('min和max相等时应该返回该值', () => {
      const random = new SeededRandom(12345);
      for (let i = 0; i < 10; i++) {
        expect(random.nextInt(5, 5)).toBe(5);
      }
    });

    it('应该能生成负数范围', () => {
      const random = new SeededRandom(12345);
      const value = random.nextInt(-10, -5);
      expect(value).toBeGreaterThanOrEqual(-10);
      expect(value).toBeLessThanOrEqual(-5);
    });

    it('相同种子应该产生相同的整数序列', () => {
      const random1 = new SeededRandom(12345);
      const random2 = new SeededRandom(12345);

      const sequence1: number[] = [];
      const sequence2: number[] = [];

      for (let i = 0; i < 10; i++) {
        sequence1.push(random1.nextInt(0, 100));
        sequence2.push(random2.nextInt(0, 100));
      }

      expect(sequence1).toEqual(sequence2);
    });
  });

  describe('nextFloat', () => {
    it('应该生成指定范围内的浮点数', () => {
      const random = new SeededRandom(12345);
      for (let i = 0; i < 100; i++) {
        const value = random.nextFloat(0, 10);
        expect(value).toBeGreaterThanOrEqual(0);
        expect(value).toBeLessThan(10);
      }
    });

    it('应该能生成小数', () => {
      const random = new SeededRandom(12345);
      let hasDecimal = false;

      for (let i = 0; i < 10; i++) {
        const value = random.nextFloat(0, 1);
        if (value !== Math.floor(value)) {
          hasDecimal = true;
          break;
        }
      }

      expect(hasDecimal).toBe(true);
    });
  });

  describe('choice', () => {
    it('应该从数组中随机选择元素', () => {
      const random = new SeededRandom(12345);
      const array = ['a', 'b', 'c', 'd', 'e'];

      for (let i = 0; i < 20; i++) {
        const choice = random.choice(array);
        expect(array).toContain(choice);
      }
    });

    it('单元素数组应该返回该元素', () => {
      const random = new SeededRandom(12345);
      const array = ['only'];

      for (let i = 0; i < 10; i++) {
        expect(random.choice(array)).toBe('only');
      }
    });

    it('空数组应该抛出错误', () => {
      const random = new SeededRandom(12345);
      expect(() => random.choice([])).toThrow('Cannot choose from empty array');
    });

    it('相同种子应该产生相同的选择序列', () => {
      const array = [1, 2, 3, 4, 5];
      const random1 = new SeededRandom(12345);
      const random2 = new SeededRandom(12345);

      const choices1: number[] = [];
      const choices2: number[] = [];

      for (let i = 0; i < 10; i++) {
        choices1.push(random1.choice(array));
        choices2.push(random2.choice(array));
      }

      expect(choices1).toEqual(choices2);
    });
  });

  describe('sample', () => {
    it('应该从数组中选择n个不重复元素', () => {
      const random = new SeededRandom(12345);
      const array = [1, 2, 3, 4, 5];
      const sample = random.sample(array, 3);

      expect(sample).toHaveLength(3);
      expect(new Set(sample).size).toBe(3);
      sample.forEach(item => {
        expect(array).toContain(item);
      });
    });

    it('sample大小等于数组长度时应该返回所有元素', () => {
      const random = new SeededRandom(12345);
      const array = [1, 2, 3, 4, 5];
      const sample = random.sample(array, 5);

      expect(sample).toHaveLength(5);
      expect(new Set(sample).size).toBe(5);
    });

    it('sample大小超出数组长度应该抛出错误', () => {
      const random = new SeededRandom(12345);
      const array = [1, 2, 3];

      expect(() => random.sample(array, 5)).toThrow(
        'Sample size cannot exceed array length'
      );
    });

    it('相同种子应该产生相同的sample结果', () => {
      const array = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
      const random1 = new SeededRandom(12345);
      const random2 = new SeededRandom(12345);

      const sample1 = random1.sample(array, 5);
      const sample2 = random2.sample(array, 5);

      expect(sample1).toEqual(sample2);
    });
  });

  describe('shuffle', () => {
    it('应该打乱数组顺序', () => {
      const random = new SeededRandom(12345);
      const array = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
      const shuffled = random.shuffle(array);

      expect(shuffled).toHaveLength(array.length);
      expect(shuffled.sort()).toEqual(array.sort());
    });

    it('不应该修改原数组', () => {
      const random = new SeededRandom(12345);
      const array = [1, 2, 3, 4, 5];
      const original = [...array];

      random.shuffle(array);

      expect(array).toEqual(original);
    });

    it('相同种子应该产生相同的打乱结果', () => {
      const array = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
      const random1 = new SeededRandom(12345);
      const random2 = new SeededRandom(12345);

      const shuffled1 = random1.shuffle(array);
      const shuffled2 = random2.shuffle(array);

      expect(shuffled1).toEqual(shuffled2);
    });

    it('单元素数组应该返回相同的数组', () => {
      const random = new SeededRandom(12345);
      const array = [1];
      const shuffled = random.shuffle(array);

      expect(shuffled).toEqual([1]);
    });
  });

  describe('boolean', () => {
    it('默认概率应该生成true或false', () => {
      const random = new SeededRandom(12345);
      const results = new Set<boolean>();

      for (let i = 0; i < 100; i++) {
        results.add(random.boolean());
      }

      expect(results.has(true) || results.has(false)).toBe(true);
    });

    it('概率为1时应该总是返回true', () => {
      const random = new SeededRandom(12345);
      for (let i = 0; i < 10; i++) {
        expect(random.boolean(1.0)).toBe(true);
      }
    });

    it('概率为0时应该总是返回false', () => {
      const random = new SeededRandom(12345);
      for (let i = 0; i < 10; i++) {
        expect(random.boolean(0.0)).toBe(false);
      }
    });

    it('相同种子应该产生相同的布尔序列', () => {
      const random1 = new SeededRandom(12345);
      const random2 = new SeededRandom(12345);

      const sequence1: boolean[] = [];
      const sequence2: boolean[] = [];

      for (let i = 0; i < 10; i++) {
        sequence1.push(random1.boolean());
        sequence2.push(random2.boolean());
      }

      expect(sequence1).toEqual(sequence2);
    });
  });

  describe('游戏场景测试', () => {
    it('应该能用于每日挑战的可重现随机', () => {
      const seed = 20250104; // 日期作为种子
      const random1 = new SeededRandom(seed);
      const random2 = new SeededRandom(seed);

      // 模拟生成关卡数据
      const level1 = {
        obstacles: random1.nextInt(5, 10),
        targetScore: random1.nextInt(1000, 5000),
        colors: random1.sample([1, 2, 3, 4], 3),
      };

      const level2 = {
        obstacles: random2.nextInt(5, 10),
        targetScore: random2.nextInt(1000, 5000),
        colors: random2.sample([1, 2, 3, 4], 3),
      };

      expect(level1).toEqual(level2);
    });

    it('应该能用于方块袋子的洗牌', () => {
      const shapes = ['I', 'O', 'T', 'L', 'J', 'S', 'Z'];
      const random1 = new SeededRandom(12345);
      const random2 = new SeededRandom(12345);

      const bag1 = random1.shuffle(shapes);
      const bag2 = random2.shuffle(shapes);

      expect(bag1).toEqual(bag2);
    });
  });
});
