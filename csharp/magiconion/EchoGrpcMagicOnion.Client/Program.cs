using EchoGrpcMagicOnion.Shared;
using Grpc.Core;
using MagicOnion.Client;
using System;
using System.Threading.Tasks;

namespace EchoGrpcMagicOnion.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var r1 = RunAsync("localhost", 12345, "r1");
            var r2 = RunAsync("localhost", 12345, "r2");
            await Task.WhenAll(r1, r2);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task RunAsync(string endpoint, int port, string prefix)
        {
            var channel = new Channel(endpoint, port, ChannelCredentials.Insecure);
            var client = MagicOnionClient.Create<IEchoService>(channel);
            var reply = await client.EchoAsync("hogemoge");
            Console.WriteLine("Echo: " + reply);

            // duplex
            var receiver = new MyHubReceiver();
            var hubClient = StreamingHubClient.Connect<IMyHub, IMyHubReceiver>(channel, receiver);
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
