using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace GrpcService
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override async Task StreamingBothWays(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            var readTask = Task.Run(async () =>
            {
                _logger.LogInformation($"Request Header:");
                foreach (var header in context.RequestHeaders)
                {
                    _logger.LogInformation($"  {header.Key} = {header.Value}");
                }

                await foreach (var message in requestStream.ReadAllAsync())
                {
                    await responseStream.WriteAsync(new HelloReply()
                    {
                        Message = "Echo Duplex " + message.Name,
                    });
                }
            });

            while (!readTask.IsCompleted)
            {
                await responseStream.WriteAsync(new HelloReply()
                {
                    Message = "Duplex",
                });
                await Task.Delay(TimeSpan.FromMilliseconds(500), context.CancellationToken);
            }
        }
    }
}
