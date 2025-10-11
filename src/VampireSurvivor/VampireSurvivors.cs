#if CLIENT
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using Events;
using GameCore.Event;

using GameUI.Brush;
using GameUI.Control.Primitive;
using GameUI.Control.Struct;
using GameUI.Enum;
using GameUI.Struct;
using GameUI.TriggerEvent;
using System.Drawing;
using System.Numerics;

namespace GameEntry;

public class VampireSurvivors : IGameClass, IThinker
{
    // 触发器字段
    private static Trigger<EventGameStart>? gameStartTrigger;
    private Trigger<EventGameKeyDown>? keyDownTrigger;
    private Trigger<EventGameKeyUp>? keyUpTrigger;
    
    // 游戏常量
    private const float GAME_WIDTH = 1200f;
    private const float GAME_HEIGHT = 800f;
    private const float CAMERA_FOLLOW_SPEED = 5f;
    
    // 游戏组件
    private Canvas? gameCanvas;
    private Panel? gamePanel;
    private Panel? uiPanel;
    
    // UI组件
    private Label? healthLabel;
    private Label? levelLabel;
    private Label? timeLabel;
    private Label? killCountLabel;
    private Panel? experienceBar;
    private Panel? experienceBarFill;
    private Panel? upgradePanel;
    private Button[]? upgradeButtons;
    private Label[]? upgradeLabels;
    
    // 游戏对象
    private MyPlayer? player;
    private readonly List<Enemy> enemies = new();
    private readonly List<Projectile> projectiles = new();
    private readonly List<ExperienceOrb> experienceOrbs = new();
    
    // 游戏状态
    private bool isGameRunning;
    private bool isGamePaused;
    private float gameTime;
    private int killCount;
    private float enemySpawnTimer;
    private float cameraX, cameraY;
    
    // 输入状态
    private bool inputLeft, inputRight, inputUp, inputDown;

    public static void OnRegisterGameClass()
    {

            Game.OnGameTriggerInitialization += RegisterAll;
    }

    public static void RegisterAll()
    {
        // 如果游戏模式不是吸血鬼幸存者2D, 则不注册触发器
        if (Game.GameModeLink != ScopeData.GameMode.VampireSurvivors2D)
        {
            return;
        }
        gameStartTrigger = new Trigger<EventGameStart>(async (s, d) =>
        {
            Game.Logger.LogInformation("Vampire Survivors game started");
            var game = new VampireSurvivors();
            game.Initialize();
            return true;
        }, true);
        gameStartTrigger.Register(Game.Instance);
    }

    private void Initialize()
    {
        Game.Logger.LogInformation("Initializing Vampire Survivors game");

        // 创建主游戏面板
        gamePanel = new Panel()
        {
            Width = GAME_WIDTH,
            Height = GAME_HEIGHT,
            Background = new SolidColorBrush(Color.FromArgb(255, 20, 30, 20)) // 深绿色背景
        };

        // 创建Canvas用于绘制游戏世界
        gameCanvas = new Canvas()
        {
            Width = GAME_WIDTH,
            Height = GAME_HEIGHT,
            Parent = gamePanel
        };

        // 创建UI面板
        uiPanel = new Panel()
        {
            Width = GAME_WIDTH,
            Height = GAME_HEIGHT,
            Parent = gamePanel
        };

        InitializeUI();
        SetupInputHandlers();

        // 创建玩家
        player = new MyPlayer(0, 0);
        player.SetGameInstance(this);

        // 将游戏面板添加到视觉树
        _ = gamePanel.AddToVisualTree();

        // 开始游戏
        StartGame();
    }

    private void InitializeUI()
    {
        // 血量显示
        healthLabel = new Label()
        {
            Text = "Health: 100/100",
            FontSize = 18,
            TextColor = Color.Red,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(10, 10, 0, 0),
            Parent = uiPanel
        };

        // 等级显示
        levelLabel = new Label()
        {
            Text = "Level: 1",
            FontSize = 18,
            TextColor = Color.Yellow,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(10, 35, 0, 0),
            Parent = uiPanel
        };

        // 时间显示
        timeLabel = new Label()
        {
            Text = "Time: 0:00",
            FontSize = 18,
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 10, 10, 0),
            Parent = uiPanel
        };

