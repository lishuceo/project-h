import Phaser from 'phaser';
import { StartScene } from './scenes/StartScene';
import { GameScene } from './scenes/GameScene';
import { ChallengeSelectorScene } from './scenes/ChallengeSelectorScene';
import { DailyChallengeScene } from './scenes/DailyChallengeScene';
import { RankingScene } from './scenes/RankingScene';
import { SCREEN_WIDTH, SCREEN_HEIGHT } from './config/constants';
import { ChallengeManager } from './challenge/ChallengeManager';

// 使用固定的设计分辨率，让 Phaser 的 FIT 模式自动适配
const gameSize = { width: SCREEN_WIDTH, height: SCREEN_HEIGHT };

/**
 * Phaser游戏配置
 */
const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  width: gameSize.width,
  height: gameSize.height,
  parent: 'game-container',
  backgroundColor: '#1a1a2e',
  scene: [StartScene, GameScene, ChallengeSelectorScene, DailyChallengeScene, RankingScene], // 场景顺序：开始 → 游戏 → 挑战选择 → 每日挑战 → 排行榜
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
    width: gameSize.width,
    height: gameSize.height,
    parent: 'game-container',
  },
};

// 创建游戏实例
const game = new Phaser.Game(config);

console.log('🎮 俄罗斯方块流沙版 - Pixel Quicksand');
console.log('游戏尺寸:', gameSize.width, 'x', gameSize.height);
console.log('缩放模式: FIT (自动适配所有设备)');
console.log('三方向下落物理系统启动成功！');

// 导出游戏实例和调试工具
(window as any).game = game;
(window as any).gameSize = gameSize;
(window as any).debugChallengeStorage = () => {
  ChallengeManager.getInstance().debugShowStorage();
};

console.log('💡 调试提示: 在控制台输入 debugChallengeStorage() 查看挑战存储状态');

