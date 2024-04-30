using System.ComponentModel;

namespace Fisher;

public partial class FormGame : Form
{
    private Game _game;
    private Button btnReady;
    private Label lInfo;
    private Label lTime;
    private Label lEnemy;
    private Label lSelfScore;
    private Label lEnemyScore;
    private DoubleBufferedPanel gamePanel;
    private Panel fishPanel;
    private int fishWidth = 100;
    private int fishHeight = 50;
    
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            // Включаем двойную буферизацию при создании панели
            DoubleBuffered = true;
        }
    }
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
        
        lEnemy = new Label();
        lEnemy.Text = "";
        lEnemy.Left = 20;
        lEnemy.Top = 20;
        lEnemy.Size = new Size(300, 30);
        Controls.Add(lEnemy);
        
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
        lTime.Size = new Size(100, 30);
        Controls.Add(lTime);
        
        lSelfScore = new Label();
        lSelfScore.Text = "Ваши очки: ";
        lSelfScore.Left = 900;
        lSelfScore.Top = 20;
        lSelfScore.Size = new Size(300, 30);
        Controls.Add(lSelfScore);
        
        lEnemyScore = new Label();
        lEnemyScore.Text = "Очки противника: ";
        lEnemyScore.Left = 1200;
        lEnemyScore.Top = 20;
        lEnemyScore.Size = new Size(300, 30);
        Controls.Add(lEnemyScore);

        gamePanel = new DoubleBufferedPanel();
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
        _game.ActionUpdateEnemyScore += UpdateEnemyScore;
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
        Action lInfoAction = () => lInfo.Text = "Игра началась";
        if (lInfo.InvokeRequired)
            lInfo.Invoke(lInfoAction); 
        else lInfoAction();
        
        Action lTimeAction = () => lTime.Text = "60";
        if (lTime.InvokeRequired)
            lTime.Invoke(lTimeAction); 
        else lTimeAction();
        
        Action lSelfScoreAction = () => lSelfScore.Text = $"Ваши очки: {_game.SelfScore}";
        if (lSelfScore.InvokeRequired)
            lSelfScore.Invoke(lSelfScoreAction); 
        else lSelfScoreAction();
        
        Action lEnemyScoreAction = () => lEnemyScore.Text = $"Очки противника: {_game.EnemyScore}";
        if (lEnemyScore.InvokeRequired)
            lEnemyScore.Invoke(lEnemyScoreAction); 
        else lEnemyScoreAction();
        
        _game.GameProcessStart();
    }

    private void NewFish(int x, int y)
    {
        // gamePanel.SuspendLayout();
        Action lDeleteFishAction = () => gamePanel.Controls.Remove(fishPanel);
        if (gamePanel.InvokeRequired)
            gamePanel.Invoke(lDeleteFishAction); 
        else lDeleteFishAction();
        
        fishPanel = new Panel();
        fishPanel.Size = new Size(fishWidth, fishHeight);
        fishPanel.BackColor = Color.Transparent;
        fishPanel.Left = x;
        fishPanel.Top = y;
        fishPanel.BackgroundImage = Image.FromFile(@"..\..\..\images\fish1.png");
        fishPanel.BackgroundImageLayout = ImageLayout.Stretch;
        fishPanel.Click += FishClick;
        
        Action lAddFishAction = () => gamePanel.Controls.Add(fishPanel);
        if (gamePanel.InvokeRequired)
            gamePanel.Invoke(lAddFishAction); 
        else lAddFishAction();
        // gamePanel.ResumeLayout(); 
        Invalidate();
    }

    private void FishClick(object? sender, EventArgs e)
    {
        _game.SelfScore++;
        
        Action lSelfScoreAction = () => lSelfScore.Text = $"Ваши очки: {_game.SelfScore}";
        if (lSelfScore.InvokeRequired)
            lSelfScore.Invoke(lSelfScoreAction); 
        else lSelfScoreAction();
        
        _game.SendMessageFishClick();
    }

    private void ShowTime(int time)
    {
        Action lAction1 = () => lTime.Text = time.ToString();
        if (lTime.InvokeRequired)
            lTime.Invoke(lAction1); 
        else lAction1();
        // Invalidate();
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

    private void UpdateEnemyScore(int score)
    {
        _game.EnemyScore = score;
        Action lEnemyScoreAction = () => lEnemyScore.Text = $"Очки противника: {_game.EnemyScore}";
        if (lEnemyScore.InvokeRequired)
            lEnemyScore.Invoke(lEnemyScoreAction); 
        else lEnemyScoreAction();
    }
}