using System;
using Avalonia.Media;

namespace ReMarkableRemember.Images;

public interface IImageLoader
{
    IImage Bitmap(String path);

    IImage Svg(String path);
}
