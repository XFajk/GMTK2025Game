using Godot;
using System;
using System.Collections.Generic;

/// machines and containers; things with resources that can be connected together
public abstract partial class Connectable : Node3D {
    [Signal]
    // Handled by Game.cs
    public delegate void OnConnectionClickEventHandler(Connectable machine, ConnectionNode point);

    [Export]
    public Connectable SharedWith = null;

    protected Area3D _hoverDetectionArea = null;

    protected StatusInterface _statusInterface = null;

    private Tween _highlightTween;
    private Node3D _visuals;

    public override void _Ready() {
        _statusInterface = GetNodeOrNull<StatusInterface>("MachineStatusInterface");
        if (_statusInterface != null) _statusInterface.MachineNameLabel.Text = Name;

        _hoverDetectionArea = GetNode<Area3D>("HoverDetectionArea");

        _visuals = GetNodeOrNull<Node3D>("Visuals");
        _visuals ??= SharedWith.GetNode<Node3D>("Visuals");

        _hoverDetectionArea.SetCollisionLayerValue(1, false);
        _hoverDetectionArea.SetCollisionMaskValue(1, false);

        _hoverDetectionArea.SetCollisionLayerValue(5, true);
        _hoverDetectionArea.SetCollisionMaskValue(5, true);

        _hoverDetectionArea.AddToGroup("MachineDetectionAreas");

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

        if (_hoverDetectionArea is ConnectionNode node) {
            _hoverDetectionArea.InputEvent += (_, eventType, _, _, _) => {
                if (eventType.IsActionPressed("interact")) {
                    EmitSignal(SignalName.OnConnectionClick, this, node);
                }
            };
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

    public static Connection ConnectNodes(Connectable aMachine, ConnectionNode aNode, Connectable bMachine, ConnectionNode bNode) {
        ConnectionNode.ConnectNodes(aNode, bNode);
        return new(aMachine, aNode, bMachine, bNode);
    }

    public void SetHighlight(HighlighType type) {
        PrepareNewHighlight();
        
        switch (type) {
            case HighlighType.Off:
                return;

            case HighlighType.Selected:
                _visuals.Visible = false;
                _highlightTween = GetTree().CreateTween().SetLoops();
                _highlightTween.TweenCallback(Callable.From(() => _visuals.Visible = true))
                            .SetDelay(0.1f);
                _highlightTween.TweenCallback(Callable.From(() => _visuals.Visible = false))
                            .SetDelay(0.3f);
                return;

            case HighlighType.ConectionRefused:
                // TODO

                // don't forget to return to Off later!
                _highlightTween = GetTree().CreateTween();
                _highlightTween.TweenCallback(Callable.From(() => SetHighlight(HighlighType.Off)))
                            .SetDelay(1f);
                return;

            case HighlighType.Hovered:
                // todo
                return;
        }
    }

    private void PrepareNewHighlight() {
        _highlightTween?.Kill();
        _visuals.Visible = true;
    }

    public enum HighlighType {
        Off,
        Hovered,
        Selected,
        ConectionRefused,
    }
}
