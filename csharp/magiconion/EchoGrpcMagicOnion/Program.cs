using EchoGrpcMagicOnion.Shared;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
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
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(options =>
                    {
                        // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during development.
                        options.ConfigureEndpointDefaults(endpointOptions =>
                        {
                            endpointOptions.Protocols = HttpProtocols.Http2;
                        });
                    })
                    .UseStartup<Startup>();
                });
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(); // MagicOnion depends on ASP.NET Core gRPC service.
            services.AddMagicOnion();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMagicOnionService();
            });
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
        private readonly ILogger<EchoService> _logger;

        public EchoService(ILogger<EchoService> logger) => _logger = logger;
        public async UnaryResult<string> EchoAsync(string request)
        {
            _logger.LogDebug($"Handling Echo request '{request}' with context {Context}");
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
        private readonly ILogger<MyHub> _logger;

        public MyHub(ILogger<MyHub> logger) => _logger = logger;

        public async Task<Player[]> JoinAsync(string roomName, string username)
        {
            _logger.LogDebug($"Handling Join request '{roomName}/{username}' with context {Context}");

            player = new Player
            {
                Name = username
            };

            (room, storage) = await Group.AddAsync(roomName, player);
            Broadcast(room).OnJoin(player);

            return storage.AllValues.ToArray();
        }

        public Task SendAsync(string message)
        {
            _logger.LogDebug($"Handling Send request '{room.GroupName}/{player.Name}' with context {Context}");
            Broadcast(room).OnSend("message");
            return Task.CompletedTask;
        }

        public async Task LeaveAsync()
        {
            _logger.LogDebug($"Handling Leave request '{room.GroupName}/{player.Name}' with context {Context}");

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
            _logger.LogDebug($"OnConnecting {Context.ContextId}");
            var hostName = Environment.MachineName;
            var metadata = new Metadata.Entry("hostname", hostName);
            Context.CallContext.ResponseTrailers.Add(metadata);
        }
        protected override ValueTask OnDisconnected()
        {
            _logger.LogDebug($"OnDisconnected {Context.ContextId}");
            return CompletedTask;
        }
    }
}