        // 击杀数显示
        killCountLabel = new Label()
        {
            Text = "Kills: 0",
            FontSize = 18,
            TextColor = Color.Orange,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 35, 10, 0),
            Parent = uiPanel
        };

        // 经验条背景
        experienceBar = new Panel()
        {
            Width = GAME_WIDTH - 20,
            Height = 20,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 30),
            Background = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50)),
            Parent = uiPanel
        };

        // 经验条填充
        experienceBarFill = new Panel()
        {
            Width = 0,
            Height = 20,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Background = new SolidColorBrush(Color.FromArgb(255, 0, 150, 255)),
            Parent = experienceBar
        };

        // 升级面板（初始隐藏）
        CreateUpgradePanel();
    }

    private void CreateUpgradePanel()
    {
        upgradePanel = new Panel()
        {
            Width = 600,
            Height = 400,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.FromArgb(200, 30, 30, 30)),
            Visible = false,
            Parent = uiPanel
        };

        var titleLabel = new Label()
        {
            Text = "LEVEL UP! Choose an upgrade:",
            FontSize = 24,
            TextColor = Color.Yellow,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 20, 0, 0),
            Parent = upgradePanel
        };

        // 创建三个升级选项按钮
        upgradeButtons = new Button[3];
        upgradeLabels = new Label[3];

        for (int i = 0; i < 3; i++)
        {
            upgradeButtons[i] = new Button()
            {
                Width = 500,
                Height = 80,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 80 + i * 90, 0, 0),
                Background = new SolidColorBrush(Color.FromArgb(255, 60, 60, 60)),
                Parent = upgradePanel
            };

            upgradeLabels[i] = new Label()
            {
                Text = $"Upgrade Option {i + 1}",
                FontSize = 16,
                TextColor = Color.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Parent = upgradeButtons[i]
            };

            int buttonIndex = i;
            upgradeButtons[i].OnPointerClicked += (s, e) => SelectUpgrade(buttonIndex);
        }
    }

    private void SetupInputHandlers()
    {
        // 键盘输入
        keyDownTrigger = new Trigger<EventGameKeyDown>(async (s, d) =>
        {
            switch (d.Key)
            {
                case GameCore.Platform.SDL.VirtualKey.A:
                case GameCore.Platform.SDL.VirtualKey.Left:
                    inputLeft = true;
                    break;
                case GameCore.Platform.SDL.VirtualKey.D:
                case GameCore.Platform.SDL.VirtualKey.Right:
                    inputRight = true;
                    break;
                case GameCore.Platform.SDL.VirtualKey.W:
                case GameCore.Platform.SDL.VirtualKey.Up:
                    inputUp = true;
                    break;
                case GameCore.Platform.SDL.VirtualKey.S:
                case GameCore.Platform.SDL.VirtualKey.Down:
                    inputDown = true;
                    break;
                case GameCore.Platform.SDL.VirtualKey.Escape:
                    TogglePause();
                    break;
            }
            return true;
        }, true);
        keyDownTrigger.Register(Game.Instance);

        keyUpTrigger = new Trigger<EventGameKeyUp>(async (s, d) =>
        {
            switch (d.Key)
            {
                case GameCore.Platform.SDL.VirtualKey.A:
                case GameCore.Platform.SDL.VirtualKey.Left:
                    inputLeft = false;
                    break;
                case GameCore.Platform.SDL.VirtualKey.D:
                case GameCore.Platform.SDL.VirtualKey.Right:
                    inputRight = false;
                    break;
                case GameCore.Platform.SDL.VirtualKey.W:
                case GameCore.Platform.SDL.VirtualKey.Up:
                    inputUp = false;
                    break;
                case GameCore.Platform.SDL.VirtualKey.S:
                case GameCore.Platform.SDL.VirtualKey.Down:
                    inputDown = false;
                    break;
            }
            return true;
        }, true);
        keyUpTrigger.Register(Game.Instance);
    }

    private void StartGame()
    {
        Game.Logger.LogInformation("Starting Vampire Survivors game");

        // 重置游戏状态
        isGameRunning = true;
        isGamePaused = false;
        gameTime = 0;
        killCount = 0;
        enemySpawnTimer = 0;

        enemies.Clear();
        projectiles.Clear();
        experienceOrbs.Clear();

        player?.Reset();
        UpdateUI();

        (this as IThinker).DoesThink = true;
    }

    public void Think(int deltaInMs)
    {
        if (!isGameRunning || isGamePaused) return;

        float deltaTime = deltaInMs / 1000f;
        GameLoop(deltaTime);
    }

    private void GameLoop(float deltaTime)
    {
        gameTime += deltaTime;

        // 更新玩家
        UpdatePlayer(deltaTime);

        // 更新敌人
        UpdateEnemies(deltaTime);

        // 更新投射物
        UpdateProjectiles(deltaTime);

        // 更新经验球
        UpdateExperienceOrbs(deltaTime);

        // 生成敌人
        SpawnEnemies(deltaTime);

        // 更新摄像机
        UpdateCamera();

        // 绘制游戏
        DrawGame();

        // 更新UI
        UpdateUI();
    }

    private void UpdatePlayer(float deltaTime)
    {
        if (player == null) return;

        // 处理玩家移动
        float moveX = 0, moveY = 0;
        if (inputLeft) moveX -= 1;
        if (inputRight) moveX += 1;
        if (inputUp) moveY -= 1;
        if (inputDown) moveY += 1;

        player.Move(moveX, moveY, deltaTime);
        player.Update(deltaTime);

        // 检查玩家与经验球的碰撞
        for (int i = experienceOrbs.Count - 1; i >= 0; i--)
        {
            var orb = experienceOrbs[i];
            if (Vector2.Distance(player.Position, orb.Position) < 30f)
            {
                player.AddExperience(orb.ExperienceValue);
                experienceOrbs.RemoveAt(i);
            }
        }
    }

    private void UpdateEnemies(float deltaTime)
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var enemy = enemies[i];
            enemy.Update(deltaTime, player?.Position ?? Vector2.Zero);

            // 检查敌人与玩家的碰撞
            if (player != null && Vector2.Distance(enemy.Position, player.Position) < 25f)
            {
                player.TakeDamage(enemy.Damage * deltaTime);
            }

            // 检查敌人与投射物的碰撞
            for (int j = projectiles.Count - 1; j >= 0; j--)
            {
                var projectile = projectiles[j];
                if (Vector2.Distance(enemy.Position, projectile.Position) < 20f)
                {
                    enemy.TakeDamage(projectile.Damage);
                    projectiles.RemoveAt(j);

                    if (enemy.Health <= 0)
                    {
                        // 敌人死亡，掉落经验球
                        experienceOrbs.Add(new ExperienceOrb(enemy.Position, enemy.ExperienceValue));
                        enemies.RemoveAt(i);
                        killCount++;
                        break;
                    }
                }
            }

            // 移除死亡的敌人
            if (enemy.Health <= 0 && enemies.Contains(enemy))
            {
                experienceOrbs.Add(new ExperienceOrb(enemy.Position, enemy.ExperienceValue));
                enemies.Remove(enemy);
                killCount++;
            }
        }
    }

    private void UpdateProjectiles(float deltaTime)
    {
        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            var projectile = projectiles[i];
            projectile.Update(deltaTime);

            // 移除超出范围的投射物
            if (projectile.LifeTime <= 0)
            {
                projectiles.RemoveAt(i);
            }
        }
    }

    private void UpdateExperienceOrbs(float deltaTime)
    {
        for (int i = experienceOrbs.Count - 1; i >= 0; i--)
        {
            var orb = experienceOrbs[i];
            orb.Update(deltaTime, player?.Position ?? Vector2.Zero);

            // 移除过期的经验球
            if (orb.LifeTime <= 0)
            {
                experienceOrbs.RemoveAt(i);
            }
        }
    }

    private void SpawnEnemies(float deltaTime)
    {
        enemySpawnTimer += deltaTime;
        
        float spawnRate = 1f + (gameTime / 30f); // 随时间增加生成速度
        float spawnInterval = 1f / spawnRate;

        if (enemySpawnTimer >= spawnInterval)
        {
            enemySpawnTimer = 0;
            SpawnRandomEnemy();
        }
    }

    private void SpawnRandomEnemy()
    {
        if (player == null) return;

        // 在玩家周围随机位置生成敌人
        var random = Random.Shared;
        float angle = random.NextSingle() * 2 * MathF.PI;
        float distance = 400f + random.NextSingle() * 200f;

        float x = player.Position.X + MathF.Cos(angle) * distance;
        float y = player.Position.Y + MathF.Sin(angle) * distance;

        enemies.Add(new BasicEnemy(x, y));
    }

    private void UpdateCamera()
    {
        if (player == null) return;

        // 摄像机跟随玩家
        float targetX = player.Position.X - GAME_WIDTH / 2;
        float targetY = player.Position.Y - GAME_HEIGHT / 2;

        cameraX += (targetX - cameraX) * CAMERA_FOLLOW_SPEED * (1f / 60f);
        cameraY += (targetY - cameraY) * CAMERA_FOLLOW_SPEED * (1f / 60f);
    }

    private void DrawGame()
    {
        if (gameCanvas == null) return;

        gameCanvas.ResetState();

        // 绘制背景网格
        DrawBackground();

        // 绘制经验球
        foreach (var orb in experienceOrbs)
        {
            DrawExperienceOrb(orb);
        }

        // 绘制敌人
        foreach (var enemy in enemies)
        {
            DrawEnemy(enemy);
        }

        // 绘制投射物
        foreach (var projectile in projectiles)
        {
            DrawProjectile(projectile);
        }

        // 绘制玩家
        if (player != null)
        {
            DrawPlayer(player);
        }
    }

    private void DrawBackground()
    {
        gameCanvas!.StrokePaint = Color.FromArgb(50, 100, 100, 100);
        gameCanvas.StrokeWidth = 1;

        // 绘制网格
        int gridSize = 50;
        int startX = (int)(cameraX / gridSize) * gridSize;
        int startY = (int)(cameraY / gridSize) * gridSize;

        for (int x = startX; x < cameraX + GAME_WIDTH; x += gridSize)
        {
            gameCanvas.DrawLine(x - cameraX, 0, x - cameraX, GAME_HEIGHT);
        }

        for (int y = startY; y < cameraY + GAME_HEIGHT; y += gridSize)
        {
            gameCanvas.DrawLine(0, y - cameraY, GAME_WIDTH, y - cameraY);
        }
    }

    private void DrawPlayer(MyPlayer player)
    {
        float screenX = player.Position.X - cameraX;
        float screenY = player.Position.Y - cameraY;

        gameCanvas!.FillPaint = Color.Blue;
        gameCanvas.FillCircle(screenX, screenY, 15);

        // 绘制生命值条
        float healthPercentage = player.Health / player.MaxHealth;
        gameCanvas.FillPaint = Color.Red;
        gameCanvas.FillRectangle(screenX - 20, screenY - 25, 40, 5);
        gameCanvas.FillPaint = Color.Green;
        gameCanvas.FillRectangle(screenX - 20, screenY - 25, 40 * healthPercentage, 5);
    }

    private void DrawEnemy(Enemy enemy)
    {
        float screenX = enemy.Position.X - cameraX;
        float screenY = enemy.Position.Y - cameraY;

        // 只绘制屏幕内的敌人
        if (screenX < -50 || screenX > GAME_WIDTH + 50 || screenY < -50 || screenY > GAME_HEIGHT + 50)
            return;

        gameCanvas!.FillPaint = Color.Red;
        gameCanvas.FillCircle(screenX, screenY, 12);

        // 绘制生命值条
        if (enemy.Health < enemy.MaxHealth)
        {
            float healthPercentage = enemy.Health / enemy.MaxHealth;
            gameCanvas.FillPaint = Color.DarkRed;
            gameCanvas.FillRectangle(screenX - 15, screenY - 20, 30, 3);
            gameCanvas.FillPaint = Color.LightGreen;
            gameCanvas.FillRectangle(screenX - 15, screenY - 20, 30 * healthPercentage, 3);
        }
    }

    private void DrawProjectile(Projectile projectile)
    {
        float screenX = projectile.Position.X - cameraX;
        float screenY = projectile.Position.Y - cameraY;

        gameCanvas!.FillPaint = Color.Yellow;
        gameCanvas.FillCircle(screenX, screenY, 3);
    }

    private void DrawExperienceOrb(ExperienceOrb orb)
    {
        float screenX = orb.Position.X - cameraX;
        float screenY = orb.Position.Y - cameraY;

        gameCanvas!.FillPaint = Color.Cyan;
        gameCanvas.FillCircle(screenX, screenY, 5);
    }

    private void UpdateUI()
    {
        if (player != null)
        {
            healthLabel!.Text = $"Health: {(int)player.Health}/{(int)player.MaxHealth}";
            levelLabel!.Text = $"Level: {player.Level}";

            // 更新经验条
            float expPercentage = player.Experience / player.ExperienceToNextLevel;
            experienceBarFill!.Width = (GAME_WIDTH - 20) * expPercentage;
        }

        timeLabel!.Text = $"Time: {(int)(gameTime / 60)}:{(int)(gameTime % 60):D2}";
        killCountLabel!.Text = $"Kills: {killCount}";

        // 检查是否需要显示升级面板
        if (player?.ShouldLevelUp == true)
        {
            ShowUpgradePanel();
        }
    }

    private void ShowUpgradePanel()
    {
        isGamePaused = true;
        upgradePanel!.Visible = true;

        // 生成随机升级选项
        var upgrades = GenerateUpgradeOptions();
        for (int i = 0; i < upgradeLabels!.Length && i < upgrades.Length; i++)
        {
            upgradeLabels[i].Text = upgrades[i];
        }
    }

    private string[] GenerateUpgradeOptions()
    {
        var options = new[]
        {
            "增加生命值 (+20 HP)",
            "增加移动速度 (+10%)",
            "增加攻击力 (+15%)",
            "增加攻击速度 (+20%)",
            "增加经验获取 (+25%)",
            "恢复生命值 (Full HP)"
        };

        // 随机选择3个选项
        var selected = new string[3];
        var random = Random.Shared;
        var usedIndices = new HashSet<int>();

        for (int i = 0; i < 3; i++)
        {
            int index;
            do
            {
                index = random.Next(options.Length);
            } while (usedIndices.Contains(index));

            usedIndices.Add(index);
            selected[i] = options[index];
        }

        return selected;
    }

    private void SelectUpgrade(int upgradeIndex)
    {
        if (player == null || upgradeLabels == null || upgradeIndex >= upgradeLabels.Length) return;

        string selectedUpgrade = upgradeLabels[upgradeIndex]?.Text ?? "";

        // 应用升级效果
        if (selectedUpgrade.Contains("生命值 (+20 HP)"))
        {
            player.MaxHealth += 20;
            player.Health = Math.Min(player.Health + 20, player.MaxHealth);
        }
        else if (selectedUpgrade.Contains("移动速度"))
        {
            player.MoveSpeed *= 1.1f;
        }
        else if (selectedUpgrade.Contains("攻击力"))
        {
            player.AttackDamage *= 1.15f;
        }
        else if (selectedUpgrade.Contains("攻击速度"))
        {
            player.AttackSpeed *= 1.2f;
        }
        else if (selectedUpgrade.Contains("经验获取"))
        {
            player.ExperienceMultiplier *= 1.25f;
        }
        else if (selectedUpgrade.Contains("恢复生命值"))
        {
            player.Health = player.MaxHealth;
        }

        // 完成升级
        player.CompleteUpgrade();
        upgradePanel!.Visible = false;
        isGamePaused = false;
    }

    private void TogglePause()
    {
        isGamePaused = !isGamePaused;
    }

    public void AddProjectile(Projectile projectile)
    {
        projectiles.Add(projectile);
    }
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#else
namespace GameEntry.VampireSurvivor;

public class VampireSurvivors : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.Logger.LogWarning("Vampire Survivors game is only available in CLIENT configuration");
    }

    public static void RegisterAll()
    {
        Game.Logger.LogWarning("Vampire Survivors game is only available in CLIENT configuration");
    }
}
#endif 