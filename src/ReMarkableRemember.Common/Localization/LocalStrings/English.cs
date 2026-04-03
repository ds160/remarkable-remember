using System;

namespace ReMarkableRemember.Common.Localization.LocalStrings;

internal class English : ILocalStrings
{
    public String AboutText { get { return "A cross-platform management application for the reMarkable tablet."; } }
    public String AboutTitle { get { return "About"; } }

    public String AppMenuAbout { get { return "About reMarkable Remember"; } }
    public String AppMenuSettings { get { return "Settings..."; } }

    public String ButtonCancel { get { return "Cancel"; } }
    public String ButtonClose { get { return "Close"; } }
    public String ButtonInstall { get { return "Install"; } }
    public String ButtonNo { get { return "No"; } }
    public String ButtonOK { get { return "OK"; } }
    public String ButtonRestore { get { return "Restore"; } }
    public String ButtonSave { get { return "Save"; } }
    public String ButtonUpload { get { return "Upload"; } }
    public String ButtonYes { get { return "Yes"; } }

    public String ErrorTitle { get { return "Error"; } }

    public String FilePickerEpub { get { return "EPUB e-book"; } }
    public String FilePickerPdf { get { return "PDF document"; } }
    public String FilePickerPng { get { return "PNG image"; } }
    public String FilePickerSvg { get { return "SVG image"; } }

    public String HandwritingRecognitionTitle { get { return "Handwriting Recognition"; } }

    public String ItemHintExistsInTarget { get { return "Exists already in target directory"; } }
    public String ItemHintModified { get { return "Modified"; } }
    public String ItemHintNew { get { return "New"; } }
    public String ItemHintNotFoundInTarget { get { return "Not found in target directory"; } }
    public String ItemHintSyncPathChanged { get { return "Sync path changed"; } }
    public String ItemHintUpToDate { get { return "Up-to-date"; } }

    public String ItemsTreeViewBackupInformation { get { return "Backup Information"; } }
    public String ItemsTreeViewName { get { return "Name"; } }
    public String ItemsTreeViewModified { get { return "Modified"; } }
    public String ItemsTreeViewSyncInformation { get { return "Sync Information"; } }
    public String ItemsTreeViewSyncPath { get { return "Sync Path"; } }

    public String JobAndJoin { get { return " and "; } }
    public String JobBackup { get { return "Backup"; } }
    public String JobDownload { get { return "Downloading File"; } }
    public String JobGetItems { get { return "Getting Items"; } }
    public String JobHandwritingRecognition { get { return "Handwriting Recognition"; } }
    public String JobInstallLamyEraser { get { return "Installing Lamy Eraser"; } }
    public String JobManageTemplates { get { return "Managing Templates"; } }
    public String JobSync { get { return "Syncing"; } }
    public String JobUpload { get { return "Uploading File"; } }
    public String JobUploadTemplate { get { return "Uploading Template"; } }

    public String LamyEraserTitle { get { return "Lamy Eraser Options"; } }

    public String MyScriptAuthorizationError { get { return "MyScript authorization information not configured or wrong."; } }
    public String MyScriptLanguageNotSupported(String language) { return $"Language is not supported by MyScript: {language}"; }
    public String MyScriptPageAnalyzeError(Int32 pageNumber) { return $"MyScript cannot analyze page {pageNumber}, it contains too much content."; }

    public String NotebookBlockHeaderInvalid(Byte unknown) { return $"Invalid reMarkable .lines file block header: '{unknown}'."; }
    public String NotebookBlockHeaderVersionInvalid { get { return "Invalid reMarkable .lines file block header version."; } }
    public String NotebookBlockHeaderVersionUnknown(Byte version) { return $"Unknown reMarkable .lines file block header version: '{version}'."; }
    public String NotebookBlockTagIndexInvalid { get { return "Invalid reMarkable .lines file block tag index."; } }
    public String NotebookBlockTagTypeInvalid { get { return "Invalid reMarkable .lines file block tag type."; } }
    public String NotebookHeaderUnknown { get { return "Unknown reMarkable .lines file header."; } }

    public String SettingsBackupFolder { get { return "Backup Folder"; } }
    public String SettingsTabletPasswordRequired { get { return "Password is required"; } }
    public String SettingsTabletIp { get { return "reMarkable IP"; } }
    public String SettingsTabletIpInvalid { get { return "Invalid IP address"; } }
    public String SettingsTabletIpPlaceholder { get { return "Can be left blank if connected via USB"; } }
    public String SettingsTitle { get { return "Settings"; } }

    public String SyncTargetFolder { get { return "Sync Target Folder"; } }

    public String TemplatePropertyRequired(String name) { return $"{name} is required"; }
    public String TemplateSourceFile { get { return "Source File"; } }
    public String TemplateTitle { get { return "Template"; } }

    public String TemplatesTitle { get { return "Templates"; } }

    public String TabletFileFormatVersionInvalid(Int32 formatVersion) { return $"Invalid reMarkable file format version: '{formatVersion}'."; }
    public String TabletFileTooLarge { get { return "File is too large."; } }
    public String TabletFileTypeInvalid(String type) { return $"Invalid reMarkable file type: '{type}'."; }
    public String TabletFileTypeNotSupported(String extension) { return $"File type is not supported: '{extension}'."; }
    public String TabletItemsNotReadable { get { return "Failed to read following files from tablet:"; } }
    public String TabletLamyEraserNotSupported(String type) { return $"Lamy Eraser is not supported on {type}."; }
    public String TabletNotSupported { get { return "The connected reMarkable is not supported."; } }
    public String TabletRestartQuestion { get { return "Would you like to restart your reMarkable tablet now?"; } }
    public String TabletRestartReasonTemplate { get { return "The template information has been changed."; } }
    public String TabletRestartSaveWork { get { return "Please save your work on your tablet by going to the main screen before restarting."; } }
    public String TabletRestartTakeEffect { get { return "A restart is required for the changes to take effect."; } }
    public String TabletRestartTitle { get { return "Restart"; } }
    public String TabletSoftwareVersionUnknown { get { return "The reMarkable software verion cannot be identified."; } }
    public String TabletSshNotConfigured { get { return "SSH protocol information are not configured or wrong."; } }
    public String TabletSshNotConnected { get { return "reMarkable is not connected via WiFi or USB."; } }
    public String TabletStatusConnected { get { return "Connected"; } }
    public String TabletStatusNotConnected { get { return "Not connected"; } }
    public String TabletStatusNotSupported { get { return "Connected reMarkable not supported"; } }
    public String TabletStatusSshNotConfigured { get { return "SSH protocol information are not configured or wrong"; } }
    public String TabletStatusSshNotConnected { get { return "Not connected via WiFi or USB"; } }
    public String TabletStatusUsbNotActived { get { return "USB connection is not activated"; } }
    public String TabletStatusUsbNotConnected { get { return "Not connected via USB"; } }
    public String TabletTemplateImageRequired { get { return "A PNG or SVG image is required to upload a template."; } }
    public String TabletUsbNotActived { get { return "USB web interface is not activated."; } }
    public String TabletUsbNotConnected { get { return "reMarkable is not connected via USB."; } }
}
