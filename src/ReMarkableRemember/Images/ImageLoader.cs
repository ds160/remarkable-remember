using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg;

namespace ReMarkableRemember.Images;

internal sealed class ImageLoader : IImageLoader
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
    private static readonly Dictionary<String, IImage> images = new Dictionary<String, IImage>();

    public IImage Bitmap(String path)
    {
        if (!images.TryGetValue(path, out IImage? image))
        {
            image = new Bitmap(LoadAsset(path));
            images.Add(path, image);
        }
        return image;
    }

    public IImage Svg(String path)
    {
        if (!images.TryGetValue(path, out IImage? image))
        {
            image = new SvgImage() { Source = SvgSource.Load(LoadAsset(path)) };
            images.Add(path, image);
        }
        return image;
    }

    private static Stream LoadAsset(String path)
    {
        return AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/{path}"));
    }
}
