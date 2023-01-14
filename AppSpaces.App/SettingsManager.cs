using System.Text.Json;
using AppSpaces.App.Models;

namespace AppSpaces.App;

public static class SettingsManager
{
	private const string SettingsFile = "AppSpaces.settings.json";

	public static async Task<Settings> LoadSettings()
	{
		// TODO: Setting validation
		var settingsPath = Path.Join(Environment.SpecialFolder.LocalApplicationData.ToString(), SettingsFile);
		if (!File.Exists(settingsPath))
		{
			return await DefaultSettings.Create();
		}

		using var stream = File.OpenText(settingsPath);
		var settingsData = await stream.ReadToEndAsync();
		if (string.IsNullOrEmpty(settingsData))
		{
			return await DefaultSettings.Create();
		}

		var settings = JsonSerializer.Deserialize<Settings>(settingsData);

		return settings ?? await DefaultSettings.Create();
	}

	public static async Task SaveSettings(Settings settings)
	{
		// TODO: Setting validation
		var settingsPath = Path.Join(Environment.SpecialFolder.LocalApplicationData.ToString(), SettingsFile);
		var file = new FileInfo(settingsPath);
		file.Directory?.Create();

		var settingsData = JsonSerializer.Serialize(settings, new JsonSerializerOptions
		{
			WriteIndented = true
		});

		await File.WriteAllTextAsync(settingsPath, settingsData);
	}
}