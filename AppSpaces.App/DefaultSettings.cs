using AppSpaces.App.Models;

namespace AppSpaces.App;

public static class DefaultSettings
{
	public static async Task<Settings> Create()
	{
		var settings = new Settings
		{
			AppSpaces = new List<AppSpace>
			{
				new()
				{
					Spaces = new List<Space>
					{
						new()
						{
							Apps = new List<AppSearch>
							{
								new()
								{
									SearchType = SearchType.Title,
									SearchQuery = "Visual Studio Code"
								}
							},
							IsPrimary = true,
							Location = new ScreenLocation(0, 0, 2840, 1560)
						},
						new()
						{
							Apps = new List<AppSearch>
							{
								new()
								{
									SearchType = SearchType.Title,
									SearchQuery = "Total Commander"
								},
								new()
								{
									SearchType = SearchType.Title,
									SearchQuery = "Firefox"
								}
							},
							IsPrimary = false,
							Location = new ScreenLocation(2840, 0, 1000, 1560)
						}

					}
				}
			}
		};

		await SettingsManager.SaveSettings(settings);

		return settings;
	}
}