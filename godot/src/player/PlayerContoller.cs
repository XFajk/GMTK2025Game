using Godot;
using System;

public partial class PlayerContoller : Camera3D {

    [Export]
    public float DragScalar = 0.01f;

    [Export]
    public float ZoomSpeed = 120.0f;

    private Vector2 _previousePosition = Vector2.Zero;
    private Vector2 _previouseMousePosition = Vector2.Zero;


    public override void _Ready() {
        _previousePosition = new Vector2(Position.X, Position.Y);
        _previouseMousePosition = GetViewport().GetMousePosition();
    }
    public override void _Process(double delta) {
        if (Input.IsActionPressed("drag")) {
            Vector2 mousePositionDifference = _previouseMousePosition - GetViewport().GetMousePosition();
            Vector2 positionOffset = _previousePosition + mousePositionDifference;
            Position = new Vector3(_previousePosition.X + positionOffset.X * DragScalar * (Fov * 0.01f), _previousePosition.Y - positionOffset.Y * DragScalar * (Fov * 0.01f), Position.Z);
        } else {
            _previouseMousePosition = GetViewport().GetMousePosition();
            _previousePosition = new Vector2(Position.X, Position.Y);
        }


        if (Input.IsActionPressed("zoom_in")) {
            Fov -= ZoomSpeed * (float)delta;
        }
        if (Input.IsActionPressed("zoom_out")) {
            Fov += ZoomSpeed * (float)delta;
        }

        Fov = float.Clamp(Fov, 20.0f, 100.0f);

    }
}
