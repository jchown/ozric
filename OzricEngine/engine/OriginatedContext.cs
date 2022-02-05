using System;

namespace OzricEngine;

class OriginatedContext
{
    public readonly DateTime sent;
    public readonly EventCallService callService;
        
    public OriginatedContext(DateTime now, EventCallService callService)
    {
        sent = now;
        this.callService = callService;
    }

    public bool Expired(DateTime now)
    {
        return (now - sent).TotalSeconds > Engine.SELF_EVENT_SECS;
    }
}