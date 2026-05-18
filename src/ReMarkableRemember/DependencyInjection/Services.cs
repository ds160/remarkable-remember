using System;
using Microsoft.Extensions.DependencyInjection;
using ReMarkableRemember.Images;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Settings;

namespace ReMarkableRemember.DependencyInjection;

internal sealed class Services : IServices
{
    private Services(IServiceProvider serviceProvider)
    {
        this.Data = serviceProvider.GetRequiredService<IDataService>();
        this.HandWritingRecognition = serviceProvider.GetRequiredService<IHandWritingRecognitionService>();
        this.ImageLoader = serviceProvider.GetRequiredService<IImageLoader>();
        this.Settings = serviceProvider.GetRequiredService<ISettingsService>();
        this.Tablet = serviceProvider.GetRequiredService<ITabletService>();
    }

    public IDataService Data { get; }

    public IHandWritingRecognitionService HandWritingRecognition { get; }

    public IImageLoader ImageLoader { get; }

    public ISettingsService Settings { get; }

    public ITabletService Tablet { get; }

    public static IServices Create(String[]? args)
    {
        IServiceProvider serviceProvider = new ServiceCollection()
            .UseAssetLoaderForImages()
            .UseConfigurationServiceForSettingsService()
            .UseDataServiceForConfigurationService()
            .UseMyScriptForHandWritingRecognitionService()
            .UseSqliteForDataService(args)
            .UseSshForTabletService()
            .BuildServiceProvider();

        return new Services(serviceProvider);
    }
}
