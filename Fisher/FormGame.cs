using System.ComponentModel;

namespace Fisher;

public partial class FormGame : Form
{
    private Game _game;
    private Button btnReady;
    private Label lInfo;
    public FormGame(Game game)
    {
        InitializeComponent();
        Size = new Size(1800, 1000);
        btnReady = new Button();
        btnReady.Top = 600;
        btnReady.Left = 490;
        btnReady.Size = new Size(100, 40);
        btnReady.Text = "Готов";
        btnReady.Click += btnReady_Click;
        Controls.Add(btnReady);
        btnReady.Hide();

        lInfo = new Label();
        lInfo.Text = "Ожидание оппонента...";
        lInfo.Left = 300;
        lInfo.Top = 100;
        lInfo.Size = new Size(400, 30);
        Controls.Add(lInfo);
        
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
    }
}