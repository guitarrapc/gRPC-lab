using Google.Appengine.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcEdsService.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcEdsService.Services
{
    public class EnvoyEndpointRegisterService : EndpointRegisterService.EndpointRegisterServiceBase
    {
        private readonly GrpcEdsServiceModel _model;
        private readonly ILogger<EnvoyEndpointRegisterService> _logger;

        public EnvoyEndpointRegisterService(GrpcEdsServiceModel model, ILogger<EnvoyEndpointRegisterService> logger)
        {
            _model = model;
            _logger = logger;
        }

        // todo: get endpoint (GET)
        // todo: add endpoint (POST)
        // todo: update endpoint (PUT)
        // todo: delete endpoint (DELETE)

        /// <summary>
        /// List all resources
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<RegisterListResponse> List(Empty request, ServerCallContext context)
        {            
            var service = new Google.Protobuf.Collections.MapField<string, RegisterService>
            {
               _model.Gets(),
            };
            var response = new RegisterListResponse();
            response.Services.Add(service);
            return Task.FromResult(response);
        }

        /// <summary>
        /// List hosts for service
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<RegisterService> Get(RegisterGetRequest request, ServerCallContext context)
        {
            var service = _model.Get(request.ServiceName);
            if (service != null)
            {
                return Task.FromResult(service);
            }
            throw new RpcException(new Status(StatusCode.NotFound, $"Service {request.ServiceName} doesn't exist."));
        }

        /// <summary>
        /// Create a given resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<RegisterService> Add(RegisterAddRequest request, ServerCallContext context)
        {
            if (!_model.Exists(request.ServiceName))
            {
                _model.Add(request.ServiceName, request.Service);
                return Task.FromResult(_model.Get(request.ServiceName)!);
            }
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"Service {request.ServiceName} already exists."));
        }

        /// <summary>
        /// Update a given resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<RegisterService> Update(RegisterUpdateRequest request, ServerCallContext context)
        {
            if (_model.Exists(request.ServiceName))
            {
                _model.Update(request.ServiceName, request.Service);
                return Task.FromResult(_model.Get(request.ServiceName)!);
            }
            throw new RpcException(new Status(StatusCode.NotFound, $"Service {request.ServiceName} doesn't exist."));
        }

        /// <summary>
        /// Delete a given resource
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<Empty> Delete(RegisterDeleteRequest request, ServerCallContext context)
        {
            if (_model.Exists(request.ServiceName))
            {
                _model.Delete(request.ServiceName);
                return Task.FromResult(new Empty());
            }
            throw new RpcException(new Status(StatusCode.NotFound, $"Service {request.ServiceName} doesn't exist."));
        }
    }
}
