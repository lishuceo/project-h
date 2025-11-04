import Phaser from 'phaser';
import { PixelBlock, Color } from '@/types';
import { PIXEL_SIZE, GAME_AREA_OFFSET_X, GAME_AREA_OFFSET_Y } from '@/config/constants';

/**
 * 消除动画系统
 * 实现科技感风格的消除特效：识别高亮 + 粒子爆炸
 */

// 动画配置
const ANIM_CONFIG = {
  // 阶段1: 识别高亮 (300ms)
  PHASE1_DURATION: 300,
  PHASE1_PULSE_COUNT: 2,
  PHASE1_SCALE_MAX: 1.4,        // 增大缩放幅度（1.2 → 1.4）
  PHASE1_GLOW_COLOR: 0xffffff,
  PHASE1_GLOW_INTENSITY: 2.5,   // 发光强度

  // 阶段3: 粒子爆炸 (400ms)
  PHASE3_DURATION: 400,
  PHASE3_PARTICLES_PER_PIXEL: 6,
  PHASE3_PARTICLE_SPEED: 200,
  PHASE3_PARTICLE_GRAVITY: 300,

  // 性能优化：限制粒子数量（高亮不再限制，使用共享 Tween）
  MAX_PARTICLES: 2000,          // 最多 2000 个粒子

  // 总时长
  TOTAL_DURATION: 700, // 300 + 400
};

// 粒子数据结构
interface Particle {
  sprite: Phaser.GameObjects.Rectangle;
  vx: number;
  vy: number;
  lifetime: number;
  maxLifetime: number;
  size: number;
}

export class EliminationAnimation {
  private scene: Phaser.Scene;
  private animationLayer: Phaser.GameObjects.Container;
  private particles: Particle[] = [];
  private highlightSprites: Phaser.GameObjects.Rectangle[] = [];

  // 性能优化：跟踪 TimerEvent 以防止泄漏
  private updateInterval: Phaser.Time.TimerEvent | null = null;
  private completionTimer: Phaser.Time.TimerEvent | null = null;

  // 性能优化：跟踪 Tween 对象以便清理
  private activeTweens: Phaser.Tweens.Tween[] = [];
  private activeTextObjects: Phaser.GameObjects.Text[] = [];

  constructor(scene: Phaser.Scene) {
    this.scene = scene;
    // 创建动画层（独立于游戏层）
    this.animationLayer = this.scene.add.container(0, 0);
    this.animationLayer.setDepth(100); // 确保在最上层
  }

  /**
   * 播放完整的消除动画
   * @param pixels 要消除的像素块数组
   * @param score 本次消除得分
   * @param chainLevel 连锁等级（1=无连锁，2+=连锁）
   * @param onComplete 动画完成回调
   */
  playEliminationAnimation(
    pixels: PixelBlock[],
    score: number,
    chainLevel: number,
    onComplete: () => void
  ): void {
    const startTime = performance.now();
    console.log(`🎬 开始消除动画 - 像素: ${pixels.length}, 得分: ${score}, 连锁: x${chainLevel}`);

    // 性能优化：先清理之前的动画（防止快速连续消除导致的泄漏）
    this.cleanup();

    // 计算消除区域的中心位置
    const centerPos = this.calculateCenter(pixels);

    // 显示分数飘字
    this.showScorePopup(centerPos, score, chainLevel);

    // 阶段1: 识别高亮
    this.playPhase1_Highlight(pixels, () => {
      // 阶段3: 粒子爆炸（直接跳过阶段2）
      this.playPhase3_Explosion(pixels, () => {
        // 动画完成，清理并回调
        this.cleanup();
        const totalTime = performance.now() - startTime;
        console.log(`✅ 消除动画完成 - 总耗时: ${totalTime.toFixed(1)}ms`);
        onComplete();
      });
    });
  }

