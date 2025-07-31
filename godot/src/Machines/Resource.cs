public enum Resource {
    CoolantHot,
    CoolantCold,
    Humidity,
    Water,
    Food,
    FluidWaste,
    SolidWaste,
    Oxygen,
    CarbonDioxide,
    Disposables,
    Garbage
}

/// helper classes
public class Resources {
    /// returns true if this resource can flow without a connection
    public static bool IsFloating(Resource resource) => resource switch {
        Resource.Humidity or Resource.Oxygen or Resource.CarbonDioxide => true,
        _ => false,
    };
}