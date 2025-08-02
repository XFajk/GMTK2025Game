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

    public Sprite3D AlienSprite;
    private float _alienSpriteAnimatinTimerValue = 0.0f;
    private int _alienSpriteAnimationRotationChanger = 1;


    private static PackedScene GarbageScene = GD.Load<PackedScene>("res://scenes/entities/garbage.tscn");
    private static Timer _garbageTimer;

    public override void _Ready() {
        _rng.Randomize();

        AddToGroup("Crew");

        AlienSprite = GetNode<Sprite3D>("AlienSprite");

        _garbageTimer = new();
        AddChild(_garbageTimer);
        _garbageTimer.Start(_rng.RandfRange(15.0f, 30.0f));

        FloorPath parent = GetParent<FloorPath>();

        int numberOfFloors = 0;
        if (parent != null) {
            ParentFloorPath = parent;
            FloorNumber = parent.FloorNumber;
            numberOfFloors = parent.FloorElevator.Floors.Count;
        } else {
            GD.PrintErr("Person Is not attached to a FloorPath");
        }

        _garbageTimer.Timeout += () => {
            if (InElevator) {
                return;
            }
            var garbage = GarbageScene.Instantiate<Pickupable>();
            parent.GetParent().AddChild(garbage);
            garbage.GlobalPosition = GlobalPosition + new Vector3(0.0f, 1.0f, 0.0f) * 0.1f;
            garbage.OriginalPosition = GlobalPosition + new Vector3(0.0f, 1.0f, 0.0f) * 0.1f;
            _garbageTimer.Start(_rng.RandfRange(15.0f, 30.0f));
            GD.Print("Garbage spawned at " + garbage.GlobalPosition);
        };

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
            AlienSprite.Rotation = new Vector3(0, 0, 0);
            return;
        }
        if (DoorInFront != null && !DoorInFront.DoorOpen) {
            AlienSprite.Rotation = new Vector3(0, 0, 0);
            return;
        }
        if (ShipTargets.Count == 0) {
            AlienSprite.Rotation = new Vector3(0, 0, 0);
            return;
        }

        float adjustedSpeed = (CurrentTask == null) ? Speed : SpeedWithTask;
        ProgressRatio = Mathf.MoveToward(ProgressRatio, ShipTargets[0].Ratio, (float)delta * adjustedSpeed);
        if (ShipTargets[0].Ratio - ProgressRatio < 0.0f) {
            AlienSprite.FlipH = true;
        } else {
            AlienSprite.FlipH = false;
        }

        _alienSpriteAnimatinTimerValue += (float)delta;
        if (_alienSpriteAnimatinTimerValue > 0.2f) {
            _alienSpriteAnimatinTimerValue = 0.0f;
            _alienSpriteAnimationRotationChanger *= -1;
            AlienSprite.RotateZ(Mathf.DegToRad(20 * _alienSpriteAnimationRotationChanger));
        }

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
