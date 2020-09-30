using EchoGrpcMagicOnion.Shared;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Hosting;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EchoGrpcMagicOnion
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            await MagicOnionHost.CreateDefaultBuilder()
                .UseMagicOnion()
                .RunConsoleAsync();
        }
    }

    public class HealthzService : ServiceBase<IHealthzService>, IHealthzService
    {
        public UnaryResult<int> Readiness()
        {
            return UnaryResult<int>(0);
        }
    }

    public class EchoService : ServiceBase<IEchoService>, IEchoService
    {
        public async UnaryResult<string> EchoAsync(string request)
        {
            Logger.Debug($"Handling Echo request '{request}' with context {Context}");
            var hostName = Environment.MachineName;
            var metadata = new Metadata();
            metadata.Add("hostname", hostName);
            await Context.CallContext.WriteResponseHeadersAsync(metadata);

            return request;
        }
    }

    public class MyHub : StreamingHubBase<IMyHub, IMyHubReceiver>, IMyHub
    {
        private Player player;
        private IGroup room;
        private IInMemoryStorage<Player> storage;

        public async Task<Player[]> JoinAsync(string roomName, string username)
        {
            Logger.Debug($"Handling Join request '{roomName}/{username}' with context {Context}");

            player = new Player
            {
                Name = username
            };

            (room, storage) = await Group.AddAsync(roomName, player);
            Broadcast(room).OnJoin(player);

            return storage.AllValues.ToArray();
        }

        public async Task SendAsync(string message)
        {
            Logger.Debug($"Handling Send request '{room.GroupName}/{player.Name}' with context {Context}");
            Broadcast(room).OnSend("message");
        }

        public async Task LeaveAsync()
        {
            Logger.Debug($"Handling Leave request '{room.GroupName}/{player.Name}' with context {Context}");

            var headers = Context.CallContext.RequestHeaders;
            if (!headers.Any(x => x.Key == "hostname"))
            {
                var hostName = Environment.MachineName;
                headers.Add("hostname", hostName);
            }

            await room.RemoveAsync(this.Context);
            Broadcast(room).OnLeave(player);
        }

        protected override async ValueTask OnConnecting()
        {
            Logger.Debug($"OnConnecting {Context.ContextId}");
            var metadata = new Metadata();
            var hostName = Environment.MachineName;
            metadata.Add("hostname", hostName);
            await Context.CallContext.WriteResponseHeadersAsync(metadata);
        }
        protected override ValueTask OnDisconnected()
        {
            Logger.Debug($"OnDisconnected {Context.ContextId}");
            return CompletedTask;
        }
    }
}
