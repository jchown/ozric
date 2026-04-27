using System.IO;
using System.Threading.Tasks;
using Ozric.Engine.Utils;
using Xunit;

namespace Ozric.Engine.Tests
{
    public class MessageWriterTests
    {
        [Fact]
        async Task TestWriting()
        {
            var filename = "test.txt";

            if (File.Exists(filename))
                File.Delete(filename);
            
            var writer = new MessageWriter(filename);
            for (int i = 0; i < 100; i++)
                writer.Write($"{i}");
            await writer.Close();

            var lines = File.ReadAllLines(filename);
            Assert.Equal(100, lines.Length);
            
            File.Delete(filename);
        }
    }
}