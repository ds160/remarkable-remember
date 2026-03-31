using System;

namespace ReMarkableRemember.Common.Localization;

public interface ILocalStrings
{
    String MyScriptAuthorizationError { get; }
    String MyScriptLanguageNotSupported(String language);
    String MyScriptPageAnalyzeError(Int32 pageNumber);

    String NotebookBlockHeaderInvalid(Byte unknown);
    String NotebookBlockHeaderVersionInvalid { get; }
    String NotebookBlockHeaderVersionUnknown(Byte version);
    String NotebookBlockTagIndexInvalid { get; }
    String NotebookBlockTagTypeInvalid { get; }
    String NotebookHeaderUnknown { get; }

    String TabletFileFormatVersionInvalid(Int32 formatVersion);
    String TabletFileTooLarge { get; }
    String TabletFileTypeInvalid(String type);
    String TabletFileTypeNotSupported(String extension);
    String TabletLamyEraserNotSupported(String type);
    String TabletNotSupported { get; }
    String TabletSoftwareVersionUnknown { get; }
    String TabletSshNotConfigured { get; }
    String TabletSshNotConnected { get; }
    String TabletTemplateImageRequired { get; }
    String TabletUsbNotActived { get; }
    String TabletUsbNotConnected { get; }
}
