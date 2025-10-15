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
import { SCREEN_WIDTH, UI_COLORS } from '../config/constants';

export class DailyChallengeScene extends GameScene {
  // 挑战相关
  private challengeManager!: ChallengeManager;
  private challengeData!: DailyChallengeData;
  private timer!: ChallengeTimer;
  private stepCount: number = 0;
  private levelGenerator!: LevelGenerator;
  private currentChallengeId: 1 | 2 | 3 = 1; // 当前挑战ID

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
  create(data?: { challengeId?: 1 | 2 | 3 }): void {
    console.log('🎮 每日挑战场景启动');

    // 获取传入的挑战ID
    this.currentChallengeId = data?.challengeId || 1;
    console.log(`🎯 当前挑战ID: ${this.currentChallengeId}`);

    // 重置状态
    this.stepCount = 0;
    this.overlayGraphics = null;
    this.completionUI = null;
    this.cachedPixelCount = 0;
    this.lastCountUpdateTime = 0;

    // 初始化挑战管理器
    this.challengeManager = ChallengeManager.getInstance();
    this.levelGenerator = new LevelGenerator();

    // 获取指定的挑战
    this.challengeData = this.challengeManager.getTodayChallenge(this.currentChallengeId);
    console.log(`📅 挑战 ${this.currentChallengeId}: ${this.challengeData.date}`);
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
      if (color === 0xf87171) colorName = '霓虹红';
      if (color === 0x60a5fa) colorName = '霓虹蓝';
      if (color === 0x4ade80) colorName = '霓虹绿';
      if (color === 0xfbbf24) colorName = '霓虹黄';
      return `${colorName}(${count}个)`;
    }));
    
    // 统计逻辑格子数
    const logicalCells = Math.ceil(loaded / 100);
    console.log(`📊 约 ${logicalCells} 个逻辑格子`);
  }
  
  /**
   * 创建挑战UI（现代化设计）
   */
  private createChallengeUI(): void {
    // 创建头部容器（简洁扁平）
    const headerBg = this.add.graphics();

    // 柔和阴影
    headerBg.fillStyle(UI_COLORS.SHADOW_DEEP, 0.3);
    headerBg.fillRoundedRect(14, 14, SCREEN_WIDTH - 28, 100, 12);

    // 主背景（扁平纯色）
    headerBg.fillStyle(UI_COLORS.CARD_BG, 1);
    headerBg.fillRoundedRect(10, 10, SCREEN_WIDTH - 20, 100, 12);

    // 细边框
    headerBg.lineStyle(1, UI_COLORS.BORDER_GLOW, 0.5);
    headerBg.strokeRoundedRect(10, 10, SCREEN_WIDTH - 20, 100, 12);

    // 日期和难度（左侧）
    const dateContainer = this.add.container(30, 30);

    // 图标背景圆
    const dateIconBg = this.add.circle(0, 0, 18, UI_COLORS.ACCENT_PRIMARY, 0.2);
    const dateIcon = this.add.text(0, 0, '📅', { fontSize: '20px' });
    dateIcon.setOrigin(0.5);

    const stars = '⭐'.repeat(this.challengeData.difficulty);
    const dateText = this.add.text(30, 0, `${this.challengeData.date}`, {
      fontSize: '16px',
      color: '#e2e8f0',
      fontFamily: 'Arial, sans-serif',
      fontStyle: 'bold'
    });
    dateText.setOrigin(0, 0.5);

    const difficultyText = this.add.text(30, 20, `难度: ${stars}`, {
      fontSize: '14px',
      color: '#fbbf24',
      fontFamily: 'Arial, sans-serif'
    });
    difficultyText.setOrigin(0, 0.5);

    dateContainer.add([dateIconBg, dateIcon, dateText, difficultyText]);

    // 状态卡片行
    const statsY = 65;

    // 计时器卡片（绿色背景）
    this.createStatCard(30, statsY, 210, '⏱️', '00:00', 0x2d5a3d, 'timer');

    // 步数卡片（蓝色背景）
    const maxSteps = this.challengeData.maxSteps || '∞';
    this.createStatCard(255, statsY, 210, '🚶', `0/${maxSteps}`, 0x2d4a5a, 'steps');

    // 剩余卡片（橙色背景）
    this.createStatCard(480, statsY, 210, '📦', '剩余: 17格', 0x5a4a2d, 'progress');

    // 底部图标按钮
    this.createBottomChallengeButtons();

    // 初始化进度显示
    this.updateProgress();

    // 开发模式快捷键
    this.setupDevKeys();
  }

  /**
   * 创建现代状态卡片
   */
  private createStatCard(
    x: number,
    y: number,
    width: number,
    icon: string,
    value: string,
    accentColor: number,
    cardType: 'timer' | 'steps' | 'progress'
  ): Phaser.GameObjects.Container {
    const container = this.add.container(x, y);

    // 卡片背景（更鲜艳的色彩块）
    const bg = this.add.graphics();

    // 阴影
    bg.fillStyle(0x000000, 0.3);
    bg.fillRoundedRect(2, 2, width, 36, 8);

    // 主背景（实色，不透明）
    bg.fillStyle(accentColor, 1);
    bg.fillRoundedRect(0, 0, width, 36, 8);

    // 无边框或极细边框
    bg.lineStyle(1, 0x000000, 0.2);
    bg.strokeRoundedRect(0, 0, width, 36, 8);

    // 图标
    const iconText = this.add.text(12, 18, icon, {
      fontSize: '20px'
    });
    iconText.setOrigin(0, 0.5);

    // 数值文本
    const valueText = this.add.text(42, 18, value, {
      fontSize: '18px',
      color: '#f1f5f9',
      fontFamily: 'Arial, sans-serif',
      fontStyle: 'bold'
    });
    valueText.setOrigin(0, 0.5);

    container.add([bg, iconText, valueText]);

    // 保存引用以便更新
    if (cardType === 'timer') {
      this.timerText = valueText;
    } else if (cardType === 'steps') {
      this.stepsText = valueText;
    } else if (cardType === 'progress') {
      this.progressText = valueText;
    }

    return container;
  }
  
  /**
   * 创建底部挑战按钮（扁平简洁设计，靠两侧）
   */
  private createBottomChallengeButtons(): void {
    const buttonY = 1180; // 底部位置（调整为更靠下，距离底部约100px）
    const leftX = 80; // 左侧按钮位置
    const rightX = 720 - 80; // 右侧按钮位置（SCREEN_WIDTH - 80）

    // 返回按钮（左侧 - 仅图标）
    this.createIconOnlyButton(
      leftX,
      buttonY,
      '←',
      0x555555, // 灰色
      () => {
        this.returnToMenu();
      }
    );

    // 重新开始按钮（右侧 - 仅图标）
    this.createIconOnlyButton(
      rightX,
      buttonY,
      '↻',
      0x4a90e2, // 蓝色
      () => {
        this.restartChallenge();
      }
    );
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
      challengeId: this.currentChallengeId,
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

    // 验证保存
    const savedRecord = this.challengeManager.getTodayRecord(this.currentChallengeId);
    console.log('✅ 保存后验证 - 挑战记录:', savedRecord);
    console.log('✅ 保存后验证 - 挑战是否完成:', savedRecord?.completed);

    // 检查下一个挑战是否解锁
    if (this.currentChallengeId < 3) {
      const nextChallengeId = (this.currentChallengeId + 1) as 1 | 2 | 3;
      const nextUnlocked = this.challengeManager.isChallengeUnlocked(nextChallengeId);
      console.log(`🔓 挑战${nextChallengeId}解锁状态:`, nextUnlocked);
    }

    // 显示完成界面
    this.showCompletionScreen(result);
  }
  
  /**
   * 挑战失败
   */
  private onChallengeFailed(reason: string): void {
    const result: ChallengeResult = {
      challengeId: this.currentChallengeId,
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
   * 返回挑战选择
   */
  private returnToMenu(): void {
    console.log('← 返回挑战选择');
    this.scene.start('ChallengeSelectorScene');
  }
}
