using System;

namespace ReMarkableRemember.Common.Localization.LocalStrings;

internal class English : ILocalStrings
{
    public String MyScriptAuthorizationError { get { return "MyScript authorization information not configured or wrong."; } }
    public String MyScriptLanguageNotSupported(String language) { return $"Language is not supported by MyScript: {language}"; }
    public String MyScriptPageAnalyzeError(Int32 pageNumber) { return $"MyScript cannot analyze page {pageNumber}, it contains too much content."; }

    public String NotebookBlockHeaderInvalid(Byte unknown) { return $"Invalid reMarkable .lines file block header: '{unknown}'."; }
    public String NotebookBlockHeaderVersionInvalid { get { return "Invalid reMarkable .lines file block header version."; } }
    public String NotebookBlockHeaderVersionUnknown(Byte version) { return $"Unknown reMarkable .lines file block header version: '{version}'."; }
    public String NotebookBlockTagIndexInvalid { get { return "Invalid reMarkable .lines file block tag index."; } }
    public String NotebookBlockTagTypeInvalid { get { return "Invalid reMarkable .lines file block tag type."; } }
    public String NotebookHeaderUnknown { get { return "Unknown reMarkable .lines file header."; } }

    public String TabletFileFormatVersionInvalid(Int32 formatVersion) { return $"Invalid reMarkable file format version: '{formatVersion}'."; }
    public String TabletFileTooLarge { get { return "File is too large."; } }
    public String TabletFileTypeInvalid(String type) { return $"Invalid reMarkable file type: '{type}'."; }
    public String TabletFileTypeNotSupported(String extension) { return $"File type is not supported: '{extension}'."; }
    public String TabletLamyEraserNotSupported(String type) { return $"Lamy Eraser is not supported on {type}."; }
    public String TabletNotSupported { get { return "The connected reMarkable is not supported."; } }
    public String TabletSoftwareVersionUnknown { get { return "The reMarkable software verion cannot be identified."; } }
    public String TabletSshNotConfigured { get { return "SSH protocol information are not configured or wrong."; } }
    public String TabletSshNotConnected { get { return "reMarkable is not connected via WiFi or USB."; } }
    public String TabletTemplateImageRequired { get { return "A PNG or SVG image is required to upload a template."; } }
    public String TabletUsbNotActived { get { return "USB web interface is not activated."; } }
    public String TabletUsbNotConnected { get { return "reMarkable is not connected via USB."; } }
}
