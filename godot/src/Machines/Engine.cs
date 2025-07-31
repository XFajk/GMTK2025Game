public partial class Engine : Machine {
    public const float DefaultEnginePower = 0.1f;
    public float EnginePower = DefaultEnginePower;
    private float _maxProcessingPerSecond;

    public override void _Process(double deltaTime) {
        _processingPerSecond = _maxProcessingPerSecond * EnginePower;
        base._Process(deltaTime);
    }
}