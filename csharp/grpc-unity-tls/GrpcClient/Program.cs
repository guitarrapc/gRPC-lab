using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using GrpcService;
using System;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var r1 = RunAsync("http://localhost:5000", "r1");
            var r2 = RunAsync("http://localhost:5000", "r2");
            await Task.WhenAll(r1, r2);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task RunAsync(string endpoint, string prefix)
        {
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress(endpoint);

            // health
            var healthClient = new Health.HealthClient(channel);
            var health = await healthClient.CheckAsync(new HealthCheckRequest
            {
                Service = "",
            });
            Console.WriteLine($"Health Status: {health.Status}");

            // unary
            var client = new Greeter.GreeterClient(channel);
            var reply = await client.SayHelloAsync(new HelloRequest { Name = $"{prefix} GreeterClient" });
            Console.WriteLine("Greeting: " + reply.Message);

            // duplex
            var requestHeaders = new Metadata
            {
                { "x-host-port", "10-0-0-10" },
            };
            using var streaming = client.StreamingBothWays(requestHeaders);
            var readTask = Task.Run(async () =>
            {
                await foreach (var response in streaming.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine(response.Message);
                }
            });

            var i = 0;
            while (i++ < 100)
            {
                await streaming.RequestStream.WriteAsync(new HelloRequest
                {
                    Name = $"{prefix} {i}",
                });
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }

            await streaming.RequestStream.CompleteAsync();
            await readTask;
        }
    }
}
