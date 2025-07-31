using System;

public enum Resource {
    CoolantHot,
    CoolantCold,
    Humidity,
    Water,
    Food,
    Waste,
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