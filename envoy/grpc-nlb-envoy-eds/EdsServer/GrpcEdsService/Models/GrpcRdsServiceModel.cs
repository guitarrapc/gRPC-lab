using Envoy.Api.V2.Route;
using GrpcEdsService;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GrpcEdsService.Models
{
    public class GrpcRdsServiceModel
    {
        private readonly ConcurrentDictionary<string, VirtualHost> _virtualHosts = new ConcurrentDictionary<string, VirtualHost>();        
        private readonly ILogger<GrpcRdsServiceModel> _logger;

        public GrpcRdsServiceModel(ILogger<GrpcRdsServiceModel> logger)
        {
            _logger = logger;
        }

        public IDictionary<string, VirtualHost> Gets()
        {
            return _virtualHosts.ToImmutableDictionary();
        }
        
        public VirtualHost? Get(string serviceName)
        {
            return _virtualHosts.TryGetValue(serviceName, out var service)
                ? service
                : null;
        }

        public bool Exists(string serviceName)
        {
            return _virtualHosts.TryGetValue(serviceName, out var _);
        }

        public void Add(string serviceName, VirtualHost data)
        {
            if (!_virtualHosts.TryAdd(serviceName, data))
            {
                _logger.LogInformation($"Could not add {serviceName}");
            }
        }

        public void Update(string serviceName, VirtualHost data)
        {
            if (_virtualHosts.TryGetValue(serviceName, out var currentValue))
            {
                if (!_virtualHosts.TryUpdate(serviceName, data, currentValue))
                {
                    _logger.LogInformation($"Could not update {serviceName}");
                }
            }
        }

        public void Delete(string serviceName)
        {
            if (!_virtualHosts.TryRemove(serviceName, out var _))
            {
                _logger.LogInformation($"Could not delete {serviceName}");
            }
        }
    }
}
