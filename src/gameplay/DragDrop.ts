import Phaser from 'phaser';
import { TetrominoData, LogicalCell } from '@/types';
import { Grid } from '@/core/Grid';
import {
  PIXEL_SIZE,
  CELL_TO_PIXEL_RATIO,
  GAME_AREA_OFFSET_X,
  GAME_AREA_OFFSET_Y,
} from '@/config/constants';

/**
 * 拖放管理器
 * 参考设计文档第6章
 */
export class DragDropManager {
  private scene: Phaser.Scene;
  private grid: Grid;
  private isDragging: boolean = false;
  private draggedTetromino: TetrominoData | null = null;
  private draggedSprites: Phaser.GameObjects.Rectangle[] = [];
  private dragStartSlotIndex: number = -1;
  private currentLogicalPos: LogicalCell = { x: 0, y: 0 };

  constructor(scene: Phaser.Scene, grid: Grid) {
    this.scene = scene;
    this.grid = grid;
  }

  /**
   * 开始拖动方块
   */
  startDrag(tetromino: TetrominoData, slotIndex: number): void {
    const startTime = performance.now();

    if (this.isDragging) {
      return;
    }

    this.isDragging = true;
    this.draggedTetromino = tetromino;
    this.dragStartSlotIndex = slotIndex;

    // 创建拖动中的方块精灵
    this.createDragSprites();

    const createTime = performance.now() - startTime;
    console.log(`  开始拖动 ${tetromino.shape} - 创建精灵耗时: ${createTime.toFixed(1)}ms`);
  }

  // 性能监控：追踪 updateDrag 调用
  private updateDragCallCount = 0;
  private updateDragTotalTime = 0;

  /**
   * 更新拖动位置
   */
  updateDrag(pointerX: number, pointerY: number): void {
    if (!this.isDragging || !this.draggedTetromino) {
      console.warn('updateDrag: 没有拖动状态或方块');
      return;
    }

    const startTime = performance.now();

    // 移动端优化：对触摸位置应用向上偏移
    const TOUCH_OFFSET_Y = -260; // 向上偏移100px
    const adjustedPointerY = pointerY + TOUCH_OFFSET_Y;

    // 转换为网格坐标（基于偏移后的位置）
    const gridX = Math.floor((pointerX - GAME_AREA_OFFSET_X) / PIXEL_SIZE);
    const gridY = Math.floor((adjustedPointerY - GAME_AREA_OFFSET_Y) / PIXEL_SIZE);
    const logicalPos = this.grid.pixelToLogical(gridX, gridY);

    this.currentLogicalPos = logicalPos;

    // 更新精灵位置和颜色反馈（不需要额外偏移，logicalPos已经是偏移后的）
    this.updateDragSprites(logicalPos);

    // 性能监控
    const updateTime = performance.now() - startTime;
    this.updateDragCallCount++;
    this.updateDragTotalTime += updateTime;

    // 每 60 帧输出一次统计（约 1 秒）
    if (this.updateDragCallCount % 60 === 0) {
      const avgTime = this.updateDragTotalTime / this.updateDragCallCount;
      console.log(`  🎯 updateDrag 统计 - 调用: ${this.updateDragCallCount}次, 平均耗时: ${avgTime.toFixed(2)}ms`);
    }
  }

  /**
   * 结束拖动（尝试放置）
   */
  endDrag(): {
    success: boolean;
    tetromino: TetrominoData | null;
    position: LogicalCell | null;
    slotIndex: number;
  } {
    if (!this.isDragging || !this.draggedTetromino) {
      return { success: false, tetromino: null, position: null, slotIndex: -1 };
    }

    const tetromino = this.draggedTetromino;
    const position = this.currentLogicalPos;
    const slotIndex = this.dragStartSlotIndex;

    // 检查是否可以放置
    const canPlace = this.canPlaceTetromino(tetromino, position.x, position.y);

    // 清理拖动精灵
    this.clearDragSprites();

    this.isDragging = false;
    this.draggedTetromino = null;
    this.dragStartSlotIndex = -1;

    return {
      success: canPlace,
      tetromino: canPlace ? tetromino : null,
      position: canPlace ? position : null,
      slotIndex: slotIndex,
    };
  }