  /**
   * 阶段1: 识别高亮动画
   * 性能优化：使用共享 Tween + 批量更新，而非为每个精灵创建 Tween
   */
  private playPhase1_Highlight(pixels: PixelBlock[], onComplete: () => void): void {
    const startTime = performance.now();

    // 为所有像素块创建高亮精灵（保证视觉完整）
    pixels.forEach(pixel => {
      const screenPos = this.pixelToScreen(pixel.x, pixel.y);

      const highlight = this.scene.add.rectangle(
        screenPos.x,
        screenPos.y,
        PIXEL_SIZE,
        PIXEL_SIZE,
        pixel.color,
        1.0
      );

      // 增强边框效果（更粗更亮）
      highlight.setStrokeStyle(2, ANIM_CONFIG.PHASE1_GLOW_COLOR, 1.0);
      this.animationLayer.add(highlight);
      this.highlightSprites.push(highlight);
    });

    const createTime = performance.now() - startTime;

    // 性能优化：只创建 1 个共享 Tween，在回调中批量更新所有精灵
    const animationState = {
      scale: 1.0,
      alpha: 1.0,
      strokeWidth: 2,
      strokeAlpha: 1.0
    };

    const highlightTween = this.scene.tweens.add({
      targets: animationState,
      scale: ANIM_CONFIG.PHASE1_SCALE_MAX,
      alpha: 1.0,
      strokeWidth: ANIM_CONFIG.PHASE1_GLOW_INTENSITY * 2, // 边框脉冲到 5px
      strokeAlpha: 1.0,
      duration: ANIM_CONFIG.PHASE1_DURATION / (ANIM_CONFIG.PHASE1_PULSE_COUNT * 2),
      yoyo: true,
      repeat: ANIM_CONFIG.PHASE1_PULSE_COUNT - 1,
      ease: 'Sine.easeInOut',
      onUpdate: () => {
        // 批量更新所有高亮精灵
        this.highlightSprites.forEach(sprite => {
          sprite.setScale(animationState.scale);
          sprite.setAlpha(animationState.alpha);
          // 更新边框效果（发光脉冲）
          sprite.setStrokeStyle(
            animationState.strokeWidth,
            ANIM_CONFIG.PHASE1_GLOW_COLOR,
            animationState.strokeAlpha
          );
        });
      }
    });

    // 保存 Tween 引用以便清理
    this.activeTweens.push(highlightTween);

    console.log(`  📍 阶段1 - 高亮精灵: ${pixels.length}个, Tween: 1个(共享), 耗时: ${createTime.toFixed(1)}ms`);

    // 阶段1完成后回调
    this.scene.time.delayedCall(ANIM_CONFIG.PHASE1_DURATION, () => {
      onComplete();
    });
  }

  /**
   * 阶段3: 粒子爆炸动画
   */
  private playPhase3_Explosion(pixels: PixelBlock[], onComplete: () => void): void {
    const startTime = performance.now();

    // 先隐藏高亮精灵
    this.highlightSprites.forEach((sprite) => {
      sprite.setVisible(false);
    });

    // 性能优化：限制粒子数量
    const maxPixelsForParticles = Math.floor(ANIM_CONFIG.MAX_PARTICLES / ANIM_CONFIG.PHASE3_PARTICLES_PER_PIXEL);
    const sampledPixels = pixels.length > maxPixelsForParticles
      ? this.samplePixels(pixels, maxPixelsForParticles)
      : pixels;

    if (sampledPixels.length < pixels.length) {
      console.log(`  ⚡ 性能优化 - 采样粒子: ${sampledPixels.length}/${pixels.length}个像素块`);
    }

    // 为每个像素块创建粒子
    sampledPixels.forEach((pixel) => {
      this.createParticlesForPixel(pixel);
    });

    const particleCount = this.particles.length;
    const createTime = performance.now() - startTime;
    console.log(`  💥 阶段3 - 粒子: ${particleCount}个 (${sampledPixels.length}/${pixels.length}像素 × ${ANIM_CONFIG.PHASE3_PARTICLES_PER_PIXEL}), 耗时: ${createTime.toFixed(1)}ms`);

    // 启动粒子更新循环（保存引用以防泄漏）
    this.updateInterval = this.scene.time.addEvent({
      delay: 16, // 约60 FPS
      callback: () => {
        this.updateParticles();
      },
      loop: true,
    });

    // 阶段3完成后停止更新并回调（保存引用以防泄漏）
    this.completionTimer = this.scene.time.delayedCall(ANIM_CONFIG.PHASE3_DURATION, () => {
      if (this.updateInterval) {
        this.updateInterval.remove();
        this.updateInterval = null;
      }
      onComplete();
    });
  }

