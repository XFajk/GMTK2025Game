using System.Collections.Generic;
using Godot;

public interface IEvent {
    /// mechanical effecs of this events: changes to machine stockpile levels and crews
    void ApplyEffect(Ship ship);

    Properties GetProperties();

    bool DoPanic();

    public class Properties {
        public string Description;
        public Vector3 IconPosition;
    }
}