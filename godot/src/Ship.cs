using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ship : Node {
    [Export]
    public float ConnectionTransferRate = 10;

    public List<Machine> Machines { get; private set; } = new();
    public List<StorageContainer> Containers { get; private set; } = new();
    /// all machines and containers
    public List<Connectable> Connectables => [.. Machines, .. Containers];

    private List<Connection> _connections = new();
    private Node _connectionsNode;

    public override void _Ready() {
        foreach (Node node in GetChildren()) {
            if (node is Machine machine) {
                Machines.Add(machine);
            }
            if (node is StorageContainer container) {
                Containers.Add(container);
            }
        }
        _connectionsNode = GetNode("Connections");
    }

    public override void _Process(double deltaTime) {
        foreach (Connection connection in _connections) {
            // flow a -> b
            foreach (InputOutput aOutput in connection.A.Outputs()) {
                foreach (InputOutput bInput in connection.B.Inputs()) {
                    TryFlow(aOutput, bInput, (float)deltaTime);
                }
            }
            // flow b -> a
            foreach (InputOutput bOutput in connection.B.Outputs()) {
                foreach (InputOutput aInput in connection.A.Inputs()) {
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

    public void AddConnection(Connectable a, Connectable b) {
        GD.Print($"Connected {a.Name} and {b.Name}");
        Connection connection = new(a, b);
        _connections.Add(connection);
        // TODO add connection node to _connectionsNode
    }
}
