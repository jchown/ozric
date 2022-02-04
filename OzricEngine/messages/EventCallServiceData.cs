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
        public StateContext context { get; set; }
        
        /// <summary>
        /// Does this data match the given command? 
        /// </summary>
        /// <param name="ccs"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>

        public bool DueTo(ClientCallService ccs)
        {
            if (domain != ccs.domain)
                return false;
            
            if (service != ccs.service)
                return false;

            if (service_data != ccs.service_data)
                return false;
            
            return true;
        }
    }
}