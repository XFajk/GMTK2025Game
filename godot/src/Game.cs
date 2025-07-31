using Godot;
using System;
using System.Collections.Generic;

public partial class Game : Control {
    private Ship _shipNode;
    private Node _connectionsNode;

    private Machine _selectedMachine;
    private List<Connection> _connectedMachines;

    public override void _Ready() {
        _shipNode = GetNode<Ship>("Ship");
        // _Ready of child nodes will always be first
        foreach (Machine machine in _shipNode.Machines) {
            machine.OnConnectionClick += OnConnectionClick;
        }

        _connectionsNode = GetNode("Connections");
    }
    
    private void OnConnectionClick(Machine machine) {
        if (_selectedMachine == null) {
            _selectedMachine = machine;
        } else {
            Connection connection = new(_selectedMachine, machine);
            _connectedMachines.Add(connection);
            _selectedMachine = null;
        }
    }
}
