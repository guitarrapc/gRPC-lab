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
using System.Threading.Tasks;
using Grpc.Core;
using GrpcGreeter;
using GrpcHealth;

namespace GreeterServer
{
    class GreeterImpl : Greeter.GreeterBase
    {
        private static int count = 0;
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            Console.WriteLine($"request commint: {count++}");
            return Task.FromResult(new HelloReply { Message = "Hello Server " + request.Name });
        }
    }
    class HealthImpl : GrpcHealth.Health.HealthBase
    {
        readonly object myLock = new object();
        readonly Dictionary<string, HealthCheckResponse.Types.ServingStatus> statusMap = new Dictionary<string, HealthCheckResponse.Types.ServingStatus>();

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
                Console.WriteLine($"Check Health Status");
                var service = request.Service;
                HealthCheckResponse.Types.ServingStatus status;
                if (!statusMap.TryGetValue(service, out status))
                {
                    throw new RpcException(new Status(StatusCode.NotFound, ""));
                }
                return Task.FromResult(new HealthCheckResponse { Status = status });
            }
        }
    }

    class Program
    {
        const int Port = 50051;

        public static async Task Main(string[] args)
        {
            Server server = new Server
            {
                Services = { Greeter.BindService(new GreeterImpl()) },
                Ports = { new ServerPort("0.0.0.0", Port, ServerCredentials.Insecure) }
            };
            RegisterHealthCheck(server, "Check");
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
#if DEBUG
            Console.ReadKey();
#else
             await Task.Delay(TimeSpan.FromDays(1));
#endif

            server.ShutdownAsync().Wait();
        }

        private static void RegisterHealthCheck(Server server, string serviceName)
        {
            var healthImplementation = new HealthImpl();
            healthImplementation.SetStatus(serviceName, HealthCheckResponse.Types.ServingStatus.Serving);
            server.Services.Add(Health.BindService(healthImplementation));
        }
    }
}
