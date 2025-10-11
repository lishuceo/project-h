#if CLIENT
using Events;

using GameCore.Event;
using GameCore.ResourceType;
using GameCore.Shape;
using GameCore.Shape.Data;
using GameCore.ActorSystem;
using GameCore.Drawing;

using GameUI.Brush;
using GameUI.Control.Primitive;
using GameUI.Control.Struct;
using GameUI.Enum;
using GameUI.Struct;
using GameUI.TriggerEvent;
using GameUI.Graphics;
using GameUI.Device;

using System.Drawing;
using System.Numerics;

namespace GameEntry.JumpJumpGame;

/// <summary>
/// 跳一跳小游戏 - 经典跳一跳重制版
/// 玩家控制一个角色，通过蓄力跳跃在不同平台间移动，收集分数
/// 特色：蓄力机制、精准跳跃、经典视觉效果、美化版
/// </summary>
public class JumpJump : IGameClass, IThinker
{
    // 游戏常量
    private const float GRAVITY = 1500f;           // 重力加速度
    private const float MIN_JUMP_FORCE = 400f;    // 最小跳跃力度
    private const float MAX_JUMP_FORCE = 1200f;   // 最大跳跃力度
    private const float MAX_CHARGE_TIME = 2f;     // 最大蓄力时间
    private const float PLAYER_SIZE = 40f;        // 玩家大小
    private const float PLATFORM_WIDTH = 80f;     // 平台宽度
    private const float PLATFORM_HEIGHT = 20f;    // 平台高度
    private const float MIN_PLATFORM_DISTANCE = 60f;  // 最小平台间距
    private const float MAX_PLATFORM_DISTANCE = 180f; // 最大平台间距
    private const float WORLD_WIDTH = 2000f;      // 世界宽度
    private const float CAMERA_FOLLOW_SPEED = 3f; // 相机跟随速度
    private const float CAMERA_HEIGHT = 200f;     // 相机高度
    private const float CAMERA_DISTANCE = 300f;   // 相机距离

    // UI 常量
    private const float UI_MARGIN = 20f;
    private const float CHARGE_BAR_WIDTH = 200f;
    private const float CHARGE_BAR_HEIGHT = 20f;

    // 游戏状态
    private float playerX;              // X轴水平位置
    private float playerZ;              // Z轴高度（向上为正）
    private float playerVelocityX;      // X轴速度（水平）
    private float playerVelocityZ;      // Z轴速度（垂直方向）
    private bool isCharging;            // 是否正在蓄力
    private bool isOnGround;            // 是否在地面上
    private float chargeTime;           // 蓄力时间
    private int score;                  // 分数
    private int perfectJumps;           // 连续完美跳跃次数
    private bool isGameRunning;
    private bool isGameStarted;
    private float gameTime;
    private float cameraX;              // 相机X位置
    private float cameraY;              // 相机Y位置（深度）

    // 3D游戏对象
    private readonly List<Platform> platforms = [];
    private readonly List<Collectible> collectibles = [];
    private readonly List<ActorModel> gameActors = [];
    private readonly List<ActorModel> particleEffects = [];
    private ActorModel? playerActor;

    // UI组件
    private Label? scoreLabel;
    private Label? perfectJumpLabel;
    private Label? instructionLabel;
    private Label? gameOverLabel;
    private Button? startButton;
    private Label? startButtonLabel;
    private Button? restartButton;
    private Label? restartButtonLabel;
    private Panel? uiPanel;
    private Panel? chargeBarBackground;
    private Panel? chargeBarFill;
    private Label? chargeHintLabel;
    private Panel? gameTitlePanel;
    private Label? gameTitleLabel;

    // 平台类
    private class Platform
    {
        public float X { get; set; }        // X轴位置
        public float Z { get; set; }        // Z轴高度
        public float Width { get; set; }
        public float Height { get; set; }
        public required ActorModel Actor { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsPerfectTarget { get; set; } = false;  // 是否是完美跳跃目标
        public PlatformType Type { get; set; } = PlatformType.Normal; // 平台类型
    }

    // 平台类型枚举
    private enum PlatformType
    {
        Normal,     // 普通平台
        Bouncy,     // 弹跳平台
        Moving,     // 移动平台
        Fragile     // 易碎平台
    }

    // 收集品类
    private class Collectible
    {
        public float X { get; set; }        // X轴位置
        public float Z { get; set; }        // Z轴高度
        public required ActorModel Actor { get; set; }
        public bool IsCollected { get; set; }
        public int Points { get; set; } = 10;
        public CollectibleType Type { get; set; } = CollectibleType.Gem; // 收集品类型
    }

