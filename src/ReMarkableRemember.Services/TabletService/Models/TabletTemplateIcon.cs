using System;
using System.Collections.Generic;
using ReMarkableRemember.Common.Localization;

namespace ReMarkableRemember.Services.TabletService.Models;

public sealed class TabletTemplateIcon
{
    private static readonly List<TabletTemplateIcon> icons = new List<TabletTemplateIcon>()
    {
        new TabletTemplateIcon("\uE9FE", "E9FE", () => Language.Current.TemplateFileNameBlank, false),
        new TabletTemplateIcon("\uE9FD", "E9FD", () => Language.Current.TemplateFileNameBlank, true),
        new TabletTemplateIcon("\uE9AA", "E9AA", () => Language.Current.TemplateFileNameChecklistDouble, true),
        new TabletTemplateIcon("\uE9AB", "E9AB", () => Language.Current.TemplateFileNameChecklist, true),
        new TabletTemplateIcon("\uE9AC", "E9AC", () => Language.Current.TemplateFileNameDayplanner, true),
        new TabletTemplateIcon("\uE9AD", "E9AD", () => Language.Current.TemplateFileNameDotsBottom, true),
        new TabletTemplateIcon("\uE9B4", "E9B4", () => Language.Current.TemplateFileNameDotsTop, true),
        new TabletTemplateIcon("\uE9B6", "E9B6", () => Language.Current.TemplateFileNameGridBottom, true),
        new TabletTemplateIcon("\uE9BC", "E9BC", () => Language.Current.TemplateFileNameGridMarginLarge, true),
        new TabletTemplateIcon("\uE9C9", "E9C9", () => Language.Current.TemplateFileNameGridMargin, true),
        new TabletTemplateIcon("\uE9B7", "E9B7", () => Language.Current.TemplateFileNameGridTop, true),
        new TabletTemplateIcon("\uE9BA", "E9BA", () => Language.Current.TemplateFileNameLinesBottom, true),
        new TabletTemplateIcon("\uE9B8", "E9B8", () => Language.Current.TemplateFileNameLinesMedium, true),
        new TabletTemplateIcon("\uE9B9", "E9B9", () => Language.Current.TemplateFileNameLinesSmall, true),
        new TabletTemplateIcon("\uE9BB", "E9BB", () => Language.Current.TemplateFileNameLinesTop, true),
        new TabletTemplateIcon("\uE9C8", "E9C8", () => Language.Current.TemplateFileNameMarginMedium, true),
        new TabletTemplateIcon("\uE9CA", "E9CA", () => Language.Current.TemplateFileNameMarginSmall, true),
        new TabletTemplateIcon("\uE9CC", "E9CC", () => Language.Current.TemplateFileNameOneStoryboard1, true),
        new TabletTemplateIcon("\uE9CB", "E9CB", () => Language.Current.TemplateFileNameOneStoryboard2, true),
        new TabletTemplateIcon("\uE9D4", "E9D4", () => Language.Current.TemplateFileNameOneStoryboard, false),
        new TabletTemplateIcon("\uE9D7", "E9D7", () => Language.Current.TemplateFileNameTwoStoryboards, false),
        new TabletTemplateIcon("\uE9CD", "E9CD", () => Language.Current.TemplateFileNameTwoStoryboards, true),
        new TabletTemplateIcon("\uE9B5", "E9B5", () => Language.Current.TemplateFileNameFourStoryboards, true),
        new TabletTemplateIcon("\uE997", "E997", () => Language.Current.TemplateFileNameFourStoryboards, false),
        new TabletTemplateIcon("\uE9CE", "E9CE", () => Language.Current.TemplateFileNameWeekplannerUS, true),
        new TabletTemplateIcon("\uE9CF", "E9CF", () => Language.Current.TemplateFileNameWeekplanner, true),
        new TabletTemplateIcon("\uE98F", "E98F", () => Language.Current.TemplateFileNameChecklist, false),
        new TabletTemplateIcon("\uE9FF", "E9FF", () => Language.Current.TemplateFileNameCornell, false),
        new TabletTemplateIcon("\uE991", "E991", () => Language.Current.TemplateFileNameDayplanner, false),
        new TabletTemplateIcon("\uE993", "E993", () => Language.Current.TemplateFileNameDotsBottom, false),
        new TabletTemplateIcon("\uE996", "E996", () => Language.Current.TemplateFileNameDotsTop, false),
        new TabletTemplateIcon("\uE995", "E995", () => Language.Current.TemplateFileNameDotsSmall, false),
        new TabletTemplateIcon("\uE994", "E994", () => Language.Current.TemplateFileNameDotsLarge, false),
        new TabletTemplateIcon("\uE9F9", "E9F9", () => Language.Current.TemplateFileNameDotsSmall, true),
        new TabletTemplateIcon("\uE9F8", "E9F8", () => Language.Current.TemplateFileNameDotsLarge, true),
        new TabletTemplateIcon("\uE999", "E999", () => Language.Current.TemplateFileNameGridBottom, false),
        new TabletTemplateIcon("\uE99A", "E99A", () => Language.Current.TemplateFileNameGridLarge, false),
        new TabletTemplateIcon("\uE99D", "E99D", () => Language.Current.TemplateFileNameGridMedium, false),
        new TabletTemplateIcon("\uE99E", "E99E", () => Language.Current.TemplateFileNameGridSmall, false),
        new TabletTemplateIcon("\uE9FC", "E9FC", () => Language.Current.TemplateFileNameGridLarge, true),
        new TabletTemplateIcon("\uE9FB", "E9FB", () => Language.Current.TemplateFileNameGridMedium, true),
        new TabletTemplateIcon("\uE9FA", "E9FA", () => Language.Current.TemplateFileNameGridSmall, true),
        new TabletTemplateIcon("\uE99B", "E99B", () => Language.Current.TemplateFileNameGridMarginLarge, false),
        new TabletTemplateIcon("\uE99C", "E99C", () => Language.Current.TemplateFileNameGridMargin, false),
        new TabletTemplateIcon("\uE99F", "E99F", () => Language.Current.TemplateFileNameGridTop, false),
        new TabletTemplateIcon("\uE9A5", "E9A5", () => Language.Current.TemplateFileNameLinedBottom, false),
        new TabletTemplateIcon("\uE9A0", "E9A0", () => Language.Current.TemplateFileNameLinedHeading, false),
        new TabletTemplateIcon("\uE9A9", "E9A9", () => Language.Current.TemplateFileNameLinedTop, false),
        new TabletTemplateIcon("\uE9A6", "E9A6", () => Language.Current.TemplateFileNameLinesLarge, false),
        new TabletTemplateIcon("\uE9A7", "E9A7", () => Language.Current.TemplateFileNameLinesMedium, false),
        new TabletTemplateIcon("\uE9A8", "E9A8", () => Language.Current.TemplateFileNameLinesSmall, false),
        new TabletTemplateIcon("\uE9D0", "E9D0", () => Language.Current.TemplateFileNameMarginLarge, false),
        new TabletTemplateIcon("\uE9D1", "E9D1", () => Language.Current.TemplateFileNameMarginMedium, false),
        new TabletTemplateIcon("\uE9D2", "E9D2", () => Language.Current.TemplateFileNameMarginSmall, false),
        new TabletTemplateIcon("\uE9D8", "E9D8", () => Language.Current.TemplateFileNameUSCollege, false),
        new TabletTemplateIcon("\uE9D9", "E9D9", () => Language.Current.TemplateFileNameUSLegal, false),
        new TabletTemplateIcon("\uE9DC", "E9DC", () => Language.Current.TemplateFileNameWeekplanner1, false),
        new TabletTemplateIcon("\uE9DA", "E9DA", () => Language.Current.TemplateFileNameWeekplanner2, false),
        new TabletTemplateIcon("\uE9DB", "E9DB", () => Language.Current.TemplateFileNameWeekplannerUS, false),
        new TabletTemplateIcon("\uEA00", "EA00", () => Language.Current.TemplateFileNameIsometric, false),
        new TabletTemplateIcon("\uE9D5", "E9D5", () => Language.Current.TemplateFileNamePerspective1, false),
        new TabletTemplateIcon("\uE9D6", "E9D6", () => Language.Current.TemplateFileNamePerspective2, false),
        new TabletTemplateIcon("\uE990", "E990", () => Language.Current.TemplateFileNameCalligraphyLarge, true),
        new TabletTemplateIcon("\uE9A1", "E9A1", () => Language.Current.TemplateFileNameCalligraphyMedium, true),
        new TabletTemplateIcon("\uE970", "E970", () => Language.Current.TemplateFileNamePianoSheetLarge, true),
        new TabletTemplateIcon("\uE975", "E975", () => Language.Current.TemplateFileNamePianoSheetMedium, true),
        new TabletTemplateIcon("\uE976", "E976", () => Language.Current.TemplateFileNamePianoSheetSmall, true),
        new TabletTemplateIcon("\uE98D", "E98D", () => Language.Current.TemplateFileNameCalligraphyLarge, false),
        new TabletTemplateIcon("\uE98E", "E98E", () => Language.Current.TemplateFileNameCalligraphyMedium, false),
        new TabletTemplateIcon("\uE9D3", "E9D3", () => Language.Current.TemplateFileNameMusic, false),
        new TabletTemplateIcon("\uE9C0", "E9C0", () => Language.Current.TemplateFileNameMusicBassTablature, false),
        new TabletTemplateIcon("\uE9B2", "E9B2", () => Language.Current.TemplateFileNameMusicGuitarChords, false),
        new TabletTemplateIcon("\uE9C5", "E9C5", () => Language.Current.TemplateFileNameMusicGuitarTablature, false),
        new TabletTemplateIcon("\uE977", "E977", () => Language.Current.TemplateFileNamePianoSheetLarge, false),
        new TabletTemplateIcon("\uE978", "E978", () => Language.Current.TemplateFileNamePianoSheetMedium, false),
        new TabletTemplateIcon("\uE979", "E979", () => Language.Current.TemplateFileNamePianoSheetSmall, false),
        new TabletTemplateIcon("\uE97B", "E97B", () => Language.Current.TemplateFileNameHexagonLarge, false),
        new TabletTemplateIcon("\uE982", "E982", () => Language.Current.TemplateFileNameHexagonMedium, false),
        new TabletTemplateIcon("\uE98C", "E98C", () => Language.Current.TemplateFileNameHexagonSmall, false)
    };

    private readonly Func<String> name;

    private TabletTemplateIcon(String code, String imageName, Func<String> name, Boolean isLandscape)
    {
        this.name = name;

        this.Code = code;
        this.ImageName = imageName;
        this.IsLandscape = isLandscape;
    }

    public String Code { get; }
    public String ImageName { get; }
    public Boolean IsLandscape { get; }

    public String GetName()
    {
        return this.name();
    }

    public static IEnumerable<TabletTemplateIcon> Icons { get { return icons; } }
}
