using ReMarkableRemember.Services.DataService;
using ReMarkableRemember.Services.HandWritingRecognitionService;
using ReMarkableRemember.Services.TabletService;
using ReMarkableRemember.Settings;

namespace ReMarkableRemember.DependencyInjection;

public interface IServices
{
    IDataService Data { get; }

    IHandWritingRecognitionService HandWritingRecognition { get; }

    ISettingsService Settings { get; }

    ITabletService Tablet { get; }
}
