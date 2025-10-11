#if CLIENT
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
using Events;

using GameCore.Event;
using GameCore.ResourceType;

using GameUI.Brush;
using GameUI.Control.Primitive;
using GameUI.Control.Struct;
using GameUI.Enum;
using GameUI.Struct;
using GameUI.TriggerEvent;
using GameUI.Graphics;

using System.Drawing;

namespace GameEntry.FlappyBirdGame;

public class FlappyBird : IGameClass, IThinker
{
    // 游戏常量 - 调整为更大的尺寸和更平衡的物理感受
    private const float GRAVITY = 1800f;  // 减小重力，更优雅的下降
    private const float JUMP_VELOCITY = -600f;  // 增强跳跃，更从容的操作
    private const float PIPE_SPEED = 300f;
    private const float PIPE_GAP = 260f;  // 增大间隙，匹配新的跳跃高度
    private const float PIPE_WIDTH = 100f;
    private const float PIPE_SPAWN_INTERVAL = 2.5f;
    private const float BIRD_SIZE = 60f;
    private const float GROUND_HEIGHT = 120f;

    // 游戏组件
    private Canvas? gameCanvas;
    private Label? scoreLabel;
    private Label? fpsLabel;
    private Label? gameOverLabel;
    private Button? restartButton;
    private Label? restartButtonLabel;
    private Panel? gamePanel;

    // 游戏状态
    private float birdY;
    private float birdVelocity;
    private readonly List<Pipe> pipes = [];
    private int score;
    private bool isGameRunning;
    private bool isGameOver;
    private float pipeSpawnTimer;
    private float gameWidth = 1200f; // 扩大到全视口
    private float gameHeight = 800f;

    // 小鸟动画相关
    private float birdRotation = 0f;
    private float wingAnimationTimer = 0f;
    private bool wingUp = true;
    
    // 美化效果相关
    private float cloudOffset = 0f; // 云朵移动偏移
    private readonly List<Particle> jumpParticles = []; // 跳跃粒子
    private float sunRotation = 0f; // 太阳旋转角度
    
    // FPS计算相关
    private float fpsTimer = 0f; // FPS更新计时器
    private int frameCount = 0; // 帧数计数
    private float currentFps = 0f; // 当前FPS
    
