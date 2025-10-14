/**
 * 每日挑战场景
 * 继承自GameScene，添加挑战相关逻辑
 */

import { GameScene } from './GameScene';
import { ChallengeManager } from '../challenge/ChallengeManager';
import { ChallengeTimer } from '../challenge/Timer';
import { DailyChallengeData, ChallengeResult, PixelBlockData } from '../types/challenge';
import { GameState, PixelBlock, TetrominoData } from '../types';
import { LevelGenerator } from '../challenge/LevelGenerator';
import { PreviewSlots } from '../gameplay/PreviewSlots';

export class DailyChallengeScene extends GameScene {
  // 挑战相关
  private challengeManager!: ChallengeManager;
  private challengeData!: DailyChallengeData;
  private timer!: ChallengeTimer;
  private stepCount: number = 0;
  private levelGenerator!: LevelGenerator;
  
  // 性能优化：缓存像素块总数
  private cachedPixelCount: number = 0;
  private lastCountUpdateTime: number = 0;
  
  // 挑战UI
  private timerText!: Phaser.GameObjects.Text;
  private stepsText!: Phaser.GameObjects.Text;
  private progressText!: Phaser.GameObjects.Text;
  
  // 遮罩层（用于完成/失败界面）
  private overlayGraphics: Phaser.GameObjects.Graphics | null = null;
  private completionUI: Phaser.GameObjects.Container | null = null;
  
  constructor() {
    super('DailyChallengeScene');
  }
  
  /**
   * 重写create方法
   */
  create(): void {
    console.log('🎮 每日挑战场景启动');
    
    // 重置状态
    this.stepCount = 0;
    this.overlayGraphics = null;
    this.completionUI = null;
    this.cachedPixelCount = 0;
    this.lastCountUpdateTime = 0;
    
    // 初始化挑战管理器
    this.challengeManager = ChallengeManager.getInstance();
    this.levelGenerator = new LevelGenerator();
    
    // 获取今日挑战
    this.challengeData = this.challengeManager.getTodayChallenge();
    console.log(`📅 今日挑战: ${this.challengeData.date}`);
    console.log(`🎲 种子: ${this.challengeData.seed}`);
    console.log(`⭐ 难度: ${this.challengeData.difficulty}`);
    console.log(`🔐 校验和: ${this.challengeData.checksum}`);
    
    // 初始化计时器
    this.timer = new ChallengeTimer();
    
    // 调用父类初始化（核心系统）
    super.create();
    
    console.log('✅ 父类初始化完成');
    
    // 🎯 关键修复：用种子化的PreviewSlots替换父类创建的随机PreviewSlots
    // 这样每次重启都会得到相同的方块序列，确保公平竞技！
    // 🎯 重要：传入关卡使用的颜色，确保玩家方块只使用关卡中存在的颜色，避免无解！
    this.previewSlots = new PreviewSlots(
      this.challengeData.seed, 
      this.challengeData.availableColors
    );
    console.log('🎯 已启用种子化方块系统，确保公平竞技');
    console.log('🎨 玩家可用颜色:', this.challengeData.availableColors);
    
    // 🐛 关键修复：替换PreviewSlots后，必须重新更新UI显示！
    // 否则UI显示的是旧的随机方块，但实际使用的是新的种子方块
    this.updatePreviewSlotsUI();
    console.log('✅ 预览槽位UI已更新，显示种子化方块');
    
    // 隐藏父类的UI元素（避免与挑战UI重叠）
    this.hideParentUI();
    
    // 加载初始布局
    this.loadInitialLayout(this.challengeData.initialLayout);
    
    // 创建挑战UI（覆盖部分父类UI）
    this.createChallengeUI();
    
    console.log('✅ 每日挑战场景初始化完成');
  }
  
  /**
   * 隐藏父类的UI元素
   */
  private hideParentUI(): void {
    // 隐藏父类的分数、状态、连锁文本
    if (this.scoreText) {
      this.scoreText.setVisible(false);
    }
    if (this.stateText) {
      this.stateText.setVisible(false);
    }
    if (this.chainText) {
      this.chainText.setVisible(false);
    }
  }
  
