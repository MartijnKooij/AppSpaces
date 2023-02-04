using AppSpaces.App.Models;

namespace AppSpaces.App;

public static class DefaultSettings
{
	public static async Task<Settings> Create()
	{
		using var workspace = new Win32Workspace();
		workspace.Open();

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
							IsStreaming = true,
							Location = new ScreenLocation(0, 0, workspace.DisplayManager.PrimaryDisplay.WorkArea.Width / 3 * 2, workspace.DisplayManager.PrimaryDisplay.WorkArea.Height)
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
							IsStreaming = false,
							Location = new ScreenLocation(workspace.DisplayManager.PrimaryDisplay.WorkArea.Width / 3 * 2, 0, workspace.DisplayManager.PrimaryDisplay.WorkArea.Width / 3 * 1, workspace.DisplayManager.PrimaryDisplay.WorkArea.Height)
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
							IsStreaming = true,
							Location = new ScreenLocation(0, 0, workspace.DisplayManager.PrimaryDisplay.WorkArea.Width / 3 * 2, workspace.DisplayManager.PrimaryDisplay.WorkArea.Height)
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
							IsStreaming = false,
							Location = new ScreenLocation(workspace.DisplayManager.PrimaryDisplay.WorkArea.Width / 3 * 2, 0, workspace.DisplayManager.PrimaryDisplay.WorkArea.Width / 3 * 1, workspace.DisplayManager.PrimaryDisplay.WorkArea.Height / 3 * 2)
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
							IsStreaming = false,
							Location = new ScreenLocation(workspace.DisplayManager.PrimaryDisplay.WorkArea.Width / 3 * 2, workspace.DisplayManager.PrimaryDisplay.WorkArea.Height / 3 * 2, workspace.DisplayManager.PrimaryDisplay.WorkArea.Width / 3 * 1, workspace.DisplayManager.PrimaryDisplay.WorkArea.Height / 3 * 1)
						}
					}
				}
			}
		};

		await SettingsManager.SaveSettings(settings);

		return settings;
	}
}