    // 粒子类
    private class Particle
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float Life { get; set; }
        public float MaxLife { get; set; }
        public Color Color { get; set; }
        public float Size { get; set; }
    }

    private class Pipe
    {
        public float X { get; set; }
        public float GapY { get; set; }
        public bool Scored { get; set; }
    }

    public static void OnRegisterGameClass()
    {

        Game.Logger.LogInformation("Flappy Bird game registered");
        Game.OnGameTriggerInitialization += RegisterAll;
    }

    public static void RegisterAll()
    {
        // 如果游戏模式不是Flappy Bird，则不注册触发器
        if (Game.GameModeLink != ScopeData.GameMode.FlappyBird)
        {
            return;
        }
        Trigger<EventGameStart> trigger = new(async (s, d) =>
        {
            Game.Logger.LogInformation("Flappy Bird game started");
            var game = new FlappyBird();
            game.Initialize();
            return true;
        });
        trigger.Register(Game.Instance);
    }

    private void Initialize()
    {
        Game.Logger.LogInformation("Initializing Flappy Bird game");

        // 获取视口尺寸并扩大到全视口
        var viewportSize = GameUI.Device.ScreenViewport.Primary.Size;
        gameWidth = Math.Max(1200f, viewportSize.Width);
        gameHeight = Math.Max(800f, viewportSize.Height);

        // 创建主游戏面板 - 扩大到整个视口
        gamePanel = new Panel()
        {
            Width = gameWidth,
            Height = gameHeight,
            Background = new SolidColorBrush(Color.FromArgb(255, 135, 206, 235)) // 天蓝色背景
        };

        // 创建Canvas用于绘制游戏
        gameCanvas = new Canvas()
        {
            Width = gameWidth,
            Height = gameHeight,
            Parent = gamePanel
        };

        // 创建分数标签 - 调整为更大的字体
        scoreLabel = new Label()
        {
            Text = "Score: 0",
            FontSize = 36,
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 30, 0, 0),
            Parent = gamePanel
        };

        // 创建FPS显示标签
        fpsLabel = new Label()
        {
            Text = "FPS: 0",
            FontSize = 24,
            TextColor = Color.Yellow,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(20, 20, 0, 0),
            Parent = gamePanel
        };

        // 创建游戏结束UI
        gameOverLabel = new Label()
        {
            Text = "Game Over!",
            FontSize = 64,
            TextColor = Color.Red,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visible = false,
            Parent = gamePanel
        };

        // 创建重新开始按钮 - 调整为更大的尺寸
        restartButton = new Button()
        {
            Width = 160,
            Height = 60,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 120, 0, 0),
            Visible = false,
            Parent = gamePanel
        };

        // 为按钮添加文本标签
        restartButtonLabel = new Label()
        {
            Text = "Restart",
            FontSize = 24,
            TextColor = Color.Black,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Parent = restartButton
        };

        // 设置事件处理
        gameCanvas.OnPointerPressed += OnCanvasClicked;
        restartButton.OnPointerPressed += OnRestartClicked;

        // 监听键盘事件
        Trigger<EventGameKeyDown> keyDownTrigger = new(async (s, d) =>
        {
            if (d.Key == GameCore.Platform.SDL.VirtualKey.Space)
            {
                Jump();
            }
            return true;
        });
        keyDownTrigger.Register(Game.Instance);

        // 将游戏面板添加到视觉树
        _ = gamePanel.AddToVisualTree();

        // 开始游戏
        StartGame();
    }

    private void StartGame()
    {
        Game.Logger.LogInformation("Starting Flappy Bird game");

        // 重置游戏状态
        birdY = gameHeight / 2;
        birdVelocity = 0;
        birdRotation = 0f;
        wingAnimationTimer = 0f;
        wingUp = true;
        pipes.Clear();
        score = 0;
        isGameRunning = true;
        isGameOver = false;
        pipeSpawnTimer = 0;
        
        // 重置美化效果
        jumpParticles.Clear();
        cloudOffset = 0f;
        sunRotation = 0f;
        
        // 重置FPS计算
        fpsTimer = 0f;
        frameCount = 0;
        currentFps = 0f;

        // 更新UI
        UpdateScore();
        gameOverLabel!.Visible = false;
        restartButton!.Visible = false;

        // 启用思考器
        (this as IThinker).DoesThink = true;
    }

    public void Think(int deltaInMs)
    {
        // 计算FPS
        UpdateFPS(deltaInMs);
        
        GameLoop(deltaInMs / 1000f); // 将毫秒转换为秒
    }
    
    private void GameLoop(float deltaTimeInSec)
    {
        if (!isGameRunning || isGameOver)
        {
            return;
        }

        // 更新小鸟物理
        birdVelocity += GRAVITY * deltaTimeInSec;
        birdY += birdVelocity * deltaTimeInSec;

        // 更新小鸟旋转角度（添加平滑过渡，避免角度跳跃）
        var targetRotation = Math.Clamp(birdVelocity * 0.08f, -25f, 75f); // 减小系数和范围
        var rotationSpeed = 180f; // 每秒最大旋转速度（度）
        var maxRotationChange = rotationSpeed * deltaTimeInSec;
        
        // 平滑插值到目标角度，避免突然跳跃
        if (Math.Abs(targetRotation - birdRotation) <= maxRotationChange)
        {
            birdRotation = targetRotation;
        }
        else
        {
            birdRotation += Math.Sign(targetRotation - birdRotation) * maxRotationChange;
        }

        // 更新翅膀动画
        wingAnimationTimer += deltaTimeInSec * 8f; // 加快翅膀动画
        if (wingAnimationTimer >= 1f)
        {
            wingAnimationTimer = 0f;
            wingUp = !wingUp;
        }
        
        // 更新美化效果
        UpdateVisualEffects(deltaTimeInSec);

        // 检查边界碰撞
        if (birdY - (BIRD_SIZE / 2) <= 0 || birdY + (BIRD_SIZE / 2) >= gameHeight - GROUND_HEIGHT)
        {
            GameOver();
            return;
        }

        // 更新管道
        UpdatePipes(deltaTimeInSec);

        // 检查管道碰撞
        CheckPipeCollisions();

        // 生成新管道
        pipeSpawnTimer += deltaTimeInSec;
        if (pipeSpawnTimer >= PIPE_SPAWN_INTERVAL)
        {
            pipeSpawnTimer = 0;
            SpawnPipe();
        }

        // 绘制游戏
        DrawGame();
    }
    
    private void UpdateVisualEffects(float deltaTime)
    {
        // 更新云朵移动
        cloudOffset += deltaTime * 15f; // 缓慢移动

        // 更新太阳旋转
        sunRotation += deltaTime * 10f; // 慢慢旋转

        // 更新粒子效果
        for (var i = jumpParticles.Count - 1; i >= 0; i--)
        {
            var particle = jumpParticles[i];
            particle.X += particle.VelocityX * deltaTime;
            particle.Y += particle.VelocityY * deltaTime;
            particle.VelocityY += 300f * deltaTime; // 重力影响
            particle.Life -= deltaTime;

            if (particle.Life <= 0)
            {
                jumpParticles.RemoveAt(i);
            }
        }
    }

    private void CreateJumpParticles()
    {
        // 创建跳跃羽毛粒子效果
        var birdX = gameWidth / 2;
        for (var i = 0; i < 5; i++)
        {
            var particle = new Particle
            {
                X = birdX + (Random.Shared.NextSingle() - 0.5f) * 20f,
                Y = birdY + (Random.Shared.NextSingle() - 0.5f) * 20f,
                VelocityX = (Random.Shared.NextSingle() - 0.5f) * 100f,
                VelocityY = Random.Shared.NextSingle() * -150f - 50f,
                Life = 1.0f + Random.Shared.NextSingle() * 0.5f,
                MaxLife = 1.5f,
                Color = Color.FromArgb(
                    255,
                    (byte)(200 + Random.Shared.Next(55)),
                    (byte)(180 + Random.Shared.Next(75)),
                    (byte)(100 + Random.Shared.Next(100))
                ),
                Size = 3f + Random.Shared.NextSingle() * 4f
            };
            particle.MaxLife = particle.Life;
            jumpParticles.Add(particle);
        }
    }

    private void UpdatePipes(float deltaTime)
    {
        for (var i = pipes.Count - 1; i >= 0; i--)
        {
            var pipe = pipes[i];
            pipe.X -= PIPE_SPEED * deltaTime;

            // 移除屏幕外的管道
            if (pipe.X + PIPE_WIDTH < 0)
            {
                pipes.RemoveAt(i);
                continue;
            }

            // 检查得分
            if (!pipe.Scored && pipe.X + PIPE_WIDTH < gameWidth / 2)
            {
                pipe.Scored = true;
                score++;
                UpdateScore();
            }
        }
    }

    private void CheckPipeCollisions()
    {
        var birdX = gameWidth / 2;

        foreach (var pipe in pipes)
        {
            // 检查X轴重叠
            if (birdX + (BIRD_SIZE / 2) > pipe.X && birdX - (BIRD_SIZE / 2) < pipe.X + PIPE_WIDTH)
            {
                // 检查Y轴碰撞
                if (birdY - (BIRD_SIZE / 2) < pipe.GapY - (PIPE_GAP / 2) ||
                    birdY + (BIRD_SIZE / 2) > pipe.GapY + (PIPE_GAP / 2))
                {
                    GameOver();
                    return;
                }
            }
        }
    }

    private void SpawnPipe()
    {
        var minGapY = (PIPE_GAP / 2) + 80;
        var maxGapY = gameHeight - GROUND_HEIGHT - (PIPE_GAP / 2) - 80;
        var gapY = (Random.Shared.NextSingle() * (maxGapY - minGapY)) + minGapY;

        pipes.Add(new Pipe
        {
            X = gameWidth,
            GapY = gapY,
            Scored = false
        });
    }

    private void DrawGame()
    {
        if (gameCanvas == null)
        {
            return;
        }

        // 清空画布
        gameCanvas.ResetState();

        // 绘制背景元素（从远到近）
        DrawSkyBackground();
        DrawMountains();
        DrawSun();
        DrawClouds();

        // 绘制游戏元素
        DrawPipes();
        DrawGround();
        DrawParticles();
        DrawBird();
    }

    private void DrawSkyBackground()
    {
        if (gameCanvas == null) return;
        
        // 绘制天空渐变（从顶部到地面）
        gameCanvas.FillPaint = new LinearGradientPaint(
            new PointF(0, 0),
            new PointF(0, gameHeight - GROUND_HEIGHT),
            Color.FromArgb(255, 135, 206, 250), // 天蓝色
            Color.FromArgb(255, 255, 218, 185)  // 温暖的米色
        );
        gameCanvas.FillRectangle(0, 0, gameWidth, gameHeight - GROUND_HEIGHT);
    }

    private void DrawMountains()
    {
        if (gameCanvas == null) return;
        
        // 绘制远山轮廓
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(150, 70, 130, 180)); // 半透明钢蓝色
        
        // 远山1
        gameCanvas.FillTriangle(0, gameHeight - GROUND_HEIGHT - 50,
                               300, gameHeight - GROUND_HEIGHT - 150,
                               600, gameHeight - GROUND_HEIGHT - 80);
        
        // 远山2
        gameCanvas.FillTriangle(400, gameHeight - GROUND_HEIGHT - 60,
                               700, gameHeight - GROUND_HEIGHT - 120,
                               1000, gameHeight - GROUND_HEIGHT - 70);
        
        // 远山3
        gameCanvas.FillTriangle(800, gameHeight - GROUND_HEIGHT - 50,
                               1100, gameHeight - GROUND_HEIGHT - 100,
                               1400, gameHeight - GROUND_HEIGHT - 60);
    }

    private void DrawSun()
    {
        if (gameCanvas == null) return;
        
        var sunX = gameWidth - 150f;
        var sunY = 120f;
        var sunRadius = 40f;
        
        gameCanvas.SaveState();
        gameCanvas.Translate(sunX, sunY);
        gameCanvas.RotateDegrees(sunRotation);
        
        // 绘制太阳光芒
        gameCanvas.StrokePaint = new SolidPaint(Color.FromArgb(200, 255, 215, 0)); // 半透明金黄色
        gameCanvas.StrokeWidth = 3f;
        for (var i = 0; i < 8; i++)
        {
            var angle = i * 45f;
            gameCanvas.SaveState();
            gameCanvas.RotateDegrees(angle);
            gameCanvas.DrawLine(0, -sunRadius - 10, 0, -sunRadius - 25);
            gameCanvas.RestoreState();
        }
        
        // 绘制太阳主体
        gameCanvas.FillPaint = new RadialGradientPaint(
            new PointF(0, 0), 0, sunRadius,
            Color.FromArgb(255, 255, 255, 0),   // 亮黄色中心
            Color.FromArgb(255, 255, 140, 0)    // 橙色边缘
        );
        gameCanvas.FillCircle(0, 0, sunRadius);
        
        gameCanvas.RestoreState();
    }

    private void DrawClouds()
    {
        if (gameCanvas == null) return;

        // 绘制移动的装饰云朵，使用椭圆让云朵看起来更自然
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(180, 255, 255, 255)); // 半透明白色
        
        // 云朵1 - 使用椭圆组合（带移动效果）
        var cloud1X = (150 - cloudOffset) % (gameWidth + 200) - 100;
        gameCanvas.FillEllipse(cloud1X, 100, 45, 25);
        gameCanvas.FillEllipse(cloud1X + 30, 85, 35, 30);
        gameCanvas.FillEllipse(cloud1X + 60, 100, 40, 22);
        
        // 云朵2 - 不同形状的椭圆（反向移动）
        var cloud2X = (450 + cloudOffset * 0.7f) % (gameWidth + 200) - 100;
        gameCanvas.FillEllipse(cloud2X, 150, 30, 20);
        gameCanvas.FillEllipse(cloud2X + 20, 135, 40, 28);
        gameCanvas.FillEllipse(cloud2X + 45, 150, 35, 18);
        
        // 云朵3 - 更扁的椭圆（慢速移动）
        var cloud3X = (750 - cloudOffset * 0.5f) % (gameWidth + 200) - 100;
        gameCanvas.FillEllipse(cloud3X, 80, 42, 20);
        gameCanvas.FillEllipse(cloud3X + 25, 70, 38, 25);
        gameCanvas.FillEllipse(cloud3X + 50, 85, 45, 18);
    }

    private void DrawParticles()
    {
        if (gameCanvas == null) return;
        
        // 绘制跳跃粒子效果
        foreach (var particle in jumpParticles)
        {
            var alpha = (byte)(255 * (particle.Life / particle.MaxLife));
            var color = Color.FromArgb(alpha, particle.Color.R, particle.Color.G, particle.Color.B);
            
            gameCanvas.FillPaint = new SolidPaint(color);
            gameCanvas.FillCircle(particle.X, particle.Y, particle.Size * (particle.Life / particle.MaxLife));
        }
    }

    private void DrawPipes()
    {
        if (gameCanvas == null) return;

        foreach (var pipe in pipes)
        {
            // 绘制管道阴影
            gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(60, 0, 0, 0)); // 半透明黑色阴影
            gameCanvas.FillRectangle(pipe.X + 8, 8, PIPE_WIDTH, pipe.GapY - (PIPE_GAP / 2) - 8);
            gameCanvas.FillRectangle(pipe.X + 8, pipe.GapY + (PIPE_GAP / 2) + 8, PIPE_WIDTH,
                gameHeight - GROUND_HEIGHT - (pipe.GapY + (PIPE_GAP / 2)) - 8);

            // 绘制管道主体渐变
            var topHeight = pipe.GapY - (PIPE_GAP / 2);
            var bottomY = pipe.GapY + (PIPE_GAP / 2);
            var bottomHeight = gameHeight - GROUND_HEIGHT - bottomY;

            // 上管道
            gameCanvas.FillPaint = new LinearGradientPaint(
                new PointF(pipe.X, 0),
                new PointF(pipe.X + PIPE_WIDTH, 0),
                Color.FromArgb(255, 34, 139, 34),  // 森林绿
                Color.FromArgb(255, 0, 100, 0)     // 暗绿
            );
            gameCanvas.FillRectangle(pipe.X, 0, PIPE_WIDTH, topHeight);

            // 下管道
            gameCanvas.FillRectangle(pipe.X, bottomY, PIPE_WIDTH, bottomHeight);

            // 绘制管道边缘高光
            gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(255, 50, 205, 50)); // 亮绿色
            gameCanvas.FillRectangle(pipe.X, topHeight - 25, PIPE_WIDTH, 25);
            gameCanvas.FillRectangle(pipe.X, bottomY, PIPE_WIDTH, 25);

            // 绘制管道装饰线条
            gameCanvas.StrokePaint = new SolidPaint(Color.FromArgb(255, 0, 80, 0)); // 深绿色
            gameCanvas.StrokeWidth = 3f;
            gameCanvas.DrawRectangle(pipe.X, 0, PIPE_WIDTH, topHeight);
            gameCanvas.DrawRectangle(pipe.X, bottomY, PIPE_WIDTH, bottomHeight);
        }
    }

    private void DrawGround()
    {
        if (gameCanvas == null) return;

        // 绘制地面渐变
        gameCanvas.FillPaint = new LinearGradientPaint(
            new PointF(0, gameHeight - GROUND_HEIGHT),
            new PointF(0, gameHeight),
            Color.FromArgb(255, 160, 82, 45),  // 鞍褐色
            Color.FromArgb(255, 101, 67, 33)   // 深褐色
        );
        gameCanvas.FillRectangle(0, gameHeight - GROUND_HEIGHT, gameWidth, GROUND_HEIGHT);

        // 绘制地面纹理线条
        gameCanvas.StrokePaint = new SolidPaint(Color.FromArgb(255, 80, 50, 20)); // 深褐色
        gameCanvas.StrokeWidth = 2f;
        for (var i = 0; i < gameWidth; i += 60)
        {
            gameCanvas.DrawLine(i, gameHeight - GROUND_HEIGHT, i, gameHeight);
        }

        // 绘制草地装饰
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(255, 34, 139, 34)); // 森林绿
        for (var i = 0; i < gameWidth; i += 30)
        {
            // 随机草叶高度
            var grassHeight = 8 + (Random.Shared.NextSingle() * 12);
            gameCanvas.FillTriangle(i, gameHeight - GROUND_HEIGHT, 
                                  i + 5, gameHeight - GROUND_HEIGHT - grassHeight, 
                                  i + 10, gameHeight - GROUND_HEIGHT);
        }
    }

    private void DrawBird()
    {
        if (gameCanvas == null) return;

        var birdX = gameWidth / 2;
        
        // 保存状态并应用变换
        gameCanvas.SaveState();
        
        // 移动到小鸟中心并旋转（使用明确的度数API）
        gameCanvas.Translate(birdX, birdY);
        gameCanvas.RotateDegrees(birdRotation);

        // === 绘制顺序：从后到前 ===
        
        // 1. 绘制尾巴（最后面）
        var tailOffset = wingUp ? 1f : -1f; // 轻微摆动
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(255, 255, 140, 0)); // 暗金色
        gameCanvas.FillTriangle(-BIRD_SIZE/2.2f, tailOffset - 6,
                               -BIRD_SIZE/2.2f - 12, tailOffset,
                               -BIRD_SIZE/2.2f, tailOffset + 6);
        
        // 尾巴内层（亮色）
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(255, 255, 190, 0)); // 亮金色
        gameCanvas.FillTriangle(-BIRD_SIZE/2.2f + 1, tailOffset - 4,
                               -BIRD_SIZE/2.2f - 8, tailOffset,
                               -BIRD_SIZE/2.2f + 1, tailOffset + 4);

        // 2. 绘制翅膀（在身体两侧）
        var wingOffset = wingUp ? -3f : 3f;
        
        // 翅膀阴影
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(120, 200, 100, 0)); // 半透明暗色
        gameCanvas.FillEllipse(-BIRD_SIZE/4 + 1, wingOffset + 1, BIRD_SIZE/3, BIRD_SIZE/5);
        
        // 主翅膀
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(255, 255, 165, 0)); // 橙色
        gameCanvas.FillEllipse(-BIRD_SIZE/4, wingOffset, BIRD_SIZE/3, BIRD_SIZE/5);
        
        // 翅膀纹理
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(255, 255, 140, 0)); // 暗橙色
        gameCanvas.FillEllipse(-BIRD_SIZE/4 + 3, wingOffset, BIRD_SIZE/5, BIRD_SIZE/8);

        // 3. 绘制身体主体（中心）
        gameCanvas.FillPaint = new RadialGradientPaint(
            new PointF(-2, 2), 0, BIRD_SIZE / 2.5f,
            Color.FromArgb(255, 255, 220, 0),   // 亮金黄色中心
            Color.FromArgb(255, 255, 160, 0)    // 暗金色边缘
        );
        gameCanvas.FillEllipse(0, 0, BIRD_SIZE / 1.9f, BIRD_SIZE / 2.4f); // 主身体

        // 4. 绘制肚子（身体前下方）
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(255, 255, 248, 220)); // 象牙色
        gameCanvas.FillEllipse(4, 6, BIRD_SIZE / 3.2f, BIRD_SIZE / 5.5f); // 小肚子

        // // 5. 绘制头部区域强化
        // gameCanvas.FillPaint = new RadialGradientPaint(
        //     new PointF(8, -6), 0, 12,
        //     Color.FromArgb(255, 255, 235, 0),   // 更亮的头部
        //     Color.FromArgb(255, 255, 200, 0)    // 渐变到金色
        // );
        // gameCanvas.FillEllipse(8, -6, 14, 12); // 头部区域

        // 6. 绘制眼睛（在头部合适位置）
        // 眼白
        gameCanvas.FillPaint = new SolidPaint(Color.White);
        gameCanvas.FillEllipse(12, -8, 8, 7); // 调整到头部区域
        
        // 瞳孔
        gameCanvas.FillPaint = new SolidPaint(Color.Black);
        gameCanvas.FillEllipse(13, -8, 5, 4); // 椭圆瞳孔
        
        // 眼睛高光
        gameCanvas.FillPaint = new SolidPaint(Color.White);
        gameCanvas.FillEllipse(14, -9, 2f, 1.5f); // 小高光

        // 7. 绘制嘴巴（在头部前端）
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(255, 255, 140, 0)); // 橙色
        gameCanvas.FillTriangle(BIRD_SIZE/2.1f - 2, -3,
                               BIRD_SIZE/2.1f + 6, -1,
                               BIRD_SIZE/2.1f - 2, 1);
        
        // 嘴巴高光
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(255, 255, 180, 0)); // 亮橙色
        gameCanvas.FillTriangle(BIRD_SIZE/2.1f - 1, -2,
                               BIRD_SIZE/2.1f + 3, -1,
                               BIRD_SIZE/2.1f - 1, 0);

        // 8. 最后添加细节装饰
        // 身体上的小装饰斑点
        gameCanvas.FillPaint = new SolidPaint(Color.FromArgb(100, 255, 180, 0)); // 半透明装饰
        gameCanvas.FillCircle(-4, 2, 2);
        gameCanvas.FillCircle(2, -2, 1.5f);

        // 恢复状态
        gameCanvas.RestoreState();
    }

    private void Jump()
    {
        if (isGameRunning && !isGameOver)
        {
            birdVelocity = JUMP_VELOCITY;
            CreateJumpParticles(); // 创建跳跃粒子效果
        }
    }

    private void GameOver()
    {
        Game.Logger.LogInformation("Game Over! Final score: {score}", score);

        isGameOver = true;
        isGameRunning = false;

        gameOverLabel!.Visible = true;
        restartButton!.Visible = true;

        (this as IThinker).DoesThink = false; // 停止思考器
    }

    private void UpdateScore()
    {
        scoreLabel!.Text = $"Score: {score}";
    }

    private void UpdateFPS(int deltaInMs)
    {
        // 累计帧数和时间
        frameCount++;
        fpsTimer += deltaInMs;
        
        // 每0.5秒更新一次FPS显示
        if (fpsTimer >= 500f)
        {
            // 计算FPS: 帧数 / 时间(秒)
            currentFps = frameCount / (fpsTimer / 1000f);
            
            // 更新FPS显示
            fpsLabel!.Text = $"FPS: {currentFps:F1}";
            
            // 重置计数器
            frameCount = 0;
            fpsTimer = 0f;
        }
    }

    private void OnCanvasClicked(object? sender, PointerEventArgs e)
    {
        Jump();
    }

    private void OnRestartClicked(object? sender, PointerEventArgs e)
    {
        StartGame();
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#else
namespace GameEntry.FlappyBirdGame;

public class FlappyBird : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.Logger.LogWarning("Flappy Bird game is only available in CLIENT configuration");
    }

    public static void RegisterAll()
    {
        Game.Logger.LogWarning("Flappy Bird game is only available in CLIENT configuration");
    }
}
#endif 
