/**
 * 每日挑战相关的类型定义
 */

import { Color } from './index';

/**
 * 像素块数据（用于序列化初始布局）
 */
export interface PixelBlockData {
  x: number;        // 0-119
  y: number;        // 0-219
  color: Color;     // 颜色枚举
}

/**
 * 每日挑战数据
 */
export interface DailyChallengeData {
  // 基础信息
  date: string;              // '2025-10-13' 日期标识（UTC）
  seed: number;              // 随机种子（基于日期生成）
  difficulty: 1 | 2 | 3;     // 难度等级
  checksum: string;          // 校验和，用于验证关卡一致性
  
  // 初始布局
  initialLayout: PixelBlockData[];  // 预设的像素块
  
  // 游戏限制
  maxSteps?: number;         // 最大步数限制（可选）
  timeLimit?: number;        // 时间限制（秒，可选）
  
  // 提供的方块池（未来扩展，可选）
  availableShapes?: string[];  // 限定可用的方块形状
}

/**
 * 挑战结果
 */
export interface ChallengeResult {
  completed: boolean;        // 是否完成
  timeUsed: number;          // 用时（秒）
  stepsUsed: number;         // 步数
  score: number;             // 综合评分
  stars: 1 | 2 | 3;          // 星级评价
  checksum: string;          // 关卡校验和
}

/**
 * 挑战记录（本地保存）
 */
export interface ChallengeRecord {
  date: string;              // 挑战日期
  bestTime: number;          // 最佳用时
  bestSteps: number;         // 最少步数
  bestScore: number;         // 最高分数
  bestStars: 1 | 2 | 3;      // 最高星级
  attempts: number;          // 尝试次数
  completed: boolean;        // 是否完成过
  timestamp: string;         // 最后完成时间
}

/**
 * 排行榜条目
 */
export interface LeaderboardEntry {
  rank: number;              // 排名
  playerName: string;        // 玩家名称
  playerId: string;          // 玩家ID
  time: number;              // 用时（秒）
  steps: number;             // 步数
  score: number;             // 分数
  stars: 1 | 2 | 3;          // 星级
  timestamp: string;         // 完成时间戳
}

/**
 * 星级阈值配置
 */
export interface StarThresholds {
  time3star: number;         // 3星时间要求（秒）
  time2star: number;         // 2星时间要求（秒）
  steps3star: number;        // 3星步数要求
  steps2star: number;        // 2星步数要求
}

