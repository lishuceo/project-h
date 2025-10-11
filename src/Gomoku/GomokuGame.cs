using System.Text.Json.Serialization;

using static GameEntry.Gomoku.GomokuGame;

namespace GameEntry.Gomoku;

/// <summary>
/// 五子棋游戏的核心逻辑和数据结构
/// </summary>
public static class GomokuGame
{
    public const int BOARD_SIZE = 15;
    public const int WIN_CONDITION = 5;

    /// <summary>
    /// 棋子类型
    /// </summary>
    public enum PieceType : byte
    {
        None = 0,
        Black = 1,
        White = 2
    }

    /// <summary>
    /// 游戏状态
    /// </summary>
    public enum GameState : byte
    {
        WaitingForPlayers = 0,
        InProgress = 1,
        BlackWins = 2,
        WhiteWins = 3,
        Draw = 4
    }

    /// <summary>
    /// 游戏数据结构
    /// </summary>
    public struct GameData
    {
        public PieceType[] Board { get; set; }
        public GameState State { get; set; }
        public PieceType CurrentPlayer { get; set; }
        public int Player1Id { get; set; }  // 黑棋玩家
        public int Player2Id { get; set; }  // 白棋玩家
        public DateTime GameStartTime { get; set; }
        public int MoveCount { get; set; }
        public Position LastMove { get; set; }

        public GameData()
        {
            Board = new PieceType[GomokuGame.BOARD_SIZE * GomokuGame.BOARD_SIZE];
            State = GameState.WaitingForPlayers;
            CurrentPlayer = PieceType.Black;
            Player1Id = -1;  // 未分配玩家1
            Player2Id = -1;  // 未分配玩家2
            GameStartTime = DateTime.UtcNow;
            MoveCount = 0;
            LastMove = new Position(-1, -1);
        }

        /// <summary>
        /// 获取棋盘位置的棋子
        /// </summary>
        public PieceType GetPiece(int row, int col)
        {
            return row < 0 || row >= GomokuGame.BOARD_SIZE || col < 0 || col >= GomokuGame.BOARD_SIZE
                ? PieceType.None
                : Board[(row * GomokuGame.BOARD_SIZE) + col];
        }

        /// <summary>
        /// 设置棋盘位置的棋子
        /// </summary>
        public void SetPiece(int row, int col, PieceType piece)
        {
            if (row >= 0 && row < GomokuGame.BOARD_SIZE && col >= 0 && col < GomokuGame.BOARD_SIZE)
            {
                Board[(row * GomokuGame.BOARD_SIZE) + col] = piece;
            }
        }
    }

    /// <summary>
    /// 位置结构
    /// </summary>
    public struct Position
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public Position(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public bool IsValid => Row >= 0 && Row < GomokuGame.BOARD_SIZE && Col >= 0 && Col < GomokuGame.BOARD_SIZE;
    }
}

/// <summary>
/// 网络消息类型
/// </summary>
public enum GomokuMessageType : byte
{
    PlaceStone = 1,
    GameStateUpdate = 2,
    GameOver = 3,
    RestartGame = 4,
    PlayerJoined = 5,
    Error = 6,
    Undo = 7,
    Hint = 8,
    StartGame = 9
}

/// <summary>
/// 落子请求消息
/// </summary>
public class PlaceStoneRequest
{
    public int Row { get; set; }
    public int Col { get; set; }
    public int PlayerId { get; set; }
}

