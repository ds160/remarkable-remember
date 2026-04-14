using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Settings;

namespace ReMarkableRemember.DependencyInjection;

internal sealed class ServicesCollection : IServices
{
    public ServicesCollection(IDataService dataService, IHandWritingRecognitionService handWritingRecognitionService, ISettingsService settingsService, ITabletService tabletService)
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
}
