using Google.Appengine.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcEdsService.Services
{
    public class EnvoyClusterRegisterService : ClusterRegisterService.ClusterRegisterServiceBase
    {
        private readonly ILogger<EnvoyClusterRegisterService> _logger;

        public EnvoyClusterRegisterService(ILogger<EnvoyClusterRegisterService> logger)
        {
            _logger = logger;
        }

        // todo: get endpoint (GET)
        // todo: add endpoint (POST)
        // todo: update endpoint (PUT)
        // todo: delete endpoint (DELETE)

        /// <summary>
        /// grpcurl -insecure -d '{}' localhost:5001 envoy.ClusterRegisterService.List
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<RegisterListResponse> List(Empty request, ServerCallContext context)
        {
            var service = new Google.Protobuf.Collections.MapField<string, RegisterService>
            {
                { "myservice", new RegisterService()
                    {
                        Hosts = new RegisterServiceHost
                        {
                            IpAddress = "127.0.0.1",
                            Port = 8001,
                            Tags = new RegisterServiceTags
                            {
                                Az = "us-east1",
                                Canary = false,
                                LoadBalancingWeight = 50,
                            },
                        },
                    }
                },
            };
            var response = new RegisterListResponse();
            response.Message.Add(service);
            return Task.FromResult(response);
        }

        /// <summary>
        /// grpcurl -insecure -d '{ "service_name": "myservice" }' localhost:5001 envoy.ClusterRegisterService.Get
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<RegisterService> Get(RegisterGetRequest request, ServerCallContext context)
        {
            var service = new RegisterService()
            {
                Hosts = new RegisterServiceHost
                {
                    IpAddress = "127.0.0.1",
                    Port = 8001,
                    Tags = new RegisterServiceTags
                    {
                        Az = "us-east1",
                        Canary = false,
                        LoadBalancingWeight = 50,
                    },
                },
            };
            return Task.FromResult(service);
        }
    }
}
