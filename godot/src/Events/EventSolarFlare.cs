using Godot;
using System;
using System.Collections.Generic;

public partial class EventSolarFlare : Node, IEvent {

    [Export(PropertyHint.Range, "0, 100, 5")]
    public int QuantityUsed;

    [Export(PropertyHint.ExpEasing)] 
    public float ConversionPerSecond = 1000;

    IEvent.Properties IEvent.GetProperties() => new() {
        Description = $"A solar flare hit our ship! The emergency cooling systems must use {Resources.ToUnit(Resource.CoolantCold, QuantityUsed)} coolant to dispose of the excess heat."
    };

    public void ApplyEffect(Ship ship) {
        // set an effect to make it simpler
        var ratioCoolant = Resources.GetRatio(Resource.CoolantCold, Resource.CoolantHot);
        var ratioHumidity = Resources.GetRatio(Resource.CoolantCold, Resource.Humidity);
        
        EventEffectResourceConvert _conversionEffect = new() {
            ConversionPerSecond = ConversionPerSecond,
            Conversion = [
                new InputOutput() {
                    QuantityChangeInReceipe = -(ratioCoolant.Key * ratioHumidity.Key),
                    Container = ship.GetContainer(Resource.CoolantCold),
                },
                new InputOutput() {
                    QuantityChangeInReceipe = ratioCoolant.Value,
                    Container = ship.GetContainer(Resource.CoolantHot),
                },
                new InputOutput() {
                    QuantityChangeInReceipe = ratioHumidity.Value,
                    Container = ship.GetContainer(Resource.Humidity),
                }
            ]
        };

        ship.ActiveEffects.Add(_conversionEffect);
    }
}
