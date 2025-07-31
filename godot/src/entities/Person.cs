using Godot;
using System;

public partial class Person : PathFollow3D {

    [Export]
    public float Speed = 0.01f;

    [Export]
    public float MinRecalculationTime = 1.0f;
    [Export]
    public float MaxRecalculationTime = 3.0f;

    public bool InElevator = false;
    [Export]
    public int FloorNumber = 0;

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    private Timer _recalulateTimer;
    private float _targetRatio = 0.0f;

    public override void _Ready() {
        _rng.Randomize();
        ProgressRatio = _rng.Randf();
        _recalulateTimer = GetNode<Timer>("RecalulateTimer");
        _recalulateTimer.Timeout += () => {
            _targetRatio = _rng.Randf();
            _recalulateTimer.Stop();
        };

        _targetRatio = _rng.Randf();
    }
    public override void _Process(double delta) {
        if (InElevator) {
            return;
        }
        ProgressRatio = Mathf.MoveToward(ProgressRatio, _targetRatio, (float)delta * Speed);  
        if (Mathf.IsEqualApprox(ProgressRatio, _targetRatio) && _recalulateTimer.IsStopped()) {
            _recalulateTimer.Start(_rng.RandfRange(MinRecalculationTime, MaxRecalculationTime));
        }
    } 
}
