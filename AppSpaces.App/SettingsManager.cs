using System.Text.Json;
using AppSpaces.App.Models;

namespace AppSpaces.App;

public static class SettingsManager
{
	private const string SettingsFile = "AppSpaces.settings.json";

	public static async Task<Settings> LoadSettings()
	{
		var settingsPath = Path.Join(Environment.SpecialFolder.LocalApplicationData.ToString(), SettingsFile);
		if (!File.Exists(settingsPath))
		{
			return await DefaultSettings.Create();
		}

		var stream = File.OpenText(settingsPath);
		var settingsData = await stream.ReadToEndAsync();
		if (string.IsNullOrEmpty(settingsData))
		{
			return await DefaultSettings.Create();
		}

		var settings = JsonSerializer.Deserialize<Settings>(settingsData);
		if (settings == null || !settings.Validate())
		{
			return await DefaultSettings.Create();
		}
		stream.Dispose();

		// Store again in case the model had changed.
		await SaveSettings(settings);

		return settings;
	}

	public static async Task SaveSettings(Settings settings)
	{
		if (!settings.Validate())
		{
			throw new ApplicationException("Invalid settings provided", new ApplicationException(JsonSerializer.Serialize(settings)));
		}

		var settingsPath = Path.Join(Environment.SpecialFolder.LocalApplicationData.ToString(), SettingsFile);
		var file = new FileInfo(settingsPath);
		file.Directory?.Create();

		var settingsData = JsonSerializer.Serialize(settings, new JsonSerializerOptions
		{
			WriteIndented = true
		});

		await TrySaveSettings(settingsPath, settingsData);
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