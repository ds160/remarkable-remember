using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReMarkableRemember.Models.Interfaces;

internal interface IController : IDisposable
{
    Settings Settings { get; }

    Task<Boolean> BackupItem(Item item);

    Task<TabletConnectionError?> GetConnectionStatus();

    Task<IEnumerable<Item>> GetItems();

    Task<String> HandWritingRecognition(Item item, String language);

    Task<Boolean> SyncItem(Item item);

    Task UploadTemplate(TabletTemplate template);
}
