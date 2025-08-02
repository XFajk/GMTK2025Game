using Godot;
using System;
using System.Collections.Generic;

/// machines and containers; things with resources that can be connected together
public abstract partial class Connectable : Node3D {
    [Signal]
    // Handled by Game.cs
    public delegate void OnConnectionClickEventHandler(Connectable machine, ConnectionNode point);

    protected Area3D _hoverDetectionArea = null;
    protected StatusInterface _statusInterface = null;

    public override void _Ready() {

        _statusInterface = GetNodeOrNull<StatusInterface>("MachineStatusInterface");
        _hoverDetectionArea = GetNodeOrNull<Area3D>("HoverDetectionArea");
        if (_hoverDetectionArea != null) {
            _hoverDetectionArea.MouseEntered += () => {
                if (_statusInterface != null) {
                    _statusInterface.Visible = true;
                }
            };
            
            _hoverDetectionArea.MouseExited += () => {
                if (_statusInterface != null) {
                    _statusInterface.Visible = false;
                }
            };
        }

        foreach (Node child in GetNode("ConnectionNodes").GetChildren()) {
            if (child is ConnectionNode node) {
                node.InputEvent += (_, eventType, _, _, _) => {
                    if (eventType.IsActionPressed("interact")) {
                        EmitSignal(SignalName.OnConnectionClick, this, node);
                    }
                };

                GD.Print($"Added signal to {node.Name}");
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
}
