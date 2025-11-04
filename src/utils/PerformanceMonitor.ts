/**
 * 性能监控工具
 * 用于测试和追踪游戏性能
 */

export interface PerformanceMetrics {
  fps: number;
  frameTime: number; // ms
  renderTime: number; // ms
  updateTime: number; // ms
  pixelCount: number;
  memoryUsage?: number; // MB
}

export class PerformanceMonitor {
  private scene: Phaser.Scene;
  private fpsText?: Phaser.GameObjects.Text;
  private metricsText?: Phaser.GameObjects.Text;
  private enabled: boolean = false;

  // 性能数据
  private frameTimeHistory: number[] = [];
  private renderTimeHistory: number[] = [];
  private updateTimeHistory: number[] = [];
  private maxHistoryLength = 60; // 保存最近60帧

  // 计时器
  private lastFrameTime: number = 0;
  private renderStartTime: number = 0;
  private updateStartTime: number = 0;

  constructor(scene: Phaser.Scene) {
    this.scene = scene;
  }

  /**
   * 切换性能监控显示
   */
  toggle(): void {
    this.enabled = !this.enabled;

    if (this.enabled) {
      this.createUI();
    } else {
      this.destroyUI();
    }
  }

  /**
   * 创建性能监控 UI
   */
  private createUI(): void {
    // FPS 显示（右上角）
    this.fpsText = this.scene.add.text(
      this.scene.scale.width - 10,
      10,
      'FPS: --',
      {
        fontSize: '32px', // 增大字体
        color: '#00ff00',
        fontFamily: 'monospace',
        backgroundColor: '#000000dd', // 更深的背景
        padding: { x: 12, y: 6 },
      }
    );
    this.fpsText.setOrigin(1, 0);
    this.fpsText.setDepth(10000);
    this.fpsText.setScrollFactor(0);

    // 详细指标显示（右上角下方）
    this.metricsText = this.scene.add.text(
      this.scene.scale.width - 10,
      55, // 调整位置
      '',
      {
        fontSize: '18px', // 增大字体
        color: '#ffffff',
        fontFamily: 'monospace',
        backgroundColor: '#000000cc',
        padding: { x: 10, y: 6 },
        lineSpacing: 4, // 增加行距
      }
    );
    this.metricsText.setOrigin(1, 0);
    this.metricsText.setDepth(10000);
    this.metricsText.setScrollFactor(0);
  }

  /**
   * 销毁性能监控 UI
   */
  private destroyUI(): void {
    if (this.fpsText) {
      this.fpsText.destroy();
      this.fpsText = undefined;
    }
    if (this.metricsText) {
      this.metricsText.destroy();
      this.metricsText = undefined;
    }
  }

  /**
   * 标记渲染开始
   */
  markRenderStart(): void {
    if (!this.enabled) return;
    this.renderStartTime = performance.now();
  }

  /**
   * 标记渲染结束
   */
  markRenderEnd(): void {
    if (!this.enabled) return;
    const renderTime = performance.now() - this.renderStartTime;
    this.renderTimeHistory.push(renderTime);
    if (this.renderTimeHistory.length > this.maxHistoryLength) {
      this.renderTimeHistory.shift();
    }
  }

  /**
   * 标记更新开始
   */
  markUpdateStart(): void {
    if (!this.enabled) return;
    this.updateStartTime = performance.now();
  }

  /**
   * 标记更新结束
   */
  markUpdateEnd(): void {
    if (!this.enabled) return;
    const updateTime = performance.now() - this.updateStartTime;
    this.updateTimeHistory.push(updateTime);
    if (this.updateTimeHistory.length > this.maxHistoryLength) {
      this.updateTimeHistory.shift();
    }
  }

  /**
   * 更新显示
   */
  update(pixelCount: number): void {
    if (!this.enabled) return;

    // 计算帧时间
    const now = performance.now();
    const frameTime = now - this.lastFrameTime;
    this.lastFrameTime = now;

    this.frameTimeHistory.push(frameTime);
    if (this.frameTimeHistory.length > this.maxHistoryLength) {
      this.frameTimeHistory.shift();
    }

    // 更新显示
    this.updateDisplay(pixelCount);
  }

