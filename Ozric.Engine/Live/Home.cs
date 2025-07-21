using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ozric.Engine.Model;
using Ozric.Engine.Utils;
using OzricEngine;
using OzricEngine.messages;
using OzricEngine.Nodes;

namespace Ozric.Engine;

public class Home: OzricObject, IHome
{
    public override string Name => "Home";

    private readonly IComms _comms;

    private readonly Dictionary<string, EntityState> _entityStates = new();
    
    private List<EntityConfig> _entityConfigs;
    private List<AreaConfig> _areaConfigs;
    private List<DeviceConfig> _deviceConfigs;

    private readonly SemaphoreSlim _syncedSemaphore = new(0, 1);
    private readonly LinkedList<EntityUpdate> _entityUpdateHistory = new();
    private CancellationToken _cancelToken;

    private bool _hasData;

    //  How recent an event must be to be assumed a response so an update 
        
    public const int SelfEventSecs = 10;

    //  We should definitely not spam
        
    private const double MIN_UPDATE_INTERVAL_SECS = 0.5;

    //  Throttling: Some devices have a "number of updates within a period" limit   
        
    private const int UPDATE_THROTTLE_PERIOD_SECS = 15;
    private const int UPDATE_THROTTLE_MAX_NUMBER = 5;

    //  How long after an "external" update we can take control back 
        
    private const int SecondsToAllowOverrideByOthers = 10 * 60;
    
    private const int SyncMessageTimeoutMs = 5000;

    public Home(IComms comms, CancellationToken cancelToken = default)
    {
        _entityStates = new Dictionary<string, EntityState>();

        _comms = comms;
        _cancelToken = cancelToken;
       Tasks.Run(Sync, cancelToken);
    }

    public Home(List<EntityState> stateList, CancellationToken cancelToken = default)
    {
        _cancelToken = cancelToken;
        _entityStates = new Dictionary<string, EntityState>();
        
        foreach (var state in stateList)
        {
            _entityStates.Add(state.entity_id, state);

            if (state.entity_id.StartsWith("light."))
                state.LogLightState();
        }
    }
    
    public async Task WaitForEntities()
    {
        if (_hasData)
        {
            return;
        }
        
        Log(LogLevel.Info, "Waiting for entities to be synced...");

        await _syncedSemaphore.WaitAsync(_cancelToken);
    }

    private async Task Sync()
    {
        while (!_cancelToken.IsCancellationRequested)
        {
            try
            {
                await SyncConfigs();
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, e.Message);
            }

            try
            {
                await SyncEntityStates();
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, e.Message);
            }

