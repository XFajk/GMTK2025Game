using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class Person : PathFollow3D {

    [Signal]
    public delegate void ReachedDestinationEventHandler(Person person);

    [Export]
    public float Speed = 0.01f;
    [Export]
    public float MinRecalculationTime = 1.0f;
    [Export]
    public float MaxRecalculationTime = 3.0f;

    public bool InElevator = false;
    public int FloorNumber = 0;
    public int NumberOfFloors = 0;

    private RandomNumberGenerator _rng = new();

    public Timer RecalculateTimer;

    public List<ShipLocation> ShipTargets = [];
    private FloorPath ParentFloorPath;

    public Door DoorInFront;

    public override void _Ready() {
        _rng.Randomize();

        AddToGroup("Crew");

        FloorPath parent = GetParent<FloorPath>();
        if (parent != null) {
            ParentFloorPath = parent;
            FloorNumber = parent.FloorNumber;
            NumberOfFloors = parent.FloorElevator.Floors.Count;
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
            if (NumberOfFloors < 2) {
                targetFloor = 0;
            } else {
                int idx = _rng.RandiRange(0, NumberOfFloors - 2);
                targetFloor = idx >= FloorNumber ? idx + 1 : idx;
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
        
        ProgressRatio = Mathf.MoveToward(ProgressRatio, ShipTargets[0].Ratio, (float)delta * Speed);

        if (Mathf.IsEqualApprox(ProgressRatio, ShipTargets[0].Ratio) && RecalculateTimer.IsStopped()) {
            if (ShipTargets[0].IsElevator) {
                var elevator = ParentFloorPath.FloorElevator;
                var detector = GetNode<Area3D>("ElevatorDetector");
                elevator.OnAreaEntered(detector);
                return;
            }
            RecalculateTimer.Start(_rng.RandfRange(MinRecalculationTime, MaxRecalculationTime));
            ShipTargets.Clear();
            EmitSignalReachedDestination(this);
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
