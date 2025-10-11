#if SERVER
using Events;
using GameCore;
using GameCore.Components;
using GameCore.Struct;
using GameCore.Data;
using GameData;
using static GameCore.ScopeData;

namespace GameEntry.TowerDefenseGame;

/// <summary>
/// 塔防游戏属性公式管理器
/// 负责注册和管理复杂数值公式属性
/// </summary>
public class TowerDefensePropertyFormulaManager : IGameClass
{
    private static bool _formulasRegistered = false;

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        // 支持多种游戏模式：塔防模式和GameUITest模式
        if (Game.GameModeLink != ScopeData.GameMode.TowerDefense && 
            Game.GameModeLink != ScopeData.GameMode.GameUITest)
        {
            return;
        }

        // 避免重复注册
        if (_formulasRegistered)
        {
            return;
        }

        RegisterPropertyFormulas();
        _formulasRegistered = true;
    }

    /// <summary>
    /// 注册所有属性公式
    /// </summary>
    public static void RegisterPropertyFormulas()
    {
        RegisterMoveSpeedFormula();
        
        Game.Logger.LogInformation("📊 Tower Defense Property Formulas registered for game mode");
    }

    /// <summary>
    /// 注册移动速度公式：基础值 * 乘数
    /// </summary>
    private static void RegisterMoveSpeedFormula()
    {
        var moveSpeedFormula = new UnitPropertyFormula
        {
            Formula = (propertyComplex) =>
            {
                var baseValue = propertyComplex.GetFixed(UnitProperty.MoveSpeed, PropertySubType.Base);
                var multiplier = propertyComplex.GetFixed(UnitProperty.MoveSpeed, PropertySubTypeTowerDefense.Multiplier);
                
                // 将百分比值转换为小数（例如：50 -> 0.5）
                var multiplierDecimal = multiplier / 100.0;
                return baseValue * (1 + multiplierDecimal);
            },
            DependedProperties = new HashSet<IGameLink<GameDataUnitProperty>>
            {
                UnitProperty.MoveSpeed // 依赖自身的子属性
            }
        };

        UnitPropertyComplex.RegisterFormula(UnitProperty.MoveSpeed, moveSpeedFormula);
        
        Game.Logger.LogInformation("✅ MoveSpeed formula registered: Base * (Multiplier/100)");
    }

    /// <summary>
    /// 手动注册公式（用于测试或特殊情况）
    /// </summary>
    public static void ForceRegisterFormulas()
    {
        if (!_formulasRegistered)
        {
            RegisterPropertyFormulas();
            _formulasRegistered = true;
        }
    }

    /// <summary>
    /// 检查公式是否已注册
    /// </summary>
    public static bool AreFormulasRegistered => _formulasRegistered;
}
#endif
