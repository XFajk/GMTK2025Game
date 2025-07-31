using Godot;
using System;
using System.Collections.Generic;

public abstract partial class Connectable : Node3D {
    [Signal]
    // Handled by Game.cs
    public delegate void OnConnectionClickEventHandler(Machine machine);

    // camera: Node, event: InputEvent, event_position: Vector3, normal: Vector3, shape_idx: int
    public void HandleConnectionClicked(Node camera, InputEvent eventType, Vector3 clickPosition, Vector3 clickNormal, int shapeIndex) {
        if (eventType.IsActionPressed("left_click")) {
            EmitSignal(SignalName.OnConnectionClick, this);
        }
    }

    public abstract IEnumerable<InputOutput> Inputs();
    public abstract IEnumerable<InputOutput> Outputs();
}
