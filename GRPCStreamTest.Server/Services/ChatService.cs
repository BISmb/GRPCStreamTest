using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GRPCStreamTest.Server.Services
{
    public class ChatService : GRPCStreamTest.Server.Chat.ChatBase
    {
        private readonly ILogger<ChatService> _logger;

        public ChatService(ILogger<ChatService> logger)
        {
            _logger = logger;
        }

        public override async Task SendMessage(
            IAsyncStreamReader<ClientToServerMessage> requestStream, 
            IServerStreamWriter<ServerToClientMessage> responseStream, 
            ServerCallContext context)
        {
            var clientHandleTask = ClientToServerPingAsync(requestStream, context);
            var serverHandleTask = ServerToClientPingAsync(responseStream, context);

            await Task.WhenAll(clientHandleTask, serverHandleTask);
        }

        public override async Task<ServerToClientMessage> GetMessages(
            IAsyncStreamReader<ServerToClientMessage> requestStream, 
            ServerCallContext context)
        {
            var messages = new ServerToClientMessage()
            {
                Text = "Message",
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
            };

            await Task.Delay(1000);
            return messages;
        }

        public override async Task GetMessagesStream(
            IAsyncStreamReader<ServerToClientMessage> requestStream, 
            IServerStreamWriter<ServerToClientMessage> responseStream, 
            ServerCallContext context)
        {
            for (int i = 0; i < 30; i++)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    break;


                await responseStream.WriteAsync(new ServerToClientMessage()
                {
                    Text = "Message",
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                });

                await Task.Delay(1000);
            }

            _logger.LogInformation("Requedt cancelled");

        }

        public async Task ClientToServerPingAsync(IAsyncStreamReader<ClientToServerMessage> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext() && context.CancellationToken.IsCancellationRequested)
            {
                var message = requestStream.Current;
                _logger.LogInformation("The Client said {Message}", message.Text);
            }
        }

        public async Task ServerToClientPingAsync(IServerStreamWriter<ServerToClientMessage> responseStream, ServerCallContext context)
        {
            var pintCount = 0;
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(new ServerToClientMessage
                {
                    Text = $"Server said hi {++pintCount} times",
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                });

                await Task.Delay(1000);
            }
        }
    }
}
