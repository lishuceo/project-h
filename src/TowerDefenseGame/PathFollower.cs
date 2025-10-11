using System.Numerics;
using GameCore;
using GameCore.SceneSystem;
using GameCore.EntitySystem;

namespace GameEntry.TowerDefenseGame;

/// <summary>
/// 🚶‍♂️ 路径跟随组件 - 让单位沿着预定义路径移动 喵～
/// </summary>
public class PathFollower
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
    /// 🎪 路径跟随事件
    /// </summary>
    public event Action<PathFollower, int>? OnPointReached;
    public event Action<PathFollower>? OnPathCompleted;

    public PathFollower(Unit unit, PathSystem.GamePath path, float arrivalThreshold = 50f)
    {
        _unit = unit ?? throw new ArgumentNullException(nameof(unit));
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _arrivalThreshold = arrivalThreshold;
        _currentPointIndex = 0;
        _isMoving = false;
        _hasReachedEnd = false;

        Game.Logger.LogInformation("🚶‍♂️ PathFollower created for {unit} on path {path}", 
            _unit.Cache?.Name ?? "Unknown", _path.Name);
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

        // 检查是否到达当前目标点
        var currentPosition = _unit.Position;
        var targetPosition = _currentTarget.Value.Vector3;
        var distance = Vector3.Distance(currentPosition.Vector3, targetPosition);

        if (distance <= _arrivalThreshold)
        {
            OnPointReached?.Invoke(this, _currentPointIndex);
            Game.Logger.LogDebug("📍 Reached point {index} at {position}", 
                _currentPointIndex, _path.GetPoint(_currentPointIndex));

            // 移动到下一个点
            var nextIndex = _path.GetNextIndex(_currentPointIndex);
            if (nextIndex == -1)
            {
                // 到达路径终点
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
    /// 🎯 移动到当前路径点
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

        // 使用单位的移动指令系统
        try
        {
            var moveResult = _unit.MoveTo(_currentTarget.Value);
            _isMoving = true;
            
            Game.Logger.LogDebug("🚀 Move command issued: {result}", moveResult);
            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ Failed to move to point {index}", _currentPointIndex);
            return false;
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

        Game.Logger.LogInformation("🏁 {unit} completed path {path}", 
            _unit.Cache?.Name ?? "Unknown", _path.Name);

        OnPathCompleted?.Invoke(this);
    }

    /// <summary>
    /// ⏹️ 停止路径跟随
    /// </summary>
    public void Stop()
    {
        _isMoving = false;
        _currentTarget = null;
        
        // TODO: 停止单位的当前移动指令
        Game.Logger.LogInformation("⏹️ PathFollower stopped for {unit}", _unit.Cache?.Name ?? "Unknown");
    }

    /// <summary>
    /// 🔄 重置路径跟随状态
    /// </summary>
    public void Reset()
    {
        _currentPointIndex = 0;
        _isMoving = false;
        _hasReachedEnd = false;
        _currentTarget = null;

        Game.Logger.LogInformation("🔄 PathFollower reset for {unit}", _unit.Cache?.Name ?? "Unknown");
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

    public override string ToString()
    {
        return $"PathFollower({_unit.Cache?.Name ?? "Unknown"}, {_path.Name}, {_currentPointIndex}/{_path.Count})";
    }
}
