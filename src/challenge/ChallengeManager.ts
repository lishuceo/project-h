/**
 * 每日挑战管理器（单例）
 * 负责：
 * - 生成今日挑战
 * - 保存/加载记录
 * - 排行榜管理
 */

import { LevelGenerator } from './LevelGenerator';
import { DailyChallengeData, ChallengeResult, ChallengeRecord } from '../types/challenge';

export class ChallengeManager {
  private static instance: ChallengeManager;
  private levelGenerator: LevelGenerator;
  
  private constructor() {
    this.levelGenerator = new LevelGenerator();
  }
  
  /**
   * 获取单例实例
   */
  public static getInstance(): ChallengeManager {
    if (!ChallengeManager.instance) {
      ChallengeManager.instance = new ChallengeManager();
    }
    return ChallengeManager.instance;
  }
  
  /**
   * 获取今日挑战
   * 核心方法：基于UTC日期生成全球统一的关卡
   * @param forceRefresh 强制刷新（开发模式用）
   */
  public getTodayChallenge(forceRefresh: boolean = false): DailyChallengeData {
    const today = this.getTodayDate();
    console.log(`📅 获取今日挑战: ${today}`);
    
    // 检查缓存（除非强制刷新）
    if (!forceRefresh) {
      const cached = this.loadCachedChallenge(today);
      if (cached) {
        console.log('✅ 从缓存加载今日挑战');
        return cached;
      }
    } else {
      console.log('🔄 强制刷新今日挑战');
    }
    
    // 生成新挑战
    const seed = this.dateToSeed(today);
    const challenge = this.levelGenerator.generate(seed, today);
    
    // 缓存挑战数据
    this.cacheChallenge(challenge);
    
    return challenge;
  }
  
  /**
   * 清除今日挑战缓存（开发用）
   */
  public clearTodayCache(): void {
    const today = this.getTodayDate();
    const key = `challenge_${today}`;
    localStorage.removeItem(key);
    console.log(`🗑️ 已清除今日挑战缓存: ${key}`);
  }
  
  /**
   * 日期转种子（保证全球统一）
   * '2025-10-13' -> 20251013
   */
  private dateToSeed(date: string): number {
    return parseInt(date.replace(/-/g, ''));
  }
  
  /**
   * 获取今日日期（UTC时间，保证全球统一）
   * 返回格式：'2025-10-13'
   */
  private getTodayDate(): string {
    const now = new Date();
    // 使用UTC时间，避免时区差异
    const utc = new Date(now.getTime() + now.getTimezoneOffset() * 60000);
    return utc.toISOString().split('T')[0];
  }
  
  /**
   * 保存挑战结果
   */
  public saveResult(result: ChallengeResult): void {
    const today = this.getTodayDate();
    console.log(`💾 保存挑战结果 [日期: ${today}]`);
    
    const records = this.loadRecords();
    const existing = records.find(r => r.date === today);
    
    if (existing) {
      // 更新现有记录
      existing.attempts++;
      
      // 只保存最佳成绩
      if (result.completed) {
        existing.completed = true;
        
        if (result.timeUsed < existing.bestTime || existing.bestTime === 0) {
          existing.bestTime = result.timeUsed;
          console.log(`🎉 新的最佳时间: ${result.timeUsed.toFixed(2)}秒`);
        }
        
        if (result.stepsUsed < existing.bestSteps || existing.bestSteps === 0) {
          existing.bestSteps = result.stepsUsed;
          console.log(`🎉 新的最少步数: ${result.stepsUsed}步`);
        }
        
        if (result.score > existing.bestScore) {
          existing.bestScore = result.score;
          console.log(`🎉 新的最高分数: ${result.score}分`);
        }
        
        if (result.stars > existing.bestStars) {
          existing.bestStars = result.stars;
          console.log(`🎉 新的最高星级: ${result.stars}星`);
        }
        
        existing.timestamp = new Date().toISOString();
      }
    } else {
      // 新记录
      const newRecord: ChallengeRecord = {
        date: today,
        bestTime: result.completed ? result.timeUsed : 0,
        bestSteps: result.completed ? result.stepsUsed : 0,
        bestScore: result.completed ? result.score : 0,
        bestStars: result.completed ? result.stars : 1,
        attempts: 1,
        completed: result.completed,
        timestamp: new Date().toISOString()
      };
      records.push(newRecord);
      console.log('✨ 创建新的挑战记录');
    }
    
    // 保存到本地存储
    this.saveRecords(records);
    
    // TODO: 如果完成，上传到服务器排行榜
    if (result.completed) {
      this.uploadToLeaderboard(result).catch(err => {
        console.warn('⚠️ 上传排行榜失败:', err);
      });
    }
  }
  
