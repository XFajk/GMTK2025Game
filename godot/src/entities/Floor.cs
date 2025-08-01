using Godot;
using System;

[GlobalClass]
public partial class Floor : Node3D {
    [Export]
    public int FloorNumber = 0;

    [Export]
    public FloorPath FloorPath;


    public override void _Ready() {
        AddToGroup("Floors");
    }
}
