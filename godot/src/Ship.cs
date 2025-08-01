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

    public List<IEventEffect> ActiveEffects = new();

    private List<Connection> _connections = new();
    private Node _connectionsNode;

    public List<Floor> Floors; 
    public List<Person> Crew;
    private List<Person> _crewDoingTasks = new();

    private RandomNumberGenerator _rng = new();

    public override void _Ready() {
        _rng.Randomize();

        foreach (Node node in GetChildren()) {
            if (node is Machine machine) {
                Machines.Add(machine);
            }
            if (node is StorageContainer container) {
                Containers.Add(container);
            }
        }
        _connectionsNode = GetNode("Connections");

        _floatingResourceManager.Ready(Machines, GetNode("FloatingResources"));

        // initialize resource buffers
        foreach (IContainer buffer in AllContainers()) {
            if (_initialResources.TryGetValue(buffer.GetResource(), out float fraction)) {
                buffer.SetQuantity(buffer.GetMaxQuantity() * fraction);
                GD.Print($"Set {buffer.GetName()} to {buffer.GetQuantity()} {buffer.GetResource()}");
            }
        }

        Crew = GetTree().GetNodesInGroup("Crew").OfType<Person>().ToList();
        Floors = GetTree().GetNodesInGroup("Floors").OfType<Floor>().ToList();
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
            foreach (IContainer aOutput in connection.A.Outputs()) {
                foreach (IContainer bInput in connection.B.Inputs()) {
                    TryFlow(aOutput, bInput, (float)deltaTime);
                }
            }
            // flow b -> a
            foreach (IContainer bOutput in connection.B.Outputs()) {
                foreach (IContainer aInput in connection.A.Inputs()) {
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
            foreach (IContainer buffer in m.Inputs()) {
                yield return buffer;
            }
            foreach (IContainer buffer in m.Outputs()) {
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

    private void TryFlow(IContainer output, IContainer input, float deltaTime) {
        if (output.GetResource() == input.GetResource()) {
            float maxTransferQuantity = ConnectionTransferRate * deltaTime;
            // try pull as much as possible
            float quantityNotPulled = output.RemainderOfRemove(maxTransferQuantity);

            // try push what we got as much as possible
            float quantityNotPushed = input.RemainderOfAdd(maxTransferQuantity - quantityNotPulled);
            // put whatever we pulled but couldn't pull back
            output.AddQuantity(quantityNotPushed);

            float actualTransferQuantity = maxTransferQuantity - quantityNotPushed;
            if (actualTransferQuantity < 1E-3) return;

            GD.Print($"Transferred {actualTransferQuantity} units of {output.GetResource()} from {output.GetName()} to {input.GetName()}, bringing the buffer to {input.GetQuantity()}");
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

        // then check machine inputs or outputs
        if (quantity > 0) {
            foreach (Machine m in Machines) {
                foreach (IContainer buffer in m.Inputs()) {
                    leftToAdd = buffer.RemainderOfAdd(leftToAdd);
                    if (leftToAdd == 0) return;
                }
            }
        } else {
            // quantity < 0; only check outputs
            foreach (Machine m in Machines) {
                foreach (IContainer buffer in m.Outputs()) {
                    leftToAdd = buffer.RemainderOfAdd(leftToAdd);
                    if (leftToAdd == 0) return;
                }
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

    public void HireForTask(CrewTask task) {
        foreach (Person person in Crew) {
            if (!_crewDoingTasks.Contains(person)) {
                _crewDoingTasks.Add(person);
                person.SetTarget(ShipLocation.ClosesToPoint(task.Location, Floors));

                person.ReachedDestination += OnPersonReachedDestination;
                person.RecalculateTimer.WaitTime = task.Duration;
            }
        }
    }

    private void OnPersonReachedDestination(Person person) {
        _crewDoingTasks.Remove(person);
        person.ReachedDestination -= OnPersonReachedDestination;

        person.RecalculateTimer.Start();
    }
}
