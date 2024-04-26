using System.ComponentModel;

namespace Fisher;

public partial class FormGame : Form
{
    private Game _game;
    private Button btnReady;
    private Label lInfo;
    private Label lEnemy;
    private Panel gamePanel;
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
        // string currentDirectory = Directory.GetCurrentDirectory();
        // string imagePath = Path.Combine(currentDirectory, @"images\lake2.jpg");
        gamePanel.BackgroundImage = Image.FromFile(@"..\..\..\images\lake2.jpg");
        gamePanel.BackgroundImageLayout = ImageLayout.Stretch;
        Controls.Add(gamePanel);
        
        _game = game;
        Closing += (sender, args) => { Application.Exit(); };
        _game.ActionEnemyJoin += EnemyJoin;
        _game.ActionGameStart += StartGame;
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
        Action lAction = () => lInfo.Text = "Игра началась";
        if (lInfo.InvokeRequired)
            lInfo.Invoke(lAction); 
        else lAction();
        _game.GameProcessStart(gamePanel.Width, gamePanel.Height);
    }

    private void NewFish(int x, int y)
    {
        
    }
}