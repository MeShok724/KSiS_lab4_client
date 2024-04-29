using System.Net.WebSockets;
using System.Text;
using Thread = System.Threading.Thread;
using System.Text.Json;
using System.Text.Json.Serialization;
using Timer = System.Threading.Timer;

namespace Fisher;

public class Game
{
    public delegate void MessageDelegate(GameMessage message);
    public delegate void EndGameDelegate(EndGameMessage message);
    public delegate void NewFishDelegate(int x, int y);
    public delegate void TimerDelegate(int time);
    public delegate void EmptyDelegate();

    public event MessageDelegate? ActionMessageCome;
    public event MessageDelegate? ActionEnemyJoin;
    public event EmptyDelegate? ActionConnection;
    public event EmptyDelegate? ActionGameStart;
    public event EndGameDelegate? ActionEndGame;
    public event NewFishDelegate? ActionNewFish;
    public event TimerDelegate? ActionTimerTick;
    
    private ClientWebSocket? _client;
    
    public const string messageConnected = "Connected";
    public const string messageStart = "Start";
    public const string messageEndGame = "End";
    public const string messageEnemy = "Enemy";
    public const string messageEmpty = "Empty";
    public const string messageUpdate = "Update";
    public const string messageNewFish = "Fish";
    private Thread _thread;
    private Thread receivingThread;
    
    public const string Ready = "Ready";
    public int EnemyScore;
    public int SelfScore;

    public volatile int remainingTime;
    public Timer timer;
    public string SelfName;
    public string EnemyName;

    public class GameMessage
    {
        public string Command { get; set; }
        public string Name { get; set; }
        public int Scores { get; set; }
        public int FishX { get; set; }
        public int FishY { get; set; }
        public GameMessage(){}
        public GameMessage(string command)
        {
            Command = command;
        }
        public GameMessage(string command, string name, int scores)
        {
            Command = command;
            Name = name;
            Scores = scores;
        }
    }

    public class EndGameMessage
    {
        public bool isYouWin;
        public bool isCorrect;
        public string winnerName;
        public bool isNoWinner;
        public int winnerScore;
        public int loserScore;
    }
    public async void Connect(string ip, string port)
    {
        try
        {
            _client = new ClientWebSocket();
            Uri serverUri = new Uri($"ws://{ip}:{port}/ws");
            await _client.ConnectAsync(serverUri, CancellationToken.None);
            StartReceiving();
            ActionConnection();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    public async Task SendMessage(GameMessage message)
    {
        Console.WriteLine("Sending message: " + message);
        string json = JsonSerializer.Serialize(message);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        await _client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public void SendMessageFishClick()
    {
        GameMessage message = new GameMessage("Fish");
        SendMessage(message);
    }

    private async void MessageReceiver()
    {
        while (_client.State == WebSocketState.Open)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result = await _client.ReceiveAsync(buffer, CancellationToken.None);

            string message = Encoding.UTF8.GetString(buffer.Array != null ? buffer.Array : Array.Empty<byte>(), 0, result.Count);
            GameMessage gameMessage = JsonSerializer.Deserialize<GameMessage>(message);
            ActionMessageCome(gameMessage);
            // ActionMessageCome!=null?ActionMessageCome(message):;
            Console.WriteLine("Received message: " + message);
        }
        
         await _client.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "", CancellationToken.None);
    }

    private void StartReceiving()
    {
        ActionMessageCome += ActionMessageHandler;
        receivingThread = new Thread(MessageReceiver);
        receivingThread.Start();
    }

    private void StopClient()
    {
        _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Method StopClient", CancellationToken.None);
    }

    private void ActionMessageHandler(GameMessage message)
    {
        switch (message.Command)
        {
            case messageStart:
                Console.WriteLine("Game is starting");
                ActionGameStart();
                break;
            case messageEndGame:
                Console.WriteLine("Game is over");
                CreateGameResult(false);
                StopClient();
                break;
            case messageConnected:
                Console.WriteLine("Tou are connected");
                break;
            case messageEnemy:
                ActionEnemyJoin(message);
                break;
            case messageUpdate:
                EnemyScore = message.Scores;
                break;
            case messageNewFish:
                ActionNewFish(message.FishX, message.FishY);
                break;
        }
    }

    public void GameProcessStart()
    {
        _thread = new Thread(() => GameProcessThread());
        _thread.Start();
    }

    private void CreateGameResult(bool isCorrect = true)
    {
        if (!isCorrect)
        {
            EndGameMessage message = new EndGameMessage();
            message.isCorrect = false;
            ActionEndGame(message);
        }
        if (SelfScore > EnemyScore)
        {
            EndGameMessage message = new EndGameMessage();
            message.isYouWin = true;
            message.isNoWinner = false;
            message.winnerScore = SelfScore;
            message.loserScore = EnemyScore;
            message.winnerName = SelfName;
            message.isCorrect = false;
            ActionEndGame(message);
        } else if (SelfScore < EnemyScore)
        {
            EndGameMessage message = new EndGameMessage();
            message.isYouWin = false;
            message.isNoWinner = false;
            message.winnerScore = EnemyScore;
            message.loserScore = SelfScore;
            message.winnerName = EnemyName;
            message.isCorrect = false;
            ActionEndGame(message);
        }
        else
        {
            EndGameMessage message = new EndGameMessage();
            message.isYouWin = false;
            message.isNoWinner = true;
            message.winnerScore = EnemyScore;
            message.loserScore = SelfScore;
            message.winnerName = EnemyName;
            message.isCorrect = false;
            ActionEndGame(message);
        }
    }

    private void GameProcessThread()
    {
        remainingTime = 60; // Время в секундах
        timer = new Timer(TimerCallback, null, 0, 1000); // Запуск таймера с интервалом 1 секунда (1000 миллисекунд)
    }
    private void TimerCallback(object state)
    {
        remainingTime--;

        if (remainingTime <= 0)
        {
            timer.Dispose();
            CreateGameResult();
        }
        else
        {
            ActionTimerTick(remainingTime);
        }
    }
}