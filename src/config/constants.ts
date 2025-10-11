// 网格常量（参考设计文档第3章）
export const LOGICAL_GRID_WIDTH = 12;
export const LOGICAL_GRID_HEIGHT = 15; // 减少到15行（原22行的2/3）
export const CELL_TO_PIXEL_RATIO = 10;
export const PIXEL_GRID_WIDTH = LOGICAL_GRID_WIDTH * CELL_TO_PIXEL_RATIO; // 120
export const PIXEL_GRID_HEIGHT = LOGICAL_GRID_HEIGHT * CELL_TO_PIXEL_RATIO; // 150

// 预览槽位常量
export const PREVIEW_SLOTS = 3;

// 物理常量
export const PHYSICS_TIME_STEP = 1 / 60; // 60 FPS
export const PIXEL_SIZE = 6; // 每个像素块在屏幕上的实际像素大小（放大到6px）

// 动画常量
export const ELIMINATION_ANIM_DURATION = 1000; // 毫秒
export const PARTICLE_LIFETIME = 1000; // 毫秒

// 游戏区域渲染尺寸
export const GAME_AREA_WIDTH = PIXEL_GRID_WIDTH * PIXEL_SIZE; // 600像素
export const GAME_AREA_HEIGHT = PIXEL_GRID_HEIGHT * PIXEL_SIZE; // 1100像素

// 屏幕尺寸
export const SCREEN_WIDTH = 800;
export const SCREEN_HEIGHT = 1400;

// 游戏区域偏移
export const GAME_AREA_OFFSET_X = (SCREEN_WIDTH - GAME_AREA_WIDTH) / 2;
export const GAME_AREA_OFFSET_Y = 100;

