import { ShapeType, Color, TetrominoData, LogicalCell } from '@/types';

/**
 * 方块形状定义
 * 参考设计文档第4章
 */
export const TETROMINO_SHAPES: Record<ShapeType, LogicalCell[][]> = {
  // I形 - 长条 (2种旋转状态)
  [ShapeType.I]: [
    [
      { x: 0, y: 0 },
      { x: 1, y: 0 },
      { x: 2, y: 0 },
      { x: 3, y: 0 },
    ],
    [
      { x: 0, y: 0 },
      { x: 0, y: 1 },
      { x: 0, y: 2 },
      { x: 0, y: 3 },
    ],
  ],

  // O形 - 正方形 (1种旋转状态)
  [ShapeType.O]: [
    [
      { x: 0, y: 0 },
      { x: 1, y: 0 },
      { x: 0, y: 1 },
      { x: 1, y: 1 },
    ],
  ],

  // T形 (4种旋转状态)
  [ShapeType.T]: [
    [
      { x: 1, y: 0 },
      { x: 0, y: 1 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
    ],
    [
      { x: 1, y: 0 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
      { x: 1, y: 2 },
    ],
    [
      { x: 0, y: 1 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
      { x: 1, y: 2 },
    ],
    [
      { x: 1, y: 0 },
      { x: 0, y: 1 },
      { x: 1, y: 1 },
      { x: 1, y: 2 },
    ],
  ],

  // L形 (4种旋转状态)
  [ShapeType.L]: [
    [
      { x: 2, y: 0 },
      { x: 0, y: 1 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
    ],
    [
      { x: 1, y: 0 },
      { x: 1, y: 1 },
      { x: 1, y: 2 },
      { x: 2, y: 2 },
    ],
    [
      { x: 0, y: 1 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
      { x: 0, y: 2 },
    ],
    [
      { x: 0, y: 0 },
      { x: 1, y: 0 },
      { x: 1, y: 1 },
      { x: 1, y: 2 },
    ],
  ],

  // J形 - 反L形 (4种旋转状态)
  [ShapeType.J]: [
    [
      { x: 0, y: 0 },
      { x: 0, y: 1 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
    ],
    [
      { x: 1, y: 0 },
      { x: 2, y: 0 },
      { x: 1, y: 1 },
      { x: 1, y: 2 },
    ],
    [
      { x: 0, y: 1 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
      { x: 2, y: 2 },
    ],
    [
      { x: 1, y: 0 },
      { x: 1, y: 1 },
      { x: 0, y: 2 },
      { x: 1, y: 2 },
    ],
  ],

  // S形 (2种旋转状态)
  [ShapeType.S]: [
    [
      { x: 1, y: 0 },
      { x: 2, y: 0 },
      { x: 0, y: 1 },
      { x: 1, y: 1 },
    ],
    [
      { x: 1, y: 0 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
      { x: 2, y: 2 },
    ],
  ],

  // Z形 (2种旋转状态)
  [ShapeType.Z]: [
    [
      { x: 0, y: 0 },
      { x: 1, y: 0 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
    ],
    [
      { x: 2, y: 0 },
      { x: 1, y: 1 },
      { x: 2, y: 1 },
      { x: 1, y: 2 },
    ],
  ],
};

/**
 * 颜色池（6种颜色）
 */
export const COLOR_POOL: Color[] = [
  Color.RED,
  Color.BLUE,
  Color.GREEN,
  Color.YELLOW,
  Color.PURPLE,
  Color.WHITE,
];

/**
 * 7-Bag随机系统
 * 参考设计文档4.3节
 */
export class BagSystem {
  private bag: ShapeType[] = [];

  /**
   * 获取下一个形状
   */
  getNextShape(): ShapeType {
    if (this.bag.length === 0) {
      this.refillBag();
    }
    return this.bag.pop()!;
  }

  /**
   * 重新填充袋子并打乱
   */
  private refillBag(): void {
    this.bag = [
      ShapeType.I,
      ShapeType.O,
      ShapeType.T,
      ShapeType.L,
      ShapeType.J,
      ShapeType.S,
      ShapeType.Z,
    ];
    this.shuffle(this.bag);
  }

  /**
   * Fisher-Yates洗牌算法
   */
  private shuffle<T>(array: T[]): void {
    for (let i = array.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [array[i], array[j]] = [array[j], array[i]];
    }
  }
}

/**
 * 随机选择颜色
 */
export function randomColor(): Color {
  return COLOR_POOL[Math.floor(Math.random() * COLOR_POOL.length)];
}

/**
 * 创建方块数据
 */
export function createTetromino(shape: ShapeType, color: Color, rotation = 0): TetrominoData {
  const rotations = TETROMINO_SHAPES[shape];
  const cells = rotations[rotation % rotations.length];

  return {
    shape,
    color,
    rotation,
    cells,
  };
}

/**
 * 获取方块的边界框（用于渲染）
 */
export function getTetrominoBounds(tetromino: TetrominoData): {
  minX: number;
  maxX: number;
  minY: number;
  maxY: number;
} {
  let minX = Infinity;
  let maxX = -Infinity;
  let minY = Infinity;
  let maxY = -Infinity;

  tetromino.cells.forEach((cell) => {
    minX = Math.min(minX, cell.x);
    maxX = Math.max(maxX, cell.x);
    minY = Math.min(minY, cell.y);
    maxY = Math.max(maxY, cell.y);
  });

  return { minX, maxX, minY, maxY };
}