  /**
   * 加载预设的初始布局
   */
  private loadInitialLayout(layout: PixelBlockData[]): void {
    console.log(`📦 加载初始布局：${layout.length} 个像素块`);
    
    let loaded = 0;
    const colorStats = new Map<number, number>();
    
    layout.forEach((data, index) => {
      // 调试前5个像素块
      if (index < 5) {
        console.log(`像素块${index}:`, { x: data.x, y: data.y, color: data.color.toString(16) });
      }
      
      const pixel: PixelBlock = {
        x: data.x,
        y: data.y,
        color: data.color,
        isStable: true,      // 初始布局都是稳定的
        groupId: -1          // 特殊标记：初始布局
      };
      
      this.grid.setPixel(data.x, data.y, pixel);
      loaded++;
      
      // 统计颜色
      colorStats.set(data.color, (colorStats.get(data.color) || 0) + 1);
    });
    
    console.log(`✅ 成功加载 ${loaded} 个像素块`);
    console.log('🎨 颜色统计:', Array.from(colorStats.entries()).map(([color, count]) => {
      let colorName = '未知';
      if (color === 0xff0000) colorName = '红色';
      if (color === 0x0000ff) colorName = '蓝色';
      if (color === 0x00ff00) colorName = '绿色';
      if (color === 0xffff00) colorName = '黄色';
      if (color === 0xff00ff) colorName = '紫色';
      if (color === 0xffffff) colorName = '白色';
      return `${colorName}(${count}个)`;
    }));
    
    // 统计逻辑格子数
    const logicalCells = Math.ceil(loaded / 100);
    console.log(`📊 约 ${logicalCells} 个逻辑格子`);
  }
  
  /**
   * 创建挑战UI
   */
  private createChallengeUI(): void {
    const uiY = 20;
    const lineHeight = 30;
    
    // 日期和难度在一行
    const stars = '⭐'.repeat(this.challengeData.difficulty);
    this.add.text(20, uiY, `📅 ${this.challengeData.date}  难度: ${stars}`, {
      fontSize: '18px',
      color: '#ffffff',
      fontFamily: 'Arial'
    });
    
    // 计时器
    this.timerText = this.add.text(20, uiY + lineHeight, '⏱️ 00:00', {
      fontSize: '22px',
      color: '#4CAF50',
      fontFamily: 'Arial',
      fontStyle: 'bold'
    });
    
    // 步数
    const maxSteps = this.challengeData.maxSteps || '∞';
    this.stepsText = this.add.text(180, uiY + lineHeight, `🚶 步数: 0 / ${maxSteps}`, {
      fontSize: '18px',
      color: '#2196F3',
      fontFamily: 'Arial'
    });
    
    // 进度（剩余像素块）
    this.progressText = this.add.text(420, uiY + lineHeight, '📦 剩余: ???', {
      fontSize: '18px',
      color: '#FF9800',
      fontFamily: 'Arial'
    });
    
    // 返回按钮（移到屏幕底部）
    const backButton = this.add.text(20, 1220, '← 返回菜单', {
      fontSize: '18px',
      color: '#ffffff',
      fontFamily: 'Arial',
      backgroundColor: '#f44336',
      padding: { x: 10, y: 5 }
    });
    backButton.setInteractive({ useHandCursor: true });
    backButton.on('pointerdown', () => {
      this.returnToMenu();
    });
    
    // 重新开始按钮
    const restartButton = this.add.text(200, 1220, '🔄 重新开始', {
      fontSize: '18px',
      color: '#ffffff',
      fontFamily: 'Arial',
      backgroundColor: '#2196F3',
      padding: { x: 10, y: 5 }
    });
    restartButton.setInteractive({ useHandCursor: true });
    restartButton.on('pointerdown', () => {
      this.restartChallenge();
    });
    
    // 初始化进度显示
    this.updateProgress();
    
    // 开发模式快捷键
    this.setupDevKeys();
  }
  
