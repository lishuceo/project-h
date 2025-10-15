import Phaser from 'phaser';
import { sceSDKManager } from '@/sdk/SceSDKManager';
import { SCREEN_WIDTH, SCREEN_HEIGHT } from '@/config/constants';
import { initTestData } from '@/utils/initTestData';
import { Color } from '@/types';
import { ChallengeManager } from '@/challenge/ChallengeManager';

// 全局类型声明
declare global {
  interface Window {
    challengeManager: ChallengeManager;
  }
}

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

    // 暴露 ChallengeManager 到全局（方便调试）
    window.challengeManager = ChallengeManager.getInstance();

    // 设置清新的渐变背景
    this.createGradientBackground();

    // 异步加载SDK数据（不阻塞界面显示）
    this.loadSDKData();

    // 继续创建UI（不等待SDK）
    this.createUI();

    // 淡入效果
    this.cameras.main.fadeIn(800);
  }

  /**
   * 异步加载SDK数据（带超时）
   */
  private async loadSDKData(): Promise<void> {
    try {
      // 超时控制（2秒）
      const timeout = 2000;
      const loadPromise = (async () => {
        await sceSDKManager.initialize();
        return await sceSDKManager.getHighestScore();
      })();

      const timeoutPromise = new Promise<number>((_, reject) => {
        setTimeout(() => reject(new Error('加载超时')), timeout);
      });

      this.highestScore = await Promise.race([loadPromise, timeoutPromise]);
      console.log('SDK 数据加载完成，最高分:', this.highestScore);
      
      // 更新最高分显示（如果已经创建）
      this.updateHighScoreDisplay();
    } catch (error) {
      console.warn('SDK 加载失败或超时:', error);
      this.highestScore = 0;
    }
  }

  /**
   * 更新最高分显示
   */
  private highScoreTextObj: Phaser.GameObjects.Text | null = null;

  private updateHighScoreDisplay(): void {
    if (this.highestScore > 0 && !this.highScoreTextObj) {
      this.highScoreTextObj = this.add.text(
        SCREEN_WIDTH / 2,
        820, // 调整位置，配合按钮下移
        `最高分: ${this.highestScore}`,
        {
          fontSize: '28px', // 放大字体
          color: '#fef3c7',
          fontFamily: 'Arial',
          fontStyle: 'bold'
        }
      );
      this.highScoreTextObj.setOrigin(0.5);
      this.highScoreTextObj.setAlpha(0);

      // 淡入动画
      this.tweens.add({
        targets: this.highScoreTextObj,
        alpha: 1,
        duration: 500
      });
    }
  }

  /**
   * 创建UI（扁平纯色风格）
   */
  private createUI(): void {

    // 创建背景装饰（霓虹色系）
    this.createBackgroundDecoration();

    // 游戏标题（白色，在中等背景上清晰）
    const titleText = this.add.text(SCREEN_WIDTH / 2, 450, '像素流沙', { // 优化位置，避免过于靠上
      fontSize: '72px', // 放大字体
      color: '#ffffff',
      fontFamily: 'Arial',
      fontStyle: 'bold',
      stroke: '#1e3a5f',
      strokeThickness: 4
    });
    titleText.setOrigin(0.5);

    // 标题轻微呼吸效果
    this.tweens.add({
      targets: titleText,
      scale: 1.05,
      duration: 2000,
      yoyo: true,
      repeat: -1,
      ease: 'Sine.easeInOut'
    });

    // 副标题
    const subtitleText = this.add.text(SCREEN_WIDTH / 2, 580, '超爽的物理效果', { // 紧凑间距
      fontSize: '26px', // 放大字体
      color: '#e0f2fe',
      fontFamily: 'Arial'
    });
    subtitleText.setOrigin(0.5);

    // 最高分会在SDK加载完成后异步显示（见 updateHighScoreDisplay）

    // 按钮Y坐标起始位置 - 调整到接近居中
    const buttonStartY = 1000; // 向下移动，更接近屏幕中心
    const buttonSpacing = 130; // 缩小间距，让按钮更紧凑

    // 开始游戏按钮（普通模式）
    this.createFlatButton(
      SCREEN_WIDTH / 2,
      buttonStartY,
      '🎮 普通模式',
      0x4ade80, // 霓虹绿
      () => {
        this.cameras.main.fadeOut(300);
        this.time.delayedCall(300, () => {
          this.scene.start('GameScene');
        });
      }
    );

    // 每日挑战按钮 - 改为导航到挑战选择场景
    this.createFlatButton(
      SCREEN_WIDTH / 2,
      buttonStartY + buttonSpacing,
      '⭐ 每日挑战',
      0xfbbf24, // 霓虹黄
      () => {
        this.cameras.main.fadeOut(300);
        this.time.delayedCall(300, () => {
          this.scene.start('ChallengeSelectorScene');
        });
      }
    );

    // 排行榜按钮
    this.createFlatButton(
      SCREEN_WIDTH / 2,
      buttonStartY + buttonSpacing * 2,
      '📊 排行榜',
      0x60a5fa, // 霓虹蓝
      () => {
        this.cameras.main.fadeOut(300);
        this.time.delayedCall(300, () => {
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
        color: '#e2e8f0',
        fontFamily: 'Arial'
      }
    );
    copyrightText.setOrigin(0.5);
  }

  /**
   * 创建扁平风格按钮（纯色+投影）
   */
  private createFlatButton(
    x: number,
    y: number,
    text: string,
    color: number,
    callback: () => void
  ): Phaser.GameObjects.Container {
    const container = this.add.container(x, y);
    const buttonWidth = 486; // 360 × 1.35 继续放大35%
    const buttonHeight = 115; // 85 × 1.35 继续放大35%
    const cornerRadius = 12;

    // 深色投影（偏移）
    const shadow = this.add.graphics();
    shadow.fillStyle(0x000000, 0.5);
    shadow.fillRoundedRect(
      -buttonWidth / 2 + 6,
      -buttonHeight / 2 + 6,
      buttonWidth,
      buttonHeight,
      cornerRadius
    );
    shadow.setName('shadow');

    // 按钮主体（纯色）
    const bg = this.add.graphics();
    bg.fillStyle(color, 1);
    bg.fillRoundedRect(
      -buttonWidth / 2,
      -buttonHeight / 2,
      buttonWidth,
      buttonHeight,
      cornerRadius
    );
    bg.setName('bg');

    // 按钮文本（白色 + 投影）
    const buttonText = this.add.text(0, 0, text, {
      fontSize: '43px', // 32 × 1.35 继续放大35%
      color: '#ffffff',
      fontFamily: 'Arial',
      fontStyle: 'bold',
      shadow: {
        offsetX: 0,
        offsetY: 1,
        color: '#000000',
        blur: 3,
        fill: true
      }
    });
    buttonText.setOrigin(0.5);
    buttonText.setName('text');

    container.add([shadow, bg, buttonText]);
    container.setSize(buttonWidth, buttonHeight);
    container.setInteractive({ useHandCursor: true });

    // 悬停效果（轻微上浮+投影增强）
    container.on('pointerover', () => {
      this.tweens.add({
        targets: container,
        y: y - 5,
        scaleX: 1.03,
        scaleY: 1.03,
        duration: 150,
        ease: 'Quad.easeOut'
      });

      // 投影增强
      const shadowGraphics = container.getByName('shadow') as Phaser.GameObjects.Graphics;
      if (shadowGraphics) {
        shadowGraphics.clear();
        shadowGraphics.fillStyle(0x000000, 0.7);
        shadowGraphics.fillRoundedRect(
          -buttonWidth / 2 + 8,
          -buttonHeight / 2 + 8,
          buttonWidth,
          buttonHeight,
          cornerRadius
        );
      }

      // 按钮变亮
      const bgGraphics = container.getByName('bg') as Phaser.GameObjects.Graphics;
      if (bgGraphics) {
        bgGraphics.clear();
        bgGraphics.fillStyle(this.lightenColor(color, 0.15), 1);
        bgGraphics.fillRoundedRect(
          -buttonWidth / 2,
          -buttonHeight / 2,
          buttonWidth,
          buttonHeight,
          cornerRadius
        );
      }
    });

    container.on('pointerout', () => {
      this.tweens.add({
        targets: container,
        y: y,
        scaleX: 1.0,
        scaleY: 1.0,
        duration: 150,
        ease: 'Quad.easeIn'
      });

      // 投影还原
      const shadowGraphics = container.getByName('shadow') as Phaser.GameObjects.Graphics;
      if (shadowGraphics) {
        shadowGraphics.clear();
        shadowGraphics.fillStyle(0x000000, 0.5);
        shadowGraphics.fillRoundedRect(
          -buttonWidth / 2 + 6,
          -buttonHeight / 2 + 6,
          buttonWidth,
          buttonHeight,
          cornerRadius
        );
      }

      // 按钮颜色还原
      const bgGraphics = container.getByName('bg') as Phaser.GameObjects.Graphics;
      if (bgGraphics) {
        bgGraphics.clear();
        bgGraphics.fillStyle(color, 1);
        bgGraphics.fillRoundedRect(
          -buttonWidth / 2,
          -buttonHeight / 2,
          buttonWidth,
          buttonHeight,
          cornerRadius
        );
      }
    });

    // 点击效果（按下感）
    container.on('pointerdown', () => {
      this.tweens.add({
        targets: container,
        y: y + 2,
        scaleX: 0.98,
        scaleY: 0.98,
        duration: 80,
        yoyo: true,
        yoyoDuration: 120,
        ease: 'Quad.easeOut',
        onComplete: callback
      });

      // 投影缩小
      const shadowGraphics = container.getByName('shadow') as Phaser.GameObjects.Graphics;
      if (shadowGraphics) {
        shadowGraphics.clear();
        shadowGraphics.fillStyle(0x000000, 0.3);
        shadowGraphics.fillRoundedRect(
          -buttonWidth / 2 + 3,
          -buttonHeight / 2 + 3,
          buttonWidth,
          buttonHeight,
          cornerRadius
        );
      }
    });

    return container;
  }

  /**
   * 颜色变亮工具函数
   */
  private lightenColor(color: number, amount: number): number {
    const r = ((color >> 16) & 0xFF);
    const g = ((color >> 8) & 0xFF);
    const b = (color & 0xFF);

    const newR = Math.min(255, Math.floor(r + (255 - r) * amount));
    const newG = Math.min(255, Math.floor(g + (255 - g) * amount));
    const newB = Math.min(255, Math.floor(b + (255 - b) * amount));

    return (newR << 16) | (newG << 8) | newB;
  }

  /**
   * 创建清新的渐变背景
   */
  private createGradientBackground(): void {
    // 使用Graphics创建渐变背景（调暗的蓝灰色渐变）
    const bg = this.add.graphics();

    // Phaser的fillGradientStyle创建垂直渐变
    // 参数：左上色、右上色、左下色、右下色、透明度
    bg.fillGradientStyle(
      0x4a7a9e, 0x4a7a9e,  // 顶部：深蓝灰（调暗）
      0x5e8ba8, 0x5e8ba8,  // 底部：浅蓝灰（调暗）
      1
    );
    bg.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
    bg.setDepth(-100); // 置于最底层
  }

  /**
   * 创建背景装饰（霓虹色系，柔和融入背景）
   */
  private createBackgroundDecoration(): void {
    // 霓虹色系
    const neonColors = [
      Color.RED,    // 0xf87171
      Color.BLUE,   // 0x60a5fa
      Color.GREEN,  // 0x4ade80
      Color.YELLOW, // 0xfbbf24
    ];

    // 创建随机漂浮的装饰方块
    for (let i = 0; i < 10; i++) {
      const x = Phaser.Math.Between(50, SCREEN_WIDTH - 50);
      const y = Phaser.Math.Between(50, SCREEN_HEIGHT - 50);
      const size = Phaser.Math.Between(30, 60);
      const color = Phaser.Utils.Array.GetRandom(neonColors);

      const rect = this.add.rectangle(x, y, size, size, color, 1.0); // 完全不透明
      rect.setRotation(Phaser.Math.Between(0, 360) * (Math.PI / 180));
      rect.setDepth(-50); // 设置在背景渐变之上，但在所有UI元素之下

      // 漂浮动画
      this.tweens.add({
        targets: rect,
        y: y + Phaser.Math.Between(-50, 50),
        duration: Phaser.Math.Between(4000, 7000),
        yoyo: true,
        repeat: -1,
        ease: 'Sine.easeInOut'
      });

      // 缓慢旋转
      this.tweens.add({
        targets: rect,
        rotation: rect.rotation + Math.PI * 2,
        duration: Phaser.Math.Between(10000, 18000),
        repeat: -1,
        ease: 'Linear'
      });
    }
  }
}

