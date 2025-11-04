import { describe, it, expect, beforeEach } from 'vitest';
import { ScoringSystem } from '@/gameplay/Scoring';

describe('ScoringSystem', () => {
  let scoringSystem: ScoringSystem;

  beforeEach(() => {
    scoringSystem = new ScoringSystem();
  });

  describe('基础分数计算', () => {
    it('应该使用平方根公式计算基础分数', () => {
      expect(scoringSystem.calculateBaseScore(1)).toBe(10);
      expect(scoringSystem.calculateBaseScore(4)).toBe(20);
      expect(scoringSystem.calculateBaseScore(9)).toBe(30);
      expect(scoringSystem.calculateBaseScore(16)).toBe(40);
      expect(scoringSystem.calculateBaseScore(100)).toBe(100);
    });

    it('1056个格子应该约等于373分', () => {
      const score = scoringSystem.calculateBaseScore(1056);
      expect(score).toBeCloseTo(324, 0);
    });

    it('0个格子应该返回0分', () => {
      expect(scoringSystem.calculateBaseScore(0)).toBe(0);
    });

    it('大量格子应该返回合理的分数', () => {
      const score = scoringSystem.calculateBaseScore(10000);
      expect(score).toBeGreaterThan(0);
      expect(score).toBeLessThan(10000);
    });
  });

  describe('消除得分（无连锁）', () => {
    it('首次消除应该没有连锁加成', () => {
      const score = scoringSystem.addEliminationScore(100, false);
      const baseScore = scoringSystem.calculateBaseScore(100);

      expect(score).toBe(baseScore * 1);
      expect(scoringSystem.chainLevel).toBe(1);
    });

    it('应该正确累加总分', () => {
      scoringSystem.addEliminationScore(100, false);
      const firstTotal = scoringSystem.score;

      scoringSystem.addEliminationScore(100, false);
      const secondTotal = scoringSystem.score;

      expect(secondTotal).toBeGreaterThan(firstTotal);
      expect(secondTotal).toBe(firstTotal + scoringSystem.calculateBaseScore(100));
    });
  });

  describe('连锁系统', () => {
    it('连锁应该增加倍数', () => {
      const base = scoringSystem.calculateBaseScore(100);

      const score1 = scoringSystem.addEliminationScore(100, false);
      expect(score1).toBe(base * 1);
      expect(scoringSystem.chainLevel).toBe(1);

      const score2 = scoringSystem.addEliminationScore(100, true);
      expect(score2).toBe(base * 2);
      expect(scoringSystem.chainLevel).toBe(2);

      const score3 = scoringSystem.addEliminationScore(100, true);
      expect(score3).toBe(base * 3);
      expect(scoringSystem.chainLevel).toBe(3);
    });

    it('连锁应该能达到更高倍数', () => {
      for (let i = 1; i <= 10; i++) {
        scoringSystem.addEliminationScore(100, i === 1 ? false : true);
      }
      expect(scoringSystem.chainLevel).toBe(10);
    });

    it('非连锁消除应该重置连锁倍数为1', () => {
      scoringSystem.addEliminationScore(100, false);
      scoringSystem.addEliminationScore(100, true);
      scoringSystem.addEliminationScore(100, true);
      expect(scoringSystem.chainLevel).toBe(3);

      scoringSystem.addEliminationScore(100, false);
      expect(scoringSystem.chainLevel).toBe(1);
    });

    it('resetChain应该重置连锁倍数为0', () => {
      scoringSystem.addEliminationScore(100, false);
      scoringSystem.addEliminationScore(100, true);
      expect(scoringSystem.chainLevel).toBe(2);

      scoringSystem.resetChain();
      expect(scoringSystem.chainLevel).toBe(0);
    });
  });

  describe('得分查询', () => {
    it('初始分数应该为0', () => {
      expect(scoringSystem.score).toBe(0);
    });

    it('初始连锁倍数应该为0', () => {
      expect(scoringSystem.chainLevel).toBe(0);
    });

    it('应该正确返回当前总分', () => {
      scoringSystem.addEliminationScore(100, false);
      const expected = scoringSystem.calculateBaseScore(100);
      expect(scoringSystem.score).toBe(expected);
    });
  });

  describe('重置功能', () => {
    it('reset应该清零所有数据', () => {
      scoringSystem.addEliminationScore(100, false);
      scoringSystem.addEliminationScore(100, true);
      expect(scoringSystem.score).toBeGreaterThan(0);
      expect(scoringSystem.chainLevel).toBeGreaterThan(0);

      scoringSystem.reset();
      expect(scoringSystem.score).toBe(0);
      expect(scoringSystem.chainLevel).toBe(0);
    });

    it('重置后应该能正常计分', () => {
      scoringSystem.addEliminationScore(100, false);
      scoringSystem.reset();

      const score = scoringSystem.addEliminationScore(100, false);
      expect(score).toBe(scoringSystem.calculateBaseScore(100));
      expect(scoringSystem.score).toBe(score);
    });
  });

  describe('实际游戏场景', () => {
    it('模拟连续消除场景', () => {
      const scores: number[] = [];

      scores.push(scoringSystem.addEliminationScore(120, false));
      scores.push(scoringSystem.addEliminationScore(80, true));
      scores.push(scoringSystem.addEliminationScore(100, true));

      expect(scoringSystem.chainLevel).toBe(3);
      expect(scoringSystem.score).toBe(scores.reduce((a, b) => a + b, 0));
    });

    it('模拟连锁中断场景', () => {
      scoringSystem.addEliminationScore(100, false);
      scoringSystem.addEliminationScore(100, true);
      expect(scoringSystem.chainLevel).toBe(2);

      scoringSystem.resetChain();
      expect(scoringSystem.chainLevel).toBe(0);

      scoringSystem.addEliminationScore(100, false);
      expect(scoringSystem.chainLevel).toBe(1);
    });

    it('模拟大量消除的高分场景', () => {
      for (let i = 0; i < 5; i++) {
        scoringSystem.addEliminationScore(500, i === 0 ? false : true);
      }

      expect(scoringSystem.chainLevel).toBe(5);
      expect(scoringSystem.score).toBeGreaterThan(1000);
    });
  });
});
