using System.Net.WebSockets;

namespace Fisher;

public partial class FormConnect : Form
{
    Button btnConnect = new Button();
    TextBox tbIP = new TextBox();
    TextBox tbPort = new TextBox();
    TextBox tbName = new TextBox();
    private Game _game;
    public FormConnect()
    {
        InitializeComponent();
        
        btnConnect.Top = 300;
        btnConnect.Left = 300;
        btnConnect.Text = "Подключиться";
        btnConnect.Size = new Size(170, 40);
        btnConnect.Click += btnConnect_Click;
        Controls.Add(btnConnect);
        
        tbName.Size = new Size(200, 30);
        tbName.Top = 100;
        tbName.Left = 300;
        tbName.Text = "Player1";
        Controls.Add(tbName);
        
        tbIP.Size = new Size(200, 30);
        tbIP.Top = 150;
        tbIP.Left = 300;
        tbIP.Text = "127.0.0.1";
        Controls.Add(tbIP);
        
        tbPort.Size = new Size(200, 30);
        tbPort.Top = 200;
        tbPort.Left = 300;
        tbPort.Text = "8080";
        Controls.Add(tbPort);
    }

    private void btnConnect_Click(object? sender, EventArgs e)
    {
        _game = new Game();
        _game.ActionConnection += CreateGameForm;
        _game.Connect(tbIP.Text, tbPort.Text, tbName.Text);
    }

    private void CreateGameForm()
    {
        FormGame formGame = new FormGame(_game);
        formGame.Show();
        this.Hide();
    }
}