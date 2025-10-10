/**
 * 计分系统
 * 参考设计文档第9章
 */
export class ScoringSystem {
  private totalScore: number = 0;
  private currentChainLevel: number = 0;

  /**
   * 计算基础分数
   * 参考设计文档9.2节
   * 1056个逻辑格子 ≈ 373分
   */
  calculateBaseScore(cellsEliminated: number): number {
    // 使用平方根公式
    return Math.floor(10 * Math.sqrt(cellsEliminated));
  }

  /**
   * 添加消除得分（含连锁加成）
   * 参考设计文档9.3节
   */
  addEliminationScore(cellsEliminated: number, isChain: boolean = false): number {
    const baseScore = this.calculateBaseScore(cellsEliminated);

    if (isChain) {
      this.currentChainLevel++;
    } else {
      this.currentChainLevel = 1;
    }

    const finalScore = baseScore * this.currentChainLevel;
    this.totalScore += finalScore;

    console.log(
      `消除 ${cellsEliminated} 格，基础分 ${baseScore}，连锁 x${this.currentChainLevel}，得分 ${finalScore}`
    );

    return finalScore;
  }

  /**
   * 重置连锁计数
   */
  resetChain(): void {
    this.currentChainLevel = 0;
  }

  /**
   * 获取当前总分
   */
  get score(): number {
    return this.totalScore;
  }

  /**
   * 获取当前连锁倍数
   */
  get chainLevel(): number {
    return this.currentChainLevel;
  }

  /**
   * 重置分数（新游戏）
   */
  reset(): void {
    this.totalScore = 0;
    this.currentChainLevel = 0;
  }
}

