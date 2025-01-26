using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReMarkableRemember.Services.DataService.Models;

namespace ReMarkableRemember.Services.DataService;

public interface IDataService
{
    Task<ItemData> GetItem(String id);

    Task<ItemData> SetItemBackup(String id, DateTime modified);

    Task<ItemData> SetItemSync(String id, DateTime modified, String path);

    Task<ItemData> SetItemSyncTargetDirectory(String id, String? targetDirectory);



    Task LoadSettings(IEnumerable<SettingData> settings);

    Task SaveSettings(IEnumerable<SettingData> settings);



    Task DeleteTemplate(String category, String name);

    Task<IEnumerable<TemplateData>> GetTemplates();

    Task SetTemplate(TemplateData templateData);
}
