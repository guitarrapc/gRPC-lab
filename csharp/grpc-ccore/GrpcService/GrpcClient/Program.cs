using Grpc.Core;
using GrpcService;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            var host = Environment.GetEnvironmentVariable("GRPC_SERVER", EnvironmentVariableTarget.Process) ?? "127.0.0.1";
            // key1=value1,key2=value2,key3=value3
            var optionsString = Environment.GetEnvironmentVariable("GRPC_CHANNEL_OPTIONS", EnvironmentVariableTarget.Process);
            var options = GetOptions(optionsString);

            var r1 = RunAsync($"{host}:30051", "rc1", options);
            var r2 = RunAsync($"{host}:30051", "rc2", options);
            await Task.WhenAll(r1, r2);
        }

        static async Task RunAsync(string endpoint, string prefix, ChannelOption[] options)
        {
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

        private static ChannelOption[] GetOptions(string optionsString)
        {
            var options = string.IsNullOrEmpty(optionsString)
            ? new ChannelOption[] {
                // force gRPC client to open TCP Port for each channel
                // https://stackoverflow.com/questions/53564748/how-do-i-force-my-grpc-client-to-open-multiple-connections-to-the-server
                // new ChannelOption("grpc.use_local_subchannel_pool", 1),
            }
            : optionsString.Split(",")
            .Where(x => x.Contains('=') && x.Split('=').Length == 2)
            .Select(x =>
            {
                var kv = x.Split('=');
                var option = int.TryParse(kv[1], out var value)
                    ? new ChannelOption(kv[0], value)
                    : new ChannelOption(kv[0], kv[1]);
                return option;
            })
            .ToArray();

            DebugChannelOptions(options);

            return options;
        }

        private static void DebugChannelOptions(ChannelOption[] options)
        {
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
        }
    }
}
