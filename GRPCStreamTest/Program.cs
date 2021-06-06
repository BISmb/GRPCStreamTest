using Grpc.Net.Client;

using GRPCStreamTest.Client;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace GRPCStreamTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new Chat.ChatClient(channel);

            var stream = client.GetMessagesStream();

            CancellationTokenSource source = new CancellationTokenSource();
            var cancellationToken = source.Token;
            int i = 0;

            while (!cancellationToken.IsCancellationRequested && await stream.ResponseStream.MoveNext(cancellationToken))
            {
                var message = stream.ResponseStream.Current.Text;
                Console.WriteLine(message);

                i++;

                if ( i == 5)
                    source.Cancel();
            }


            //while (!client.CancellationToken.IsCancellationRequested)
            //{
            //await stream.ResponseStream.WriteAsync(new ServerToClientMessage
            //{
            //    Text = $"Server said hi {++pintCount} times",
            //    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            //});

            await Task.Delay(1000);
            //}

            //await foreach(var message in GRPCStreamTest.Client.)


            //GRPCStreamTest.Client.



        }
    }
}
