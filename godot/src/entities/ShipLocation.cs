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

        FloorPath bestPath = null;
        float bestYDistance = float.MaxValue;
        foreach (Floor floor in floors) {
            FloorPath currentPath = floor.FloorPath;
            Vector3 closestPoint = currentPath.Curve.GetClosestPoint(globalPoint);
            float yDistance = Math.Abs(closestPoint.Y - globalPoint.Y);

            if (bestPath == null || yDistance < bestYDistance) {
                bestPath = currentPath;
                bestYDistance = yDistance;
            }
        }

        FloorPath targetPath = bestPath;
        Vector3 localPoint = targetPath.ToLocal(globalPoint);

        Vector3 closestPointOnTheCurveToTheGlobalPoint = targetPath.Curve.GetClosestPoint(localPoint);

        float offset = targetPath.Curve.GetClosestOffset(closestPointOnTheCurveToTheGlobalPoint);
        float length = targetPath.Curve.GetBakedLength();

        return new(targetPath.FloorNumber, length > 0f ? offset / length : 0f);
    }
}