            if (_hasData)
            {
                await Task.Delay(30000, _cancelToken);
            }
            else
            {
                await Task.Delay(1000, _cancelToken);
            }
        }
    }

    private async Task SyncEntityStates()
    {
        var entityStateResult = await _comms.SendCommand<ServerGetStates>(new ClientGetStates(), SyncMessageTimeoutMs);

        OnStatesReceived(entityStateResult);
    }

    private async Task SyncConfigs()
    {
        //  Attempt to get the state of pretty much everything at the same time
                
        var entityListResult = await _comms.SendCommand<ServerConfigEntityList>(new ClientConfigEntityList(), SyncMessageTimeoutMs);
        var areaListResult = await _comms.SendCommand<ServerConfigAreaList>(new ClientConfigAreaList(), SyncMessageTimeoutMs);
        var deviceListResult = await _comms.SendCommand<ServerConfigDeviceList>(new ClientConfigDeviceList(), SyncMessageTimeoutMs);

        OnConfigsReceived(entityListResult, areaListResult, deviceListResult);
    }
    
    void OnStatesReceived(ServerGetStates states)
    {
        if (states.success)
        {
            foreach (var state in states.result)
            {
                if (_entityStates.ContainsKey(state.entity_id))
                {
                    _entityStates[state.entity_id] = state;
                }
                else
                {
                    _entityStates.Add(state.entity_id, state);
                }

                if (state.entity_id.StartsWith("light."))
                    state.LogLightState();
            }
            
            var missing = _entityStates.Keys.Except(states.result.Select(s => s.entity_id)).ToList();
            if (missing.Any())
            {
                Log(LogLevel.Warning, "Missing entity states: {0}", string.Join(", ", missing));
            }
        }

        UpdateHasData();
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
        
        UpdateHasData();
    }

    private void UpdateHasData()
    {
        var hadData = _hasData;
        _hasData = _entityStates.Count > 0 && _entityConfigs.Count > 0 && _areaConfigs.Count > 0 && _deviceConfigs.Count > 0;

        if (!hadData && _hasData)
        {
            Log(LogLevel.Info, $"Home data received. {_entityStates.Count} entities in {_areaConfigs.Count} areas, via {_deviceConfigs.Count} devices");

            _syncedSemaphore.Release();
        }
    }

    public EntityState? GetEntityState(string entityID)
    {
        return _entityStates.GetValueOrDefault(entityID);
    }

    public virtual DateTime GetTime()
    {
        return DateTime.Now;
    }

    public List<EntityState> GetEntityStates()
    {
        return _entityStates.Values.ToList();
    }

    public List<AreaConfig> GetAreas()
    {
        var globalArea = _areaConfigs.FirstOrDefault(a => a.area_id == IHome.GlobalAreaId);
        if (globalArea == null)
        {
            globalArea = new AreaConfig
            {
                area_id = IHome.GlobalAreaId,
                name = "Home",
                icon = "mdi:home"
            };
            
            _areaConfigs.Insert(0, globalArea);
        }
        
        var colorsArea = _areaConfigs.FirstOrDefault(a => a.area_id == IHome.PaletteId);
        if (colorsArea == null)
        {
            colorsArea = new AreaConfig
            {
                area_id = IHome.PaletteId,
                name = "Colors",
                icon = "mdi:palette"
            };
            
            _areaConfigs.Insert(1, colorsArea);
        }
        
        return _areaConfigs;
    }

    public List<EntityState> GetEntityStates(List<string> entityIDs)
    {
        var selected = _entityStates.Values.Where(es => entityIDs.Contains(es.entity_id)).ToList();
        selected.Sort((e1, e2) => string.CompareOrdinal(e1.entity_id, e2.entity_id));
        return selected;
    }

    public void SetUpdatedTime(string entityID)
    {
        var entityState = GetEntityState(entityID)!;
        var time = GetTime();

        entityState.last_updated = time;

        lock (_entityUpdateHistory)
        {
            DiscardOldUpdates();
                
            _entityUpdateHistory.AddFirst(new EntityUpdate(entityID, time));
        }
    }
        
    private void DiscardOldUpdates()
    {
        var obsolete = GetTime().AddMinutes(-10);

        lock (_entityUpdateHistory)
        {
            while (_entityUpdateHistory.Count > 0 && _entityUpdateHistory.Last!.Value.when < obsolete)
                _entityUpdateHistory.RemoveLast();
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
        lock (_entityUpdateHistory)
        {
            var lastUpdate = _entityUpdateHistory.FirstOrDefault(u => u.entityID == entityID);
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

        lock (_entityUpdateHistory)
        {
            foreach (var update in _entityUpdateHistory)
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

        if (entityState.IsOverridden(GetTime(), SecondsToAllowOverrideByOthers))
            return false;

        if (GetNumberOfUpdatesByOzric(entityState.entity_id, UPDATE_THROTTLE_PERIOD_SECS) > UPDATE_THROTTLE_MAX_NUMBER)
            return false;

        return true;
    }

    public List<DeviceConfig> GetDevicesInArea(string areaId)
    {
        return _deviceConfigs.Where(d => d.area_id == areaId).ToList();
    }

    public List<EntityConfig> GetEntitiesInArea(string areaId)
    {
        return _entityConfigs.Where(e =>
        {
            var entityAreaId = e.area_id;
            if (entityAreaId == null && e.device_id != null)
            {
                var device = _deviceConfigs.FirstOrDefault(device => device.id == e.device_id);
                entityAreaId = device?.area_id;
            }
            return entityAreaId == areaId;
        }).ToList();
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

            var expected = Live.Engine.IGNORE_OWN_STATE_CHANGES && WasRecentlyUpdatedByOzric(entityState.entity_id, Home.SelfEventSecs);
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

    public ReadOnlyCollection<EntityConfig> GetEntityConfigs()
    {
        return _entityConfigs.AsReadOnly();
    }
    
    public ReadOnlyCollection<DeviceConfig> GetDeviceConfigs()
    {
        return _deviceConfigs.AsReadOnly();
    }
}

public record EntityUpdate(string entityID, DateTime when);