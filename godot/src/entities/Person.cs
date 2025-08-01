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

    public bool InElevator = false;
    public int FloorNumber = 0;

    private RandomNumberGenerator _rng = new();

    public Timer RecalculateTimer;

    public List<ShipLocation> ShipTargets = [];
    private FloorPath ParentFloorPath;

    public Door DoorInFront;
    public CrewTask CurrentTask = null;


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
        RecalculateTimer = GetNode<Timer>("RecalculateTimer");

        // This sets a callback that resets everything and sets a new target
        RecalculateTimer.Timeout += () => {
            RecalculateTimer.Stop();
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
        if (DoorInFront != null && !DoorInFront.DoorOpen)  {
            return;
        }
        if (ShipTargets.Count == 0) {
            return;
        }

        float adjustedSpeed = (CurrentTask == null) ? Speed : SpeedWithTask;
        ProgressRatio = Mathf.MoveToward(ProgressRatio, ShipTargets[0].Ratio, (float)delta * adjustedSpeed);

        if (Mathf.IsEqualApprox(ProgressRatio, ShipTargets[0].Ratio) && RecalculateTimer.IsStopped()) {
            if (ShipTargets[0].IsElevator) {
                var elevator = ParentFloorPath.FloorElevator;
                var detector = GetNode<Area3D>("ElevatorDetector");
                elevator.OnAreaEntered(detector);
                return;
            }

            if (CurrentTask != null) {
                // task completed
                CurrentTask.OnTaskComplete.Invoke(this);
                RecalculateTimer.WaitTime = CurrentTask.Duration;
                RecalculateTimer.Start();
            } else {
                // idle about
                RecalculateTimer.Start(_rng.RandfRange(MinRecalculationTime, MaxRecalculationTime));
                ShipTargets.Clear();
            }
        }
    }

    public void SetTarget(ShipLocation location) {
        ShipTargets.Clear();
        RecalculateTimer.Stop();
        if (location.Floor == FloorNumber) {
            ShipTargets.Add(location);
            return;
        }

        ShipTargets.Add(new ShipLocation(FloorNumber, ParentFloorPath.ElevatorRatio, true));
        ShipTargets.Add(location);
    }

}
