using Godot;
using System;

[GlobalClass]
public partial class Floor : Node3D {
    [Export]
    public int FloorNumber = 0;

    [Export]
    public FloorPath FloorPath;
}
