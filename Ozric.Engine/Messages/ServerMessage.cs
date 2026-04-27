using JetBrains.Annotations;

namespace Ozric.Engine
{
    /// <summary>
    /// Base class for messages from the server, with a "type" that indicates the specific type.  
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class ServerMessage
    {
        public string type;

        public override string ToString()
        {
            return Json.Serialize(this, GetType());
        }
    }
}