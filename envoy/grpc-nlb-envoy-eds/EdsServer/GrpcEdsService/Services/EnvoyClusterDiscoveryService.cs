using Envoy.Api.V2;
using Envoy.Api.V2.Core;
using Envoy.Api.V2.Endpoint;
using Envoy.Service.Discovery.V2;
using Google.Appengine.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcEdsService.Services
{
    public class EnvoyClusterDiscoveryService : ClusterDiscoveryService.ClusterDiscoveryServiceBase
    {
        private readonly ILogger<EnvoyClusterDiscoveryService> _logger;

        public EnvoyClusterDiscoveryService(ILogger<EnvoyClusterDiscoveryService> logger)
        {
            _logger = logger;
        }

        public override Task<DiscoveryResponse> FetchClusters(DiscoveryRequest request, ServerCallContext context)
        {
            var resourceNames = request.ResourceNames;
            if (resourceNames == null || !resourceNames.Any())
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Service Name not provided"));
            }

            foreach (var resource in resourceNames)
            {

            }

            var endpoint = new Endpoint
            {
                Address = new Address
                {
                    SocketAddress = new SocketAddress
                    {
                        Address = "127.0.0.1",
                        PortValue = 8081,
                    },
                },
            };
            var response = new DiscoveryResponse();
            response.Resources.Add(new Google.Protobuf.WellKnownTypes.Any
            {
                Value = endpoint.ToByteString(),
            });
            return Task.FromResult(response);
        }
    }
}