  /**
   * 设置开发模式快捷键
   */
  private setupDevKeys(): void {
    // C键：清除缓存并刷新关卡
    this.input.keyboard?.on('keydown-C', () => {
      console.log('🔄 [开发模式] 清除缓存并刷新关卡');
      this.challengeManager.clearTodayCache();
      this.scene.restart();
    });
    
    console.log('💡 开发快捷键: C键 = 清除缓存并刷新');
  }
  
  /**
   * 重写placeTetromino，添加步数统计
   */
  protected placeTetromino(
    tetromino: TetrominoData,
    logicalX: number,
    logicalY: number
  ): void {
    // 第一步开始计时
    if (this.stepCount === 0) {
      this.timer.start();
      console.log('⏱️ 开始计时！');
    }
    
    // 步数+1
    this.stepCount++;
    console.log(`🚶 第 ${this.stepCount} 步`);
    
    // 调用父类逻辑
    super.placeTetromino(tetromino, logicalX, logicalY);
  }
  
  /**
   * 重写消除检查，添加胜利条件判定
   */
  protected checkGameOver(): void {
    // 在每次消除结算后检查胜利条件
    this.checkVictoryOrFailureOptimized();
    
    // 如果没有胜利/失败，调用父类的游戏结束检查
    if (!this.overlayGraphics) {
      super.checkGameOver();
    }
  }
  
  /**
   * 重写update方法
   */
  update(): void {
    super.update();
    
    // 更新挑战UI
    if (this.timer && this.timerText) {
      this.timerText.setText(`⏱️ ${this.timer.formatTime()}`);
    }
    
    if (this.stepsText) {
      const maxSteps = this.challengeData.maxSteps || '∞';
      this.stepsText.setText(`🚶 步数: ${this.stepCount} / ${maxSteps}`);
      
      // 步数接近限制时变红
      if (this.challengeData.maxSteps && this.stepCount >= this.challengeData.maxSteps * 0.8) {
        this.stepsText.setColor('#f44336');
      }
    }
    
    // 【性能优化】只在IDLE状态且至少500ms更新一次进度
    if (this.stateManager.state === GameState.IDLE) {
      const now = Date.now();
      if (now - this.lastCountUpdateTime > 500) {
        this.updateProgress();
        this.lastCountUpdateTime = now;
      }
    }
  }
  
  /**
   * 优化的胜利/失败检查（使用缓存的像素块计数）
   */
  private checkVictoryOrFailureOptimized(): void {
    // 防止重复检查
    if (this.overlayGraphics) return;
    
    console.log('🔍 检查胜利/失败条件（优化版）...');
    
    // 【性能优化】使用getAllPixels而不是重新遍历
    const allPixels = this.grid.getAllPixels();
    const remainingPixels = allPixels.length;
    console.log(`📦 剩余像素块: ${remainingPixels}`);
    
    // 更新缓存
    this.cachedPixelCount = remainingPixels;
    
    // 胜利条件：清空所有像素块
    if (remainingPixels === 0) {
      console.log('🎉 挑战完成！所有像素块已清除！');
      this.timer.stop();
      this.onChallengeCompleted();
      return;
    }
    
    // 失败条件1：达到步数限制且未完成
    if (this.challengeData.maxSteps && this.stepCount >= this.challengeData.maxSteps) {
      console.log('❌ 挑战失败：步数用尽');
      this.timer.stop();
      this.onChallengeFailed('步数用尽');
      return;
    }
    
    // 失败条件2：无法继续放置
    const canPlace = this.previewSlots.hasAnyPlaceableBlock((tetromino) => {
      return this.canPlaceAnywhere(tetromino);
    });
    
    if (!canPlace) {
      console.log('❌ 挑战失败：无法继续放置方块');
      this.timer.stop();
      this.onChallengeFailed('无法继续放置');
      return;
    }
  }
  
