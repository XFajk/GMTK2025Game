using Godot;
using System;

[GlobalClass]
public partial class Interacter : Node {
    /// <summary>
    /// Emitted once at the start of the interaction.
    /// </summary>
    [Signal]
    public delegate void StartOfInteractionEventHandler();

    /// <summary>
    /// Emitted every frame or physics tick while interacting.
    /// </summary>
    [Signal]
    public delegate void InteractionEventHandler();

    /// <summary>
    /// Emitted when the interaction stops.
    /// </summary>
    [Signal]
    public delegate void EndOfInteractionEventHandler();

    private bool _startGuard = false;
    private bool _endGuard = false;

    /// <summary>
    /// Determines if the interaction lifecycle logic is processed every physics tick or every frame.
    /// </summary>
    [Export]
    public bool PhysicsInteracter = false;

    public override void _Ready() {
        GetParent().AddToGroup("Interactable");
    }

    public override void _Process(double delta) {
        if (!PhysicsInteracter) {
            HandleGuards();
        }
    }

    public override void _PhysicsProcess(double delta) {
        if (PhysicsInteracter) {
            HandleGuards();
        }
    }

    /// <summary>
    /// Starts the interaction lifecycle and is intended to be called every frame of the interaction.
    /// </summary>
    public void Interact() {
        if (!_startGuard) {
            EmitSignalStartOfInteraction();
            _startGuard = true;
        }

        EmitSignalInteraction();
        _endGuard = true;
    }

    private void HandleGuards() {
        if (_startGuard && !_endGuard) {
            _startGuard = false;
            EmitSignalEndOfInteraction();
        } else {
            _endGuard = false;
        }
    }
}
