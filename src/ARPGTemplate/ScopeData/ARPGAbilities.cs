using GameCore.AbilitySystem.Data;
using GameData;
using GameEntry.ARPGTemplate.ScopeData.Abilities.Swordsman;
using GameEntry.ARPGTemplate.ScopeData.Abilities.Werewolf;
using GameEntry.ARPGTemplate.ScopeData.Abilities.Slime;
using GameEntry.ARPGTemplate.ScopeData.Abilities.Spider;
using GameEntry.ARPGTemplate.ScopeData.Abilities.Gunner;
using GameEntry.ARPGTemplate.ScopeData.Abilities.Mage;
using GameEntry.ARPGTemplate.ScopeData.Abilities.Warrior;
using GameEntry.ARPGTemplate.ScopeData.Abilities.Items;

namespace GameEntry.ARPGTemplate.ScopeData;

/// <summary>
/// ARPG技能系统统一注册入口
/// 负责协调各职业技能的注册和对外提供技能引用
/// </summary>
public class ARPGAbilities : IGameClass
{
    #region 剑客技能引用
    /// <summary>
    /// 剑客技能定义
    /// </summary>
    public static class Ability
    {
        // 挥剑 - 近战范围物理伤害
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> SwordSlash = SwordSlashAbility.SwordSlash;
        // 冲刺 - 辅助位移技能
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Dash = DashAbility.Dash;
        // 痛击 - 近战单体减益破防
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> CrushingBlow = CrushingBlowAbility.CrushingBlow;

        // 狼人技能
        // 爪击 - 近战单体物理攻击
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> WerewolfMeleeAttack = Abilities.Werewolf.WerewolfMeleeAttack.MeleeAttack;

        // 史莱姆技能
        // 主动近战攻击 - 主动攻击附近敌人并造成伤害
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> SlimeSearchAttack = Abilities.Slime.SlimeSearchAttack.SearchAttack;

        // 蜘蛛技能
        // 主动远程攻击 - 主动发射投掷物攻击远程敌人
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> SpiderRangedAttack = Abilities.Spider.SpiderRangedAttack.RangedAttack;

        // 枪手技能
        // 射击 - 远程单体物理攻击
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Gunshot = GunshotAbility.Gunshot;
        // 翻滚 - 辅助位移带无敌
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Roll = RollAbility.Roll;
        // 绷带 - 治疗技能
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Bandage = BandageAbility.Bandage;

        // 法师技能
        // 火球术 - 远程单体魔法攻击
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Fireball = FireballAbility.Fireball;
        // 陨石术 - 远程范围魔法攻击
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Meteor = MeteorAbility.Meteor;
        // 超载 - 减CD增益技能
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Overload = OverloadAbility.Overload;

        // 战士技能
        // 刺击 - 矩形近战范围物理攻击
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Thrust = ThrustAbility.Thrust;
        // 突击 - A-B直线位移真实伤害
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> Charge = ChargeAbility.Charge;
        // 越战越勇 - 被动技能，生命越低攻击力越高
        public static readonly GameLink<GameDataAbility, GameDataAbility> BerserkerRage = BerserkerRageAbility.BerserkerRage;

        // 药剂技能
        // 生命恢复 - 瞬间恢复生命值
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> HealthRestore = HealthPotionAbility.HealthRestore;
        // 力量强化 - 临时增加攻击力和移动速度
        public static readonly GameLink<GameDataAbility, GameDataAbilityExecute> StrengthEnhance = StrengthPotionAbility.StrengthEnhance;
    }
    #endregion

    public static void OnRegisterGameClass()
    {
        // 注册各技能子系统
        SwordSlashAbility.OnRegisterGameClass();
        DashAbility.OnRegisterGameClass();
        CrushingBlowAbility.OnRegisterGameClass();
        WerewolfMeleeAttack.OnRegisterGameClass();
        Abilities.Slime.SlimeSearchAttack.OnRegisterGameClass();
        Abilities.Spider.SpiderRangedAttack.OnRegisterGameClass();
        
        // 注册枪手技能
        GunshotAbility.OnRegisterGameClass();
        RollAbility.OnRegisterGameClass();
        BandageAbility.OnRegisterGameClass();
        
        // 注册法师技能
        FireballAbility.OnRegisterGameClass();
        MeteorAbility.OnRegisterGameClass();
        OverloadAbility.OnRegisterGameClass();
        
        // 注册战士技能
        ThrustAbility.OnRegisterGameClass();
        ChargeAbility.OnRegisterGameClass();
        BerserkerRageAbility.OnRegisterGameClass();
        
        // 注册药剂技能
        HealthPotionAbility.OnRegisterGameClass();
        StrengthPotionAbility.OnRegisterGameClass();
        
        Game.Logger.LogInformation("✅ ARPG Abilities System registered successfully!");
        Game.Logger.LogInformation("   - 剑客技能: 挥剑、冲刺、痛击");
        Game.Logger.LogInformation("   - 狼人技能: 爪击");
        Game.Logger.LogInformation("   - 史莱姆技能: 主动粘液攻击");
        Game.Logger.LogInformation("   - 蜘蛛技能: 主动毒液射击");
        Game.Logger.LogInformation("   - 🔫 枪手技能: 射击、翻滚、绷带");
        Game.Logger.LogInformation("   - 🔥 法师技能: 火球术、陨石术、超载");
        Game.Logger.LogInformation("   - ⚔️ 战士技能: 刺击、突击、越战越勇");
        Game.Logger.LogInformation("   - 🧪 药剂技能: 生命恢复、力量强化");
    }
}
