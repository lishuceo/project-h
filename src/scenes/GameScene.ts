import Phaser from 'phaser';
import { Grid } from '@/core/Grid';
import { PhysicsManager } from '@/core/PhysicsManager';
import { PixelRenderer } from '@/rendering/PixelRenderer';
import { GameStateManager } from '@/core/GameStateManager';
import { PreviewSlots } from '@/gameplay/PreviewSlots';
import { EliminationSystem } from '@/gameplay/Elimination';
import { ScoringSystem } from '@/gameplay/Scoring';
import { DragDropManager } from '@/gameplay/DragDrop';
import { GameState, PixelBlock, TetrominoData } from '@/types';
import { CELL_TO_PIXEL_RATIO, SCREEN_WIDTH, GAME_AREA_OFFSET_X, GAME_AREA_OFFSET_Y } from '@/config/constants';

/**
 * 主游戏场景
 * 参考设计文档第10章
 */
export class GameScene extends Phaser.Scene {
  private grid!: Grid;
  private physicsManager!: PhysicsManager;
  private pixelRenderer!: PixelRenderer;
  private stateManager!: GameStateManager;
  private previewSlots!: PreviewSlots;
  private eliminationSystem!: EliminationSystem;
  private scoringSystem!: ScoringSystem;
  private dragDropManager!: DragDropManager;
  
  private scoreText!: Phaser.GameObjects.Text;
  private stateText!: Phaser.GameObjects.Text;
  private chainText!: Phaser.GameObjects.Text;
  private previewSlotsUI: Phaser.GameObjects.Container[] = [];
  private nextGroupId: number = 1;
  
  // 当前拖动的方块信息（用于取消时恢复）
  private currentDraggedTetromino: TetrominoData | null = null;
  private currentDraggedSlotIndex: number = -1;

  constructor() {
    super({ key: 'GameScene' });
  }