  /**
   * 更新显示内容
   */
  private updateDisplay(pixelCount: number): void {
    if (!this.fpsText || !this.metricsText) return;

    // 计算平均值和最大值
    const avgFrameTime = this.average(this.frameTimeHistory);
    const maxFrameTime = Math.max(...this.frameTimeHistory);
    const avgRenderTime = this.average(this.renderTimeHistory);
    const avgUpdateTime = this.average(this.updateTimeHistory);
    const fps = Math.round(1000 / avgFrameTime);

    // 计算其他开销（Phaser 内部、Tween、对象创建等）
    const otherTime = avgFrameTime - avgRenderTime - avgUpdateTime;

    // 获取活跃的 Tween 数量
    const activeTweenCount = this.scene.tweens.getTweens().length;

    // FPS 显示（根据性能着色）
    let fpsColor = '#00ff00'; // 绿色
    if (fps < 45) fpsColor = '#ffff00'; // 黄色
    if (fps < 30) fpsColor = '#ff0000'; // 红色

    this.fpsText.setText(`FPS: ${fps}`);
    this.fpsText.setColor(fpsColor);

    // 详细指标
    const metrics = [
      `帧时间: ${avgFrameTime.toFixed(1)}ms (峰: ${maxFrameTime.toFixed(1)}ms)`,
      `├ 更新: ${avgUpdateTime.toFixed(1)}ms`,
      `├ 渲染: ${avgRenderTime.toFixed(1)}ms`,
      `└ 其他: ${otherTime.toFixed(1)}ms`,
      `像素: ${pixelCount}`,
      `Tween: ${activeTweenCount}`, // 显示活跃 Tween 数量
    ];

    // 添加内存信息（如果可用）
    if (performance.memory) {
      const memoryMB = (performance.memory.usedJSHeapSize / 1024 / 1024).toFixed(1);
      metrics.push(`内存: ${memoryMB}MB`);
    }

    this.metricsText.setText(metrics.join('\n'));
  }

  /**
   * 计算数组平均值
   */
  private average(arr: number[]): number {
    if (arr.length === 0) return 0;
    return arr.reduce((a, b) => a + b, 0) / arr.length;
  }

  /**
   * 获取当前性能指标
   */
  getMetrics(): PerformanceMetrics {
    const avgFrameTime = this.average(this.frameTimeHistory);
    const avgRenderTime = this.average(this.renderTimeHistory);
    const avgUpdateTime = this.average(this.updateTimeHistory);

    return {
      fps: Math.round(1000 / avgFrameTime),
      frameTime: avgFrameTime,
      renderTime: avgRenderTime,
      updateTime: avgUpdateTime,
      pixelCount: 0, // 需要外部传入
      memoryUsage: performance.memory
        ? performance.memory.usedJSHeapSize / 1024 / 1024
        : undefined,
    };
  }

  /**
   * 生成性能报告
   */
  generateReport(pixelCount: number): string {
    const metrics = this.getMetrics();
    metrics.pixelCount = pixelCount;

    const report = [
      '=== 性能报告 ===',
      `FPS: ${metrics.fps}`,
      `平均帧时间: ${metrics.frameTime.toFixed(2)}ms`,
      `平均渲染时间: ${metrics.renderTime.toFixed(2)}ms`,
      `平均更新时间: ${metrics.updateTime.toFixed(2)}ms`,
      `像素块数量: ${metrics.pixelCount}`,
    ];

    if (metrics.memoryUsage) {
      report.push(`内存使用: ${metrics.memoryUsage.toFixed(1)}MB`);
    }

    // 性能评估
    report.push('');
    report.push('=== 性能评估 ===');
    if (metrics.fps >= 55) {
      report.push('✅ 优秀 - 性能非常流畅');
    } else if (metrics.fps >= 45) {
      report.push('✅ 良好 - 性能流畅');
    } else if (metrics.fps >= 30) {
      report.push('⚠️ 一般 - 可能有轻微卡顿');
    } else {
      report.push('❌ 较差 - 存在明显卡顿');
    }

    // 渲染性能评估
    if (metrics.renderTime < 5) {
      report.push('✅ 渲染性能: 优秀');
    } else if (metrics.renderTime < 10) {
      report.push('✅ 渲染性能: 良好');
    } else {
      report.push('⚠️ 渲染性能: 需要优化');
    }

    return report.join('\n');
  }

  /**
   * 销毁
   */
  destroy(): void {
    this.destroyUI();
    this.frameTimeHistory = [];
    this.renderTimeHistory = [];
    this.updateTimeHistory = [];
  }
}
