/**
 * 每日挑战管理器（单例）
 * 负责：
 * - 生成今日3个挑战（简单、中等、困难）
 * - 保存/加载记录
 * - 解锁机制管理
 * - 排行榜管理
 */

import { LevelGenerator } from './LevelGenerator';
import { DailyChallengeData, ChallengeResult, ChallengeRecord, DailyRecord } from '../types/challenge';

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
   * 获取今日指定的挑战
   * @param challengeId 挑战ID（1=简单，2=中等，3=困难）
   * @param forceRefresh 强制刷新（开发模式用）
   */
  public getTodayChallenge(challengeId: 1 | 2 | 3 = 1, forceRefresh: boolean = false): DailyChallengeData {
    const today = this.getTodayDate();
    console.log(`📅 获取今日挑战 ${challengeId}: ${today}`);

    // 检查缓存（除非强制刷新）
    if (!forceRefresh) {
      const cached = this.loadCachedChallenge(today, challengeId);
      if (cached) {
        console.log(`✅ 从缓存加载今日挑战 ${challengeId}`);
        return cached;
      }
    } else {
      console.log(`🔄 强制刷新今日挑战 ${challengeId}`);
    }

    // 生成新挑战（种子包含日期和挑战ID）
    const seed = this.dateToSeed(today) + challengeId * 1000;
    const challenge = this.levelGenerator.generate(seed, today, challengeId);

    // 缓存挑战数据
    this.cacheChallenge(challenge);

    return challenge;
  }

  /**
   * 获取今日所有3个挑战
   */
  public getTodayChallenges(): DailyChallengeData[] {
    return [
      this.getTodayChallenge(1),
      this.getTodayChallenge(2),
      this.getTodayChallenge(3)
    ];
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
    console.log(`💾 保存挑战结果 [日期: ${today}, 挑战: ${result.challengeId}]`);

    const dailyRecord = this.loadDailyRecord(today);

    // 获取或创建该挑战的记录
    let challengeRecord = dailyRecord.challenges[result.challengeId];

    if (challengeRecord) {
      // 更新现有记录
      challengeRecord.attempts++;

      // 只保存最佳成绩
      if (result.completed) {
        challengeRecord.completed = true;

        if (result.timeUsed < challengeRecord.bestTime || challengeRecord.bestTime === 0) {
          challengeRecord.bestTime = result.timeUsed;
          console.log(`🎉 新的最佳时间: ${result.timeUsed.toFixed(2)}秒`);
        }

        if (result.stepsUsed < challengeRecord.bestSteps || challengeRecord.bestSteps === 0) {
          challengeRecord.bestSteps = result.stepsUsed;
          console.log(`🎉 新的最少步数: ${result.stepsUsed}步`);
        }

        if (result.score > challengeRecord.bestScore) {
          challengeRecord.bestScore = result.score;
          console.log(`🎉 新的最高分数: ${result.score}分`);
        }

        if (result.stars > challengeRecord.bestStars) {
          challengeRecord.bestStars = result.stars;
          console.log(`🎉 新的最高星级: ${result.stars}星`);
        }

        challengeRecord.timestamp = new Date().toISOString();
      }
    } else {
      // 新记录
      challengeRecord = {
        date: today,
        challengeId: result.challengeId,
        bestTime: result.completed ? result.timeUsed : 0,
        bestSteps: result.completed ? result.stepsUsed : 0,
        bestScore: result.completed ? result.score : 0,
        bestStars: result.completed ? result.stars : 1,
        attempts: 1,
        completed: result.completed,
        timestamp: new Date().toISOString()
      };
      dailyRecord.challenges[result.challengeId] = challengeRecord;
      console.log(`✨ 创建新的挑战${result.challengeId}记录`);
    }

    // 保存到本地存储
    this.saveDailyRecord(dailyRecord);

    // 如果完成，上传到服务器排行榜
    if (result.completed) {
      this.uploadToLeaderboard(result).catch(err => {
        console.warn('⚠️ 上传排行榜失败:', err);
      });
    }
  }
  
  /**
   * 检查挑战是否已解锁
   * 解锁规则：
   * - 挑战1：总是解锁
   * - 挑战2：完成挑战1后解锁
   * - 挑战3：完成挑战2后解锁
   */
  public isChallengeUnlocked(challengeId: 1 | 2 | 3): boolean {
    if (challengeId === 1) {
      return true; // 挑战1总是解锁
    }

    const today = this.getTodayDate();
    const dailyRecord = this.loadDailyRecord(today);

    if (challengeId === 2) {
      // 挑战2需要完成挑战1
      return dailyRecord.challenges[1]?.completed || false;
    }

    if (challengeId === 3) {
      // 挑战3需要完成挑战2
      return dailyRecord.challenges[2]?.completed || false;
    }

    return false;
  }

  /**
   * 获取今日指定挑战的记录
   */
  public getTodayRecord(challengeId: 1 | 2 | 3): ChallengeRecord | null {
    const today = this.getTodayDate();
    const dailyRecord = this.loadDailyRecord(today);
    return dailyRecord.challenges[challengeId] || null;
  }

  /**
   * 获取今日所有挑战的记录
   */
  public getTodayAllRecords(): DailyRecord {
    const today = this.getTodayDate();
    return this.loadDailyRecord(today);
  }
  
  /**
   * 获取历史记录（所有天的所有挑战）
   * @param limit 返回最近N天的记录（默认30天）
   */
  public getRecords(limit: number = 30): DailyRecord[] {
    const allRecords = this.loadAllDailyRecords();

    // 按日期降序排序
    allRecords.sort((a, b) => b.date.localeCompare(a.date));

    return allRecords.slice(0, limit);
  }
  
  /**
   * 获取统计信息（所有挑战汇总）
   */
  public getStats(): {
    totalAttempts: number;
    totalCompleted: number;
    completionRate: number;
    averageStars: number;
    bestTime: number;
    bestSteps: number;
  } {
    const allRecords = this.loadAllDailyRecords();

    // 收集所有挑战记录
    const allChallenges: ChallengeRecord[] = [];
    allRecords.forEach(daily => {
      Object.values(daily.challenges).forEach(challenge => {
        allChallenges.push(challenge);
      });
    });

    const completed = allChallenges.filter(r => r.completed);
    const totalAttempts = allChallenges.reduce((sum, r) => sum + r.attempts, 0);
    const totalCompleted = completed.length;
    const completionRate = allChallenges.length > 0 ? (totalCompleted / allChallenges.length) * 100 : 0;

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

  /**
   * 调试方法：显示所有localStorage中的挑战相关数据
   */
  public debugShowStorage(): void {
    console.log('====== 🔍 localStorage调试信息 ======');
    console.log(`📅 当前日期 (UTC): ${this.getTodayDate()}`);
    console.log('');

    console.log('📦 所有挑战缓存:');
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key && key.startsWith('challenge_')) {
        const data = localStorage.getItem(key);
        if (data) {
          console.log(`  ${key}:`, JSON.parse(data));
        }
      }
    }
    console.log('');

    console.log('📝 所有每日记录:');
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key && key.startsWith('daily_record_')) {
        const data = localStorage.getItem(key);
        if (data) {
          const record = JSON.parse(data);
          console.log(`  ${key}:`);
          console.log(`    挑战1完成: ${record.challenges[1]?.completed || false}`, record.challenges[1] || '无记录');
          console.log(`    挑战2完成: ${record.challenges[2]?.completed || false}`, record.challenges[2] || '无记录');
          console.log(`    挑战3完成: ${record.challenges[3]?.completed || false}`, record.challenges[3] || '无记录');
        }
      }
    }
    console.log('');

    console.log('🔓 当前解锁状态:');
    console.log(`  挑战1: ${this.isChallengeUnlocked(1)}`);
    console.log(`  挑战2: ${this.isChallengeUnlocked(2)}`);
    console.log(`  挑战3: ${this.isChallengeUnlocked(3)}`);
    console.log('=====================================');
  }
  
  // ===== 私有方法 =====

  /**
   * 加载指定日期的每日记录
   */
  private loadDailyRecord(date: string): DailyRecord {
    try {
      const key = `daily_record_${date}`;
      const data = localStorage.getItem(key);
      if (data) {
        return JSON.parse(data);
      }
    } catch (error) {
      console.error('❌ 加载每日记录失败:', error);
    }

    // 返回空记录
    return {
      date,
      challenges: {}
    };
  }

  /**
   * 保存每日记录到本地存储
   */
  private saveDailyRecord(record: DailyRecord): void {
    try {
      const key = `daily_record_${record.date}`;
      const jsonData = JSON.stringify(record);
      localStorage.setItem(key, jsonData);
      console.log(`✅ 每日记录已保存 [${record.date}]`);
      console.log(`📊 保存的数据:`, record);
      console.log(`🔑 保存的键名: ${key}`);

      // 立即验证保存是否成功
      const verification = localStorage.getItem(key);
      if (verification) {
        console.log(`✅ 验证成功: localStorage中存在该记录`);
        const parsed = JSON.parse(verification);
        console.log(`📋 验证的挑战完成状态:`, {
          挑战1: parsed.challenges[1]?.completed || false,
          挑战2: parsed.challenges[2]?.completed || false,
          挑战3: parsed.challenges[3]?.completed || false
        });
      } else {
        console.error(`❌ 验证失败: localStorage中不存在该记录！`);
      }
    } catch (error) {
      console.error('❌ 保存每日记录失败:', error);
    }
  }

  /**
   * 加载所有每日记录
   */
  private loadAllDailyRecords(): DailyRecord[] {
    const records: DailyRecord[] = [];

    try {
      // 遍历localStorage中所有以daily_record_开头的键
      for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        if (key && key.startsWith('daily_record_')) {
          const data = localStorage.getItem(key);
          if (data) {
            records.push(JSON.parse(data));
          }
        }
      }
    } catch (error) {
      console.error('❌ 加载所有记录失败:', error);
    }

    return records;
  }
  
  /**
   * 缓存挑战数据
   */
  private cacheChallenge(challenge: DailyChallengeData): void {
    try {
      const key = `challenge_${challenge.date}_${challenge.challengeId}`;
      localStorage.setItem(key, JSON.stringify(challenge));
      console.log(`✅ 挑战数据已缓存 [${key}]`);
    } catch (error) {
      console.error('❌ 缓存挑战失败:', error);
    }
  }

  /**
   * 从缓存加载挑战数据
   */
  private loadCachedChallenge(date: string, challengeId: 1 | 2 | 3): DailyChallengeData | null {
    try {
      const key = `challenge_${date}_${challengeId}`;
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
   * 每个挑战有独立的排行榜
   */
  private async uploadToLeaderboard(_result: ChallengeResult): Promise<void> {
    // TODO: 集成SDK后实现
    const today = this.getTodayDate();
    const boardName = `daily_challenge_${_result.challengeId}_${today}`;
    console.log(`📤 准备上传成绩到排行榜 [${boardName}]...`);
    console.log('⚠️ SDK未集成，跳过上传');

    // 示例代码：
    // await SceSDKManager.submitScore({
    //   boardName: `daily_challenge_${result.challengeId}`,
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
   * 获取指定挑战的排行榜（需要SDK）
   */
  public async getChallengeLeaderboard(challengeId: 1 | 2 | 3): Promise<any[]> {
    // TODO: 集成SDK后实现
    const today = this.getTodayDate();
    const boardName = `daily_challenge_${challengeId}_${today}`;
    console.log(`📥 准备获取排行榜 [${boardName}]...`);
    console.log('⚠️ SDK未集成，返回空数组');
    return [];

    // 示例代码：
    // const today = this.getTodayDate();
    // return await SceSDKManager.getLeaderboard(`daily_challenge_${challengeId}`);
  }
}

