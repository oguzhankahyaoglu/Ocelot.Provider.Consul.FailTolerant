using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Responses;

namespace Ocelot.Provider.Consul;

/// <summary>
/// Normalde mevcut implementasyonu PollInterval değerini consul'dan okuyordu.
/// Fakat buna gerek olmadığı, lokal konfigden okuyacağı şekline çevirildi.
/// Ayrıca, startup esnasında ilk isteği hemen consul'a yapması sonra PollInterval'i beklemesi sağlandı.
/// </summary>
public class ConsulFileConfigurationPollerOptionExtended : IFileConfigurationPollerOptions
{
    private readonly IInternalConfigurationRepository _internalConfigRepo;
    private readonly ILogger<ConsulFileConfigurationPollerOptionExtended> _logger;
    private readonly IFileConfigurationRepository _fileConfigurationRepository;
    private readonly IOptions<FileConfiguration> _fileConfiguration;

    public ConsulFileConfigurationPollerOptionExtended(
        IInternalConfigurationRepository internalConfigurationRepository,
        IFileConfigurationRepository fileConfigurationRepository,
        ILogger<ConsulFileConfigurationPollerOptionExtended> logger, IOptions<FileConfiguration> fileConfiguration)
    {
        _internalConfigRepo = internalConfigurationRepository;
        _fileConfigurationRepository = fileConfigurationRepository;
        _logger = logger;
        _fileConfiguration = fileConfiguration;
    }

    private bool isStartupDelayRequested = true;

    public int Delay => GetDelay();

    private int GetDelay()
    {
        //if polling delay is specified in config, use it directly!
        if (_fileConfiguration?.Value?.GlobalConfiguration?.ServiceDiscoveryProvider?.PollingInterval > 0)
        {
            if (isStartupDelayRequested)
            {
                isStartupDelayRequested = false;
                return 1; //consul fetch on initial startup must be fetched immediately
            }
            return _fileConfiguration.Value.GlobalConfiguration.ServiceDiscoveryProvider.PollingInterval;
        }

        var result = GetFileConfiguration();
        if (result?.Data?.GlobalConfiguration?.ServiceDiscoveryProvider != null && !result.IsError &&
            result.Data.GlobalConfiguration.ServiceDiscoveryProvider.PollingInterval > 0)
        {
            return result.Data.GlobalConfiguration.ServiceDiscoveryProvider.PollingInterval;
        }

        var response = _internalConfigRepo.Get();
        if (response?.Data?.ServiceProviderConfiguration != null && !response.IsError &&
            response.Data.ServiceProviderConfiguration.PollingInterval > 0)
        {
            return response.Data.ServiceProviderConfiguration.PollingInterval;
        }

        return Debugger.IsAttached ? 1000 * 30 : 1000 * 5;
    }

    private Response<FileConfiguration> GetFileConfiguration()
    {
        try
        {
            return Task.Run(async () => await GetConfigurationAsync()).Result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task<Response<FileConfiguration>> GetConfigurationAsync()
    {
        try
        {
            return await _fileConfigurationRepository.Get();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }
}
