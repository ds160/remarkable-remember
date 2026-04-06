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
    String ButtonDelete { get; }
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

    String HandwritingRecognitionCopyToClipboard { get; }
    String HandwritingRecognitionRemoveLineEndings { get; }
    String HandwritingRecognitionTitle { get; }

    String ItemHintExistsInTarget { get; }
    String ItemHintModified { get; }
    String ItemHintNew { get; }
    String ItemHintNotFoundInTarget { get; }
    String ItemHintSyncPathChanged { get; }
    String ItemHintUpToDate { get; }
    String ItemSyncTargetFolder { get; }

    String ItemsTreeHeaderBackupInformation { get; }
    String ItemsTreeHeaderName { get; }
    String ItemsTreeHeaderModified { get; }
    String ItemsTreeHeaderSyncInformation { get; }
    String ItemsTreeHeaderSyncPath { get; }

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

    String LamyEraserEraseBehavior { get; }
    String LamyEraserEraseBehaviorPress { get; }
    String LamyEraserEraseBehaviorToggle { get; }
    String LamyEraserDoubleClickBehavior { get; }
    String LamyEraserDoubleClickBehaviorRedo { get; }
    String LamyEraserDoubleClickBehaviorUndo { get; }
    String LamyEraserLeftHanded { get; }
    String LamyEraserLeftHandedNo { get; }
    String LamyEraserLeftHandedYes { get; }
    String LamyEraserTitle { get; }

    String MenuBackup { get; }
    String MenuDownload { get; }
    String MenuHandwritingRecognition { get; }
    String MenuHandwritingRecognitionLanguage { get; }
    String MenuLamyEraser { get; }
    String MenuOpen { get; }
    String MenuSettings { get; }
    String MenuSync { get; }
    String MenuSyncAndBackup { get; }
    String MenuSyncDirectoryReset { get; }
    String MenuSyncDirectorySet { get; }
    String MenuTemplates { get; }
    String MenuTemplateUpload { get; }
    String MenuUpload { get; }

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
    String SettingsLanguageHandwritingRecognition { get; }
    String SettingsMyScriptApplicationKey { get; }
    String SettingsMyScriptHmacKey { get; }
    String SettingsTabletPassword { get; }
    String SettingsTabletPasswordPlaceholder { get; }
    String SettingsTabletPasswordRequired { get; }
    String SettingsTabletIp { get; }
    String SettingsTabletIpInvalid { get; }
    String SettingsTabletIpPlaceholder { get; }
    String SettingsTitle { get; }

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

    String TemplateCategory { get; }
    String TemplateFileNameBlank { get; }
    String TemplateFileNameChecklistDouble { get; }
    String TemplateFileNameChecklist { get; }
    String TemplateFileNameDayplanner { get; }
    String TemplateFileNameDotsBottom { get; }
    String TemplateFileNameDotsTop { get; }
    String TemplateFileNameGridBottom { get; }
    String TemplateFileNameGridMarginLarge { get; }
    String TemplateFileNameGridMargin { get; }
    String TemplateFileNameGridTop { get; }
    String TemplateFileNameLinesBottom { get; }
    String TemplateFileNameLinesMedium { get; }
    String TemplateFileNameLinesSmall { get; }
    String TemplateFileNameLinesTop { get; }
    String TemplateFileNameMarginMedium { get; }
    String TemplateFileNameMarginSmall { get; }
    String TemplateFileNameOneStoryboard1 { get; }
    String TemplateFileNameOneStoryboard2 { get; }
    String TemplateFileNameOneStoryboard { get; }
    String TemplateFileNameTwoStoryboards { get; }
    String TemplateFileNameFourStoryboards { get; }
    String TemplateFileNameWeekplannerUS { get; }
    String TemplateFileNameWeekplanner { get; }
    String TemplateFileNameCornell { get; }
    String TemplateFileNameDotsSmall { get; }
    String TemplateFileNameDotsLarge { get; }
    String TemplateFileNameGridLarge { get; }
    String TemplateFileNameGridMedium { get; }
    String TemplateFileNameGridSmall { get; }
    String TemplateFileNameLinedBottom { get; }
    String TemplateFileNameLinedHeading { get; }
    String TemplateFileNameLinedTop { get; }
    String TemplateFileNameLinesLarge { get; }
    String TemplateFileNameMarginLarge { get; }
    String TemplateFileNameUSCollege { get; }
    String TemplateFileNameUSLegal { get; }
    String TemplateFileNameWeekplanner1 { get; }
    String TemplateFileNameWeekplanner2 { get; }
    String TemplateFileNameIsometric { get; }
    String TemplateFileNamePerspective1 { get; }
    String TemplateFileNamePerspective2 { get; }
    String TemplateFileNameCalligraphyLarge { get; }
    String TemplateFileNameCalligraphyMedium { get; }
    String TemplateFileNamePianoSheetLarge { get; }
    String TemplateFileNamePianoSheetMedium { get; }
    String TemplateFileNamePianoSheetSmall { get; }
    String TemplateFileNameMusic { get; }
    String TemplateFileNameMusicBassTablature { get; }
    String TemplateFileNameMusicGuitarChords { get; }
    String TemplateFileNameMusicGuitarTablature { get; }
    String TemplateFileNameHexagonLarge { get; }
    String TemplateFileNameHexagonMedium { get; }
    String TemplateFileNameHexagonSmall { get; }
    String TemplateIcon { get; }
    String TemplateLandscape { get; }
    String TemplateName { get; }
    String TemplatePortrait { get; }
    String TemplatePropertyRequired(String name);
    String TemplateSourceFilePath { get; }
    String TemplateTitle { get; }

    String TemplatesTitle { get; }
}
