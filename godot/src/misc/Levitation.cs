using Godot;
using System;

public partial class Levitation : Node3D {
    [Export]
    public float LevitationFrequency = 1.0f;
    [Export]
    public float LevitationStrength = 1.0f;
    [Export]
    public float RotationSpeed = 0.0f;

    public override void _Process(double delta) {
        float time = (float)Time.GetTicksMsec() / 1000.0f;
        float offset = Mathf.Sin(time * LevitationFrequency) * LevitationStrength * 0.001f;

        Position = new Vector3(Position.X, Position.Y + offset, Position.Z);
        RotateY(RotationSpeed * (float)delta);
    }
}
