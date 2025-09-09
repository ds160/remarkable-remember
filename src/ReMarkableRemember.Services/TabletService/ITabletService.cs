using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReMarkableRemember.Common.Notebook;
using ReMarkableRemember.Services.TabletService.Configuration;
using ReMarkableRemember.Services.TabletService.Models;

namespace ReMarkableRemember.Services.TabletService;

public interface ITabletService : IDisposable
{
    ITabletConfiguration Configuration { get; }

    Task Backup(String id);

    Task DeleteTemplate(TabletTemplate tabletTemplate);

    Task Download(String id, String targetPath);

    Task<TabletConnectionStatus> GetConnectionStatus();

    Task<IEnumerable<TabletItem>> GetItems();

    Task<Notebook> GetNotebook(String id);

    Task InstallLamyEraser(Boolean press, Boolean undo, Boolean leftHanded);

    Task InstallWebInterfaceOnBoot(String release = "v1.2.4");

    Task Restart();

    Task UploadFile(String path, String? parentId);

    Task UploadTemplate(TabletTemplate tabletTemplate);
}
