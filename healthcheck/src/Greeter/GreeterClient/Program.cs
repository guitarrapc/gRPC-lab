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

namespace GreeterClient
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("connect to the server");
            Channel channel = new Channel("localhost:50051", ChannelCredentials.Insecure);

            var client = new Greeter.GreeterClient(channel);
            String user = "you";
            var reply = client.SayHello(new HelloRequest { Name = user });
            Console.WriteLine("Greeting: " + reply.Message);

            var health = new Health.HealthClient(channel);
            for (var i = 0; i < 5000; i++)
            {
                Console.Write($"Health Check({i}):");
                var response = health.Check(new HealthCheckRequest
                {
                    Service = "Check"
                });
                Console.WriteLine($"Health Checked {response.Status}:");
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
