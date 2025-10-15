import Phaser from 'phaser';
import { Grid } from '@/core/Grid';
import { PhysicsManager } from '@/core/PhysicsManager';
import { PixelRenderer } from '@/rendering/PixelRenderer';
import { EliminationAnimation } from '@/rendering/EliminationAnimation';
import { GameStateManager } from '@/core/GameStateManager';
import { PreviewSlots } from '@/gameplay/PreviewSlots';
import { EliminationSystem } from '@/gameplay/Elimination';
import { ScoringSystem } from '@/gameplay/Scoring';
import { DragDropManager } from '@/gameplay/DragDrop';
import { sceSDKManager } from '@/sdk/SceSDKManager';
import { GameState, PixelBlock, TetrominoData } from '@/types';
import { CELL_TO_PIXEL_RATIO, SCREEN_WIDTH, GAME_AREA_OFFSET_Y, LOGICAL_GRID_HEIGHT, LOGICAL_GRID_WIDTH, PIXEL_SIZE, UI_COLORS } from '@/config/constants';

/**
 * 主游戏场景
 * 参考设计文档第10章
 */
export class GameScene extends Phaser.Scene {
  protected grid!: Grid;
  protected physicsManager!: PhysicsManager;
  protected pixelRenderer!: PixelRenderer;
  protected eliminationAnimation!: EliminationAnimation;
  protected stateManager!: GameStateManager;
  protected previewSlots!: PreviewSlots;
  protected eliminationSystem!: EliminationSystem;
  protected scoringSystem!: ScoringSystem;
  protected dragDropManager!: DragDropManager;
  
  protected scoreText!: Phaser.GameObjects.Text;
  protected stateText!: Phaser.GameObjects.Text;
  protected chainText!: Phaser.GameObjects.Text;
  private previewSlotsUI: Phaser.GameObjects.Container[] = [];
  private nextGroupId: number = 1;
  
  // 当前拖动的方块信息（用于取消时恢复）
  private currentDraggedTetromino: TetrominoData | null = null;
  private currentDraggedSlotIndex: number = -1;

  // 自动保存相关
  private lastSaveTime: number = 0;
  private sessionStartHighestScore: number = 0;
  private autoSaveInterval: number = 3 * 60 * 1000; // 3分钟（毫秒）
  private isSaving: boolean = false;

  constructor(key: string = 'GameScene') {
    super({ key });
  }

  create(): void {
    // 🐛 修复：清空预览槽位UI数组（避免场景重启时引用旧的已销毁Container）
    this.previewSlotsUI = [];
    
    // 初始化核心系统
    this.grid = new Grid();
    this.physicsManager = new PhysicsManager(this.grid);
    this.pixelRenderer = new PixelRenderer(this, this.grid);
    this.eliminationAnimation = new EliminationAnimation(this);
    this.stateManager = new GameStateManager(GameState.READY);
    this.previewSlots = new PreviewSlots();
    this.eliminationSystem = new EliminationSystem(this.grid);
    this.scoringSystem = new ScoringSystem();
    this.dragDropManager = new DragDropManager(this, this.grid);

    // 初始化自动保存相关数据
    this.initAutoSave();

    // 创建蓝色渐变背景
    this.createGradientBackground();

    // 渲染游戏区域
    this.pixelRenderer.renderBorder();
    this.pixelRenderer.renderGrid();

    // 创建UI
    this.createUI();

    // 设置输入处理
    this.setupInput();

    // 开始游戏
    this.stateManager.setState(GameState.IDLE);

    console.log('游戏场景初始化完成');
  }

  update(): void {
    // 主游戏循环
    this.gameLoop();

    // 渲染
    this.pixelRenderer.renderPixels();

    // 更新UI
    this.updateUI();

    // ⚠️ 安全检查：如果有拖动中的方块但状态不是DRAGGING，自动恢复
    if (this.currentDraggedTetromino !== null && !this.stateManager.is(GameState.DRAGGING)) {
      console.warn('⚠️ 检测到方块丢失：状态异常，自动恢复方块到槽位');
      if (this.currentDraggedSlotIndex >= 0) {
        this.previewSlots.setSlot(this.currentDraggedSlotIndex, this.currentDraggedTetromino);
        this.updatePreviewSlotsUI();
        console.log(`✅ 已恢复方块到槽位 ${this.currentDraggedSlotIndex + 1}`);
      }
      this.currentDraggedTetromino = null;
      this.currentDraggedSlotIndex = -1;
      this.dragDropManager.cancelDrag();
    }
  }

