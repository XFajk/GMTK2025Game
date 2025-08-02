using Godot;
using System;

public partial class Pickupable : Area3D {
    [Export]
    public Resource Resource = Resource.Unset;

    [Export]
    public float Quantity = 1.0f;

    [Export]
    public StringName ConnecatableName = new();

    public bool IsPickedUp = false;

    private Camera3D _camera;
    private Vector3 _originalPosition = Vector3.Zero;
    private float _zDelta = 0.0f;
    private RayCast3D _machineDetectionRay = null;

    public override void _Ready() {
        SetCollisionLayerValue(1, false);
        SetCollisionMaskValue(1, false);

        SetCollisionLayerValue(4, true);
        SetCollisionMaskValue(4, true);

        _originalPosition = GlobalPosition;
        _camera = GetViewport().GetCamera3D();

        if (Resource == Resource.Unset) {
            GD.PrintErr("Pickupable resource is not set for " + Name);
        }

        Connect(SignalName.InputEvent, new Callable(this, nameof(InteractedWith)));
    }

    public override void _Process(double delta) {
        if (IsPickedUp) {
            Vector2 mousePosition = GetViewport().GetMousePosition();
            GlobalPosition = _camera.ProjectPosition(mousePosition, _zDelta - 2.0f);
            if (_machineDetectionRay == null) {
                _machineDetectionRay = new() {
                    GlobalPosition = _camera.GlobalPosition,
                    CollideWithAreas = true,
                };

                _machineDetectionRay.SetCollisionMaskValue(1, false);
                _machineDetectionRay.SetCollisionMaskValue(5, true);

                AddChild(_machineDetectionRay);
            }
            _machineDetectionRay.GlobalPosition = _camera.GlobalPosition;
            _machineDetectionRay.TargetPosition = _camera.ProjectPosition(mousePosition, 100.0f);

            GodotObject collisionObject = _machineDetectionRay.GetCollider();
            if (collisionObject is Area3D area) {
                Connectable connectable = ExtractConnectable(area);
                if (connectable == null) {
                    return;
                }
                if (connectable is Machine machine) {
                    AssingSelfToMachine(machine);
                } else if (connectable is StorageContainer container) {
                    AssingSelfToContainer(container);
                }
            }
        } else {
            GlobalPosition = _originalPosition;
        }
    }

    private void InteractedWith(Camera3D _c, InputEvent @event, Vector3 _p, Vector3 _n, long _i) {
        if (@event.IsActionPressed("interact")) {
            if (IsPickedUp) {
                IsPickedUp = false;
                if (_machineDetectionRay != null) {
                    _machineDetectionRay.QueueFree();
                    _machineDetectionRay = null;
                }
                return;
            }

            IsPickedUp = true;

            _machineDetectionRay = new() {
                GlobalPosition = _camera.GlobalPosition,
                CollideWithAreas = true,
            };

            _machineDetectionRay.SetCollisionMaskValue(1, false);
            _machineDetectionRay.SetCollisionMaskValue(5, true);

            AddChild(_machineDetectionRay);

            _zDelta = Mathf.Abs(_camera.GlobalPosition.Z - GlobalPosition.Z);
            _originalPosition = GlobalPosition;
        }
    }

    private Connectable ExtractConnectable(Area3D area) {

        if (!area.IsInGroup("MachineDetectionAreas")) {
            return null;
        }
        Connectable parent = area.GetParent() as Connectable;
        if (parent is null) {
            return null;
        }
        if (parent.Name != ConnecatableName) {
            return null;
        }
        return parent;
    }

    private void AssingSelfToMachine(Machine machine) { 
        foreach (MachineBuffer buffer in machine.Inputs()) {
            if (buffer.Resource == Resource) {
                buffer.Quantity += Quantity;
                IsPickedUp = false;
                QueueFree();
                return;
            }
        }
    }

    private void AssingSelfToContainer(StorageContainer container) { 
        if (container.Resource == Resource) {
            container.Quantity += Quantity;
            IsPickedUp = false;
            QueueFree();
            return;
        }
    }
}