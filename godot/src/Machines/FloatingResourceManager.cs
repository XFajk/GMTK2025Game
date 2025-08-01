using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FloatingResourceManager {
    private Dictionary<FloatingResource, List<InputOutput>> _inputs = new();
    private Dictionary<FloatingResource, List<InputOutput>> _outputs = new();

    public IEnumerable<FloatingResource> Resources() => _inputs.Keys.Concat(_outputs.Keys).Distinct();

    public void Ready(List<Machine> machines, Node floatingResourcesNode) {
        foreach (Node node in floatingResourcesNode.GetChildren()) {
            if (node is FloatingResource floater) {
                List<InputOutput> inputs = new();
                List<InputOutput> outputs = new();

                foreach (Machine machine in machines) {
                    foreach (InputOutput io in machine.Inputs()) {
                        if (io.Resource == floater.Resource) {
                            inputs.Add(io);
                        }
                    }
                    foreach (InputOutput io in machine.Outputs()) {
                        if (io.Resource == floater.Resource) {
                            outputs.Add(io);
                        }
                    }
                }
                _inputs.Add(floater, inputs);
                _outputs.Add(floater, outputs);
            }
        }
    }

    /// we assume unlimited flow
    public void Process(double delta) {
        // Every machine gets a budget to fill.
        // This may cause machines to not be able to push their resources even if there is space
        // but only if the buffer is nearly full and this is rather realistic
        foreach (var output in _outputs) {
            FloatingResource floatingBuffer = output.Key;
            List<InputOutput> machineBuffers = output.Value;
            float freeSpaceInFloating = (floatingBuffer as IContainer).GetQuantityFree();
            float budgetPerOutput = freeSpaceInFloating / machineBuffers.Count;

            foreach (InputOutput io in machineBuffers) {
                float quantityToMove = Mathf.Min(budgetPerOutput, io.Quantity);
                floatingBuffer.Quantity += quantityToMove;
                io.Quantity -= quantityToMove;

                if (quantityToMove > 0) {
                    GD.Print($"Pulled {quantityToMove} {floatingBuffer.Resource} from {io.Name}");
                }

                if (floatingBuffer.Quantity > floatingBuffer.MaxQuantity || io.Quantity < 0) {
                    throw new Exception("quantities out of bounds");
                }
            }
        }

        // Every machine gets a budget to take.
        // This may cause machines to not be able to get all required resources even if there is enough
        // but only if the buffer is nearly empty and this is rather realistic
        foreach (var input in _inputs) {
            FloatingResource floatingBuffer = input.Key;
            List<InputOutput> machineBuffers = input.Value;
            float budgetPerInput = floatingBuffer.Quantity / machineBuffers.Count;

            foreach (InputOutput io in machineBuffers) {
                float quantityFree = (io as IContainer).GetQuantityFree();
                float quantityToMove = Mathf.Min(budgetPerInput, quantityFree);
                floatingBuffer.Quantity -= quantityToMove;
                io.Quantity += quantityToMove;

                if (quantityToMove > 0) {
                    GD.Print($"Pushed {quantityToMove} {floatingBuffer.Resource} to {io.Name}");
                }

                if (floatingBuffer.Quantity < 0 || io.Quantity > io.MaxQuantity) {
                    throw new Exception("quantities out of bounds");
                }
            }
        }
    }
}
