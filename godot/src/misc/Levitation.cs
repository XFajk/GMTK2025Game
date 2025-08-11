using Godot;
using System;

public partial class Levitation : Node3D {
    [Export]
    public float LevitationFrequency = 1.0f;
    [Export]
    public float LevitationStrength = 1.0f;
    [Export]
    public float RotationSpeed = 0.0f;

    private Vector3 _initialPosition;

    public override void _Ready() {
        _initialPosition = Position;
    }

    public override void _Process(double delta) {
        float time = (float)Time.GetTicksMsec() / 1000.0f;
        float offset = Mathf.Sin(time * LevitationFrequency) * LevitationStrength * 0.02f;

        Position = new Vector3(_initialPosition.X, _initialPosition.Y + offset, _initialPosition.Z);
        RotateY(RotationSpeed * (float)delta);
    }
}
