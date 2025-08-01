using Godot;
using System;

public partial class ConnectionNode : Area3D {
    [Export]
    private Material _highlightMaterial;
    [Export]
    private Material _connectedMaterial;
    private Material _disconnectedMaterial;

    private MeshInstance3D _meshNode;
    private GpuParticles3D _particlesNode;
    public ConnectionNode ConnectedTo = null;
    public bool _isHovered = false;

    public override void _Ready() {
        _meshNode = GetNode<MeshInstance3D>("MeshInstance3D");
        _particlesNode = GetNode<GpuParticles3D>("GpuParticles3D");
        _disconnectedMaterial = _meshNode.GetSurfaceOverrideMaterial(0);

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
            _meshNode.SetSurfaceOverrideMaterial(0, _highlightMaterial);
        } else if (ConnectedTo != null) {
            _meshNode.SetSurfaceOverrideMaterial(0, _connectedMaterial);
        } else {
            _meshNode.SetSurfaceOverrideMaterial(0, _disconnectedMaterial);
        }
    }


    public static void ConnectNodes(ConnectionNode a, ConnectionNode b) {
        a.ConnectToNode(b);
        b.ConnectToNode(a);
    }

    private void ConnectToNode(ConnectionNode other) {
        if (ConnectedTo != null) {
            ConnectedTo.DisconnectNode();
        }
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
}
