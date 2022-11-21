using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OzricEngine.Values;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricEngine.Nodes;

/// <summary>
/// Combines dawn & dusk times with the current weather to determine the overall light level. 1 = bright sunshine, 0 = darkness
/// </summary>
[TypeKey(NodeType.Weather)]
public class Weather: EntityNode
{
    public override NodeType nodeType => NodeType.Weather;
        
    public const string State = "state";
    public const string Temperature = "temperature";
    public const string Humidity = "humidity";
    public const string Pressure = "pressure";
    public const string WindBearing = "wind_bearing";
    public const string WindSpeed = "wind_speed";

    public Weather(string id) : base(id, "weather.home", null, new() { new(State, ValueType.Mode), new(Temperature, ValueType.Number), new(Humidity, ValueType.Number), new(Pressure, ValueType.Number), new(WindBearing, ValueType.Number), new(WindSpeed, ValueType.Number)})
    {
    }
         
    public override Task OnInit(Context context)
    {
        UpdateOutputs(context);
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        UpdateOutputs(context);
        return Task.CompletedTask;
    }

    private void UpdateOutputs(Context context)
    {
        var entity = context.home.GetEntityState(entityID);
        if (entity == null)
            return;

        SetOutputValue(State, new Mode(entity.state), context);
        SetOutputValue(Temperature, GetAttribute(entity, "temperature"), context);
        SetOutputValue(Humidity, GetAttribute(entity, "humidity"), context);
        SetOutputValue(Pressure, GetAttribute(entity, "pressure"), context);
        SetOutputValue(WindBearing, GetAttribute(entity, "wind_bearing"), context);
        SetOutputValue(WindSpeed, GetAttribute(entity, "wind_speed"), context);
    }

    private Value GetAttribute(EntityState entity, string key)
    {
        var value = entity.attributes.GetValueOrDefault(key);
        switch (value)
        {
            case double d:
                return new Number((float) d);
            
            case float f:
                return new Number(f);
            
            case int i:
                return new Number(i);
            
            default:
                return Number.ZERO;
        } 
    }
}