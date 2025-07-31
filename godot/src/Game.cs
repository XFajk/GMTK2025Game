using Godot;
using System;
using System.Collections.Generic;

public partial class Game : Node {
    private Ship _shipNode;
    private Node _connectionsNode;

    private Connectable _selectedMachine;

    public override void _Ready() {
        _shipNode = GetNode<Ship>("Ship");
        // _Ready of child nodes will always be first
        foreach (Connectable machine in _shipNode.Connectables) {
            machine.OnConnectionClick += OnConnectionClick;
        }

        _connectionsNode = GetNode("Connections");
    }
    
    private void OnConnectionClick(Connectable machine, ConnectionNode point) {
        if (_selectedMachine == null) {
            _selectedMachine = machine;
        } else {
            _shipNode.AddConnection(_selectedMachine, machine);
            _selectedMachine = null;
        }
    }
}
