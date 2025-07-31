using System.Collections.Generic;
using Godot;

/// TODO
public class CrewMember { }

public interface IEvent {
    /// mechanical effecs of this events: changes to machine stockpile levels and crews
    public void ApplyEffect(List<Machine> machines, List<CrewMember> crew);
    
    
}