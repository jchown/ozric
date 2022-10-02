using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ValueType = OzricEngine.Values.ValueType;
using Boolean = OzricEngine.Values.Boolean;

namespace OzricEngine.Nodes;

[TypeKey(NodeType.Switch)]
public class Switch: EntityNode
{
    public override NodeType nodeType => NodeType.Switch;

    public const string INPUT_NAME = "state";

    public Switch(string id, string entityID) : base(id, entityID, new List<Pin> { new(INPUT_NAME, ValueType.Boolean) }, null)
    {
    }

    public override Task OnInit(Context context)
    {
        if (entityID == null)
        {
            Log(LogLevel.Error, $"Socket state {id}: Entity ID is null");
            return Task.CompletedTask;
        }
            
        UpdateValue(context);
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Context context)
    {
        UpdateValue(context);
        return Task.CompletedTask;
    }

    private void UpdateValue(Context context)
    {
        var input = GetInput(INPUT_NAME);
        if (input.value == null)
        {
            Log(LogLevel.Error, "{0} input {1} is not set", id, INPUT_NAME);
            return;
        }

        var entityState = context.home.GetEntityState(entityID)!;
        if (!context.home.CanUpdateEntity(entityState))
            return;
            
        bool on = (entityState.state == "on");
        Log(LogLevel.Debug, "{0}.on = {1}", entityID, on);

        bool desired = (input.value as Boolean)?.value ?? false;
            
        if (on != desired)
        {
            var newState = desired ? "on" : "off";
            Log(LogLevel.Info, "Turning {0}", newState);

            var callServices = new ClientCallService
            {
                domain = "switch",
                service = desired ? "turn_on" : "turn_off",
                target = new Attributes
                {
                    { "entity_id", new List<string> { entityID } }
                },
            };

            context.commands.Add(callServices, result =>
            {
                if (!result.success)
                {
                    Log(LogLevel.Warning, "Service call failed ({0}) - {1}",  result.error.code, result.error.message);
                    return;
                }
                    
                //  Success, record the state 

                entityState.state = newState;
            });
        }
    }
}
    
