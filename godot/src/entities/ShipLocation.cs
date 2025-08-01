using Godot;
using System;

public class ShipLocation {
    public int Floor;
    public float Ratio;
    public bool IsElevator; // if the ShipTarget is just a target to get to the elevator;

    public ShipLocation(int floor, float ratio, bool isElevator = false) {
        Floor = floor;
        Ratio = ratio;
        IsElevator = isElevator;
    }
}