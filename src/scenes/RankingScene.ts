import Phaser from 'phaser';
import { sceSDKManager, RankingItem } from '@/sdk/SceSDKManager';
import { SCREEN_WIDTH, SCREEN_HEIGHT } from '@/config/constants';

/**
 * 排行榜场景
 */
export class RankingScene extends Phaser.Scene {
  private rankings: RankingItem[] = [];
  private playerRank: number = -1;
  private highestScore: number = 0;
  private loadingText!: Phaser.GameObjects.Text;

  constructor() {
    super({ key: 'RankingScene' });
  }

  async create(): Promise<void> {
    // 设置背景
    this.cameras.main.setBackgroundColor(0x1a1a2e);

    // 显示加载提示
    this.loadingText = this.add.text(
      SCREEN_WIDTH / 2,
      SCREEN_HEIGHT / 2,
      '加载排行榜中...',
      {
        fontSize: '32px',
        color: '#ffffff',
        fontFamily: 'Arial'
      }
    );
    this.loadingText.setOrigin(0.5);

    // 加载数据
    await this.loadRankingData();

    // 清除加载提示
    this.loadingText.destroy();

    // 显示排行榜内容
    this.displayRanking();

    // 淡入效果
    this.cameras.main.fadeIn(500);
  }

  /**
   * 加载排行榜数据
   */
  private async loadRankingData(): Promise<void> {
    try {
      this.rankings = await sceSDKManager.getRankings(10); // 获取前10名
      this.playerRank = await sceSDKManager.getPlayerRank();
      this.highestScore = await sceSDKManager.getHighestScore();
      console.log('排行榜数据加载完成');
    } catch (error) {
      console.error('加载排行榜数据失败:', error);
    }
  }

  /**
   * 显示排行榜
   */
  private displayRanking(): void {
    // 标题
    const titleText = this.add.text(SCREEN_WIDTH / 2, 100, '🏆 排行榜 🏆', {
      fontSize: '56px',
      color: '#ffd700',
      fontFamily: 'Arial',
      fontStyle: 'bold',
      stroke: '#000000',
      strokeThickness: 6
    });
    titleText.setOrigin(0.5);

    // 玩家信息卡片
    if (this.highestScore > 0) {
      this.createPlayerCard();
    }

    // 排行榜列表
    const listTitleY = 340; // 标题位置
    const startY = 410; // 第一名开始位置（标题下方70px）
    
    if (this.rankings.length > 0) {
      // 排行榜标题
      const listTitleText = this.add.text(
        SCREEN_WIDTH / 2,
        listTitleY,
        '── 全服排行 ──',
        {
          fontSize: '28px',
          color: '#ffffff',
          fontFamily: 'Arial',
          fontStyle: 'bold'
        }
      );
      listTitleText.setOrigin(0.5);

      // 显示每个排名
      this.rankings.forEach((ranking, index) => {
        this.createRankingItem(ranking, startY + index * 70);
      });
    } else {
      // 无数据提示
      const noDataText = this.add.text(
        SCREEN_WIDTH / 2,
        startY + 100,
        '暂无排行榜数据\n快来成为第一名吧！',
        {
          fontSize: '24px',
          color: '#888888',
          fontFamily: 'Arial',
          align: 'center'
        }
      );
      noDataText.setOrigin(0.5);
    }

    // 返回按钮
    this.createBackButton();
  }

  /**
   * 创建玩家信息卡片
   */
  private createPlayerCard(): void {
    const cardY = 200;
    const cardWidth = 600;
    const cardHeight = 100;

    // 卡片背景
    const cardBg = this.add.rectangle(
      SCREEN_WIDTH / 2,
      cardY,
      cardWidth,
      cardHeight,
      0x2a2a4e,
      0.9
    );
    cardBg.setStrokeStyle(3, 0x00ffff, 0.8);

    // 玩家最高分
    const scoreText = this.add.text(
      SCREEN_WIDTH / 2 - 200,
      cardY,
      `你的最高分\n${this.highestScore}`,
      {
        fontSize: '24px',
        color: '#ffff00',
        fontFamily: 'Arial',
        fontStyle: 'bold',
        align: 'center'
      }
    );
    scoreText.setOrigin(0.5);

    // 玩家排名
    let rankText = '';
    let rankColor = '#00ff00';
    
    if (this.playerRank > 0) {
      rankText = `你的排名\n第 ${this.playerRank} 名`;
      if (this.playerRank <= 3) {
        rankColor = '#ffd700'; // 前三名金色
      }
    } else {
      rankText = `你的排名\n未上榜`;
      rankColor = '#888888';
    }

    const rankTextObj = this.add.text(
      SCREEN_WIDTH / 2 + 200,
      cardY,
      rankText,
      {
        fontSize: '24px',
        color: rankColor,
        fontFamily: 'Arial',
        fontStyle: 'bold',
        align: 'center'
      }
    );
    rankTextObj.setOrigin(0.5);
  }

