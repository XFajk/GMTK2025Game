using System;

public enum Resource {
    /// 1 coolant + 4 hydro
    CoolantHot,
    /// 1 coolant + 4 hydro
    CoolantCold,
    /// 1 hydro
    Humidity,
    /// 1 hydro
    Water,
    /// 1 hydro + 1 carbon
    Food,
    /// 1 hydro
    FluidWaste,
    /// 1 carbon
    SolidWaste,
    /// 1 oxygen
    Oxygen,
    /// 1 carbon + 2 oxygen
    CarbonDioxide,
    /// 8 carbon
    Disposables,
    /// 8 carbon
    Garbage
}

/// helper classes
public class Resources {
    /// returns true if this resource can flow without a connection
    public static bool IsFloating(Resource resource) => resource switch {
        Resource.Humidity or Resource.Oxygen or Resource.CarbonDioxide => true,
        _ => false,
    };

    public static string ToUnit(Resource resource, int quantity) {
        switch (resource) {
            case Resource.CoolantHot:
                return $"{quantity} l";
            case Resource.CoolantCold:
                return $"{quantity} l";
            case Resource.Humidity:
                return $"{quantity} %";
            case Resource.Water:
                return $"{quantity} l";
            case Resource.Food:
                return $"{quantity} kg";
            case Resource.Waste:
                return $"{quantity} kg";
            case Resource.Oxygen:
                return $"{quantity} l";
            case Resource.CarbonDioxide:
                return $"{quantity} l";
            case Resource.Disposables:
                return $"{quantity}";
            case Resource.Garbage:
                return $"{quantity}";
            default:
                throw new ArgumentOutOfRangeException(nameof(resource), resource, null);
        }
    }
}