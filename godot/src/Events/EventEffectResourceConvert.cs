using Godot;

public class EventEffectResourceConvert : IEventEffect {

    public Ship Ship;
    public float ConversionPerSecond;
    
    public InputOutput[] Conversion;

    public float MaxCycles = float.MaxValue;

    private float _cyclesCompletedCount;

    public void Process(double deltaTime) {
        if (Finished) return;

        float fractionOfCycle = ConversionPerSecond * (float)deltaTime;

        if (_cyclesCompletedCount + fractionOfCycle > MaxCycles) {
            fractionOfCycle = MaxCycles - _cyclesCompletedCount;
        }

        foreach (var io in Conversion) {
            if (io.QuantityChangeInReceipe > 0) {
                float maxMultiplier = io.Container.GetQuantityFree() / io.QuantityChangeInReceipe;
                fractionOfCycle = Mathf.Min(fractionOfCycle, maxMultiplier);
            } else {
                float maxMultiplier = io.Container.GetQuantity() / -io.QuantityChangeInReceipe;
                fractionOfCycle = Mathf.Min(fractionOfCycle, maxMultiplier);
            }
        }

        foreach (var io in Conversion) {
            io.Container.AddQuantity(io.QuantityChangeInReceipe * fractionOfCycle);
        }

        _cyclesCompletedCount += fractionOfCycle;
    }

    public float TotalChangeOf(Resource resource) {
        foreach (var io in Conversion) {
            if (io.Container.GetResource() == resource) {
                return io.QuantityChangeInReceipe * _cyclesCompletedCount;
            }
        }
        return 0;
    }

    public bool Finished => Mathf.Abs(_cyclesCompletedCount) >= Mathf.Abs(MaxCycles);
}