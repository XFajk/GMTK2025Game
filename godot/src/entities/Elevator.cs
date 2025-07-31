using Godot;
using System;
using System.Collections.Generic;

public partial class Elevator : Area3D {
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    public List<Floor> Floors = new List<Floor>();

    public override void _Ready() {
        _rng.Randomize();
        AreaEntered += OnAreaEntered;
        foreach (Node child in GetChildren()) {
            if (child is Floor floor) {
                Floors.Add(floor);
            }
        }
        Floors.Sort((a, b) => a.FloorNumber.CompareTo(b.FloorNumber));
    }

    private void OnAreaEntered(Area3D area) {
        Person person = area.GetParent<Person>();

        if (person != null) {
            if (person.InElevator) {
                return;
            }
            bool goIntoElevator = false;
            int newFloorIndex;
            if (person.TargetFloor != null && person.FloorNumber != person.TargetFloor) {
                goIntoElevator = true;
                newFloorIndex = (int)person.TargetFloor;
            } else {
                goIntoElevator = _rng.RandWeighted([0.5f, 0.5f]) == 0;
                // This code makes sure that the new floor we want to transport the player to is different than the floor he is currently on
                int count = Floors.Count;
                if (count < 2) {
                    return;
                } else {
                    int idx = _rng.RandiRange(0, count - 2);
                    newFloorIndex = idx >= person.FloorNumber ? idx + 1 : idx;
                }
            }
            if (goIntoElevator) {

                var tween = GetTree().CreateTween();
                Node personParent = person.GetParent();
                person.InElevator = true;
                var storedPersonPosition = person.GlobalPosition;

                personParent.RemoveChild(person);

                AddChild(person);
                person.GlobalPosition = storedPersonPosition;

                Floor currentFloor = Floors[person.FloorNumber];
                tween.TweenProperty(person, "global_position", currentFloor.GlobalPosition, 1.0);

                Floor targetFloor = Floors[newFloorIndex];
                tween.TweenProperty(person, "global_position", targetFloor.GlobalPosition, 2.0).SetTrans(Tween.TransitionType.Sine);

                Vector3 GlobalPathPoint = targetFloor.FloorPath.ToGlobal(targetFloor.FloorPath.ClosestPointToElevator);

                tween.TweenProperty(person, "global_position", GlobalPathPoint, 0.1);

                tween.TweenCallback(Callable.From(() => {
                    RemoveChild(person);
                    targetFloor.FloorPath.AddChild(person);
                    person.ProgressRatio = targetFloor.FloorPath.ElevatorRatio;
                    person.InElevator = false;
                    person.FloorNumber = targetFloor.FloorNumber;
                }));
            }
        }
    }

}
