using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using OzricEngine.messages;
using OzricEngine.Nodes;

namespace OzricEngine;

public class Home: OzricObject, IHome
{
    public override string Name => "Home";
    
    public Dictionary<string, EntityState> entityStates { get; set; } = new();

    [JsonIgnore]
    public LinkedList<EntityUpdate> entityUpdateHistory = new();

    private CancellationToken _cancelToken;
    
    private readonly IComms _comms;
    
    private List<EntityConfig> _entityConfigs;
    private List<AreaConfig> _areaConfigs;
    private List<DeviceConfig> _deviceConfigs;

    //  How recent an event must be to be assumed a response so an update 
        
    public const int SELF_EVENT_SECS = 10;

    //  We should definitely not spam
        
    private const double MIN_UPDATE_INTERVAL_SECS = 0.5;

    //  Throttling: Some devices have a "number of updates within a period" limit   
        
    private const int UPDATE_THROTTLE_PERIOD_SECS = 15;
    private const int UPDATE_THROTTLE_MAX_NUMBER = 5;

    //  How long after an "external" update we can take control back 
        
    private const int secondsToAllowOverrideByOthers = 10 * 60;

    public Home()
    {
    }

    
    public Home(IComms comms)
    {
        entityStates = new Dictionary<string, EntityState>();
        //areas = new Dictionary<string, Area>();
        //devices = new Dictionary<string, Device>();

        _comms = comms;
        _ = Task.Run(Listen);
    }
    
    void OnStatesReceived(ServerGetStates states)
    {
        if (states.success && states.result != null)
        {
            foreach (var state in states.result)
            {
                if (entityStates.ContainsKey(state.entity_id))
                {
                    entityStates[state.entity_id] = state;
                }
                else
                {
                    entityStates.Add(state.entity_id, state);
                }

                if (state.entity_id.StartsWith("light."))
                    state.LogLightState();
            }
        }
    }

    public Home(List<EntityState> stateList)
    {
        entityStates = new Dictionary<string, EntityState>();
        foreach (var state in stateList)
        {
            entityStates.Add(state.entity_id, state);

            if (state.entity_id.StartsWith("light."))
                state.LogLightState();
        }
    }

    private async Task Listen()
    {
        var messageTimeout = 5000;
        
        while (!_cancelToken.IsCancellationRequested)
        {
            try
            {
                //  Attempt to get the state of pretty much everything at the same time
                
                var entityListResult = await _comms.SendCommand<ServerConfigEntityList>(new ClientConfigEntityList(), messageTimeout);
                var areaListResult = await _comms.SendCommand<ServerConfigAreaList>(new ClientConfigAreaList(), messageTimeout);
                var deviceListResult = await _comms.SendCommand<ServerConfigDeviceList>(new ClientConfigDeviceList(), messageTimeout);

                OnConfigsReceived(entityListResult, areaListResult, deviceListResult);
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, e.Message);
            }

            try
            {
                var entityStateResult = await _comms.SendCommand<ServerGetStates>(new ClientGetStates(), messageTimeout);
                
                OnStatesReceived(entityStateResult);
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, e.Message);
            }
            
