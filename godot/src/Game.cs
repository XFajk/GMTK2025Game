using Godot;
using System;
using System.Collections.Generic;

public partial class Game : Control {
    private Ship _shipNode;
    private Node _connectionsNode;

    private Machine _selectedMachine;
    private List<EventScheduleEntry> _events;

    public override void _Ready() {
        _shipNode = GetNode<Ship>("Ship");
        // _Ready of child nodes will always be first
        foreach (Machine machine in _shipNode.Machines) {
            machine.OnConnectionClick += OnConnectionClick;
        }

        _connectionsNode = GetNode("Connections");

        Node eventSchedule = GetNode("EventSchedule");
        foreach (Node eventEntry in eventSchedule.GetChildren()) {
            if (eventEntry is EventScheduleEntry entry) {
                _events.Add(entry);
            }
        }
    }

    private void OnConnectionClick(Machine machine) {
        if (_selectedMachine == null) {
            _selectedMachine = machine;
        } else {
            _shipNode.AddConnection(_selectedMachine, machine);
            _selectedMachine = null;
        }
    }
}
