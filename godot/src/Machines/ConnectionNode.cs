using Godot;
using System;

public partial class ConnectionNode : Area3D {
    [Export]
    private Color _highlightColor;
    [Export]
    private Color _connectedColor;
    [Export]
    private Color _declineColor;
    // not exported; use values of MeshInstance3D
    private Color _defaultColor;
    private StandardMaterial3D _material;

    private MeshInstance3D _meshNode;
    private GpuParticles3D _particlesNode;
    public ConnectionNode ConnectedTo = null;
    public bool _isHovered = false;

    private Tween _activeTween;

    public override void _Ready() {
        _meshNode = GetNode<MeshInstance3D>("MeshInstance3D");
        _particlesNode = GetNode<GpuParticles3D>("GpuParticles3D");

        // explicity make our own copy of the material
        _material = (StandardMaterial3D)_meshNode.GetSurfaceOverrideMaterial(0).Duplicate();
        _meshNode.SetSurfaceOverrideMaterial(0, _material);
        _defaultColor = _material.AlbedoColor;

        // explicity make our own copy of the material
        ParticleProcessMaterial processMaterial = (ParticleProcessMaterial)_particlesNode.ProcessMaterial;
        _particlesNode.ProcessMaterial = (Material)processMaterial.Duplicate();
    }

    public void OnMouseEntered() {
        _isHovered = true;
        UpdateMaterial();
    }

    public void OnMouseExited() {
        _isHovered = false;
        UpdateMaterial();
    }

    private void UpdateMaterial() {
        if (_isHovered) {
            if (_activeTween != null) {
                _material.AlbedoColor = _highlightColor;
            }
        } else if (ConnectedTo != null) {
            _activeTween?.Kill();
            _material.AlbedoColor = _connectedColor;
        } else {
            if (_activeTween != null) {
                _material.AlbedoColor = _defaultColor;
            }

        }
    }


    public static void ConnectNodes(ConnectionNode a, ConnectionNode b) {
        a.ConnectToNode(b);
        b.ConnectToNode(a);
    }

    private void ConnectToNode(ConnectionNode other) {
        ConnectedTo?.DisconnectNode();

        ConnectedTo = other;
        UpdateMaterial();

        Vector3 thisToOther = other.GlobalPosition - GlobalPosition;

        _particlesNode.Emitting = true;
        ParticleProcessMaterial processMaterial = (ParticleProcessMaterial)_particlesNode.ProcessMaterial;
        processMaterial.Direction = thisToOther.Normalized();
        processMaterial.InitialVelocityMin = thisToOther.Length();
        processMaterial.InitialVelocityMax = thisToOther.Length();
    }

    private void DisconnectNode() {
        ConnectedTo = null;
        _particlesNode.Emitting = false;
        UpdateMaterial();
    }

    public void DeclineConnection() {
        _material.AlbedoColor = _declineColor;
        _activeTween = GetTree().CreateTween();
        _activeTween.TweenProperty(_material, "albedo_color", _defaultColor, 1);
    }
}
