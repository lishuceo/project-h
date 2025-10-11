using GameCore.Container;
using GameCore.Container.Data;
using GameCore.SceneSystem;
using GameCore.SceneSystem.Data;
using GameCore.SceneSystem.Data.Struct;
using GameData;
using System.Numerics;

namespace GameEntry.XianJianQiXiaZhuan.ScopeData;

/// <summary>
/// 仙剑奇侠传主数据配置 - 场景和整体初始化
/// 技能系统请参考 ScopeData/XianJianAbilities.cs
/// 单位系统请参考 ScopeData/XianJianUnits.cs  
/// 物品系统请参考 ScopeData/XianJianItems.cs
/// </summary>
public class XianJianScopeData : IGameClass
{
    #region 场景定义
    public static class Scene
    {
        /// <summary>仙剑奇侠传主场景 - 余杭镇</summary>
        public static readonly GameLink<GameDataScene, GameDataScene> YuHangTown = new("YuHangTown"u8);
        
        /// <summary>仙灵岛</summary>
        public static readonly GameLink<GameDataScene, GameDataScene> XianLingIsland = new("XianLingIsland"u8);
        
        /// <summary>锁妖塔</summary>
        public static readonly GameLink<GameDataScene, GameDataScene> SuoYaoTower = new("SuoYaoTower"u8);
    }
    #endregion

    #region 背包系统定义
    public static class Inventory
    {
        /// <summary>主角物品栏</summary>
        public static readonly GameLink<GameDataInventory, GameDataInventory> HeroMainInventory = new("HeroMainInventory"u8);
        
        /// <summary>主角装备栏</summary>
        public static readonly GameLink<GameDataInventory, GameDataInventory> HeroEquipInventory = new("HeroEquipInventory"u8);
        
        /// <summary>药品快捷栏</summary>
        public static readonly GameLink<GameDataInventory, GameDataInventory> HeroQuickBarInventory = new("HeroQuickBarInventory"u8);
    }
    #endregion

    public static void OnRegisterGameClass()
    {
        // 先移除可能已存在的订阅，避免重复注册
        Game.OnGameDataInitialization -= OnGameDataInitialization;
        Game.OnGameDataInitialization += OnGameDataInitialization;
    }

    private static void OnGameDataInitialization()
    {
        // 只在仙剑奇侠传模式下初始化
        if (Game.GameModeLink != GameEntry.ScopeData.GameMode.XianJianQiXiaZhuan)
        {
            return;
        }

        Game.Logger.LogInformation("🗡️ 初始化仙剑奇侠传游戏系统...");

        // 初始化各个子系统
        InitializeInventories();     // 背包系统
        InitializeScenes();          // 场景系统

        Game.Logger.LogInformation("✅ 仙剑奇侠传系统初始化完成!");
    }

    /// <summary>
    /// 初始化仙剑场景配置
    /// </summary>
    private static void InitializeScenes()
    {
        Game.Logger.LogInformation("🌅 配置仙剑奇侠传场景...");

        // ========== 余杭镇场景配置 ==========
        _ = new GameDataScene(Scene.YuHangTown)
        {
            DefaultCamera = GameEntry.ScopeData.Camera.DefaultCamera,
            Name = "余杭镇",
            HostedSceneTag = "default"u8, // 使用默认场景资源
            Size = new(8192, 8192), // 大型开放世界地图
            OnLoaded = static (scene) => Game.Logger.LogInformation("🏘️ 余杭镇场景 {scene} 已加载", scene),
            PlacedPlayerObjects = new()
            {
                // ========== 🏠 逍遥客栈区域 - 世界中心 (4096, 4096) ==========
                // 主角 - 李逍遥：站在世界中心的逍遥客栈
                {
                    1, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.LiXiaoYao,
                        OwnerPlayerId = 1,
                        Position = new(4096, 4096, 0), // 世界正中心
                        IsMainUnit = true,
                        TriggerGetter = true,
                        UniqueId = 1,
                    }
                },
                // 赵灵儿：在客栈内休息
                {
                    2, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.ZhaoLingEr,
                        OwnerPlayerId = 1,
                        Position = new(3950, 4150, 0), // 客栈西侧
                        TriggerGetter = true,
                        UniqueId = 2,
                    }
                },
                // 林月如：在客栈外练剑
                {
                    3, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.LinYueRu,
                        OwnerPlayerId = 1,
                        Position = new(4240, 4150, 0), // 客栈东侧
                        TriggerGetter = true,
                        UniqueId = 3,
                    }
                },

