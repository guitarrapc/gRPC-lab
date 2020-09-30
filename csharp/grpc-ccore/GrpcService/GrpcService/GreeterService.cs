using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcService
{
    public class GreeterService : Greeter.GreeterBase
    {
        // avoid same time write exception "Only one write can be pending at a time."
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
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
                while(await requestStream.MoveNext())
                {
                    try
                    {
                        var current = requestStream.Current;
                        var message = "Echo Duplex " + current?.Name;
                        await _semaphore.WaitAsync();
                        try
                        {
                            await responseStream.WriteAsync(new HelloReply()
                            {
                                Message = message,
                            });
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }
                    catch (Exception e)
                    {
                        var debugInfo = new DebugInfo();
                        debugInfo.Detail = e.Message;
                        debugInfo.StackEntries.AddRange(e.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
                        context.ResponseTrailers.Add("debug-info-bin", debugInfo.ToByteArray());
                        Console.WriteLine(e);
                    }
                }
            });

            while (!readTask.IsCompleted)
            {
                await _semaphore.WaitAsync();
                try
                {
                    await responseStream.WriteAsync(new HelloReply()
                    {
                        Message = "Duplex",
                    });
                }
                finally
                {
                    _semaphore.Release();
                }
                await Task.Delay(TimeSpan.FromMilliseconds(500), context.CancellationToken);
            }
        }
    }

    public class DebugInfo
    {
        public string Detail { get; set; }
        public List<string> StackEntries { get; set; } = new List<string>();

        public byte[] ToByteArray()
        {
            IEnumerable<byte> bytes = Encoding.UTF8.GetBytes(Detail);
            foreach (var entry in StackEntries)
            {
                bytes = bytes.Concat(Encoding.UTF8.GetBytes(entry));
            }
            return bytes.ToArray();
        }
    }
}
