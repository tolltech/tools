namespace Tolltech.Core;

public class AppSettings
{
    public Dictionary<string, AppSettingsItem> Settings { get; set; } = new();
}

public class AppSettingsItem
{
    public Dictionary<string, string> KeyValues { get; set; } = new();
}