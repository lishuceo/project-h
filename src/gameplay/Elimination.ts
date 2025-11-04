import { Grid } from '@/core/Grid';
import { Cluster, Color, LogicalCell, PixelBlock } from '@/types';
import { PIXEL_GRID_WIDTH, PIXEL_GRID_HEIGHT } from '@/config/constants';

/**
 * 消除系统 - 使用BFS算法检测连通集群
 * 参考设计文档第8章
 * 
 * 重要改进：在像素网格层面进行BFS，避免三角形堆积导致的断连问题
 */
export class EliminationSystem {
  private grid: Grid;

  constructor(grid: Grid) {
    this.grid = grid;
  }

  /**
   * 检查并返回所有可消除的集群（包含像素块列表）
   * 在像素网格层面进行BFS，避免三角形堆积导致的断连
   */
  checkElimination(): Array<{
    cluster: Cluster;
    pixels: PixelBlock[];
  }> {
    // 在像素网格层面查找连通集群
    const pixelClusters = this.findPixelClusters();
    
    console.log(`找到 ${pixelClusters.length} 个像素集群`);
    // 只显示可能被消除的集群
    pixelClusters.filter(c => c.touchesLeft || c.touchesRight).forEach((cluster, index) => {
      console.log(`集群${index}: 颜色=${cluster.color}, 像素数=${cluster.pixels.length}, 触及左=${cluster.touchesLeft}, 触及右=${cluster.touchesRight}`);
    });

    // 筛选出同时触及左右边界的集群
    const eliminationClusters = pixelClusters.filter(
      (cluster) => cluster.touchesLeft && cluster.touchesRight
    );
    
    console.log(`可消除集群数: ${eliminationClusters.length}`);

    // 返回包含逻辑集群和像素块列表的结果
    return eliminationClusters.map(pc => ({
      cluster: this.pixelClusterToLogicalCluster(pc),
      pixels: pc.pixels
    }));
  }
  
  /**
   * 在像素网格层面查找连通集群
   * 性能优化：只遍历活跃像素块，而非整个网格
   */
  private findPixelClusters(): Array<{
    color: Color;
    pixels: PixelBlock[];
    touchesLeft: boolean;
    touchesRight: boolean;
  }> {
    const visited = new Set<PixelBlock>();
    const clusters: Array<{
      color: Color;
      pixels: PixelBlock[];
      touchesLeft: boolean;
      touchesRight: boolean;
    }> = [];

    // 性能优化：只遍历活跃像素块（避免扫描整个 26,400 个网格）
    const activePixels = this.grid.getActivePixelsSet();

    activePixels.forEach((pixel) => {
      if (!visited.has(pixel)) {
        // 发现新的未访问像素块，进行BFS
        const cluster = this.bfsPixelSearch(pixel.x, pixel.y, pixel.color, visited);
        clusters.push(cluster);
      }
    });

    return clusters;
  }
  
  /**
   * 像素层面的BFS搜索
   * 性能优化：使用 Set<PixelBlock> 代替字符串键，避免字符串拼接和解析
   */
  private bfsPixelSearch(
    startX: number,
    startY: number,
    color: Color,
    visited: Set<PixelBlock>
  ): {
    color: Color;
    pixels: PixelBlock[];
    touchesLeft: boolean;
    touchesRight: boolean;
  } {
    const startPixel = this.grid.getPixel(startX, startY);
    if (!startPixel) {
      return {
        color: color,
        pixels: [],
        touchesLeft: false,
        touchesRight: false,
      };
    }

    const queue: PixelBlock[] = [startPixel];
    const cluster = {
      color: color,
      pixels: [] as PixelBlock[],
      touchesLeft: false,
      touchesRight: false,
    };

    visited.add(startPixel);

    while (queue.length > 0) {
      const pixel = queue.shift()!;
      cluster.pixels.push(pixel);

      // 检查是否触及边界（像素网格边界）
      if (pixel.x === 0) cluster.touchesLeft = true;
      if (pixel.x === PIXEL_GRID_WIDTH - 1) cluster.touchesRight = true;

      // 检查八个方向的相邻像素（包括对角线）
      const neighbors = [
        { x: pixel.x - 1, y: pixel.y }, // 左
        { x: pixel.x + 1, y: pixel.y }, // 右
        { x: pixel.x, y: pixel.y - 1 }, // 上
        { x: pixel.x, y: pixel.y + 1 }, // 下
        { x: pixel.x - 1, y: pixel.y - 1 }, // 左上
        { x: pixel.x + 1, y: pixel.y - 1 }, // 右上
        { x: pixel.x - 1, y: pixel.y + 1 }, // 左下
        { x: pixel.x + 1, y: pixel.y + 1 }, // 右下
      ];

      for (const { x: nx, y: ny } of neighbors) {
        // 边界检查
        if (nx < 0 || nx >= PIXEL_GRID_WIDTH || ny < 0 || ny >= PIXEL_GRID_HEIGHT) {
          continue;
        }

        // 获取邻接像素块
        const neighborPixel = this.grid.getPixel(nx, ny);
        if (!neighborPixel) {
          continue;
        }

        // 已访问检查（性能优化：Set.has() 是 O(1)）
        if (visited.has(neighborPixel)) {
          continue;
        }

        // 颜色匹配检查
        if (neighborPixel.color === color) {
          visited.add(neighborPixel);
          queue.push(neighborPixel);
        }
      }
    }

    return cluster;
  }
  
  /**
   * 将像素集群转换为逻辑集群（用于后续处理）
   */
  private pixelClusterToLogicalCluster(pixelCluster: {
    color: Color;
    pixels: PixelBlock[];
    touchesLeft: boolean;
    touchesRight: boolean;
  }): Cluster {
    // 收集所有涉及的逻辑格子
    const logicalCells = new Set<string>();
    
    pixelCluster.pixels.forEach(pixel => {
      const logicalPos = this.grid.pixelToLogical(pixel.x, pixel.y);
      logicalCells.add(`${logicalPos.x},${logicalPos.y}`);
    });
    
    const cells: LogicalCell[] = [];
    logicalCells.forEach(key => {
      const [x, y] = key.split(',').map(Number);
      cells.push({x, y});
    });
    
    return {
      color: pixelCluster.color,
      cells: cells,
      touchesLeft: pixelCluster.touchesLeft,
      touchesRight: pixelCluster.touchesRight
    };
  }

  /**
   * 执行消除（移除像素块）
   * 直接删除像素块数组中的所有像素块
   */
  eliminatePixels(pixels: PixelBlock[]): void {
    console.log(`开始删除 ${pixels.length} 个像素块`);
    let deletedCount = 0;
    
    // 直接删除所有像素块
    pixels.forEach(pixel => {
      const existing = this.grid.getPixel(pixel.x, pixel.y);
      if (existing) {
        this.grid.setPixel(pixel.x, pixel.y, null);
        deletedCount++;
      }
    });
    
    console.log(`实际删除了 ${deletedCount} 个像素块`);
  }
}

