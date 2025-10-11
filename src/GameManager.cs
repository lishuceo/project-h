using GameCore;
using System.Reflection;

namespace GameEntry;

/// <summary>
/// 游戏管理器 - 负责自动发现和注册所有游戏类
/// </summary>
public static class GameManager
{
    /// <summary>
    /// 自动注册所有实现了IGameClass接口的游戏类
    /// </summary>
    public static void RegisterAllGames()
    {
        Game.Logger.LogInformation("🎮 开始自动注册游戏类...");
        
        try
        {
            // 获取当前程序集
            var assembly = Assembly.GetExecutingAssembly();
            
            // 查找所有实现了IGameClass接口的类型
            var gameClassTypes = assembly.GetTypes()
                .Where(type => typeof(IGameClass).IsAssignableFrom(type) && 
                              !type.IsInterface && 
                              !type.IsAbstract)
                .ToList();
            
            Game.Logger.LogInformation($"🔍 发现了 {gameClassTypes.Count} 个游戏类");
            
            foreach (var gameType in gameClassTypes)
            {
                try
                {
                    // 寻找OnRegisterGameClass静态方法
                    var registerMethod = gameType.GetMethod("OnRegisterGameClass", 
                        BindingFlags.Static | BindingFlags.Public);
                    
                    if (registerMethod != null)
                    {
                        Game.Logger.LogInformation($"📝 注册游戏类: {gameType.Name}");
                        registerMethod.Invoke(null, null);
                    }
                    else
                    {
                        Game.Logger.LogWarning($"⚠️ 游戏类 {gameType.Name} 没有找到 OnRegisterGameClass 静态方法");
                    }
                }
                catch (Exception ex)
                {
                    Game.Logger.LogError(ex, $"❌ 注册游戏类 {gameType.Name} 时出错");
                }
            }
            
            Game.Logger.LogInformation("✅ 游戏类注册完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "❌ 自动注册游戏类时发生错误");
        }
    }
}