    // 收集品类型枚举
    private enum CollectibleType
    {
        Gem,        // 宝石
        Star,       // 星星
        Coin,       // 金币
        PowerUp     // 能量道具
    }

    public static void OnRegisterGameClass()
    {
        Game.Logger.LogInformation("Jump Jump game (redesigned) registered");
        Game.OnGameTriggerInitialization += RegisterAll;
    }

    public static void RegisterAll()
    {
        // 如果游戏模式不是JumpJump，则不注册触发器
        if (Game.GameModeLink != ScopeData.GameMode.JumpJump)
        {
            return;
        }

        Game.Logger.LogInformation("Registering Jump Jump game triggers");
        Trigger<EventGameStart> trigger = new(async (s, d) =>
        {
            Game.Logger.LogInformation("Jump Jump game started");
            var game = new JumpJump();
            game.Initialize();
            return true;
        });
        trigger.Register(Game.Instance);
    }

    private void Initialize()
    {
        Game.Logger.LogInformation("Initializing Jump Jump game (redesigned)");

        // 初始化游戏状态
        playerX = 100f;
        playerZ = 100f;             // Z轴作为高度
        playerVelocityX = 0f;
        playerVelocityZ = 0f;
        isCharging = false;
        isOnGround = false;
        chargeTime = 0f;
        score = 0;
        perfectJumps = 0;
        isGameRunning = false;
        isGameStarted = false;
        gameTime = 0f;
        cameraX = 0f;
        cameraY = 0f;

        // 创建UI
        CreateUI();

        // 注册输入事件
        RegisterInputEvents();

        // 启用思考器
        (this as IThinker).DoesThink = true;
    }

    private void CreateUI()
    {
        // 创建主UI面板
        uiPanel = new Panel
        {
            Width = 800,
            Height = 600,
            Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))
        };

        // 游戏标题
        gameTitlePanel = new Panel
        {
            Width = 400,
            Height = 80,
            Position = new UIPosition(200, 50),
            PositionType = UIPositionType.Absolute,
            Background = new SolidColorBrush(Color.FromArgb(200, 100, 50, 200))
        };

        gameTitleLabel = new Label
        {
            Text = "🦘 JUMP JUMP",
            FontSize = 32,
            TextColor = Color.White,
            Position = new UIPosition(50, 20),
            PositionType = UIPositionType.Absolute
        };

        gameTitlePanel.AddChild(gameTitleLabel);

        // 开始按钮
        startButton = new Button
        {
            Width = 150,
            Height = 50,
            Position = new UIPosition(325, 200),
            PositionType = UIPositionType.Absolute,
            Background = new SolidColorBrush(Color.FromArgb(255, 50, 150, 50))
        };

        startButtonLabel = new Label
        {
            Text = "开始游戏",
            FontSize = 18,
            TextColor = Color.White,
            Position = new UIPosition(35, 15),
            PositionType = UIPositionType.Absolute
        };

        startButton.AddChild(startButtonLabel);
        startButton.OnPointerClicked += OnStartClicked;

        // 分数标签
        scoreLabel = new Label
        {
            Text = "分数: 0",
            FontSize = 24,
            TextColor = Color.White,
            Position = new UIPosition(UI_MARGIN, UI_MARGIN),
            PositionType = UIPositionType.Absolute,
            Visible = false
        };

        // 完美跳跃标签
        perfectJumpLabel = new Label
        {
            Text = "完美跳跃 x0",
            FontSize = 18,
            TextColor = Color.Gold,
            Position = new UIPosition(UI_MARGIN, UI_MARGIN + 40),
            PositionType = UIPositionType.Absolute,
            Visible = false
        };

        // 游戏说明
        instructionLabel = new Label
        {
            Text = "按住空格键蓄力，释放跳跃！\n收集宝石获得分数，连续完美跳跃有额外奖励！",
            FontSize = 16,
            TextColor = Color.LightBlue,
            Position = new UIPosition(200, 300),
            PositionType = UIPositionType.Absolute,
            Visible = false
        };

        // 蓄力条背景
        chargeBarBackground = new Panel
        {
            Width = CHARGE_BAR_WIDTH,
            Height = CHARGE_BAR_HEIGHT,
            Position = new UIPosition(400 - CHARGE_BAR_WIDTH / 2, 500),
            PositionType = UIPositionType.Absolute,
            Background = new SolidColorBrush(Color.FromArgb(150, 50, 50, 50)),
            Visible = false
        };

        // 蓄力条填充
        chargeBarFill = new Panel
        {
            Width = 0,
            Height = CHARGE_BAR_HEIGHT - 4,
            Position = new UIPosition(2, 2),
            PositionType = UIPositionType.Absolute,
            Background = new SolidColorBrush(Color.Orange)
        };