  /**
   * 主游戏循环
   * 参考设计文档5.4.3节和第10章
   */
  private gameLoop(): void {
    const state = this.stateManager.state;

    // 在非游戏结束状态下，始终更新物理（支持并发放置）
    if (state !== GameState.GAME_OVER && state !== GameState.READY) {
      this.physicsManager.update();
    }

    if (state === GameState.PHYSICS_RUNNING) {
      // 检查是否所有像素块都已稳定
      if (this.physicsManager.allStable) {
        // 活跃像素块全部稳定，增量检查是否有新的不稳定像素块
        const beforeCount = this.physicsManager.activeCount;
        this.physicsManager.recheckStability();
        const afterCount = this.physicsManager.activeCount;
        
        if (afterCount > beforeCount) {
          // 发现新的不稳定像素块，继续物理模拟
          console.log(`发现 ${afterCount - beforeCount} 个新的不稳定像素块，继续下落`);
        } else {
          // 真正全部稳定，进入消除检测
          this.stateManager.setState(GameState.CHECKING_ELIMINATION);
        }
      }
    } else if (state === GameState.CHECKING_ELIMINATION) {
      // 检查消除
      this.checkAndPerformElimination();
    }
  }

  /**
   * 检查并执行消除
   * 参考设计文档第8章
   * 带动画版本：先播放动画，动画完成后再删除像素块
   */
  private checkAndPerformElimination(): void {
    const eliminationResults = this.eliminationSystem.checkElimination();

    if (eliminationResults.length > 0) {
      console.log(`发现 ${eliminationResults.length} 个可消除集群`);
      console.log(`消除前像素块总数: ${this.grid.getAllPixels().length}`);

      // 进入消除动画状态
      this.stateManager.setState(GameState.ELIMINATING);

      // 收集所有要消除的像素块
      let totalCells = 0;
      let totalPixels = 0;
      const allPixelsToEliminate: PixelBlock[] = [];
      
      eliminationResults.forEach((result) => {
        totalCells += result.cluster.cells.length;
        totalPixels += result.pixels.length;
        console.log(`消除集群: ${result.cluster.cells.length}个逻辑格子, ${result.pixels.length}个像素块`);
        allPixelsToEliminate.push(...result.pixels);
      });

      // 提前计算分数和连锁等级（用于动画显示）
      const baseScore = this.scoringSystem.calculateBaseScore(totalCells);
      const nextChainLevel = this.scoringSystem.chainLevel + 1; // 下一个连锁等级
      const score = baseScore * nextChainLevel;

      // 播放消除动画（传递分数和连锁信息）
      this.eliminationAnimation.playEliminationAnimation(
        allPixelsToEliminate, 
        score, 
        nextChainLevel, 
        () => {
          // 动画完成后的回调：删除像素块并触发重力
          
          // 删除像素块
          eliminationResults.forEach((result) => {
            this.eliminationSystem.eliminatePixels(result.pixels);
          });

          console.log(`消除后像素块总数: ${this.grid.getAllPixels().length}`);

          // 正式记录分数（增加连锁）
          this.scoringSystem.addEliminationScore(totalCells, true);
          console.log(`消除 ${totalCells} 格 (${totalPixels}像素)，得分 ${score}`);

          // 检查是否需要自动保存
          this.checkAutoSave();

        // 重新检查稳定性并触发重力
        this.physicsManager.recheckStability();
        console.log(`切换到物理运行状态，活跃像素块: ${this.physicsManager.activeCount}`);
        this.stateManager.setState(GameState.PHYSICS_RUNNING);
      });
    } else {
      // 无消除，重置连锁并返回空闲状态
      this.scoringSystem.resetChain();
      this.checkGameOver();
      this.stateManager.setState(GameState.IDLE);
    }
  }

  /**
   * 检查游戏结束
   * 参考设计文档第11章
   */
  protected checkGameOver(): void {
    const canPlace = this.previewSlots.hasAnyPlaceableBlock((tetromino) => {
      return this.canPlaceAnywhere(tetromino);
    });

    if (!canPlace) {
      this.stateManager.setState(GameState.GAME_OVER);
      console.log('游戏结束！最终分数:', this.scoringSystem.score);
      this.showGameOver();
    }
  }

  /**
   * 检查方块是否可以放置在任意位置
   */
  protected canPlaceAnywhere(tetromino: TetrominoData): boolean {
    for (let y = 0; y < LOGICAL_GRID_HEIGHT; y++) {
      for (let x = 0; x < LOGICAL_GRID_WIDTH; x++) {
        if (this.canPlaceTetromino(tetromino, x, y)) {
          return true;
        }
      }
    }
    return false;
  }

