using System;
using Avalonia.Platform.Storage;

namespace ReMarkableRemember.Helper;

internal static class FilePickerFileTypesExtensions
{
    public static FilePickerFileType Epub { get; } = new FilePickerFileType("EPUB e-book")
    {
        Patterns = new String[1] { "*.epub" },
        AppleUniformTypeIdentifiers = new String[1] { "org.idpf.epub-container" },
        MimeTypes = new String[1] { "application/epub+zip" }
    };

    public static FilePickerFileType ImageSvg { get; } = new("SVG image")
    {
        Patterns = new[] { "*.svg" },
        AppleUniformTypeIdentifiers = new[] { "public.svg-image" },
        MimeTypes = new[] { "image/svg+xml" }
    };
}
