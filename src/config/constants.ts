// 网格常量（参考设计文档第3章）
export const LOGICAL_GRID_WIDTH = 11; // 缩小为11列（左右留出空隙）
export const LOGICAL_GRID_HEIGHT = 12; // 从10行增加到12行（更适合长屏手机）
export const CELL_TO_PIXEL_RATIO = 10;
export const PIXEL_GRID_WIDTH = LOGICAL_GRID_WIDTH * CELL_TO_PIXEL_RATIO; // 110
export const PIXEL_GRID_HEIGHT = LOGICAL_GRID_HEIGHT * CELL_TO_PIXEL_RATIO; // 120 (增加了20)

// 预览槽位常量
export const PREVIEW_SLOTS = 3;

// 物理常量
export const PHYSICS_TIME_STEP = 1 / 60; // 60 FPS
export const PIXEL_SIZE = 9.5; // 每个像素块在屏幕上的实际像素大小（9.5px，使游戏区域对齐信息栏宽度）

// 动画常量
export const ELIMINATION_ANIM_DURATION = 1000; // 毫秒
export const PARTICLE_LIFETIME = 1000; // 毫秒

// 游戏区域渲染尺寸
export const GAME_AREA_WIDTH = PIXEL_GRID_WIDTH * PIXEL_SIZE; // 1045px (110 × 9.5)
export const GAME_AREA_HEIGHT = PIXEL_GRID_HEIGHT * PIXEL_SIZE; // 1140px (120 × 9.5) - 增加2行

// 屏幕尺寸（19.5:9现代移动端比例，完美适配iPhone 12/13/14系列）
export const SCREEN_WIDTH = 1080;  // 1080px宽度（Full HD移动端标准）
export const SCREEN_HEIGHT = 2340; // 19.5:9比例，匹配现代iPhone

// 游戏区域偏移（优化布局：与信息栏对齐）
export const GAME_AREA_OFFSET_X = (SCREEN_WIDTH - GAME_AREA_WIDTH) / 2; // 17.5px 左右边距（1080-1045）/2
export const GAME_AREA_OFFSET_Y = 320; // 顶部留白，避开刘海和信息栏，为进度文本留出空间

// 深色科幻/霓虹街机风格配色方案（Dark + Neon）
export const UI_COLORS = {
  // 背景色 - 深色科幻
  BG_PRIMARY: 0x0f1419,      // 极深蓝黑背景
  BG_SECONDARY: 0x33395C,    // 卡片背景
  BG_TERTIARY: 0x252a3a,     // 悬浮元素

  // 背景渐变色（参考 BLOCK BLAST）
  // BG_GRADIENT_TOP: 0x2440B0,    // 渐变顶部：深蓝色
  // BG_GRADIENT_BOTTOM: 0x3D6ADB, // 渐变底部：稍浅的蓝色
  BG_GRADIENT_TOP: 0x395395,    // 渐变顶部：深蓝色
  BG_GRADIENT_BOTTOM: 0x5E7FCC, // 渐变底部：稍浅的蓝色

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
  BORDER_LOCKED: 0x555555,   // 未解锁边框（中灰）
  GLOW_ALPHA: 0.35,          // 发光透明度

  // 特殊颜色
  CARD_BG: 0x2a3f5f,           // 卡片背景（默认/已解锁）- 深蓝色，更协调
  CARD_BG_LOCKED: 0x3d4556,    // 未解锁卡片背景（蓝灰）
  CARD_BG_COMPLETED: 0x2d5a3d, // 已完成卡片背景（深绿）
  SHADOW_DEEP: 0x000000,       // 深色阴影

  // 按钮颜色
  BUTTON_COMPLETED: 0x64748b,  // 已完成按钮（灰蓝）
  BUTTON_COMPLETED_HOVER: 0x748096, // 已完成按钮悬停
  BUTTON_COMPLETED_BORDER: 0x475569, // 已完成按钮边框
  BUTTON_START: 0x4ade80,      // 开始按钮（同 ACCENT_SUCCESS）
  BUTTON_START_HOVER: 0x66bb6a // 开始按钮悬停
};