            await Task.Delay(30000, _cancelToken);
        }
    }

    private void OnConfigsReceived(ServerConfigEntityList entityListResult, ServerConfigAreaList areaListResult, ServerConfigDeviceList deviceListResult)
    {
        if (entityListResult.success)
        {
            _entityConfigs = entityListResult.result;
        }
        
        if (areaListResult.success)
        {
            _areaConfigs = areaListResult.result;
        }
        
        if (deviceListResult.success)
        {
            _deviceConfigs = deviceListResult.result;
        }
    }

    public EntityState? GetEntityState(string entityID)
    {
        return entityStates.GetValueOrDefault(entityID);
    }

    public virtual DateTime GetTime()
    {
        return DateTime.Now;
    }

    public List<EntityState> GetEntityStates()
    {
        return entityStates.Values.ToList();
    }

    public List<EntityState> GetEntityStates(List<string> entityIDs)
    {
        var selected = entityStates.Values.Where(es => entityIDs.Contains(es.entity_id)).ToList();
        selected.Sort((e1, e2) => string.CompareOrdinal(e1.entity_id, e2.entity_id));
        return selected;
    }

    public void SetUpdatedTime(string entityID)
    {
        var entityState = GetEntityState(entityID)!;
        var time = GetTime();

        entityState.last_updated = time;

        lock (entityUpdateHistory)
        {
            DiscardOldUpdates();
                
            entityUpdateHistory.AddFirst(new EntityUpdate(entityID, time));
        }
    }
        
    private void DiscardOldUpdates()
    {
        var obsolete = GetTime().AddMinutes(-10);

        lock (entityUpdateHistory)
        {
            while (entityUpdateHistory.Count > 0 && entityUpdateHistory.Last!.Value.when < obsolete)
                entityUpdateHistory.RemoveLast();
        }
    }

    /// <summary>
    /// Did we "recently" send an update to an entity?
    /// </summary>
    /// <param name="now"></param>
    /// <param name="secondsRecent"></param>
    /// <returns></returns>

    public bool WasRecentlyUpdatedByOzric(string entityID, double secondsRecent)
    {
        lock (entityUpdateHistory)
        {
            var lastUpdate = entityUpdateHistory.FirstOrDefault(u => u.entityID == entityID);
            if (lastUpdate == null)
                return false;

            return (GetTime() - lastUpdate.when).TotalSeconds < secondsRecent;
        }
    }
        
    /// <summary>
    /// How many times have we "recently" sent an update to an entity?
    /// </summary>
    /// <param name="now"></param>
    /// <param name="secondsRecent"></param>
    /// <returns></returns>

    public int GetNumberOfUpdatesByOzric(string entityID, int secondsRecent)
    {
        int num = 0;
        var since = GetTime().AddSeconds(-secondsRecent);

        lock (entityUpdateHistory)
        {
            foreach (var update in entityUpdateHistory)
            {
                if (update.when < since)
                    break;

                if (update.entityID != entityID)
                    continue;

                num++;
            }
        }

        return num;
    }

    public bool CanUpdateEntity(EntityState entityState)
    {
        //  Spamming some lights causes them to stop responding
            
        if (WasRecentlyUpdatedByOzric(entityState.entity_id, MIN_UPDATE_INTERVAL_SECS))
            return false;

        if (entityState.IsOverridden(GetTime(), secondsToAllowOverrideByOthers))
            return false;

        if (GetNumberOfUpdatesByOzric(entityState.entity_id, UPDATE_THROTTLE_PERIOD_SECS) > UPDATE_THROTTLE_MAX_NUMBER)
            return false;

        return true;
    }

    public bool OnEventStateChanged(EventStateChanged stateChanged)
    {
        var newState = stateChanged.data.new_state;

        var entityState = GetEntityState(newState.entity_id);
        if (entityState == null)
        {
            //  TODO: New device?
                
            return false;
        }

        lock (entityState)
        {
            if (entityState.entity_id.StartsWith("light."))
            {
                //  Check only the relevant details, ignoring timers etc.

                if (entityState.state == newState.state && entityState.attributes.EqualsKeys(newState.attributes, Light.ATTRIBUTE_KEYS))
                {
                    return false;
                }
            }

            var expected = Engine.IGNORE_OWN_STATE_CHANGES && WasRecentlyUpdatedByOzric(entityState.entity_id, Home.SELF_EVENT_SECS);
            if (!expected)
            {
                entityState.state = newState.state;
                entityState.attributes = newState.attributes;
                entityState.last_updated = GetTime();

                if (entityState.entity_id.StartsWith("light."))
                    entityState.LogLightState();
            }

            return !expected;
        }
    }

}

public record EntityUpdate(string entityID, DateTime when);