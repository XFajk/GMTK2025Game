using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FloatingResourceManager {
    private Dictionary<FloatingResource, List<InputOutput>> _floatingInputs = new();
    private Dictionary<FloatingResource, List<InputOutput>> _floatingOutputs = new();

    public IEnumerable<FloatingResource> Resources() => _floatingInputs.Keys.Concat(_floatingOutputs.Keys).Distinct();

    public void Ready(Ship shipNode, Node floatingResourcesNode) {
        foreach (Node node in floatingResourcesNode.GetChildren()) {
            if (node is FloatingResource floater) {
                List<InputOutput> inputs = new();
                List<InputOutput> outputs = new();

                foreach (Machine machine in shipNode.Machines) {
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
                _floatingInputs.Add(floater, inputs);
                _floatingOutputs.Add(floater, outputs);
            }
        }
    }

    /// we assume unlimited flow
    public void Process(double delta) {
        // Every machine gets a budget to fill.
        // This may cause machines to not be able to push their resources even if there is space
        // but only if the buffer is nearly full and this is rather realistic
        foreach (var output in _floatingOutputs) {
            FloatingResource resource = output.Key;
            List<InputOutput> machineBuffers = output.Value;
            float budgetPerOutput = resource.Quantity / machineBuffers.Count;

            foreach (InputOutput io in machineBuffers) {
                float quantityToMove = Mathf.Min(budgetPerOutput, io.Quantity);
                resource.Quantity += quantityToMove;
                io.Quantity -= quantityToMove;

                if (resource.Quantity > resource.MaxQuantity || io.Quantity < 0) {
                    throw new Exception("quantities out of bounds");
                }
            }
        }

        // Every machine gets a budget to take.
        // This may cause machines to not be able to get all required resources even if there is enough
        // but only if the buffer is nearly empty and this is rather realistic
        foreach (var input in _floatingInputs) {
            FloatingResource resource = input.Key;
            List<InputOutput> machineBuffers = input.Value;
            float budgetPerInput = resource.Quantity / machineBuffers.Count;

            foreach (InputOutput io in machineBuffers) {
                float quantityFree = (io.MaxQuantity - io.Quantity);
                float quantityToMove = Mathf.Min(budgetPerInput, quantityFree);
                resource.Quantity -= quantityToMove;
                io.Quantity += quantityToMove;

                if (resource.Quantity < 0 || io.Quantity > io.MaxQuantity) {
                    throw new Exception("quantities out of bounds");
                }
            }
        }
    }
}
