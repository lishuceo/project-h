using GameCore;
using GameCore.ActorSystem.Data;
using GameData;
using System.ComponentModel;

namespace GameEntry.TowerDefenseGame.SpawnSystem;

/// <summary>
/// 塔防游戏刷怪器数据类别
/// </summary>
[GameDataCategory]
public abstract partial class GameDataSpawner
{
    /// <summary>
    /// 怪物类型引用
    /// </summary>
    [DisplayName("怪物类型")]
    [Description("要生成的怪物单位类型")]
    public GameLink<GameDataUnit, GameDataUnit>? Monster { get; set; }

    /// <summary>
    /// 生成次数
    /// </summary>
    [DisplayName("生成次数")]
    [Description("这个刷怪器总共生成怪物的次数")]
    public int Times { get; set; } = 1;

    /// <summary>
    /// 每次生成数量
    /// </summary>
    [DisplayName("每次生成数量")]
    [Description("每次生成的怪物数量")]
    public int Number { get; set; } = 1;

    /// <summary>
    /// 生成间隔（秒）
    /// </summary>
    [DisplayName("生成间隔")]
    [Description("每次生成之间的间隔时间，单位：秒")]
    public float Pulse { get; set; } = 1.0f;

    /// <summary>
    /// 初始延迟（秒）
    /// </summary>
    [DisplayName("初始延迟")]
    [Description("刷怪器启动后的延迟时间，单位：秒")]
    public float Delay { get; set; } = 0.0f;

    /// <summary>
    /// 怪物行动路线
    /// </summary>
    [DisplayName("怪物行动路线")]
    [Description("生成的怪物将遵循的路径")]
    public string? LineEx { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [DisplayName("是否启用")]
    [Description("该刷怪器是否启用")]
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// 基础刷怪器节点类型
/// </summary>
[GameDataNodeType<GameDataSpawner, GameDataSpawner>]
public partial class GameDataSpawnerBasic
{
    // 继承自GameDataSpawner的所有属性
}