/// <summary>
/// 游戏状态更新消息
/// </summary>
public class GameStateUpdateMessage
{
    public GomokuGame.PieceType[] Board { get; set; } = new GomokuGame.PieceType[GomokuGame.BOARD_SIZE * GomokuGame.BOARD_SIZE];
    public GomokuGame.GameState State { get; set; }
    public GomokuGame.PieceType CurrentPlayer { get; set; }
    public int Player1Id { get; set; }
    public int Player2Id { get; set; }
    public int MoveCount { get; set; }
    public GomokuGame.Position LastMove { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 游戏结束消息
/// </summary>
public class GameOverMessage
{
    public GomokuGame.GameState FinalState { get; set; }
    public int WinnerId { get; set; }
    public List<GomokuGame.Position> WinningLine { get; set; } = [];
    public int TotalMoves { get; set; }
    public TimeSpan GameDuration { get; set; }
}

/// <summary>
/// 错误消息
/// </summary>
public class ErrorMessage
{
    public string Message { get; set; } = string.Empty;
    public int PlayerId { get; set; }
}

/// <summary>
/// 重新开始游戏请求
/// </summary>
public class RestartGameRequest
{
    public int PlayerId { get; set; }
}

/// <summary>
/// 开始游戏请求
/// </summary>
public class StartGameRequest
{
    public int PlayerId { get; set; }
}

/// <summary>
/// 玩家加入消息
/// </summary>
public class PlayerJoinedMessage
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public GomokuGame.PieceType AssignedPiece { get; set; }
}

// JSON序列化上下文
[JsonSerializable(typeof(PlaceStoneRequest))]
[JsonSerializable(typeof(GameStateUpdateMessage))]
[JsonSerializable(typeof(GameOverMessage))]
[JsonSerializable(typeof(ErrorMessage))]
[JsonSerializable(typeof(RestartGameRequest))]
[JsonSerializable(typeof(StartGameRequest))]
[JsonSerializable(typeof(PlayerJoinedMessage))]
public partial class GomokuJsonContext : JsonSerializerContext { }

#if SERVER
/// <summary>
/// 服务器端五子棋游戏管理器
/// </summary>
public class GomokuServerManager : IGameClass
{
    private static readonly object _lock = new();
    private GomokuGame.GameData _gameData;
    private readonly Dictionary<int, Player> _players = [];
    private readonly HashSet<int> _playersReady = []; // 准备开始的玩家列表
    private static GomokuServerManager? instance;

    // 触发器字段
    private static Trigger<EventClientMessage>? clientMessageTrigger;
    private static Trigger<EventPlayerUserConnected>? playerConnectedTrigger;
    private static Trigger<EventPlayerUserDisconnected>? playerDisconnectedTrigger;

