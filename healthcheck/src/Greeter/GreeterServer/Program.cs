// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcGreeter;
using GrpcHealth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreeterServer
{
    class GreeterImpl : Greeter.GreeterBase
    {
        readonly ILogger<GreeterImpl> logger;
        public GreeterImpl(ILogger<GreeterImpl> logger) => this.logger = logger;
        private static int count = 0;
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            logger.LogInformation($"request come: {count++}");
            return Task.FromResult(new HelloReply { Message = "Hello Server " + request.Name });
        }
    }
    class HealthImpl : GrpcHealth.Health.HealthBase
    {
        readonly object myLock = new object();
        readonly Dictionary<string, HealthCheckResponse.Types.ServingStatus> statusMap = new Dictionary<string, HealthCheckResponse.Types.ServingStatus>();

        readonly ILogger<HealthImpl> logger;
        public HealthImpl(ILogger<HealthImpl> logger) => this.logger = logger;

        public void SetStatus(string service, HealthCheckResponse.Types.ServingStatus status)
        {
            lock (myLock)
            {
                statusMap[service] = status;
            }
        }
        public void ClearStatus(string service)
        {
            lock(myLock)
            {
                statusMap.Remove(service);
            }
        }
        public void ClearAll()
        {
            lock (myLock)
            {
                statusMap.Clear();
            }
        }
        public override Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
        {
            //return base.Check(request, context);
            lock (myLock)
            {
                logger.LogInformation($"Check Health Status");
                var service = request.Service;
                HealthCheckResponse.Types.ServingStatus status;
                if (!statusMap.TryGetValue(service, out status))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, ""));
                }
                var res = global::Google.Protobuf.MessageExtensions.ToByteArray(new HealthCheckResponse { Status = status });
                var ns = global::Google.Protobuf.MessageExtensions.ToByteArray(new HealthCheckResponse { Status = HealthCheckResponse.Types.ServingStatus.NotServing });
                var su = global::Google.Protobuf.MessageExtensions.ToByteArray(new HealthCheckResponse { Status = HealthCheckResponse.Types.ServingStatus.ServiceUnknown });
                var s = global::Google.Protobuf.MessageExtensions.ToByteArray(new HealthCheckResponse { Status = HealthCheckResponse.Types.ServingStatus.Serving });
                var u = global::Google.Protobuf.MessageExtensions.ToByteArray(new HealthCheckResponse { Status = HealthCheckResponse.Types.ServingStatus.Unknown });
                var ub = new byte[] { }; // unknown
                var sb = new byte[] { 8, 1 }; // serving
                var nb = new byte[] { 8, 2 }; // notserving
                var sub = new byte[] { 8, 3 }; // service unknown
                return Task.FromResult(new HealthCheckResponse { Status = status });
            }
        }
    }

    class Program
    {
        const int Port = 50051;

        public static async Task Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());
            await new HostBuilder()
                .ConfigureLogging(logging => logging.AddSimpleConsole())
                .ConfigureServices((hostContext, services) =>
                {
                    // register grpc service implementation
                    services.AddSingleton<HealthImpl>();
                    services.AddSingleton<GreeterImpl>();

                    var provider = services.BuildServiceProvider();
                    Server server = new Server
                    {
                        Services = { Greeter.BindService(provider.GetService<GreeterImpl>()) },
                        Ports = { new ServerPort("0.0.0.0", Port, ServerCredentials.Insecure) }
                    };
                    RegisterHealthCheck(server, "Check", provider);
                    services.AddSingleton<Server>(server);
                    services.AddSingleton<IHostedService, GrpcHostedService>();
                })
                .RunConsoleAsync(); // SIGTERM, SIGKILL
        }

        private static void RegisterHealthCheck(Server server, string serviceName, ServiceProvider provider)
        {
            var healthImplementation = provider.GetService<HealthImpl>();
            healthImplementation.SetStatus(serviceName, HealthCheckResponse.Types.ServingStatus.Serving);
            server.Services.Add(Health.BindService(healthImplementation));
        }
    }

    public class GrpcHostedService : IHostedService
    {
        private Server server;
        private ILogger<GrpcHostedService> logger;
        public GrpcHostedService(Server server, ILogger<GrpcHostedService> logger)
        {
            this.server = server;
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Greeter server listening on port {string.Join(",", server.Ports)}");
            server.Start();
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // TODO: Gracefulshutdown
            await server.ShutdownAsync();
        }
    }

    public class SimpleConsoleLoggerProvider : ILoggerProvider
    {
        readonly SimpleConsoleLogger logger;

        public SimpleConsoleLoggerProvider()
        {
            logger = new SimpleConsoleLogger();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return logger;
        }

        public void Dispose()
        {
        }
    }

    public class SimpleConsoleLogger : ILogger
    {
        public SimpleConsoleLogger()
        {
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            var msg = formatter(state, exception);


            if (!string.IsNullOrEmpty(msg))
            {
                Console.WriteLine(msg);
            }

            if (exception != null)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        class NullDisposable : IDisposable
        {
            public static readonly IDisposable Instance = new NullDisposable();

            public void Dispose()
            {
            }
        }
    }

    public static class SimpleConsoleLoggerExtensions
    {
        public static ILoggingBuilder AddSimpleConsole(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SimpleConsoleLoggerProvider>());
            return builder;
        }
    }
}
