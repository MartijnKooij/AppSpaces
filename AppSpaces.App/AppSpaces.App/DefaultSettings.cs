using System.Drawing;
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
							Location = new Rectangle(new Point(0, 0), new Size(1120, 1040))
						},
						new()
						{
							Apps = new List<AppSearch>
							{
								new()
								{
									SearchType = SearchType.Title,
									SearchQuery = "Notepad"
								}
							},
							IsPrimary = true,
							Location = new Rectangle(new Point(1120, 0), new Size(800, 1040))
						}

					}
				}
			}
		};

		await SettingsManager.SaveSettings(settings);

		return settings;
	}
}