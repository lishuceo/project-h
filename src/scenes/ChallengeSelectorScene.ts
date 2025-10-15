/**
 * 挑战选择场景
 * 显示今日3个挑战（简单、中等、困难），支持解锁机制
 */

import Phaser from 'phaser';
import { ChallengeManager } from '@/challenge/ChallengeManager';
import { DailyChallengeData } from '@/types/challenge';
import { SCREEN_WIDTH, SCREEN_HEIGHT, UI_COLORS } from '@/config/constants';
import { BackButton } from '@/ui/BackButton';

export class ChallengeSelectorScene extends Phaser.Scene {
  private challengeManager!: ChallengeManager;
  private challenges: DailyChallengeData[] = [];

  constructor() {
    super({ key: 'ChallengeSelectorScene' });
  }

  create(): void {
    console.log('🎮 挑战选择场景启动');

    // 初始化挑战管理器
    this.challengeManager = ChallengeManager.getInstance();

    // 获取今日3个挑战
    this.challenges = this.challengeManager.getTodayChallenges();
    console.log('📅 今日挑战已加载:', this.challenges.length);

    // 调试信息：显示当前日期和解锁状态
    const today = this.challenges[0].date;
    console.log('📅 当前日期 (UTC):', today);
    console.log('🔓 解锁状态:');
    console.log('  挑战1:', this.challengeManager.isChallengeUnlocked(1));
    console.log('  挑战2:', this.challengeManager.isChallengeUnlocked(2), '- 需要完成挑战1');
    console.log('  挑战3:', this.challengeManager.isChallengeUnlocked(3), '- 需要完成挑战2');

    // 显示完成记录
    const allRecords = this.challengeManager.getTodayAllRecords();
    console.log('📊 今日完成记录:', allRecords);

    // 创建UI
    this.createUI();

    // 淡入效果
    this.cameras.main.fadeIn(500);
  }

  /**
   * 创建UI
   */
  private createUI(): void {
    // 创建渐变背景
    this.createGradientBackground();

    // 标题
    const title = this.add.text(SCREEN_WIDTH / 2, 120, '每日挑战', {
      fontSize: '48px',
      color: '#ffffff',
      fontFamily: 'Arial',
      fontStyle: 'bold',
      stroke: '#1e3a5f',
      strokeThickness: 4
    });
    title.setOrigin(0.5);

    // 日期显示
    const today = this.challenges[0].date;
    const dateText = this.add.text(SCREEN_WIDTH / 2, 180, `${today}`, {
      fontSize: '20px',
      color: '#e0f2fe',
      fontFamily: 'Arial'
    });
    dateText.setOrigin(0.5);

    // 创建3个挑战卡片
    const cardY = 350; // 从350开始，避免和日期文本重叠
    const cardSpacing = 290;

    this.challenges.forEach((challenge, index) => {
      this.createChallengeCard(
        SCREEN_WIDTH / 2,
        cardY + index * cardSpacing,
        challenge,
        index + 1 as 1 | 2 | 3
      );
    });

    // 返回按钮
    this.createBackButton();
  }

