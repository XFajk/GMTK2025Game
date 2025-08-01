using Godot;
using System;
using System.Collections.Generic;

public partial class Door : Area3D {
    private AnimationPlayer _movingPartAnimationPlayer;
    private List<Person> _peopleWaitingForAnOpenDoor = new List<Person>();
    public bool DoorOpen = false;

    public override void _Ready() {
        _movingPartAnimationPlayer = GetNode<AnimationPlayer>("MovingPart/AnimationPlayer");
        _movingPartAnimationPlayer.AnimationFinished += (animationName) => {
            switch (animationName) {
                case "Open":
                    DoorOpen = true;
                    break;
                case "Close":
                    DoorOpen = false;
                    break;
                default:
                    DoorOpen = true;
                    break;
            }
        };

        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    public override void _Process(double delta) {
        if (_peopleWaitingForAnOpenDoor.Count == 0 && DoorOpen) {
            if (!_movingPartAnimationPlayer.IsPlaying()) _movingPartAnimationPlayer.Play("Close");
        } else if (_peopleWaitingForAnOpenDoor.Count > 0 && !DoorOpen) {
            if (!_movingPartAnimationPlayer.IsPlaying()) _movingPartAnimationPlayer.Play("Open");
        }
    }

    private void OnAreaEntered(Area3D area) {
        Person person = area.GetParent<Person>();
        if (person == null) return;

        _peopleWaitingForAnOpenDoor.Add(person);
    }

    private void OnAreaExited(Area3D area) {
        Person person = area.GetParent<Person>();
        if (person == null) return;

        _peopleWaitingForAnOpenDoor.Remove(person);
    }
}
