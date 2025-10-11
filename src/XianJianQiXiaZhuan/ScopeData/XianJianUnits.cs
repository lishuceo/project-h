using EngineInterface.BaseType;
using GameCore.AbilitySystem.Data;
using GameCore.SceneSystem;
using GameCore.AbilitySystem.Data.Enum;
using GameCore.ActorSystem.Data;
using GameCore.AISystem.Data;
using GameCore.AISystem.Data.Enum;
using GameCore.CollisionSystem.Data.Struct;
using GameCore.CollisionSystem.Data.Enum;
using GameCore.Behavior;
using GameCore.BuffSystem.Data;
using GameCore.Container;
using GameCore.Container.Data;
using GameCore.CooldownSystem.Data;
using GameCore.Data;
using GameCore.EntitySystem.Data.Enum;
using GameCore.Execution.Data;
using GameCore.Leveling.Data;
using GameCore.Execution.Data.Enum;
using GameCore.GameSystem.Data;
using GameCore.PlayerAndUsers.Enum;
using GameCore.ResourceType.Data;
using GameCore.ResourceType.Data.Enum;
using GameCore.SceneSystem.Data;
using GameCore.SceneSystem.Data.Struct;
using GameData;
using GameCore.TargetingSystem.Data;
using GameCore.ActorSystem.Data.Enum;
using GameCore.Animation.Enum;
using GameCore.ModelAnimation.Data;
using GameCore;
using GameCore.AbilitySystem.Manager;
using GameCore.EntitySystem;
using GameCore.OrderSystem;
using System.Numerics;
using static GameCore.ScopeData;
using GameEntry.ArtAsset;

namespace GameEntry.XianJianQiXiaZhuan.ScopeData;

/// <summary>
/// 仙剑奇侠传单位系统定义
/// </summary>
public class XianJianUnits : IGameClass
{
    #region 单位定义
    public static class Unit
    {
        /// <summary>李逍遥 - 主角</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> LiXiaoYao = new("LiXiaoYao"u8);
        
        /// <summary>赵灵儿 - 女主角，拥有强大的仙术</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> ZhaoLingEr = new("ZhaoLingEr"u8);
        
        /// <summary>林月如 - 女剑客，善用剑法</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> LinYueRu = new("LinYueRu"u8);
        
        /// <summary>蛇妖 - 敌对怪物</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> SnakeDemon = new("SnakeDemon"u8);
        
        /// <summary>树妖 - 敌对怪物</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> TreeDemon = new("TreeDemon"u8);
        
        /// <summary>蜀山剑侠 - 中立NPC</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> ShuShanSwordsman = new("ShuShanSwordsman"u8);
        
        /// <summary>黑无常 - 强力敌人BOSS</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> HeiWuChang = new("HeiWuChang"u8);
        
        /// <summary>白无常 - 强力敌人BOSS</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> BaiWuChang = new("BaiWuChang"u8);
        
        /// <summary>剑魔 - 终极BOSS</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> JianMo = new("JianMo"u8);
        
        /// <summary>姜子牙 - 智者NPC导师</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> JiangZiYa = new("JiangZiYa"u8);
        
        /// <summary>剑仙 - 高级NPC</summary>
        public static readonly GameLink<GameDataUnit, GameDataUnit> JianXian = new("JianXian"u8);
    }
    #endregion

    #region 模型定义
    public static class Model
    {
        /// <summary>李逍遥模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> LiXiaoYaoModel = new("LiXiaoYaoModel"u8);
        
        /// <summary>赵灵儿模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> ZhaoLingErModel = new("ZhaoLingErModel"u8);
        
        /// <summary>林月如模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> LinYueRuModel = new("LinYueRuModel"u8);
        
        /// <summary>蛇妖模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> SnakeDemonModel = new("SnakeDemonModel"u8);
        
        /// <summary>树妖模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> TreeDemonModel = new("TreeDemonModel"u8);
        
        /// <summary>蜀山剑侠模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> ShuShanSwordsmanModel = new("ShuShanSwordsmanModel"u8);
        
        /// <summary>黑无常模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> HeiWuChangModel = new("HeiWuChangModel"u8);
        
        /// <summary>白无常模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> BaiWuChangModel = new("BaiWuChangModel"u8);
        
        /// <summary>剑魔模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> JianMoModel = new("JianMoModel"u8);
        
        /// <summary>姜子牙模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> JiangZiYaModel = new("JiangZiYaModel"u8);
        
        /// <summary>剑仙模型</summary>
        public static readonly GameLink<GameDataModel, GameDataModel> JianXianModel = new("JianXianModel"u8);
    }
    #endregion

