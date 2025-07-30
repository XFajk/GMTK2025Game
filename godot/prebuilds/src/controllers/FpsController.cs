using Godot;
using System;

[GlobalClass]
public partial class FpsController : Node {

    public enum PlayerState {
        Standing,
        Moving,
        Nothing,
    }

    public PlayerState State = PlayerState.Standing;

    [ExportCategory("Movement")]
    [Export]
    public float MovemetnSpeed = 2.0f;

    [Export]
    public float RunningSpeed = 3.5f;

    [Export]
    public float Acceleration = 15.0f;

    [Export]
    public float Gravity = -20.0f;

    [Export]
    public float JumpStrength = 7.5f;

    [Export]
    public float TerminalVelocity = -100.0f;

    public float VerticalSpeed = 0.0f;
    public Vector3 Direction = Vector3.Zero;

    [ExportCategory("Stamina")]
    [Export]
    public bool StaminaOn = false;

    [Export]
    public float StaminaRechargeSpeed = 30.0f;

    [Export]
    public float StaminaDegradationSpeed = 15.0f;

    public CharacterBody3D Body;
    public Node3D Head;

    public override void _Ready() {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        Body = GetParent() as CharacterBody3D;
        if (Body == null) {
            GD.PushError("FpsController is attached to a NON CharacterBody3D object");
        }

        Head = Body.GetNode("Head") as Node3D;
        if (Head == null) {
            GD.PushError("FpsController's body is missing a Node3D named \"Head\"");
        }
    }

    public override void _Input(InputEvent @event) {
        Node globalSingleton = GetNode("/root/Global");
        if (globalSingleton == null) {
            GD.PushError("Could not get the Global Singleton Object from the scene tree");
        }

        float mouseSensitivity = globalSingleton.Get("mouse_sensitivity").As<float>();

        if (@event is InputEventMouseMotion mouseMotion && State != PlayerState.Nothing && Input.MouseMode == Input.MouseModeEnum.Captured) {
            Body.RotateY(float.DegreesToRadians(-mouseMotion.Relative.X * mouseSensitivity));
            Head.RotateX(float.DegreesToRadians(-mouseMotion.Relative.Y * mouseSensitivity));

            float clampedX = float.Clamp(Head.RotationDegrees.X, -70, 70);
            Head.Rotation = new Vector3(float.DegreesToRadians(clampedX), Head.Rotation.Y, Head.Rotation.Z);
        }
    }

    public override void _PhysicsProcess(double delta) {
        switch (State) {
            case PlayerState.Standing:
                Standing(delta);
                break;
            case PlayerState.Moving:
                Moving(delta);
                break;
            default:
                break;
        }

        Body.MoveAndSlide();
    }

    private void Standing(double delta) {
        if (IsTryingToMove()) {
            State = PlayerState.Moving;
        }

        AddGravity(delta);

        if (Input.IsActionPressed("jump") && Body.IsOnFloor()) {
            Jump();
        }

        Body.Velocity = Body.Velocity.MoveToward(Vector3.Zero, Acceleration * (float)delta);
        Direction = Vector3.Zero;
    }

    private void Moving(double delta) {

        Direction = Vector3.Zero;
        if (!IsTryingToMove()) {
            State = PlayerState.Standing;
        }

        if (Input.IsActionPressed("move_forward")) {
            Direction -= Body.Transform.Basis.Z;
        }
        if (Input.IsActionPressed("move_backward")) {
            Direction += Body.Transform.Basis.Z;
        }
        if (Input.IsActionPressed("move_right")) {
            Direction += Body.Transform.Basis.X;
        }
        if (Input.IsActionPressed("move_left")) {
            Direction -= Body.Transform.Basis.X;
        }

        AddGravity(delta);

        if (Input.IsActionPressed("jump") && Body.IsOnFloor()) {
            Jump();
        }

        if (Direction.Length() != 0.0f) {
            Direction = Direction.Normalized();
            float goalSpeed = MovemetnSpeed;
            if (Input.IsActionPressed("sprint")) {
                goalSpeed = RunningSpeed;
            }
            Body.Velocity = Body.Velocity.MoveToward(Direction * goalSpeed, Acceleration * (float)delta);
        }
    }

    private void Jump() {
        VerticalSpeed = JumpStrength;
        Body.Velocity = new Vector3(Body.Velocity.X, VerticalSpeed, Body.Velocity.Z);
    }

    private void AddGravity(double delta) {
        if (!Body.IsOnFloor() && VerticalSpeed > TerminalVelocity) {
            VerticalSpeed += Gravity * (float)delta;
        } else if (!Body.IsOnFloor()) {
            VerticalSpeed = TerminalVelocity;
        } else {
            VerticalSpeed = 0.0f;
        }

        if (Body.IsOnCeiling()) {
            VerticalSpeed = -10.0f;
        }

        Body.Velocity = new Vector3(Body.Velocity.X, VerticalSpeed, Body.Velocity.Z);
    }

    private bool IsTryingToMove() {
        return Input.IsActionPressed("move_forward") || Input.IsActionPressed("move_backward") ||
               Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right");
    }

}