        chargeBarBackground.AddChild(chargeBarFill);

        // 蓄力提示
        chargeHintLabel = new Label
        {
            Text = "蓄力中...",
            FontSize = 14,
            TextColor = Color.White,
            Position = new UIPosition(400 - 30, 470),
            PositionType = UIPositionType.Absolute,
            Visible = false
        };

        // 游戏结束标签
        gameOverLabel = new Label
        {
            Text = "游戏结束！",
            FontSize = 36,
            TextColor = Color.Red,
            Position = new UIPosition(300, 250),
            PositionType = UIPositionType.Absolute,
            Visible = false
        };

        // 重新开始按钮
        restartButton = new Button
        {
            Width = 150,
            Height = 50,
            Position = new UIPosition(325, 350),
            PositionType = UIPositionType.Absolute,
            Background = new SolidColorBrush(Color.FromArgb(255, 150, 50, 50)),
            Visible = false
        };

        restartButtonLabel = new Label
        {
            Text = "重新开始",
            FontSize = 18,
            TextColor = Color.White,
            Position = new UIPosition(35, 15),
            PositionType = UIPositionType.Absolute
        };

        restartButton.AddChild(restartButtonLabel);
        restartButton.OnPointerClicked += OnRestartClicked;

        // 添加所有UI元素到主面板
        uiPanel.AddChild(gameTitlePanel);
        uiPanel.AddChild(startButton);
        uiPanel.AddChild(scoreLabel);
        uiPanel.AddChild(perfectJumpLabel);
        uiPanel.AddChild(instructionLabel);
        uiPanel.AddChild(chargeBarBackground);
        uiPanel.AddChild(chargeHintLabel);
        uiPanel.AddChild(gameOverLabel);
        uiPanel.AddChild(restartButton);

