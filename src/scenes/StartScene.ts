import Phaser from 'phaser';
import { sceSDKManager } from '@/sdk/SceSDKManager';
import { SCREEN_WIDTH, SCREEN_HEIGHT } from '@/config/constants';
import { initTestData } from '@/utils/initTestData';

/**
 * 游戏开始场景（封面）
 */
export class StartScene extends Phaser.Scene {
  private highestScore: number = 0;

  constructor() {
    super({ key: 'StartScene' });
  }

  async create(): Promise<void> {
    // 初始化测试数据（仅开发环境）
    initTestData();

    // 初始化 SDK
    try {
      await sceSDKManager.initialize();
      console.log('SDK 初始化完成');
      
      // 获取最高分
      this.highestScore = await sceSDKManager.getHighestScore();
    } catch (error) {
      console.warn('SDK 初始化失败:', error);
      this.highestScore = 0;
    }

    // 设置背景
    this.cameras.main.setBackgroundColor(0x1a1a2e);

    // 创建渐变背景效果（装饰）
    this.createBackgroundDecoration();

    // 游戏标题
    const titleText = this.add.text(SCREEN_WIDTH / 2, 250, '像素流沙', {
      fontSize: '72px',
      color: '#ffffff',
      fontFamily: 'Arial',
      fontStyle: 'bold',
      stroke: '#000000',
      strokeThickness: 8
    });
    titleText.setOrigin(0.5);

    // 标题闪光效果
    this.tweens.add({
      targets: titleText,
      alpha: 0.7,
      duration: 1500,
      yoyo: true,
      repeat: -1,
      ease: 'Sine.easeInOut'
    });

    // 副标题
    const subtitleText = this.add.text(SCREEN_WIDTH / 2, 340, '超爽、解压', {
      fontSize: '24px',
      color: '#00ffff',
      fontFamily: 'Arial'
    });
    subtitleText.setOrigin(0.5);

    // 最高分显示
    if (this.highestScore > 0) {
      const highScoreText = this.add.text(
        SCREEN_WIDTH / 2, 
        420, 
        `最高分: ${this.highestScore}`,
        {
          fontSize: '28px',
          color: '#ffff00',
          fontFamily: 'Arial',
          fontStyle: 'bold'
        }
      );
      highScoreText.setOrigin(0.5);
    }

    // 开始游戏按钮
    this.createButton(
      SCREEN_WIDTH / 2,
      550,
      '开始游戏',
      0x00cc00,
      () => {
        // 淡出效果
        this.cameras.main.fadeOut(500);
        this.time.delayedCall(500, () => {
          this.scene.start('GameScene');
        });
      }
    );

    // 排行榜按钮
    this.createButton(
      SCREEN_WIDTH / 2,
      680,
      '排行榜',
      0x0066ff,
      () => {
        // 淡出效果
        this.cameras.main.fadeOut(500);
        this.time.delayedCall(500, () => {
          this.scene.start('RankingScene');
        });
      }
    );

    // 版权信息
    const copyrightText = this.add.text(
      SCREEN_WIDTH / 2,
      SCREEN_HEIGHT - 50,
      'Powered by Phaser & SCE SDK',
      {
        fontSize: '16px',
        color: '#888888',
        fontFamily: 'Arial'
      }
    );
    copyrightText.setOrigin(0.5);

    // 淡入效果
    this.cameras.main.fadeIn(800);
  }

  /**
   * 创建按钮
   */
  private createButton(
    x: number,
    y: number,
    text: string,
    color: number,
    callback: () => void
  ): Phaser.GameObjects.Container {
    const container = this.add.container(x, y);

    // 按钮背景
    const bg = this.add.rectangle(0, 0, 320, 80, color, 1);
    bg.setStrokeStyle(4, 0xffffff, 0.8);
    
    // 按钮文本
    const buttonText = this.add.text(0, 0, text, {
      fontSize: '36px',
      color: '#ffffff',
      fontFamily: 'Arial',
      fontStyle: 'bold'
    });
    buttonText.setOrigin(0.5);

    container.add([bg, buttonText]);
    container.setSize(320, 80);
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
      bg.setStrokeStyle(4, 0xffff00, 1);
    });

    container.on('pointerout', () => {
      this.tweens.add({
        targets: container,
        scaleX: 1.0,
        scaleY: 1.0,
        duration: 200,
        ease: 'Back.easeIn'
      });
      bg.setStrokeStyle(4, 0xffffff, 0.8);
    });

    // 点击效果
    container.on('pointerdown', () => {
      this.tweens.add({
        targets: container,
        scaleX: 0.95,
        scaleY: 0.95,
        duration: 100,
        yoyo: true,
        onComplete: callback
      });
    });

    return container;
  }

  /**
   * 创建背景装饰
   */
  private createBackgroundDecoration(): void {
    // 创建一些随机的装饰方块
    for (let i = 0; i < 15; i++) {
      const x = Phaser.Math.Between(50, SCREEN_WIDTH - 50);
      const y = Phaser.Math.Between(50, SCREEN_HEIGHT - 50);
      const size = Phaser.Math.Between(20, 50);
      const colors = [0xff0000, 0x0000ff, 0x00ff00, 0xffff00];
      const color = Phaser.Utils.Array.GetRandom(colors);

      const rect = this.add.rectangle(x, y, size, size, color, 0.1);
      rect.setRotation(Phaser.Math.Between(0, 360) * (Math.PI / 180));

      // 漂浮动画
      this.tweens.add({
        targets: rect,
        y: y + Phaser.Math.Between(-30, 30),
        duration: Phaser.Math.Between(2000, 4000),
        yoyo: true,
        repeat: -1,
        ease: 'Sine.easeInOut'
      });

      // 旋转动画
      this.tweens.add({
        targets: rect,
        rotation: rect.rotation + Math.PI * 2,
        duration: Phaser.Math.Between(5000, 10000),
        repeat: -1,
        ease: 'Linear'
      });
    }
  }
}

