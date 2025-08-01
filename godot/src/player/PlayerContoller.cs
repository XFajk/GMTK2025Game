using Godot;
using System;

public partial class PlayerContoller : Camera3D {

    [Export]
    public float DragScalar = 0.01f;

    [Export]
    public float ZoomSpeed = 120.0f;

    [Export]
    public Aabb BoundingBox = new Aabb(new Vector3(-100, -100, 0), new Vector3(200, 200, 2));

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
            Position = new Vector3(
                _previousePosition.X + positionOffset.X * DragScalar * (Fov * 0.01f), 
                _previousePosition.Y - positionOffset.Y * DragScalar * (Fov * 0.01f), 
                Position.Z
            );
        } else {
            _previouseMousePosition = GetViewport().GetMousePosition();
            _previousePosition = new Vector2(Position.X, Position.Y);
        }

        Position = new Vector3(
            float.Clamp(Position.X, BoundingBox.Position.X, BoundingBox.End.X),
            float.Clamp(Position.Y, BoundingBox.Position.Y, BoundingBox.End.Y),
            Position.Z
        );


        if (Input.IsActionPressed("zoom_in")) {
            Fov -= ZoomSpeed * (float)delta;
        }
        if (Input.IsActionPressed("zoom_out")) {
            Fov += ZoomSpeed * (float)delta;
        }

        if (Input.IsActionJustPressed("mouse_zoom_in")) {
            Fov -= ZoomSpeed / 60.0f; 
        }
        if (Input.IsActionJustPressed("mouse_zoom_out")) {
            Fov += ZoomSpeed / 60.0f; 
        }

        Fov = float.Clamp(Fov, 20.0f, 100.0f);

    }
}
