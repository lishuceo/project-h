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
  PHASE1_SCALE_MAX: 1.2,
  PHASE1_GLOW_COLOR: 0xffffff,
  
  // 阶段3: 粒子爆炸 (400ms)
  PHASE3_DURATION: 400,
  PHASE3_PARTICLES_PER_PIXEL: 6,
  PHASE3_PARTICLE_SPEED: 200,
  PHASE3_PARTICLE_GRAVITY: 300,
  
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
    console.log(`开始播放消除动画，像素块数量: ${pixels.length}, 得分: ${score}, 连锁: x${chainLevel}`);
    
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
        onComplete();
      });
    });
  }

  /**
   * 阶段1: 识别高亮动画
   */
  private playPhase1_Highlight(pixels: PixelBlock[], onComplete: () => void): void {
    // 为每个像素块创建高亮精灵
    pixels.forEach(pixel => {
      const screenPos = this.pixelToScreen(pixel.x, pixel.y);
      
      // 创建高亮方块
      const highlight = this.scene.add.rectangle(
        screenPos.x,
        screenPos.y,
        PIXEL_SIZE,
        PIXEL_SIZE,
        pixel.color,
        1.0
      );
      
      // 添加发光边框（科技感）
      highlight.setStrokeStyle(1, ANIM_CONFIG.PHASE1_GLOW_COLOR, 1.0);
      
      this.animationLayer.add(highlight);
      this.highlightSprites.push(highlight);
      
      // 脉冲动画：缩放 + 亮度
      this.scene.tweens.add({
        targets: highlight,
        scaleX: ANIM_CONFIG.PHASE1_SCALE_MAX,
        scaleY: ANIM_CONFIG.PHASE1_SCALE_MAX,
        alpha: 1.0,
        duration: ANIM_CONFIG.PHASE1_DURATION / (ANIM_CONFIG.PHASE1_PULSE_COUNT * 2),
        yoyo: true,
        repeat: ANIM_CONFIG.PHASE1_PULSE_COUNT - 1,
        ease: 'Sine.easeInOut'
      });
      
      // 发光边框脉冲
      this.scene.tweens.add({
        targets: highlight,
        lineWidth: 2,
        duration: ANIM_CONFIG.PHASE1_DURATION / (ANIM_CONFIG.PHASE1_PULSE_COUNT * 2),
        yoyo: true,
        repeat: ANIM_CONFIG.PHASE1_PULSE_COUNT - 1,
        ease: 'Sine.easeInOut'
      });
    });
    
    // 阶段1完成后回调
    this.scene.time.delayedCall(ANIM_CONFIG.PHASE1_DURATION, () => {
      onComplete();
    });
  }

  /**
   * 阶段3: 粒子爆炸动画
   */
  private playPhase3_Explosion(pixels: PixelBlock[], onComplete: () => void): void {
    // 先隐藏高亮精灵
    this.highlightSprites.forEach(sprite => {
      sprite.setVisible(false);
    });
    
    // 为每个像素块创建粒子
    pixels.forEach(pixel => {
      this.createParticlesForPixel(pixel);
    });
    
    // 启动粒子更新循环
    const updateInterval = this.scene.time.addEvent({
      delay: 16, // 约60 FPS
      callback: () => {
        this.updateParticles();
      },
      loop: true
    });
    
    // 阶段3完成后停止更新并回调
    this.scene.time.delayedCall(ANIM_CONFIG.PHASE3_DURATION, () => {
      updateInterval.remove();
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
      [Color.PURPLE]: 0xff66ff,  // 未使用
      [Color.WHITE]: 0xffffff,   // 未使用
    };
    return colors[originalColor] || 0xffffff;
  }

  /**
   * 更新所有粒子
   */
  private updateParticles(): void {
    const dt = 16 / 1000; // 约 16ms = 1/60秒
    
    for (let i = this.particles.length - 1; i >= 0; i--) {
      const particle = this.particles[i];
      
      // 更新生命周期
      particle.lifetime += 16;
      const progress = particle.lifetime / particle.maxLifetime;
      
      if (progress >= 1.0) {
        // 粒子生命结束，销毁
        particle.sprite.destroy();
        this.particles.splice(i, 1);
        continue;
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
    }
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

      // 连锁文字动画：淡入 + 放大 + 淡出（原地）
      this.scene.tweens.add({
        targets: chainText,
        alpha: 1,
        scale: 1.3,
        duration: 200,
        ease: 'Back.easeOut',
        onComplete: () => {
          this.scene.tweens.add({
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
        }
      });
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

    // 分数动画：弹出 → 停留 → 淡出（原地）
    this.scene.tweens.add({
      targets: scoreText,
      scale: 1.5,
      duration: 150,
      ease: 'Back.easeOut',
      onComplete: () => {
        // 停留一段时间
        this.scene.tweens.add({
          targets: scoreText,
          scale: 1.2,
          duration: 100,
          ease: 'Linear',
          onComplete: () => {
            // 原地淡出并放大消失
            this.scene.tweens.add({
              targets: scoreText,
              scale: 1.8,
              alpha: 0,
              duration: 400,
              ease: 'Cubic.easeOut',
              onComplete: () => {
                scoreText.destroy();
              }
            });
          }
        });
      }
    });
  }

  /**
   * 清理所有动画资源
   */
  private cleanup(): void {
    // 清理高亮精灵
    this.highlightSprites.forEach(sprite => {
      sprite.destroy();
    });
    this.highlightSprites = [];
    
    // 清理剩余粒子
    this.particles.forEach(particle => {
      particle.sprite.destroy();
    });
    this.particles = [];
    
    console.log('消除动画清理完成');
  }

  /**
   * 销毁动画系统
   */
  destroy(): void {
    this.cleanup();
    this.animationLayer.destroy();
  }
}

