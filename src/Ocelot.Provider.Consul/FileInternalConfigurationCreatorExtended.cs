using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocelot.Configuration;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Validator;
using Ocelot.Responses;

namespace Ocelot.Provider.Consul;

public class FileInternalConfigurationCreatorExtended : IInternalConfigurationCreator
{
    private readonly IConfigurationValidator _configurationValidator;
    private readonly IConfigurationCreator _configCreator;
    private readonly IDynamicsCreator _dynamicsCreator;
    private readonly IRoutesCreator _routesCreator;
    private readonly IAggregatesCreator _aggregatesCreator;
    private readonly ILogger<FileInternalConfigurationCreatorExtended> _logger;

    public FileInternalConfigurationCreatorExtended(
        IConfigurationValidator configurationValidator,
        IRoutesCreator routesCreator,
        IAggregatesCreator aggregatesCreator,
        IDynamicsCreator dynamicsCreator,
        IConfigurationCreator configCreator, ILogger<FileInternalConfigurationCreatorExtended> logger)
    {
        _configCreator = configCreator;
        _logger = logger;
        _dynamicsCreator = dynamicsCreator;
        _aggregatesCreator = aggregatesCreator;
        _routesCreator = routesCreator;
        _configurationValidator = configurationValidator;
    }

    public async Task<Response<IInternalConfiguration>> Create(
        FileConfiguration fileConfiguration)
    {
        var response = await _configurationValidator.IsValid(fileConfiguration);
        if (response.Data.IsError)
        {
            var errors1 = string.Join(",", response.Errors.Select(x => x.Message));
            var errors2 = string.Join(",", response.Data.Errors.Select(x => x.Message));
            _logger.LogError($"error parsing config, errors are {errors1} {errors2}");
            return new ErrorResponse<IInternalConfiguration>(response.Data.Errors);
        }

        var routeList = _routesCreator.Create(fileConfiguration);
        var second1 = _aggregatesCreator.Create(fileConfiguration, routeList);
        var second2 = _dynamicsCreator.Create(fileConfiguration);
        var list = routeList.Union(second1).Union(second2)
            .ToList();
        return new OkResponse<IInternalConfiguration>(
            _configCreator.Create(fileConfiguration, list));
    }
}