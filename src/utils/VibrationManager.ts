/**
 * 震动反馈管理器
 * 提供不同强度和时长的震动反馈，提升游戏手感
 */

export class VibrationManager {
  private static instance: VibrationManager;
  private isSupported: boolean = false;

  private constructor() {
    // 检查设备是否支持震动
    this.isSupported = 'vibrate' in navigator;

    if (!this.isSupported) {
      console.log('⚠️ 设备不支持震动API');
    } else {
      console.log('✅ 震动API已启用');
    }
  }

  /**
   * 获取单例实例
   */
  public static getInstance(): VibrationManager {
    if (!VibrationManager.instance) {
      VibrationManager.instance = new VibrationManager();
    }
    return VibrationManager.instance;
  }

  /**
   * 执行震动
   * @param pattern 震动模式（毫秒）或震动模式数组 [震动, 暂停, 震动, ...]
   */
  private vibrate(pattern: number | number[]): void {
    if (!this.isSupported) {
      return;
    }

    try {
      navigator.vibrate(pattern);
    } catch (error) {
      console.warn('震动执行失败:', error);
    }
  }

  /**
   * 极短震动 - 拾取方块时
   * 时长: 10ms
   */
  public vibratePickup(): void {
    this.vibrate(10);
    console.log('🎮 震动: 拾取方块');
  }

  /**
   * 极短震动 - 放下方块时
   * 时长: 15ms
   */
  public vibrateDrop(): void {
    this.vibrate(15);
    console.log('🎮 震动: 放下方块');
  }

  /**
   * 短震动 - 消除成功
   * 时长: 30ms
   */
  public vibrateElimination(): void {
    this.vibrate(30);
    console.log('🎮 震动: 消除成功');
  }

  /**
   * 中等震动 - 连消成功
   * 模式: 震40ms, 停20ms, 震40ms
   */
  public vibrateCombo(): void {
    this.vibrate([40, 20, 40]);
    console.log('🎮 震动: 连消成功');
  }

  /**
   * 长震动 - 游戏结束
   * 模式: 震50ms, 停30ms, 震50ms, 停30ms, 震80ms
   */
  public vibrateGameOver(): void {
    this.vibrate([50, 30, 50, 30, 80]);
    console.log('🎮 震动: 游戏结束');
  }

  /**
   * 成功震动 - 挑战完成
   * 模式: 震30ms, 停20ms, 震30ms, 停20ms, 震60ms
   */
  public vibrateSuccess(): void {
    this.vibrate([30, 20, 30, 20, 60]);
    console.log('🎮 震动: 挑战完成');
  }

  /**
   * 停止所有震动
   */
  public stop(): void {
    if (this.isSupported) {
      navigator.vibrate(0);
    }
  }

  /**
   * 检查是否支持震动
   */
  public isVibrationSupported(): boolean {
    return this.isSupported;
  }
}

// 导出单例
export const vibrationManager = VibrationManager.getInstance();
