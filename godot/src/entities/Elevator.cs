using Godot;
using System;
using System.Collections.Generic;

public partial class Elevator : Area3D {
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private List<Floor> Floors = new List<Floor>();

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
        var parent = area.GetParent();

        if (parent is Person person) {
            if (person.InElevator) {
                return;
            }
            bool goIntoElevator = _rng.RandWeighted([0.5f, 0.5f]) == 0;
            if (goIntoElevator) {
                int newFloorIndex;
                // This code makes sure that the new floor we want to transport the player to is different than the floor he is currently on
                int count = Floors.Count;
                if (count < 2) {
                    return;
                } else {
                    int idx = _rng.RandiRange(0, count - 2);
                    newFloorIndex = idx >= person.FloorNumber ? idx + 1 : idx;
                } 

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

                Vector3 targetFloorInPathLocalSpace = targetFloor.FloorPath.ToLocal(targetFloor.GlobalPosition);
                Vector3 closedPointToFloor = targetFloor.FloorPath.Curve.GetClosestPoint(targetFloorInPathLocalSpace);
                Vector3 GlobalPathPoint = targetFloor.FloorPath.ToGlobal(closedPointToFloor);

                tween.TweenProperty(person, "global_position", GlobalPathPoint, 0.1);
                tween.TweenCallback(Callable.From(() => {
                    float offset = targetFloor.FloorPath.Curve.GetClosestOffset(closedPointToFloor);
                    float length = targetFloor.FloorPath.Curve.GetBakedLength();
                    float ratio = length > 0f ? offset / length : 0f;

                    RemoveChild(person);
                    targetFloor.FloorPath.AddChild(person);
                    person.ProgressRatio = ratio;
                    person.InElevator = false;
                    person.FloorNumber = targetFloor.FloorNumber;
                }));
            }
        }
    }

}
