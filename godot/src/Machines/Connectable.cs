using Godot;
using System;
using System.Collections.Generic;

/// machines and containers; things with resources that can be connected together
public abstract partial class Connectable : Node3D {
    [Signal]
    // Handled by Game.cs
    public delegate void OnConnectionClickEventHandler(Connectable machine, ConnectionNode point);
    [Signal]
    public delegate void OnHoverStartEventHandler(Connectable machine);
    [Signal]
    public delegate void OnHoverEndEventHandler(Connectable machine);

    [Export]
    public Connectable SharedConnectable = null;
    [Export]
    public bool IsPlayerConnectable = true;

    protected Area3D _hoverDetectionArea = null;
    protected StatusInterface _statusInterface = null;

    public static StandardMaterial3D HoverMaterial = GD.Load<StandardMaterial3D>("res://assets/3d/materials/outline.tres");
    public static StandardMaterial3D HoverBadMaterial = GD.Load<StandardMaterial3D>("res://assets/3d/materials/outline_bad.tres");
    public static StandardMaterial3D HoverGoodMaterial = GD.Load<StandardMaterial3D>("res://assets/3d/materials/outline_good.tres");

    public List<MeshInstance3D> OutlineMeshes = [];

    public override void _Ready() {

        _statusInterface = GetNodeOrNull<StatusInterface>("MachineStatusInterface");
        if (_statusInterface != null) _statusInterface.MachineNameLabel.Text = Name;

        _hoverDetectionArea = GetNode<Area3D>("HoverDetectionArea");

        _hoverDetectionArea.SetCollisionLayerValue(1, false);
        _hoverDetectionArea.SetCollisionMaskValue(1, false);

        _hoverDetectionArea.SetCollisionLayerValue(5, true);
        _hoverDetectionArea.SetCollisionMaskValue(5, true);

        _hoverDetectionArea.AddToGroup("MachineDetectionAreas");

        var meshNode = GetNodeOrNull<Node>("Mesh");
        if (meshNode != null) {
            // WARNING! this code here is recursive 
            // it goes and search the tree of a node for Nodes named Outline 
            // and recursion is the easiest and most readable way to do this 
            void FindOutlineMeshes(Node node) {
                if (node.Name == "Outline" && node is MeshInstance3D meshInstance) {
                    OutlineMeshes.Add(meshInstance);
                }
                foreach (Node child in node.GetChildren()) {
                    FindOutlineMeshes(child);
                }
            }
            FindOutlineMeshes(meshNode);
        }


        if (_hoverDetectionArea != null) {
            _hoverDetectionArea.MouseEntered += () => {
                if (_statusInterface != null) {
                    _statusInterface.Visible = true;
                    EmitSignalOnHoverStart(this);
                }
            };

            _hoverDetectionArea.MouseExited += () => {
                if (_statusInterface != null) {
                    _statusInterface.Visible = false;
                    EmitSignalOnHoverEnd(this);
                }
            };
        }

        foreach (Node child in GetChildren()) {
            if (child is ConnectionNode node) {
                node.InputEvent += (_, eventType, _, _, _) => {
                    if (eventType.IsActionPressed("interact")) {
                        EmitSignal(SignalName.OnConnectionClick, this, node);
                    }
                };

                GD.Print($"Added connection node {node.Name}");
            }
        }
    }

    public abstract IEnumerable<IContainer> Inputs();
    public abstract IEnumerable<IContainer> Outputs();

    public static bool CanConnect(Connectable a, Connectable b) {
        // a -> b
        foreach (IContainer output in a.Outputs()) {
            foreach (IContainer input in b.Inputs()) {
                if (output.GetResource() == input.GetResource()) return true;
            }
        }
        // b -> a
        foreach (IContainer output in b.Outputs()) {
            foreach (IContainer input in a.Inputs()) {
                if (output.GetResource() == input.GetResource()) return true;
            }
        }

        return false;
    }

    internal static Connection ConnectNodes(Connectable aMachine, ConnectionNode aNode, Connectable bMachine, ConnectionNode bNode) {
        ConnectionNode.ConnectNodes(aNode, bNode);
        return new(aMachine, aNode, bMachine, bNode);
    }

    public void ShowOutline(bool show, StandardMaterial3D material = null) {
        foreach (MeshInstance3D outlineMesh in OutlineMeshes) {
            outlineMesh.Visible = show;
            outlineMesh.MaterialOverride = show ? material : null;
        }
    }
}
