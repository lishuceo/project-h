// 颜色枚举（当前使用4种颜色）
export enum Color {
  RED = 0xff0000,
  BLUE = 0x0000ff,
  GREEN = 0x00ff00,
  YELLOW = 0xffff00,
  PURPLE = 0xff00ff,  // 保留定义但不使用
  WHITE = 0xffffff,   // 保留定义但不使用
}

// 方块形状枚举
export enum ShapeType {
  I = 'I',
  O = 'O',
  T = 'T',
  L = 'L',
  J = 'J',
  S = 'S',
  Z = 'Z',
}

// 游戏状态枚举
export enum GameState {
  READY = 'READY',
  IDLE = 'IDLE',
  DRAGGING = 'DRAGGING',
  PLACING = 'PLACING',
  PHYSICS_RUNNING = 'PHYSICS_RUNNING',
  CHECKING_ELIMINATION = 'CHECKING_ELIMINATION',
  ELIMINATING = 'ELIMINATING',
  GAME_OVER = 'GAME_OVER',
}

// 像素块数据结构
export interface PixelBlock {
  x: number;              // X坐标 (0-119)
  y: number;              // Y坐标 (0-219)
  color: Color;           // 颜色
  isStable: boolean;      // 是否已稳定（落定）
  groupId: number;        // 所属方块组ID
  updatedThisFrame?: boolean; // 本帧是否已更新
}

// 逻辑格子坐标
export interface LogicalCell {
  x: number; // 0-11
  y: number; // 0-21
}

// 方块定义
export interface TetrominoData {
  shape: ShapeType;
  color: Color;
  rotation: number; // 0, 1, 2, 3
  cells: LogicalCell[]; // 相对坐标
}

// 集群数据
export interface Cluster {
  color: Color;
  cells: LogicalCell[];
  touchesLeft: boolean;
  touchesRight: boolean;
}

// 活跃方块组
export interface ActiveBlockGroup {
  id: number;
  pixels: PixelBlock[];
  placedTime: number;
  isFullyStable: boolean;
  hasBeenChecked: boolean;
}

