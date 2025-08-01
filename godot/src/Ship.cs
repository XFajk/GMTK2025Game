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

    public List<IEventEffect> ActiveEffects;

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
        foreach (IContainer buffer in AllContainers()) {
            if (_initialResources.TryGetValue(buffer.GetResource(), out float fraction)) {
                buffer.SetQuantity(buffer.GetMaxQuantity() * fraction);
                GD.Print($"Set {buffer.GetName()} to {buffer.GetQuantity()} {buffer.GetResource()}");
            }
        }
    }

    public FloatingResource GetFloatingResource(Resource resource) {
        foreach (var res in _floatingResourceManager.Resources()) {
            if (res.Resource == resource) return res;
        }
        return null;
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

        foreach (IEventEffect effect in ActiveEffects) {
            effect.Process(deltaTime);
        }
    }

    public IEnumerable<IContainer> AllContainers() {
        foreach (Machine m in Machines) {
            foreach (InputOutput buffer in m.Inputs()) {
                yield return buffer;
            }
            foreach (InputOutput buffer in m.Outputs()) {
                yield return buffer;
            }
        }

        foreach (StorageContainer c in Containers) {
            yield return c;
        }

        foreach (FloatingResource r in _floatingResourceManager.Resources()) {
            yield return r;
        }
    }

    public Dictionary<Resource, float> GetTotalResourceQuantities() {
        Dictionary<Resource, float> totals = new();
        foreach (IContainer buffer in AllContainers()) {
            float current = totals.GetValueOrDefault(buffer.GetResource(), 0);
            totals[buffer.GetResource()] = current + buffer.GetQuantity();
        }

        return totals;
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

    public void AddResource(Resource resource, int quantity) {
        float leftToAdd = quantity;

        // first check floating resources
        foreach (FloatingResource res in _floatingResourceManager.Resources()) {
            if (res.Resource == resource) {
                leftToAdd = (res as IContainer).RemainderOfAdd(leftToAdd);
                if (leftToAdd == 0) return;
            }
        }

        // then check storages
        foreach (StorageContainer container in Containers) {
            if (container.Resource == resource) {
                leftToAdd = (container as IContainer).RemainderOfAdd(leftToAdd);
                if (leftToAdd == 0) return;
            }
        }

        // then check machine inputs
        foreach (Machine m in Machines) {
            foreach (InputOutput buffer in m.Inputs()) {
                leftToAdd = (buffer as IContainer).RemainderOfAdd(leftToAdd);
                if (leftToAdd == 0) return;
            }
        }

        // otherwise it is lost
    }

    public void RemoveResource(Resource resource, int quantity) => AddResource(resource, -quantity);

    public void AddConnection(Connectable a, Connectable b) {
        GD.Print($"Connected {a.Name} and {b.Name}");
        Connection connection = new(a, b);
        _connections.Add(connection);
        // TODO add connection node to _connectionsNode
    }

}
