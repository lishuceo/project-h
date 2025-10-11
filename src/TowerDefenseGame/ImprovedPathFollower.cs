using System.Numerics;
using GameCore;
using GameCore.SceneSystem;
using GameCore.EntitySystem;
using GameCore.OrderSystem;
using GameCore.Components;

namespace GameEntry.TowerDefenseGame;

/// <summary>
/// 🚀 改进版路径跟随器 - 使用底层Command系统获得更精确的移动控制 喵～
/// </summary>
public class ImprovedPathFollower
{
    private readonly Unit _unit;
    private readonly PathSystem.GamePath _path;
    private int _currentPointIndex;
    private bool _isMoving;
    private ScenePoint? _currentTarget;
    private readonly float _arrivalThreshold;
    private bool _hasReachedEnd;

    /// <summary>
    /// 🎯 当前目标点索引
    /// </summary>
    public int CurrentPointIndex => _currentPointIndex;

    /// <summary>
    /// 🏁 是否到达路径终点
    /// </summary>
    public bool HasReachedEnd => _hasReachedEnd;

    /// <summary>
    /// 🚀 是否正在移动
    /// </summary>
    public bool IsMoving => _isMoving;

    /// <summary>
    /// 📍 当前目标位置
    /// </summary>
    public ScenePoint? CurrentTarget => _currentTarget;

    /// <summary>
    /// 🦸 跟随路径的单位
    /// </summary>
    public Unit Unit => _unit;

    /// <summary>
    /// 🎪 路径跟随事件
    /// </summary>
    public event Action<ImprovedPathFollower, int>? OnPointReached;
    public event Action<ImprovedPathFollower>? OnPathCompleted;

    public ImprovedPathFollower(Unit unit, PathSystem.GamePath path, float arrivalThreshold = 50f)
    {
        _unit = unit ?? throw new ArgumentNullException(nameof(unit));
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _arrivalThreshold = arrivalThreshold;
        _currentPointIndex = 0;
        _isMoving = false;
        _hasReachedEnd = false;

        // Game.Logger.LogInformation("🚀 Improved PathFollower created for {unit} on path {path}", 
        //     _unit.Cache?.Name ?? "Unknown", _path.Name);
    }

    /// <summary>
    /// 🚀 开始沿路径移动
    /// </summary>
    public bool StartFollowing()
    {
        if (_hasReachedEnd)
        {
            Game.Logger.LogWarning("⚠️ Cannot start following: path already completed");
            return false;
        }

        if (_path.Count == 0)
        {
            Game.Logger.LogWarning("⚠️ Cannot start following: path has no points");
            return false;
        }

        _currentPointIndex = 0;
        return MoveToCurrentPoint();
    }

    /// <summary>
    /// 🔄 更新路径跟随状态 - 需要在游戏循环中调用
    /// </summary>
    public void Update()
    {
        if (!_isMoving || _hasReachedEnd || _currentTarget == null)
            return;

        // 🎯 简化方案：主要使用距离检测，避免Order API兼容性问题
        CheckDistanceBasedArrival();
    }

    /// <summary>
    /// 🎯 移动到当前路径点 - 使用底层Command系统
    /// </summary>
    private bool MoveToCurrentPoint()
    {
        if (_currentPointIndex < 0 || _currentPointIndex >= _path.Count)
        {
            Game.Logger.LogError("❌ Invalid point index: {index}", _currentPointIndex);
            return false;
        }

        var pathPoint = _path.GetPoint(_currentPointIndex);
        _currentTarget = pathPoint.ToScenePoint(_unit.Scene);

        // Game.Logger.LogDebug("🎯 Moving to point {index}: {point}", _currentPointIndex, pathPoint);

        try
        {
            // 🆕 使用底层Command系统而不是MoveTo
            var moveCommand = new Command()
            {
                Index = CommandIndex.Move,
                Type = ComponentTag.Walkable,
                Target = _currentTarget.Value,
                Player = _unit.Player, // 🔑 关键：设置正确的Player
                Flag = CommandFlag.IsAI // 🔑 使用AI标志以避免权限问题
            };

            var result = moveCommand.IssueOrder(_unit);
            if (result.IsSuccess)
            {
                _isMoving = true;
                // Game.Logger.LogDebug("Move command issued to point {index}", _currentPointIndex);
                return true;
            }
            else
            {
                Game.Logger.LogError("❌ Failed to issue move command: {error}", result.Error);
                return false;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Exception while issuing move command to point {index}", _currentPointIndex);
            return false;
        }
    }

    /// <summary>
    /// 🎯 备用距离检查机制
    /// </summary>
    private void CheckDistanceBasedArrival()
    {
        if (_currentTarget == null) return;

        var currentPosition = _unit.Position;
        var targetPosition = _currentTarget.Value.Vector3;
        var distance = Vector3.Distance(currentPosition.Vector3, targetPosition);

        if (distance <= _arrivalThreshold)
        {
            OnPointReached?.Invoke(this, _currentPointIndex);

            // 移动到下一个点
            var nextIndex = _path.GetNextIndex(_currentPointIndex);
            if (nextIndex == -1)
            {
                CompletePathing();
            }
            else
            {
                _currentPointIndex = nextIndex;
                MoveToCurrentPoint();
            }
        }
    }

    /// <summary>
    /// 🏁 完成路径跟随
    /// </summary>
    private void CompletePathing()
    {
        _isMoving = false;
        _hasReachedEnd = true;
        _currentTarget = null;

        OnPathCompleted?.Invoke(this);
    }

    /// <summary>
    /// ⏹️ 停止路径跟随
    /// </summary>
    public void Stop()
    {
        _isMoving = false;
        _currentTarget = null;

        // 发送停止移动指令
        try
        {
            var stopCommand = new Command()
            {
                Index = CommandIndex.Stop,
                Type = ComponentTag.Walkable,
                Player = _unit.Player,
                Flag = CommandFlag.IsAI
            };
            stopCommand.IssueOrder(_unit);
        }
        catch (Exception ex)
        {
            Game.Logger.LogWarning(ex, "⚠️ Failed to stop movement for {unit}", _unit.Cache?.Name ?? "Unknown");
        }
    }

    /// <summary>
    /// 🔄 重置路径跟随状态
    /// </summary>
    public void Reset()
    {
        Stop();
        _currentPointIndex = 0;
        _hasReachedEnd = false;
    }

    /// <summary>
    /// 📊 获取路径进度 (0.0 - 1.0)
    /// </summary>
    public float GetProgress()
    {
        if (_path.Count <= 1) return 1.0f;
        if (_hasReachedEnd) return 1.0f;

        return (float)_currentPointIndex / (_path.Count - 1);
    }

    /// <summary>
    /// 📏 获取到当前目标的距离
    /// </summary>
    public float GetDistanceToCurrentTarget()
    {
        if (_currentTarget == null) return 0f;
        
        return Vector3.Distance(_unit.Position.Vector3, _currentTarget.Value.Vector3);
    }

    /// <summary>
    /// 📋 获取当前移动状态信息
    /// </summary>
    public string GetOrderStatus()
    {
        return $"Moving: {_isMoving}, Point: {_currentPointIndex}/{_path.Count}, Target: {_currentTarget?.Vector3}";
    }

    public override string ToString()
    {
        return $"ImprovedPathFollower({_unit.Cache?.Name ?? "Unknown"}, {_path.Name}, {_currentPointIndex}/{_path.Count})";
    }
}
