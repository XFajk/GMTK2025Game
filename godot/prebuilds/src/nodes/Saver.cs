using Godot;
using System;
using Godot.Collections;

/// <summary>
/// This Node saves requested properties of the parent node.
/// Used for saving the state of an object between game sessions tied to the SaveSystem.
/// Properties must be primitive types.
/// </summary>
/// 
[GlobalClass]
public partial class Saver : Node {
    /// <summary>
    /// List of property names to save from the parent node.
    /// These properties must be primitive types.
    /// </summary>
    [Export]
    public Array<StringName> PropertiesToSave { get; set; } = new();

    private Node _parent;

    /// <summary>
    /// Called when the node enters the scene tree.
    /// Adds this node to the "Savers" group.
    /// </summary>
    public override void _Ready() {
        _parent = GetParent();
        AddToGroup("Savers");
    }

    /// <summary>
    /// Saves all properties listed in <see cref="PropertiesToSave"/> from the parent node.
    /// </summary>
    /// <returns>A dictionary mapping property names to their string representations.</returns>
    public Dictionary<StringName, string> SaveOut() {
        var result = new Dictionary<StringName, string>();

        foreach (StringName property in PropertiesToSave) {
            result[property] = GD.VarToStr(_parent.Get(property));
        }

        GD.Print(result);
        return result;
    }

    /// <summary>
    /// Loads all properties from the given dictionary into the parent node.
    /// </summary>
    /// <param name="data">A dictionary mapping property names to their string representations.</param>
    public void LoadIn(Dictionary<StringName, string> data) {
        GD.Print(data);

        foreach (var (property, value) in data) {
            _parent.Set(property, GD.StrToVar(value));
        }
    }
}


