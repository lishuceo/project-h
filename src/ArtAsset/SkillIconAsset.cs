using System;

namespace GameEntry.ArtAsset;

/// <summary>
/// 🎨 技能图标资产管理类
/// 管理所有技能图标的SVG文件路径
/// </summary>
public static class SkillIconAsset
{
    #region ⚔️ 剑术类技能图标
    
    /// <summary>🗡️ 剑气斩图标</summary>
    /// <remarks>
    /// 技能：李逍遥的基础攻击技能
    /// 效果：发射剑气攻击远程敌人
    /// 设计：蓝色剑气+闪光效果
    /// </remarks>
    public static readonly string SwordSlash = "GameEntry/ArtAsset/Icons/Skills/SwordSlash.svg";
    
    /// <summary>👊 百花错拳图标</summary>
    /// <remarks>
    /// 技能：林月如的连击技能
    /// 效果：连续多次攻击敌人
    /// 设计：双拳+连击轨迹线
    /// </remarks>
    public static readonly string SwordCombo = "GameEntry/ArtAsset/Icons/Skills/SwordCombo.svg";
    
    /// <summary>⚡ 疾风剑图标</summary>
    /// <remarks>
    /// 技能：林月如的高速攻击
    /// 效果：快速突进攻击敌人
    /// 设计：倾斜剑身+风旋效果
    /// </remarks>
    public static readonly string QuickStrike = "GameEntry/ArtAsset/Icons/Skills/QuickStrike.svg";
    
    #endregion
    
    #region 🔮 法术类技能图标
    
    /// <summary>💚 治疗术图标</summary>
    /// <remarks>
    /// 技能：赵灵儿的回复技能
    /// 效果：治疗目标或自己，恢复生命值
    /// 设计：绿色十字+治疗光环
    /// </remarks>
    public static readonly string HealingSpell = "GameEntry/ArtAsset/Icons/Skills/HealingSpell.svg";
    
    /// <summary>⚡ 掌心雷图标</summary>
    /// <remarks>
    /// 技能：赵灵儿的攻击法术
    /// 效果：召唤雷电攻击敌人
    /// 设计：黄色闪电+电弧效果
    /// </remarks>
    public static readonly string LightningSpell = "GameEntry/ArtAsset/Icons/Skills/LightningSpell.svg";
    
    /// <summary>🌊 流水剑图标</summary>
    /// <remarks>
    /// 技能：赵灵儿的控制法术
    /// 效果：操控水流攻击敌人
    /// 设计：水流形状+波纹效果
    /// </remarks>
    public static readonly string WaterSpell = "GameEntry/ArtAsset/Icons/Skills/WaterSpell.svg";
    
    #endregion
    
    #region 🌪️ 身法类技能图标
    
    /// <summary>🌀 仙风云体术图标</summary>
    /// <remarks>
    /// 技能：李逍遥的身法技能
    /// 效果：提升移动速度，如御风而行
    /// 设计：人形轮廓+仙风环绕
    /// 格式：PNG (64x64)
    /// </remarks>
    public static readonly string XianFengSpell = "GameEntry/ArtAsset/Icons/Skills/XianFengSpell.png";
    
    #endregion
    
    #region 💀 敌人技能图标
    
    /// <summary>🐍 毒牙图标</summary>
    /// <remarks>
    /// 技能：蛇妖的毒攻击
    /// 效果：释放毒素攻击敌人
    /// 设计：蛇牙+毒液滴落+毒气
    /// </remarks>
    public static readonly string PoisonAttack = "GameEntry/ArtAsset/Icons/Skills/PoisonAttack.svg";
    
    #endregion
    
    #region 📊 图标使用统计
    
    /// <summary>
    /// 🎯 技能图标使用指南
    /// </summary>
    public static class UsageGuide
    {
        /// <summary>主角技能图标 - 高优先级显示</summary>
        public static readonly string[] HeroSkills = [
            SwordSlash,      // 李逍遥 - 剑气斩
            XianFengSpell,   // 李逍遥 - 仙风云体术
            HealingSpell,    // 赵灵儿 - 治疗术
            LightningSpell,  // 赵灵儿 - 掌心雷
            WaterSpell,      // 赵灵儿 - 流水剑
            SwordCombo,      // 林月如 - 百花错拳
            QuickStrike,     // 林月如 - 疾风剑
        ];
        
        /// <summary>敌人技能图标 - 用于AI显示</summary>
        public static readonly string[] EnemySkills = [
            PoisonAttack,    // 蛇妖 - 毒牙
        ];
        
        /// <summary>攻击类技能图标</summary>
        public static readonly string[] AttackSkills = [
            SwordSlash,      // 物理攻击
            LightningSpell,  // 法术攻击
            WaterSpell,      // 控制攻击
            SwordCombo,      // 连击攻击
            QuickStrike,     // 突进攻击
            PoisonAttack,    // 毒素攻击
        ];
        
        /// <summary>辅助类技能图标</summary>
        public static readonly string[] SupportSkills = [
            HealingSpell,    // 治疗回复
            XianFengSpell,   // 增益状态
        ];
    }
    
    #endregion
    
    #region 🎨 图标主题配色
    
    /// <summary>
    /// 🌈 技能图标配色方案参考
    /// </summary>
    public static class ColorThemes
    {
        /// <summary>剑术类 - 冷色调（蓝、银）</summary>
        public const string SwordSkills = "蓝色系 (#1a1a2e, #87ceeb, #4169e1)";
        
        /// <summary>法术类 - 多彩（绿、黄、蓝）</summary>
        public const string MagicSkills = "多色系 (治疗绿 #90ee90, 雷电黄 #ffff00, 水蓝 #00bfff)";
        
        /// <summary>身法类 - 紫色调</summary>
        public const string BodySkills = "紫色系 (#2e1065, #9c27b0, #e1bee7)";
        
        /// <summary>毒系类 - 绿色调</summary>
        public const string PoisonSkills = "毒绿系 (#1a4a1a, #32cd32, #9acd32)";
    }
    
    #endregion
}
