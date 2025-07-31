using Godot;
using System;
using System.Collections.Generic;

public partial class Game : Node {
    private Ship _shipNode;

    private Connectable _selectedMachine;
    private ConnectionNode _selectedNode;

    public override void _Ready() {
        _shipNode = GetNode<Ship>("Ship");
        // _Ready of child nodes will always be first
        foreach (Connectable connectable in _shipNode.Connectables) {
            connectable.OnConnectionClick += OnConnectionClick;
        }
    }
    
    private void OnConnectionClick(Connectable machine, ConnectionNode node) {
        if (_selectedMachine == null || _selectedMachine == machine) {
            _selectedMachine = machine;
            _selectedNode = node;
        } else {
            _shipNode.AddConnection(_selectedMachine, machine);
            _selectedMachine = null;
        }
    }
}
