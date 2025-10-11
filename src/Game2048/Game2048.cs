#if CLIENT
using GameUI.Control.Extensions;
using GameUI.Control.Primitive;
using GameUI.Control.Enum;
using GameUI.Enum;
using GameUI.Brush;
using static GameUI.Control.Extensions.UI;
using System.Drawing;

namespace GameEntry.Game2048;

public class Game2048 : IGameClass, IThinker
{
    // 游戏常量
    private const int GRID_SIZE = 4;
    private const int CELL_SIZE = 120;
    private const int CELL_GAP = 10;
    
    // 游戏状态
    private int[,] grid = new int[GRID_SIZE, GRID_SIZE];
    private int score = 0;
    private bool gameWon = false;
    private bool gameOver = false;
    
    // UI组件
    private Panel? gamePanel;
    private Label? scoreLabel;
    private Label? gameStatusLabel;
    private Panel[,]? gridCells = new Panel[GRID_SIZE, GRID_SIZE];
    private Label[,]? cellLabels = new Label[GRID_SIZE, GRID_SIZE];
    private Button? restartButton;
    
    public static void OnRegisterGameClass()
    {
        Game.Logger.LogInformation("2048 Game registered");
        Game.OnGameTriggerInitialization += RegisterAll;
    }
    
    public static void RegisterAll()
    {
        if (Game.GameModeLink != ScopeData.GameMode.Game2048)
        {
            return;
        }
        
        Game.Logger.LogInformation("Initializing 2048 Game triggers");
        
        var game2048Instance = new Game2048();
        Game.OnGameUIInitialization += game2048Instance.InitializeUI;
        Game.RegisterThinker(game2048Instance);
    }
    
    private void InitializeUI()
    {
        Game.Logger.LogInformation("Initializing 2048 Game UI");
        
        // 初始化游戏
        InitializeGame();
        
        // 创建主游戏界面
        CreateGameUI();
        
        // 显示UI
        gamePanel?.Show().AddToRoot();
        
        Game.Logger.LogInformation("2048 Game UI initialized successfully");
    }
    
    private void InitializeGame()
    {
        // 清空网格
        grid = new int[GRID_SIZE, GRID_SIZE];
        score = 0;
        gameWon = false;
        gameOver = false;
        
        // 添加初始方块
        AddRandomTile();
        AddRandomTile();
    }
    
    private void CreateGameUI()
    {
        gamePanel = VStack(30,
            // 标题和分数
            VStack(20,
                Title("2048")
                    .FontSize(64)
                    .Bold()
                    .TextColor(Colors.Primary),
                    
                HStack(40,
                    CreateScorePanel(),
                    CreateBestScorePanel()
                )
            ),
            
            // 游戏状态标签
            (gameStatusLabel = Label("点击方向按钮移动方块")
                .FontSize(18)
                .TextColor(Colors.OnSurface)),
            
            // 游戏网格
            CreateGameGrid(),
            
            // 方向控制按钮
            VStack(20,
                // 上按钮
                HStack(0,
                    Spacer(),
                    Secondary("↑")
                        .FontSize(24)
                        .Size(60, 60)
                        .Click(() => HandleMove("up")),
                    Spacer()
                ),
                // 左右按钮
                HStack(20,
                    Secondary("←")
                        .FontSize(24)
                        .Size(60, 60)
                        .Click(() => HandleMove("left")),
                    Spacer(),
                    Secondary("→")
                        .FontSize(24)
                        .Size(60, 60)
                        .Click(() => HandleMove("right"))
                ),
                // 下按钮
                HStack(0,
                    Spacer(),
                    Secondary("↓")
                        .FontSize(24)
                        .Size(60, 60)
                        .Click(() => HandleMove("down")),
                    Spacer()
                )
            ),
            
            // 重新开始按钮
            (restartButton = Primary("重新开始")
                .FontSize(20)
                .Padding(20, 12)
                .CornerRadius(8)
                .Click(RestartGame)),
            
            // 游戏说明
            VStack(10,
                Body("游戏规则：").Bold(),
                Body("• 点击方向按钮移动方块"),
                Body("• 相同数字的方块会合并成更大的数字"),
                Body("• 目标是创造出 2048 方块！")
            )
        )
        .Padding(40)
        .Background(Colors.Background)
        .Stretch()
        .GrowRatio(1, 1);
        
    }
    
    private Panel CreateScorePanel()
    {
        return UI.Panel()
            .Background(Colors.Surface)
            .CornerRadius(8)
            .Padding(20)
            .Add(
                VStack(5,
                    Caption("分数").Bold().TextColor(Colors.OnSurface),
                    (scoreLabel = Label("0")
                        .FontSize(24)
                        .Bold()
                        .TextColor(Colors.Primary))
                )
            );
    }
    
