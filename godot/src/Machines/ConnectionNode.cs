using Godot;
using System;

public partial class ConnectionNode : Area3D
{
    [Signal]
    // Handled by Connectable.cs
    public delegate void OnConnectionClickEventHandler(ConnectionNode point);

    public override void _Ready() {
        InputEvent += HandleConnectionClicked;
    }

    // camera: Node, event: InputEvent, event_position: Vector3, normal: Vector3, shape_idx: int
    private void HandleConnectionClicked(Node camera, InputEvent eventType, Vector3 clickPosition, Vector3 clickNormal, long shapeIndex) {
        if (eventType.IsActionPressed("left_click")) {
            EmitSignal(SignalName.OnConnectionClick, this);
        }
    }
}
