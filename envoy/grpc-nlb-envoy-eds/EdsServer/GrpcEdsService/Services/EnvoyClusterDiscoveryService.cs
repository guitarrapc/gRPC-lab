using Envoy.Api.V2;
using Envoy.Api.V2.Core;
using Envoy.Api.V2.Endpoint;
using Envoy.Service.Discovery.V2;
using Google.Appengine.V1;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using GrpcEdsService.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GrpcEdsService.Services
{
    public class EnvoyClusterDiscoveryService : ClusterDiscoveryService.ClusterDiscoveryServiceBase
    {
        private readonly ServiceVersionContext _versionContext;
        private readonly GrpcEdsServiceModel _model;
        private readonly ILogger<EnvoyClusterDiscoveryService> _logger;

        public EnvoyClusterDiscoveryService(ServiceVersionContext versionContext, GrpcEdsServiceModel model, ILogger<EnvoyClusterDiscoveryService> logger)
        {
            _versionContext = versionContext;
            _model = model;
            _logger = logger;
        }

        public override Task StreamClusters(IAsyncStreamReader<DiscoveryRequest> requestStream, IServerStreamWriter<DiscoveryResponse> responseStream, ServerCallContext context)
        {
            _logger.LogInformation(JsonSerializer.Serialize(requestStream.Current.TypeUrl));
            return base.StreamClusters(requestStream, responseStream, context);
        }

        public override Task DeltaClusters(IAsyncStreamReader<DeltaDiscoveryRequest> requestStream, IServerStreamWriter<DeltaDiscoveryResponse> responseStream, ServerCallContext context)
        {
            _logger.LogInformation(JsonSerializer.Serialize(requestStream.Current.TypeUrl));
            return base.DeltaClusters(requestStream, responseStream, context);
        }
    }
}