  /**
   * 为单个像素块创建粒子
   */
  private createParticlesForPixel(pixel: PixelBlock): void {
    const screenPos = this.pixelToScreen(pixel.x, pixel.y);
    const particleCount = ANIM_CONFIG.PHASE3_PARTICLES_PER_PIXEL;
    
    for (let i = 0; i < particleCount; i++) {
      // 随机速度和方向
      const angle = (Math.PI * 2 * i) / particleCount + (Math.random() - 0.5) * 0.5;
      const speed = ANIM_CONFIG.PHASE3_PARTICLE_SPEED * (0.5 + Math.random() * 0.5);
      const vx = Math.cos(angle) * speed;
      const vy = Math.sin(angle) * speed - 50; // 向上偏移
      
      // 粒子大小
      const size = PIXEL_SIZE * (0.3 + Math.random() * 0.4);
      
      // 创建粒子精灵
      const particleSprite = this.scene.add.rectangle(
        screenPos.x,
        screenPos.y,
        size,
        size,
        this.getParticleColor(pixel.color),
        1.0
      );
      
      // 科技感：添加发光边框
      particleSprite.setStrokeStyle(0.5, 0x00ffff, 0.8);
      
      this.animationLayer.add(particleSprite);
      
      // 添加到粒子列表
      this.particles.push({
        sprite: particleSprite,
        vx: vx,
        vy: vy,
        lifetime: 0,
        maxLifetime: ANIM_CONFIG.PHASE3_DURATION,
        size: size
      });
    }
  }

  /**
   * 获取粒子颜色（科技感：偏蓝白）
   */
  private getParticleColor(originalColor: Color): number {
    // 科技感配色：在原色基础上增加蓝白光晕
    // 当前使用4种颜色：红、黄、绿、蓝
    const colors: Record<Color, number> = {
      [Color.RED]: 0xff6666,
      [Color.YELLOW]: 0xffff66,
      [Color.GREEN]: 0x66ff66,
      [Color.BLUE]: 0x6666ff,
    };
    return colors[originalColor] || 0xffffff;
  }

  /**
   * 更新所有粒子
   * 性能优化：使用 filter 代替 splice，避免 O(n²) 复杂度
   */
  private updateParticles(): void {
    const dt = 16 / 1000; // 约 16ms = 1/60秒

    // 性能优化：使用 filter 一次性移除所有过期粒子
    this.particles = this.particles.filter((particle) => {
      // 更新生命周期
      particle.lifetime += 16;
      const progress = particle.lifetime / particle.maxLifetime;

      if (progress >= 1.0) {
        // 粒子生命结束，销毁
        particle.sprite.destroy();
        return false; // 从数组中移除
      }

      // 更新位置
      particle.sprite.x += particle.vx * dt;
      particle.sprite.y += particle.vy * dt;

      // 应用重力
      particle.vy += ANIM_CONFIG.PHASE3_PARTICLE_GRAVITY * dt;

      // 更新透明度（淡出）
      particle.sprite.setAlpha(1.0 - progress);

      // 更新大小（缩小）
      const scale = 1.0 - progress * 0.5;
      particle.sprite.setScale(scale);

      return true; // 保留在数组中
    });
  }

  /**
   * 像素坐标转屏幕坐标
   */
  private pixelToScreen(pixelX: number, pixelY: number): { x: number; y: number } {
    return {
      x: GAME_AREA_OFFSET_X + pixelX * PIXEL_SIZE + PIXEL_SIZE / 2,
      y: GAME_AREA_OFFSET_Y + pixelY * PIXEL_SIZE + PIXEL_SIZE / 2
    };
  }

  /**
   * 性能优化：采样像素块（随机采样，更自然）
   */
  private samplePixels(pixels: PixelBlock[], maxCount: number): PixelBlock[] {
    if (pixels.length <= maxCount) {
      return pixels;
    }

    // 使用集合记录已选择的索引，避免重复
    const selected = new Set<number>();
    const sampled: PixelBlock[] = [];

    // 随机选择 maxCount 个不重复的像素块
    while (sampled.length < maxCount) {
      const randomIndex = Math.floor(Math.random() * pixels.length);
      if (!selected.has(randomIndex)) {
        selected.add(randomIndex);
        sampled.push(pixels[randomIndex]);
      }
    }

    return sampled;
  }

  /**
   * 计算消除区域的中心位置
   */
  private calculateCenter(pixels: PixelBlock[]): { x: number; y: number } {
    if (pixels.length === 0) {
      return { x: 400, y: 600 }; // 默认中心
    }

    let sumX = 0;
    let sumY = 0;
    
    pixels.forEach(pixel => {
      const screenPos = this.pixelToScreen(pixel.x, pixel.y);
      sumX += screenPos.x;
      sumY += screenPos.y;
    });
    
    return {
      x: sumX / pixels.length,
      y: sumY / pixels.length
    };
  }