        // 将主面板添加到游戏
        uiPanel.AddToVisualTree();
    }

    private void CreateGameScene()
    {
        // 创建背景（使用平面作为天空盒）
        CreateBackground();

        // 创建初始平台
        CreateInitialPlatforms();

        // 创建玩家
        CreatePlayer();

        // 创建一些收集品
        CreateInitialCollectibles();

        // 设置相机
        SetupCamera();
    }

    private void CreateBackground()
    {
        // 创建远处的地平线装饰（可选）
        // 注意：在WasiCore中，通常不需要手动创建天空背景
        // 框架会自动处理背景渲染
        
        Game.Logger.LogDebug("背景设置完成 - 使用框架默认背景");
    }

    private void CreateInitialPlatforms()
    {
        // 创建起始平台
        var startPlatform = CreatePlatform(100f, 0f, 120f, 30f, PlatformType.Normal);
        platforms.Add(startPlatform);

        // 创建几个初始平台
        for (int i = 1; i <= 5; i++)
        {
            float x = 100f + i * Random.Shared.Next(80, 150);
            float z = Random.Shared.Next(0, 100);
            var platform = CreatePlatform(x, z, Random.Shared.Next(60, 100), Random.Shared.Next(20, 40), PlatformType.Normal);
            platforms.Add(platform);
        }
    }

    private Platform CreatePlatform(float x, float z, float width, float height, PlatformType type)
    {
        // 根据平台类型选择形状和颜色
        PrimitiveShape shape;
        HdrColor color;
        
        switch (type)
        {
            case PlatformType.Bouncy:
                shape = PrimitiveShape.Cube;
                color = new HdrColor(Color.SpringGreen);
                break;
            case PlatformType.Moving:
                shape = PrimitiveShape.Cube;
                color = new HdrColor(Color.Orange);
                break;
            case PlatformType.Fragile:
                shape = PrimitiveShape.Cube;
                color = new HdrColor(Color.Red);
                break;
            default:
                shape = PrimitiveShape.Cube;
                color = new HdrColor(Color.SaddleBrown);
                break;
        }

        // 创建平台Actor
        var platformActor = AIShapeFactory.CreateShape(
            shape,
            new ScenePoint(new Vector3(x, 0, z), Game.LocalScene),
            new Vector3(width / 100f, 20f / 100f, height / 100f), // 注意：Y轴是深度，Z轴是高度
            null
        );

        // 应用自定义颜色
        if (platformActor is IActorColorizable tintable)
        {
            tintable.InitializeTintColorAggregators();
            tintable.SetTintColor(color, GameCore.ActorSystem.Enum.TintColorType.Override, "platform_color");
        }

        gameActors.Add(platformActor);

        return new Platform
        {
            X = x,
            Z = z,
            Width = width,
            Height = height,
            Actor = platformActor,
            Type = type
        };
    }

    private void CreatePlayer()
    {
        // 创建玩家Actor - 使用胶囊形状，更像经典跳一跳的角色
        playerActor = AIShapeFactory.CreateShape(
            PrimitiveShape.Capsule,
            new ScenePoint(new Vector3(playerX, 0, playerZ), Game.LocalScene), // X位置，Y水平(0)，Z高度
            new Vector3(0.9f, 1.2f, 0.9f), // 稍高一点的胶囊形状
            null
        );

        // 应用玩家颜色（蓝色）
        if (playerActor is IActorColorizable tintable)
        {
            tintable.InitializeTintColorAggregators();
            tintable.SetTintColor(new HdrColor(Color.DodgerBlue), GameCore.ActorSystem.Enum.TintColorType.Override, "player_color");
        }

        gameActors.Add(playerActor);
        Game.Logger.LogDebug("创建玩家: 位置({X}, {Z})", playerX, playerZ);
    }

    private void CreateInitialCollectibles()
    {
        // 创建一些初始收集品
        for (int i = 0; i < 3; i++)
        {
            float x = Random.Shared.Next(200, 600);
            float z = Random.Shared.Next(50, 150);
            CreateCollectible(x, z, CollectibleType.Gem);
        }
    }

    private void CreateCollectible(float x, float z, CollectibleType type)
    {
        // 根据收集品类型选择形状和颜色
        PrimitiveShape shape;
        HdrColor color;
        int points;
        
        switch (type)
        {
            case CollectibleType.Star:
                shape = PrimitiveShape.Cone;
                color = new HdrColor(Color.Gold);
                points = 20;
                break;
            case CollectibleType.Coin:
                shape = PrimitiveShape.Cylinder;
                color = new HdrColor(Color.Yellow);
                points = 15;
                break;
            case CollectibleType.PowerUp:
                shape = PrimitiveShape.Sphere;
                color = new HdrColor(Color.Magenta);
                points = 25;
                break;
            default: // Gem
                shape = PrimitiveShape.Cone;
                color = new HdrColor(Color.Cyan);
                points = 10;
                break;
        }

        var collectibleActor = AIShapeFactory.CreateShape(
            shape,
            new ScenePoint(new Vector3(x, 0, z), Game.LocalScene),
            new Vector3(0.4f, 0.4f, 0.4f), // 较小的收集品
            null
        );

        // 应用自定义颜色
        if (collectibleActor is IActorColorizable tintable)
        {
            tintable.InitializeTintColorAggregators();
            tintable.SetTintColor(color, GameCore.ActorSystem.Enum.TintColorType.Override, "collectible_color");
        }

        // 添加旋转动画
        collectibleActor.Rotation = new Vector3(0, 0, 0);

        gameActors.Add(collectibleActor);

        collectibles.Add(new Collectible
        {
            X = x,
            Z = z,
            Actor = collectibleActor,
            Type = type,
            Points = points
        });
    }

    private void SetupCamera()
    {
        try
        {
            // 设置相机初始位置 - 使用框架的默认镜头设置
            cameraX = playerX;
            cameraY = -CAMERA_DISTANCE; // Y轴负方向（向后）
            
            ScreenViewport.Primary.Camera.SetPosition(
                new Vector3(cameraX, cameraY, CAMERA_HEIGHT), // 相机在上方稍后观察：X跟随，Y深度，Z高度
                TimeSpan.FromSeconds(0.1)
            );
            
            // 设置相机朝向（俯视角）
            ScreenViewport.Primary.Camera.SetRotation(
                new GameCore.CameraSystem.Struct.CameraRotation(-90f, -70f, 0f), // Yaw: -90°, Pitch: -70°, Roll: 0°
                TimeSpan.FromSeconds(0.1)
            );
            
            Game.Logger.LogDebug("相机设置完成");
        }
        catch (Exception ex)
        {
            Game.Logger.LogWarning("设置相机失败: {Error}", ex.Message);
        }
    }

    private void RegisterInputEvents()
    {
        // 监听空格键按下（开始蓄力）
        Trigger<EventGameKeyDown> keyDownTrigger = new(async (s, d) =>
        {
            if (d.Key == GameCore.Platform.SDL.VirtualKey.Space && isGameRunning && isOnGround && !isCharging)
            {
                StartCharging();
            }
            return true;
        });
        keyDownTrigger.Register(Game.Instance);

        // 监听空格键释放（执行跳跃）
        Trigger<EventGameKeyUp> keyUpTrigger = new(async (s, d) =>
        {
            if (d.Key == GameCore.Platform.SDL.VirtualKey.Space && isGameRunning && isCharging)
            {
                ExecuteJump();
            }
            return true;
        });
        keyUpTrigger.Register(Game.Instance);
    }

    private void OnStartClicked(object? sender, PointerEventArgs e)
    {
        StartGame();
    }

    private void OnRestartClicked(object? sender, PointerEventArgs e)
    {
        RestartGame();
    }

    private void StartGame()
    {
        if (!isGameStarted)
        {
            isGameStarted = true;
            isGameRunning = true;
            
            // 隐藏开始按钮，显示游戏UI
            if (startButton != null) startButton.Visible = false;
            if (gameTitlePanel != null) gameTitlePanel.Visible = false;
            if (instructionLabel != null) instructionLabel.Visible = true;
            
            // 创建游戏场景
            CreateGameScene();
            
            Game.Logger.LogInformation("跳一跳游戏开始！");
        }
    }

    private void StartCharging()
    {
        if (!isOnGround || isCharging) return;

        isCharging = true;
        chargeTime = 0f;
        
        // 显示蓄力UI
        if (chargeBarBackground != null) chargeBarBackground.Visible = true;
        if (chargeHintLabel != null) chargeHintLabel.Visible = true;
        
        // 创建蓄力特效
        CreateChargingEffect();
        
        Game.Logger.LogDebug("开始蓄力");
    }

    private void CreateChargingEffect()
    {
        // 在玩家周围创建蓄力粒子特效
        var effectPosition = new ScenePoint(new Vector3(playerX, 0, playerZ + 30), Game.LocalScene);
        var effectActor = AIShapeFactory.CreateShape(
            PrimitiveShape.Sphere,
            effectPosition,
            new Vector3(0.3f, 0.3f, 0.3f),
            null
        );

        // 设置特效颜色
        if (effectActor is IActorColorizable tintable)
        {
            tintable.InitializeTintColorAggregators();
            tintable.SetTintColor(new HdrColor(Color.Orange), GameCore.ActorSystem.Enum.TintColorType.Override, "charge_effect");
        }

        particleEffects.Add(effectActor);
    }

    private void ExecuteJump()
    {
        if (!isCharging) return;

        isCharging = false;
        isOnGround = false;

        // 隐藏蓄力UI
        if (chargeBarBackground != null) chargeBarBackground.Visible = false;
        if (chargeHintLabel != null) chargeHintLabel.Visible = false;

        // 清理蓄力特效
        ClearChargingEffects();

        // 计算跳跃力度（基于蓄力时间）
        float chargeRatio = Math.Min(chargeTime / MAX_CHARGE_TIME, 1f);
        float jumpForce = MIN_JUMP_FORCE + (MAX_JUMP_FORCE - MIN_JUMP_FORCE) * chargeRatio;
        
        // 设置跳跃速度（45度角跳跃）
        float angle = MathF.PI / 4f; // 45度
        playerVelocityX = jumpForce * MathF.Cos(angle) * 0.8f; // 水平速度
        playerVelocityZ = jumpForce * MathF.Sin(angle);        // 垂直速度（Z轴向上为正）

        // 创建跳跃特效
        CreateJumpEffect();

        Game.Logger.LogDebug("执行跳跃: 蓄力时间={ChargeTime:F2}s, 力度={JumpForce:F1}", chargeTime, jumpForce);
    }

    private void CreateJumpEffect()
    {
        // 在玩家脚下创建跳跃特效
        var effectPosition = new ScenePoint(new Vector3(playerX, 0, playerZ), Game.LocalScene);
        var effectActor = AIShapeFactory.CreateShape(
            PrimitiveShape.Cone,
            effectPosition,
            new Vector3(0.5f, 0.5f, 0.5f),
            null
        );

        // 设置特效颜色
        if (effectActor is IActorColorizable tintable)
        {
            tintable.InitializeTintColorAggregators();
            tintable.SetTintColor(new HdrColor(Color.Yellow), GameCore.ActorSystem.Enum.TintColorType.Override, "jump_effect");
        }

        particleEffects.Add(effectActor);
    }

    private void ClearChargingEffects()
    {
        // 清理蓄力特效
        foreach (var effect in particleEffects.ToList())
        {
            if (effect != null)
            {
                effect.Destroy();
                particleEffects.Remove(effect);
            }
        }
    }

    private void UpdateGame(float deltaTime)
    {
        if (!isGameRunning) return;

        gameTime += deltaTime;

        // 更新蓄力
        UpdateCharging(deltaTime);

        // 更新玩家物理
        UpdatePlayerPhysics(deltaTime);

        // 更新相机
        UpdateCamera(deltaTime);

        // 检测碰撞
        CheckCollisions();

        // 生成新平台
        GenerateNewPlatforms();

        // 清理远处对象
        CleanupDistantObjects();

        // 更新UI
        UpdateUI();

        // 更新粒子特效
        UpdateParticleEffects(deltaTime);

        // 检查游戏结束
        CheckGameOver();
    }

    private void UpdateCharging(float deltaTime)
    {
        if (!isCharging) return;

        chargeTime += deltaTime;
        chargeTime = Math.Min(chargeTime, MAX_CHARGE_TIME);

        // 更新蓄力条
        if (chargeBarFill != null)
        {
            float chargeRatio = chargeTime / MAX_CHARGE_TIME;
            chargeBarFill.Width = (CHARGE_BAR_WIDTH - 4) * chargeRatio;
            
            // 根据蓄力程度改变颜色
            Color barColor = chargeRatio < 0.7f ? Color.Orange : 
                           chargeRatio < 0.9f ? Color.Yellow : Color.Red;
            chargeBarFill.Background = new SolidColorBrush(barColor);
        }

        // 蓄力时让玩家轻微颤动（视觉反馈）
        if (playerActor != null)
        {
            float shake = MathF.Sin(gameTime * 20f) * chargeTime * 2f;
            playerActor.Position = new ScenePoint(new Vector3(playerX + shake, 0, playerZ), Game.LocalScene);
        }
    }

    private void UpdatePlayerPhysics(float deltaTime)
    {
        if (isCharging) return; // 蓄力时不更新物理

        // 应用重力（向下）
        if (!isOnGround)
        {
            playerVelocityZ -= GRAVITY * deltaTime;
        }

        // 更新位置
        playerX += playerVelocityX * deltaTime;
        playerZ += playerVelocityZ * deltaTime;

        // 更新玩家Actor位置
        if (playerActor != null)
        {
            playerActor.Position = new ScenePoint(new Vector3(playerX, 0, playerZ), Game.LocalScene);
        }

        // 应用空气阻力
        if (!isOnGround)
        {
            playerVelocityX *= 0.99f; // 轻微空气阻力
        }
    }

    private void UpdateCamera(float deltaTime)
    {
        if (playerActor == null) return;

        // 平滑跟随玩家
        float targetCameraX = playerX;
        cameraX = cameraX + (targetCameraX - cameraX) * CAMERA_FOLLOW_SPEED * deltaTime;

        // 更新相机位置
        try
        {
            ScreenViewport.Primary.Camera.SetPosition(
                new Vector3(cameraX, cameraY, CAMERA_HEIGHT),
                TimeSpan.FromSeconds(deltaTime)
            );
        }
        catch (Exception ex)
        {
            Game.Logger.LogWarning("更新相机失败: {Error}", ex.Message);
        }
    }

    private void UpdateParticleEffects(float deltaTime)
    {
        // 更新粒子特效的旋转和缩放
        for (int i = particleEffects.Count - 1; i >= 0; i--)
        {
            var effect = particleEffects[i];
            if (effect == null || !effect.IsValid)
            {
                particleEffects.RemoveAt(i);
                continue;
            }

            // 让特效旋转
            effect.Rotation = new Vector3(
                effect.Rotation.X,
                effect.Rotation.Y + 90f * deltaTime, // Y轴旋转
                effect.Rotation.Z
            );

            // 让特效轻微缩放
            float scale = 1f + MathF.Sin(gameTime * 5f) * 0.1f;
            effect.Scale = new Vector3(scale, scale, scale);
        }
    }

    private void CheckCollisions()
    {
        // 检查平台碰撞
        for (int i = platforms.Count - 1; i >= 0; i--)
        {
            var platform = platforms[i];
            if (!platform.IsActive) continue;

            // 检查玩家是否落在平台上
            if (playerX + PLAYER_SIZE / 2 > platform.X &&
                playerX - PLAYER_SIZE / 2 < platform.X + platform.Width &&
                playerZ - PLAYER_SIZE / 2 <= platform.Z + platform.Height &&
                playerZ + PLAYER_SIZE / 2 >= platform.Z &&
                playerVelocityZ < 0) // 只有下落时才能落在平台上
            {
                // 玩家落在平台上
                playerZ = platform.Z + platform.Height + PLAYER_SIZE / 2;
                playerVelocityZ = 0f;
                playerVelocityX = 0f;
                isOnGround = true;

                // 检查是否是完美跳跃（落在平台中心）
                float platformCenter = platform.X + platform.Width / 2;
                float playerCenter = playerX;
                float centerDistance = Math.Abs(playerCenter - platformCenter);
                
                if (centerDistance < platform.Width * 0.2f) // 落在中心20%范围内
                {
                    perfectJumps++;
                    score += 20; // 完美跳跃额外奖励
                    
                    // 创建完美跳跃特效
                    CreatePerfectJumpEffect();
                    
                    Game.Logger.LogDebug("完美跳跃！连续次数: {Count}", perfectJumps);
                }
                else
                {
                    perfectJumps = 0; // 重置连续完美跳跃
                    score += 5; // 普通落地分数
                }

                // 根据平台类型处理特殊效果
                HandlePlatformSpecialEffects(platform);

                Game.Logger.LogDebug("玩家落在平台上: ({X}, {Z})", platform.X, platform.Z);
                break;
            }
        }

        // 检查收集品碰撞
        for (int i = collectibles.Count - 1; i >= 0; i--)
        {
            var collectible = collectibles[i];
            if (collectible.IsCollected) continue;

            float distance = Vector2.Distance(
                new Vector2(playerX, playerZ),
                new Vector2(collectible.X, collectible.Z)
            );

            if (distance < PLAYER_SIZE)
            {
                // 收集收集品
                collectible.IsCollected = true;
                score += collectible.Points;
                
                // 创建收集特效
                CreateCollectionEffect(collectible);
                
                // 销毁收集品Actor
                collectible.Actor.Destroy();
                gameActors.Remove(collectible.Actor);
                
                Game.Logger.LogDebug("收集到收集品: +{Points} 分", collectible.Points);
            }
        }
    }

    private void HandlePlatformSpecialEffects(Platform platform)
    {
        switch (platform.Type)
        {
            case PlatformType.Bouncy:
                // 弹跳平台
                playerVelocityZ = 800f;
                isOnGround = false;
                CreateBounceEffect();
                break;
                
            case PlatformType.Fragile:
                // 易碎平台
                platform.IsActive = false;
                platform.Actor.Destroy();
                gameActors.Remove(platform.Actor);
                CreateBreakEffect(platform);
                break;
        }
    }

    private void CreatePerfectJumpEffect()
    {
        // 创建完美跳跃特效
        var effectPosition = new ScenePoint(new Vector3(playerX, 0, playerZ + 50), Game.LocalScene);
        var effectActor = AIShapeFactory.CreateShape(
            PrimitiveShape.Sphere,
            effectPosition,
            new Vector3(0.8f, 0.8f, 0.8f),
            null
        );

        // 设置特效颜色
        if (effectActor is IActorColorizable tintable)
        {
            tintable.InitializeTintColorAggregators();
            tintable.SetTintColor(new HdrColor(Color.Gold), GameCore.ActorSystem.Enum.TintColorType.Override, "perfect_jump");
        }

        particleEffects.Add(effectActor);
    }

    private void CreateCollectionEffect(Collectible collectible)
    {
        // 创建收集特效
        var effectPosition = new ScenePoint(new Vector3(collectible.X, 0, collectible.Z), Game.LocalScene);
        var effectActor = AIShapeFactory.CreateShape(
            PrimitiveShape.Cone,
            effectPosition,
            new Vector3(0.6f, 0.6f, 0.6f),
            null
        );

        // 设置特效颜色
        if (effectActor is IActorColorizable tintable)
        {
            tintable.InitializeTintColorAggregators();
            tintable.SetTintColor(new HdrColor(Color.White), GameCore.ActorSystem.Enum.TintColorType.Override, "collection");
        }

        particleEffects.Add(effectActor);
    }

    private void CreateBounceEffect()
    {
        // 创建弹跳特效
        var effectPosition = new ScenePoint(new Vector3(playerX, 0, playerZ), Game.LocalScene);
        var effectActor = AIShapeFactory.CreateShape(
            PrimitiveShape.Sphere,
            effectPosition,
            new Vector3(0.7f, 0.7f, 0.7f),
            null
        );

        // 设置特效颜色
        if (effectActor is IActorColorizable tintable)
        {
            tintable.InitializeTintColorAggregators();
            tintable.SetTintColor(new HdrColor(Color.SpringGreen), GameCore.ActorSystem.Enum.TintColorType.Override, "bounce");
        }

        particleEffects.Add(effectActor);
    }

    private void CreateBreakEffect(Platform platform)
    {
        // 创建平台破碎特效
        var effectPosition = new ScenePoint(new Vector3(platform.X + platform.Width / 2, 0, platform.Z), Game.LocalScene);
        var effectActor = AIShapeFactory.CreateShape(
            PrimitiveShape.Cube,
            effectPosition,
            new Vector3(0.5f, 0.5f, 0.5f),
            null
        );

        // 设置特效颜色
        if (effectActor is IActorColorizable tintable)
        {
            tintable.InitializeTintColorAggregators();
            tintable.SetTintColor(new HdrColor(Color.Red), GameCore.ActorSystem.Enum.TintColorType.Override, "break");
        }

        particleEffects.Add(effectActor);
    }

    private void GenerateNewPlatforms()
    {
        if (platforms.Count < 8) // 保持平台数量
        {
            var lastPlatform = platforms[^1];
            float x = lastPlatform.X + Random.Shared.Next(80, 150);
            float z = Random.Shared.Next(0, 120);
            
            // 随机选择平台类型
            PlatformType type = Random.Shared.Next(100) < 20 ? PlatformType.Bouncy :
                               Random.Shared.Next(100) < 15 ? PlatformType.Fragile :
                               PlatformType.Normal;
            
            var platform = CreatePlatform(x, z, Random.Shared.Next(60, 100), Random.Shared.Next(20, 40), type);
            platforms.Add(platform);

            // 随机创建收集品
            if (Random.Shared.Next(100) < 30)
            {
                float collectibleX = x + Random.Shared.Next(-20, 20);
                float collectibleZ = z + Random.Shared.Next(30, 60);
                
                CollectibleType collectibleType = Random.Shared.Next(100) < 20 ? CollectibleType.Star :
                                               Random.Shared.Next(100) < 30 ? CollectibleType.Coin :
                                               Random.Shared.Next(100) < 40 ? CollectibleType.PowerUp :
                                               CollectibleType.Gem;
                
                CreateCollectible(collectibleX, collectibleZ, collectibleType);
            }
        }
    }

    private void CleanupDistantObjects()
    {
        // 清理远处的平台
        for (int i = platforms.Count - 1; i >= 0; i--)
        {
            if (platforms[i].X < playerX - 600f)
            {
                var platform = platforms[i];
                if (platform.Actor != null)
                {
                    platform.Actor.Destroy();
                    gameActors.Remove(platform.Actor);
                }
                platforms.RemoveAt(i);
            }
        }

        // 清理远处的收集品
        for (int i = collectibles.Count - 1; i >= 0; i--)
        {
            if (collectibles[i].X < playerX - 600f)
            {
                var collectible = collectibles[i];
                if (!collectible.IsCollected && collectible.Actor != null)
                {
                    collectible.Actor.Destroy();
                    gameActors.Remove(collectible.Actor);
                }
                collectibles.RemoveAt(i);
            }
        }
    }

    private void UpdateUI()
    {
        if (scoreLabel != null)
        {
            scoreLabel.Text = $"分数: {score}";
        }

        if (perfectJumpLabel != null)
        {
            if (perfectJumps > 0)
            {
                perfectJumpLabel.Text = $"完美跳跃 x{perfectJumps}";
                perfectJumpLabel.Visible = true;
            }
            else
            {
                perfectJumpLabel.Visible = false;
            }
        }
    }

    private void CheckGameOver()
    {
        // 检查玩家是否掉出世界
        if (playerZ < -300f) // 掉得太低（Z轴负值）
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameRunning = false;

        // 显示游戏结束UI
        if (gameOverLabel != null) gameOverLabel.Visible = true;
        if (restartButton != null) restartButton.Visible = true;
        if (instructionLabel != null) instructionLabel.Visible = false;

        // 隐藏蓄力UI
        if (chargeBarBackground != null) chargeBarBackground.Visible = false;
        if (chargeHintLabel != null) chargeHintLabel.Visible = false;

        // 停止思考器
        (this as IThinker).DoesThink = false;
        
        Game.Logger.LogInformation("游戏结束！最终分数: {Score}", score);
    }

    private void RestartGame()
    {
        // 清理游戏对象
        foreach (var actor in gameActors)
        {
            if (actor != null) actor.Destroy();
        }

        foreach (var effect in particleEffects)
        {
            if (effect != null) effect.Destroy();
        }

        gameActors.Clear();
        particleEffects.Clear();
        platforms.Clear();
        collectibles.Clear();
        playerActor = null;

        // 隐藏游戏结束UI
        if (gameOverLabel != null) gameOverLabel.Visible = false;
        if (restartButton != null) restartButton.Visible = false;

        // 显示开始按钮和标题
        if (startButton != null) startButton.Visible = true;
        if (gameTitlePanel != null) gameTitlePanel.Visible = true;

        // 重新初始化
        Initialize();
        Game.Logger.LogInformation("游戏重新开始");
    }

    public void Think(int deltaTime)
    {
        UpdateGame(deltaTime / 1000f); // 转换为秒
    }
}
#else
namespace GameEntry.JumpJumpGame;

public class JumpJump : IGameClass
{
    public static void OnRegisterGameClass()
    {
        Game.Logger.LogWarning("Jump Jump game is only available in CLIENT configuration");
    }

    public static void RegisterAll()
    {
        Game.Logger.LogWarning("Jump Jump game is only available in CLIENT configuration");
    }
}
#endif