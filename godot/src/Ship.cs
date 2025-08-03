using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public partial class Ship : Node {
	[Export]
	public float ConnectionTransferRate = 10;

	[Export]
	private Godot.Collections.Dictionary<Resource, int> _initialResources;

	[Export]
	private int MaxConnectionCount = 1;

	private FloatingResourceManager _floatingResourceManager = new();
	public List<Machine> Machines { get; private set; } = new();
	public List<StorageContainer> Containers { get; private set; } = new();
	/// all machines and containers
	public List<Connectable> Connectables => [.. Machines, .. Containers];

	public List<IEventEffect> ActiveEffects = new();

	private Queue<Connection> _connections = new();
	private Node _connectionsNode;
	private Pipes _pipes;
	public List<Floor> Floors;
	public List<Person> Crew;
	private RandomNumberGenerator _rng = new();

	private List<Node> _unhandledEvents = new();

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
		_pipes = GetNode<Pipes>("Pipes");

		_floatingResourceManager.Ready(Machines, GetNode("FloatingResources"));

		// initialize resource buffers
		foreach (var entry in _initialResources) {
			AddResource(entry.Key, entry.Value);
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
			foreach (IContainer aOutput in connection.aMachine.Outputs()) {
				foreach (IContainer bInput in connection.bMachine.Inputs()) {
					TryFlow(aOutput, bInput, (float)deltaTime);
				}
			}
			// flow b -> a
			foreach (IContainer bOutput in connection.bMachine.Outputs()) {
				foreach (IContainer aInput in connection.aMachine.Inputs()) {
					TryFlow(bOutput, aInput, (float)deltaTime);
				}
			}
		}

		foreach (IEventEffect effect in ActiveEffects) {
			effect.Process(deltaTime);
		}
	}

	public enum Select {
		NoMachines,
		All,
		OnlyInputs,
		OnlyOutputs
	}

	public IEnumerable<IContainer> AllContainers(Select selection) {
		foreach (FloatingResource r in _floatingResourceManager.Resources()) {
			yield return r;
		}
		foreach (StorageContainer c in Containers) {
			yield return c;
		}
		if (selection != Select.NoMachines) {
			foreach (Machine m in Machines) {
				if (selection != Select.OnlyOutputs) {
					foreach (IContainer buffer in m.Inputs()) {
						yield return buffer;
					}
				}

				if (selection != Select.OnlyInputs) {
					foreach (IContainer buffer in m.Outputs()) {
						yield return buffer;
					}
				}
			}
		}
	}

	public Dictionary<Resource, float> GetTotalResourceQuantities() {
		Dictionary<Resource, float> totals = new();
		foreach (IContainer buffer in AllContainers(Select.All)) {
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

			// GD.Print($"Transferred {actualTransferQuantity} units of {output.GetResource()} from {output.GetName()} to {input.GetName()}, bringing the buffer to {input.GetQuantity()}");
		}
	}

	public float AddResource(Resource resource, float quantity) {
		float leftToAdd = quantity;

		Select selection = (quantity > 0) ? Select.OnlyInputs : Select.OnlyOutputs;
		foreach (IContainer c in AllContainers(selection)) {
			if (c.GetResource() == resource) {
				leftToAdd = c.RemainderOfAdd(leftToAdd);
				if (leftToAdd == 0) return 0;
			}
		}

		return leftToAdd;
	}

	public float RemoveResource(Resource resource, float quantity) => AddResource(resource, -quantity);

	public bool HasResource(Resource resource, int quantity) {
		float leftToGet = quantity;

		foreach (IContainer c in AllContainers(Select.OnlyOutputs)) {
			if (c.GetResource() == resource) {
				leftToGet -= c.GetQuantity();
				if (leftToGet <= 0) return true;
			}
		}

		return false;
	}

	public Connection AddConnection(Connection connection) {
		Connection toRemove = null;
		if (_connections.Count == MaxConnectionCount) {
			toRemove = _connections.Dequeue();
			_pipes.GetPipe(toRemove.aMachine, toRemove.bMachine).Visible = false;
		}

		_connections.Enqueue(connection);
		_pipes.GetPipe(connection.aMachine, connection.bMachine).Visible = true;

		return toRemove;
	}

	// previously HireForTask
	// if crewMember is null, we find one that is available
	public void ScheduleCrewTask(CrewTask task, Person crewMember = null) {
		ShipLocation location = ShipLocation.ClosesToPoint(task.Location, Floors);

		// if not set, pick one from the same floor
		crewMember ??= GetClosestCrew(task.Location, crewMember, location.Floor);
		// still nothing? anyone will do then
		crewMember ??= GetClosestCrew(task.Location, crewMember);

		crewMember.SetCurrentTask(task, location);
	}

	private Person GetClosestCrew(Vector3 position, Person crewMember, int? floor = null) {
		float leastDistance = float.MaxValue;
		foreach (Person person in Crew) {
			if (floor != null && person.FloorNumber != floor) continue;

			float distance = person.Position.DistanceSquaredTo(position);
			if (distance < leastDistance) {
				leastDistance = distance;
				crewMember = person;
			}
		}

		return crewMember;
	}


	public void ScheduleEvent(Node mission) {
		_unhandledEvents.Add(mission);
	}

	public Node TryTakeEvent() {
		if (_unhandledEvents.Count == 0) return null;
		Node mission = _unhandledEvents.First();
		_unhandledEvents.RemoveAt(0);
		return mission;
	}

	public IContainer GetContainer(Resource resource, Select selection = Select.NoMachines) {
		foreach (IContainer c in AllContainers(selection)) {
			if (c.GetResource() == resource) {
				return c;
			}
		}

		throw new ArgumentOutOfRangeException(nameof(resource), resource, $"No {resource} container found for selection {selection}");
	}

	public bool CanConnect(Connectable a, Connectable b) => _pipes.GetPipe(a, b) != null;
}
