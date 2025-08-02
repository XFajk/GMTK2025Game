using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class Person : PathFollow3D {
    [Export]
    public float Speed = 0.01f;
    [Export]
    public float SpeedWithTask = 0.02f;
    [Export]
    public float MinRecalculationTime = 1.0f;
    [Export]
    public float MaxRecalculationTime = 3.0f;
    [Export]
    public bool IsCaptain = false;

    public bool InElevator = false;
    public int FloorNumber = 0;

    private RandomNumberGenerator _rng = new();

    private Timer _recalculateTimer;

    public List<ShipLocation> ShipTargets = [];
    private FloorPath ParentFloorPath;

    public Door DoorInFront;
    private CrewTask _currentTask = null;
    public bool DoIdle = true;


    public override void _Ready() {
        _rng.Randomize();

        AddToGroup("Crew");

        FloorPath parent = GetParent<FloorPath>();

        int numberOfFloors = 0;
        if (parent != null) {
            ParentFloorPath = parent;
            FloorNumber = parent.FloorNumber;
            numberOfFloors = parent.FloorElevator.Floors.Count;
        } else {
            GD.PrintErr("Person Is not attached to a FloorPath");
        }

        ProgressRatio = _rng.Randf();
        _recalculateTimer = GetNode<Timer>("RecalculateTimer");

        // This sets a callback that resets everything and sets a new target
        _recalculateTimer.Timeout += () => {
            _recalculateTimer.Stop();
            // This code makes sure that the new floor we want to transport the player to is different than the floor he is currently on
            int targetFloor;
            if (numberOfFloors < 1) {
                targetFloor = 0;
            } else {
                targetFloor = _rng.RandiRange(0, numberOfFloors - 1);
            }
            SetTarget(new ShipLocation(targetFloor, _rng.Randf()));
        };

        SetTarget(new ShipLocation(FloorNumber, _rng.Randf()));
    }

    public override void _Process(double delta) {
        if (InElevator) {
            return;
        }
        if (DoorInFront != null && !DoorInFront.DoorOpen) {
            return;
        }
        if (ShipTargets.Count == 0) {
            return;
        }

        float adjustedSpeed = (_currentTask == null) ? Speed : SpeedWithTask;
        ProgressRatio = Mathf.MoveToward(ProgressRatio, ShipTargets[0].Ratio, (float)delta * adjustedSpeed);

        if (Mathf.IsEqualApprox(ProgressRatio, ShipTargets[0].Ratio) && _recalculateTimer.IsStopped()) {
            if (ShipTargets[0].IsElevator) {
                var elevator = ParentFloorPath.FloorElevator;
                var detector = GetNode<Area3D>("ElevatorDetector");
                elevator.OnAreaEntered(detector);
                return;
            }

            if (_currentTask != null) {
                // task completed
                _currentTask.OnTaskComplete.Invoke(this);
                _recalculateTimer.WaitTime = _currentTask.Duration;
                _recalculateTimer.Start();
            } else {
                // idle about
                _recalculateTimer.Start(_rng.RandfRange(MinRecalculationTime, MaxRecalculationTime));
                ShipTargets.Clear();
            }
        }
    }

    public void SetTarget(ShipLocation location) {
        ShipTargets.Clear();
        _recalculateTimer.Stop();
        if (location.Floor == FloorNumber) {
            ShipTargets.Add(location);
            return;
        }

        ShipTargets.Add(new ShipLocation(FloorNumber, ParentFloorPath.ElevatorRatio, true));
        ShipTargets.Add(location);
    }

    public void SetCurrentTask(CrewTask task, ShipLocation location = null) {
        ShipTargets.Clear();
        if (location != null) SetTarget(location);

        if (_currentTask != null) {
            _currentTask.OnTaskAbort.Invoke(this);
        }

        _currentTask = task;

        if (task == null) {
            // explicitly start iding
            _recalculateTimer.Start(_rng.RandfRange(0, 2));
        }
    }
}
