using System;

namespace GameEntry.ArtAsset;

/// <summary>
/// 🎭 模型资产清单 - 项目中确实存在并被使用的模型资源
/// 关于模型资产动作的说明：角色的动画是固定的：idle, move, die, attack_01,(可能会有attack_02……） death
/// </summary>
public static class ModelAsset
{
    #region 📂 角色模型 - Characters
    
    /// <summary>🗡️ 白衣剑客模型 - 主要剑客角色使用</summary>
    /// <remarks>
    /// 用途：李逍遥、剑客、武侠角色
    /// 标准动画：idle, move, die, attack_01, attack_02, death
    /// </remarks>
    public static readonly string BaiYiJianKe = "characters1/baiyijianke_e4wa/model.prefab";
    
    /// <summary>🐭 大耳鼠模型 - 小型角色</summary>
    /// <remarks>
    /// 用途：赵灵儿、树妖、小型NPC
    /// 标准动画：idle, move, die, attack_01, death (部分模型可能有cast动画)
    /// </remarks>
    public static readonly string DaErShu = "characters/palu_hx/sk_daershu/model.prefab";
    
    /// <summary>⚔️ 基础模型2 - 通用角色模型</summary>
    /// <remarks>
    /// 用途：林月如、蛇妖、蜀山剑侠、通用角色
    /// 标准动画：idle, move, die, attack_01, death
    /// </remarks>
    public static readonly string BasicCharacter2 = "characters/general/sk_basic2/model.prefab";
    
    #endregion
    
    #region 🐺 怪物模型 - Monsters
    
    /// <summary>🐺 狼人模型</summary>
    /// <remarks>
    /// 用途：狼人敌人
    /// 标准动画：idle, move, die, attack_01, death
    /// </remarks>
    public static readonly string Werewolf = "characters/monster/sk_werewolf/model.prefab";
    
    /// <summary>🕷️ 蜘蛛模型</summary>
    /// <remarks>
    /// 用途：蜘蛛敌人
    /// 标准动画：idle, move, die, attack_01, death
    /// </remarks>
    public static readonly string Spider = "characters/monster/sk_spider_burrow/model.prefab";
    
    /// <summary>💧 史莱姆A型</summary>
    /// <remarks>标准动画：idle, move, die, attack_01, death</remarks>
    public static readonly string SlimeA = "characters/monster/sm_slm_a/model.prefab";
    
    /// <summary>💧 史莱姆B型</summary>
    /// <remarks>标准动画：idle, move, die, attack_01, death</remarks>
    public static readonly string SlimeB = "characters/monster/sm_slm_b/model.prefab";
    
    /// <summary>💧 史莱姆C型</summary>
    /// <remarks>标准动画：idle, move, die, attack_01, death</remarks>
    public static readonly string SlimeC = "characters/monster/sm_slm_c/model.prefab";
    
    #endregion
    
    #region ⚔️ 武器模型 - Weapons
    
    /// <summary>🐉 龙纹剑模型</summary>
    /// <remarks>
    /// 用途：主角武器、剑客装备
    /// </remarks>
    public static readonly string DragonSword = "eqpt/weapon/sm_dr_sword_04_02/model.prefab";
    
    #endregion
    
    #region 🎒 道具模型 - Items
    
    /// <summary>🎒 背包模型</summary>
    /// <remarks>
    /// 用途：掉落物品、背包道具
    /// </remarks>
    public static readonly string Backpack = "deco/handpaintedvol2_campsite/sm_backpack_01_base/model.prefab";
    
    #endregion
    
    #region 🏗️ 建筑模型 - Buildings
    
    /// <summary>🔴 红色建造网格</summary>
    public static readonly string BuildGridRed = "deco/engine/SM_Plane_A01/model.prefab";
    
    /// <summary>🟢 绿色建造网格</summary>
    public static readonly string BuildGridGreen = "deco/engine/SM_Plane_A02/model.prefab";
    
    /// <summary>🟡 黄色建造网格</summary>
    public static readonly string BuildGridYellow = "deco/engine/SM_Plane_A03/model.prefab";
    
