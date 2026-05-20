using System.IO;
using System.Text.Json;

namespace PsWinSnip.Services;

public class AppSettings
{
    public double? LastAspectRatio { get; set; } = 1.0;
}

public static class SettingsService
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PsWinSnip",
        "settings.json"
    );

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }
        return new AppSettings();
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            string? directory = Path.GetDirectoryName(FilePath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }
}