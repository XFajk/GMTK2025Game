using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Ship : Node {
    [Export]
    public float ConnectionTransferRate = 10;

    public List<Machine> Machines;
    private List<Connection> _connectedMachines;

    public override void _Ready() {
        foreach (Node node in GetChildren()) {
            if (node is Machine machine) {
                Machines.Add(machine);
            }
        }
    }

    public override void _Process(double deltaTime) {
        foreach (Connection connection in _connectedMachines) {
            // flow a -> b
            foreach (InputOutput aOutput in connection.MachineA.Outputs) {
                foreach (InputOutput bInput in connection.MachineB.Inputs) {
                    TryFlow(aOutput, bInput, (float)deltaTime);
                }
            }
            // flow b -> a
            foreach (InputOutput bOutput in connection.MachineB.Outputs) {
                foreach (InputOutput aInput in connection.MachineA.Inputs) {
                    TryFlow(bOutput, aInput, (float)deltaTime);
                }
            }
        }
    }

    private void TryFlow(InputOutput output, InputOutput input, float deltaTime) {
        if (output.Resource == input.Resource) {
            float TransferQuantity = ConnectionTransferRate * deltaTime;

            if (output.Quantity > TransferQuantity && input.Quantity + TransferQuantity < input.MaxQuantity) {
                output.Quantity -= TransferQuantity;
                input.Quantity += TransferQuantity;
            }
        }
    }

    public void AddConnection(Machine a, Machine b) {
        _connectedMachines.Add(new Connection(a, b));
    }
}