  /**
   * 创建UI（Dark + Neon 卡片式设计）
   */
  private createUI(): void {
    // 创建顶部信息栏容器
    const infoBarY = 40;
    const cardWidth = (SCREEN_WIDTH - 48) / 2; // 16px 边距 + 16px 间距

    // 左侧卡片 - 分数
    const scoreCard = this.add.graphics();
    scoreCard.fillStyle(UI_COLORS.BG_SECONDARY, 1);
    scoreCard.fillRoundedRect(16, infoBarY, cardWidth, 72, 12);
    scoreCard.lineStyle(2, UI_COLORS.BORDER_GLOW, 0.3);
    scoreCard.strokeRoundedRect(16, infoBarY, cardWidth, 72, 12);

    // 分数标签
    this.add.text(32, infoBarY + 16, '分数', {
      fontSize: '16px',
      color: '#9aa4b2',
      fontFamily: 'Arial',
    });

    // 分数数值（大号、醒目）
    this.scoreText = this.add.text(32, infoBarY + 36, '0', {
      fontSize: '32px',
      color: '#ffffff',
      fontFamily: 'Arial, sans-serif',
      fontStyle: '700',
    });

    // 右侧卡片 - 状态/连锁
    const statusCard = this.add.graphics();
    statusCard.fillStyle(UI_COLORS.BG_SECONDARY, 1);
    statusCard.fillRoundedRect(cardWidth + 32, infoBarY, cardWidth, 72, 12);
    statusCard.lineStyle(2, UI_COLORS.BORDER_GLOW, 0.3);
    statusCard.strokeRoundedRect(cardWidth + 32, infoBarY, cardWidth, 72, 12);

    // 状态标签
    this.add.text(cardWidth + 48, infoBarY + 16, '状态', {
      fontSize: '16px',
      color: '#9aa4b2',
      fontFamily: 'Arial',
    });

    // 状态文本
    this.stateText = this.add.text(cardWidth + 48, infoBarY + 36, '空闲', {
      fontSize: '20px',
      color: '#ffffff',
      fontFamily: 'Arial',
    });

    // 连锁显示（浮动提示，初始隐藏）
    this.chainText = this.add.text(SCREEN_WIDTH / 2, infoBarY + 130, '', {
      fontSize: '28px',
      color: '#fbbf24',
      fontFamily: 'Arial',
      fontStyle: 'bold',
    });
    this.chainText.setOrigin(0.5, 0.5);
    this.chainText.setVisible(false);

    // 预览槽位UI
    this.createPreviewSlotsUI();

    // 底部按钮栏
    this.createBottomButtons();
  }

  /**
   * 创建底部按钮栏（扁平简洁设计，靠两侧）
   */
  private createBottomButtons(): void {
    const buttonY = 1180; // 底部位置（调整为更靠下，距离底部约100px）
    const leftX = 80; // 左侧按钮位置
    const rightX = SCREEN_WIDTH - 80; // 右侧按钮位置

    // 返回按钮（左侧 - 次要操作）
    this.createIconOnlyButton(
      leftX,
      buttonY,
      '←',
      UI_COLORS.TEXT_SECONDARY, // 中灰蓝（次要）
      () => {
        this.cameras.main.fadeOut(300);
        this.time.delayedCall(300, () => {
          this.scene.start('StartScene');
        });
      }
    );

    // 重新开始按钮（右侧 - 主要操作）
    this.createIconOnlyButton(
      rightX,
      buttonY,
      '↻',
      UI_COLORS.ACCENT_PRIMARY, // 霓虹蓝（主要）
      () => {
        this.cameras.main.fadeOut(300);
        this.time.delayedCall(300, () => {
          this.scene.restart();
        });
      }
    );
  }

