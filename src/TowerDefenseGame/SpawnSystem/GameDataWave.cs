using GameCore;
using GameData;
using System.ComponentModel;

namespace GameEntry.TowerDefenseGame.SpawnSystem;

/// <summary>
/// 塔防游戏波次数据类别
/// </summary>
[GameDataCategory]
public abstract partial class GameDataWave
{
    /// <summary>
    /// 波次延迟（秒）
    /// </summary>
    [DisplayName("波次延迟")]
    [Description("这个波次开始前的延迟时间，单位：秒")]
    public float WaveDelay { get; set; } = 0.0f;

    /// <summary>
    /// 波次包含的刷怪器列表
    /// </summary>
    [DisplayName("刷怪器列表")]
    [Description("这个波次包含的所有刷怪器配置")]
    public List<GameLink<GameDataSpawner, GameDataSpawnerBasic>> WaveData { get; set; } = new();

    /// <summary>
    /// 波次名称
    /// </summary>
    [DisplayName("波次名称")]
    [Description("波次的显示名称")]
    public string? WaveName { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [DisplayName("是否启用")]
    [Description("该波次是否启用")]
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// 基础波次节点类型
/// </summary>
[GameDataNodeType<GameDataWave, GameDataWave>]
public partial class GameDataWaveBasic
{
    // 继承自GameDataWave的所有属性
}
