using JetBrains.Annotations;

namespace OzricEngine
{
    /// <summary>
    /// Base class for messages from the server, with a "type" that indicates the specific type.  
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class ServerMessage
    {
        public string type { get; set; }
    }
}