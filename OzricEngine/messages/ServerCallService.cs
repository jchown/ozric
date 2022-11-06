using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OzricEngine
{
    /// <summary>
    /// Is this a dupe of EventCallService?
    /// </summary>
    [TypeKey("call_service")]
    public class ServerCallService : ServerMessage
    {
        public ServerCallService() { }

        public string domain;
        public string service;

        public Attributes service_data;
        public Attributes target;
    }
}