  /**
   * 挑战完成
   */
  private onChallengeCompleted(): void {
    const result: ChallengeResult = {
      completed: true,
      timeUsed: this.timer.getElapsedTime(),
      stepsUsed: this.stepCount,
      score: this.calculateChallengeScore(),
      stars: this.calculateStars(),
      checksum: this.challengeData.checksum
    };
    
    console.log('🏆 挑战结果:', result);
    
    // 保存记录
    this.challengeManager.saveResult(result);
    
    // 显示完成界面
    this.showCompletionScreen(result);
  }
  
  /**
   * 挑战失败
   */
  private onChallengeFailed(reason: string): void {
    const result: ChallengeResult = {
      completed: false,
      timeUsed: this.timer.getElapsedTime(),
      stepsUsed: this.stepCount,
      score: 0,
      stars: 1,
      checksum: this.challengeData.checksum
    };
    
    // 保存尝试记录
    this.challengeManager.saveResult(result);
    
    // 显示失败界面
    this.showFailureScreen(reason);
  }
  
  /**
   * 计算挑战评分
   */
  private calculateChallengeScore(): number {
    const time = this.timer.getElapsedTime();
    const steps = this.stepCount;
    
    // 时间分：越快越高（最多5000分）
    const timeScore = Math.max(0, 5000 - time * 10);
    
    // 步数分：越少越高（最多5000分）
    const stepsScore = Math.max(0, 5000 - steps * 50);
    
    return Math.floor(timeScore + stepsScore);
  }
  
  /**
   * 计算星级
   */
  private calculateStars(): 1 | 2 | 3 {
    const time = this.timer.getElapsedTime();
    const steps = this.stepCount;
    
    const thresholds = this.levelGenerator.getStarThresholds(this.challengeData.difficulty);
    
    if (time <= thresholds.time3star && steps <= thresholds.steps3star) {
      return 3;
    } else if (time <= thresholds.time2star && steps <= thresholds.steps2star) {
      return 2;
    } else {
      return 1;
    }
  }
  
  /**
   * 更新进度显示（使用缓存优化性能）
   */
  private updateProgress(): void {
    if (!this.progressText) return;
    
    // 【性能优化】缓存像素块总数，避免每帧遍历
    const now = Date.now();
    if (now - this.lastCountUpdateTime < 500) {
      // 使用缓存值
      const logicalCells = Math.ceil(this.cachedPixelCount / 100);
      this.progressText.setText(`📦 剩余: ${logicalCells} 格`);
      return;
    }
    
    // 更新缓存
    this.cachedPixelCount = this.grid.getTotalPixelCount();
    this.lastCountUpdateTime = now;
    
    const logicalCells = Math.ceil(this.cachedPixelCount / 100);
    this.progressText.setText(`📦 剩余: ${logicalCells} 格`);
    
    // 接近完成时变绿
    if (this.cachedPixelCount > 0 && this.cachedPixelCount < 500) {
      this.progressText.setColor('#4CAF50');
    }
  }
  
  /**
   * 显示完成界面
   */
  private showCompletionScreen(result: ChallengeResult): void {
    // 创建遮罩
    this.overlayGraphics = this.add.graphics();
    this.overlayGraphics.fillStyle(0x000000, 0.85);
    this.overlayGraphics.fillRect(0, 0, this.cameras.main.width, this.cameras.main.height);
    this.overlayGraphics.setDepth(1000);
    
    // 创建UI容器
    this.completionUI = this.add.container(this.cameras.main.centerX, this.cameras.main.centerY);
    this.completionUI.setDepth(1001);
    
    // 标题
    const title = this.add.text(0, -200, '🎉 挑战完成！', {
      fontSize: '48px',
      color: '#ffdd00',
      fontFamily: 'Arial',
      fontStyle: 'bold'
    });
    title.setOrigin(0.5);
    this.completionUI.add(title);
    
    // 星级
    const starsText = '⭐'.repeat(result.stars);
    const stars = this.add.text(0, -130, starsText, {
      fontSize: '64px'
    });
    stars.setOrigin(0.5);
    this.completionUI.add(stars);
    
    // 统计信息
    const stats = this.add.text(0, -30, 
      `⏱️ 用时: ${this.timer.formatTime()}\n` +
      `🚶 步数: ${result.stepsUsed}\n` +
      `🏆 得分: ${result.score}`, 
      {
        fontSize: '28px',
        color: '#ffffff',
        align: 'center',
        lineSpacing: 15,
        fontFamily: 'Arial'
      }
    );
    stats.setOrigin(0.5);
    this.completionUI.add(stats);
    
    // 按钮
    this.createButton(this.completionUI, -120, 120, '🔄 再来一次', () => {
      this.restartChallenge();
    });
    
    this.createButton(this.completionUI, 120, 120, '📊 查看记录', () => {
      this.returnToMenu();
    });
  }
  
