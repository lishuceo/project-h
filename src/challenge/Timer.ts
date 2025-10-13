/**
 * 挑战计时器
 * 支持开始、暂停、继续、重置功能
 */
export class ChallengeTimer {
  private startTime: number = 0;
  private pausedTime: number = 0;
  private totalPausedDuration: number = 0;
  private isPaused: boolean = false;
  private isRunning: boolean = false;
  
  /**
   * 开始计时
   */
  public start(): void {
    if (!this.isRunning) {
      this.startTime = Date.now();
      this.isRunning = true;
      this.isPaused = false;
      this.totalPausedDuration = 0;
      console.log('⏱️ 计时器启动');
    }
  }
  
  /**
   * 暂停计时
   */
  public pause(): void {
    if (this.isRunning && !this.isPaused) {
      this.pausedTime = Date.now();
      this.isPaused = true;
      console.log('⏸️ 计时器暂停');
    }
  }
  
  /**
   * 继续计时
   */
  public resume(): void {
    if (this.isRunning && this.isPaused) {
      const pauseDuration = Date.now() - this.pausedTime;
      this.totalPausedDuration += pauseDuration;
      this.isPaused = false;
      console.log('▶️ 计时器继续');
    }
  }
  
  /**
   * 停止计时（保留时间）
   */
  public stop(): void {
    if (this.isRunning) {
      this.pause();
      console.log('⏹️ 计时器停止');
    }
  }
  
  /**
   * 重置计时器
   */
  public reset(): void {
    this.startTime = 0;
    this.pausedTime = 0;
    this.totalPausedDuration = 0;
    this.isPaused = false;
    this.isRunning = false;
    console.log('🔄 计时器重置');
  }
  
  /**
   * 获取已用时间（秒，保留2位小数）
   */
  public getElapsedTime(): number {
    if (!this.isRunning) return 0;
    
    const now = this.isPaused ? this.pausedTime : Date.now();
    const elapsed = (now - this.startTime - this.totalPausedDuration) / 1000;
    return Math.max(0, elapsed);
  }
  
  /**
   * 获取已用时间（毫秒）
   */
  public getElapsedTimeMs(): number {
    if (!this.isRunning) return 0;
    
    const now = this.isPaused ? this.pausedTime : Date.now();
    return Math.max(0, now - this.startTime - this.totalPausedDuration);
  }
  
  /**
   * 格式化时间显示 MM:SS
   */
  public formatTime(): string {
    const seconds = Math.floor(this.getElapsedTime());
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }
  
  /**
   * 格式化时间显示（带毫秒） MM:SS.mmm
   */
  public formatTimeWithMs(): string {
    const totalMs = this.getElapsedTimeMs();
    const seconds = Math.floor(totalMs / 1000);
    const ms = totalMs % 1000;
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}.${Math.floor(ms / 100)}`;
  }
  
  /**
   * 获取状态
   */
  public getIsRunning(): boolean {
    return this.isRunning;
  }
  
  public getIsPaused(): boolean {
    return this.isPaused;
  }
}

