using Godot;
using System;
using System.Dynamic;

public partial class Person : PathFollow3D {

    [Export]
    public float Speed = 0.01f;

    [Export]
    public float MinRecalculationTime = 1.0f;
    [Export]
    public float MaxRecalculationTime = 3.0f;

    public bool InElevator = false;
    public int FloorNumber = 0;

    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    private Timer _recalculateTimer;
    private float _targetRatio = 0.0f; // the target ration on the correct floor the person is at
    private float _globalTargetRation = 0.0f; // the  target that is cupled with the _targetFloor aka this target only applies if on the correct floor 

    private int? _targetFloor = null;

    public int? TargetFloor { get { return _targetFloor; } }

    public Door DoorInFront;

    public override void _Ready() {
        _rng.Randomize();

        ProgressRatio = _rng.Randf();
        _recalculateTimer = GetNode<Timer>("RecalculateTimer");

        // This sets a callback that resets everything and sets a new target
        _recalculateTimer.Timeout += () => {
            _targetRatio = _rng.Randf();
            _recalculateTimer.Stop();
            _globalTargetRation = 0.0f;
            _targetFloor = null;
        };

        _targetRatio = _rng.Randf();
    }

    public override void _Process(double delta) {
        if (InElevator) {
            return;
        }
        if (DoorInFront != null && !DoorInFront.DoorOpen) {
            return;
        }
        if (_targetFloor != null && _targetFloor == FloorNumber) {
            _targetRatio = _globalTargetRation;
        }
        ProgressRatio = Mathf.MoveToward(ProgressRatio, _targetRatio, (float)delta * Speed);
        if (Mathf.IsEqualApprox(ProgressRatio, _targetRatio) && _recalculateTimer.IsStopped()) {
            _recalculateTimer.Start(_rng.RandfRange(MinRecalculationTime, MaxRecalculationTime));
        }
    }

    public void SetTarget(float ratio, int floor) {
        FloorPath parentPath = GetParent<FloorPath>();
        if (parentPath == null) {
            GD.PrintErr("Person Node is not attached to a FloorPath; can't set target at current point");
            return;
        }
        _recalculateTimer.Stop();
        _targetRatio = parentPath.ElevatorRatio;
        _targetFloor = floor;
        _globalTargetRation = ratio;
    }

}
