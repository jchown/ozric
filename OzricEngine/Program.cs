using System.Threading.Tasks;

namespace OzricEngine
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            using (var engine = new Engine())
            {
                await engine.Authenticate();
                
                await engine.Send(new ClientEventSubscribe());
                
                await engine.Receive<ServerEventSubscribeResult>();

                while (true)
                {
                    await engine.Receive<ServerEvent>();
                }
            }
        }
    }
}