    #region 等级系统定义
    public static class UnitLeveling
    {
        /// <summary>主角等级系统</summary>
        public static readonly GameLink<GameDataUnitLeveling, GameDataUnitLeveling> HeroLeveling = new("HeroLeveling"u8);
        
        /// <summary>普通敌人等级系统</summary>
        public static readonly GameLink<GameDataUnitLeveling, GameDataUnitLeveling> NormalEnemyLeveling = new("NormalEnemyLeveling"u8);
        
        /// <summary>精英敌人等级系统</summary>
        public static readonly GameLink<GameDataUnitLeveling, GameDataUnitLeveling> EliteEnemyLeveling = new("EliteEnemyLeveling"u8);
        
        /// <summary>BOSS等级系统</summary>
        public static readonly GameLink<GameDataUnitLeveling, GameDataUnitLeveling> BossLeveling = new("BossLeveling"u8);
        
        /// <summary>NPC等级系统</summary>
        public static readonly GameLink<GameDataUnitLeveling, GameDataUnitLeveling> NPCLeveling = new("NPCLeveling"u8);
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

        Game.Logger.LogInformation("👥 初始化仙剑奇侠传角色系统...");

        // 初始化各个子系统
        InitializeModels();        // 模型系统
        InitializeLevelingSystems();  // 等级系统
        InitializeUnits();         // 单位系统

        Game.Logger.LogInformation("✅ 仙剑奇侠传角色系统初始化完成!");
    }

