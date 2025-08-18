using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;

public partial class Person : PathFollow3D {
    public const float GarbageThrowRadius = 1.0f;

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
    public State state = State.Idle;
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

            if (_currentTask != null) {
                // task completed
                _currentTask.OnTaskComplete.Invoke(this);
                HandleActionType(_currentTask, false);
                _currentTask = null;
            }

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

    public bool ThrowGarbage(Pickupable garbage) {
        if (state == State.InElevator) return false;
        var offset = new Vector3(0.0f, 0.1f, 0.0f);
        offset.X += _rng.RandfRange(-GarbageThrowRadius, GarbageThrowRadius);
        offset.Z += _rng.RandfRange(-GarbageThrowRadius, GarbageThrowRadius);

        garbage.GlobalPosition = GlobalPosition + offset;
        garbage.OriginalPosition = garbage.GlobalPosition;
        return true;
    }

    public override void _Process(double delta) {
        if (state == State.Floating) {
            AlienSprite.RotateZ(1.0f * (float) delta);
            return;
        }
        if (state == State.InElevator) {
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
            if (ShipTargets[0].IsElevator && ShipTargets.Count > 1) {
                var elevator = ParentFloorPath.FloorElevator;
                var detector = GetNode<Area3D>("ElevatorDetector");
                elevator.OnAreaEntered(detector);
                return;
            }

            if (_currentTask != null) {
                // task starts
                GD.Print($"Person {Name} started with task {_currentTask.ActionType}");
                RecalculateTimer.WaitTime = _currentTask.Duration;
                RecalculateTimer.Start();
                HandleActionType(_currentTask, true);

            } else {
                // idle about
                RecalculateTimer.Start(_rng.RandfRange(MinRecalculationTime, MaxRecalculationTime));
                ShipTargets.Clear();
            }
        }
    }

    private void HandleActionType(CrewTask task, bool whenStarting) {
        if (task.ActionType == CrewTask.Type.Disappear) {
            Visible = !whenStarting;
        }
    }


    public void SetTarget(ShipLocation location) {
        ShipTargets.Clear();
        RecalculateTimer.Stop();

        if (location.Floor != FloorNumber) {
            ShipTargets.Add(new ShipLocation(FloorNumber, ParentFloorPath.ElevatorRatio, true));
        }

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
        } else {
            GD.Print($"Person {Name} will do task {task.ActionType}");
        }
    }

    public bool HasTask() => (_currentTask != null);
    
    public enum State {
        Idle,
        InElevator,
        Floating
    }
}
