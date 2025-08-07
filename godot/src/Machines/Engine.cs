using Godot;

public partial class Engine : Machine {
    public const double ChangeDuration = 2f;
    public const float DefaultEnginePower = 0.1f;
    private float _enginePower = DefaultEnginePower;
    private float _maxProcessingPerSecond;
    private JetEngineFire _jetFireVfx;
    private Core _coreVfx;

    public float GetEnginePower() => _enginePower;
    public void SetEnginePower(float powerFactor) {
        _enginePower = powerFactor;
        _processingPerSecond = _maxProcessingPerSecond * _enginePower;
        _jetFireVfx.SetEnginePower(_enginePower);
        _coreVfx.SetEnginePower(_enginePower);
    }


    public override void _Ready() {
        _maxProcessingPerSecond = _processingPerSecond;

        Node3D vfx = GetNode<Node3D>("VFX");
        vfx.SetVisible(true);

        _jetFireVfx = vfx.GetNode<JetEngineFire>("JetEngineFire");
        _coreVfx = vfx.GetNode<Core>("Core");

        base._Ready();
        SetEnginePower(DefaultEnginePower);
    }
}