    public static GomokuServerManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    instance ??= new GomokuServerManager();
                }
            }
            return instance;
        }
    }

    public static void OnRegisterGameClass()
    {
        Game.OnGameTriggerInitialization += OnGameTriggerInitialization;
    }

    private static void OnGameTriggerInitialization()
    {
        if (Game.GameModeLink != ScopeData.GameMode.Gomoku)
        {
            return;
        }

        // 注册客户端消息监听器
        clientMessageTrigger = new Trigger<EventClientMessage>(OnClientMessageReceived, true);
        clientMessageTrigger.Register(Game.Instance);

        // 注册玩家连接事件监听器
        playerConnectedTrigger = new Trigger<EventPlayerUserConnected>(OnPlayerConnected, true);
        playerConnectedTrigger.Register(Game.Instance);

        playerDisconnectedTrigger = new Trigger<EventPlayerUserDisconnected>(OnPlayerDisconnected, true);
        playerDisconnectedTrigger.Register(Game.Instance);

        Instance.InitializeGame();
        Game.Logger.LogInformation("🎮 Gomoku Server Manager initialized");
    }

    private void InitializeGame()
    {
        _gameData = new GomokuGame.GameData();
        _players.Clear();
        _playersReady.Clear();
        Game.Logger.LogInformation("🎲 Gomoku game initialized - waiting for players");

        // 发送初始游戏状态
        _ = BroadcastGameState();
    }

    private static async Task<bool> OnClientMessageReceived(object sender, EventClientMessage eventArgs)
    {
        try
        {
            var messageBytes = eventArgs.Message;
            if (messageBytes.Length == 0)
            {
                return false;
            }

            var messageType = (GomokuMessageType)messageBytes[0];
            var jsonBytes = messageBytes.AsSpan(1);
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

            switch (messageType)
            {
                case GomokuMessageType.PlaceStone:
                    var placeStoneRequest = JsonSerializer.Deserialize<PlaceStoneRequest>(json);
                    if (placeStoneRequest != null)
                    {
                        Instance.HandlePlaceStone(eventArgs.Player, placeStoneRequest);
                    }
                    break;

                case GomokuMessageType.RestartGame:
                    var restartRequest = JsonSerializer.Deserialize<RestartGameRequest>(json);
                    if (restartRequest != null)
                    {
                        Instance.HandleRestartGame(eventArgs.Player, restartRequest);
                    }
                    break;

                case GomokuMessageType.StartGame:
                    var startRequest = JsonSerializer.Deserialize<StartGameRequest>(json);
                    if (startRequest != null)
                    {
                        Instance.HandleStartGame(eventArgs.Player, startRequest);
                    }
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error processing Gomoku client message");
            return false;
        }
    }

    private void HandlePlaceStone(Player player, PlaceStoneRequest request)
    {
        try
        {
            // 验证游戏状态
            if (_gameData.State != GameState.InProgress)
            {
                SendErrorMessage(player, "游戏尚未开始或已结束");
                return;
            }

            // 验证玩家身份
            var playerPiece = GetPlayerPiece(player.Id);
            if (playerPiece == PieceType.None)
            {
                SendErrorMessage(player, "您不是游戏参与者");
                return;
            }

            // 验证是否是该玩家的回合
            if (_gameData.CurrentPlayer != playerPiece)
            {
                SendErrorMessage(player, "不是您的回合");
                return;
            }

            // 验证位置有效性
            var position = new Position(request.Row, request.Col);
            if (!position.IsValid)
            {
                SendErrorMessage(player, "无效的位置");
                return;
            }

            // 验证位置是否为空
            if (_gameData.GetPiece(request.Row, request.Col) != PieceType.None)
            {
                SendErrorMessage(player, "该位置已有棋子");
                return;
            }

            // 落子
            _gameData.SetPiece(request.Row, request.Col, playerPiece);
            _gameData.LastMove = position;
            _gameData.MoveCount++;

            // 检查获胜条件
            if (CheckWin(request.Row, request.Col, playerPiece))
            {
                _gameData.State = playerPiece == PieceType.Black ? GameState.BlackWins : GameState.WhiteWins;
                var winningLine = GetWinningLine(request.Row, request.Col, playerPiece);
                _ = SendGameOverMessage(player.Id, winningLine);
            }
            else if (_gameData.MoveCount >= GomokuGame.BOARD_SIZE * GomokuGame.BOARD_SIZE)
            {
                _gameData.State = GameState.Draw;
                _ = SendGameOverMessage(-1, []);
            }
            else
            {
                // 切换玩家
                _gameData.CurrentPlayer = _gameData.CurrentPlayer == PieceType.Black ? PieceType.White : PieceType.Black;
            }

            // 广播游戏状态更新
            _ = BroadcastGameState();

            Game.Logger.LogInformation("🎯 Player {PlayerId} placed {Piece} at ({Row}, {Col})",
                player.Id, playerPiece, request.Row, request.Col);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error handling place stone request");
            SendErrorMessage(player, "处理落子请求时发生错误");
        }
    }

    private void HandleRestartGame(Player player, RestartGameRequest request)
    {
        try
        {
            // 验证玩家身份
            if (player.Id != _gameData.Player1Id && player.Id != _gameData.Player2Id)
            {
                SendErrorMessage(player, "只有游戏参与者才能重新开始游戏");
                return;
            }

            // 重新初始化游戏
            InitializeGame();

            Game.Logger.LogInformation("🔄 Game restarted by player {PlayerId}", player.Id);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error handling restart game request");
            SendErrorMessage(player, "重新开始游戏时发生错误");
        }
    }

    private void HandleStartGame(Player player, StartGameRequest request)
    {
        try
        {
            if (_gameData.State != GameState.WaitingForPlayers)
            {
                SendErrorMessage(player, "游戏已经开始或已结束");
                return;
            }

            if (_players.Count < 2)
            {
                SendErrorMessage(player, "需要至少2个玩家才能开始游戏");
                return;
            }

            // 检查玩家是否是游戏参与者
            if (player.Id != _gameData.Player1Id && player.Id != _gameData.Player2Id)
            {
                SendErrorMessage(player, "只有游戏参与者才能开始游戏");
                return;
            }

            // 单人确认模式：任何一个玩家点击就开始
            // 如果要启用双人确认模式，请将下面的注释取消，并注释掉StartGame()调用

            /*
            // 双人确认模式：需要两个玩家都点击才开始
            _playersReady.Add(player.Id);
            
            var player1Ready = _playersReady.Contains(_gameData.Player1Id);
            var player2Ready = _playersReady.Contains(_gameData.Player2Id);
            
            if (player1Ready && player2Ready)
            {
                // 两个玩家都准备好了，开始游戏
                StartGame();
                Game.Logger.LogInformation("🎮 Game started - both players ready");
            }
            else
            {
                // 还有玩家没准备好，发送通知
                var waitingFor = player1Ready ? "白棋玩家" : "黑棋玩家";
                var message = $"🎯 玩家 {player.Id} 已准备，等待{waitingFor}确认...";
                
                // 通知所有玩家当前状态
                var errorMsg = new ErrorMessage
                {
                    Message = message,
                    PlayerId = -1 // -1 表示广播给所有人
                };
                _ = SendGomokuMessage(GomokuMessageType.Error, errorMsg);
                
                Game.Logger.LogInformation("🎮 Player {PlayerId} ready, waiting for other player", player.Id);
            }
            */

            // 单人确认模式（当前启用）
            StartGame();
            Game.Logger.LogInformation("🎮 Game manually started by player {PlayerId}", player.Id);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error handling start game request");
            SendErrorMessage(player, "开始游戏时发生错误");
        }
    }

    private static async Task<bool> OnPlayerConnected(object sender, EventPlayerUserConnected eventArgs)
    {
        try
        {
            var player = eventArgs.Player;
            Instance._players[player.Id] = player;

            // 分配玩家角色
            if (Instance._gameData.Player1Id == -1)
            {
                // 分配第一个连接的玩家为黑棋
                Instance._gameData.Player1Id = player.Id;
                Game.Logger.LogInformation("🎯 Player {PlayerId} assigned as Player 1 (Black)", player.Id);
            }
            else if (Instance._gameData.Player2Id == -1 && player.Id != Instance._gameData.Player1Id)
            {
                // 分配第二个连接的玩家为白棋
                Instance._gameData.Player2Id = player.Id;
                Game.Logger.LogInformation("🎯 Player {PlayerId} assigned as Player 2 (White)", player.Id);
            }

            Game.Logger.LogInformation("👋 Player {PlayerId} connected. Total players: {Count}",
                player.Id, Instance._players.Count);

            // 发送玩家加入消息
            var joinMessage = new PlayerJoinedMessage
            {
                PlayerId = player.Id,
                PlayerName = $"Player{player.Id}",
                AssignedPiece = Instance.GetPlayerPiece(player.Id)
            };
            await Instance.SendGomokuMessage(GomokuMessageType.PlayerJoined, joinMessage);

            // 广播更新的游戏状态
            _ = Instance.BroadcastGameState();

            // 如果有足够玩家，可以考虑自动开始（可选）
            if (Instance._players.Count >= 2 && Instance._gameData.State == GameState.WaitingForPlayers)
            {
                Game.Logger.LogInformation("🎮 Two players connected, game can now be started");
                // 可以选择自动开始或等待手动开始
                // Instance.StartGame(); // 取消注释以启用自动开始
            }

            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error handling player connected event");
            return false;
        }
    }

    private static async Task<bool> OnPlayerDisconnected(object sender, EventPlayerUserDisconnected eventArgs)
    {
        try
        {
            var player = eventArgs.Player;
            _ = Instance._players.Remove(player.Id);
            _ = Instance._playersReady.Remove(player.Id); // 清除准备状态

            Game.Logger.LogInformation("👋 Player {PlayerId} disconnected. Total players: {Count}",
                player.Id, Instance._players.Count);

            // 如果游戏正在进行中且关键玩家断开，可以暂停游戏
            if (Instance._gameData.State == GameState.InProgress &&
                (player.Id == Instance._gameData.Player1Id || player.Id == Instance._gameData.Player2Id))
            {
                Game.Logger.LogWarning("⚠️ Key player disconnected during game");
                // 可以选择暂停游戏等待重连，或结束游戏
                // Instance._gameData.State = GameState.WaitingForPlayers;
                // _ = Instance.BroadcastGameState();
            }

            return true;
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error handling player disconnected event");
            return false;
        }
    }

    private PieceType GetPlayerPiece(int playerId)
    {
        if (playerId == _gameData.Player1Id)
        {
            return PieceType.Black;
        }

        return playerId == _gameData.Player2Id ? PieceType.White : PieceType.None;
    }

    private bool CheckWin(int row, int col, PieceType piece)
    {
        // 检查四个方向：水平、垂直、斜线
        int[][] directions =
        {
            [0, 1],   // 水平
            [1, 0],   // 垂直
            [1, 1],   // 主对角线
            [1, -1]   // 反对角线
        };

        foreach (var direction in directions)
        {
            var count = 1; // 包含当前棋子

            // 正方向检查
            for (var i = 1; i < GomokuGame.WIN_CONDITION; i++)
            {
                var newRow = row + (direction[0] * i);
                var newCol = col + (direction[1] * i);

                if (newRow < 0 || newRow >= GomokuGame.BOARD_SIZE || newCol < 0 || newCol >= GomokuGame.BOARD_SIZE)
                {
                    break;
                }

                if (_gameData.GetPiece(newRow, newCol) != piece)
                {
                    break;
                }

                count++;
            }

            // 反方向检查
            for (var i = 1; i < GomokuGame.WIN_CONDITION; i++)
            {
                var newRow = row - (direction[0] * i);
                var newCol = col - (direction[1] * i);

                if (newRow < 0 || newRow >= GomokuGame.BOARD_SIZE || newCol < 0 || newCol >= GomokuGame.BOARD_SIZE)
                {
                    break;
                }

                if (_gameData.GetPiece(newRow, newCol) != piece)
                {
                    break;
                }

                count++;
            }

            if (count >= GomokuGame.WIN_CONDITION)
            {
                return true;
            }
        }

        return false;
    }

    private List<Position> GetWinningLine(int row, int col, PieceType piece)
    {
        int[][] directions =
        {
            new[] {0, 1},   // 水平
            new[] {1, 0},   // 垂直
            new[] {1, 1},   // 主对角线
            new[] {1, -1}   // 反对角线
        };

        foreach (var direction in directions)
        {
            var positions = new List<Position> { new(row, col) };

            // 正方向检查
            for (var i = 1; i < GomokuGame.WIN_CONDITION; i++)
            {
                var newRow = row + (direction[0] * i);
                var newCol = col + (direction[1] * i);

                if (newRow < 0 || newRow >= GomokuGame.BOARD_SIZE || newCol < 0 || newCol >= GomokuGame.BOARD_SIZE)
                {
                    break;
                }

                if (_gameData.GetPiece(newRow, newCol) != piece)
                {
                    break;
                }

                positions.Add(new Position(newRow, newCol));
            }

            // 反方向检查
            for (var i = 1; i < GomokuGame.WIN_CONDITION; i++)
            {
                var newRow = row - (direction[0] * i);
                var newCol = col - (direction[1] * i);

                if (newRow < 0 || newRow >= GomokuGame.BOARD_SIZE || newCol < 0 || newCol >= GomokuGame.BOARD_SIZE)
                {
                    break;
                }

                if (_gameData.GetPiece(newRow, newCol) != piece)
                {
                    break;
                }

                positions.Insert(0, new Position(newRow, newCol));
            }

            if (positions.Count >= GomokuGame.WIN_CONDITION)
            {
                return positions;
            }
        }

        return [];
    }

    private async Task BroadcastGameState()
    {
        try
        {
            var stateMessage = new GameStateUpdateMessage
            {
                Board = _gameData.Board,
                State = _gameData.State,
                CurrentPlayer = _gameData.CurrentPlayer,
                Player1Id = _gameData.Player1Id,
                Player2Id = _gameData.Player2Id,
                MoveCount = _gameData.MoveCount,
                LastMove = _gameData.LastMove
            };

            await SendGomokuMessage(GomokuMessageType.GameStateUpdate, stateMessage);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error broadcasting game state");
        }
    }

    private async Task SendGameOverMessage(int winnerId, List<Position> winningLine)
    {
        try
        {
            var gameOverMessage = new GameOverMessage
            {
                FinalState = _gameData.State,
                WinnerId = winnerId,
                WinningLine = winningLine,
                TotalMoves = _gameData.MoveCount,
                GameDuration = DateTime.UtcNow - _gameData.GameStartTime
            };

            await SendGomokuMessage(GomokuMessageType.GameOver, gameOverMessage);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error sending game over message");
        }
    }

    private void SendErrorMessage(Player player, string message)
    {
        try
        {
            var errorMessage = new ErrorMessage
            {
                Message = message,
                PlayerId = player.Id
            };

            _ = SendGomokuMessage(GomokuMessageType.Error, errorMessage, player);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error sending error message");
        }
    }

    private async Task SendGomokuMessage<T>(GomokuMessageType messageType, T data, Player? specificPlayer = null)
    {
        try
        {
            var jsonData = JsonSerializer.Serialize(data);
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

            var messageBytes = new byte[1 + jsonBytes.Length];
            messageBytes[0] = (byte)messageType;
            Array.Copy(jsonBytes, 0, messageBytes, 1, jsonBytes.Length);

            var customMessage = new ProtoCustomMessage { Message = messageBytes };

            if (specificPlayer != null)
            {
                _ = customMessage.SendTo(specificPlayer);
            }
            else
            {
                customMessage.Broadcast();
            }

            Game.Logger.LogDebug("📤 Sent {MessageType} message", messageType);
        }
        catch (Exception ex)
        {
            Game.Logger.LogError(ex, "Error sending Gomoku message {MessageType}", messageType);
        }
    }

    public void StartGame()
    {
        if (_gameData.State == GameState.WaitingForPlayers)
        {
            _gameData.State = GameState.InProgress;
            _gameData.GameStartTime = DateTime.UtcNow;
            _ = BroadcastGameState();
            Game.Logger.LogInformation("🎮 Gomoku game started");
        }
    }
}
#endif