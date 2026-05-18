using Microsoft.Extensions.DependencyInjection;

namespace ReMarkableRemember.Images;

public static class DependencyInjection
{
    public static IServiceCollection UseAssetLoaderForImages(this IServiceCollection services)
    {
        return services.AddSingleton<IImageLoader, ImageLoader>();
    }
}
