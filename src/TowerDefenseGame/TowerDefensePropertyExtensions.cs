using GameCore;

namespace GameEntry.TowerDefenseGame;

/// <summary>
/// 塔防游戏的 PropertySubType 扩展
/// 用于支持复杂数值公式属性的子类型
/// </summary>
[EnumExtension(Extends = typeof(PropertySubType))]
internal enum EPropertySubTypeTowerDefense
{
    /// <summary>
    /// 乘数，用于百分比加成计算
    /// </summary>
    Multiplier = 1000,
}
