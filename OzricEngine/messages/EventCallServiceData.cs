using System;
using System.Runtime.Remoting.Contexts;

namespace OzricEngine
{
    public class EventCallServiceData
    {
        public string domain { get; set; }
        public string service { get; set; }
        public EventDataServiceData service_data { get; set; }
        public string origin { get; set; }
        public DateTime time_fired { get; set; }
        public StateContext context { get; set; }
    }
}