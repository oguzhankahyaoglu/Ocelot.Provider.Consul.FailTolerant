using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ocelot.Configuration;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;

namespace Ocelot.Provider.Consul.Controllers;

[Route("gateway-routes")]
public class GatewayRoutesController : ControllerBase
{
    private readonly IInternalConfigurationRepository _internalConfigurationRepository;
    private readonly IOptions<FileConfiguration> _fileConfiguration;

    public GatewayRoutesController(IInternalConfigurationRepository internalConfigurationRepository,
        IOptions<FileConfiguration> fileConfiguration)
    {
        _internalConfigurationRepository = internalConfigurationRepository;
        _fileConfiguration = fileConfiguration;
    }

    public object Index()
    {
        var consulConfig = _internalConfigurationRepository.Get();
        if (consulConfig.IsError)
        {
            throw new Exception("Could not fetch internal configuration");
        }

        var fileConfigDtos = _fileConfiguration.Value.Routes
            .Select(r => new GatewayRouteDto(r))
            .ToArray();
        var internalConfigDtos = consulConfig.Data.Routes
            .Select(r => new GatewayRouteDto(r))
            .ToArray();

        return new
        {
            LastOcelotConfigFetchedSuccessfully =
                ToHumanize(ConsulConfigurationRepository.LastOcelotConfigFetchedSuccessfully),
            LastOcelotConfigFetchTried =
                ToHumanize(ConsulConfigurationRepository.LastOcelotConfigFetchTried),
            FileConfig = fileConfigDtos,
            CurrentConfig = internalConfigDtos,
        };
    }

    private string ToHumanize(DateTime? input)
    {
        return input != null
            ? DateTime.Now.Subtract(input.Value)
                .ToString("g") + " ago"
            : "";
    }

    private record GatewayRouteDto(string From, string To)
    {
        public GatewayRouteDto(FileRoute r)
            : this(GetUpstreamPathTemplate(r),
                GetDownstreamPathTemplate(r))
        {
        }

        public GatewayRouteDto(Route r)
            : this(GetUpstreamPathTemplate(r),
                GetDownstreamPathTemplate(r))
        {
        }

        private static string GetDownstreamPathTemplate(Route r)
        {
            var host = r.DownstreamRoute.FirstOrDefault()?.DownstreamAddresses?.FirstOrDefault()?.Host;
            var port = r.DownstreamRoute.FirstOrDefault()?.DownstreamAddresses?.FirstOrDefault()?.Port;
            var address = r.DownstreamRoute?.FirstOrDefault()?.DownstreamPathTemplate?.Value;
            return $"{address}=> {host}:{port}";
        }

        private static string GetUpstreamPathTemplate(Route r)
        {
            return r.UpstreamHttpMethod?.FirstOrDefault() + ":" + r.UpstreamTemplatePattern.OriginalValue;
        }

        private static string GetDownstreamPathTemplate(FileRoute r)
        {
            var host = r.DownstreamHostAndPorts.FirstOrDefault()?.Host;
            var port = r.DownstreamHostAndPorts.FirstOrDefault()?.Port;
            var address = r.DownstreamHttpMethod + ":" + r.DownstreamPathTemplate;
            return $"{address}=> {host}:{port}";
        }

        private static string GetUpstreamPathTemplate(FileRoute r)
        {
            return r.UpstreamHttpMethod.FirstOrDefault() + ":" + r.UpstreamPathTemplate;
        }
    }
}
