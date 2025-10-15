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

  async create(): Promise<void> {
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

    // 后台更新所有已完成挑战的排名（不阻塞UI显示）
    this.updateRanksInBackground();

    // 淡入效果
    this.cameras.main.fadeIn(500);
  }

  /**
   * 后台更新排名
   */
  private async updateRanksInBackground(): Promise<void> {
    try {
      console.log('🔄 后台更新排名中...');
      await this.challengeManager.updateAllRanks();

      // 排名更新完成后，重新创建UI以显示最新排名
      console.log('✅ 排名更新完成，刷新UI');
      this.children.removeAll(true); // 清除所有现有UI
      this.createUI(); // 重新创建UI
    } catch (error) {
      console.error('❌ 更新排名失败:', error);
    }
  }

  /**
   * 创建UI
   */
  private createUI(): void {
    // 创建渐变背景
    this.createGradientBackground();

    // 标题 - 放大
    const title = this.add.text(SCREEN_WIDTH / 2, 219, '每日挑战', {
      fontSize: '64px', // 放大
      color: '#ffffff',
      fontFamily: 'Arial',
      fontStyle: 'bold',
      stroke: '#1e3a5f',
      strokeThickness: 4
    });
    title.setOrigin(0.5);

    // 日期显示 - 放大
    const today = this.challenges[0].date;
    const dateText = this.add.text(SCREEN_WIDTH / 2, 329, `${today}`, {
      fontSize: '28px', // 放大
      color: '#e0f2fe',
      fontFamily: 'Arial'
    });
    dateText.setOrigin(0.5);

    // 创建3个挑战卡片 - 更紧凑的布局
    const cardY = 550; // 调整起始位置
    const cardSpacing = 380; // 缩小间距，让卡片更紧凑

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
    const cardWidth = 900; // 增加宽度
    const cardHeight = 320; // 增加高度
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

    // 难度标识 - 继续放大
    const difficultyLabels = ['简单', '中等', '困难'];
    const difficultyColors = [0x4ade80, 0xfbbf24, 0xf87171]; // 绿、黄、红
    const stars = '⭐'.repeat(challenge.difficulty);

    const difficultyBadge = this.add.container(-cardWidth / 2 + 60, -cardHeight / 2 + 60);
    const badgeBg = this.add.circle(0, 0, 50, difficultyColors[challengeId - 1]); // 继续放大
    const badgeText = this.add.text(0, 0, `${challengeId}`, {
      fontSize: '40px', // 继续放大
      color: '#ffffff',
      fontFamily: 'Arial',
      fontStyle: 'bold'
    });
    badgeText.setOrigin(0.5);
    difficultyBadge.add([badgeBg, badgeText]);
    container.add(difficultyBadge);

    // 标题 - 继续放大
    const titleText = this.add.text(-cardWidth / 2 + 150, -cardHeight / 2 + 50,
      `挑战${challengeId}：${difficultyLabels[challengeId - 1]}`, {
      fontSize: '44px', // 继续放大
      color: isUnlocked ? '#ffffff' : '#888888',
      fontFamily: 'Arial',
      fontStyle: 'bold'
    });
    titleText.setOrigin(0, 0.5);
    container.add(titleText);

    // 星级 - 继续放大
    const starsText = this.add.text(-cardWidth / 2 + 150, -cardHeight / 2 + 105, stars, {
      fontSize: '34px' // 继续放大
    });
    starsText.setOrigin(0, 0.5);
    container.add(starsText);

    // 状态信息
    if (!isUnlocked) {
      // 未解锁：显示大锁图标
      const lockIcon = this.add.text(0, -40, '🔒', {
        fontSize: '60px', // 继续放大
        padding: { top: 10, bottom: 10, left: 0, right: 0 }
      });
      lockIcon.setOrigin(0.5, 0);
      container.add(lockIcon);

      const unlockText = this.add.text(0, 50, `完成挑战${challengeId - 1}后解锁`, {
        fontSize: '28px', // 继续放大
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

        // 如果有全球排名，显示排名信息
        if (record.globalRank && record.totalPlayers) {
          const rankText = this.add.text(0, -60,
            `🌍 全球排名: ${record.globalRank} / ${record.totalPlayers}`, {
            fontSize: '32px', // 继续放大
            color: '#ffd700',
            fontFamily: 'Arial',
            fontStyle: 'bold'
          });
          rankText.setOrigin(0.5);
          container.add(rankText);

          // 调整完成状态文本位置
          const status = this.add.text(0, -20, statusText, {
            fontSize: '26px', // 继续放大
            color: statusColor,
            fontFamily: 'Arial'
          });
          status.setOrigin(0.5);
          container.add(status);

          // 关卡信息（只在已解锁时显示）
          const infoText = this.add.text(0, 25,
            `目标: 清除所有方块 | 步数限制: ${challenge.maxSteps}`, {
            fontSize: '22px', // 继续放大
            color: '#94a3b8',
            fontFamily: 'Arial'
          });
          infoText.setOrigin(0.5);
          container.add(infoText);
        } else {
          // 没有排名数据，正常显示
          const status = this.add.text(0, -35, statusText, {
            fontSize: '28px', // 继续放大
            color: statusColor,
            fontFamily: 'Arial'
          });
          status.setOrigin(0.5);
          container.add(status);

          // 关卡信息
          const infoText = this.add.text(0, 25,
            `目标: 清除所有方块 | 步数限制: ${challenge.maxSteps}`, {
            fontSize: '24px', // 继续放大
            color: '#94a3b8',
            fontFamily: 'Arial'
          });
          infoText.setOrigin(0.5);
          container.add(infoText);
        }
      } else if (record && !record.completed) {
        statusText = `已尝试 ${record.attempts} 次`;
        statusColor = '#fbbf24';

        const status = this.add.text(0, -35, statusText, {
          fontSize: '28px', // 继续放大
          color: statusColor,
          fontFamily: 'Arial'
        });
        status.setOrigin(0.5);
        container.add(status);

        // 关卡信息
        const infoText = this.add.text(0, 25,
          `目标: 清除所有方块 | 步数限制: ${challenge.maxSteps}`, {
          fontSize: '24px', // 继续放大
          color: '#94a3b8',
          fontFamily: 'Arial'
        });
        infoText.setOrigin(0.5);
        container.add(infoText);
      } else {
        statusText = '等待挑战';

        const status = this.add.text(0, -35, statusText, {
          fontSize: '28px', // 继续放大
          color: statusColor,
          fontFamily: 'Arial'
        });
        status.setOrigin(0.5);
        container.add(status);

        // 关卡信息
        const infoText = this.add.text(0, 25,
          `目标: 清除所有方块 | 步数限制: ${challenge.maxSteps}`, {
          fontSize: '24px', // 继续放大
          color: '#94a3b8',
          fontFamily: 'Arial'
        });
        infoText.setOrigin(0.5);
        container.add(infoText);
      }
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
    const buttonWidth = 320; // 继续加宽
    const buttonHeight = 85; // 继续加高
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
      fontSize: isCompleted ? '30px' : '34px', // 继续放大
      color: isCompleted ? '#e2e8f0' : '#ffffff',
      fontFamily: 'Arial',
      fontStyle: isCompleted ? 'normal' : 'bold'
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
    BackButton.create(this, 100, 2200, () => { // 与GameScene保持一致
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
