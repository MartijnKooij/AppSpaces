﻿using AppSpaces.App.Models;
using H.Hooks;

namespace AppSpaces.App;

public static class DefaultSettings
{
	public static async Task<Settings> Create()
	{
		var defaultId0 = Guid.NewGuid();
		var defaultId1 = Guid.NewGuid();

		var settings = new Settings
		{
			ActiveAppSpaceId = defaultId0,
			KeyboardShortcuts = new List<KeyboardShortcut>
			{
				new()
				{
					AppSpaceId = defaultId0,
					UserKey = Key.NumPad0
				},
				new()
				{
					AppSpaceId = defaultId1,
					UserKey = Key.NumPad1
				}
			},
			AppSpaces = new List<AppSpace>
			{
				new()
				{
					Id = defaultId0,
					Label = "Test space 0",
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
							Location = new ScreenLocation(0, 0, 2340, 1560)
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
							Location = new ScreenLocation(2340, 0, 1500, 1560)
						}
					}
				},
				new()
				{
					Id = defaultId1,
					Label = "Test space 1",
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
							Location = new ScreenLocation(0, 0, 2340, 1560)
						},
						new()
						{
							Apps = new List<AppSearch>
							{
								new()
								{
									SearchType = SearchType.Title,
									SearchQuery = "Firefox"
								}
							},
							IsPrimary = false,
							Location = new ScreenLocation(2340, 0, 1500, 900)
						},
						new()
						{
							Apps = new List<AppSearch>
							{
								new()
								{
									SearchType = SearchType.Title,
									SearchQuery = "Total Commander"
								}
							},
							IsPrimary = false,
							Location = new ScreenLocation(2340, 900, 1500, 660)
						}
					}
				}
			}
		};

		await SettingsManager.SaveSettings(settings);

		return settings;
	}
}