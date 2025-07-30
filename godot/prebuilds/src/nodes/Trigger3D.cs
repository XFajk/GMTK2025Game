using Godot;
using System;
using Godot.Collections;

/// <summary>
/// A reusable 3D trigger zone that detects when a PhysicsBody3D enters the area,
/// emits a signal, and optionally disables itself after triggering or for a cooldown period.
/// </summary>
[GlobalClass]
public partial class Trigger3D : Area3D
{
    /// <summary>
    /// Emitted when a valid PhysicsBody3D enters the trigger area.
    /// </summary>
    [Signal]
    public delegate void TriggeredEventHandler(PhysicsBody3D body);

    /// <summary>
    /// A list of PhysicsBody3D nodes that are allowed to trigger this area.
    /// If empty, any body is allowed to trigger it.
    /// </summary>
    [Export]
    public Array<PhysicsBody3D> TriggerNodes;

    /// <summary>
    /// If true, this trigger will only activate once and then remove itself.
    /// </summary>
    [Export]
    public bool TriggerOnce = false;

    /// <summary>
    /// Time in seconds to wait before allowing the trigger to be activated again.
    /// A value of 0 disables the cooldown mechanism.
    /// </summary>
    [Export(PropertyHint.Range, "0, 10000")]
    public double RetriggerDurationSeconds = 1.0;

    private Timer _retriggerTimer = new Timer();

    public override void _Ready()
    {
        if (RetriggerDurationSeconds > 0.0)
        {
            _retriggerTimer.WaitTime = RetriggerDurationSeconds;
            _retriggerTimer.OneShot = true;
            _retriggerTimer.Timeout += OnTimeout;
            AddChild(_retriggerTimer, false, InternalMode.Back);
        }

        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D node)
    {
        PhysicsBody3D body = node as PhysicsBody3D;
        if (body == null)
            return;

        if (!TriggerNodes.Contains(body) && TriggerNodes.Count > 0)
            return;

        EmitSignalTriggered(body);

        if (TriggerOnce)
        {
            QueueFree();
        }
        else if (RetriggerDurationSeconds > 0.0)
        {
            BodyEntered -= OnBodyEntered;
            _retriggerTimer.Start();
        }
    }

    private void OnTimeout()
    {
        BodyEntered += OnBodyEntered;
    }
}
