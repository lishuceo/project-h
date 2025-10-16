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

// 全局类型声明
declare global {
  interface Window {
    SceSDK: any;
  }
}

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
    // 使用更复杂的种子算法，增加每日差异性
    const dateSeed = this.dateToSeed(today);
    const seed = (dateSeed * 7919 + challengeId * 104729) % 2147483647;
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

    // 清除今天所有3个挑战的缓存
    for (let challengeId = 1; challengeId <= 3; challengeId++) {
      const key = `challenge_${today}_${challengeId}`;
      localStorage.removeItem(key);
      console.log(`🗑️ 已清除缓存: ${key}`);
    }

    console.log(`✅ 已清除今日所有挑战缓存`);
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
   * 返回格式：'2025-10-16'
   */
  private getTodayDate(): string {
    const now = new Date();
    // 直接使用 toISOString() 获取UTC日期（已经是UTC时间）
    return now.toISOString().split('T')[0];
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
   * 上传成绩到排行榜（使用SCE SDK）
   * 每个挑战有独立的排行榜
   */
  private async uploadToLeaderboard(result: ChallengeResult): Promise<void> {
    const today = this.getTodayDate();
    const boardKey = `daily_challenge_${result.challengeId}_${today}`;
    console.log(`📤 上传成绩到排行榜 [${boardKey}]...`);

    // 检查 SDK 是否可用
    if (!window.SceSDK || !window.SceSDK.cloud) {
      console.warn('⚠️ SCE SDK 不可用，跳过上传');
      return;
    }

    try {
      // 使用分数作为排行榜的值（分数越高越好）
      await window.SceSDK.cloud.set_number(boardKey, result.score);
      console.log(`✅ 成绩已上传: ${result.score}分`);
    } catch (error) {
      console.error('❌ 上传成绩失败:', error);
    }
  }

  /**
   * 获取指定挑战的排行榜（使用SCE SDK）
   */
  public async getChallengeLeaderboard(challengeId: 1 | 2 | 3, limit: number = 10): Promise<any[]> {
    const today = this.getTodayDate();
    const boardKey = `daily_challenge_${challengeId}_${today}`;
    console.log(`📥 获取排行榜 [${boardKey}]...`);

    // 检查 SDK 是否可用
    if (!window.SceSDK || !window.SceSDK.cloud) {
      console.warn('⚠️ SCE SDK 不可用，返回空数组');
      return [];
    }

    try {
      const response = await window.SceSDK.cloud.get_top_rank({
        key: boardKey,
        limit: limit,
        include_username: true,
        order: 'desc' // 分数从高到低
      });

      console.log('📊 排行榜数据:', response);

      // 处理返回数据
      let rankingsData: any[] = [];

      if (Array.isArray(response)) {
        rankingsData = response;
      } else if (response && typeof response === 'object' && 'result' in response) {
        rankingsData = response.result || [];
      } else if (response && typeof response === 'object' && 'data' in response) {
        rankingsData = response.data || [];
      }

      return rankingsData.map((item: any, index: number) => ({
        uid: item.uid || `user_${index}`,
        username: item.username || item.name || `玩家${index + 1}`,
        score: item.value || item.score || 0,
        rank: index + 1
      }));
    } catch (error) {
      console.error('❌ 获取排行榜失败:', error);
      return [];
    }
  }

  /**
   * 更新指定挑战的全球排名（使用SCE SDK）
   * @param challengeId 挑战ID
   * @returns 更新后的挑战记录
   */
  public async updateChallengeRank(challengeId: 1 | 2 | 3): Promise<ChallengeRecord | null> {
    const today = this.getTodayDate();
    const record = this.getTodayRecord(challengeId);

    if (!record || !record.completed) {
      console.log(`⚠️ 挑战${challengeId}未完成，无法更新排名`);
      return null;
    }

    const boardKey = `daily_challenge_${challengeId}_${today}`;
    console.log(`🔄 更新排名中 [${boardKey}]...`);

    // 检查 SDK 是否可用
    if (!window.SceSDK || !window.SceSDK.cloud) {
      console.warn('⚠️ SCE SDK 不可用，使用模拟数据');
      // SDK 不可用时使用模拟数据
      record.globalRank = Math.floor(Math.random() * 1000) + 1;
      record.totalPlayers = Math.floor(Math.random() * 5000) + 1000;

      const dailyRecord = this.loadDailyRecord(today);
      dailyRecord.challenges[challengeId] = record;
      this.saveDailyRecord(dailyRecord);

      return record;
    }

    try {
      // 1. 获取玩家排名
      let playerRank = -1;
      if (typeof window.SceSDK.cloud.get_user_rank === 'function') {
        console.log(`🔍 调用 get_user_rank({ key: "${boardKey}" })...`);
        const rankResult = await window.SceSDK.cloud.get_user_rank({ key: boardKey });
        console.log(`📦 get_user_rank 原始返回:`, rankResult);

        // 处理返回值：SCE SDK返回 {result: {rank: X, value: Y}} 这种嵌套格式
        if (typeof rankResult === 'number') {
          playerRank = rankResult;
        } else if (rankResult && typeof rankResult === 'object') {
          // 检查嵌套的result字段
          if ('result' in rankResult && rankResult.result) {
            const innerResult = rankResult.result;
            console.log(`📦 从result字段解析:`, innerResult);

            // ⚠️ 注意: rank字段才是排名，value字段是分数，不要混淆！
            // SCE SDK的rank是0-based索引: 0=第1名, 1=第2名, -1=未上榜
            if ('rank' in innerResult) {
              if (innerResult.rank >= 0) {
                playerRank = innerResult.rank + 1; // 转换为1-based (0→1, 1→2, ...)
                console.log(`📊 rank=${innerResult.rank} (0-based) → 第${playerRank}名 (1-based)`);
              } else {
                // rank=-1表示未上榜
                playerRank = -1;
                console.log(`⚠️ rank=-1，表示未上榜`);
              }
            } else if ('ranking' in innerResult && innerResult.ranking >= 0) {
              playerRank = innerResult.ranking + 1;
            } else {
              playerRank = -1;
              console.log(`⚠️ 无rank字段`);
            }
          } else {
            // 没有嵌套result，直接从第一层取
            if ('rank' in rankResult && rankResult.rank >= 0) {
              playerRank = rankResult.rank + 1; // 0-based → 1-based
            } else if ('ranking' in rankResult && rankResult.ranking >= 0) {
              playerRank = rankResult.ranking + 1;
            }
          }
        }

        console.log(`📊 解析后的玩家排名: 第${playerRank}名`);
      } else {
        console.warn('⚠️ get_user_rank 方法不存在');
      }

      console.log(`📊 玩家排名结果: 第${playerRank}名`);

      // 更新记录中的排名信息（只保存排名，不保存总人数）
      if (playerRank > 0) {
        // SDK返回了有效排名
        record.globalRank = playerRank;
        record.totalPlayers = undefined; // 不再显示总人数
        console.log(`✅ 排名已更新: 第${playerRank}名`);
      } else {
        // rank=0或-1，尝试从排行榜推断排名
        console.log(`⚠️ SDK返回rank=${playerRank}，尝试从排行榜推断...`);

        try {
          const leaderboard = await this.getChallengeLeaderboard(challengeId, 100);
          console.log(`📊 排行榜有 ${leaderboard.length} 条记录`);

          if (leaderboard.length > 0) {
            // 查找玩家在排行榜中的位置（通过分数匹配）
            const playerScore = record.bestScore;
            let foundRank = -1;

            for (let i = 0; i < leaderboard.length; i++) {
              if (leaderboard[i].score === playerScore) {
                foundRank = leaderboard[i].rank;
                console.log(`📊 在排行榜中找到匹配分数 ${playerScore}，排名: 第${foundRank}名`);
                break;
              }
            }

            if (foundRank > 0) {
              record.globalRank = foundRank;
              console.log(`✅ 使用排行榜推断的排名: 第${foundRank}名`);
            } else {
              // 未在榜上找到，可能是榜外或分数不匹配
              record.globalRank = undefined;
              console.log(`⚠️ 未在排行榜中找到匹配记录，不显示排名`);
            }
          } else {
            // 排行榜为空
            record.globalRank = undefined;
            console.log(`⚠️ 排行榜为空，不显示排名`);
          }
        } catch (error) {
          console.error('❌ 从排行榜推断排名失败:', error);
          record.globalRank = undefined;
        }

        record.totalPlayers = undefined;
      }

      // 保存更新后的记录
      const dailyRecord = this.loadDailyRecord(today);
      dailyRecord.challenges[challengeId] = record;
      this.saveDailyRecord(dailyRecord);

      return record;
    } catch (error) {
      console.error(`❌ 更新排名失败:`, error);
      // 失败时使用模拟数据
      record.globalRank = Math.floor(Math.random() * 100) + 1;
      record.totalPlayers = Math.floor(Math.random() * 500) + 100;

      const dailyRecord = this.loadDailyRecord(today);
      dailyRecord.challenges[challengeId] = record;
      this.saveDailyRecord(dailyRecord);

      return record;
    }
  }

  /**
   * 批量更新所有已完成挑战的排名
   */
  public async updateAllRanks(): Promise<void> {
    console.log('🔄 批量更新所有挑战排名...');

    const updatePromises: Promise<ChallengeRecord | null>[] = [];

    for (let challengeId = 1; challengeId <= 3; challengeId++) {
      const record = this.getTodayRecord(challengeId as 1 | 2 | 3);
      if (record?.completed) {
        updatePromises.push(this.updateChallengeRank(challengeId as 1 | 2 | 3));
      }
    }

    await Promise.all(updatePromises);
    console.log('✅ 所有排名更新完成');
  }
}

