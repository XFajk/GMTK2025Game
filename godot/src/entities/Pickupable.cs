using Godot;
using System;

public partial class Pickupable : Area3D {
    [Signal]
    public delegate void OnPickupEventHandler(Pickupable what);
    [Signal]
    public delegate void OnDropdownEventHandler(Pickupable what);

    [Export]
    public Resource Resource = Resource.Unset;

    [Export]
    public float Quantity = 1.0f;

    [Export]
    public StringName ConnecatableName = new();

    public bool IsPickedUp = false;

    private Camera3D _camera;
    public Vector3 OriginalPosition = Vector3.Zero;
    private float _zDelta = 0.0f;
    private RayCast3D _machineDetectionRay = null;

    public Node3D Outline = null;
    public bool LookingAt = false;

    public override void _Ready() {
        SetCollisionLayerValue(1, false);
        SetCollisionMaskValue(1, false);

        SetCollisionLayerValue(4, true);
        SetCollisionMaskValue(4, true);

        Outline = GetNodeOrNull<Node3D>("Outline");
        if (Outline != null) {
            Outline.Visible = false;
        }

        OriginalPosition = GlobalPosition;
        _camera = GetViewport().GetCamera3D();

        if (Resource == Resource.Unset) {
            GD.PrintErr("Pickupable resource is not set for " + Name);
        }
    }

    public override void _Process(double delta) {
        if (LookingAt) {
            LookingAt = false;
        } else {
            Outline.Visible = false;
        }

        if (IsPickedUp) {
            Vector2 mousePosition = GetViewport().GetMousePosition();
            GlobalPosition = _camera.ProjectPosition(mousePosition, _zDelta - 2.0f);

            if (_machineDetectionRay == null) {
                _machineDetectionRay = new() {
                    CollideWithAreas = true,
                };

                _machineDetectionRay.SetCollisionMaskValue(1, false);
                _machineDetectionRay.SetCollisionMaskValue(5, true);

                AddChild(_machineDetectionRay);
            }
            _machineDetectionRay.GlobalPosition = _camera.GlobalPosition;
            _machineDetectionRay.GlobalRotation = _camera.GlobalRotation;
            _machineDetectionRay.TargetPosition = _camera.ToLocal(_camera.ProjectPosition(mousePosition, 100.0f));

            GodotObject collisionObject = _machineDetectionRay.GetCollider();
            if (collisionObject is Area3D area) {
                Connectable connectable = ExtractConnectable(area);
                if (connectable == null) {
                    return;
                }
                if (connectable is Machine machine) {
                    TryAddTo(machine);
                } else if (connectable is IContainer container) {
                    TryAddTo(container);
                }
            }
        } else {
            GlobalPosition = OriginalPosition;
        }
    }

    public void InteractedWith() {

        if (IsPickedUp) {
            IsPickedUp = false;
            EmitSignalOnDropdown(this);
            
            if (_machineDetectionRay != null) {
                _machineDetectionRay.QueueFree();
                _machineDetectionRay = null;
            }
            return;
        }

        IsPickedUp = true;
        EmitSignalOnPickup(this);

        _machineDetectionRay = new() {
            CollideWithAreas = true,
        };

        _machineDetectionRay.SetCollisionMaskValue(1, false);
        _machineDetectionRay.SetCollisionMaskValue(5, true);

        AddChild(_machineDetectionRay);

        _zDelta = Mathf.Abs(_camera.GlobalPosition.Z - 0);
        OriginalPosition = GlobalPosition;

    }

    private Connectable ExtractConnectable(Area3D area) {
        if (!area.IsInGroup("MachineDetectionAreas")) {
            return null;
        }
        if (area.GetParent() is Connectable c) {
            return c;
        }
        return null;
    }

    private void TryAddTo(Machine machine) {
        foreach (MachineBuffer buffer in machine.Inputs()) {
            TryAddTo(buffer);
        }
    }

    private void TryAddTo(IContainer container) {
        if (container.GetResource() == Resource.Garbage) {
            container.AddQuantity(1);
            IsPickedUp = false;
            EmitSignalOnDropdown(this);
            QueueFree();
            return;
        }
    }
}