  /**
   * 创建排行榜项
   */
  private createRankingItem(ranking: RankingItem, y: number): void {
    const container = this.add.container(SCREEN_WIDTH / 2, y);

    // 背景
    const bgWidth = 650;
    const bgHeight = 60;
    const bg = this.add.rectangle(0, 0, bgWidth, bgHeight, 0x2a2a4e, 0.6);
    bg.setStrokeStyle(2, 0xffffff, 0.3);

    // 排名颜色
    let rankColor = '#ffffff';
    let bgColor = 0x2a2a4e;
    
    if (ranking.rank === 1) {
      rankColor = '#ffd700'; // 金色
      bgColor = 0x3d3d00;
    } else if (ranking.rank === 2) {
      rankColor = '#c0c0c0'; // 银色
      bgColor = 0x3d3d3d;
    } else if (ranking.rank === 3) {
      rankColor = '#cd7f32'; // 铜色
      bgColor = 0x3d2d1d;
    }

    bg.setFillStyle(bgColor, 0.6);

    // 排名
    const rankText = this.add.text(-280, 0, `${ranking.rank}`, {
      fontSize: '32px',
      color: rankColor,
      fontFamily: 'Arial',
      fontStyle: 'bold'
    });
    rankText.setOrigin(0.5);

    // 奖牌图标
    let medalEmoji = '';
    if (ranking.rank === 1) medalEmoji = '🥇';
    else if (ranking.rank === 2) medalEmoji = '🥈';
    else if (ranking.rank === 3) medalEmoji = '🥉';

    if (medalEmoji) {
      const medal = this.add.text(-230, 0, medalEmoji, {
        fontSize: '28px'
      });
      medal.setOrigin(0.5);
      container.add(medal);
    }

    // 用户名
    const username = ranking.username || `玩家${ranking.rank}`;
    const usernameText = this.add.text(-50, 0, username, {
      fontSize: '24px',
      color: '#ffffff',
      fontFamily: 'Arial'
    });
    usernameText.setOrigin(0, 0.5);

    // 分数
    const scoreText = this.add.text(250, 0, `${ranking.score}`, {
      fontSize: '28px',
      color: rankColor,
      fontFamily: 'Arial',
      fontStyle: 'bold'
    });
    scoreText.setOrigin(1, 0.5);

    container.add([bg, rankText, usernameText, scoreText]);

    // 如果是玩家自己，高亮显示
    if (ranking.rank === this.playerRank) {
      bg.setStrokeStyle(3, 0x00ff00, 1);
      
      // 添加"你"的标识
      const youText = this.add.text(290, 0, '(你)', {
        fontSize: '20px',
        color: '#00ff00',
        fontFamily: 'Arial',
        fontStyle: 'bold'
      });
      youText.setOrigin(0, 0.5);
      container.add(youText);
    }

    // 进入动画
    container.setAlpha(0);
    container.setY(y - 50);
    
    this.tweens.add({
      targets: container,
      alpha: 1,
      y: y,
      duration: 300,
      delay: ranking.rank * 50,
      ease: 'Back.easeOut'
    });
  }

  /**
   * 创建返回按钮
   */
  private createBackButton(): void {
    const buttonY = SCREEN_HEIGHT - 120;
    const container = this.add.container(SCREEN_WIDTH / 2, buttonY);

    // 按钮背景
    const bg = this.add.rectangle(0, 0, 280, 70, 0x666666, 1);
    bg.setStrokeStyle(3, 0xffffff, 0.8);

    // 按钮文本
    const buttonText = this.add.text(0, 0, '返回', {
      fontSize: '32px',
      color: '#ffffff',
      fontFamily: 'Arial',
      fontStyle: 'bold'
    });
    buttonText.setOrigin(0.5);

    container.add([bg, buttonText]);
    container.setSize(280, 70);
    container.setInteractive({ useHandCursor: true });

    // 悬停效果
    container.on('pointerover', () => {
      this.tweens.add({
        targets: container,
        scaleX: 1.1,
        scaleY: 1.1,
        duration: 200,
        ease: 'Back.easeOut'
      });
      bg.setFillStyle(0x888888);
    });

    container.on('pointerout', () => {
      this.tweens.add({
        targets: container,
        scaleX: 1.0,
        scaleY: 1.0,
        duration: 200,
        ease: 'Back.easeIn'
      });
      bg.setFillStyle(0x666666);
    });

    // 点击返回
    container.on('pointerdown', () => {
      this.tweens.add({
        targets: container,
        scaleX: 0.95,
        scaleY: 0.95,
        duration: 100,
        yoyo: true,
        onComplete: () => {
          this.cameras.main.fadeOut(500);
          this.time.delayedCall(500, () => {
            this.scene.start('StartScene');
          });
        }
      });
    });
  }
}

