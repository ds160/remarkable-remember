using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Models;

public sealed class TabletTemplate
{
    private static readonly Dictionary<String, (String, Boolean)> iconCodes = new Dictionary<String, (String, Boolean)>()
    {
        { "\uE9FE", ("Blank", false) },
        { "\uE9FD", ("Blank", true) },
        { "\uE9AA", ("Checklist double", true) },
        { "\uE9AB", ("Checklist", true) },
        { "\uE9AC", ("Dayplanner", true) },
        { "\uE9AD", ("Dots bottom", true) },
        { "\uE9B4", ("Dots top", true) },
        { "\uE9B6", ("Grid bottom", true) },
        { "\uE9BC", ("Grid margin large", true) },
        { "\uE9C9", ("Grid margin medium", true) },
        { "\uE9B7", ("Grid top", true) },
        { "\uE9BA", ("Lines bottom", true) },
        { "\uE9B8", ("Lines medium", true) },
        { "\uE9B9", ("Lines small", true) },
        { "\uE9BB", ("Lines top", true) },
        { "\uE9C8", ("Margin medium", true) },
        { "\uE9CA", ("Margin small", true) },
        { "\uE9CC", ("One storyboard 1", true) },
        { "\uE9CB", ("One storyboard 2", true) },
        { "\uE9D4", ("One storyboard", false) },
        { "\uE9D7", ("Two storyboards", false) },
        { "\uE9CD", ("Two storyboards", true) },
        { "\uE9B5", ("Four storyboards", true) },
        { "\uE997", ("Four storyboards", false) },
        { "\uE9CE", ("Weekplanner US", true) },
        { "\uE9CF", ("Weekplanner", true) },
        { "\uE98F", ("Checklist", false) },
        { "\uE9FF", ("Cornell", false) },
        { "\uE991", ("Dayplanner", false) },
        { "\uE993", ("Dots S bottom", false) },
        { "\uE996", ("Dots S top", false) },
        { "\uE995", ("Dots S", false) },
        { "\uE994", ("Dots large", false) },
        { "\uE9F9", ("Dots S", true) },
        { "\uE9F8", ("Dots large", true) },
        { "\uE999", ("Grid bottom", false) },
        { "\uE99A", ("Grid large", false) },
        { "\uE99D", ("Grid medium", false) },
        { "\uE99E", ("Grid small", false) },
        { "\uE9FC", ("Grid large", true) },
        { "\uE9FB", ("Grid medium", true) },
        { "\uE9FA", ("Grid small", true) },
        { "\uE99B", ("Grid margin large", false) },
        { "\uE99C", ("Grid margin medium", false) },
        { "\uE99F", ("Grid top", false) },
        { "\uE9A5", ("Lined bottom", false) },
        { "\uE9A0", ("Lined heading", false) },
        { "\uE9A9", ("Lined top", false) },
        { "\uE9A6", ("Lines large", false) },
        { "\uE9A7", ("Lines medium", false) },
        { "\uE9A8", ("Lines small", false) },
        { "\uE9D0", ("Margin large", false) },
        { "\uE9D1", ("Margin medium", false) },
        { "\uE9D2", ("Margin small", false) },
        { "\uE9D8", ("US College", false) },
        { "\uE9D9", ("US Legal", false) },
        { "\uE9DC", ("Weekplanner 1", false) },
        { "\uE9DA", ("Weekplanner 2", false) },
        { "\uE9DB", ("Weekplanner US", false) },
        { "\uEA00", ("Isometric", false) },
        { "\uE9D5", ("Perspective 1", false) },
        { "\uE9D6", ("Perspective 2", false) },
        { "\uE990", ("Calligraphy large", true) },
        { "\uE9A1", ("Calligraphy medium", true) },
        { "\uE970", ("Piano sheet large", true) },
        { "\uE975", ("Piano sheet medium", true) },
        { "\uE976", ("Piano sheet small", true) },
        { "\uE98D", ("Calligraphy large", false) },
        { "\uE98E", ("Calligraphy medium", false) },
        { "\uE9D3", ("Music", false) },
        { "\uE9C0", ("Music Bass tablature", false) },
        { "\uE9B2", ("Music Guitar chords", false) },
        { "\uE9C5", ("Music Guitar tablature", false) },
        { "\uE977", ("Piano sheet large", false) },
        { "\uE978", ("Piano sheet medium", false) },
        { "\uE979", ("Piano sheet small", false) },
        { "\uE97B", ("Hexagon large", false) },
        { "\uE982", ("Hexagon medium", false) },
        { "\uE98C", ("Hexagon small", false) }
    };

    private readonly Controller controller;

    internal TabletTemplate(Controller controller, Template template)
    {
        this.controller = controller;

        this.BytesPng = template.BytesPng;
        this.BytesSvg = template.BytesSvg;
        this.Category = template.Category;
        this.IconCode = template.IconCode;
        this.Name = template.Name;
    }

    public TabletTemplate(Controller controller, String name, String category, String iconCode, String sourceFilePath)
    {
        this.controller = controller;

        String directory = Path.GetDirectoryName(sourceFilePath) ?? String.Empty;
        String fileName = Path.GetFileNameWithoutExtension(sourceFilePath);

        this.BytesPng = File.ReadAllBytes(Path.Combine(directory, $"{fileName}.png"));
        this.BytesSvg = File.ReadAllBytes(Path.Combine(directory, $"{fileName}.svg"));
        this.Category = category;
        this.IconCode = iconCode;
        this.Name = name;
    }

    public Task Delete()
    {
        throw new NotImplementedException();
    }


    public async Task Restore()
    {
        await this.controller.Tablet.UploadTemplate(this).ConfigureAwait(false);
    }

    public async Task Upload()
    {
        await this.controller.Tablet.UploadTemplate(this).ConfigureAwait(false);

        using DatabaseContext database = this.controller.CreateDatabaseContext();

        Template? template = await database.Templates.FindAsync(this.Category, this.Name).ConfigureAwait(false);
        if (template != null)
        {
            template.IconCode = this.IconCode;
            template.BytesPng = this.BytesPng;
            template.BytesSvg = this.BytesSvg;
        }
        else
        {
            template = new Template(this.Category, this.Name, this.IconCode, this.BytesPng, this.BytesSvg);
            await database.Templates.AddAsync(template).ConfigureAwait(false);
        }

        await database.SaveChangesAsync().ConfigureAwait(false);
    }

    internal Byte[] BytesPng { get; }

    internal Byte[] BytesSvg { get; }

    public String Category { get; }

    public String FileName { get { return $"{this.Category} {this.Name}"; } }

    public String IconCode { get; }

    public String Name { get; }

    public static IEnumerable<String> IconCodes { get { return iconCodes.Keys; } }

    public static String GetIconCodeName(String iconCode)
    {
        return iconCodes[iconCode].Item1;
    }

    public static Boolean IsLandscape(String iconCode)
    {
        return iconCodes[iconCode].Item2;
    }
}
