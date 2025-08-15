using Godot;

public partial class GameOver : Control {
    private Label _reasonLabel;
    private Button _playAgainButton;
    private Button _returnToMenuButton;

    static private PackedScene _gameOverScene = GD.Load<PackedScene>("res://scenes/ui/game_over.tscn");
    static private PackedScene _gameScene = GD.Load<PackedScene>("res://scenes/game.tscn");
    static private PackedScene _menuScene = GD.Load<PackedScene>("res://scenes/main_menu.tscn");

    public override void _Ready() {
        _reasonLabel = GetNode<Label>("BackPanel/Reason");
        _playAgainButton = GetNode<Button>("BackPanel/PlayAgain");
        _returnToMenuButton = GetNode<Button>("BackPanel/ReturnToMenu");

        _playAgainButton.Pressed += OnPlayAgainPressed;
        _returnToMenuButton.Pressed += OnReturnToMenuPressed;
    }

    static public void TriggerGameOver(Node parent, string reason) {
        GameOver gameOver = _gameOverScene.Instantiate<GameOver>();

        // Before _Ready() initialization
        gameOver.Name = "GameOver";
        gameOver.Position = new Vector2(0, -gameOver.Size.Y * 2.0f);

        parent.AddChild(gameOver);

        // After _Ready() initialization
        gameOver._reasonLabel.Text = reason;

        Tween tween = gameOver.GetTree().CreateTween();

        tween.TweenProperty(gameOver, "position", new Vector2(0, 0), 0.5f);
        tween.TweenCallback(Callable.From(() => gameOver.GetTree().Paused = true));  
    }

    private void OnPlayAgainPressed() {
        GetTree().Paused = false;
        GetTree().ChangeSceneToPacked(_gameScene);
    }

    private void OnReturnToMenuPressed() {
        GetTree().Paused = false; 
        GetTree().ChangeSceneToPacked(_menuScene);
    }
}