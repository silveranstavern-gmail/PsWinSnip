using System.IO;
using System.Text.Json;

namespace PsWinSnip.Services;

public class AppSettings
{
    public double? LastAspectRatio { get; set; } = 1.0;
    public double LastWidth { get; set; } = 400;
    public double LastHeight { get; set; } = 400;
    public double? LastX { get; set; }
    public double? LastY { get; set; }
    public bool ShowGrid { get; set; } = true;
    public bool RememberPosition { get; set; } = true;
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
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    // Basic validation
                    if (settings.LastWidth < 200) settings.LastWidth = 400;
                    if (settings.LastHeight < 200) settings.LastHeight = 400;
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            // If file is corrupted, maybe delete it or just return defaults
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