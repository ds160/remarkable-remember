using System;
using System.IO;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg;

namespace ReMarkableRemember.Helper;

public static class ImageLoader
{
    private static readonly String? assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

    public static Bitmap Bitmap(String path)
    {
        return new Bitmap(LoadAsset(path));
    }

    public static SvgImage Svg(String path)
    {
        return new SvgImage() { Source = SvgSource.Load(LoadAsset(path)) };
    }

    private static Stream LoadAsset(String path)
    {
        return AssetLoader.Open(new Uri($"avares://{assemblyName}/Assets/{path}"));
    }
}
