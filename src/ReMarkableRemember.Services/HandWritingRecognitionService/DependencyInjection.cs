using Microsoft.Extensions.DependencyInjection;
using ReMarkableRemember.Services.HandWritingRecognitionService.MyScript;
using ReMarkableRemember.Services.HandWritingRecognitionService.MyScript.Interfaces;

namespace ReMarkableRemember.Services.HandWritingRecognitionService;

public static class DependencyInjection
{
    public static IServiceCollection UseMyScriptForHandWritingRecognitionService(this IServiceCollection services)
    {
        return services
            .AddSingleton<IHandWritingRecognitionService, HandWritingRecognitionServiceMyScript>()
            .AddSingleton<IMyScriptCommunication, MyScriptCommunication>();
    }
}
