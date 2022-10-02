using System;
using OzricEngine.Nodes;

namespace OzricEngine;

class SentCommand
{
    public readonly DateTime sent;
    public readonly ClientCommand command;
        
    public SentCommand(DateTime now, ClientCommand command)
    {
        sent = now;
        this.command = command;
    }

    public bool Expired(DateTime now)
    {
        return (now - sent).TotalSeconds > Home.SELF_EVENT_SECS;
    }
}