using ReMarkableRemember.Services.LocalizationService.Configuration;

namespace ReMarkableRemember.Services.LocalizationService;

public interface ILocalizationService
{
    ILocalizationConfiguration Configuration { get; }
}
