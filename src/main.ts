import Phaser from 'phaser';
import { GameScene } from './scenes/GameScene';
import { SCREEN_WIDTH, SCREEN_HEIGHT } from './config/constants';

/**
 * Phaser游戏配置
 */
const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  width: SCREEN_WIDTH,
  height: SCREEN_HEIGHT,
  parent: 'game-container',
  backgroundColor: '#1a1a2e',
  scene: [GameScene],
  physics: {
    default: 'arcade',
    arcade: {
      gravity: { x: 0, y: 0 },
      debug: false,
    },
  },
  scale: {
    mode: Phaser.Scale.FIT,
    autoCenter: Phaser.Scale.CENTER_BOTH,
  },
};

// 创建游戏实例
const game = new Phaser.Game(config);

console.log('创新俄罗斯方块游戏启动');
console.log('游戏尺寸:', SCREEN_WIDTH, 'x', SCREEN_HEIGHT);

// 导出游戏实例（用于调试）
(window as any).game = game;

