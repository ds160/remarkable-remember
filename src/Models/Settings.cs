using System;
using ReMarkableRemember.Entities;

namespace ReMarkableRemember.Models;

internal sealed class Settings
{
    private const String BACKUP = "Backup";
    private const String MYSCRIPT_APPLICATION_KEY = "MyScript ApplicationKey";
    private const String MYSCRIPT_HMAC_KEY = "MyScript HmacKey";
    private const String MYSCRIPT_LANGUAGE = "MyScript Language";
    private const String TABLET_IP = "Tablet IP";
    private const String TABLET_PASSWORD = "Tablet Password";

    private readonly Controller controller;

    public Settings(Controller controller)
    {
        this.controller = controller;

        using DatabaseContext database = this.controller.CreateDatabaseContext();
        this.Backup = database.Settings.Find(BACKUP)?.Value ?? String.Empty;
        this.MyScriptApplicationKey = database.Settings.Find(MYSCRIPT_APPLICATION_KEY)?.Value ?? String.Empty;
        this.MyScriptHmacKey = database.Settings.Find(MYSCRIPT_HMAC_KEY)?.Value ?? String.Empty;
        this.MyScriptLanguage = database.Settings.Find(MYSCRIPT_LANGUAGE)?.Value ?? "en_US";
        this.TabletIp = database.Settings.Find(TABLET_IP)?.Value ?? String.Empty;
        this.TabletPassword = database.Settings.Find(TABLET_PASSWORD)?.Value ?? String.Empty;
    }

    public String Backup { get; set; }

    public String MyScriptApplicationKey { get; set; }

    public String MyScriptHmacKey { get; set; }

    public String MyScriptLanguage { get; set; }

    public String TabletIp { get; set; }

    public String TabletPassword { get; set; }

    public void SaveChanges()
    {
        using DatabaseContext database = this.controller.CreateDatabaseContext();

        SetValue(database, BACKUP, this.Backup);
        SetValue(database, MYSCRIPT_APPLICATION_KEY, this.MyScriptApplicationKey);
        SetValue(database, MYSCRIPT_HMAC_KEY, this.MyScriptHmacKey);
        SetValue(database, MYSCRIPT_LANGUAGE, this.MyScriptLanguage);
        SetValue(database, TABLET_IP, this.TabletIp);
        SetValue(database, TABLET_PASSWORD, this.TabletPassword);

        database.SaveChanges();
    }

    private static void SetValue(DatabaseContext database, String key, String value)
    {
        Setting? setting = database.Settings.Find(key);
        if (setting != null)
        {
            setting.Value = value;
        }
        else
        {
            database.Settings.Add(new Setting(key, value));
        }
    }
}
