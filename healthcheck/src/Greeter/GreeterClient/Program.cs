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
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcGreeter;
using GrpcHealth;
using MicroBatchFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreeterClient
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await BatchHost.CreateDefaultBuilder().RunBatchEngineAsync<GrpcClient>(args);
        }
    }

    public class GrpcClient : BatchBase
    {
        readonly IConfiguration config;
        public GrpcClient(IConfiguration config)
        {
            this.config = config;
        }

        public async Task Run()
        {
            var host = config.GetValue("GRPC_HOST", "localhost");
            var port = config.GetValue("GRPC_PORT", "50051");
            this.Context.Logger.LogInformation($"connect to the server. {host}:{port}");
            Channel channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);

            var client = new Greeter.GreeterClient(channel);
            String user = "you";
            var reply = client.SayHello(new HelloRequest { Name = user });
            this.Context.Logger.LogInformation("Greeting: " + reply.Message);

            this.Context.Logger.LogInformation($"Begin Health Check");
            var health = new Health.HealthClient(channel);
            for (var i = 1; i < 5000; i++)
            {
                var response = health.Check(new HealthCheckRequest
                {
                    Service = "Check"
                });
                this.Context.Logger.LogInformation($"Health Checked ({i}) {response.Status}");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            channel.ShutdownAsync().Wait();
        }
    }
}
