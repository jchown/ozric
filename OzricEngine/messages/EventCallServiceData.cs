using System;

namespace OzricEngine
{
    public class EventCallServiceData
    {
        public string domain { get; set; }
        public string service { get; set; }
        public Attributes service_data { get; set; }
        public string origin { get; set; }
        public DateTime time_fired { get; set; }
        public MessageContext context { get; set; }
        
        /// <summary>
        /// Does this data match the given command? 
        /// </summary>
        /// <param name="ccs"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>

        public bool OriginatedBy(ClientCallService ccs)
        {
            if (domain != ccs.domain)
                return false;
            
            if (service != ccs.service)
                return false;
            
            //  The entity ID(s) are in service_data in this event, but in the "target" in the client message

            if (!Attributes.AreSameList(service_data["entity_id"], ccs.target["entity_id"]))
                return false;

            if (!service_data.EqualsExcept(ccs.service_data, "entity_id"))
                return false;
            
            return true;
        }
    }
}