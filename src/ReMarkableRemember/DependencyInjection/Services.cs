using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ReMarkableRemember.Services.ConfigurationService;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.HandWritingRecognitionService.MyScript;
using ReMarkableRemember.Services.HandWritingRecognitionService.MyScript.Interfaces;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Services.TabletService.Communication;
using ReMarkableRemember.Services.TabletService.Communication.Interfaces;
using ReMarkableRemember.Services.TabletService.Files;
using ReMarkableRemember.Services.TabletService.Files.Interfaces;
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
            .AddSingleton<IConfigurationService, ConfigurationServiceDataService>()
            .AddSingleton<IDataService>(DataServiceSqlite.Create(args?.FirstOrDefault()))
            .AddSingleton<IHandWritingRecognitionService, HandWritingRecognitionServiceMyScript>()
            .AddSingleton<IMyScriptCommunication, MyScriptCommunication>()
            .AddSingleton<IServices, Services>()
            .AddSingleton<ISettingsService, SettingsService>()
            .AddSingleton<ITabletCommunication, TabletCommunication>()
            .AddSingleton<ITabletFileSerializer, TabletFileSerializer>()
            .AddSingleton<ITabletService, TabletService>()
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<IServices>();
    }
}
