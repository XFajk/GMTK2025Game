using System;
using System.Collections.Generic;
using Godot;

/// Machines that draw their requirements by themselves, without requiring connections.
/// They do not have buffers
public partial class Process : Node3D {
    [Export]
    private bool _isLossless = true;

    [Export]
    /// number of times per second a cycle is executed.
    /// Every cycle executes the changes defined by the inputs and outputs once
    protected float _processingPerSecond = 1;

    [Export]
    /// processing time will be randomized each cycle by this fraction
    protected float _processingRandomness = 0.5f;
    private float _currentTargetProgress = 1;
    private RandomNumberGenerator _rng = new();

    /// the inputs and outputs of the recipe
    private List<InputOutput> _recipeParts = new();

    /// avoid rounding errors
    private float _processProgress = 0;

    public override void _Ready() {
        base._Ready();
        foreach (Node child in GetNode("Inputs").GetChildren()) {
            if (child is InputOutput input) {
                // we just add the inputs as a negative quantity change resulting from running the receipe
                input.QuantityChangeInReceipe *= -1;
                _recipeParts.Add(input);
            }
        }

        foreach (Node child in GetNode("Outputs").GetChildren()) {
            if (child is InputOutput output) {
                _recipeParts.Add(output);
            }
        }

        if (_isLossless) Resources.VerifyLossless(_recipeParts, Name);
    }

    public override void _Process(double deltaTime) {
        // check if all ingredients are present and enough output space available
        foreach (InputOutput io in _recipeParts) {
            if (!CanCycle(io)) {
                _processProgress = 0;
                return;
            }
        }

        // now run the machine
        _processProgress += _processingPerSecond * (float)deltaTime;

        while (_processProgress > _currentTargetProgress) {
            _processProgress -= 1;
            _currentTargetProgress = _rng.RandfRange(1 - _processingRandomness, 1 + _processingRandomness);

            foreach (InputOutput io in _recipeParts) {
                io.Container.AddQuantity(io.QuantityChangeInReceipe);
            }

            // check again if we can continue to cycle
            foreach (InputOutput io in _recipeParts) {
                if (!CanCycle(io)) {
                    _processProgress = 0;
                    return;
                }
            }
        }
    }

    // returns true if we can execute the receipe at least once
    // returns false if we don't have ingredients or space
    private static bool CanCycle(InputOutput io) {
        IContainer container = io.Container;
        int quantityAfterCycle = (int)container.GetQuantity() + io.QuantityChangeInReceipe;
        return quantityAfterCycle >= 0 && quantityAfterCycle <= container.GetMaxQuantity();
    }
}
