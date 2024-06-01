using Microsoft.Extensions.DependencyInjection;
using Flat3DObjectsToSvgConverter.Services;
using Microsoft.Extensions.Configuration;
using SvgNest.Models;
using Flat3DObjectsToSvgConverter.Models;
using Flat3DObjectsToSvgConverter.Services.CleanLoops;

namespace Flat3DObjectsToSvgConverter;

public static class ServiceRegistration
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<Flat3DObjectsToSvgHostedService>()
            .AddSingleton<ObjectsLabelsToSvgConverter>()
            .AddSingleton<SvgCompactingService>()
            .AddSingleton<ObjectsToLoopsConverter>()
            .AddSingleton<ObjectLoopsToSvgConverter>()
            .AddSingleton<ObjectsLabelsPreciseLocator>()
            .AddSingleton<LoopsTabsGenerator>()
            .AddSingleton<MergeLabelsWithTabsSvg>()
            .AddSingleton<IOFileService>()
            .AddCleanupServices();

        services.AddOptions()
            .Configure<SvgNestConfig>(configuration.GetSection("SvgNest"))
            .Configure<IOSettings>(configuration.GetSection("IO"))
            .Configure<SlotsSettings>(configuration.GetSection("Slots"));

        return services;
    }
}