    /// <summary>
    /// 初始化模型配置
    /// </summary>
    private static void InitializeModels()
    {
        Game.Logger.LogInformation("🎭 配置仙剑角色模型...");

        // ========== 李逍遥模型 ==========
        _ = new GameDataModel(Model.LiXiaoYaoModel)
        {
            Name = "李逍遥",
            Radius = 52,
            Asset = "characters1/baiyijianke_e4wa/model.prefab"u8, // 白衣剑客 - 完美匹配主角形象
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "walk"u8, AnimationAlias = "walk"u8 }, // 增加行走动画
                new() { AnimationRaw = "die"u8, AnimationAlias = "die"u8 },
                new() { AnimationRaw = "attack_01"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "attack_02"u8, AnimationAlias = "skill1"u8 },
                new() { AnimationRaw = "skill_01"u8, AnimationAlias = "skill2"u8 }, // 增加更多技能动画
                new() { AnimationRaw = "skill_02"u8, AnimationAlias = "skill3"u8 }
            ]
        };

        // ========== 赵灵儿模型 ==========
        _ = new GameDataModel(Model.ZhaoLingErModel)
        {
            Name = "赵灵儿",
            Radius = 45,
            Asset = "characters/palu_hx/sk_daershu/model.prefab"u8, // ModelAsset.DaErShu
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "die"u8, AnimationAlias = "die"u8 },
                new() { AnimationRaw = "attack_01"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "cast"u8, AnimationAlias = "cast"u8 }, // 保留特殊施法动画
                new() { AnimationRaw = "death"u8, AnimationAlias = "death"u8 }
            ]
        };

        // ========== 林月如模型 ==========
        _ = new GameDataModel(Model.LinYueRuModel)
        {
            Name = "林月如",
            Radius = 48,
            Asset = "characters/general/sk_basic2/model.prefab"u8, // ModelAsset.BasicCharacter2
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "die"u8, AnimationAlias = "die"u8 },
                new() { AnimationRaw = "attack_01"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "attack_02"u8, AnimationAlias = "skill1"u8 }, // 技能动画
                new() { AnimationRaw = "death"u8, AnimationAlias = "death"u8 }
            ]
        };

        // ========== 蛇妖模型 ==========
        _ = new GameDataModel(Model.SnakeDemonModel)
        {
            Name = "蛇妖",
            Radius = 60,
            Asset = "characters/general/sk_basic2/model.prefab"u8, // ModelAsset.BasicCharacter2
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "die"u8, AnimationAlias = "die"u8 },
                new() { AnimationRaw = "attack_01"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "death"u8, AnimationAlias = "death"u8 }
            ]
        };

        // ========== 树妖模型 ==========
        _ = new GameDataModel(Model.TreeDemonModel)
        {
            Name = "树妖",
            Radius = 80,
            Asset = "characters/palu_hx/sk_daershu/model.prefab"u8, // ModelAsset.DaErShu
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "die"u8, AnimationAlias = "die"u8 },
                new() { AnimationRaw = "attack_01"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "cast"u8, AnimationAlias = "cast"u8 }, // 保留特殊施法动画
                new() { AnimationRaw = "death"u8, AnimationAlias = "death"u8 }
            ]
        };

        // ========== 蜀山剑侠模型 ==========
        _ = new GameDataModel(Model.ShuShanSwordsmanModel)
        {
            Name = "蜀山剑侠",
            Radius = 52,
            Asset = "characters/general/sk_basic2/model.prefab"u8, // ModelAsset.BasicCharacter2
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "die"u8, AnimationAlias = "die"u8 },
                new() { AnimationRaw = "attack_01"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "attack_02"u8, AnimationAlias = "skill1"u8 }, // 技能动画
                new() { AnimationRaw = "death"u8, AnimationAlias = "death"u8 }
            ]
        };

        // ========== 黑无常模型 ==========
        _ = new GameDataModel(Model.HeiWuChangModel)
        {
            Name = "黑无常",
            Radius = 65,
            Asset = "characters1/heiwuchang_o8ua/model.prefab"u8, // 黑无常 - 阴间勾魂使者
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "die"u8, AnimationAlias = "die"u8 },
                new() { AnimationRaw = "die_02"u8, AnimationAlias = "death"u8 },
                new() { AnimationRaw = "attack_01"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "attack_02"u8, AnimationAlias = "attack2"u8 },
                new() { AnimationRaw = "skill_01"u8, AnimationAlias = "skill1"u8 },
                new() { AnimationRaw = "skill_02"u8, AnimationAlias = "skill2"u8 },
                new() { AnimationRaw = "skill_03"u8, AnimationAlias = "skill3"u8 },
                new() { AnimationRaw = "skill_04"u8, AnimationAlias = "skill4"u8 },
                new() { AnimationRaw = "skill_05"u8, AnimationAlias = "skill5"u8 },
                new() { AnimationRaw = "skill_06"u8, AnimationAlias = "skill6"u8 },
                new() { AnimationRaw = "skill_06_loop"u8, AnimationAlias = "skill6_loop"u8 }
            ]
        };

        // ========== 白无常模型 ==========
        _ = new GameDataModel(Model.BaiWuChangModel)
        {
            Name = "白无常",
            Radius = 65,
            Asset = "characters1/baiwuchang_l2bb/model.prefab"u8, // 白无常 - 阴间勾魂使者
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "move_02"u8, AnimationAlias = "float"u8 }, // 漂浮移动
                new() { AnimationRaw = "die"u8, AnimationAlias = "die"u8 },
                new() { AnimationRaw = "attack_01"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "attack_02"u8, AnimationAlias = "attack2"u8 },
                new() { AnimationRaw = "skill_01"u8, AnimationAlias = "skill1"u8 },
                new() { AnimationRaw = "skill_02"u8, AnimationAlias = "skill2"u8 },
                new() { AnimationRaw = "skill_03"u8, AnimationAlias = "skill3"u8 },
                new() { AnimationRaw = "skill_03_loop"u8, AnimationAlias = "skill3_loop"u8 },
                new() { AnimationRaw = "skill_04"u8, AnimationAlias = "skill4"u8 },
                new() { AnimationRaw = "skill_04_loop"u8, AnimationAlias = "skill4_loop"u8 }
            ]
        };

        // ========== 剑魔模型 ==========
        _ = new GameDataModel(Model.JianMoModel)
        {
            Name = "剑魔",
            Radius = 60,
            Asset = "characters1/guofeng_002_do9b/model.prefab"u8, // 剑魔 - 终极BOSS
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "die"u8, AnimationAlias = "die"u8 },
                new() { AnimationRaw = "attack_01"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "attack_02"u8, AnimationAlias = "attack2"u8 },
                new() { AnimationRaw = "attack_03"u8, AnimationAlias = "attack3"u8 },
                new() { AnimationRaw = "skill_01"u8, AnimationAlias = "darkskill1"u8 },
                new() { AnimationRaw = "skill_02"u8, AnimationAlias = "darkskill2"u8 }
            ]
        };

        // ========== 姜子牙模型 ==========
        _ = new GameDataModel(Model.JiangZiYaModel)
        {
            Name = "姜子牙",
            Radius = 55,
            Asset = "characters1/jiangziya_e49b/model.prefab"u8, // 姜子牙 - 智者导师
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 },
                new() { AnimationRaw = "die"u8, AnimationAlias = "die"u8 },
                new() { AnimationRaw = "die_02"u8, AnimationAlias = "death"u8 },
                new() { AnimationRaw = "attack_01"u8, AnimationAlias = "attack"u8 },
                new() { AnimationRaw = "attack_02"u8, AnimationAlias = "staff_attack"u8 },
                new() { AnimationRaw = "skill_01"u8, AnimationAlias = "wisdom_skill"u8 },
                new() { AnimationRaw = "skill_02"u8, AnimationAlias = "divine_skill"u8 }
            ]
        };

        // ========== 剑仙模型 ==========
        _ = new GameDataModel(Model.JianXianModel)
        {
            Name = "剑仙",
            Radius = 55,
            Asset = "characters1/jianxian_9vt3/model.prefab"u8, // 剑仙 - 超凡脱俗
            ShadowSetting = new()
            {
                ShadowType = ShadowType.DeviceDependentShadow,
            },
            AnimationMappings = [
                new() { AnimationRaw = "idle"u8, AnimationAlias = "idle"u8 },
                new() { AnimationRaw = "move"u8, AnimationAlias = "move"u8 }
                // 注意：剑仙模型动画较少，保持简洁
            ]
        };

        Game.Logger.LogInformation("✅ 仙剑角色模型配置完成!");
    }

    /// <summary>
    /// 初始化等级系统配置
    /// </summary>
    private static void InitializeLevelingSystems()
    {
        Game.Logger.LogInformation("📈 配置仙剑等级系统...");

        // ========== 主角等级系统 ==========
        _ = new GameDataUnitLeveling(UnitLeveling.HeroLeveling)
        {
            ExperienceRequiredForEachLevel = [
                0,      // 1级 - 起始等级
                100,    // 2级
                300,    // 3级
                600,    // 4级
                1000,   // 5级
                1500,   // 6级
                2100,   // 7级
                2800,   // 8级
                3600,   // 9级
                4500,   // 10级
                5500,   // 11级
                6600,   // 12级
                7800,   // 13级
                9100,   // 14级
                10500,  // 15级
            ],
            Modifications = new()
            {
                // 每级成长属性
                new() { Property = UnitProperty.LifeMax, SubType = PropertySubType.Base, Value = (context) => 50.0 + context.Level * 25.0 }, // 生命值成长
                new() { Property = UnitProperty.ManaMax, SubType = PropertySubType.Base, Value = (context) => 30.0 + context.Level * 20.0 }, // 法力值成长
                new() { Property = UnitProperty.AttackDamage, SubType = PropertySubType.Base, Value = (context) => 20.0 + context.Level * 8.0 }, // 攻击力成长
                new() { Property = UnitProperty.Armor, SubType = PropertySubType.Base, Value = (context) => 5.0 + context.Level * 2.0 }, // 防御力成长
                new() { Property = UnitProperty.MagicResistance, SubType = PropertySubType.Base, Value = (context) => 5.0 + context.Level * 2.0 }, // 魔抗成长
            }
        };


        // ========== 普通敌人等级系统 ==========
        _ = new GameDataUnitLeveling(UnitLeveling.NormalEnemyLeveling)
        {
            ExperienceRequiredForEachLevel = [
                0,    // 1级
                120,  // 2级
                280,  // 3级
                480,  // 4级
                720,  // 5级
            ],
            Modifications = new()
            {
                // 普通敌人属性成长
                new() { Property = UnitProperty.LifeMax, SubType = PropertySubType.Base, Value = (context) => 80.0 + context.Level * 25.0 },
                new() { Property = UnitProperty.AttackDamage, SubType = PropertySubType.Base, Value = (context) => 18.0 + context.Level * 6.0 },
                new() { Property = UnitProperty.Armor, SubType = PropertySubType.Base, Value = (context) => 3.0 + context.Level * 1.5 },
            }
        };

        // ========== 精英敌人等级系统 ==========
        _ = new GameDataUnitLeveling(UnitLeveling.EliteEnemyLeveling)
        {
            ExperienceRequiredForEachLevel = [
                0,    // 1级
                400,  // 2级
                900,  // 3级
                1500, // 4级
                2200, // 5级
                3000, // 6级
                3900, // 7级
                4900, // 8级
            ],
            Modifications = new()
            {
                // 精英敌人属性成长
                new() { Property = UnitProperty.LifeMax, SubType = PropertySubType.Base, Value = (context) => 200.0 + context.Level * 60.0 },
                new() { Property = UnitProperty.AttackDamage, SubType = PropertySubType.Base, Value = (context) => 30.0 + context.Level * 12.0 },
                new() { Property = UnitProperty.Armor, SubType = PropertySubType.Base, Value = (context) => 8.0 + context.Level * 3.0 },
                new() { Property = UnitProperty.MagicResistance, SubType = PropertySubType.Base, Value = (context) => 6.0 + context.Level * 2.5 },
            }
        };

        // ========== BOSS等级系统 ==========
        _ = new GameDataUnitLeveling(UnitLeveling.BossLeveling)
        {
            ExperienceRequiredForEachLevel = [
                0,     // 1级
                800,   // 2级
                1800,  // 3级
                3000,  // 4级
                4400,  // 5级
                6000,  // 6级
                7800,  // 7级
                9800,  // 8级
                12000, // 9级
                14400, // 10级
            ],
            Modifications = new()
            {
                // BOSS属性成长
                new() { Property = UnitProperty.LifeMax, SubType = PropertySubType.Base, Value = (context) => 500.0 + context.Level * 120.0 },
                new() { Property = UnitProperty.AttackDamage, SubType = PropertySubType.Base, Value = (context) => 80.0 + context.Level * 25.0 },
                new() { Property = UnitProperty.Armor, SubType = PropertySubType.Base, Value = (context) => 20.0 + context.Level * 6.0 },
                new() { Property = UnitProperty.MagicResistance, SubType = PropertySubType.Base, Value = (context) => 15.0 + context.Level * 5.0 },
            }
        };

        // ========== NPC等级系统 ==========
        _ = new GameDataUnitLeveling(UnitLeveling.NPCLeveling)
        {
            ExperienceRequiredForEachLevel = [
                0,     // 1级
                2000,  // 2级
                5000,  // 3级
                9000,  // 4级
                14000, // 5级
                20000, // 6级
                27000, // 7级
                35000, // 8级
                44000, // 9级
                54000, // 10级
                65000, // 11级
                77000, // 12级
                90000, // 13级
                104000, // 14级
                119000, // 15级
                135000, // 16级
                152000, // 17级
                170000, // 18级
                189000, // 19级
                209000, // 20级
            ],
            Modifications = new()
            {
                // NPC属性成长（仙人级别）
                new() { Property = UnitProperty.LifeMax, SubType = PropertySubType.Base, Value = (context) => 400.0 + context.Level * 80.0 },
                new() { Property = UnitProperty.ManaMax, SubType = PropertySubType.Base, Value = (context) => 500.0 + context.Level * 60.0 },
                new() { Property = UnitProperty.AttackDamage, SubType = PropertySubType.Base, Value = (context) => 60.0 + context.Level * 20.0 },
                new() { Property = UnitProperty.Armor, SubType = PropertySubType.Base, Value = (context) => 30.0 + context.Level * 8.0 },
                new() { Property = UnitProperty.MagicResistance, SubType = PropertySubType.Base, Value = (context) => 40.0 + context.Level * 10.0 },
            }
        };

        Game.Logger.LogInformation("✅ 仙剑等级系统配置完成!");
        Game.Logger.LogInformation("   - 主角等级：1-15级，全面成长");
        Game.Logger.LogInformation("   - 普通敌人：1-5级，基础威胁");
        Game.Logger.LogInformation("   - 精英敌人：1-8级，中等挑战");
        Game.Logger.LogInformation("   - BOSS敌人：1-10级，高难度");
        Game.Logger.LogInformation("   - NPC导师：1-20级，仙人级别");
    }

    /// <summary>
    /// 初始化单位配置
    /// </summary>
    private static void InitializeUnits()
    {
        Game.Logger.LogInformation("⚔️ 配置仙剑角色单位...");

        // ========== 李逍遥 - 主角 ==========
        _ = new GameDataUnit(Unit.LiXiaoYao)
        {
            Name = "李逍遥",
            AttackableRadius = 60,
            Leveling = UnitLeveling.HeroLeveling, // 使用主角等级系统
            Properties = new()
            {
                { UnitProperty.LifeMax, 150 },      // 基础生命值
                { UnitProperty.ManaMax, 100 },      // 基础法力值
                { UnitProperty.Armor, 8 },          // 基础防御
                { UnitProperty.MagicResistance, 8 }, // 基础魔抗
                { UnitProperty.MoveSpeed, 380 },    // 移动速度
                { UnitProperty.TurningSpeed, 1800 }, // 转向速度
                { UnitProperty.AttackRange, 180 },   // 攻击范围
                { UnitProperty.AttackDamage, 35 },   // 基础攻击力
                { UnitProperty.InventoryPickUpRange, 350 }, // 拾取范围
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 35,
            DynamicCollisionMask = DynamicCollisionMask.Hero | DynamicCollisionMask.Building,
            Inventories = [
                XianJianScopeData.Inventory.HeroMainInventory,
                XianJianScopeData.Inventory.HeroEquipInventory,
                XianJianScopeData.Inventory.HeroQuickBarInventory
            ],
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            Model = Model.LiXiaoYaoModel,
            Abilities = [
                XianJianAbilities.Ability.SwordSlash,        // 剑气斩
                XianJianAbilities.Ability.XianFengSpell,     // 仙风云体术
                XianJianAbilities.Ability.SwordControl,     // 御剑术
            ],
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.MAIN_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Alliance, "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.MainUnit, "$$spark_core.bloodstrip.MAIN_HERO_NONE.root"u8 },
                },
            },
        };

        // ========== 赵灵儿 - 女主角 ==========
        _ = new GameDataUnit(Unit.ZhaoLingEr)
        {
            Name = "赵灵儿",
            AttackableRadius = 55,
            Leveling = UnitLeveling.HeroLeveling,
            Properties = new()
            {
                { UnitProperty.LifeMax, 120 },       // 比李逍遥血量少
                { UnitProperty.ManaMax, 180 },       // 但法力值更高
                { UnitProperty.Armor, 5 },           // 防御较低
                { UnitProperty.MagicResistance, 15 }, // 魔抗较高
                { UnitProperty.MoveSpeed, 360 },     // 移动稍慢
                { UnitProperty.TurningSpeed, 1800 },
                { UnitProperty.AttackRange, 250 },   // 法术攻击范围更远
                { UnitProperty.AttackDamage, 25 },   // 物理攻击力较低
                { UnitProperty.InventoryPickUpRange, 300 },
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 32,
            DynamicCollisionMask = DynamicCollisionMask.Hero | DynamicCollisionMask.Building,
            Inventories = [
                XianJianScopeData.Inventory.HeroMainInventory,
                XianJianScopeData.Inventory.HeroEquipInventory,
                XianJianScopeData.Inventory.HeroQuickBarInventory
            ],
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            Model = Model.ZhaoLingErModel,
            Abilities = [
                XianJianAbilities.Ability.HealingSpell,    // 治疗术
                XianJianAbilities.Ability.LightningSpell, // 雷系法术
                XianJianAbilities.Ability.WaterSpell,     // 水系法术
            ],
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Alliance, "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.MainUnit, "$$spark_core.bloodstrip.MAIN_HERO_NONE.root"u8 },
                },
            },
        };

        // ========== 林月如 - 女剑客 ==========
        _ = new GameDataUnit(Unit.LinYueRu)
        {
            Name = "林月如",
            AttackableRadius = 58,
            Leveling = UnitLeveling.HeroLeveling,
            Properties = new()
            {
                { UnitProperty.LifeMax, 140 },       // 中等血量
                { UnitProperty.ManaMax, 80 },        // 法力值较少
                { UnitProperty.Armor, 10 },          // 防御中等
                { UnitProperty.MagicResistance, 6 }, // 魔抗较低
                { UnitProperty.MoveSpeed, 420 },     // 移动速度最快
                { UnitProperty.TurningSpeed, 2000 }, // 转向很快
                { UnitProperty.AttackRange, 160 },   // 近战范围
                { UnitProperty.AttackDamage, 45 },   // 攻击力很高
                { UnitProperty.InventoryPickUpRange, 300 },
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 34,
            DynamicCollisionMask = DynamicCollisionMask.Hero | DynamicCollisionMask.Building,
            Inventories = [
                XianJianScopeData.Inventory.HeroMainInventory,
                XianJianScopeData.Inventory.HeroEquipInventory,
                XianJianScopeData.Inventory.HeroQuickBarInventory
            ],
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            Model = Model.LinYueRuModel,
            Abilities = [
                XianJianAbilities.Ability.SwordCombo,     // 剑法连击
                XianJianAbilities.Ability.QuickStrike,   // 疾风剑
            ],
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Alliance, "$$spark_core.bloodstrip.ALLY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                    { PlayerUnitRelationShip.MainUnit, "$$spark_core.bloodstrip.MAIN_HERO_NONE.root"u8 },
                },
            },
        };

        // ========== 蛇妖 - 敌对单位 ==========
        _ = new GameDataUnit(Unit.SnakeDemon)
        {
            Name = "蛇妖",
            AttackableRadius = 50,
            Leveling = UnitLeveling.NormalEnemyLeveling,
            Properties = new()
            {
                { UnitProperty.LifeMax, 200 },
                { UnitProperty.ManaMax, 50 },
                { UnitProperty.Armor, 5 },
                { UnitProperty.MagicResistance, 3 },
                { UnitProperty.MoveSpeed, 280 },
                { UnitProperty.TurningSpeed, 1200 },
                { UnitProperty.AttackRange, 120 },
                { UnitProperty.AttackDamage, 25 },
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax }
            },
            CollisionRadius = 40,
            DynamicCollisionMask = DynamicCollisionMask.None,
            Filter = [UnitFilter.Unit],
            DeathRemovalDelay = TimeSpan.FromSeconds(8),
            Model = Model.SnakeDemonModel,
            TacticalAI = GameCore.ScopeData.AI.Default, // 使用默认AI
            Abilities = [
                XianJianAbilities.Ability.PoisonAttack, // 毒攻击
            ],
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                },
            },
        };

        // ========== 树妖 - 敌对单位 ==========
        _ = new GameDataUnit(Unit.TreeDemon)
        {
            Name = "树妖",
            AttackableRadius = 55,
            Leveling = UnitLeveling.NormalEnemyLeveling,
            Properties = new()
            {
                { UnitProperty.LifeMax, 250 },       // 比蛇妖血量高
                { UnitProperty.ManaMax, 100 },       // 法系怪物，法力较高
                { UnitProperty.Armor, 8 },           // 树皮防御
                { UnitProperty.MagicResistance, 15 }, // 天然魔抗
                { UnitProperty.MoveSpeed, 250 },     // 移动较慢
                { UnitProperty.TurningSpeed, 1000 },
                { UnitProperty.AttackRange, 200 },   // 远程攻击
                { UnitProperty.AttackDamage, 30 },
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 45,
            DynamicCollisionMask = DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Unit],
            DeathRemovalDelay = TimeSpan.FromSeconds(8),
            Model = Model.TreeDemonModel,
            TacticalAI = GameCore.ScopeData.AI.Default, // 使用默认AI
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_HERO_NONE.root"u8 },
                },
            },
        };

        // ========== 蜀山剑侠 - 中立NPC ==========
        _ = new GameDataUnit(Unit.ShuShanSwordsman)
        {
            Name = "蜀山剑侠",
            AttackableRadius = 55,
            Leveling = UnitLeveling.EliteEnemyLeveling,
            Properties = new()
            {
                { UnitProperty.LifeMax, 300 },       // 高血量
                { UnitProperty.ManaMax, 150 },
                { UnitProperty.Armor, 15 },          // 高防御
                { UnitProperty.MagicResistance, 12 },
                { UnitProperty.MoveSpeed, 350 },
                { UnitProperty.TurningSpeed, 1800 },
                { UnitProperty.AttackRange, 200 },
                { UnitProperty.AttackDamage, 40 },   // 高攻击力
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 36,
            DynamicCollisionMask = DynamicCollisionMask.Hero,
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            Model = Model.ShuShanSwordsmanModel,
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                },
            },
        };

        // ========== 黑无常 - 强力BOSS ==========
        _ = new GameDataUnit(Unit.HeiWuChang)
        {
            Name = "黑无常",
            AttackableRadius = 75,
            Leveling = UnitLeveling.BossLeveling,
            Properties = new()
            {
                { UnitProperty.LifeMax, 500 },       // 高血量BOSS
                { UnitProperty.ManaMax, 300 },       
                { UnitProperty.Armor, 25 },          // 高防御
                { UnitProperty.MagicResistance, 20 },
                { UnitProperty.MoveSpeed, 320 },     // 移动稍慢但稳重
                { UnitProperty.TurningSpeed, 1200 },
                { UnitProperty.AttackRange, 200 },   
                { UnitProperty.AttackDamage, 80 },   // 高攻击力
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 45,
            DynamicCollisionMask = DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Unit],
            DeathRemovalDelay = TimeSpan.FromSeconds(3),
            Model = Model.HeiWuChangModel,
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_BOSS_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_BOSS_NONE.root"u8 },
                },
            },
        };

        // ========== 白无常 - 强力BOSS ==========
        _ = new GameDataUnit(Unit.BaiWuChang)
        {
            Name = "白无常",
            AttackableRadius = 75,
            Leveling = UnitLeveling.BossLeveling,
            Properties = new()
            {
                { UnitProperty.LifeMax, 450 },       // 比黑无常血量稍少
                { UnitProperty.ManaMax, 350 },       // 但法力更高
                { UnitProperty.Armor, 20 },          
                { UnitProperty.MagicResistance, 30 }, // 更高魔抗
                { UnitProperty.MoveSpeed, 360 },     // 更敏捷
                { UnitProperty.TurningSpeed, 1500 },
                { UnitProperty.AttackRange, 250 },   
                { UnitProperty.AttackDamage, 70 },   
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 45,
            DynamicCollisionMask = DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Unit],
            DeathRemovalDelay = TimeSpan.FromSeconds(3),
            Model = Model.BaiWuChangModel,
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_BOSS_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_BOSS_NONE.root"u8 },
                },
            },
        };

        // ========== 剑魔 - 终极BOSS ==========
        _ = new GameDataUnit(Unit.JianMo)
        {
            Name = "剑魔",
            AttackableRadius = 80,
            Leveling = UnitLeveling.BossLeveling,
            Properties = new()
            {
                { UnitProperty.LifeMax, 800 },       // 超高血量终极BOSS
                { UnitProperty.ManaMax, 400 },       
                { UnitProperty.Armor, 35 },          // 极高防御
                { UnitProperty.MagicResistance, 25 },
                { UnitProperty.MoveSpeed, 380 },     
                { UnitProperty.TurningSpeed, 1800 },
                { UnitProperty.AttackRange, 300 },   
                { UnitProperty.AttackDamage, 120 },  // 超高攻击力
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 50,
            DynamicCollisionMask = DynamicCollisionMask.Unit,
            Filter = [UnitFilter.Unit],
            DeathRemovalDelay = TimeSpan.FromSeconds(5),
            Model = Model.JianMoModel,
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.ENEMY_BOSS_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Enemy, "$$spark_core.bloodstrip.ENEMY_BOSS_NONE.root"u8 },
                },
            },
        };

        // ========== 姜子牙 - 智者导师NPC ==========
        _ = new GameDataUnit(Unit.JiangZiYa)
        {
            Name = "姜子牙",
            AttackableRadius = 60,
            Leveling = UnitLeveling.NPCLeveling,
            Properties = new()
            {
                { UnitProperty.LifeMax, 300 },
                { UnitProperty.ManaMax, 500 },       // 高法力值智者
                { UnitProperty.Armor, 15 },          
                { UnitProperty.MagicResistance, 40 }, // 极高魔抗
                { UnitProperty.MoveSpeed, 280 },     // 老者行动稍慢
                { UnitProperty.TurningSpeed, 1000 },
                { UnitProperty.AttackRange, 350 },   // 法杖攻击距离远
                { UnitProperty.AttackDamage, 60 },   
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 40,
            DynamicCollisionMask = DynamicCollisionMask.Hero,
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            Model = Model.JiangZiYaModel,
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                },
            },
        };

        // ========== 剑仙 - 高级NPC ==========
        _ = new GameDataUnit(Unit.JianXian)
        {
            Name = "剑仙",
            AttackableRadius = 65,
            Leveling = UnitLeveling.NPCLeveling,
            Properties = new()
            {
                { UnitProperty.LifeMax, 400 },       
                { UnitProperty.ManaMax, 600 },       // 超高法力值
                { UnitProperty.Armor, 30 },          
                { UnitProperty.MagicResistance, 50 }, // 仙人级魔抗
                { UnitProperty.MoveSpeed, 400 },     // 飘逸如仙
                { UnitProperty.TurningSpeed, 2000 },
                { UnitProperty.AttackRange, 400 },   // 御剑攻击距离极远
                { UnitProperty.AttackDamage, 100 },   
            },
            UpdateFlags = new()
            {
                AllowMover = true,
                Turnable = true,
                Walkable = true,
            },
            VitalProperties = new()
            {
                { PropertyVital.Health, UnitProperty.LifeMax },
                { PropertyVital.Mana, UnitProperty.ManaMax }
            },
            CollisionRadius = 40,
            DynamicCollisionMask = DynamicCollisionMask.Hero,
            Filter = [UnitFilter.Hero, UnitFilter.Unit],
            DeathRemovalDelay = Timeout.InfiniteTimeSpan,
            Model = Model.JianXianModel,
            StatusBarSetting = new()
            {
                DefaultStatusBar = "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8,
                OverrideByRelationShip = new()
                {
                    { PlayerUnitRelationShip.Neutral, "$$spark_core.bloodstrip.NEUTRAL_HERO_NONE.root"u8 },
                },
            },
        };

        Game.Logger.LogInformation("✅ 仙剑角色单位配置完成!");
        Game.Logger.LogInformation("📊 等级分布统计：");
        Game.Logger.LogInformation("   👥 主角团队:");
        Game.Logger.LogInformation("      - 李逍遥 Lv.1 (新手剑客)");
        Game.Logger.LogInformation("      - 赵灵儿 Lv.2 (仙女法师)");
        Game.Logger.LogInformation("      - 林月如 Lv.2 (女剑客)");
        Game.Logger.LogInformation("   🐍 普通敌人:");
        Game.Logger.LogInformation("      - 蛇妖 Lv.1 (基础威胁)");
        Game.Logger.LogInformation("      - 树妖 Lv.2 (法系怪物)");
        Game.Logger.LogInformation("   ⚔️ 精英敌人:");
        Game.Logger.LogInformation("      - 蜀山剑侠 Lv.4 (中等挑战)");
        Game.Logger.LogInformation("   👹 BOSS敌人:");
        Game.Logger.LogInformation("      - 黑无常 Lv.6 (强力BOSS)");
        Game.Logger.LogInformation("      - 白无常 Lv.7 (强力BOSS)");
        Game.Logger.LogInformation("      - 剑魔 Lv.10 (终极BOSS)");
        Game.Logger.LogInformation("   🧙 仙人NPC:");
        Game.Logger.LogInformation("      - 姜子牙 Lv.15 (智者导师)");
        Game.Logger.LogInformation("      - 剑仙 Lv.20 (超凡脱俗)");
    }
}
