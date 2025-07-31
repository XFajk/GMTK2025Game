using Godot;
using System;

[GlobalClass]
public partial class FloorPath : Path3D {
    [Export]
    public Elevator FloorElevator;

    [Export]
    public int FloorNumber = 0;

    public Vector3 ClosestPointToElevator;
    public float ElevatorRatio;

    public override void _Ready() {
        Floor floor = FloorElevator.Floors[FloorNumber];

        Vector3 floorPositon = floor.GlobalPosition;
        Vector3 floorPositionInOurLocalSpace = ToLocal(floorPositon);
        ClosestPointToElevator = Curve.GetClosestPoint(floorPositionInOurLocalSpace);

        float offset = Curve.GetClosestOffset(ClosestPointToElevator);
        float length = Curve.GetBakedLength();

        ElevatorRatio = length > 0f ? offset / length : 0f;

        foreach (Node child in GetChildren()) {
            if (child is Person person) 
                person.FloorNumber = FloorNumber;
        }
    }
}