    private Panel CreateBestScorePanel()
    {
        return UI.Panel()
            .Background(Colors.Surface)
            .CornerRadius(8)
            .Padding(20)
            .Add(
                VStack(5,
                    Caption("最高分").Bold().TextColor(Colors.OnSurface),
                    Label("0")
                        .FontSize(24)
                        .Bold()
                        .TextColor(Colors.Secondary)
                )
            );
    }
    
    private Panel CreateGameGrid()
    {
        var gridPanel = Panel()
            .Background(Color.FromArgb(187, 173, 160))
            .CornerRadius(10)
            .Padding(CELL_GAP);
        
        var gridContainer = VStack(CELL_GAP);
        
        for (int row = 0; row < GRID_SIZE; row++)
        {
            var rowStack = HStack(CELL_GAP);
            
            for (int col = 0; col < GRID_SIZE; col++)
            {
                var cell = CreateGameCell(row, col);
                gridCells![row, col] = cell;
                rowStack.Add(cell);
            }
            
            gridContainer.Add(rowStack);
        }
        
        gridPanel.Add(gridContainer);
        return gridPanel;
    }
    
    private Panel CreateGameCell(int row, int col)
    {
        var cell = Panel()
            .Size(CELL_SIZE, CELL_SIZE)
            .Background(Color.FromArgb(205, 193, 180))
            .CornerRadius(6);
        
        var label = Label("")
            .FontSize(36)
            .Bold();
        
        cellLabels![row, col] = label;
        cell.Add(label);
        
        return cell;
    }
    
    private void HandleMove(string direction)
    {
        if (gameOver) return;
        
        bool moved = false;
        
        switch (direction.ToLower())
        {
            case "up":
                moved = MoveUp();
                break;
            case "down":
                moved = MoveDown();
                break;
            case "left":
                moved = MoveLeft();
                break;
            case "right":
                moved = MoveRight();
                break;
        }
        
        if (moved)
        {
            AddRandomTile();
            UpdateUI();
            CheckGameState();
        }
    }
    
    private bool MoveLeft()
    {
        bool moved = false;
        
        for (int row = 0; row < GRID_SIZE; row++)
        {
            var line = new int[GRID_SIZE];
            for (int col = 0; col < GRID_SIZE; col++)
            {
                line[col] = grid[row, col];
            }
            
            var newLine = MoveLine(line);
            
            for (int col = 0; col < GRID_SIZE; col++)
            {
                if (grid[row, col] != newLine[col])
                {
                    moved = true;
                    grid[row, col] = newLine[col];
                }
            }
        }
        
        return moved;
    }
    
    private bool MoveRight()
    {
        bool moved = false;
        
        for (int row = 0; row < GRID_SIZE; row++)
        {
            var line = new int[GRID_SIZE];
            for (int col = 0; col < GRID_SIZE; col++)
            {
                line[col] = grid[row, GRID_SIZE - 1 - col];
            }
            
            var newLine = MoveLine(line);
            
            for (int col = 0; col < GRID_SIZE; col++)
            {
                if (grid[row, GRID_SIZE - 1 - col] != newLine[col])
                {
                    moved = true;
                    grid[row, GRID_SIZE - 1 - col] = newLine[col];
                }
            }
        }
        
        return moved;
    }
    
    private bool MoveUp()
    {
        bool moved = false;
        
        for (int col = 0; col < GRID_SIZE; col++)
        {
            var line = new int[GRID_SIZE];
            for (int row = 0; row < GRID_SIZE; row++)
            {
                line[row] = grid[row, col];
            }
            
            var newLine = MoveLine(line);
            
            for (int row = 0; row < GRID_SIZE; row++)
            {
                if (grid[row, col] != newLine[row])
                {
                    moved = true;
                    grid[row, col] = newLine[row];
                }
            }
        }
        
        return moved;
    }
    
    private bool MoveDown()
    {
        bool moved = false;
        
        for (int col = 0; col < GRID_SIZE; col++)
        {
            var line = new int[GRID_SIZE];
            for (int row = 0; row < GRID_SIZE; row++)
            {
                line[row] = grid[GRID_SIZE - 1 - row, col];
            }
            
            var newLine = MoveLine(line);
            
            for (int row = 0; row < GRID_SIZE; row++)
            {
                if (grid[GRID_SIZE - 1 - row, col] != newLine[row])
                {
                    moved = true;
                    grid[GRID_SIZE - 1 - row, col] = newLine[row];
                }
            }
        }
        
        return moved;
    }
    
    private int[] MoveLine(int[] line)
    {
        var result = new int[GRID_SIZE];
        int position = 0;
        
        // 移动非零元素到左边
        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (line[i] != 0)
            {
                result[position] = line[i];
                position++;
            }
        }
        
