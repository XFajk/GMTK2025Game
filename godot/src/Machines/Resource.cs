using System;
using System.Collections.Generic;
using System.Linq;

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
            case Resource.SolidWaste:
            case Resource.FluidWaste:
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

    public static bool IsLossless(IList<InputOutput> receipe) {
        Dictionary<ResourcePart, int> totals = new();

        // if all resource changes add up to 0, then we lose nothing
        foreach (InputOutput io in receipe) {
            var parts = GetParts(io.Resource);
            foreach (ResourcePart p in parts) {
                totals[p] += io.QuantityChangeInReceipe;
            }
        }

        return totals.Values.All(v => v == 0);
    }

    // gests
    private static List<ResourcePart> GetParts(Resource resource) {
        return resource switch {
            Resource.CoolantHot or Resource.CoolantCold => [ResourcePart.Coolant, .. Enumerable.Repeat(ResourcePart.Hydro, 4)],
            Resource.Humidity or Resource.Water => [ResourcePart.Hydro],
            Resource.Food => [ResourcePart.Hydro, ResourcePart.Carbon],
            Resource.FluidWaste => [ResourcePart.Hydro],
            Resource.SolidWaste => [ResourcePart.Carbon],
            Resource.Oxygen => [ResourcePart.Oxygen],
            Resource.CarbonDioxide => [ResourcePart.Carbon, ResourcePart.Oxygen],
            Resource.Disposables or Resource.Garbage => [.. Enumerable.Repeat(ResourcePart.Carbon, 8)],
            _ => throw new NotImplementedException()
        };
    }

    // every resource is composed of a combination of these atoms
    private enum ResourcePart {
        Carbon,
        Hydro,
        Oxygen,
        Coolant,
    }
}