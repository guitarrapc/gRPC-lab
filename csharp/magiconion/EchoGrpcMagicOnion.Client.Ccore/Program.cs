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
                await Task.Delay(TimeSpan.FromSeconds(60));
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
                // If you want configure KEEP_ALIVE interval, then....
                // * keep `grpc.http2.min_ping_interval_without_data_ms < grpc.http2.min_time_between_pings_ms`
                // send keepalive ping every 10 second, default is 2 hours (server), unlimit (client).
                new ChannelOption("grpc.keepalive_time_ms", 10000),
                // keepalive ping time out after 10 seconds, default is 20 seoncds
                new ChannelOption("grpc.keepalive_timeout_ms", 10000),
                // allow unlimited amount of keepalive pings without data
                new ChannelOption("grpc.http2.max_pings_without_data", 0),
                // allow keepalive pings when there's no gRPC calls
                new ChannelOption("grpc.keepalive_permit_without_calls", 1),
                //Minimum allowed time between a server receiving successive ping frames without sending any data/header frame. every 5 seconds. default 5min (server), n/a (client)
                new ChannelOption("grpc.http2.min_ping_interval_without_data_ms", 5000),

                //// allow grpc pings from client every 30 seconds (deprecated, no meaning)
                //new ChannelOption("grpc.http2.min_time_between_pings_ms", 30000),
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