                // ========== 🌿 外围荒野区域 - 普通怪物分布 ==========
                // 东部荒野 - 蛇妖群落
                {
                    20, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.SnakeDemon,
                        OwnerPlayerId = 0,
                        Position = new(5500, 4000, 0), // 东部荒野
                        TriggerGetter = true,
                        UniqueId = 20,
                    }
                },
                {
                    21, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.SnakeDemon,
                        OwnerPlayerId = 0,
                        Position = new(5700, 4300, 0), // 东部荒野深处
                        TriggerGetter = true,
                        UniqueId = 21,
                    }
                },
                
                // 西部荒野 - 树妖森林
                {
                    22, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.TreeDemon,
                        OwnerPlayerId = 0,
                        Position = new(2700, 4000, 0), // 西部森林
                        TriggerGetter = true,
                        UniqueId = 22,
                    }
                },
                {
                    23, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.TreeDemon,
                        OwnerPlayerId = 0,
                        Position = new(2400, 4300, 0), // 西部森林深处
                        TriggerGetter = true,
                        UniqueId = 23,
                    }
                },

                // 南部荒野 - 混合怪物区
                {
                    24, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.SnakeDemon,
                        OwnerPlayerId = 0,
                        Position = new(4096, 2800, 0), // 南部入口
                        TriggerGetter = true,
                        UniqueId = 24,
                    }
                },
                {
                    25, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.TreeDemon,
                        OwnerPlayerId = 0,
                        Position = new(4300, 2500, 0), // 南部深处
                        TriggerGetter = true,
                        UniqueId = 25,
                    }
                },

                // ========== ⚔️ 蜀山势力区域 - 北部高地 ==========
                // 蜀山剑士：在北部巡逻，保护一方安宁
                {
                    30, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.ShuShanSwordsman,
                        OwnerPlayerId = 1, // 友方
                        Position = new(4096, 5500, 0), // 北部高地中心
                        TriggerGetter = true,
                        UniqueId = 30,
                    }
                },
                {
                    31, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.ShuShanSwordsman,
                        OwnerPlayerId = 1,
                        Position = new(3800, 5800, 0), // 北部高地西侧
                        TriggerGetter = true,
                        UniqueId = 31,
                    }
                },
                {
                    32, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.ShuShanSwordsman,
                        OwnerPlayerId = 1,
                        Position = new(4400, 5800, 0), // 北部高地东侧
                        TriggerGetter = true,
                        UniqueId = 32,
                    }
                },

                // ========== 👹 邪恶势力四方镇守 - 地图边缘 ==========
                // 黑无常：镇守西南边界，阴气森森
                {
                    40, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.HeiWuChang,
                        OwnerPlayerId = 0,
                        Position = new(1200, 1200, 0), // 西南角边界
                        TriggerGetter = true,
                        UniqueId = 40,
                    }
                },
                // 白无常：镇守东南边界，与黑无常呼应
                {
                    41, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.BaiWuChang,
                        OwnerPlayerId = 0,
                        Position = new(6900, 1200, 0), // 东南角边界
                        TriggerGetter = true,
                        UniqueId = 41,
                    }
                },
                // 黑无常分身：镇守西北边界
                {
                    42, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.HeiWuChang,
                        OwnerPlayerId = 0,
                        Position = new(1200, 6900, 0), // 西北角边界
                        TriggerGetter = true,
                        UniqueId = 42,
                    }
                },
                // 白无常分身：镇守东北边界
                {
                    43, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.BaiWuChang,
                        OwnerPlayerId = 0,
                        Position = new(6900, 6900, 0), // 东北角边界
                        TriggerGetter = true,
                        UniqueId = 43,
                    }
                },

                // ========== 🔥 终极挑战区域 - 地图最北端 ==========
                // 剑魔：隐藏在最北端的禁地
                {
                    50, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.JianMo,
                        OwnerPlayerId = 0,
                        Position = new(4096, 7500, 0), // 最北端禁地
                        TriggerGetter = true,
                        UniqueId = 50,
                    }
                },

                // ========== 🧙 仙人圣地 - 特殊高地区域 ==========
                // 姜子牙：坐镇东北仙人台，指点迷津
                {
                    60, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.JiangZiYa,
                        OwnerPlayerId = 1,
                        Position = new(6200, 6200, 0), // 东北仙人台
                        TriggerGetter = true,
                        UniqueId = 60,
                    }
                },
                // 剑仙：在西北仙山修炼，传授高级剑术
                {
                    61, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.JianXian,
                        OwnerPlayerId = 1,
                        Position = new(2000, 6200, 0), // 西北仙山
                        TriggerGetter = true,
                        UniqueId = 61,
                    }
                },

                // ========== 💎 宝物分布 - 探索奖励 ==========
                // 镇妖剑：放在客栈附近，玩家一开始就能获得
                {
                    99, new PlacedItem()
                    {
                        Link = XianJianItems.Item.ZhenYaoJian,
                        Position = new(4200, 4000, 0), // 客栈东南侧，容易发现
                        TriggerGetter = true,
                        UniqueId = 99,
                    }
                },
                // 客栈区域宝物
                {
                    100, new PlacedItem()
                    {
                        Link = XianJianItems.Item.XianJian,
                        Position = new(4096, 3800, 0), // 客栈南侧
                        TriggerGetter = true,
                        UniqueId = 100,
                    }
                },
                // 东部探索奖励
                {
                    101, new PlacedItem()
                    {
                        Link = XianJianItems.Item.XianJian,
                        Position = new(6000, 4096, 0), // 东部深处
                        TriggerGetter = true,
                        UniqueId = 101,
                    }
                },
                // 西部探索奖励
                {
                    102, new PlacedItem()
                    {
                        Link = XianJianItems.Item.XianJian,
                        Position = new(2200, 4096, 0), // 西部深处
                        TriggerGetter = true,
                        UniqueId = 102,
                    }
                },
                // 南部探索奖励
                {
                    103, new PlacedItem()
                    {
                        Link = XianJianItems.Item.XianJian,
                        Position = new(4096, 2000, 0), // 南部边境
                        TriggerGetter = true,
                        UniqueId = 103,
                    }
                },
                // 北部探索奖励
                {
                    104, new PlacedItem()
                    {
                        Link = XianJianItems.Item.XianJian,
                        Position = new(4096, 6000, 0), // 北部高地
                        TriggerGetter = true,
                        UniqueId = 104,
                    }
                },
                // 终极宝物：在剑魔附近的隐秘宝藏
                {
                    105, new PlacedItem()
                    {
                        Link = XianJianItems.Item.XianJian,
                        Position = new(4300, 7300, 0), // 禁地宝藏
                        TriggerGetter = true,
                        UniqueId = 105,
                    }
                },
                // 第二把镇妖剑：放置在北部高地，给探索玩家的额外奖励
                {
                    106, new PlacedItem()
                    {
                        Link = XianJianItems.Item.ZhenYaoJian,
                        Position = new(4096, 5800, 0), // 北部高地中心
                        TriggerGetter = true,
                        UniqueId = 106,
                    }
                }
            }
        };

        // ========== 仙灵岛场景配置 ==========
        _ = new GameDataScene(Scene.XianLingIsland)
        {
            DefaultCamera = GameEntry.ScopeData.Camera.DefaultCamera,
            Name = "仙灵岛",
            HostedSceneTag = "ai_test_scene"u8, // 使用AI测试场景资源（比较大的地图）
            Size = new(32 * 256, 32 * 256), // 更大的仙境地图
            OnLoaded = static (scene) => Game.Logger.LogInformation("🏝️ 仙灵岛场景 {scene} 已加载", scene),
            PlacedPlayerObjects = new()
            {
                // 主角传送到仙灵岛
                {
                    1, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.LiXiaoYao,
                        OwnerPlayerId = 1,
                        Position = new(8192, 8192, 0), // 仙灵岛中央
                        IsMainUnit = true,
                        TriggerGetter = true,
                        UniqueId = 1,
                    }
                }
            }
        };

        // ========== 锁妖塔场景配置 ==========
        _ = new GameDataScene(Scene.SuoYaoTower)
        {
            DefaultCamera = GameEntry.ScopeData.Camera.DefaultCamera,
            Name = "锁妖塔",
            HostedSceneTag = "primitive_shape_test_scene"u8, // 使用形状测试场景
            Size = new(16 * 256, 16 * 256),
            OnLoaded = static (scene) => Game.Logger.LogInformation("🗼 锁妖塔场景 {scene} 已加载", scene),
            PlacedPlayerObjects = new()
            {
                // 主角进入锁妖塔
                {
                    1, new PlacedUnit()
                    {
                        Link = XianJianUnits.Unit.LiXiaoYao,
                        OwnerPlayerId = 1,
                        Position = new(2048, 2048, 0), // 塔底入口位置
                        IsMainUnit = true,
                        TriggerGetter = true,
                        UniqueId = 1,
                    }
                }
            }
        };

        Game.Logger.LogInformation("✅ 仙剑奇侠传场景配置完成!");
    }

    /// <summary>
    /// 初始化仙剑背包系统配置
    /// </summary>
    private static void InitializeInventories()
    {
        Game.Logger.LogInformation("💼 配置仙剑奇侠传背包系统...");

        // ========== 主角物品栏配置 ==========
        _ = new GameDataInventory(Inventory.HeroMainInventory)
        {
            Name = "物品栏",
            Slots = [
                new (), new (), new (), new (), new (), new (), // 第一行 6格
                new (), new (), new (), new (), new (), new (), // 第二行 6格
                new (), new (), new (), new (), new (), new (), // 第三行 6格
                new (), new (), new (), new (), new (), new (), // 第四行 6格
            ] // 总共24格，符合仙剑传统
        };

        // ========== 主角装备栏配置 ==========
        _ = new GameDataInventory(Inventory.HeroEquipInventory)
        {
            Name = "装备栏",
            InventoryFlags = new()
            {
                HandlePickUpRequest = false // 装备栏不自动拾取，需要手动装备
            },
            Slots = [
                new () // 武器槽 - 剑
                {
                    Type = ItemSlotType.Equip,
                    // 暂时移除类别限制，测试装备系统基本功能
                },
                new () // 防具槽 - 衣服
                {
                    Type = ItemSlotType.Equip,
                },
                new () // 护腕槽 - 护腕
                {
                    Type = ItemSlotType.Equip,
                },
                new () // 靴子槽 - 鞋
                {
                    Type = ItemSlotType.Equip,
                },
                new () // 饰品槽1 - 戒指
                {
                    Type = ItemSlotType.Equip,
                },
                new () // 饰品槽2 - 项链
                {
                    Type = ItemSlotType.Equip,
                },
            ]
        };

        // ========== 药品快捷栏配置 ==========
        _ = new GameDataInventory(Inventory.HeroQuickBarInventory)
        {
            Name = "药品快捷栏",
            InventoryFlags = new()
            {
                HandlePickUpRequest = false // 快捷栏不自动拾取
            },
            Slots = [
                new (), // 药品槽1
                new (), // 药品槽2
                new (), // 药品槽3
                new (), // 药品槽4
            ]
        };

        Game.Logger.LogInformation("✅ 仙剑奇侠传背包系统配置完成!");
        Game.Logger.LogInformation("   - 物品栏: 24格存储空间");
        Game.Logger.LogInformation("   - 装备栏: 6个装备槽位");
        Game.Logger.LogInformation("   - 快捷栏: 4个药品槽位");
    }
}
