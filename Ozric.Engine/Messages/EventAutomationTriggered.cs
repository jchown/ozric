namespace Ozric.Engine
{
    [TypeKey("automation_triggered")]
    public class AutomationTriggered: Event
    {
        public EventAutomationTriggeredData data { get; set; }
    }
}