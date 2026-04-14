using System;
using Avalonia.Platform.Storage;
using ReMarkableRemember.Common.Localization;

namespace ReMarkableRemember.Helper;

internal static class FilePickerFileTypesExtensions
{
    public static FilePickerFileType Epub
    {
        get
        {
            return new FilePickerFileType(Language.Current.FilePickerEpub)
            {
                Patterns = new String[1] { "*.epub" },
                AppleUniformTypeIdentifiers = new String[1] { "org.idpf.epub-container" },
                MimeTypes = new String[1] { "application/epub+zip" }
            };
        }
    }

    public static FilePickerFileType ImagePng
    {
        get
        {
            return new FilePickerFileType(Language.Current.FilePickerPng)
            {
                Patterns = new[] { "*.png" },
                AppleUniformTypeIdentifiers = new[] { "public.png" },
                MimeTypes = new[] { "image/png" }
            };
        }
    }


    public static FilePickerFileType ImageSvg
    {
        get
        {
            return new FilePickerFileType(Language.Current.FilePickerSvg)
            {
                Patterns = new[] { "*.svg" },
                AppleUniformTypeIdentifiers = new[] { "public.svg-image" },
                MimeTypes = new[] { "image/svg+xml" }
            };
        }
    }

    public static FilePickerFileType Pdf
    {
        get
        {
            return new FilePickerFileType(Language.Current.FilePickerPdf)
            {
                Patterns = new[] { "*.pdf" },
                AppleUniformTypeIdentifiers = new[] { "com.adobe.pdf" },
                MimeTypes = new[] { "application/pdf" }
            };
        }
    }
}
