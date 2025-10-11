using System.Numerics;
using GameCore.SceneSystem;

namespace GameEntry.TowerDefenseGame;

/// <summary>
/// 🗺️ 路径管理系统 - 存储和管理怪物前进路线 喵～
/// </summary>
public static class PathSystem
{
    /// <summary>
    /// 🎯 路径点结构 - 包含位置和场景信息
    /// </summary>
    public struct PathPoint
    {
        public Vector3 Position { get; }
        public string SceneTag { get; }

        public PathPoint(Vector3 position, string sceneTag)
        {
            Position = position;
            SceneTag = sceneTag;
        }

        public PathPoint(float x, float y, float z, string sceneTag)
        {
            Position = new Vector3(x, y, z);
            SceneTag = sceneTag;
        }

        /// <summary>
        /// 转换为ScenePoint对象
        /// </summary>
        public ScenePoint ToScenePoint(Scene scene)
        {
            return new ScenePoint(Position, scene);
        }

        public override string ToString()
        {
            return $"PathPoint({Position.X:F1}, {Position.Y:F1}, {Position.Z:F1}, {SceneTag})";
        }
    }

    /// <summary>
    /// 🛤️ 路径数据结构 - 包含路径点列表和路径信息
    /// </summary>
    public class GamePath
    {
        public string Name { get; }
        public List<PathPoint> Points { get; }
        public bool IsLooped { get; }

        public GamePath(string name, List<PathPoint> points, bool isLooped = false)
        {
            Name = name;
            Points = points ?? throw new ArgumentNullException(nameof(points));
            IsLooped = isLooped;
        }

        /// <summary>
        /// 获取路径点数量
        /// </summary>
        public int Count => Points.Count;

        /// <summary>
        /// 获取指定索引的路径点
        /// </summary>
        public PathPoint GetPoint(int index)
        {
            if (index < 0 || index >= Points.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return Points[index];
        }

        /// <summary>
        /// 获取下一个路径点索引
        /// </summary>
        public int GetNextIndex(int currentIndex)
        {
            if (currentIndex < 0 || currentIndex >= Points.Count)
                return -1;

            var nextIndex = currentIndex + 1;
            
            // 如果到达终点
            if (nextIndex >= Points.Count)
            {
                return IsLooped ? 0 : -1; // 循环路径回到起点，否则返回-1表示结束
            }
            
            return nextIndex;
        }

        /// <summary>
        /// 计算路径总长度
        /// </summary>
        public float GetTotalLength()
        {
            if (Points.Count < 2) return 0f;

            float totalLength = 0f;
            for (int i = 0; i < Points.Count - 1; i++)
            {
                totalLength += Vector3.Distance(Points[i].Position, Points[i + 1].Position);
            }

            // 如果是循环路径，加上最后一点到第一点的距离
            if (IsLooped && Points.Count > 2)
            {
                totalLength += Vector3.Distance(Points[^1].Position, Points[0].Position);
            }

            return totalLength;
        }

        public override string ToString()
        {
            return $"GamePath({Name}, {Points.Count} points, {(IsLooped ? "looped" : "linear")})";
        }
    }

    /// <summary>
    /// 🎮 预定义的游戏路径 - 根据主人提供的Lua路线配置
    /// </summary>
    private static readonly Dictionary<string, GamePath> _paths = new()
    {
        {
            "玩家1前进路线", new GamePath("玩家1前进路线", new List<PathPoint>
            {
                // 根据主人最新提供的Lua配置转换的路径点 喵～
                new(2047.9f, 1791.0f, 0f, "new_scene_2"),
                new(2047.9f, 4863.0f, 0f, "new_scene_2"),
                new(4735.9f, 4863.0f, 0f, "new_scene_2"),
                new(4735.9f, 4095.0f, 0f, "new_scene_2"),
                new(2815.9f, 4095.0f, 0f, "new_scene_2"),
                new(2815.9f, 3327.0f, 0f, "new_scene_2"),
                new(4735.9f, 3327.0f, 0f, "new_scene_2"),
                new(4735.9f, 2559.0f, 0f, "new_scene_2"),
                new(2815.9f, 2559.0f, 0f, "new_scene_2"),
                new(2816.0f, 1792.0f, 0f, "new_scene_2"),
                new(5120.0f, 1792.0f, 0f, "new_scene_2"),
                new(5632.0f, 1792.0f, 0f, "new_scene_2"),
                new(5631.9f, 2943.0f, 0f, "new_scene_2"),
                // 注意：移除了重复的终点，路径在最后一个有效点结束
            })
        }
    };

    /// <summary>
    /// 📍 获取指定名称的路径
    /// </summary>
    public static GamePath? GetPath(string pathName)
    {
        return _paths.TryGetValue(pathName, out var path) ? path : null;
    }

    /// <summary>
    /// 📝 注册新路径
    /// </summary>
    public static void RegisterPath(string pathName, GamePath path)
    {
        _paths[pathName] = path;
    }

    /// <summary>
    /// 🗂️ 获取所有可用路径名称
    /// </summary>
    public static IEnumerable<string> GetAllPathNames()
    {
        return _paths.Keys;
    }

    /// <summary>
    /// 📊 获取路径统计信息
    /// </summary>
    public static void LogPathInfo(string pathName)
    {
        if (_paths.TryGetValue(pathName, out var path))
        {
            Game.Logger.LogInformation("🗺️ 路径信息: {path}, 总长度: {length:F1}", 
                path, path.GetTotalLength());
            
            for (int i = 0; i < path.Points.Count; i++)
            {
                Game.Logger.LogInformation("  📍 点 {index}: {point}", i, path.Points[i]);
            }
        }
        else
        {
            Game.Logger.LogWarning("⚠️ 路径 '{pathName}' 不存在", pathName);
        }
    }
}
