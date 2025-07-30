using Godot;
using System;

[GlobalClass]
public partial class InteractionRay3D : RayCast3D {

    [Export]
    public Sprite2D Crosshair = null;

    public Interacter StoredInteracter = null;

    public override void _Process(double _) {
        if (StoredInteracter == null) {
            if (Crosshair != null) Crosshair.Scale = Vector2.One * 0.1f;
            StoredInteracter = GetInteracter();
        }

        if (StoredInteracter != null) {
            if (Crosshair != null) Crosshair.Scale = Vector2.One * 0.2f;
            if (Input.IsActionPressed("interact")) {
                StoredInteracter.Interact();
            } else {
                StoredInteracter = null;
            }
        }
    }

    private Interacter GetInteracter() {
        if (!IsColliding()) {
            return null;
        }
        var o = GetCollider() as Node;
        if (!o.IsInGroup("Interactable")) {
            return null;
        }
        foreach (var child in o.GetChildren()) {
            if (child is Interacter interacter) {
                return interacter;
            }
        }
        return null;
    }
}
