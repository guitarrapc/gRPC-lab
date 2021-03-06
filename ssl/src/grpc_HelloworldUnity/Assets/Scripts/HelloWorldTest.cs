#region Copyright notice and license

// Copyright 2019 The gRPC Authors
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

#endregion

using UnityEngine;
using System.Threading.Tasks;
using System;
using Grpc.Core;
using Helloworld;
using System.IO;

class HelloWorldTest
{
    // Can be run from commandline.
    // Example command:
    // "/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -executeMethod HelloWorldTest.RunHelloWorld -logfile"
    public static void RunHelloWorld()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        Debug.Log("==============================================================");
        Debug.Log("Starting tests");
        Debug.Log("==============================================================");

        Debug.Log("Application.platform: " + Application.platform);
        Debug.Log("Environment.OSVersion: " + Environment.OSVersion);

        var reply = Greet("Unity");
        Debug.Log("Greeting: " + reply.Message);

        Debug.Log("==============================================================");
        Debug.Log("Tests finished successfully.");
        Debug.Log("==============================================================");
    }

    public static HelloReply Greet(string greeting)
    {
        const int Port = 50051;
        //var server = StarServer(Port);

        //Channel channel = new Channel($"127.0.0.1:{Port}", ChannelCredentials.Insecure);
        //var credentials = GetClientCredentials();
        var credentials = GetRootCertificateCredentials();
        Channel channel = new Channel($"dummy.example.com:{Port}", credentials);
        var client = new Greeter.GreeterClient(channel);
        var reply = client.SayHello(new HelloRequest { Name = greeting });

        channel.ShutdownAsync().Wait();

        //server.ShutdownAsync().Wait();

        return reply;
    }

    private static ChannelCredentials GetClientCredentials()
    {
        var rootCert = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "ca.crt"));
        var clientCert = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "client.crt"));
        var clientKey = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "client.key"));
        var credentials = new SslCredentials(rootCert, new KeyCertificatePair(clientCert, clientKey));
        return credentials;
    }

    private static ChannelCredentials GetRootCertificateCredentials()
    {
        var rootCert = "";
        var assetPath = UnityEngine.Application.streamingAssetsPath;
        var path = Path.Combine(assetPath, "ca.crt");
#if UNITY_IOS
        using (var streamReader = new StreamReader(path))
        {
            rootCert = streamReader.ReadToEnd();
        }
#elif UNITY_ANDROID
        var www = new WWW(path);
        while (!www.isDone) {}

        using (var txtReader = new StringReader(www.text))
        {
            rootCert = txtReader.ReadToEnd();
        }
#else
        rootCert = File.ReadAllText(path);
#endif

        var credentials = new SslCredentials(rootCert);
        return credentials;
    }

    private static Server StarServer(int port)
    {
        Server server = new Server
        {
            Services = { Greeter.BindService(new GreeterImpl()) },
            Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
        };
        server.Start();
        return server;
    }

    class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }
    }
}