        // 合并相同的相邻元素
        for (int i = 0; i < GRID_SIZE - 1; i++)
        {
            if (result[i] != 0 && result[i] == result[i + 1])
            {
                result[i] *= 2;
                result[i + 1] = 0;
                score += result[i];
                
                if (result[i] == 2048)
                {
                    gameWon = true;
                }
            }
        }
        
        // 再次移动非零元素到左边
        var finalResult = new int[GRID_SIZE];
        position = 0;
        
        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (result[i] != 0)
            {
                finalResult[position] = result[i];
                position++;
            }
        }
        
        return finalResult;
    }
    
    private void AddRandomTile()
    {
        var emptyCells = new List<(int row, int col)>();
        
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                if (grid[row, col] == 0)
                {
                    emptyCells.Add((row, col));
                }
            }
        }
        
        if (emptyCells.Count > 0)
        {
            var random = new Random();
            var randomCell = emptyCells[random.Next(emptyCells.Count)];
            grid[randomCell.row, randomCell.col] = random.NextDouble() < 0.9 ? 2 : 4;
        }
    }
    
    private void UpdateUI()
    {
        // 更新分数
        if (scoreLabel != null)
            scoreLabel.Text = score.ToString();
        
        // 更新网格
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                var value = grid[row, col];
                var cell = gridCells![row, col];
                var label = cellLabels![row, col];
                
                if (value == 0)
                {
                    if (label != null)
                        label.Text = "";
                    if (cell != null)
                        cell.Background = new SolidColorBrush(Color.FromArgb(205, 193, 180));
                }
                else
                {
                    if (label != null)
                    {
                        label.Text = value.ToString();
                        label.TextColor = GetTextColor(value);
                    }
                    if (cell != null)
                        cell.Background = GetCellColor(value);
                }
            }
        }
    }
    
    private SolidColorBrush GetCellColor(int value)
    {
        return value switch
        {
            2 => new SolidColorBrush(Color.FromArgb(238, 228, 218)),
            4 => new SolidColorBrush(Color.FromArgb(237, 224, 200)),
            8 => new SolidColorBrush(Color.FromArgb(242, 177, 121)),
            16 => new SolidColorBrush(Color.FromArgb(245, 149, 99)),
            32 => new SolidColorBrush(Color.FromArgb(246, 124, 95)),
            64 => new SolidColorBrush(Color.FromArgb(246, 94, 59)),
            128 => new SolidColorBrush(Color.FromArgb(237, 207, 114)),
            256 => new SolidColorBrush(Color.FromArgb(237, 204, 97)),
            512 => new SolidColorBrush(Color.FromArgb(237, 200, 80)),
            1024 => new SolidColorBrush(Color.FromArgb(237, 197, 63)),
            2048 => new SolidColorBrush(Color.FromArgb(237, 194, 46)),
            _ => new SolidColorBrush(Color.FromArgb(60, 58, 50))
        };
    }
    
    private Color GetTextColor(int value)
    {
        return value <= 4 ? Color.FromArgb(119, 110, 101) : Color.White;
    }
    
    private void CheckGameState()
    {
        if (gameWon && !gameOver)
        {
            if (gameStatusLabel != null)
            {
                gameStatusLabel.Text = "🎉 恭喜！你达到了 2048！";
            }
            return;
        }
        
        // 检查是否还有空格
        bool hasEmptyCell = false;
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                if (grid[row, col] == 0)
                {
                    hasEmptyCell = true;
                    break;
                }
            }
            if (hasEmptyCell) break;
        }
        
        if (hasEmptyCell) return;
        
        // 检查是否还能合并
        bool canMerge = false;
        for (int row = 0; row < GRID_SIZE; row++)
        {
            for (int col = 0; col < GRID_SIZE; col++)
            {
                int current = grid[row, col];
                
                // 检查右边
                if (col < GRID_SIZE - 1 && grid[row, col + 1] == current)
                {
                    canMerge = true;
                    break;
                }
                
                // 检查下面
                if (row < GRID_SIZE - 1 && grid[row + 1, col] == current)
                {
                    canMerge = true;
                    break;
                }
            }
            if (canMerge) break;
        }
        
        if (!canMerge)
        {
            gameOver = true;
            if (gameStatusLabel != null)
            {
                gameStatusLabel.Text = "😢 游戏结束！没有更多移动可能。";
            }
        }
    }
    
    private void RestartGame()
    {
        InitializeGame();
        UpdateUI();
        if (gameStatusLabel != null)
        {
            gameStatusLabel.Text = "点击方向按钮移动方块";
        }
    }
    
    public void Think(int deltaInMs)
    {
        // 游戏主循环，暂时不需要实现
    }
}
#endif