/**
 * 统一的返回按钮组件
 * 用于所有场景中的返回功能，保持一致的视觉风格
 */

import Phaser from 'phaser';
import { UI_COLORS } from '@/config/constants';

export class BackButton {
  /**
   * 创建统一的返回按钮（Dark + Neon 霓虹风格圆形按钮）
   * @param scene Phaser场景
   * @param x X坐标
   * @param y Y坐标
   * @param callback 点击回调
   * @returns 返回按钮容器
   */
  public static create(
    scene: Phaser.Scene,
    x: number,
    y: number,
    callback: () => void
  ): Phaser.GameObjects.Container {
    const container = scene.add.container(x, y);
    const buttonRadius = 32;
    const color = UI_COLORS.TEXT_SECONDARY; // 中灰蓝（次要操作）

    // 深色阴影
    const shadow = scene.add.circle(3, 3, buttonRadius, UI_COLORS.SHADOW_DEEP, 0.5);
    shadow.setName('shadow');

    // 外发光层（霓虹效果）
    const glow = scene.add.circle(0, 0, buttonRadius + 4, color, 0.25);
    glow.setName('glow');

    // 主按钮背景（扁平纯色）
    const bg = scene.add.graphics();
    bg.fillStyle(UI_COLORS.BG_SECONDARY, 1);
    bg.fillCircle(0, 0, buttonRadius);

    // 霓虹边框
    bg.lineStyle(2, color, 0.6);
    bg.strokeCircle(0, 0, buttonRadius);
    bg.setName('bg');

    // 返回箭头图标（带阴影）
    const iconText = scene.add.text(0, 0, '←', {
      fontSize: '28px',
      color: '#ffffff',
      fontFamily: 'Arial, sans-serif',
      fontStyle: 'bold'
    });
    iconText.setOrigin(0.5);
    iconText.setName('icon');

    container.add([shadow, glow, bg, iconText]);

    // 交互区域
    const hitArea = new Phaser.Geom.Circle(0, 0, buttonRadius);
    container.setInteractive(hitArea, Phaser.Geom.Circle.Contains);
    container.input!.cursor = 'pointer';

    // 点击效果（0.96 scale + 120ms spring）
    container.on('pointerdown', () => {
      scene.tweens.add({
        targets: container,
        scaleX: 0.96,
        scaleY: 0.96,
        duration: 60,
        ease: 'Quad.easeOut',
        yoyo: true,
        yoyoDuration: 120,
        yoyoEase: 'Back.easeOut',
        onComplete: callback
      });
    });

    // 悬停效果（霓虹发光增强）
    container.on('pointerover', () => {
      const bgGraphics = container.getByName('bg') as Phaser.GameObjects.Graphics;
      if (bgGraphics) {
        bgGraphics.clear();
        bgGraphics.fillStyle(UI_COLORS.BG_TERTIARY, 1); // 悬浮时背景更亮
        bgGraphics.fillCircle(0, 0, buttonRadius);
        bgGraphics.lineStyle(2, color, 1); // 边框更亮
        bgGraphics.strokeCircle(0, 0, buttonRadius);
      }

      // 悬浮缩放
      scene.tweens.add({
        targets: container,
        scaleX: 1.05,
        scaleY: 1.05,
        duration: 200,
        ease: 'Back.easeOut'
      });

      // 发光脉冲动画
      const glowCircle = container.getByName('glow') as Phaser.GameObjects.Arc;
      if (glowCircle) {
        scene.tweens.add({
          targets: glowCircle,
          scaleX: 1.3,
          scaleY: 1.3,
          alpha: 0,
          duration: 500,
          ease: 'Cubic.easeOut',
          onComplete: () => {
            glowCircle.setScale(1);
            glowCircle.setAlpha(0.25);
          }
        });
      }
    });

    container.on('pointerout', () => {
      const bgGraphics = container.getByName('bg') as Phaser.GameObjects.Graphics;
      if (bgGraphics) {
        bgGraphics.clear();
        bgGraphics.fillStyle(UI_COLORS.BG_SECONDARY, 1);
        bgGraphics.fillCircle(0, 0, buttonRadius);
        bgGraphics.lineStyle(2, color, 0.6);
        bgGraphics.strokeCircle(0, 0, buttonRadius);
      }

      scene.tweens.add({
        targets: container,
        scaleX: 1.0,
        scaleY: 1.0,
        duration: 200,
        ease: 'Back.easeIn'
      });
    });

    return container;
  }
}
