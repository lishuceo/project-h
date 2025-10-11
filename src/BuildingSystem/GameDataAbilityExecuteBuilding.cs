using GameCore.AbilitySystem.Data;
using GameCore.ActorSystem.Data;
using GameCore.EntitySystem.Data;
using GameData;

namespace GameEntry.BuildingSystem;

/// <summary>
/// 建造技能执行器扩展 - 添加建造单位的配置支持
/// 继承自GameDataAbilityExecute，添加Unit字段用于指定要建造的单位
/// 通过类名约定让UI系统识别为建造技能
/// </summary>
[GameDataNodeType<GameDataAbility, GameDataAbilityExecute>]
public partial class GameDataAbilityExecuteBuilding
{
    /// <summary>
    /// 要建造的单位类型
    /// 用于摇杆系统显示预览模型和确定建造的单位
    /// </summary>
    public IGameLink<GameDataUnit>? Unit { get; set; }
    
    /// <summary>
    /// 预览Actor（当建造单位没有模型时使用）
    /// 用于摇杆系统显示建造预览
    /// </summary>
    public IGameLink<GameDataActor>? PreviewActor { get; set; }
    
    /// <summary>
    /// 建造预览模型的偏移量
    /// 用于调整预览模型相对于鼠标位置的显示偏移
    /// </summary>
    public System.Numerics.Vector3 PreviewOffset { get; set; } = System.Numerics.Vector3.Zero;
    
    /// <summary>
    /// 预览模型的透明度 (0.0 - 1.0)
    /// 用于建造预览时的视觉效果
    /// </summary>
    public float PreviewAlpha { get; set; } = 0.7f;
    
    /// <summary>
    /// 建造网格对齐大小
    /// 如果大于0，建造位置将对齐到指定大小的网格
    /// </summary>
    public float GridSnapSize { get; set; } = 0f;
    
    /// <summary>
    /// 是否在无效位置显示红色预览
    /// true: 无效位置显示红色预览
    /// false: 无效位置不显示预览
    /// </summary>
    public bool ShowInvalidPreview { get; set; } = true;
}
