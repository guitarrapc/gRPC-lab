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
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;

namespace GreeterServer
{
    class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "Hello Server " + request.Name });
        }
    }

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            var credentials = GetServerCredentials(SslClientCertificateRequestType.RequestAndVerify);

            Server server = new Server
            {
                Services = { Greeter.BindService(new GreeterImpl()) },
                //Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
                Ports = { new ServerPort("localhost", Port, credentials) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }

        private static ServerCredentials GetServerCredentials(SslClientCertificateRequestType sslClientCertificateRequestType)
        {
            var rootCert = File.ReadAllText("keys/ca.crt");
            var serverCert = File.ReadAllText("keys/server.crt");
            var serverKey = File.ReadAllText("keys/server.key");

            var credentials = new SslServerCredentials(
                new[] { new KeyCertificatePair(serverCert, serverKey) },
                rootCert,
                sslClientCertificateRequestType
            );
            return credentials;
        }
    }
}
