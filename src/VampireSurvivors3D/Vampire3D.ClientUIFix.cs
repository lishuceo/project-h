#if CLIENT
using GameCore.VitalSystem;
using GameCore.Components;

namespace GameEntry.VampireSurvivors3D;

/// <summary>
/// 客户端UI修复 - 提供正确的UI更新逻辑
/// </summary>
public static class ClientUIManager
{
    private static GameCore.Timers.Timer? uiUpdateTimer;

    /// <summary>
    /// 设置客户端UI更新系统
    /// 请在 Vampire3D.Client.cs 的 OnMainUnitChangedAsync 方法中调用此方法
    /// </summary>
    public static void SetupClientUIUpdates(Unit hero)
    {
        Game.Logger.LogInformation("🎮 Setting up client UI updates...");

        // 创建UI更新定时器
        uiUpdateTimer = new GameCore.Timers.Timer(TimeSpan.FromMilliseconds(100))
        {
            AutoReset = true
        };

        uiUpdateTimer.Elapsed += (_, __) => UpdateClientUI(hero);
        uiUpdateTimer.Start();

        Game.Logger.LogInformation("📊 Client UI update timer started");
    }

    private static void UpdateClientUI(Unit hero)
    {
        try
        {
            // 获取客户端状态
            var clientState = NetworkClientSync.GetCurrentState();
            
            // 获取英雄生命值
            var vital = hero.GetTagComponent<Vital>(PropertyVital.Health);
            int currentHealth = (int)(vital?.Current ?? 0);
            int maxHealth = (int)(vital?.Max ?? 100);

            // 更新增强UI
            EnhancedUI.UpdateGameStats(
                health: currentHealth,
                maxHealth: maxHealth,
                level: clientState.Level,
                experience: clientState.Experience,
                maxExp: clientState.ExperienceRequired,
                kills: clientState.EnemiesKilled,
                gameTime: clientState.GameTime
            );
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error updating client UI");
        }
    }

    /// <summary>
    /// 停止UI更新定时器
    /// </summary>
    public static void StopUIUpdates()
    {
        uiUpdateTimer?.Stop();
        uiUpdateTimer?.Dispose();
        uiUpdateTimer = null;
        Game.Logger.LogInformation("📊 Client UI updates stopped");
    }
}
#endif

/*
🔧 集成指南：

请在 Vampire3D.Client.cs 文件中进行以下修改：

1. 在 OnMainUnitChangedAsync 方法中，将原有的 SetupUIUpdates(mainUnit) 调用替换为：
   ClientUIManager.SetupClientUIUpdates(mainUnit);

2. 具体修改位置：
   private static async Task<bool> OnMainUnitChangedAsync(object sender, EventPlayerMainUnitChanged eventArgs)
   {
       // ... 现有代码 ...
       
       // 🔄 替换这一行
       // SetupUIUpdates(mainUnit);
       
       // 🆕 使用新的UI管理器
       ClientUIManager.SetupClientUIUpdates(mainUnit);
       
       // ... 现有代码 ...
   }

3. 确保引入命名空间：
   在文件顶部添加：using GameEntry.VampireSurvivors3D;

这样修改后，客户端UI将能够：
✅ 实时显示血量变化
✅ 显示游戏时间倒计时
✅ 显示击杀数量
✅ 显示等级和经验进度
✅ 响应升级事件
*/ 