  /**
   * 显示分数飘字动画
   */
  private showScorePopup(centerPos: { x: number; y: number }, score: number, chainLevel: number): void {
    const startTime = performance.now();
    let tweenCount = 0;

    // 如果有连锁，先显示连锁文字
    if (chainLevel > 1) {
      const chainText = this.scene.add.text(centerPos.x, centerPos.y - 60, `CHAIN x${chainLevel}!`, {
        fontSize: '40px',
        color: '#ff00ff',
        fontFamily: 'Arial',
        fontStyle: 'bold',
        stroke: '#ffffff',
        strokeThickness: 4
      });
      chainText.setOrigin(0.5);
      chainText.setDepth(101);
      chainText.setAlpha(0);

      // 保存文本对象引用
      this.activeTextObjects.push(chainText);

      // 连锁文字动画：淡入 + 放大 + 淡出（原地）
      const chainTween1 = this.scene.tweens.add({
        targets: chainText,
        alpha: 1,
        scale: 1.3,
        duration: 200,
        ease: 'Back.easeOut',
        onComplete: () => {
          const chainTween2 = this.scene.tweens.add({
            targets: chainText,
            alpha: 0,
            scale: 1.5,
            duration: 400,
            delay: 200,
            ease: 'Cubic.easeIn',
            onComplete: () => {
              chainText.destroy();
            }
          });
          this.activeTweens.push(chainTween2);
          tweenCount++;
        }
      });
      this.activeTweens.push(chainTween1);
      tweenCount++;
    }

    // 分数文字
    const scoreText = this.scene.add.text(centerPos.x, centerPos.y, `+${score}`, {
      fontSize: chainLevel > 1 ? '52px' : '44px',
      color: chainLevel > 1 ? '#ffff00' : '#ffffff',
      fontFamily: 'Arial',
      fontStyle: 'bold',
      stroke: chainLevel > 1 ? '#ff6600' : '#000000',
      strokeThickness: 6
    });
    scoreText.setOrigin(0.5);
    scoreText.setDepth(101);
    scoreText.setScale(0);

    // 保存文本对象引用
    this.activeTextObjects.push(scoreText);

    // 分数动画：弹出 → 停留 → 淡出（原地）
    const scoreTween1 = this.scene.tweens.add({
      targets: scoreText,
      scale: 1.5,
      duration: 150,
      ease: 'Back.easeOut',
      onComplete: () => {
        // 停留一段时间
        const scoreTween2 = this.scene.tweens.add({
          targets: scoreText,
          scale: 1.2,
          duration: 100,
          ease: 'Linear',
          onComplete: () => {
            // 原地淡出并放大消失
            const scoreTween3 = this.scene.tweens.add({
              targets: scoreText,
              scale: 1.8,
              alpha: 0,
              duration: 400,
              ease: 'Cubic.easeOut',
              onComplete: () => {
                scoreText.destroy();
              }
            });
            this.activeTweens.push(scoreTween3);
            tweenCount++;
          }
        });
        this.activeTweens.push(scoreTween2);
        tweenCount++;
      }
    });
    this.activeTweens.push(scoreTween1);
    tweenCount++;

    const createTime = performance.now() - startTime;
    console.log(`  ✨ 飘字 - 文本: ${chainLevel > 1 ? 2 : 1}个, Tween: ${tweenCount}个, 耗时: ${createTime.toFixed(1)}ms`);
  }

  /**
   * 清理所有动画资源
   */
  private cleanup(): void {
    // 性能优化：清理 TimerEvent（防止泄漏）
    if (this.updateInterval) {
      this.updateInterval.remove();
      this.updateInterval = null;
    }
    if (this.completionTimer) {
      this.completionTimer.remove();
      this.completionTimer = null;
    }

    // 性能优化：停止并清理所有 Tween（防止累积）
    const tweenCount = this.activeTweens.length;
    this.activeTweens.forEach((tween) => {
      if (tween && tween.isPlaying()) {
        tween.stop();
      }
      tween.remove();
    });
    this.activeTweens = [];

    // 清理所有文本对象
    this.activeTextObjects.forEach((text) => {
      if (text && text.active) {
        text.destroy();
      }
    });
    this.activeTextObjects = [];

    // 清理高亮精灵
    this.highlightSprites.forEach((sprite) => {
      sprite.destroy();
    });
    this.highlightSprites = [];

    // 清理剩余粒子
    this.particles.forEach((particle) => {
      particle.sprite.destroy();
    });
    this.particles = [];

    if (tweenCount > 0) {
      console.log(`🧹 动画清理 - 停止了 ${tweenCount} 个 Tween`);
    }
  }

  /**
   * 销毁动画系统
   */
  destroy(): void {
    this.cleanup();
    this.animationLayer.destroy();
  }
}

