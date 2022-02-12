using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace OzricEngine
{
    public class MessageWriter
    {
        private readonly StreamWriter file;
        private readonly BufferBlock<Message> queue;

        struct Message
        {
            public readonly DateTime timestamp = DateTime.Now;
            public readonly string message;

            public Message(string message)
            {
                this.message = message;
            }
        }
        
        public MessageWriter(string filename)
        {
            file = new(filename, append: false);
            queue = new BufferBlock<Message>();
            _ = Task.Run(Writer);
        }

        public void Write(string message)
        {
            _ = queue.SendAsync(new Message(message));
        }

        async Task Writer()
        {
            while (true)
            {
                try
                {
                    var message = await queue.ReceiveAsync();
                    await file.WriteAsync($"{message.timestamp} - {message.message}\n");
                    await file.FlushAsync();
                }
                catch (InvalidOperationException)
                {
                    return;
                }
            }
        }

        public async Task Close(CancellationToken? cancellationToken = null)
        {
            queue.Complete();
            await queue.Completion;
            file.Close();
        }
    }
}