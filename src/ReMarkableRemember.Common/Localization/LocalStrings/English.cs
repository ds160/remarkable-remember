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
    public String ButtonDelete { get { return "Delete"; } }
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

    public String HandwritingRecognitionCopyToClipboard { get { return "Copy To Clipboard"; } }
    public String HandwritingRecognitionRemoveLineEndings { get { return "Remove Line Endings"; } }
    public String HandwritingRecognitionTitle { get { return "Handwriting Recognition"; } }

    public String ItemHintExistsInTarget { get { return "Exists already in target directory"; } }
    public String ItemHintModified { get { return "Modified"; } }
    public String ItemHintNew { get { return "New"; } }
    public String ItemHintNotFoundInTarget { get { return "Not found in target directory"; } }
    public String ItemHintSyncPathChanged { get { return "Sync path changed"; } }
    public String ItemHintUpToDate { get { return "Up-to-date"; } }
    public String ItemSyncTargetFolder { get { return "Sync Target Folder"; } }

    public String ItemsTreeHeaderBackupInformation { get { return "Backup Information"; } }
    public String ItemsTreeHeaderName { get { return "Name"; } }
    public String ItemsTreeHeaderModified { get { return "Modified"; } }
    public String ItemsTreeHeaderSyncInformation { get { return "Sync Information"; } }
    public String ItemsTreeHeaderSyncPath { get { return "Sync Path"; } }

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

    public String LamyEraserEraseBehavior { get { return "Erase Behavior"; } }
    public String LamyEraserEraseBehaviorPress { get { return "Press and hold to erase, release to switch back"; } }
    public String LamyEraserEraseBehaviorToggle { get { return "Press the button to erase, press the button again to switch back"; } }
    public String LamyEraserDoubleClickBehavior { get { return "Double Click Behavior"; } }
    public String LamyEraserDoubleClickBehaviorRedo { get { return "Redo"; } }
    public String LamyEraserDoubleClickBehaviorUndo { get { return "Undo"; } }
    public String LamyEraserLeftHanded { get { return "Left Handed"; } }
    public String LamyEraserLeftHandedNo { get { return "No"; } }
    public String LamyEraserLeftHandedYes { get { return "Yes"; } }
    public String LamyEraserTitle { get { return "Lamy Eraser Options"; } }

    public String MenuBackup { get { return "Backup Items"; } }
    public String MenuDownload { get { return "Download File"; } }
    public String MenuHandwritingRecognition { get { return "Handwriting Recognition"; } }
    public String MenuHandwritingRecognitionLanguage { get { return "Language for Handwriting Recognition"; } }
    public String MenuLamyEraser { get { return "Install Lamy Eraser"; } }
    public String MenuOpen { get { return "Open"; } }
    public String MenuSettings { get { return "Settings"; } }
    public String MenuSync { get { return "Sync Items"; } }
    public String MenuSyncAndBackup { get { return "Sync & Backup Items"; } }
    public String MenuSyncDirectoryReset { get { return "Reset Sync Directory"; } }
    public String MenuSyncDirectorySet { get { return "Set Sync Directory"; } }
    public String MenuTemplates { get { return "Manage Templates"; } }
    public String MenuTemplateUpload { get { return "Upload Template"; } }
    public String MenuUpload { get { return "Upload File"; } }

    public String MyScriptAuthorizationError { get { return "MyScript authorization information not configured or wrong."; } }
    public String MyScriptLanguageNotSupported(String language) { return $"Language is not supported by MyScript: {language}"; }
    public String MyScriptPageAnalyzeError(Int32 pageNumber) { return $"MyScript cannot analyze page {pageNumber}, it contains too much content."; }

    public String NotebookBlockHeaderInvalid(Byte unknown) { return $"Invalid reMarkable .lines file block header: '{unknown}'."; }
    public String NotebookBlockHeaderVersionInvalid { get { return "Invalid reMarkable .lines file block header version."; } }
    public String NotebookBlockHeaderVersionUnknown(Byte version) { return $"Unknown reMarkable .lines file block header version: '{version}'."; }
    public String NotebookBlockTagIndexInvalid { get { return "Invalid reMarkable .lines file block tag index."; } }
    public String NotebookBlockTagTypeInvalid { get { return "Invalid reMarkable .lines file block tag type."; } }
    public String NotebookHeaderUnknown { get { return "Unknown reMarkable .lines file header."; } }

    public String SettingsApplicationLanguage { get { return "Language"; } }
    public String SettingsApplicationTheme { get { return "Theme"; } }
    public String SettingsApplicationThemeDark { get { return "Dark"; } }
    public String SettingsApplicationThemeDefault { get { return "System"; } }
    public String SettingsApplicationThemeLight { get { return "Light"; } }
    public String SettingsBackupFolder { get { return "Backup Folder"; } }
    public String SettingsDateTimeFormat { get { return "Time Format"; } }
    public String SettingsDateTimeFormatHours12 { get { return "12 hours (AM/PM)"; } }
    public String SettingsDateTimeFormatHours24 { get { return "24 hours"; } }
    public String SettingsLanguageDefault { get { return "Default"; } }
    public String SettingsLanguageHandwritingRecognition { get { return "Language for Handwriting Recognition"; } }
    public String SettingsMyScriptApplicationKey { get { return "MyScript Application Key"; } }
    public String SettingsMyScriptHmacKey { get { return "MyScript HMAC Key"; } }
    public String SettingsTabHeaderApplication { get { return "Application"; } }
    public String SettingsTabHeaderTablet { get { return "reMarkable"; } }
    public String SettingsTabletPassword { get { return "reMarkable Password"; } }
    public String SettingsTabletPasswordPlaceholder { get { return "SSH protocol password, see 'Copyrights and licenses'"; } }
    public String SettingsTabletPasswordRequired { get { return "Password is required"; } }
    public String SettingsTabletIp { get { return "reMarkable IP"; } }
    public String SettingsTabletIpInvalid { get { return "Invalid IP address"; } }
    public String SettingsTabletIpPlaceholder { get { return "Can be left blank if connected via USB"; } }
    public String SettingsTitle { get { return "Settings"; } }

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

    public String TemplateCategory { get { return "Category"; } }
    public String TemplateFileNameBlank { get { return "Blank"; } }
    public String TemplateFileNameChecklistDouble { get { return "Checklist double"; } }
    public String TemplateFileNameChecklist { get { return "Checklist"; } }
    public String TemplateFileNameDayplanner { get { return "Dayplanner"; } }
    public String TemplateFileNameDotsBottom { get { return "Dots bottom"; } }
    public String TemplateFileNameDotsTop { get { return "Dots top"; } }
    public String TemplateFileNameGridBottom { get { return "Grid bottom"; } }
    public String TemplateFileNameGridMarginLarge { get { return "Grid margin large"; } }
    public String TemplateFileNameGridMargin { get { return "Grid margin"; } }
    public String TemplateFileNameGridTop { get { return "Grid top"; } }
    public String TemplateFileNameLinesBottom { get { return "Lines bottom"; } }
    public String TemplateFileNameLinesMedium { get { return "Lines medium"; } }
    public String TemplateFileNameLinesSmall { get { return "Lines small"; } }
    public String TemplateFileNameLinesTop { get { return "Lines top"; } }
    public String TemplateFileNameMarginMedium { get { return "Margin medium"; } }
    public String TemplateFileNameMarginSmall { get { return "Margin small"; } }
    public String TemplateFileNameOneStoryboard1 { get { return "One storyboard 1"; } }
    public String TemplateFileNameOneStoryboard2 { get { return "One storyboard 2"; } }
    public String TemplateFileNameOneStoryboard { get { return "One storyboard"; } }
    public String TemplateFileNameTwoStoryboards { get { return "Two storyboards"; } }
    public String TemplateFileNameFourStoryboards { get { return "Four storyboards"; } }
    public String TemplateFileNameWeekplannerUS { get { return "Weekplanner US"; } }
    public String TemplateFileNameWeekplanner { get { return "Weekplanner"; } }
    public String TemplateFileNameCornell { get { return "Cornell"; } }
    public String TemplateFileNameDotsSmall { get { return "Dots small"; } }
    public String TemplateFileNameDotsLarge { get { return "Dots large"; } }
    public String TemplateFileNameGridLarge { get { return "Grid large"; } }
    public String TemplateFileNameGridMedium { get { return "Grid medium"; } }
    public String TemplateFileNameGridSmall { get { return "Grid small"; } }
    public String TemplateFileNameLinedBottom { get { return "Lined bottom"; } }
    public String TemplateFileNameLinedHeading { get { return "Lined heading"; } }
    public String TemplateFileNameLinedTop { get { return "Lined top"; } }
    public String TemplateFileNameLinesLarge { get { return "Lines large"; } }
    public String TemplateFileNameMarginLarge { get { return "Margin large"; } }
    public String TemplateFileNameUSCollege { get { return "US College"; } }
    public String TemplateFileNameUSLegal { get { return "US Legal"; } }
    public String TemplateFileNameWeekplanner1 { get { return "Weekplanner 1"; } }
    public String TemplateFileNameWeekplanner2 { get { return "Weekplanner 2"; } }
    public String TemplateFileNameIsometric { get { return "Isometric"; } }
    public String TemplateFileNamePerspective1 { get { return "Perspective 1"; } }
    public String TemplateFileNamePerspective2 { get { return "Perspective 2"; } }
    public String TemplateFileNameCalligraphyLarge { get { return "Calligraphy large"; } }
    public String TemplateFileNameCalligraphyMedium { get { return "Calligraphy medium"; } }
    public String TemplateFileNamePianoSheetLarge { get { return "Piano sheet large"; } }
    public String TemplateFileNamePianoSheetMedium { get { return "Piano sheet medium"; } }
    public String TemplateFileNamePianoSheetSmall { get { return "Piano sheet small"; } }
    public String TemplateFileNameMusic { get { return "Music"; } }
    public String TemplateFileNameMusicBassTablature { get { return "Music Bass tablature"; } }
    public String TemplateFileNameMusicGuitarChords { get { return "Music Guitar chords"; } }
    public String TemplateFileNameMusicGuitarTablature { get { return "Music Guitar tablature"; } }
    public String TemplateFileNameHexagonLarge { get { return "Hexagon large"; } }
    public String TemplateFileNameHexagonMedium { get { return "Hexagon medium"; } }
    public String TemplateFileNameHexagonSmall { get { return "Hexagon small"; } }
    public String TemplateIcon { get { return "Icon"; } }
    public String TemplateLandscape { get { return "Landscape"; } }
    public String TemplateName { get { return "Name"; } }
    public String TemplatePortrait { get { return "Portrait"; } }
    public String TemplatePropertyRequired(String name) { return $"{name} is required"; }
    public String TemplateSourceFilePath { get { return "Source File Path"; } }
    public String TemplateTitle { get { return "Template"; } }

    public String TemplatesTitle { get { return "Templates"; } }
}