  /**
   * 创建纯图标按钮（Dark + Neon 霓虹风格）
   */
  protected createIconOnlyButton(
    x: number,
    y: number,
    icon: string,
    color: number,
    callback: () => void
  ): Phaser.GameObjects.Container {
    const container = this.add.container(x, y);
    const buttonRadius = 32;

    // 深色阴影
    const shadow = this.add.circle(3, 3, buttonRadius, UI_COLORS.SHADOW_DEEP, 0.5);
    shadow.setName('shadow');

    // 外发光层（霓虹效果）
    const glow = this.add.circle(0, 0, buttonRadius + 4, color, 0.25);
    glow.setName('glow');

    // 主按钮背景（扁平纯色）
    const bg = this.add.graphics();
    bg.fillStyle(UI_COLORS.BG_SECONDARY, 1);
    bg.fillCircle(0, 0, buttonRadius);

    // 霓虹边框
    bg.lineStyle(2, color, 0.6);
    bg.strokeCircle(0, 0, buttonRadius);
    bg.setName('bg');

    // 图标（带阴影）
    const iconText = this.add.text(0, 0, icon, {
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
      this.tweens.add({
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
      this.tweens.add({
        targets: container,
        scaleX: 1.05,
        scaleY: 1.05,
        duration: 200,
        ease: 'Back.easeOut'
      });

      // 发光脉冲动画
      const glowCircle = container.getByName('glow') as Phaser.GameObjects.Arc;
      if (glowCircle) {
        this.tweens.add({
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

      this.tweens.add({
        targets: container,
        scaleX: 1.0,
        scaleY: 1.0,
        duration: 200,
        ease: 'Back.easeIn'
      });
    });

    return container;
  }

  /**
   * 创建预览槽位UI（现代化设计）
   */
  private createPreviewSlotsUI(): void {
    // 游戏区域底部位置：GAME_AREA_OFFSET_Y + GAME_AREA_HEIGHT
    const gameAreaBottom = GAME_AREA_OFFSET_Y + (LOGICAL_GRID_HEIGHT * CELL_TO_PIXEL_RATIO * PIXEL_SIZE);
    const slotY = gameAreaBottom + 165; // 游戏区域底部 + 165px间距（调整为更靠下）
    const slotSize = 160; // 槽位大小（适配新屏幕）
    const slotSpacing = 30; // 槽位之间的间距

    // 计算3个槽位的总宽度并居中
    const totalWidth = slotSize * 3 + slotSpacing * 2;
    const startX = (SCREEN_WIDTH - totalWidth) / 2 + slotSize / 2; // 居中对齐，加上半个槽位偏移

    for (let i = 0; i < 3; i++) {
      const slotX = startX + i * (slotSize + slotSpacing);

      // 创建槽位容器
      const container = this.add.container(slotX, slotY);

      // 深色阴影
      const shadowBg = this.add.graphics();
      shadowBg.fillStyle(UI_COLORS.SHADOW_DEEP, 0.4);
      shadowBg.fillRoundedRect(-slotSize/2 + 4, -slotSize/2 + 4, slotSize, slotSize, 16);
      shadowBg.setName('shadow');

      // 主背景（霓虹风格）
      const mainBg = this.add.graphics();
      mainBg.fillStyle(UI_COLORS.BG_SECONDARY, 1);
      mainBg.fillRoundedRect(-slotSize/2, -slotSize/2, slotSize, slotSize, 16);

      // 霓虹边框
      mainBg.lineStyle(2, UI_COLORS.BORDER_GLOW, 0.4);
      mainBg.strokeRoundedRect(-slotSize/2, -slotSize/2, slotSize, slotSize, 16);

      mainBg.setName('mainBg');

      // 外发光效果（悬停状态，初始隐藏）
      const glowBg = this.add.graphics();
      glowBg.lineStyle(4, UI_COLORS.BORDER_GLOW, 0);
      glowBg.strokeRoundedRect(-slotSize/2 - 2, -slotSize/2 - 2, slotSize + 4, slotSize + 4, 14);
      glowBg.setName('glow');

      container.add([shadowBg, mainBg, glowBg]);

      // 交互区域（透明矩形，用于检测点击）
      const hitArea = this.add.rectangle(0, 0, slotSize, slotSize, 0x000000, 0);
      hitArea.setInteractive({ useHandCursor: true });

      // 悬停效果
      hitArea.on('pointerover', () => {
        const glow = container.getByName('glow') as Phaser.GameObjects.Graphics;
        if (glow) {
          glow.clear();
          glow.lineStyle(4, UI_COLORS.BORDER_GLOW, 0.6);
          glow.strokeRoundedRect(-slotSize/2 - 2, -slotSize/2 - 2, slotSize + 4, slotSize + 4, 14);
        }

        this.tweens.add({
          targets: container,
          y: slotY - 8,
          scaleX: 1.05,
          scaleY: 1.05,
          duration: 150,
          ease: 'Back.easeOut'
        });
      });

      hitArea.on('pointerout', () => {
        const glow = container.getByName('glow') as Phaser.GameObjects.Graphics;
        if (glow) {
          glow.clear();
          glow.lineStyle(4, UI_COLORS.BORDER_GLOW, 0);
          glow.strokeRoundedRect(-slotSize/2 - 2, -slotSize/2 - 2, slotSize + 4, slotSize + 4, 14);
        }

        this.tweens.add({
          targets: container,
          y: slotY,
          scaleX: 1.0,
          scaleY: 1.0,
          duration: 150,
          ease: 'Back.easeIn'
        });
      });

      // 槽位点击事件
      hitArea.on('pointerdown', () => {
        this.onSlotClicked(i);
      });

      container.add(hitArea);

      this.previewSlotsUI.push(container);
    }

    // 更新槽位显示
    this.updatePreviewSlotsUI();
  }

  /**
   * 更新预览槽位UI
   */
  protected updatePreviewSlotsUI(): void {
    const slots = this.previewSlots.getAllSlots();

    slots.forEach((tetromino, index) => {
      const container = this.previewSlotsUI[index];
      if (!container || !tetromino) return;

      // 清除旧的方块显示
      // 新的容器结构：shadowBg, mainBg, glowBg, hitArea (共4个固定元素)
      // 从第5个元素开始删除（索引4开始）
      const itemsToRemove: Phaser.GameObjects.GameObject[] = [];
      for (let i = 4; i < container.length; i++) {
        itemsToRemove.push(container.list[i]);
      }
      itemsToRemove.forEach(item => {
        item.destroy();
      });

      // 绘制方块预览（槽位160px，方块也相应放大）
      const cellSize = 32; // 方块格子大小（槽位160 / 5 ≈ 32）

      // 计算方块的边界框以实现真正的居中
      let minX = Infinity, maxX = -Infinity;
      let minY = Infinity, maxY = -Infinity;
      tetromino.cells.forEach(cell => {
        minX = Math.min(minX, cell.x);
        maxX = Math.max(maxX, cell.x);
        minY = Math.min(minY, cell.y);
        maxY = Math.max(maxY, cell.y);
      });

      // 计算方块的中心点（相对于方块自身坐标系）
      const blockCenterX = (minX + maxX) / 2 * cellSize + cellSize / 2;
      const blockCenterY = (minY + maxY) / 2 * cellSize + cellSize / 2;

      // 计算偏移量使方块居中在槽位中（容器中心为0,0）
      const offsetX = -blockCenterX;
      const offsetY = -blockCenterY;

      tetromino.cells.forEach((cell) => {
        const rect = this.add.rectangle(
          cell.x * cellSize + offsetX + cellSize / 2,
          cell.y * cellSize + offsetY + cellSize / 2,
          cellSize - 2,
          cellSize - 2,
          tetromino.color
        );
        rect.setStrokeStyle(2, 0xffffff, 0.7); // 加粗边框
        container.add(rect);
      });
    });
  }

  /**
   * 槽位点击处理
   */
  protected onSlotClicked(slotIndex: number): void {
    console.log(`点击槽位 ${slotIndex + 1}`);

    // ⚠️ 安全检查1：确保当前没有正在拖动的方块
    if (this.currentDraggedTetromino !== null) {
      console.warn('已有方块正在拖动，忽略点击');
      return;
    }

    if (!this.stateManager.canPlayerPlaceBlock()) {
      console.log('当前状态不允许放置方块:', this.stateManager.state);
      return;
    }

    // 获取槽位中的方块（但不立即补充新方块）
    const tetromino = this.previewSlots.getSlot(slotIndex);
    if (!tetromino) {
      console.error('槽位为空！这可能是之前拖动未正确恢复导致的');
      return;
    }

    console.log(`获取到方块: ${tetromino.shape}, 颜色: ${tetromino.color}`);

    // 保存拖动信息（用于取消或失败时恢复）
    this.currentDraggedTetromino = tetromino;
    this.currentDraggedSlotIndex = slotIndex;

    // 清空该槽位（拖动中显示为空）
    this.previewSlots.setSlot(slotIndex, null);
    this.updatePreviewSlotsUI();

    // 开始拖动
    this.dragDropManager.startDrag(tetromino, slotIndex);
    this.stateManager.setState(GameState.DRAGGING);
    
    console.log(`拖动槽位${slotIndex + 1}的方块: ${tetromino.shape}, 状态已切换到DRAGGING`);
  }

  /**
   * 设置输入处理
   */
  private setupInput(): void {
    // 清除旧的事件监听器（避免重复绑定）
    this.input.off('pointermove');
    this.input.off('pointerup');
    
    // 鼠标/触摸移动
    this.input.on('pointermove', (pointer: Phaser.Input.Pointer) => {
      const state = this.stateManager.state;
      if (state === GameState.DRAGGING) {
        this.dragDropManager.updateDrag(pointer.x, pointer.y);
      }
    });

    // 鼠标/触摸松开
    this.input.on('pointerup', () => {
      if (this.stateManager.is(GameState.DRAGGING)) {
        const result = this.dragDropManager.endDrag();

        if (result.success && result.tetromino && result.position) {
          // 放置成功
          this.placeTetromino(result.tetromino, result.position.x, result.position.y);
          
          // 补充该槽位的新方块
          if (this.currentDraggedSlotIndex >= 0) {
            this.previewSlots.refillSlotAfterPlace(this.currentDraggedSlotIndex);
            this.updatePreviewSlotsUI();
            console.log(`放置成功，槽位${this.currentDraggedSlotIndex + 1}已补充新方块`);
          }
          
          this.currentDraggedTetromino = null;
          this.currentDraggedSlotIndex = -1;

          // 切换到物理运行状态
          this.stateManager.setState(GameState.PHYSICS_RUNNING);
          
          console.log('放置成功，方块开始下落');
        } else {
          // 放置失败，把方块放回槽位
          if (this.currentDraggedTetromino && this.currentDraggedSlotIndex >= 0) {
            this.previewSlots.setSlot(this.currentDraggedSlotIndex, this.currentDraggedTetromino);
            this.updatePreviewSlotsUI();
            console.log('放置失败，方块已放回槽位');
          }
          this.currentDraggedTetromino = null;
          this.currentDraggedSlotIndex = -1;
          this.stateManager.setState(GameState.IDLE);
        }
      }
    });

    // ESC键取消拖动
    this.input.keyboard?.on('keydown-ESC', () => {
      if (this.stateManager.is(GameState.DRAGGING)) {
        // 取消拖动，把方块放回槽位
        if (this.currentDraggedTetromino && this.currentDraggedSlotIndex >= 0) {
          this.previewSlots.setSlot(this.currentDraggedSlotIndex, this.currentDraggedTetromino);
          this.updatePreviewSlotsUI();
          console.log('取消拖动，方块已放回槽位');
        }
        this.dragDropManager.cancelDrag();
        this.currentDraggedTetromino = null;
        this.currentDraggedSlotIndex = -1;
        this.stateManager.setState(GameState.IDLE);
      }
    });

    // R键手动触发重力重新检查（调试用）
    this.input.keyboard?.on('keydown-R', () => {
      console.log('=== 手动触发重力重新检查 ===');
      this.physicsManager.recheckStability();
      if (this.physicsManager.activeCount > 0) {
        this.stateManager.setState(GameState.PHYSICS_RUNNING);
        console.log('切换到物理运行状态');
      } else {
        console.log('没有不稳定的像素块');
      }
    });

    // G键显示网格调试信息（调试用）
    this.input.keyboard?.on('keydown-G', () => {
      const allPixels = this.grid.getAllPixels();
      const stablePixels = allPixels.filter(p => p.isStable);
      const unstablePixels = allPixels.filter(p => !p.isStable);
      console.log(`=== 网格调试信息 ===`);
      console.log(`总像素块: ${allPixels.length}`);
      console.log(`稳定: ${stablePixels.length}`);
      console.log(`不稳定: ${unstablePixels.length}`);
      console.log(`活跃集合: ${this.physicsManager.activeCount}`);
    });
  }

  /**
   * 检查是否可以放置方块
   * 参考设计文档6.3.2节
   */
  private canPlaceTetromino(
    tetromino: TetrominoData,
    logicalX: number,
    logicalY: number
  ): boolean {
    for (const cell of tetromino.cells) {
      const checkX = logicalX + cell.x;
      const checkY = logicalY + cell.y;

      // 越界检查
      if (!this.grid.isValidLogicalPosition(checkX, checkY)) {
        return false;
      }

      // 重叠检查
      if (!this.grid.isLogicalCellEmpty(checkX, checkY)) {
        return false;
      }
    }
    return true;
  }

  /**
   * 放置方块
   * 参考设计文档6.3.3节
   */
  protected placeTetromino(
    tetromino: TetrominoData,
    logicalX: number,
    logicalY: number
  ): void {
    const groupId = this.nextGroupId++;
    const pixels: PixelBlock[] = [];

    // 将方块的每个逻辑格子转换为像素块
    for (const cell of tetromino.cells) {
      const cellX = logicalX + cell.x;
      const cellY = logicalY + cell.y;
      const pixelPos = this.grid.logicalToPixel(cellX, cellY);

      // 创建10×10的像素块
      for (let py = 0; py < CELL_TO_PIXEL_RATIO; py++) {
        for (let px = 0; px < CELL_TO_PIXEL_RATIO; px++) {
          const pixel: PixelBlock = {
            x: pixelPos.x + px,
            y: pixelPos.y + py,
            color: tetromino.color,
            isStable: false,
            groupId: groupId,
          };

          this.grid.setPixel(pixel.x, pixel.y, pixel);
          pixels.push(pixel);
        }
      }
    }

    // 添加到物理系统
    this.physicsManager.addPixels(pixels);

    console.log(`放置方块 ${tetromino.shape}，创建了 ${pixels.length} 个像素块`);
  }

  /**
   * 更新UI显示
   */
  private updateUI(): void {
    this.scoreText.setText(`${this.scoringSystem.score}`);

    const chainLevel = this.scoringSystem.chainLevel;
    if (chainLevel > 1) {
      this.chainText.setText(`连锁 x${chainLevel}!`);
      this.chainText.setVisible(true);
    } else {
      this.chainText.setVisible(false);
    }

    let stateStr = '';
    const state = this.stateManager.state;
    switch (state) {
      case GameState.IDLE:
        stateStr = '空闲 - 点击槽位放置方块';
        break;
      case GameState.DRAGGING:
        stateStr = '拖动中';
        break;
      case GameState.PHYSICS_RUNNING:
        stateStr = `物理模拟中 (${this.physicsManager.activeCount}个活跃像素)`;
        break;
      case GameState.CHECKING_ELIMINATION:
        stateStr = '检查消除';
        break;
      case GameState.GAME_OVER:
        stateStr = '游戏结束';
        break;
      default:
        stateStr = state;
    }

    this.stateText.setText(`${stateStr}`);
  }

  /**
   * 显示游戏结束界面
   */
  private async showGameOver(): Promise<void> {
    const centerX = SCREEN_WIDTH / 2;
    const centerY = 600;
    const finalScore = this.scoringSystem.score;

    // 上传分数并获取结果
    const uploadResult = await sceSDKManager.uploadScore(finalScore);
    const rankings = await sceSDKManager.getRankings(5); // 获取前5名
    const playerRank = await sceSDKManager.getPlayerRank();

    // 半透明背景
    this.add.rectangle(
      centerX,
      centerY,
      700,
      800,
      0x000000,
      0.9
    );

    // 游戏结束文本
    const gameOverText = this.add.text(centerX, centerY - 350, '游戏结束', {
      fontSize: '48px',
      color: '#ff0000',
      fontFamily: 'Arial',
      fontStyle: 'bold',
    });
    gameOverText.setOrigin(0.5);

    // 最终分数
    const finalScoreText = this.add.text(
      centerX,
      centerY - 270,
      `本次分数: ${finalScore}`,
      {
        fontSize: '32px',
        color: '#ffffff',
        fontFamily: 'Arial',
      }
    );
    finalScoreText.setOrigin(0.5);

    // 最高分显示
    const highestScoreText = this.add.text(
      centerX,
      centerY - 220,
      `最高分: ${uploadResult.currentHighest}`,
      {
        fontSize: '28px',
        color: '#ffff00',
        fontFamily: 'Arial',
        fontStyle: 'bold',
      }
    );
    highestScoreText.setOrigin(0.5);

    // 新记录提示
    if (uploadResult.isNewRecord) {
      const newRecordText = this.add.text(
        centerX,
        centerY - 170,
        '🎉 新纪录！ 🎉',
        {
          fontSize: '24px',
          color: '#00ff00',
          fontFamily: 'Arial',
          fontStyle: 'bold',
        }
      );
      newRecordText.setOrigin(0.5);

      // 闪烁动画
      this.tweens.add({
        targets: newRecordText,
        alpha: 0.3,
        duration: 500,
        yoyo: true,
        repeat: -1,
        ease: 'Sine.easeInOut'
      });
    }

    // 玩家排名
    if (playerRank > 0) {
      const rankText = this.add.text(
        centerX,
        centerY - 140,
        `你的排名: 第 ${playerRank} 名`,
        {
          fontSize: '22px',
          color: '#00ffff',
          fontFamily: 'Arial',
        }
      );
      rankText.setOrigin(0.5);
    }

    // 排行榜标题
    const rankTitleText = this.add.text(
      centerX,
      centerY - 90,
      '── 排行榜 TOP 5 ──',
      {
        fontSize: '24px',
        color: '#ffffff',
        fontFamily: 'Arial',
        fontStyle: 'bold',
      }
    );
    rankTitleText.setOrigin(0.5);

    // 显示排行榜
    if (rankings.length > 0) {
      rankings.forEach((ranking, index) => {
        const yPos = centerY - 40 + index * 50;
        
        // 排名颜色
        let rankColor = '#ffffff';
        if (ranking.rank === 1) rankColor = '#ffd700'; // 金色
        else if (ranking.rank === 2) rankColor = '#c0c0c0'; // 银色
        else if (ranking.rank === 3) rankColor = '#cd7f32'; // 铜色

        const rankItemText = this.add.text(
          centerX,
          yPos,
          `${ranking.rank}. ${ranking.username}: ${ranking.score}`,
          {
            fontSize: '20px',
            color: rankColor,
            fontFamily: 'Arial',
          }
        );
        rankItemText.setOrigin(0.5);
      });
    } else {
      const noDataText = this.add.text(
        centerX,
        centerY,
        '暂无排行榜数据',
        {
          fontSize: '18px',
          color: '#888888',
          fontFamily: 'Arial',
        }
      );
      noDataText.setOrigin(0.5);
    }

    // 重新开始按钮
    const restartButton = this.add.text(centerX - 120, centerY + 280, '再来一局', {
      fontSize: '28px',
      color: '#00ff00',
      fontFamily: 'Arial',
      fontStyle: 'bold',
    });
    restartButton.setOrigin(0.5);
    restartButton.setInteractive({ useHandCursor: true });
    restartButton.on('pointerdown', () => {
      this.cameras.main.fadeOut(500);
      this.time.delayedCall(500, () => {
        this.scene.restart();
      });
    });

    // 悬停效果
    restartButton.on('pointerover', () => {
      restartButton.setScale(1.1);
    });
    restartButton.on('pointerout', () => {
      restartButton.setScale(1.0);
    });

    // 返回首页按钮
    const homeButton = this.add.text(centerX + 120, centerY + 280, '返回首页', {
      fontSize: '28px',
      color: '#00aaff',
      fontFamily: 'Arial',
      fontStyle: 'bold',
    });
    homeButton.setOrigin(0.5);
    homeButton.setInteractive({ useHandCursor: true });
    homeButton.on('pointerdown', () => {
      this.cameras.main.fadeOut(500);
      this.time.delayedCall(500, () => {
        this.scene.start('StartScene');
      });
    });

    // 悬停效果
    homeButton.on('pointerover', () => {
      homeButton.setScale(1.1);
    });
    homeButton.on('pointerout', () => {
      homeButton.setScale(1.0);
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
    bg.fillRect(0, 0, SCREEN_WIDTH, 1280);
    bg.setDepth(-100);
  }

  /**
   * 初始化自动保存数据
   */
  private initAutoSave(): void {
    // 记录游戏开始时的最高分
    sceSDKManager.getHighestScore().then(score => {
      this.sessionStartHighestScore = score;
      console.log(`游戏开始，当前最高分: ${this.sessionStartHighestScore}`);
    }).catch(error => {
      console.warn('获取最高分失败:', error);
      this.sessionStartHighestScore = 0;
    });

    // 初始化保存时间为当前时间
    this.lastSaveTime = Date.now();
  }

  /**
   * 检查并执行自动保存
   * 条件：
   * 1. 当前分数超过最高分
   * 2. 距离上次保存超过3分钟
   */
  private checkAutoSave(): void {
    // 避免重复保存
    if (this.isSaving) {
      return;
    }

    const currentScore = this.scoringSystem.score;
    const currentTime = Date.now();
    const timeSinceLastSave = currentTime - this.lastSaveTime;

    // 检查条件1：当前分数是否超过最高分
    const hasNewRecord = currentScore > this.sessionStartHighestScore;

    // 检查条件2：是否超过3分钟
    const shouldSave = timeSinceLastSave >= this.autoSaveInterval;

    if (hasNewRecord && shouldSave) {
      console.log(`🔄 触发自动保存：当前分数 ${currentScore} > 最高分 ${this.sessionStartHighestScore}，距上次保存 ${Math.floor(timeSinceLastSave / 1000)}秒`);
      
      this.isSaving = true;

      // 异步保存到云端（不阻塞游戏）
      // 传入 sessionStartHighestScore 避免重复查询
      sceSDKManager.saveHighestScore(currentScore, this.sessionStartHighestScore)
        .then(success => {
          if (success) {
            console.log(`✅ 自动保存成功！分数 ${currentScore} 已保存到云端`);
            // 更新会话最高分和保存时间
            this.sessionStartHighestScore = currentScore;
            this.lastSaveTime = Date.now();
          } else {
            console.log(`⏭️ 自动保存跳过（分数未超过已知最高分）`);
          }
        })
        .catch(error => {
          console.error('自动保存失败:', error);
        })
        .finally(() => {
          this.isSaving = false;
        });
    }
  }
}

