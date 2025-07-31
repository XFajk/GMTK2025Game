using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

/// a connectable machine that processes receipes
public partial class Machine : Connectable {

    [Export]
    /// number of times per second a cycle is executed.
    /// Every cycle executes the changes defined by the inputs and outputs once
    private float _processingPerSecond = 1;
    
    /// the inputs and outputs of the receipe
    private List<InputOutput> _receipeParts = new();

    /// avoid rounding errors
    private float _processProgress = 0;

    public override IEnumerable<InputOutput> Inputs() => _receipeParts.Where(c => c.QuantityChangeInReceipe < 0);
    public override IEnumerable<InputOutput> Outputs() => _receipeParts.Where(c => c.QuantityChangeInReceipe > 0);

    public override void _Ready() {
        base._Ready();
        foreach (Node child in GetNode("Inputs").GetChildren()) {
            if (child is InputOutput input) {
                // we just add the inputs as a negative quantity change resulting from running the receipe
                input.QuantityChangeInReceipe *= -1;
                _receipeParts.Add(input);
            }
        }

        foreach (Node child in GetNode("Outputs").GetChildren()) {
            if (child is InputOutput output) {
                _receipeParts.Add(output);
            }
        }
    }

    public override void _Process(double deltaTime) {
        // check if all ingredients are present and enough output space available
        foreach (InputOutput container in _receipeParts) {
            if (!CanCycle(container)) {
                _processProgress = 0;
                return;
            }
        }

        // now run the machine
        _processProgress += _processingPerSecond * (float)deltaTime;

        while (_processProgress > 1) {
            _processProgress -= 1;

            foreach (InputOutput container in _receipeParts) {
                container.Quantity += container.QuantityChangeInReceipe;
            }

            // check again if we can continue to cycle
            foreach (InputOutput container in _receipeParts) {
                if (!CanCycle(container)) {
                    _processProgress = 0;
                    return;
                }
            }
        }
    }

    // returns true if we can execute the receipe at least once
    // returns false if we don't have ingredients or space
    private static bool CanCycle(InputOutput container) {
        int quantityAfterCycle = (int) container.Quantity + container.QuantityChangeInReceipe;
        return quantityAfterCycle >= 0 && quantityAfterCycle <= container.MaxQuantity;
    }
}
