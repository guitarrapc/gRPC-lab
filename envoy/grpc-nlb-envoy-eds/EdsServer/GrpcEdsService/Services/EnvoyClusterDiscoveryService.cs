using Envoy.Api.V2;
using Envoy.Service.Discovery.V2;
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

        // todo: get endpoint (GET)
        // todo: add endpoint (POST)
        // todo: update endpoint (PUT)
        // todo: delete endpoint (DELETE)

        public override Task<DiscoveryResponse> FetchClusters(DiscoveryRequest request, ServerCallContext context)
        {
            return base.FetchClusters(request, context);
        }
    }
}
