using System.ComponentModel;

namespace Fisher;

public partial class FormGame : Form
{
    private Game _game;
    private Button btnReady;
    private Label lInfo;
    private Label lTime;
    private Label lEnemy;
    private Panel gamePanel;
    private Panel fishPanel;
    private int fishWidth = 100;
    private int fishHeight = 50;
    public FormGame(Game game)
    {
        InitializeComponent();
        Size = new Size(1800, 1000);
        btnReady = new Button();
        btnReady.Top = 20;
        btnReady.Left = 770;
        btnReady.Size = new Size(100, 40);
        btnReady.Text = "Готов";
        btnReady.Click += btnReady_Click;
        Controls.Add(btnReady);
        btnReady.Hide();

        lInfo = new Label();
        lInfo.Text = "Ожидание оппонента...";
        lInfo.Left = 350;
        lInfo.Top = 20;
        lInfo.Size = new Size(400, 30);
        Controls.Add(lInfo);
        
        lTime = new Label();
        lTime.Text = "Время";
        lTime.Left = 800;
        lTime.Top = 20;
        lTime.Size = new Size(200, 30);
        Controls.Add(lTime);
        
        lEnemy = new Label();
        lEnemy.Text = "";
        lEnemy.Left = 20;
        lEnemy.Top = 20;
        lEnemy.Size = new Size(300, 30);
        Controls.Add(lEnemy);

        gamePanel = new Panel();
        gamePanel.Top = 70;
        gamePanel.Left = 10;
        gamePanel.Size = new Size(ClientSize.Width-20, ClientSize.Height - 80);
        int maxX = gamePanel.Width - fishWidth;
        int maxY = gamePanel.Height - fishHeight;
        // string currentDirectory = Directory.GetCurrentDirectory();
        // string imagePath = Path.Combine(currentDirectory, @"images\lake2.jpg");
        gamePanel.BackgroundImage = Image.FromFile(@"..\..\..\images\lake2.jpg");
        gamePanel.BackgroundImageLayout = ImageLayout.Stretch;
        Controls.Add(gamePanel);
        
        _game = game;
        Closing += (sender, args) => { Application.Exit(); };
        _game.ActionEnemyJoin += EnemyJoin;
        _game.ActionGameStart += StartGame;
        _game.ActionNewFish += NewFish;
        _game.ActionTimerTick += ShowTime;
        _game.ActionEndGame += GameOver;
    }

    private void EnemyJoin(Game.GameMessage gameMessage)
    {
        
        Action lAction = () => lInfo.Text = "Нажмите готов для начала игры";
        if (lInfo.InvokeRequired)
            lInfo.Invoke(lAction); 
        else lAction();
        Action lAction1 = () => lEnemy.Text = $"Ваш противник: {gameMessage.Name}\n";
        if (lEnemy.InvokeRequired)
            lEnemy.Invoke(lAction1); 
        else lAction1();
        if (btnReady.InvokeRequired)
            btnReady.Invoke(() => btnReady.Show());
        else btnReady.Show();
    }

    private async void btnReady_Click(object? sender, EventArgs e)
    {
        await _game.SendMessage(new Game.GameMessage(Game.Ready));
        btnReady.Hide();
        lInfo.Text = "Ожидание готовности оппонента...";
    }

    private void StartGame()
    {
        _game.EnemyScore = 0;
        _game.SelfScore = 0;
        Action lAction = () => lInfo.Text = "Игра началась";
        if (lInfo.InvokeRequired)
            lInfo.Invoke(lAction); 
        else lAction();
        
        Action lAction1 = () => lTime.Text = "60";
        if (lTime.InvokeRequired)
            lTime.Invoke(lAction1); 
        else lAction1();
        
        _game.GameProcessStart();
    }

    private void NewFish(int x, int y)
    {
        Controls.Remove(fishPanel);
        fishPanel = new Panel();
        fishPanel.Size = new Size(fishWidth, fishHeight);
        fishPanel.Left = x;
        fishPanel.Top = y;
        fishPanel.BackgroundImage = Image.FromFile(@"..\..\..\images\fish1.png");
        fishPanel.BackgroundImageLayout = ImageLayout.Stretch;
        fishPanel.Click += FishClick;
        
        Action lAction = () => gamePanel.Controls.Add(fishPanel);
        if (gamePanel.InvokeRequired)
            gamePanel.Invoke(lAction); 
        else lAction();
        
        Invalidate();
    }

    private void FishClick(object? sender, EventArgs e)
    {
        _game.SelfScore++;
        _game.SendMessageFishClick();
    }

    private void ShowTime(int time)
    {
        Action lAction1 = () => lTime.Text = time.ToString();
        if (lTime.InvokeRequired)
            lTime.Invoke(lAction1); 
        else lAction1();
        Invalidate();
    }

    private void GameOver(Game.EndGameMessage message)
    {
        if (!message.isCorrect)
        {
            MessageBox.Show("Возникла ошибка в соединении, ничья");
        }
        else if (message.isNoWinner)
        {
            MessageBox.Show($"Ничья, оба игрока набрали по {message.winnerScore} очков");
        }
        else if (message.isYouWin)
        {
            MessageBox.Show($"Вы выиграли, набрав {message.winnerScore} очков, ваш противник набрал {message.loserScore} очков");
        }
        else
        {
            MessageBox.Show($"Вы проиграли, ваш противник {message.winnerName} набрал {message.winnerScore} очков, вы набрали {message.loserScore} очков");
        }
        
    }
}