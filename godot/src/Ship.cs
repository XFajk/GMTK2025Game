using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ship : Node {
    [Export]
    public float ConnectionTransferRate = 10;

    [Export]
    private Godot.Collections.Dictionary<Resource, float> _initialResources;

    private FloatingResourceManager _floatingResourceManager = new();
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

        _floatingResourceManager.Ready(this, GetNode("FloatingResources"));

        // initialize resource buffers
        foreach (Machine m in Machines) {
            foreach (InputOutput buffer in m.Inputs()) {
                if (_initialResources.TryGetValue(buffer.Resource, out float fraction)) {
                    buffer.Quantity = buffer.MaxQuantity * fraction;
                    GD.Print($"Set buffer {buffer.Name} of {m.Name} to {buffer.Quantity} {buffer.Resource}");
                }
            }
        }
        foreach (StorageContainer c in Containers) {
            if (_initialResources.TryGetValue(c.Resource, out float fraction)) {
                c.Contents.Quantity = c.MaxQuantity * fraction;
                GD.Print($"Set {c.Name} to {c.Contents.Quantity} {c.Resource}");
            }
        }
        foreach (FloatingResource r in _floatingResourceManager.Resources()) {
            if (_initialResources.TryGetValue(r.Resource, out float fraction)) {
                r.Quantity = r.MaxQuantity * fraction;
                GD.Print($"Set {r.Resource} to {r.Quantity}");
            }
        }
    }

    public override void _Process(double deltaTime) {
        _floatingResourceManager.Process(deltaTime);

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

    public Dictionary<Resource, float> GetTotalResourceQuantities() {
        Dictionary<Resource, float> totals = new();
        foreach (Machine m in Machines) {
            foreach (InputOutput buffer in m.Inputs()) {
                float current = totals.GetValueOrDefault(buffer.Resource, 0);
                totals[buffer.Resource] = current + buffer.Quantity;
            }
            foreach (InputOutput buffer in m.Outputs()) {
                float current = totals.GetValueOrDefault(buffer.Resource, 0);
                totals[buffer.Resource] = current + buffer.Quantity;
            }
        }
        foreach (StorageContainer c in Containers) {
            float current = totals.GetValueOrDefault(c.Resource, 0);
            totals[c.Resource] = current + c.Contents.Quantity;
        }
        foreach (FloatingResource r in _floatingResourceManager.Resources()) {
            float current = totals.GetValueOrDefault(r.Resource, 0);
            totals[r.Resource] = current + r.Quantity;
        }

        return totals;
    }

    private void TryFlow(InputOutput output, InputOutput input, float deltaTime) {
        if (output.Resource == input.Resource) {
            float TransferQuantity = ConnectionTransferRate * deltaTime;

            if (output.Quantity > TransferQuantity && input.Quantity + TransferQuantity < input.MaxQuantity) {
                output.Quantity -= TransferQuantity;
                input.Quantity += TransferQuantity;

                GD.Print($"Transferred {TransferQuantity} from {input.Name} to {output.Name}");
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
