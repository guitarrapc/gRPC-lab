using EchoGrpcMagicOnion.Shared;
using Grpc.Core;
using Grpc.Net.Client;
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

            var endpoint = Environment.GetEnvironmentVariable("GRPC_SERVER", EnvironmentVariableTarget.Process) ?? "http://127.0.0.1:5000";

            var r1 = RunAsync(endpoint, "r1");
            //var r2 = RunAsync(endpoint, "r2");
            //await Task.WhenAll(r1, r2);
            await r1;

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task RunAsync(string endpoint, string prefix)
        {
            var channel = GrpcChannel.ForAddress(endpoint);
            var client = MagicOnionClient.Create<IEchoService>(channel);
            var reply = await client.EchoAsync("hogemoge");
            Console.WriteLine("Echo: " + reply);

            // duplex
            var receiver = new MyHubReceiver();
            var hubClient = await StreamingHubClient.ConnectAsync<IMyHub, IMyHubReceiver>(channel, receiver);
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
