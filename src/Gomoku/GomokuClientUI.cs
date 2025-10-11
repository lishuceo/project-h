#if CLIENT
using Events;

using GameCore.PlayerAndUsers;
using GameUI.Brush;
using GameUI.Control;
using GameUI.Control.Primitive;
using GameUI.Control.Struct;
using GameUI.Enum;
using GameUI.Struct;

using System.Drawing;
using System.Text.Json;

using TriggerEncapsulation.Event;
using TriggerEncapsulation.Messaging;

using static GameEntry.Gomoku.GomokuGame;

namespace GameEntry.Gomoku;

/// <summary>
/// 五子棋客户端UI管理器 - 完整版本
/// </summary>
public class GomokuClientUI : IGameClass
{
    private static GomokuClientUI? _instance;
    
    // 触发器字段
    private static Trigger<EventServerMessage>? serverMessageTrigger;

    // 主要面板
    private Panel? _mainPanel;
    private Panel? _gamePanel;
    private Panel? _infoPanel;
    private Panel? _boardPanel;
    private Panel? _controlPanel;

    // 游戏棋盘
    private Button[,]? _boardButtons;
    private Panel? _lastMoveMarker;
    private List<Panel> _winningLineMarkers = new();

    // 信息显示
    private Label? _titleLabel;
    private Label? _currentPlayerLabel;
    private Label? _gameStateLabel;
    private Label? _scoreLabel;
    private Label? _errorLabel;
    private Label? _timerLabel;

    // 控制按钮
    private Button? _startButton;
    private Button? _restartButton;
    private Button? _undoButton;
    private Button? _hintButton;

    // 游戏状态
    private PieceType[] _localBoard = new PieceType[BOARD_SIZE * BOARD_SIZE];
    private GameState _gameState = GameState.WaitingForPlayers;
    private PieceType _currentPlayer = PieceType.Black;
    
