import Phaser from 'phaser';
import { sceSDKManager, RankingItem } from '@/sdk/SceSDKManager';
import { SCREEN_WIDTH, SCREEN_HEIGHT } from '@/config/constants';
import { BackButton } from '@/ui/BackButton';

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
    // 创建蓝色渐变背景（与其他场景一致）
    this.createGradientBackground();

    // 显示加载提示
    this.loadingText = this.add.text(
      SCREEN_WIDTH / 2,
      SCREEN_HEIGHT / 2,
      '加载中...',
      {
        fontSize: '32px',
        color: '#ffffff',
        fontFamily: 'Arial'
      }
    );
    this.loadingText.setOrigin(0.5);

    // 加载动画（点点点）
    let dotCount = 0;
    const loadingInterval = this.time.addEvent({
      delay: 500,
      callback: () => {
        dotCount = (dotCount + 1) % 4;
        this.loadingText.setText('加载中' + '.'.repeat(dotCount));
      },
      loop: true
    });

    // 加载数据（带超时）
    await this.loadRankingData();

    // 停止加载动画
    loadingInterval.remove();

    // 清除加载提示
    this.loadingText.destroy();

    // 显示排行榜内容
    this.displayRanking();

    // 淡入效果
    this.cameras.main.fadeIn(500);
  }

  /**
   * 加载排行榜数据（带超时和并行加载）
   */
  private async loadRankingData(): Promise<void> {
    try {
      // 设置3秒超时
      const timeout = 3000;
      
      // 并行加载所有数据（提高速度）
      const loadPromise = Promise.all([
        sceSDKManager.getRankings(10),
        sceSDKManager.getPlayerRank(),
        sceSDKManager.getHighestScore()
      ]);

      // 添加超时控制
      const timeoutPromise = new Promise<any>((_, reject) => {
        setTimeout(() => reject(new Error('加载超时')), timeout);
      });

      const results = await Promise.race([loadPromise, timeoutPromise]);
      
      // 解构结果
      this.rankings = results[0] || [];
      this.playerRank = results[1] || -1;
      this.highestScore = results[2] || 0;
      
      console.log('排行榜数据加载完成');
    } catch (error) {
      console.warn('加载排行榜数据失败或超时:', error);
      // 即使失败也继续显示界面（显示空数据）
      this.rankings = [];
      this.playerRank = -1;
      this.highestScore = 0;
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
    BackButton.create(this, 80, 1180, () => {
      this.cameras.main.fadeOut(300);
      this.time.delayedCall(300, () => {
        this.scene.start('StartScene');
      });
    });
  }

  /**
   * 创建蓝色渐变背景
   */
  private createGradientBackground(): void {
    const bg = this.add.graphics();
    bg.fillGradientStyle(
      0x4a7a9e, 0x4a7a9e,  // 顶部：深蓝灰（调暗）
      0x5e8ba8, 0x5e8ba8,  // 底部：浅蓝灰（调暗）
      1
    );
    bg.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
    bg.setDepth(-100);
  }
}