  /**
   * 创建挑战卡片
   */
  private createChallengeCard(
    x: number,
    y: number,
    challenge: DailyChallengeData,
    challengeId: 1 | 2 | 3
  ): void {
    const container = this.add.container(x, y);
    const cardWidth = 660;
    const cardHeight = 240;
    const cornerRadius = 16;

    // 设置container的边界，确保内容不被裁剪
    container.setSize(cardWidth, cardHeight);

    // 检查是否解锁
    const isUnlocked = this.challengeManager.isChallengeUnlocked(challengeId);
    const record = this.challengeManager.getTodayRecord(challengeId);

    // 卡片背景
    const bg = this.add.graphics();

    // 阴影
    bg.fillStyle(0x000000, 0.3);
    bg.fillRoundedRect(-cardWidth / 2 + 4, -cardHeight / 2 + 4, cardWidth, cardHeight, cornerRadius);

    // 主背景（根据状态改变颜色）
    let bgColor = UI_COLORS.CARD_BG;
    if (!isUnlocked) {
      bgColor = 0x3a3a3a; // 未解锁：灰色
    } else if (record?.completed) {
      bgColor = 0x2d5a3d; // 已完成：绿色
    }

    bg.fillStyle(bgColor, 1);
    bg.fillRoundedRect(-cardWidth / 2, -cardHeight / 2, cardWidth, cardHeight, cornerRadius);

    // 边框
    bg.lineStyle(2, isUnlocked ? UI_COLORS.BORDER_GLOW : 0x555555, 0.5);
    bg.strokeRoundedRect(-cardWidth / 2, -cardHeight / 2, cardWidth, cardHeight, cornerRadius);

    container.add(bg);

    // 难度标识
    const difficultyLabels = ['简单', '中等', '困难'];
    const difficultyColors = [0x4ade80, 0xfbbf24, 0xf87171]; // 绿、黄、红
    const stars = '⭐'.repeat(challenge.difficulty);

    const difficultyBadge = this.add.container(-cardWidth / 2 + 40, -cardHeight / 2 + 40);
    const badgeBg = this.add.circle(0, 0, 30, difficultyColors[challengeId - 1]);
    const badgeText = this.add.text(0, 0, `${challengeId}`, {
      fontSize: '24px',
      color: '#ffffff',
      fontFamily: 'Arial',
      fontStyle: 'bold'
    });
    badgeText.setOrigin(0.5);
    difficultyBadge.add([badgeBg, badgeText]);
    container.add(difficultyBadge);

    // 标题
    const titleText = this.add.text(-cardWidth / 2 + 100, -cardHeight / 2 + 30,
      `挑战${challengeId}：${difficultyLabels[challengeId - 1]}`, {
      fontSize: '28px',
      color: isUnlocked ? '#ffffff' : '#888888',
      fontFamily: 'Arial',
      fontStyle: 'bold'
    });
    titleText.setOrigin(0, 0.5);
    container.add(titleText);

    // 星级
    const starsText = this.add.text(-cardWidth / 2 + 100, -cardHeight / 2 + 70, stars, {
      fontSize: '20px'
    });
    starsText.setOrigin(0, 0.5);
    container.add(starsText);

    // 状态信息
    if (!isUnlocked) {
      // 未解锁：显示大锁图标
      // 注意：emoji 的实际渲染高度可能大于 fontSize，使用 setOrigin(0.5, 0) 从顶部对齐
      const lockIcon = this.add.text(0, -30, '🔒', {
        fontSize: '36px',
        padding: { top: 10, bottom: 10, left: 0, right: 0 }  // 添加上下 padding
      });
      lockIcon.setOrigin(0.5, 0);  // 从顶部对齐，避免顶部被切
      container.add(lockIcon);

      const unlockText = this.add.text(0, 30, `完成挑战${challengeId - 1}后解锁`, {
        fontSize: '18px',
        color: '#888888',
        fontFamily: 'Arial'
      });
      unlockText.setOrigin(0.5);
      container.add(unlockText);
    } else {
      // 已解锁：显示状态文本
      let statusText = '';
      let statusColor = '#94a3b8';

      if (record?.completed) {
        statusText = `✅ 已完成 | 最佳: ${record.bestScore}分 ${record.bestStars}星`;
        statusColor = '#4ade80';
      } else if (record && !record.completed) {
        statusText = `已尝试 ${record.attempts} 次`;
        statusColor = '#fbbf24';
      } else {
        statusText = '等待挑战';
      }

      const status = this.add.text(0, -20, statusText, {
        fontSize: '18px',
        color: statusColor,
        fontFamily: 'Arial'
      });
      status.setOrigin(0.5);
      container.add(status);

      // 关卡信息（只在已解锁时显示）
      const infoText = this.add.text(0, 15,
        `目标: 清除所有方块 | 步数限制: ${challenge.maxSteps}`, {
        fontSize: '16px',
        color: '#94a3b8',
        fontFamily: 'Arial'
      });
      infoText.setOrigin(0.5);
      container.add(infoText);
    }

    // 开始按钮
    if (isUnlocked) {
      const isCompleted = record?.completed || false;
      const button = this.createStartButton(0, cardHeight / 2 - 50, challengeId, isCompleted);
      container.add(button);
    }
  }