    /// <summary>🗼 石塔A03</summary>
    public static readonly string StoneTowerA03 = "deco/dungeon/sk_garden_stonetower_a03/model.prefab";
    
    /// <summary>🗼 石塔A</summary>
    public static readonly string StoneTowerA = "deco/dungeon/sk_garden_stonetower_a/model.prefab";
    
    /// <summary>🗼 石塔A05</summary>
    public static readonly string StoneTowerA05 = "deco/dungeon/sk_garden_stonetower_a05/model.prefab";
    
    /// <summary>🗼 石塔A04</summary>
    public static readonly string StoneTowerA04 = "deco/dungeon/sk_garden_stonetower_a04/model.prefab";
    
    #endregion
    
    #region 📋 使用统计 - Usage Statistics
    
    /// <summary>
    /// 📊 模型使用频率统计
    /// </summary>
    public static class UsageStats
    {
        /// <summary>高频使用模型 - 推荐优先使用</summary>
        public static readonly string[] HighFrequency = [
            BaiYiJianKe,        // 白衣剑客 - 主角首选
            BasicCharacter2,    // 基础模型 - 通用角色
            DragonSword,        // 龙纹剑 - 主武器
        ];
        
        /// <summary>中频使用模型</summary>
        public static readonly string[] MediumFrequency = [
            DaErShu,           // 大耳鼠 - 特殊角色
            Werewolf,          // 狼人 - 敌人
            SlimeC,            // 史莱姆C - 敌人
        ];
        
        /// <summary>低频使用模型</summary>
        public static readonly string[] LowFrequency = [
            Spider,            // 蜘蛛 - 特殊敌人
            SlimeA, SlimeB,    // 其他史莱姆
            Backpack,          // 道具
        ];
    }
    
    #endregion
    
    #region 🎬 动画映射参考 - Animation Reference
    
    /// <summary>
    /// 🎭 各模型支持的标准动画列表参考
    /// 注意：所有角色模型都遵循标准动画命名规范
    /// </summary>
    public static class AnimationReference
    {
        /// <summary>📋 标准角色动画 (适用于所有角色模型)</summary>
        public static readonly string[] StandardAnimations = [
            "idle",          // 站立待机
            "move",          // 移动跑步
            "die",           // 受击/被攻击
            "attack_01",     // 普通攻击1
            "attack_02",     // 普通攻击2 (部分模型可能没有)
            "death"          // 死亡动画
        ];
        
        /// <summary>🗡️ 高级剑客模型额外动画 (BaiYiJianKe)</summary>
        /// <remarks>白衣剑客模型可能支持更多攻击动画变体</remarks>
        public static readonly string[] AdvancedSwordsmanAnimations = [
            "attack_01",     // 基础攻击
            "attack_02",     // 进阶攻击 
            // 可能还有 attack_03, attack_04 等
        ];
        
        /// <summary>🎭 特殊动画 (部分模型专有)</summary>
        public static readonly string[] SpecialAnimations = [
            "cast",          // 施法动画 (法师类角色，如 DaErShu)
            "skill",         // 技能动画 (部分高级模型)
            "block",         // 格挡动画 (战士类)
            "run",           // 奔跑动画 (区别于 move)
        ];
        
        /// <summary>⚠️ 动画映射说明</summary>
        /// <remarks>
        /// 在 GameDataModel.AnimationMappings 中配置时：
        /// - AnimationRaw: 模型文件中的实际动画名称
        /// - AnimationAlias: 游戏逻辑中使用的别名
        /// 例如：{ AnimationRaw = "attack_01"u8, AnimationAlias = "attack"u8 }
        /// </remarks>
        public static readonly string[] MappingExamples = [
            "// 标准映射示例:",
            "// new() { AnimationRaw = \"idle\"u8, AnimationAlias = \"idle\"u8 }",
            "// new() { AnimationRaw = \"move\"u8, AnimationAlias = \"move\"u8 }",
            "// new() { AnimationRaw = \"attack_01\"u8, AnimationAlias = \"attack\"u8 }",
            "// new() { AnimationRaw = \"death\"u8, AnimationAlias = \"death\"u8 }"
        ];
    }
    
    #endregion
}
