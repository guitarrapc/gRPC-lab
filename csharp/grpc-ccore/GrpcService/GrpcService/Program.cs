using Grpc.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcService
{
    class Program
    {
        const int Port = 30051;

        public static async Task Main(string[] args)
        {
            var hostAddress = Environment.GetEnvironmentVariable("HOST_ADDRESS", EnvironmentVariableTarget.Process) ?? "localhost";
            var useDelay = Environment.GetEnvironmentVariable("USE_DELAY", EnvironmentVariableTarget.Process);
            var reuse = Environment.GetEnvironmentVariable("GRPC_SOREUSE", EnvironmentVariableTarget.Process);

            var options = int.TryParse(reuse, out var soReuse)
                ? new[] {
                    new ChannelOption(ChannelOptions.SoReuseport, soReuse),
                    new ChannelOption("grpc.enable_channelz", "0"),
                }
                : new ChannelOption[] {};
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
    }
}
