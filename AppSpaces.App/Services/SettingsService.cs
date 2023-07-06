using System.IO;
using System.Text.Json;
using AppSpaces.App.Models;

namespace AppSpaces.App.Services;

public static class SettingsService
{
	private const string SettingsFile = "AppSpaces.settings.json";

	private static string SettingsPath => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppSpaces", SettingsFile);

	public static async Task<Settings> LoadSettings()
	{
		if (!File.Exists(SettingsPath))
		{
			return await CreateAndStoreDefaultSettings();
		}

		var stream = File.OpenText(SettingsPath);
		var settingsData = await stream.ReadToEndAsync();
		if (string.IsNullOrEmpty(settingsData))
		{
			return await CreateAndStoreDefaultSettings();
		}

		var settings = JsonSerializer.Deserialize<Settings>(settingsData);
		if (settings == null || !settings.Validate())
		{
			return await CreateAndStoreDefaultSettings();
		}
		stream.Dispose();

		// Store again in case the model had changed.
		await SaveSettings(settings);

		return settings;
	}

	private static async Task<Settings> CreateAndStoreDefaultSettings()
	{
		var defaultSettings = DefaultSettings.Create();
		await SaveSettings(defaultSettings);

		return defaultSettings;
	}

	public static async Task SaveSettings(Settings settings)
	{
		if (!settings.Validate())
		{
			throw new ApplicationException("Invalid settings provided", new ApplicationException(JsonSerializer.Serialize(settings)));
		}

		var file = new FileInfo(SettingsPath);
		file.Directory?.Create();

		var settingsData = JsonSerializer.Serialize(settings, new JsonSerializerOptions
		{
			WriteIndented = true
		});

		await TrySaveSettings(SettingsPath, settingsData);
	}

	private static async Task TrySaveSettings(string path, string data)
	{
		var attempts = 0;
		while (true)
		{
			try
			{
				await File.WriteAllTextAsync(path, data);
				return;
			}
			catch (Exception)
			{
				attempts++;
				if (attempts > 5) throw;

				Thread.Sleep(500);
			}
		}
	}
}