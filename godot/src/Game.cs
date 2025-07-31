using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node {
    private Ship _shipNode;
    private FloatingResourceManager _floatingResourceManager = new();

    private Connectable _selectedMachine;
    private ConnectionNode _selectedNode;

    public override void _Ready() {
        _shipNode = GetNode<Ship>("Ship");

        _floatingResourceManager.Ready(_shipNode, GetNode("FloatingResources"));

        // _Ready of child nodes will always be first
        foreach (Connectable connectable in _shipNode.Connectables) {
            connectable.OnConnectionClick += OnConnectionClick;
        }
    }

    public override void _Process(double delta) {
        _floatingResourceManager.Process(delta);
    }


    private void OnConnectionClick(Connectable machine, ConnectionNode node) {
        if (_selectedMachine == null || _selectedMachine == machine) {
            _selectedMachine = machine;
            _selectedNode = node;
        } else {
            _shipNode.AddConnection(_selectedMachine, machine);
            _selectedMachine = null;
            _selectedNode = null;
        }

        string nameOfSelectedMachine = (_selectedMachine != null) ? _selectedMachine.Name : "null";
        string nameOfSelectedNode = (_selectedNode != null) ? _selectedNode.Name : "null";
        GD.Print($"_selectedMachine = {nameOfSelectedMachine}, _selectedNode = {nameOfSelectedNode}");
    }
}
