using System;
using Microsoft.Extensions.DependencyInjection;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Settings;

namespace ReMarkableRemember.DependencyInjection;

internal sealed class Services : IServices
{
    public Services(IDataService dataService, IHandWritingRecognitionService handWritingRecognitionService, ISettingsService settingsService, ITabletService tabletService)
    {
        this.Data = dataService;
        this.HandWritingRecognition = handWritingRecognitionService;
        this.Settings = settingsService;
        this.Tablet = tabletService;
    }

    public IDataService Data { get; }

    public IHandWritingRecognitionService HandWritingRecognition { get; }

    public ISettingsService Settings { get; }

    public ITabletService Tablet { get; }

    public static IServices Create(String[]? args)
    {
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<IServices, Services>()
            .UseConfigurationServiceForSettingsService()
            .UseDataServiceForConfigurationService()
            .UseMyScriptForHandWritingRecognitionService()
            .UseSqliteForDataService(args)
            .UseTabletService()
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<IServices>();
    }
}