    // 辅助方法：二维索引转一维
    private PieceType GetPiece(int row, int col)
    {
        if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE)
            return PieceType.None;
        return _localBoard[row * BOARD_SIZE + col];
    }
    private int _localPlayerId => Player.LocalPlayer.Id;
    private PieceType _localPlayerPiece = PieceType.Black;
    private Position _lastMove = new(-1, -1);
    private List<Position> _winningLine = new();
    private int _blackWins = 0;
    private int _whiteWins = 0;
    private string _errorMessage = "";
    private DateTime _turnStartTime = DateTime.Now;

    // 常量
    private const int BUTTON_SIZE = 28;
    private const int BUTTON_SPACING = 32;
    private const int BOARD_MARGIN = 20;
    private const int PANEL_WIDTH = 800;
    private const int PANEL_HEIGHT = 600;
    private const int GAME_PANEL_WIDTH = 520;
    private const int INFO_PANEL_WIDTH = 260;

    public static GomokuClientUI Instance => _instance ??= new GomokuClientUI();

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
        Game.OnGameUIInitialization += OnGameUIInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        if (Game.GameModeLink != ScopeData.GameMode.Gomoku)
        {
            return;
        }
        serverMessageTrigger = new Trigger<EventServerMessage>(OnServerMessageReceived, true);
        serverMessageTrigger.Register(Game.Instance);
    }

    private static void OnGameUIInitialization()
    {
        if (Game.GameModeLink != ScopeData.GameMode.Gomoku)
        {
            return;
        }
        Instance.InitializeUI();
        Game.Logger.LogInformation("🎮 Gomoku Client UI initialized (Enhanced Version)");
        Game.OnGameTriggerInitialization -= OnGameTriggerInitialization;
        Game.OnGameUIInitialization -= OnGameUIInitialization;
    }

    private void InitializeUI()
    {
        CreateUI();
        ShowUI();
        _ = StartUIUpdateTimer();
    }

    private void CreateUI()
    {
        // 主面板
        _mainPanel = new Panel
        {
            Background = new SolidColorBrush(Color.FromArgb(250, 250, 250)),
            Width = PANEL_WIDTH,
            Height = PANEL_HEIGHT,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FlowOrientation = Orientation.Horizontal,
            CornerRadius = 10
        };

        CreateGamePanel();
        CreateInfoPanel();
    }

    private void CreateGamePanel()
    {
        // 游戏面板（左侧）
        _gamePanel = new Panel
        {
            Background = new SolidColorBrush(Color.FromArgb(240, 240, 240)),
            Width = GAME_PANEL_WIDTH,
            Height = PANEL_HEIGHT - 20,
            Margin = new Thickness(10, 10, 5, 10),
            CornerRadius = 8,
            FlowOrientation = Orientation.Vertical
        };

        // 游戏标题
        _titleLabel = new Label
        {
            Text = "🎯 五子棋对战",
            TextColor = Color.FromArgb(70, 70, 70),
            FontSize = 24,
            Bold = true,
            Width = GAME_PANEL_WIDTH - 20,
            Height = 40,
            Margin = new Thickness(10, 10, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _gamePanel.AddChild(_titleLabel);

        // 棋盘面板
        _boardPanel = new Panel
        {
            Background = new SolidColorBrush(Color.FromArgb(222, 184, 135)),
            Width = BOARD_SIZE * BUTTON_SPACING + BOARD_MARGIN * 2,
            Height = BOARD_SIZE * BUTTON_SPACING + BOARD_MARGIN * 2,
            Margin = new Thickness(10, 5, 10, 10),
            CornerRadius = 5,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        CreateGameBoard();
        _gamePanel.AddChild(_boardPanel);
        _mainPanel!.AddChild(_gamePanel);
    }

    private void CreateGameBoard()
    {
        _boardButtons = new Button[BOARD_SIZE, BOARD_SIZE];

        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                var button = new Button
                {
                    Background = new SolidColorBrush(Color.FromArgb(245, 222, 179)),
                    Width = BUTTON_SIZE,
                    Height = BUTTON_SIZE,
                    Position = new UIPosition(
                        BOARD_MARGIN + col * BUTTON_SPACING,
                        BOARD_MARGIN + row * BUTTON_SPACING
                    ),
                    PositionType = UIPositionType.Absolute,
                    CornerRadius = BUTTON_SIZE / 2,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // 添加网格线效果
                if (row > 0 && row < BOARD_SIZE - 1 && col > 0 && col < BOARD_SIZE - 1)
                {
                    // 交叉点
                    button.Background = new SolidColorBrush(Color.FromArgb(220, 200, 160));
                }

                int currentRow = row;
                int currentCol = col;

                // 设置点击事件
                button.OnPointerClicked += (sender, e) =>
                {
                    HandleBoardClick(currentRow, currentCol);
                };

                _boardButtons[row, col] = button;
                _boardPanel!.AddChild(button);
            }
        }
    }

    private void CreateInfoPanel()
    {
        // 信息面板（右侧）
        _infoPanel = new Panel
        {
            Background = new SolidColorBrush(Color.FromArgb(245, 245, 245)),
            Width = INFO_PANEL_WIDTH,
            Height = PANEL_HEIGHT - 20,
            Margin = new Thickness(5, 10, 10, 10),
            CornerRadius = 8,
            FlowOrientation = Orientation.Vertical
        };

        CreateInfoSection();
        CreateControlSection();
        _mainPanel!.AddChild(_infoPanel);
    }

    private void CreateInfoSection()
    {
        // 当前玩家信息
        _currentPlayerLabel = new Label
        {
            Text = "当前玩家：黑棋",
            TextColor = Color.FromArgb(50, 50, 50),
            FontSize = 16,
            Bold = true,
            Width = INFO_PANEL_WIDTH - 20,
            Height = 30,
            Margin = new Thickness(10, 15, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _infoPanel!.AddChild(_currentPlayerLabel);

        // 游戏状态
        _gameStateLabel = new Label
        {
            Text = "等待游戏开始...",
            TextColor = Color.FromArgb(100, 100, 100),
            FontSize = 14,
            Width = INFO_PANEL_WIDTH - 20,
            Height = 25,
            Margin = new Thickness(10, 5, 10, 5),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _infoPanel!.AddChild(_gameStateLabel);

        // 计时器
        _timerLabel = new Label
        {
            Text = "⏱️ 00:00",
            TextColor = Color.FromArgb(80, 80, 80),
            FontSize = 12,
            Width = INFO_PANEL_WIDTH - 20,
            Height = 20,
            Margin = new Thickness(10, 5, 10, 10),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _infoPanel!.AddChild(_timerLabel);

        // 比分显示
        _scoreLabel = new Label
        {
            Text = "🏆 比分\n黑棋：0\n白棋：0",
            TextColor = Color.FromArgb(70, 70, 70),
            FontSize = 14,
            Width = INFO_PANEL_WIDTH - 20,
            Height = 60,
            Margin = new Thickness(10, 10, 10, 10),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _infoPanel!.AddChild(_scoreLabel);

        // 错误信息显示
        _errorLabel = new Label
        {
            Text = "",
            TextColor = Color.Red,
            FontSize = 12,
            Width = INFO_PANEL_WIDTH - 20,
            Height = 40,
            Margin = new Thickness(10, 10, 10, 10),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _infoPanel!.AddChild(_errorLabel);
    }

    private void CreateControlSection()
    {
        // 控制按钮面板
        _controlPanel = new Panel
        {
            Width = INFO_PANEL_WIDTH - 20,
            Height = 200,
            Margin = new Thickness(10, 20, 10, 10),
            FlowOrientation = Orientation.Vertical
        };

        // 开始游戏按钮
        _startButton = new Button
        {
            Background = new SolidColorBrush(Color.FromArgb(76, 175, 80)),
            Width = INFO_PANEL_WIDTH - 40,
            Height = 35,
            Margin = new Thickness(10, 5, 10, 5),
            CornerRadius = 5,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var startLabel = new Label
        {
            Text = "🎮 开始游戏",
            TextColor = Color.White,
            FontSize = 14,
            Bold = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _startButton.AddChild(startLabel);
        _startButton.OnPointerClicked += (sender, e) => StartGame();
        _controlPanel.AddChild(_startButton);

        // 重新开始按钮
        _restartButton = new Button
        {
            Background = new SolidColorBrush(Color.FromArgb(255, 152, 0)),
            Width = INFO_PANEL_WIDTH - 40,
            Height = 35,
            Margin = new Thickness(10, 5, 10, 5),
            CornerRadius = 5,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var restartLabel = new Label
        {
            Text = "🔄 重新开始",
            TextColor = Color.White,
            FontSize = 14,
            Bold = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _restartButton.AddChild(restartLabel);
        _restartButton.OnPointerClicked += (sender, e) => RestartGame();
        _controlPanel.AddChild(_restartButton);

        // 悔棋按钮
        _undoButton = new Button
        {
            Background = new SolidColorBrush(Color.FromArgb(96, 125, 139)),
            Width = INFO_PANEL_WIDTH - 40,
            Height = 35,
            Margin = new Thickness(10, 5, 10, 5),
            CornerRadius = 5,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var undoLabel = new Label
        {
            Text = "↶ 悔棋",
            TextColor = Color.White,
            FontSize = 14,
            Bold = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _undoButton.AddChild(undoLabel);
        _undoButton.OnPointerClicked += (sender, e) => RequestUndo();
        _controlPanel.AddChild(_undoButton);

        // 提示按钮
        _hintButton = new Button
        {
            Background = new SolidColorBrush(Color.FromArgb(103, 58, 183)),
            Width = INFO_PANEL_WIDTH - 40,
            Height = 35,
            Margin = new Thickness(10, 5, 10, 5),
            CornerRadius = 5,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var hintLabel = new Label
        {
            Text = "💡 提示",
            TextColor = Color.White,
            FontSize = 14,
            Bold = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _hintButton.AddChild(hintLabel);
        _hintButton.OnPointerClicked += (sender, e) => RequestHint();
        _controlPanel.AddChild(_hintButton);

        _infoPanel!.AddChild(_controlPanel);
    }

    private void ShowUI()
    {
        _mainPanel?.AddToVisualTree();
        UpdateUI();
    }

    private void HandleBoardClick(int row, int col)
    {
        ClearErrorMessage();

        if (_gameState != GameState.InProgress)
        {
            ShowErrorMessage("游戏尚未开始或已结束");
            return;
        }

        if (_currentPlayer != _localPlayerPiece)
        {
            ShowErrorMessage("现在不是您的回合");
            return;
        }

        if (GetPiece(row, col) != PieceType.None)
        {
            ShowErrorMessage("该位置已有棋子");
            return;
        }

        SendPlaceStoneRequest(row, col);
    }

    private void StartGame()
    {
        if (_gameState != GameState.WaitingForPlayers)
        {
            ShowErrorMessage("游戏已经开始或已结束");
            return;
        }

        // 发送开始游戏请求给服务器
        var request = new StartGameRequest { PlayerId = _localPlayerId };
        SendGomokuMessage(GomokuMessageType.StartGame, request);
        
        ClearErrorMessage();
        Game.Logger.LogInformation("🎮 Start game request sent by player {PlayerId}", _localPlayerId);
    }

    private void RestartGame()
    {
        var request = new RestartGameRequest { PlayerId = _localPlayerId };
        SendGomokuMessage(GomokuMessageType.RestartGame, request);

        // 重置本地状态
        _localBoard = new PieceType[BOARD_SIZE * BOARD_SIZE];
        _lastMove = new Position(-1, -1);
        _gameState = GameState.WaitingForPlayers;
        _currentPlayer = PieceType.Black;
        _turnStartTime = DateTime.Now;

        ClearErrorMessage();
        ClearWinningLine();
        ClearLastMoveMarker();
        UpdateUI();
    }

    private void RequestUndo()
    {
        if (_gameState != GameState.InProgress)
        {
            ShowErrorMessage("游戏未进行中，无法悔棋");
            return;
        }

        var request = new UndoRequest { PlayerId = _localPlayerId };
        SendGomokuMessage(GomokuMessageType.Undo, request);
    }

    private void RequestHint()
    {
        if (_gameState != GameState.InProgress)
        {
            ShowErrorMessage("游戏未进行中，无法获取提示");
            return;
        }

        if (_currentPlayer != _localPlayerPiece)
        {
            ShowErrorMessage("现在不是您的回合");
            return;
        }

        var request = new HintRequest { PlayerId = _localPlayerId };
        SendGomokuMessage(GomokuMessageType.Hint, request);
    }

    private void SendPlaceStoneRequest(int row, int col)
    {
        var request = new PlaceStoneRequest { Row = row, Col = col, PlayerId = _localPlayerId };
        SendGomokuMessage(GomokuMessageType.PlaceStone, request);
    }

    private void SendGomokuMessage<T>(GomokuMessageType messageType, T data)
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(data);
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

            var messageBytes = new byte[1 + jsonBytes.Length];
            messageBytes[0] = (byte)messageType;
            Array.Copy(jsonBytes, 0, messageBytes, 1, jsonBytes.Length);

            var customMessage = new ProtoCustomMessage { Message = messageBytes };
            customMessage.SendToServer();

            Game.Logger.LogDebug("📤 Sent {MessageType} message to server", messageType);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error sending Gomoku message {MessageType}", messageType);
            ShowErrorMessage($"发送消息失败: {ex.Message}");
        }
    }

    private static async Task<bool> OnServerMessageReceived(object sender, EventServerMessage eventArgs)
    {
        var messageBytes = eventArgs.Message;
        if (messageBytes.Length == 0) return false;

        try
        {
            var messageType = (GomokuMessageType)messageBytes[0];
            var jsonBytes = messageBytes.AsSpan(1);
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

            switch (messageType)
            {
                case GomokuMessageType.GameStateUpdate:
                    var stateUpdate = JsonSerializer.Deserialize<GameStateUpdateMessage>(json);
                    if (stateUpdate != null)
                    {
                        Instance.HandleGameStateUpdate(stateUpdate);
                    }
                    break;

                case GomokuMessageType.GameOver:
                    var gameOver = JsonSerializer.Deserialize<GameOverMessage>(json);
                    if (gameOver != null)
                    {
                        Instance.HandleGameOver(gameOver);
                    }
                    break;

                case GomokuMessageType.Error:
                    var error = JsonSerializer.Deserialize<ErrorMessage>(json);
                    if (error != null)
                    {
                        Instance.HandleError(error);
                    }
                    break;

                case GomokuMessageType.Hint:
                    var hint = JsonSerializer.Deserialize<HintMessage>(json);
                    if (hint != null)
                    {
                        Instance.HandleHint(hint);
                    }
                    break;
                    
                case GomokuMessageType.PlayerJoined:
                    var playerJoined = JsonSerializer.Deserialize<PlayerJoinedMessage>(json);
                    if (playerJoined != null)
                    {
                        Instance.HandlePlayerJoined(playerJoined);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error processing server message");
            Instance.ShowErrorMessage($"处理服务器消息失败: {ex.Message}");
        }

        return true;
    }

    private void HandleGameStateUpdate(GameStateUpdateMessage stateUpdate)
    {
        _localBoard = stateUpdate.Board;
        _gameState = stateUpdate.State;
        _currentPlayer = stateUpdate.CurrentPlayer;
        _lastMove = stateUpdate.LastMove;
        _turnStartTime = DateTime.Now;

        if (_localPlayerId == stateUpdate.Player1Id)
        {
            _localPlayerPiece = PieceType.Black;
        }
        else if (_localPlayerId == stateUpdate.Player2Id)
        {
            _localPlayerPiece = PieceType.White;
        }

        ClearErrorMessage();
        UpdateUI();
        UpdateLastMoveMarker();

        Game.Logger.LogDebug("📨 Game state updated: {State}, Current: {CurrentPlayer}",
            _gameState, _currentPlayer);
    }

    private void HandleGameOver(GameOverMessage gameOver)
    {
        _gameState = gameOver.FinalState;
        _winningLine = gameOver.WinningLine ?? new List<Position>();

        // 更新比分
        if (gameOver.FinalState == GameState.BlackWins)
        {
            _blackWins++;
        }
        else if (gameOver.FinalState == GameState.WhiteWins)
        {
            _whiteWins++;
        }

        UpdateUI();
        UpdateWinningLine();

        Game.Logger.LogInformation("🎉 Game Over: {State}, Winner: {WinnerId}",
            gameOver.FinalState, gameOver.WinnerId);
    }

    private void HandleError(ErrorMessage error)
    {
        ShowErrorMessage(error.Message);
        Game.Logger.LogWarning("❌ Server error: {Message}", error.Message);
    }

    private void HandleHint(HintMessage hint)
    {
        if (hint.SuggestedMove != null)
        {
            var pos = hint.SuggestedMove.Value;
            ShowErrorMessage($"💡 建议落子位置: ({pos.Row + 1}, {pos.Col + 1})");

            // 高亮建议位置
            if (_boardButtons != null && pos.Row >= 0 && pos.Row < BOARD_SIZE &&
                pos.Col >= 0 && pos.Col < BOARD_SIZE)
            {
                var button = _boardButtons[pos.Row, pos.Col];
                var originalColor = button.Background;

                // 闪烁效果
                button.Background = new SolidColorBrush(Color.Yellow);

                // 延迟恢复原色
                Game.Delay(TimeSpan.FromSeconds(1)).ContinueWith(_ =>
                {
                    button.Background = originalColor;
                });
            }
        }
        else
        {
            ShowErrorMessage("💡 无可用提示");
        }
    }
    
    private void HandlePlayerJoined(PlayerJoinedMessage playerJoined)
    {
        // 更新本地玩家信息
        if (playerJoined.PlayerId == _localPlayerId)
        {
            _localPlayerPiece = playerJoined.AssignedPiece;
            Game.Logger.LogInformation("🎯 Local player assigned as {Piece}", 
                _localPlayerPiece == PieceType.Black ? "Black" : "White");
        }
        
        Game.Logger.LogInformation("👋 Player {PlayerId} joined as {Piece}", 
            playerJoined.PlayerId, playerJoined.AssignedPiece);
        
        // 可以在这里显示玩家加入的通知
        ShowErrorMessage($"👋 玩家 {playerJoined.PlayerId} 已加入游戏");
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateBoardUI();
        UpdateInfoUI();
        UpdateControlUI();
    }

    private void UpdateBoardUI()
    {
        if (_boardButtons == null) return;

        for (int row = 0; row < BOARD_SIZE; row++)
        {
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                var button = _boardButtons[row, col];
                var piece = GetPiece(row, col);

                Color buttonColor = piece switch
                {
                    PieceType.Black => Color.Black,
                    PieceType.White => Color.White,
                    _ => Color.FromArgb(245, 222, 179)
                };

                button.Background = new SolidColorBrush(buttonColor);

                // 添加边框效果
                if (piece != PieceType.None)
                {
                    // 可以在这里添加边框或阴影效果
                }
            }
        }
    }

    private void UpdateInfoUI()
    {
        // 更新当前玩家
        if (_currentPlayerLabel != null)
        {
            string playerText = _currentPlayer == PieceType.Black ? "黑棋" : "白棋";
            string turnText = _currentPlayer == _localPlayerPiece ? "（您的回合）" : "（对手回合）";
            _currentPlayerLabel.Text = $"当前玩家：{playerText} {turnText}";
            _currentPlayerLabel.TextColor = _currentPlayer == PieceType.Black ? Color.Black : Color.Gray;
        }

        // 更新游戏状态
        if (_gameStateLabel != null)
        {
            _gameStateLabel.Text = _gameState switch
            {
                GameState.WaitingForPlayers => "等待玩家加入...",
                GameState.InProgress => _currentPlayer == _localPlayerPiece ? "请落子！" : "等待对手落子...",
                GameState.BlackWins => "🎉 黑棋获胜！",
                GameState.WhiteWins => "🎉 白棋获胜！",
                GameState.Draw => "⚖️ 游戏平局！",
                _ => "未知状态"
            };
        }

        // 更新比分
        if (_scoreLabel != null)
        {
            _scoreLabel.Text = $"🏆 比分\n黑棋：{_blackWins}\n白棋：{_whiteWins}";
        }

        // 更新计时器
        if (_timerLabel != null && _gameState == GameState.InProgress)
        {
            var elapsed = DateTime.Now - _turnStartTime;
            _timerLabel.Text = $"⏱️ {elapsed.Minutes:00}:{elapsed.Seconds:00}";
        }
    }

    private void UpdateControlUI()
    {
        // 在这个框架中Button没有IsEnabled属性
        // 可以通过改变按钮外观或添加逻辑检查来实现类似效果

        if (_startButton != null)
        {
            // 根据游戏状态改变按钮颜色来表示是否可用
            var isEnabled = _gameState == GameState.WaitingForPlayers;
            _startButton.Background = new SolidColorBrush(isEnabled ?
                Color.FromArgb(76, 175, 80) : Color.FromArgb(150, 150, 150));
        }

        if (_undoButton != null)
        {
            var isEnabled = _gameState == GameState.InProgress;
            _undoButton.Background = new SolidColorBrush(isEnabled ?
                Color.FromArgb(96, 125, 139) : Color.FromArgb(150, 150, 150));
        }

        if (_hintButton != null)
        {
            var isEnabled = _gameState == GameState.InProgress && _currentPlayer == _localPlayerPiece;
            _hintButton.Background = new SolidColorBrush(isEnabled ?
                Color.FromArgb(103, 58, 183) : Color.FromArgb(150, 150, 150));
        }
    }

    private void UpdateLastMoveMarker()
    {
        ClearLastMoveMarker();

        if (_lastMove.Row >= 0 && _lastMove.Col >= 0 && _boardButtons != null)
        {
            _lastMoveMarker = new Panel
            {
                Background = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0)),
                Width = BUTTON_SIZE + 4,
                Height = BUTTON_SIZE + 4,
                Position = new UIPosition(
                    BOARD_MARGIN + _lastMove.Col * BUTTON_SPACING - 2,
                    BOARD_MARGIN + _lastMove.Row * BUTTON_SPACING - 2
                ),
                PositionType = UIPositionType.Absolute,
                CornerRadius = (BUTTON_SIZE + 4) / 2
            };

            _boardPanel?.AddChild(_lastMoveMarker);
        }
    }

    private void UpdateWinningLine()
    {
        ClearWinningLine();

        if (_winningLine.Count > 0 && _boardButtons != null)
        {
            foreach (var pos in _winningLine)
            {
                var marker = new Panel
                {
                    Background = new SolidColorBrush(Color.FromArgb(150, 0, 255, 0)),
                    Width = BUTTON_SIZE,
                    Height = BUTTON_SIZE,
                    Position = new UIPosition(
                        BOARD_MARGIN + pos.Col * BUTTON_SPACING,
                        BOARD_MARGIN + pos.Row * BUTTON_SPACING
                    ),
                    PositionType = UIPositionType.Absolute,
                    CornerRadius = BUTTON_SIZE / 2
                };

                _winningLineMarkers.Add(marker);
                _boardPanel?.AddChild(marker);
            }
        }
    }

    private void ClearLastMoveMarker()
    {
        if (_lastMoveMarker != null)
        {
            _lastMoveMarker.RemoveFromParent();
            _lastMoveMarker = null;
        }
    }

    private void ClearWinningLine()
    {
        foreach (var marker in _winningLineMarkers)
        {
            marker.RemoveFromParent();
        }
        _winningLineMarkers.Clear();
    }

    private void ShowErrorMessage(string message)
    {
        _errorMessage = message;
        if (_errorLabel != null)
        {
            _errorLabel.Text = $"❌ {message}";
            _errorLabel.TextColor = Color.Red;
        }

        // 自动清除错误信息
        Game.Delay(TimeSpan.FromSeconds(3)).ContinueWith(_ => ClearErrorMessage());
    }

    private void ClearErrorMessage()
    {
        _errorMessage = "";
        if (_errorLabel != null)
        {
            _errorLabel.Text = "";
        }
    }

    private async Task StartUIUpdateTimer()
    {
        // 启动UI更新计时器
        while (true)
        {
            await Game.Delay(TimeSpan.FromSeconds(1));
            if (_gameState == GameState.InProgress)
            {
                UpdateInfoUI();
            }
        }
    }

    // 消息类型定义
    public class UndoRequest
    {
        public int PlayerId { get; set; }
    }

    public class HintRequest
    {
        public int PlayerId { get; set; }
    }
    
    public class StartGameRequest
    {
        public int PlayerId { get; set; }
    }

    public class ErrorMessage
    {
        public string Message { get; set; } = "";
    }

    public class HintMessage
    {
        public Position? SuggestedMove { get; set; }
    }
}
#endif