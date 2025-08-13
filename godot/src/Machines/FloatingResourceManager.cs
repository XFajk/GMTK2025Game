using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FloatingResourceManager {
    private Dictionary<FloatingResource, List<IContainer>> _inputs = new();
    private Dictionary<FloatingResource, List<IContainer>> _outputs = new();

    public IEnumerable<FloatingResource> AllResources() => _inputs.Keys.Concat(_outputs.Keys).Distinct();

    public void Ready(List<Machine> machines, Node floatingResourcesNode) {
        foreach (Node node in floatingResourcesNode.GetChildren()) {
            if (node is FloatingResource floater) {
                List<IContainer> inputs = new();
                List<IContainer> outputs = new();

                foreach (Machine machine in machines) {
                    foreach (MachineBuffer io in machine.Inputs()) {
                        if (io.Resource == floater.Resource) {
                            inputs.Add(io);
                        }
                    }
                    foreach (MachineBuffer io in machine.Outputs()) {
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
            List<IContainer> machineBuffers = output.Value;
            float freeSpaceInFloating = (floatingBuffer as IContainer).GetQuantityFree();
            float budgetPerOutput = freeSpaceInFloating / machineBuffers.Count;

            foreach (IContainer io in machineBuffers) {
                float quantityToMove = Mathf.Min(budgetPerOutput, io.GetQuantity());
                floatingBuffer.Quantity += quantityToMove;
                io.RemoveQuantity(quantityToMove);

                if (io.GetQuantity() < 0) {
                    GD.PrintErr($"{io.GetName()} {io.GetResource()} quantity negative ({io.GetQuantity()})");
                }
            }

            if (floatingBuffer.Quantity > floatingBuffer.MaxQuantity) {
                GD.PrintErr($"{floatingBuffer.GetName()} {floatingBuffer.GetResource()} quantity out of bounds ({floatingBuffer.Quantity} / {floatingBuffer.MaxQuantity})");
            }
        }

        // Every machine gets a budget to take.
        // This may cause machines to not be able to get all required resources even if there is enough
        // but only if the buffer is nearly empty and this is rather realistic
        foreach (var input in _inputs) {
            FloatingResource floatingBuffer = input.Key;
            List<IContainer> machineBuffers = input.Value;
            float budgetPerInput = floatingBuffer.Quantity / machineBuffers.Count;

            foreach (IContainer io in machineBuffers) {
                float quantityFree = io.GetQuantityFree();
                float quantityToMove = Mathf.Min(budgetPerInput, quantityFree);
                floatingBuffer.Quantity -= quantityToMove;
                io.AddQuantity(quantityToMove);

                if (io.GetQuantity() > io.GetMaxQuantity()) {
                    GD.PrintErr($"{io.GetName()} {io.GetResource()} quantity out of bounds ({io.GetQuantity()} / {io.GetMaxQuantity()})");
                }
            }

            if (floatingBuffer.Quantity < 0) {
                GD.PrintErr($"{floatingBuffer.GetName()} {floatingBuffer.GetResource()} quantity negative ({floatingBuffer.Quantity})");
            }
        }
    }
}