  create(): void {
    // 初始化核心系统
    this.grid = new Grid();
    this.physicsManager = new PhysicsManager(this.grid);
    this.pixelRenderer = new PixelRenderer(this, this.grid);
    this.stateManager = new GameStateManager(GameState.READY);
    this.previewSlots = new PreviewSlots();
    this.eliminationSystem = new EliminationSystem(this.grid);
    this.scoringSystem = new ScoringSystem();
    this.dragDropManager = new DragDropManager(this, this.grid);

    // 设置背景
    this.cameras.main.setBackgroundColor(0x1a1a2e);

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
   */
  private checkAndPerformElimination(): void {
    const eliminationResults = this.eliminationSystem.checkElimination();

    if (eliminationResults.length > 0) {
      console.log(`发现 ${eliminationResults.length} 个可消除集群`);
      console.log(`消除前像素块总数: ${this.grid.getAllPixels().length}`);

      // 计算并删除所有像素块
      let totalCells = 0;
      let totalPixels = 0;
      
      eliminationResults.forEach((result) => {
        totalCells += result.cluster.cells.length;
        totalPixels += result.pixels.length;
        console.log(`消除集群: ${result.cluster.cells.length}个逻辑格子, ${result.pixels.length}个像素块`);
        // 直接删除像素块
        this.eliminationSystem.eliminatePixels(result.pixels);
      });

      console.log(`消除后像素块总数: ${this.grid.getAllPixels().length}`);

      const score = this.scoringSystem.addEliminationScore(totalCells, true);
      console.log(`消除 ${totalCells} 格 (${totalPixels}像素)，得分 ${score}`);

      // 重新检查稳定性并触发重力
      this.physicsManager.recheckStability();
      console.log(`切换到物理运行状态，活跃像素块: ${this.physicsManager.activeCount}`);
      this.stateManager.setState(GameState.PHYSICS_RUNNING);
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
  private checkGameOver(): void {
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
  private canPlaceAnywhere(tetromino: TetrominoData): boolean {
    for (let y = 0; y < 22; y++) {
      for (let x = 0; x < 12; x++) {
        if (this.canPlaceTetromino(tetromino, x, y)) {
          return true;
        }
      }
    }
    return false;
  }

  /**
   * 创建UI
   */
  private createUI(): void {
    // 分数显示
    this.scoreText = this.add.text(100, 30, '分数: 0', {
      fontSize: '32px',
      color: '#ffffff',
      fontFamily: 'Arial',
      fontStyle: 'bold',
    });

    // 状态显示
    this.stateText = this.add.text(100, 70, '状态: 空闲', {
      fontSize: '20px',
      color: '#aaaaaa',
      fontFamily: 'Arial',
    });

    // 连锁显示
    this.chainText = this.add.text(SCREEN_WIDTH - 200, 30, '', {
      fontSize: '28px',
      color: '#ffff00',
      fontFamily: 'Arial',
      fontStyle: 'bold',
    });

    // 预览槽位UI
    this.createPreviewSlotsUI();
  }

  /**
   * 创建预览槽位UI
   */
  private createPreviewSlotsUI(): void {
    const slotY = GAME_AREA_OFFSET_Y + 1150;
    const slotSize = 100;
    const slotSpacing = 120;
    const startX = GAME_AREA_OFFSET_X + 50;

    for (let i = 0; i < 3; i++) {
      const slotX = startX + i * slotSpacing;

      // 创建槽位容器
      const container = this.add.container(slotX, slotY);

      // 槽位背景
      const bg = this.add.rectangle(0, 0, slotSize, slotSize, 0x2a2a4e, 0.8);
      bg.setStrokeStyle(2, 0xffffff, 0.5);
      bg.setInteractive({ useHandCursor: true });

      // 槽位点击事件
      bg.on('pointerdown', () => {
        this.onSlotClicked(i);
      });

      container.add(bg);
      this.previewSlotsUI.push(container);

      // 槽位编号文本
      const label = this.add.text(0, slotSize / 2 + 20, `槽位${i + 1}`, {
        fontSize: '14px',
        color: '#ffffff',
        fontFamily: 'Arial',
      });
      label.setOrigin(0.5);
      container.add(label);
    }

    // 更新槽位显示
    this.updatePreviewSlotsUI();
  }

  /**
   * 更新预览槽位UI
   */
  private updatePreviewSlotsUI(): void {
    const slots = this.previewSlots.getAllSlots();

    slots.forEach((tetromino, index) => {
      const container = this.previewSlotsUI[index];
      if (!container || !tetromino) return;

      // 清除旧的方块显示（保留前2个：背景和文本）
      // 从后往前删除，避免索引问题
      const itemsToRemove: Phaser.GameObjects.GameObject[] = [];
      for (let i = 2; i < container.length; i++) {
        itemsToRemove.push(container.list[i]);
      }
      itemsToRemove.forEach(item => {
        item.destroy();
      });

      // 绘制方块预览
      const cellSize = 20;
      tetromino.cells.forEach((cell) => {
        const rect = this.add.rectangle(
          cell.x * cellSize - 30,
          cell.y * cellSize - 30,
          cellSize - 2,
          cellSize - 2,
          tetromino.color
        );
        rect.setStrokeStyle(1, 0xffffff, 0.5);
        container.add(rect);
      });
    });
  }

  /**
   * 槽位点击处理
   */
  private onSlotClicked(slotIndex: number): void {
    console.log(`点击槽位 ${slotIndex + 1}`);
    
    if (!this.stateManager.canPlayerPlaceBlock()) {
      console.log('当前状态不允许放置方块:', this.stateManager.state);
      return;
    }

    // 使用槽位：获取当前方块并立即补充新方块
    const tetromino = this.previewSlots.useSlot(slotIndex);
    if (!tetromino) {
      console.error('槽位为空！');
      return;
    }

    console.log(`获取到方块: ${tetromino.shape}, 颜色: ${tetromino.color}`);

    // 保存拖动信息（用于取消时恢复）
    this.currentDraggedTetromino = tetromino;
    this.currentDraggedSlotIndex = slotIndex;

    // 更新UI（现在槽位显示的是新补充的方块）
    this.updatePreviewSlotsUI();

    // 开始拖动（拖的是刚才取出的旧方块）
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
          // 放置成功，清除拖动信息
          this.placeTetromino(result.tetromino, result.position.x, result.position.y);
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
  private placeTetromino(
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
    this.scoreText.setText(`分数: ${this.scoringSystem.score}`);

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

    this.stateText.setText(`状态: ${stateStr}`);
  }

  /**
   * 显示游戏结束界面
   */
  private showGameOver(): void {
    const centerX = SCREEN_WIDTH / 2;
    const centerY = 600;

    // 半透明背景
    this.add.rectangle(
      centerX,
      centerY,
      600,
      400,
      0x000000,
      0.8
    );

    // 游戏结束文本
    const gameOverText = this.add.text(centerX, centerY - 100, '游戏结束', {
      fontSize: '48px',
      color: '#ff0000',
      fontFamily: 'Arial',
      fontStyle: 'bold',
    });
    gameOverText.setOrigin(0.5);

    // 最终分数
    const finalScoreText = this.add.text(
      centerX,
      centerY - 20,
      `最终分数: ${this.scoringSystem.score}`,
      {
        fontSize: '32px',
        color: '#ffffff',
        fontFamily: 'Arial',
      }
    );
    finalScoreText.setOrigin(0.5);

    // 重新开始按钮
    const restartButton = this.add.text(centerX, centerY + 60, '重新开始', {
      fontSize: '28px',
      color: '#00ff00',
      fontFamily: 'Arial',
      fontStyle: 'bold',
    });
    restartButton.setOrigin(0.5);
    restartButton.setInteractive({ useHandCursor: true });
    restartButton.on('pointerdown', () => {
      this.scene.restart();
    });
  }
}