  /**
   * 取消拖动
   */
  cancelDrag(): void {
    if (!this.isDragging) {
      return;
    }

    this.clearDragSprites();
    this.isDragging = false;
    this.draggedTetromino = null;
    this.dragStartSlotIndex = -1;
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
   * 创建拖动精灵
   */
  private createDragSprites(): void {
    if (!this.draggedTetromino) {
      console.error('createDragSprites: 没有拖动的方块');
      return;
    }

    const cellSize = CELL_TO_PIXEL_RATIO * PIXEL_SIZE;

    this.draggedTetromino.cells.forEach(() => {
      const sprite = this.scene.add.rectangle(
        0,
        0,
        cellSize - 2,
        cellSize - 2,
        this.draggedTetromino!.color,
        0.7 // 半透明
      );
      sprite.setStrokeStyle(2, 0xffffff, 0.8);
      sprite.setDepth(1000); // 确保在最上层
      this.draggedSprites.push(sprite);
    });
    
    console.log(`创建了 ${this.draggedSprites.length} 个拖动精灵`);
  }

  // 性能优化：缓存上一帧的位置和状态
  private lastLogicalPos: LogicalCell | null = null;
  private lastCanPlace: boolean = false;

  /**
   * 更新拖动精灵的位置和颜色
   * 性能优化：只在位置或状态改变时更新
   */
  private updateDragSprites(logicalPos: LogicalCell): void {
    if (!this.draggedTetromino) {
      console.warn('updateDragSprites: 没有拖动的方块');
      return;
    }

    if (this.draggedSprites.length === 0) {
      console.error('updateDragSprites: 没有拖动精灵！');
      return;
    }

    // 性能优化：检查位置是否改变
    const posChanged = !this.lastLogicalPos ||
      this.lastLogicalPos.x !== logicalPos.x ||
      this.lastLogicalPos.y !== logicalPos.y;

    if (!posChanged) {
      return; // 位置没变，无需更新
    }

    const canPlace = this.canPlaceTetromino(
      this.draggedTetromino,
      logicalPos.x,
      logicalPos.y
    );

    // 性能优化：检查放置状态是否改变
    const stateChanged = canPlace !== this.lastCanPlace;

    const cellSize = CELL_TO_PIXEL_RATIO * PIXEL_SIZE;

    this.draggedTetromino.cells.forEach((cell, index) => {
      const sprite = this.draggedSprites[index];
      if (sprite) {
        // 位置改变时才更新位置
        if (posChanged) {
          const pixelPos = this.grid.logicalToPixel(
            logicalPos.x + cell.x,
            logicalPos.y + cell.y
          );

          const screenX = GAME_AREA_OFFSET_X + pixelPos.x * PIXEL_SIZE;
          const screenY = GAME_AREA_OFFSET_Y + pixelPos.y * PIXEL_SIZE;

          sprite.setPosition(
            screenX + cellSize / 2,
            screenY + cellSize / 2
          );
          sprite.setVisible(true);
        }

        // 状态改变时才更新颜色
        if (stateChanged || posChanged) {
          if (canPlace) {
            sprite.setFillStyle(this.draggedTetromino!.color, 0.7);
            sprite.setStrokeStyle(2, 0x00ff00, 0.9);
          } else {
            sprite.setFillStyle(0xff0000, 0.5);
            sprite.setStrokeStyle(2, 0xff0000, 0.9);
          }
        }
      } else {
        console.error(`精灵 ${index} 不存在`);
      }
    });

    // 缓存当前状态
    this.lastLogicalPos = { x: logicalPos.x, y: logicalPos.y };
    this.lastCanPlace = canPlace;
  }

  /**
   * 清理拖动精灵
   */
  private clearDragSprites(): void {
    this.draggedSprites.forEach((sprite) => sprite.destroy());
    this.draggedSprites = [];
    // 重置缓存
    this.lastLogicalPos = null;
    this.lastCanPlace = false;
    // 重置性能监控
    this.updateDragCallCount = 0;
    this.updateDragTotalTime = 0;
  }

  /**
   * 获取是否正在拖动
   */
  get dragging(): boolean {
    return this.isDragging;
  }
}

