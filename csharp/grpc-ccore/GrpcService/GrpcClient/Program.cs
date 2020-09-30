using Grpc.Core;
using GrpcService;
using System;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Environment.GetEnvironmentVariable("GRPC_SERVER", EnvironmentVariableTarget.Process) ?? "127.0.0.1";
            var reuse = Environment.GetEnvironmentVariable("GRPC_SOREUSE", EnvironmentVariableTarget.Process);
            await Task.Delay(TimeSpan.FromSeconds(3));
            var r1 = RunAsync($"{host}:30051", "rc1", reuse);
            var r2 = RunAsync($"{host}:30051", "rc2", reuse);
            await Task.WhenAll(r1, r2);
        }

        static async Task RunAsync(string endpoint, string prefix, string reuse)
        {
            var options = int.TryParse(reuse, out var soReuse)
                ? new[] {
                    new ChannelOption(ChannelOptions.SoReuseport, soReuse),
                    new ChannelOption("grpc.enable_channelz", "false"),
                }
                : new ChannelOption[] {};
            Console.WriteLine($"Channel Options: {options.Length}");
            foreach (var option in options)
            {
                if (option.Type == ChannelOption.OptionType.String)
                {
                    Console.WriteLine($"{option.Name}: {option.StringValue}");
                }
                else
                {
                    Console.WriteLine($"{option.Name}: {option.IntValue}");
                }
            }

            var channel = new Grpc.Core.Channel(endpoint, ChannelCredentials.Insecure, options);
            var client = new Greeter.GreeterClient(channel);
            var user = "you";

            var reply = client.SayHello(new HelloRequest { Name = user });
            Console.WriteLine("Greeting: " + reply.Message);

            // duplex
            using var streaming = client.StreamingBothWays();
            var readTask = Task.Run(async () =>
            {
                while (await streaming.ResponseStream.MoveNext())
                {
                    var response = streaming.ResponseStream.Current;
                    Console.WriteLine($"{prefix} {response.Message}");
                }
            });

            var i = 0;
            while (i++ < 100)
            {
                try
                {
                    await streaming.RequestStream.WriteAsync(new HelloRequest
                    {
                        Name = $"{prefix} {i}",
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{prefix} error {channel.State} {ex.Message} {ex.StackTrace}");
                }
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            await streaming.RequestStream.CompleteAsync();
            await readTask;

            channel.ShutdownAsync().Wait();
        }
    }
}
