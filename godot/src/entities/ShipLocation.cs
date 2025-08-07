using Godot;
using System;
using System.Collections.Generic;

public class ShipLocation {
    public int Floor;
    public float Ratio;
    public bool IsElevator; // if the ShipTarget is just a target to get to the elevator;

    public ShipLocation(int floor, float ratio, bool isElevator = false) {
        Floor = floor;
        Ratio = ratio;
        IsElevator = isElevator;
    }

    public static ShipLocation ClosesToPoint(Vector3 globalPoint, List<Floor> floors) {

        FloorPath bestFloor = null;
        float bestYDistance = float.MaxValue;
        Vector3 bestLocalPointOnCurve = Vector3.Zero;

        foreach (Floor floor in floors) {
            FloorPath currentPath = floor.FloorPath;
            Vector3 localPoint = currentPath.ToLocal(globalPoint);
            Vector3 closestPoint = currentPath.Curve.GetClosestPoint(localPoint);
            float yDistance = Math.Abs(closestPoint.Y - localPoint.Y);

            if (yDistance < bestYDistance) {
                bestFloor = currentPath;
                bestYDistance = yDistance;
                bestLocalPointOnCurve = localPoint;
            }
        }

        float offset = bestFloor.Curve.GetClosestOffset(bestLocalPointOnCurve);
        float length = bestFloor.Curve.GetBakedLength();

        return new(bestFloor.FloorNumber, offset / length);
    }
}