  /**
   * 创建开始按钮
   */
  private createStartButton(x: number, y: number, challengeId: 1 | 2 | 3, isCompleted: boolean): Phaser.GameObjects.Container {
    const container = this.add.container(x, y);
    const buttonWidth = 200;
    const buttonHeight = 60;
    const cornerRadius = 10;

    // 按钮背景（已完成的挑战使用灰蓝色，未完成的使用亮绿色）
    const bg = this.add.graphics();
    const bgColor = isCompleted ? 0x64748b : 0x4ade80; // 灰蓝色 vs 绿色
    const bgAlpha = isCompleted ? 0.6 : 1.0; // 已完成的半透明

    bg.fillStyle(bgColor, bgAlpha);
    bg.fillRoundedRect(-buttonWidth / 2, -buttonHeight / 2, buttonWidth, buttonHeight, cornerRadius);
    bg.lineStyle(2, isCompleted ? 0x475569 : 0x000000, 0.3);
    bg.strokeRoundedRect(-buttonWidth / 2, -buttonHeight / 2, buttonWidth, buttonHeight, cornerRadius);
    bg.setName('bg');

    // 按钮文本（根据是否已完成显示不同文字和样式）
    const buttonText = isCompleted ? '再次挑战' : '开始挑战';
    const text = this.add.text(0, 0, buttonText, {
      fontSize: isCompleted ? '20px' : '24px', // 已完成的字号更小
      color: isCompleted ? '#e2e8f0' : '#ffffff', // 已完成的颜色更浅
      fontFamily: 'Arial',
      fontStyle: isCompleted ? 'normal' : 'bold' // 已完成的不加粗
    });
    text.setOrigin(0.5);
    text.setName('text');

    container.add([bg, text]);
    container.setSize(buttonWidth, buttonHeight);
    container.setInteractive({ useHandCursor: true });

    // 悬停效果（根据是否已完成有不同的悬停样式）
    container.on('pointerover', () => {
      this.tweens.add({
        targets: container,
        scaleX: 1.05,
        scaleY: 1.05,
        duration: 100
      });

      const bgGraphics = container.getByName('bg') as Phaser.GameObjects.Graphics;
      if (bgGraphics) {
        bgGraphics.clear();
        if (isCompleted) {
          // 已完成：悬停时略微变亮
          bgGraphics.fillStyle(0x748096, 0.8);
        } else {
          // 未完成：悬停时更亮的绿色
          bgGraphics.fillStyle(0x66bb6a, 1);
        }
        bgGraphics.fillRoundedRect(-buttonWidth / 2, -buttonHeight / 2, buttonWidth, buttonHeight, cornerRadius);
        bgGraphics.lineStyle(2, isCompleted ? 0x475569 : 0x000000, 0.3);
        bgGraphics.strokeRoundedRect(-buttonWidth / 2, -buttonHeight / 2, buttonWidth, buttonHeight, cornerRadius);
      }
    });

    container.on('pointerout', () => {
      this.tweens.add({
        targets: container,
        scaleX: 1.0,
        scaleY: 1.0,
        duration: 100
      });

      const bgGraphics = container.getByName('bg') as Phaser.GameObjects.Graphics;
      if (bgGraphics) {
        bgGraphics.clear();
        bgGraphics.fillStyle(bgColor, bgAlpha);
        bgGraphics.fillRoundedRect(-buttonWidth / 2, -buttonHeight / 2, buttonWidth, buttonHeight, cornerRadius);
        bgGraphics.lineStyle(2, isCompleted ? 0x475569 : 0x000000, 0.3);
        bgGraphics.strokeRoundedRect(-buttonWidth / 2, -buttonHeight / 2, buttonWidth, buttonHeight, cornerRadius);
      }
    });

    // 点击事件
    container.on('pointerdown', () => {
      this.startChallenge(challengeId);
    });

    return container;
  }

  /**
   * 开始挑战
   */
  private startChallenge(challengeId: 1 | 2 | 3): void {
    console.log(`🎮 开始挑战 ${challengeId}`);

    this.cameras.main.fadeOut(300);
    this.time.delayedCall(300, () => {
      this.scene.start('DailyChallengeScene', { challengeId });
    });
  }

  /**
   * 创建返回按钮
   */
  private createBackButton(): void {
    BackButton.create(this, 80, 1180, () => {
      this.cameras.main.fadeOut(300);
      this.time.delayedCall(300, () => {
        this.scene.start('StartScene');
      });
    });
  }

  /**
   * 创建渐变背景
   */
  private createGradientBackground(): void {
    const bg = this.add.graphics();
    bg.fillGradientStyle(
      0x4a7a9e, 0x4a7a9e,
      0x5e8ba8, 0x5e8ba8,
      1
    );
    bg.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
    bg.setDepth(-100);
  }
}
