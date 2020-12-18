using Grpc.HealthCheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcService.HostedServices
{
    public class GrpcHealthCheckHostedService : BackgroundService
    {
        private readonly HealthServiceImpl _healthService;
        private readonly HealthCheckService _healthCheckService;

        public GrpcHealthCheckHostedService(HealthServiceImpl healthService, HealthCheckService healthCheckService)
        {
            _healthService = healthService;
            _healthCheckService = healthCheckService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var health = await _healthCheckService.CheckHealthAsync(stoppingToken);

                _healthService.SetStatus("Greeting", health.Status == HealthStatus.Healthy
                    ? Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.Serving
                    : Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.NotServing);

                _healthService.SetStatus("", health.Status == HealthStatus.Healthy
                    ? Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.Serving
                    : Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.NotServing);

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
