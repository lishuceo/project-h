// 网格常量（参考设计文档第3章）
export const LOGICAL_GRID_WIDTH = 11; // 缩小为11列（左右留出空隙）
export const LOGICAL_GRID_HEIGHT = 10; // 优化为10行（更适合UI布局）
export const CELL_TO_PIXEL_RATIO = 10;
export const PIXEL_GRID_WIDTH = LOGICAL_GRID_WIDTH * CELL_TO_PIXEL_RATIO; // 110
export const PIXEL_GRID_HEIGHT = LOGICAL_GRID_HEIGHT * CELL_TO_PIXEL_RATIO; // 100

// 预览槽位常量
export const PREVIEW_SLOTS = 3;

// 物理常量
export const PHYSICS_TIME_STEP = 1 / 60; // 60 FPS
export const PIXEL_SIZE = 6; // 每个像素块在屏幕上的实际像素大小（放大到6px）

// 动画常量
export const ELIMINATION_ANIM_DURATION = 1000; // 毫秒
export const PARTICLE_LIFETIME = 1000; // 毫秒

// 游戏区域渲染尺寸
export const GAME_AREA_WIDTH = PIXEL_GRID_WIDTH * PIXEL_SIZE; // 720px
export const GAME_AREA_HEIGHT = PIXEL_GRID_HEIGHT * PIXEL_SIZE; // 600px (10行 × 10像素 × 6px)

// 屏幕尺寸（9:16标准手机比例）
export const SCREEN_WIDTH = 720;  // 720px宽度（正好容纳游戏区域）
export const SCREEN_HEIGHT = 1280; // 16:9比例

// 游戏区域偏移（优化布局：居中对齐）
export const GAME_AREA_OFFSET_X = (SCREEN_WIDTH - GAME_AREA_WIDTH) / 2;
export const GAME_AREA_OFFSET_Y = 150; // 顶部留白（增加到150px，让布局更均衡）

// 深色科幻/霓虹街机风格配色方案（Dark + Neon）
export const UI_COLORS = {
  // 背景色 - 深色科幻
  BG_PRIMARY: 0x0f1419,      // 极深蓝黑背景
  BG_SECONDARY: 0x1a1f2e,    // 卡片背景
  BG_TERTIARY: 0x252a3a,     // 悬浮元素

  // 霓虹方块色（降饱和度、提高亮度）
  BLOCK_GREEN: 0x4ade80,     // 霓虹绿
  BLOCK_GREEN_GLOW: 0x22c55e,// 绿色发光
  BLOCK_BLUE: 0x60a5fa,      // 霓虹蓝
  BLOCK_BLUE_GLOW: 0x3b82f6, // 蓝色发光
  BLOCK_RED: 0xf87171,       // 霓虹红
  BLOCK_RED_GLOW: 0xef4444,  // 红色发光

  // 强调色 - 霓虹色系
  ACCENT_PRIMARY: 0x1b9cff,  // 霓虹蓝（主色）
  ACCENT_SUCCESS: 0x4ade80,  // 霓虹绿
  ACCENT_WARNING: 0xfbbf24,  // 霓虹黄
  ACCENT_DANGER: 0xf87171,   // 霓虹红

  // 文字色 - 高对比
  TEXT_PRIMARY: 0xffffff,    // 纯白
  TEXT_SECONDARY: 0x9aa4b2,  // 中灰蓝
  TEXT_MUTED: 0x6b7280,      // 深灰

  // 边框与发光
  BORDER_GRID: 0x2a3344,     // 网格线（25%透明度）
  BORDER_GLOW: 0x1b9cff,     // 霓虹蓝发光
  GLOW_ALPHA: 0.35,          // 发光透明度

  // 特殊颜色
  CARD_BG: 0x1a1f2e,         // 卡片背景
  SHADOW_DEEP: 0x000000      // 深色阴影
};

