using System.Collections.Generic;
using Godot;

/// TODO
public class CrewMember { }

public interface IEvent {
    /// mechanical effecs of this events: changes to machine stockpile levels and crews
    void ApplyEffect(Ship ship);
}