  /**
   * 显示失败界面
   */
  private showFailureScreen(reason: string): void {
    // 创建遮罩
    this.overlayGraphics = this.add.graphics();
    this.overlayGraphics.fillStyle(0x000000, 0.85);
    this.overlayGraphics.fillRect(0, 0, this.cameras.main.width, this.cameras.main.height);
    this.overlayGraphics.setDepth(1000);
    
    // 创建UI容器
    this.completionUI = this.add.container(this.cameras.main.centerX, this.cameras.main.centerY);
    this.completionUI.setDepth(1001);
    
    // 标题
    const title = this.add.text(0, -150, '💔 挑战失败', {
      fontSize: '48px',
      color: '#f44336',
      fontFamily: 'Arial',
      fontStyle: 'bold'
    });
    title.setOrigin(0.5);
    this.completionUI.add(title);
    
    // 失败原因
    const reasonText = this.add.text(0, -80, reason, {
      fontSize: '24px',
      color: '#ff9800',
      fontFamily: 'Arial'
    });
    reasonText.setOrigin(0.5);
    this.completionUI.add(reasonText);
    
    // 统计信息
    const stats = this.add.text(0, 0, 
      `⏱️ 用时: ${this.timer.formatTime()}\n` +
      `🚶 步数: ${this.stepCount}`, 
      {
        fontSize: '24px',
        color: '#ffffff',
        align: 'center',
        lineSpacing: 15,
        fontFamily: 'Arial'
      }
    );
    stats.setOrigin(0.5);
    this.completionUI.add(stats);
    
    // 按钮
    this.createButton(this.completionUI, -120, 100, '🔄 再来一次', () => {
      this.restartChallenge();
    });
    
    this.createButton(this.completionUI, 120, 100, '← 返回菜单', () => {
      this.returnToMenu();
    });
  }
  
  /**
   * 创建按钮
   */
  private createButton(
    container: Phaser.GameObjects.Container,
    x: number,
    y: number,
    text: string,
    callback: () => void
  ): void {
    const bg = this.add.rectangle(x, y, 200, 50, 0x4CAF50);
    bg.setInteractive({ useHandCursor: true });
    bg.on('pointerdown', callback);
    bg.on('pointerover', () => bg.setFillStyle(0x66BB6A));
    bg.on('pointerout', () => bg.setFillStyle(0x4CAF50));
    
    const label = this.add.text(x, y, text, {
      fontSize: '20px',
      color: '#ffffff',
      fontFamily: 'Arial'
    });
    label.setOrigin(0.5);
    
    container.add(bg);
    container.add(label);
  }
  
  /**
   * 重新开始挑战
   */
  private restartChallenge(): void {
    console.log('🔄 重新开始挑战');
    
    // 清理当前场景的UI
    if (this.overlayGraphics) {
      this.overlayGraphics.destroy();
      this.overlayGraphics = null;
    }
    
    if (this.completionUI) {
      this.completionUI.destroy();
      this.completionUI = null;
    }
    
    // 重启场景
    this.scene.restart();
  }
  
  /**
   * 返回菜单
   */
  private returnToMenu(): void {
    console.log('← 返回菜单');
    this.scene.start('StartScene');
  }
}
