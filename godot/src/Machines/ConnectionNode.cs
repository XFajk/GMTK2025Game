using Godot;
using System;

public partial class ConnectionNode : Area3D {
    [Export]
    private Color _highlightColor;
    [Export]
    private Color _connectedColor;
    [Export]
    private Color _declineColor;
    [Export]
    private Color _defaultColor = Colors.Transparent;

    private GpuParticles3D _particlesNode;
    public ConnectionNode ConnectedTo = null;

    public override void _Ready() {
        _particlesNode = GetNode<GpuParticles3D>("GpuParticles3D");

        if (_particlesNode != null) {
            // explicity make our own copy of the material
            ParticleProcessMaterial processMaterial = (ParticleProcessMaterial)_particlesNode.ProcessMaterial;
            _particlesNode.ProcessMaterial = (Material)processMaterial.Duplicate();
        }
    }

    public static void ConnectNodes(ConnectionNode a, ConnectionNode b) {
        a.ConnectToNode(b);
        b.ConnectToNode(a);
    }

    public void DisconnectNode() {
        ConnectedTo?.DisconnectThisNode();
        DisconnectThisNode();
    }

    private void ConnectToNode(ConnectionNode other) {
        ConnectedTo?.DisconnectThisNode();

        ConnectedTo = other;

        Vector3 thisToOther = other.GlobalPosition - GlobalPosition;

        if (_particlesNode != null) {
            _particlesNode.Emitting = true;
            ParticleProcessMaterial processMaterial = (ParticleProcessMaterial)_particlesNode.ProcessMaterial;
            processMaterial.Direction = thisToOther.Normalized();
            processMaterial.InitialVelocityMin = thisToOther.Length();
            processMaterial.InitialVelocityMax = thisToOther.Length();
        }
    }

    private void DisconnectThisNode() {
        ConnectedTo = null;
        if (_particlesNode != null) _particlesNode.Emitting = false;
    }

    public void DeclineConnection() {
        // _activeTween = GetTree().CreateTween();
        // _activeTween.TweenProperty(_material, "albedo_color", oldColor, 1);
        // _activeTween.TweenCallback(Callable.From(() => _activeTween = null));
        // _activeTween.TweenCallback(Callable.From(UpdateMaterial));
    }
}
