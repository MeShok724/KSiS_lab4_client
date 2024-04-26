using System.Net.WebSockets;
using System.Text;
using Thread = System.Threading.Thread;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fisher;

public class Game
{
    public delegate void MessageDelegate(GameMessage message);
    public delegate void EmptyDelegate();

    public event MessageDelegate? ActionMessageCome;
    public event MessageDelegate? ActionEnemyJoin;
    public event EmptyDelegate? ActionConnection;
    public event EmptyDelegate? ActionGameStart;
    public event MessageDelegate? ActionEndGame;
    
    private ClientWebSocket? _client;
    
    public const string Connected = "Connected";
    public const string Start = "Start";
    public const string Lose = "Lose";
    public const string Win = "Win";
    public const string NoWinner = "No winner";
    public const string EndGame = "End";
    public const string Enemy = "Enemy";
    
    public const string Ready = "Ready";

    public class GameMessage
    {
        public string Command { get; set; }
        public string OpponentName { get; set; }
        public int OpponentScores { get; set; }
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
        string json = JsonSerializer.Serialize(message);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        await _client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async void MessageReceiver()
    {
        while (_client.State == WebSocketState.Open)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result = await _client.ReceiveAsync(buffer, CancellationToken.None);

            string message = Encoding.UTF8.GetString(buffer.Array != null ? buffer.Array : Array.Empty<byte>(), 0, result.Count);
            ActionMessageCome(message);
            // ActionMessageCome!=null?ActionMessageCome(message):;
            Console.WriteLine("Received message: " + message);
        }
        
         await _client.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "", CancellationToken.None);
    }

    private void StartReceiving()
    {
        ActionMessageCome += ActionMessageHandler;
        Thread thread = new Thread(MessageReceiver);
        thread.Start();
    }

    private void StopClient()
    {
        _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Method StopClient", CancellationToken.None);
    }

    private void ActionMessageHandler(GameMessage message)
    {
        switch (message.Command)
        {
            case Start:
                Console.WriteLine("Game is starting");
                break;
            case EndGame:
                Console.WriteLine("Game is over");
                ActionEndGame(NoWinner);
                StopClient();
                break;
            case Win:
                Console.WriteLine("You win");
                ActionEndGame(Win);
                StopClient();
                break;
            case Lose:
                Console.WriteLine("Tou lose");
                ActionEndGame(Lose);
                StopClient();
                break;
            case Connected:
                Console.WriteLine("Tou are connected");
                break;
            case Enemy:
                // Console.WriteLine("Tou are connected");
                ActionEnemyJoin(string.Empty);
                break;
        }
    }
}