using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ReMarkableRemember.Services.ConfigurationService.Configuration;
using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.DataService.Models;

namespace ReMarkableRemember.Services.ConfigurationService;
public class ConfigurationServiceDataService : IConfigurationService
{
    private readonly IDataService dataService;

    public ConfigurationServiceDataService(IDataService dataService)
    {
        this.dataService = dataService;
    }

    public T Load<T>() where T : IConfiguration, new()
    {
        T configuration = new T();

        List<SettingData> settings = GetSettings(configuration);
        this.dataService.LoadSettings(settings).Wait();

        Dictionary<String, PropertyInfo> properties = GetProperties(configuration);
        foreach (SettingData setting in settings)
        {
            properties[setting.Key].SetValue(configuration, setting.Value);
        }

        return configuration;
    }

    public async Task Save<T>(T configuration) where T : IConfiguration
    {
        List<SettingData> settings = GetSettings(configuration);
        await this.dataService.SaveSettings(settings).ConfigureAwait(false);
    }

    private static List<SettingData> GetSettings<T>(T configuration) where T : IConfiguration
    {
        String prefix = configuration.GetPrefix();
        List<SettingData> settings = new List<SettingData>();

        foreach (PropertyInfo property in GetProperties(configuration).Values)
        {
            settings.Add(new SettingData(prefix, property.Name, property.GetValue(configuration) as String ?? String.Empty));
        }

        return settings;
    }

    private static Dictionary<String, PropertyInfo> GetProperties(IConfiguration configuration)
    {
        return configuration
            .GetType()
            .GetProperties()
            .Where(prop => prop.PropertyType == typeof(String) && prop.CanRead && prop.CanWrite)
            .ToDictionary(prop => prop.Name);
    }
}
