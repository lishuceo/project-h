import Phaser from 'phaser';
import { StartScene } from './scenes/StartScene';
import { GameScene } from './scenes/GameScene';
import { ChallengeSelectorScene } from './scenes/ChallengeSelectorScene';
import { DailyChallengeScene } from './scenes/DailyChallengeScene';
import { RankingScene } from './scenes/RankingScene';
import { SCREEN_WIDTH, SCREEN_HEIGHT } from './config/constants';
import { ChallengeManager } from './challenge/ChallengeManager';

/**
 * Phaser游戏配置
 */
const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  width: SCREEN_WIDTH,
  height: SCREEN_HEIGHT,
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
    mode: Phaser.Scale.FIT, // 保持宽高比适配
    autoCenter: Phaser.Scale.CENTER_BOTH,
    width: SCREEN_WIDTH,
    height: SCREEN_HEIGHT,
    parent: 'game-container',
  },
};

// 创建游戏实例
const game = new Phaser.Game(config);

console.log('🎮 像素流沙 - Pixel Quicksand');
console.log('游戏尺寸:', SCREEN_WIDTH, 'x', SCREEN_HEIGHT);
console.log('三方向下落物理系统启动成功！');

// 导出游戏实例和调试工具（用于调试）
(window as any).game = game;
(window as any).debugChallengeStorage = () => {
  ChallengeManager.getInstance().debugShowStorage();
};

console.log('💡 调试提示: 在控制台输入 debugChallengeStorage() 查看挑战存储状态');

