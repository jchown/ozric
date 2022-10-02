using System;
using OzricEngine.Nodes;

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
        return (now - sent).TotalSeconds > Home.SELF_EVENT_SECS;
    }
}