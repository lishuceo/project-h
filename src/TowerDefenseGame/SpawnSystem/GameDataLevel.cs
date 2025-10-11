using GameCore;
using GameData;
using System.ComponentModel;

namespace GameEntry.TowerDefenseGame.SpawnSystem;

/// <summary>
/// 塔防游戏关卡数据类别
/// </summary>
[GameDataCategory]
public abstract partial class GameDataLevel
{
    /// <summary>
    /// 关卡名称
    /// </summary>
    [DisplayName("关卡名称")]
    [Description("关卡的显示名称")]
    public string? LevelName { get; set; }

    /// <summary>
    /// 关卡包含的波次列表
    /// </summary>
    [DisplayName("波次列表")]
    [Description("关卡包含的所有波次配置，按顺序执行")]
    public List<GameLink<GameDataWave, GameDataWaveBasic>> Waves { get; set; } = new();

    /// <summary>
    /// 关卡描述
    /// </summary>
    [DisplayName("关卡描述")]
    [Description("关卡的详细描述")]
    public string? Description { get; set; }

    /// <summary>
    /// 难度级别
    /// </summary>
    [DisplayName("难度级别")]
    [Description("关卡的难度级别（1-10）")]
    public int DifficultyLevel { get; set; } = 1;

    /// <summary>
    /// 是否启用
    /// </summary>
    [DisplayName("是否启用")]
    [Description("该关卡是否启用")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 玩家初始血量
    /// </summary>
    [DisplayName("玩家初始血量")]
    [Description("玩家在这个关卡开始时的初始血量")]
    public int InitialPlayerHealth { get; set; } = 20;

    /// <summary>
    /// 玩家初始金币
    /// </summary>
    [DisplayName("玩家初始金币")]
    [Description("玩家在这个关卡开始时的初始金币")]
    public int InitialPlayerGold { get; set; } = 10;
}

/// <summary>
/// 基础关卡节点类型
/// </summary>
[GameDataNodeType<GameDataLevel, GameDataLevel>]
public partial class GameDataLevelBasic
{
    // 继承自GameDataLevel的所有属性
}
