using Godot;

public class EventEffectResourceAdd : IEventEffect {
    // the target container that receives resources
    public IContainer Target;

    // to drain a resource, set this to negative
    public float AdditionPerSecond;

    // when draining a resource, set this to positive (max resources to drain)
    public float MaxResourcesToAdd = float.MaxValue;

    // when draining a resource, set this to positive (max resources to drain)
    public float TotalResourcesAdded;

    public void Process(double deltaTime) {
        if (Finished) return;

        float addition = AdditionPerSecond * (float)deltaTime;

        if (AdditionPerSecond > 0) {
            if (TotalResourcesAdded + addition > MaxResourcesToAdd) {
                addition = MaxResourcesToAdd - TotalResourcesAdded;
            }
        } else {
            if (TotalResourcesAdded - addition > MaxResourcesToAdd) {
                addition = -(MaxResourcesToAdd - TotalResourcesAdded);
            }
        }

        // if addition is negative, notAdded is negative as well
        float notAdded = Target.RemainderOfAdd(addition);
        // -x - -y = -(x - y)
        TotalResourcesAdded += Mathf.Abs(addition - notAdded);

    }

    public bool Finished => Mathf.Abs(TotalResourcesAdded) >= Mathf.Abs(MaxResourcesToAdd);
}