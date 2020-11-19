using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcService
{
    class Program
    {
        const int Port = 5000;

        public static async Task Main(string[] args)
        {
            var hostAddress = Environment.GetEnvironmentVariable("HOST_ADDRESS", EnvironmentVariableTarget.Process) ?? "localhost";
            var useDelay = Environment.GetEnvironmentVariable("USE_DELAY", EnvironmentVariableTarget.Process);
            // key1=value1,key2=value2,key3=value3
            var optionsString = Environment.GetEnvironmentVariable("GRPC_CHANNEL_OPTIONS", EnvironmentVariableTarget.Process);

            var options = GetOptions(optionsString);
            Server server = new Server(options)
            {
                Services = { Greeter.BindService(new GreeterService()) },
                Ports = { new ServerPort(hostAddress, Port, ServerCredentials.Insecure) },
                
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            if (string.IsNullOrEmpty(useDelay))
            {
                Console.WriteLine("Press any key to stop the server...");
                Console.ReadKey();
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(int.Parse(useDelay)));
            }

            server.ShutdownAsync().Wait();
        }

        private static ChannelOption[] GetOptions(string optionsString)
        {
            var options = string.IsNullOrEmpty(optionsString)
            ? new ChannelOption[] {
                // force gRPC client to open TCP Port for each channel
                // https://stackoverflow.com/questions/53564748/how-do-i-force-my-grpc-client-to-open-multiple-connections-to-the-server
                // new ChannelOption("grpc.use_local_subchannel_pool", 1),
            }
            : optionsString.Split(",")
            .Where(x => x.Contains('=') && x.Split('=').Length == 2)
            .Select(x =>
            {
                var kv = x.Split('=');
                var option = int.TryParse(kv[1], out var value)
                    ? new ChannelOption(kv[0], value)
                    : new ChannelOption(kv[0], kv[1]);
                return option;
            })
            .ToArray();

            DebugChannelOptions(options);

            return options;
        }

        private static void DebugChannelOptions(ChannelOption[] options)
        {
            Console.WriteLine($"Channel Options: {options.Length}");
            foreach (var option in options)
            {
                if (option.Type == ChannelOption.OptionType.String)
                {
                    Console.WriteLine($"{option.Name}: {option.StringValue}");
                }
                else
                {
                    Console.WriteLine($"{option.Name}: {option.IntValue}");
                }
            }
        }
    }
}
