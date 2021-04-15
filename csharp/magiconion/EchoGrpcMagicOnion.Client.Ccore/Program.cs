using EchoGrpcMagicOnion.Shared;
using Grpc.Core;
using MagicOnion.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EchoGrpcMagicOnion.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            var host = Environment.GetEnvironmentVariable("GRPC_SERVER", EnvironmentVariableTarget.Process) ?? "127.0.0.1";
            // key1=value1,key2=value2,key3=value3
            var optionsString = Environment.GetEnvironmentVariable("GRPC_CHANNEL_OPTIONS", EnvironmentVariableTarget.Process);
            var options = GetOptions(optionsString);

            var r1 = RunAsync(host, 5000, "r1", options);
            var r2 = RunAsync(host, 5000, "r2", options);
            await Task.WhenAll(r1, r2);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task RunAsync(string endpoint, int port, string prefix, ChannelOption[] options)
        {
            var channel = new Channel(endpoint, port, ChannelCredentials.Insecure, options);
            var client = MagicOnionClient.Create<IEchoService>(new DefaultCallInvoker(channel));
            var reply = await client.EchoAsync("hogemoge");
            Console.WriteLine("Echo: " + reply);

            // duplex
            var receiver = new MyHubReceiver();
            var hubClient = await StreamingHubClient.ConnectAsync<IMyHub, IMyHubReceiver>(new DefaultCallInvoker(channel), receiver);
            var roomPlayers = await hubClient.JoinAsync($"room {prefix}", "hoge");
            foreach (var player in roomPlayers)
            {
                receiver.OnJoin(player);
            }

            var i = 0;
            while (i++ < 100)
            {
                await hubClient.SendAsync($"{prefix} {i}");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            await hubClient.LeaveAsync();

            await hubClient.DisposeAsync();
            await channel.ShutdownAsync();
        }

        private static ChannelOption[] GetOptions(string optionsString)
        {
            var options = string.IsNullOrEmpty(optionsString)
            ? new ChannelOption[] {
                // force gRPC client to open TCP Port for each channel
                // https://stackoverflow.com/questions/53564748/how-do-i-force-my-grpc-client-to-open-multiple-connections-to-the-server
                new ChannelOption("grpc.use_local_subchannel_pool", 1),
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

    public class MyHubReceiver : IMyHubReceiver
    {
        public void OnJoin(Player player)
        {
        }

        public void OnSend(string message)
        {
        }

        public void OnLeave(Player player)
        {
        }
    }
}
