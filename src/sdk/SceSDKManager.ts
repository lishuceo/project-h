import 'sce-game-sdk';

/**
 * SCE Game SDK 管理类
 * 封装 SDK 功能：排行榜、云存储等
 */

// 全局类型声明
declare global {
  interface Window {
    SceSDK: any;
  }
}

export interface RankingItem {
  uid: string;
  username: string;
  score: number;
  rank: number;
}

export class SceSDKManager {
  private isInitialized: boolean = false;
  private isDevelopment: boolean = true;

  constructor() {
    this.checkEnvironment();
  }

  /**
   * 检查运行环境
   */
  private checkEnvironment(): void {
    // 检查是否在星火平台内运行
    this.isDevelopment = !window.SceSDK || typeof window.SceSDK.init_for_dev === 'function';
  }

  /**
   * 初始化 SDK（开发环境需要）
   */
  async initialize(developerToken: string = 'db2c71a6-ed95-4fe7-a2ba-dfa2f6fd78b0'): Promise<void> {
    if (this.isInitialized) {
      console.log('SDK 已初始化');
      return;
    }

    try {
      if (this.isDevelopment && window.SceSDK && window.SceSDK.init_for_dev) {
        // 开发环境初始化
        if (developerToken) {
          await window.SceSDK.init_for_dev({
            sce_developer_token: developerToken
          });
          console.log('SDK 初始化成功（开发模式）');
        } else {
          console.warn('开发环境需要 developer token，跳过初始化');
          // 不抛出错误，允许在没有token的情况下继续运行
        }
      } else {
        console.log('SDK 初始化成功（生产环境）');
      }
      this.isInitialized = true;
    } catch (error) {
      console.error('SDK 初始化失败:', error);
      // 不抛出错误，允许游戏在SDK不可用时继续运行
    }
  }

  /**
   * 检查 SDK 是否可用
   */
  private isSDKAvailable(): boolean {
    if (!window.SceSDK || !window.SceSDK.cloud) {
      console.warn('SCE SDK 不可用，跳过操作');
      return false;
    }
    return true;
  }

  /**
   * 获取玩家最高分
   */
  async getHighestScore(): Promise<number> {
    if (!this.isSDKAvailable()) {
      // SDK 不可用，从 localStorage 读取
      const localScore = localStorage.getItem('highest_score');
      return localScore ? parseInt(localScore, 10) : 0;
    }

    try {
      const result = await window.SceSDK.cloud.get_number('highest_score');
      console.log('从云端获取最高分:', result);
      
      // 处理返回值：可能是数字、{error: ...} 或其他格式
      if (typeof result === 'number') {
        return result;
      } else if (result && typeof result === 'object' && 'error' in result) {
        // 有错误，使用本地存储
        const localScore = localStorage.getItem('highest_score');
        return localScore ? parseInt(localScore, 10) : 0;
      }
      
      return 0;
    } catch (error) {
      console.error('获取最高分失败:', error);
      // 失败时从 localStorage 读取
      const localScore = localStorage.getItem('highest_score');
      return localScore ? parseInt(localScore, 10) : 0;
    }
  }

  /**
   * 保存玩家最高分（直接保存，不做判断）
   * @param score 要保存的分数
   * @param previousHighest 之前的最高分（调用者提供，避免重复查询）
   */
  async saveHighestScore(score: number, previousHighest: number = -1): Promise<boolean> {
    // 先保存到 localStorage（本地备份）
    localStorage.setItem('highest_score', score.toString());

    if (!this.isSDKAvailable()) {
      console.log('SDK 不可用，分数已保存到本地:', score);
      return true;
    }

    try {
      // 如果调用者提供了 previousHighest，则检查；否则直接保存
      if (previousHighest >= 0 && score <= previousHighest) {
        console.log(`分数 ${score} 未超过已知最高分 ${previousHighest}，跳过云端保存`);
        return false;
      }

      await window.SceSDK.cloud.set_number('highest_score', score);
      console.log('最高分已保存到云端:', score);
      return true;
    } catch (error) {
      console.error('保存最高分到云端失败:', error);
      return false;
    }
  }

  /**
   * 获取排行榜数据
   */
  async getRankings(limit: number = 10): Promise<RankingItem[]> {
    if (!this.isSDKAvailable()) {
      console.warn('SDK 不可用，返回空排行榜');
      return [];
    }

    try {
      const response = await window.SceSDK.cloud.get_top_rank({
        key: 'highest_score',
        limit: limit,
        include_username: true,
        order: 'desc'
      });

      console.log('获取排行榜数据:', response);

      // 处理返回数据：可能是 {result: []} 或直接是数组
      let rankingsData: any[] = [];
      
      if (Array.isArray(response)) {
        rankingsData = response;
      } else if (response && typeof response === 'object' && 'result' in response) {
        rankingsData = response.result || [];
      } else if (response && typeof response === 'object' && 'data' in response) {
        rankingsData = response.data || [];
      }

      // 转换为标准格式
      return rankingsData.map((item: any, index: number) => ({
        uid: item.uid || `user_${index}`,
        username: item.username || item.name || `玩家${index + 1}`,
        score: item.value || item.score || 0,
        rank: index + 1
      }));
    } catch (error) {
      console.error('获取排行榜失败:', error);
      return [];
    }
  }

  /**
   * 获取玩家在排行榜中的排名
   */
  async getPlayerRank(): Promise<number> {
    if (!this.isSDKAvailable()) {
      return -1;
    }

    try {
      // 检查API是否存在
      if (typeof window.SceSDK.cloud.get_user_rank !== 'function') {
        console.warn('get_user_rank API 不可用');
        return -1;
      }

      // 使用对象参数格式（与 get_top_rank 一致）
      const rankResult = await window.SceSDK.cloud.get_user_rank({ key: 'highest_score' });

      // 处理返回值：可能是数字或对象
      let rank = -1;
      if (typeof rankResult === 'number') {
        rank = rankResult;
      } else if (rankResult && typeof rankResult === 'object') {
        rank = rankResult.rank || rankResult.value || -1;
      }

      console.log('玩家当前排名:', rank);
      return rank > 0 ? rank : -1;
    } catch (error) {
      console.warn('获取排名失败:', error);
      return -1;
    }
  }

  /**
   * 上传当前游戏分数（游戏结束时调用）
   */
  async uploadScore(score: number): Promise<{
    isNewRecord: boolean;
    previousHighest: number;
    currentHighest: number;
  }> {
    const previousHighest = await this.getHighestScore();
    console.log(`上传分数检查：当前 ${score}，历史最高 ${previousHighest}`);
    
    const isNewRecord = score > previousHighest;

    if (isNewRecord) {
      // 传入 previousHighest 避免重复查询
      await this.saveHighestScore(score, previousHighest);
    } else {
      console.log(`分数 ${score} 未超过历史最高分 ${previousHighest}，不上传`);
    }

    return {
      isNewRecord,
      previousHighest,
      currentHighest: isNewRecord ? score : previousHighest
    };
  }
}

// 导出单例
export const sceSDKManager = new SceSDKManager();

