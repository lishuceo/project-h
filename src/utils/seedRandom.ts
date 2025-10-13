/**
 * 基于种子的伪随机数生成器
 * 保证相同种子产生相同的随机序列
 * 
 * 使用LCG算法（线性同余生成器）
 * X(n+1) = (a * X(n) + c) mod m
 */
export class SeededRandom {
  private seed: number;
  
  constructor(seed: number) {
    this.seed = seed % 2147483647;
    if (this.seed <= 0) this.seed += 2147483646;
  }
  
  /**
   * 生成下一个随机数 [0, 1)
   */
  public next(): number {
    // LCG参数（来自Numerical Recipes）
    this.seed = (this.seed * 16807) % 2147483647;
    return (this.seed - 1) / 2147483646;
  }
  
  /**
   * 生成整数随机数 [min, max]（包含边界）
   */
  public nextInt(min: number, max: number): number {
    const range = max - min + 1;
    return Math.floor(this.next() * range) + min;
  }
  
  /**
   * 生成浮点随机数 [min, max)
   */
  public nextFloat(min: number, max: number): number {
    return this.next() * (max - min) + min;
  }
  
  /**
   * 从数组中随机选择一个元素
   */
  public choice<T>(array: T[]): T {
    if (array.length === 0) {
      throw new Error('Cannot choose from empty array');
    }
    return array[this.nextInt(0, array.length - 1)];
  }
  
  /**
   * 从数组中随机选择n个不重复的元素
   */
  public sample<T>(array: T[], n: number): T[] {
    if (n > array.length) {
      throw new Error('Sample size cannot exceed array length');
    }
    
    const result: T[] = [];
    const copy = [...array];
    
    for (let i = 0; i < n; i++) {
      const index = this.nextInt(0, copy.length - 1);
      result.push(copy[index]);
      copy.splice(index, 1);
    }
    
    return result;
  }
  
  /**
   * 打乱数组（Fisher-Yates洗牌算法）
   * 返回新数组，不修改原数组
   */
  public shuffle<T>(array: T[]): T[] {
    const result = [...array];
    for (let i = result.length - 1; i > 0; i--) {
      const j = this.nextInt(0, i);
      [result[i], result[j]] = [result[j], result[i]];
    }
    return result;
  }
  
  /**
   * 生成布尔值（概率为p）
   */
  public boolean(probability: number = 0.5): boolean {
    return this.next() < probability;
  }
  
  /**
   * 重置种子
   */
  public reset(seed: number): void {
    this.seed = seed % 2147483647;
    if (this.seed <= 0) this.seed += 2147483646;
  }
  
  /**
   * 获取当前种子状态
   */
  public getSeed(): number {
    return this.seed;
  }
}

