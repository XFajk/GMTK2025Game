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

    public Timer RecalculateTimer;

    public List<ShipLocation> ShipTargets = [];
    private FloorPath ParentFloorPath;

    public Door DoorInFront;
    private CrewTask _currentTask = null;
    public bool DoIdle = true;

    public Sprite3D AlienSprite;
    private float _alienSpriteAnimatinTimerValue = 0.0f;
    private int _alienSpriteAnimationRotationChanger = 1;

    private static PackedScene GarbageScene = GD.Load<PackedScene>("res://scenes/entities/garbage.tscn");

    public static Texture2D[] AlienSpriteTextures = {
        GD.Load<Texture2D>("res://assets/sprites/simon.png"),
        GD.Load<Texture2D>("res://assets/sprites/matej_goc.png"),
        GD.Load<Texture2D>("res://assets/sprites/igi.png"),
        GD.Load<Texture2D>("res://assets/sprites/vilo.png"),
        GD.Load<Texture2D>("res://assets/sprites/oliver.png"),
        GD.Load<Texture2D>("res://assets/sprites/kristian.png"),
    };

    public override void _Ready() {
        _rng.Randomize();

        AddToGroup("Crew");

        AlienSprite = GetNode<Sprite3D>("AlienSprite");
        AlienSprite.Texture = AlienSpriteTextures[_rng.RandiRange(0, AlienSpriteTextures.Length - 1)];

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

    public bool ThrowGarbage() {
        if (InElevator) return false;

        var garbage = GarbageScene.Instantiate<Pickupable>();
        ParentFloorPath.GetParent().AddChild(garbage);
        garbage.GlobalPosition = GlobalPosition + new Vector3(0.0f, 1.0f, 0.0f) * 0.1f;
        garbage.OriginalPosition = GlobalPosition + new Vector3(0.0f, 1.0f, 0.0f) * 0.1f;
        GD.Print("Garbage spawned at " + garbage.GlobalPosition);
        return true;
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

        float adjustedSpeed = (_currentTask == null) ? Speed : SpeedWithTask;
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

            if (_currentTask != null) {
                // task completed
                _currentTask.OnTaskComplete.Invoke(this);
                RecalculateTimer.WaitTime = _currentTask.Duration;
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

    public void SetCurrentTask(CrewTask task, ShipLocation location = null) {
        ShipTargets.Clear();
        if (location != null) SetTarget(location);

        if (_currentTask != null) {
            _currentTask.OnTaskAbort.Invoke(this);
        }

        _currentTask = task;

        if (task == null) {
            // explicitly start iding
            RecalculateTimer.Start(_rng.RandfRange(0, 2));
        }
    }
}
