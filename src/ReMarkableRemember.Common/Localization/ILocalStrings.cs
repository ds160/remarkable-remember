using System;

namespace ReMarkableRemember.Common.Localization;

public interface ILocalStrings
{
    String AboutText { get; }
    String AboutTitle { get; }

    String AppMenuAbout { get; }
    String AppMenuSettings { get; }

    String ButtonCancel { get; }
    String ButtonClose { get; }
    String ButtonInstall { get; }
    String ButtonNo { get; }
    String ButtonOK { get; }
    String ButtonRestore { get; }
    String ButtonSave { get; }
    String ButtonUpload { get; }
    String ButtonYes { get; }

    String ErrorTitle { get; }

    String FilePickerEpub { get; }
    String FilePickerPdf { get; }
    String FilePickerPng { get; }
    String FilePickerSvg { get; }

    String HandwritingRecognitionTitle { get; }

    String ItemHintExistsInTarget { get; }
    String ItemHintModified { get; }
    String ItemHintNew { get; }
    String ItemHintNotFoundInTarget { get; }
    String ItemHintSyncPathChanged { get; }
    String ItemHintUpToDate { get; }

    String ItemsTreeViewBackupInformation { get; }
    String ItemsTreeViewName { get; }
    String ItemsTreeViewModified { get; }
    String ItemsTreeViewSyncInformation { get; }
    String ItemsTreeViewSyncPath { get; }

    String JobAndJoin { get; }
    String JobBackup { get; }
    String JobDownload { get; }
    String JobGetItems { get; }
    String JobHandwritingRecognition { get; }
    String JobInstallLamyEraser { get; }
    String JobManageTemplates { get; }
    String JobSync { get; }
    String JobUpload { get; }
    String JobUploadTemplate { get; }

    String LamyEraserTitle { get; }

    String MyScriptAuthorizationError { get; }
    String MyScriptLanguageNotSupported(String language);
    String MyScriptPageAnalyzeError(Int32 pageNumber);

    String NotebookBlockHeaderInvalid(Byte unknown);
    String NotebookBlockHeaderVersionInvalid { get; }
    String NotebookBlockHeaderVersionUnknown(Byte version);
    String NotebookBlockTagIndexInvalid { get; }
    String NotebookBlockTagTypeInvalid { get; }
    String NotebookHeaderUnknown { get; }

    String SettingsBackupFolder { get; }
    String SettingsTabletPasswordRequired { get; }
    String SettingsTabletIp { get; }
    String SettingsTabletIpInvalid { get; }
    String SettingsTabletIpPlaceholder { get; }
    String SettingsTitle { get; }

    String SyncTargetFolder { get; }

    String TemplatePropertyRequired(String name);
    String TemplateSourceFile { get; }
    String TemplateTitle { get; }

    String TemplatesTitle { get; }

    String TabletFileFormatVersionInvalid(Int32 formatVersion);
    String TabletFileTooLarge { get; }
    String TabletFileTypeInvalid(String type);
    String TabletFileTypeNotSupported(String extension);
    String TabletItemsNotReadable { get; }
    String TabletLamyEraserNotSupported(String type);
    String TabletNotSupported { get; }
    String TabletRestartQuestion { get; }
    String TabletRestartReasonTemplate { get; }
    String TabletRestartSaveWork { get; }
    String TabletRestartTakeEffect { get; }
    String TabletRestartTitle { get; }
    String TabletSoftwareVersionUnknown { get; }
    String TabletSshNotConfigured { get; }
    String TabletSshNotConnected { get; }
    String TabletStatusConnected { get; }
    String TabletStatusNotConnected { get; }
    String TabletStatusNotSupported { get; }
    String TabletStatusSshNotConfigured { get; }
    String TabletStatusSshNotConnected { get; }
    String TabletStatusUsbNotActived { get; }
    String TabletStatusUsbNotConnected { get; }
    String TabletTemplateImageRequired { get; }
    String TabletUsbNotActived { get; }
    String TabletUsbNotConnected { get; }
}
