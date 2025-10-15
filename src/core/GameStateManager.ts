import { GameState } from '@/types';

/**
 * 游戏状态管理器
 * 参考设计文档第10章
 */
export class GameStateManager {
  private currentState: GameState;
  private stateChangeCallbacks: Map<GameState, (() => void)[]>;

  constructor(initialState: GameState = GameState.READY) {
    this.currentState = initialState;
    this.stateChangeCallbacks = new Map();
  }

  /**
   * 获取当前状态
   */
  get state(): GameState {
    return this.currentState;
  }

  /**
   * 设置新状态
   */
  setState(newState: GameState): void {
    if (this.currentState === newState) {
      return;
    }

    const oldState = this.currentState;
    this.currentState = newState;

    console.log(`状态转换: ${oldState} -> ${newState}`);

    // 触发状态变化回调
    this.triggerCallbacks(newState);
  }

  /**
   * 检查是否在指定状态
   */
  is(state: GameState): boolean {
    return this.currentState === state;
  }

  /**
   * 检查是否在多个状态之一
   */
  isAnyOf(...states: GameState[]): boolean {
    return states.includes(this.currentState);
  }

  /**
   * 注册状态变化回调
   */
  onStateChange(state: GameState, callback: () => void): void {
    if (!this.stateChangeCallbacks.has(state)) {
      this.stateChangeCallbacks.set(state, []);
    }
    this.stateChangeCallbacks.get(state)!.push(callback);
  }

  /**
   * 触发状态变化回调
   */
  private triggerCallbacks(state: GameState): void {
    const callbacks = this.stateChangeCallbacks.get(state);
    if (callbacks) {
      callbacks.forEach((callback) => callback());
    }
  }

  /**
   * 检查玩家是否可以放置方块
   * 参考设计文档10.3节
   *
   * ⚠️ 重要：不允许在ELIMINATING状态下拖动方块
   * 原因：消除动画期间拖动会导致状态混乱，方块可能丢失
   */
  canPlayerPlaceBlock(): boolean {
    return this.isAnyOf(
      GameState.IDLE,
      GameState.PHYSICS_RUNNING
    );
  }
}