  /**
   * 获取今日记录
   */
  public getTodayRecord(): ChallengeRecord | null {
    const today = this.getTodayDate();
    const records = this.loadRecords();
    return records.find(r => r.date === today) || null;
  }
  
  /**
   * 获取历史记录
   * @param limit 返回最近N条记录（默认30天）
   */
  public getRecords(limit: number = 30): ChallengeRecord[] {
    const records = this.loadRecords();
    
    // 按日期降序排序
    records.sort((a, b) => b.date.localeCompare(a.date));
    
    return records.slice(0, limit);
  }
  
  /**
   * 获取统计信息
   */
  public getStats(): {
    totalAttempts: number;
    totalCompleted: number;
    completionRate: number;
    averageStars: number;
    bestTime: number;
    bestSteps: number;
  } {
    const records = this.loadRecords();
    
    const completed = records.filter(r => r.completed);
    const totalAttempts = records.reduce((sum, r) => sum + r.attempts, 0);
    const totalCompleted = completed.length;
    const completionRate = records.length > 0 ? (totalCompleted / records.length) * 100 : 0;
    
    const averageStars = completed.length > 0
      ? completed.reduce((sum, r) => sum + r.bestStars, 0) / completed.length
      : 0;
    
    const bestTime = completed.length > 0
      ? Math.min(...completed.map(r => r.bestTime))
      : 0;
    
    const bestSteps = completed.length > 0
      ? Math.min(...completed.map(r => r.bestSteps))
      : 0;
    
    return {
      totalAttempts,
      totalCompleted,
      completionRate,
      averageStars,
      bestTime,
      bestSteps
    };
  }
  
  /**
   * 清除所有记录（慎用）
   */
  public clearAllRecords(): void {
    localStorage.removeItem('dailyChallengeRecords');
    console.log('🗑️ 已清除所有挑战记录');
  }
  
  // ===== 私有方法 =====
  
  /**
   * 从本地存储加载记录
   */
  private loadRecords(): ChallengeRecord[] {
    try {
      const data = localStorage.getItem('dailyChallengeRecords');
      if (data) {
        return JSON.parse(data);
      }
    } catch (error) {
      console.error('❌ 加载记录失败:', error);
    }
    return [];
  }
  
  /**
   * 保存记录到本地存储
   */
  private saveRecords(records: ChallengeRecord[]): void {
    try {
      localStorage.setItem('dailyChallengeRecords', JSON.stringify(records));
      console.log('✅ 记录已保存');
    } catch (error) {
      console.error('❌ 保存记录失败:', error);
    }
  }
  
  /**
   * 缓存挑战数据
   */
  private cacheChallenge(challenge: DailyChallengeData): void {
    try {
      const key = `challenge_${challenge.date}`;
      localStorage.setItem(key, JSON.stringify(challenge));
      console.log(`✅ 挑战数据已缓存 [${key}]`);
    } catch (error) {
      console.error('❌ 缓存挑战失败:', error);
    }
  }
  
  /**
   * 从缓存加载挑战数据
   */
  private loadCachedChallenge(date: string): DailyChallengeData | null {
    try {
      const key = `challenge_${date}`;
      const data = localStorage.getItem(key);
      if (data) {
        return JSON.parse(data);
      }
    } catch (error) {
      console.error('❌ 加载缓存失败:', error);
    }
    return null;
  }
  
  /**
   * 上传成绩到排行榜（需要SDK）
   */
  private async uploadToLeaderboard(_result: ChallengeResult): Promise<void> {
    // TODO: 集成SDK后实现
    console.log('📤 准备上传成绩到排行榜...');
    console.log('⚠️ SDK未集成，跳过上传');
    
    // 示例代码：
    // const today = this.getTodayDate();
    // await SceSDKManager.submitScore({
    //   boardName: `daily_${today}`,
    //   score: result.score,
    //   metadata: {
    //     time: result.timeUsed,
    //     steps: result.stepsUsed,
    //     stars: result.stars,
    //     checksum: result.checksum
    //   }
    // });
  }
  
  /**
   * 获取今日排行榜（需要SDK）
   */
  public async getTodayLeaderboard(): Promise<any[]> {
    // TODO: 集成SDK后实现
    console.log('📥 准备获取排行榜...');
    console.log('⚠️ SDK未集成，返回空数组');
    return [];
    
    // 示例代码：
    // const today = this.getTodayDate();
    // return await SceSDKManager.getLeaderboard(`daily_${today}`);
  }
}

