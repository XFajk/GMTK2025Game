using Godot;
using System;
using System.Collections.Generic;

public partial class Pipes : Node {
    private List<Pipe> _pipes = new();

    public override void _Ready() {
        foreach (Node child in GetChildren()) {
            if (child is Pipe pipe) {
                _pipes.Add(pipe);
                pipe.Visible = false;
            }
        }
    }

    public Pipe GetPipe(Connectable a, Connectable b) {
        foreach (Pipe pipe in _pipes) {
            if ((pipe.A == a && pipe.B == b) || (pipe.A == b && pipe.B == a)) {
                return pipe;
            }
        }

        return null;
    }
}
