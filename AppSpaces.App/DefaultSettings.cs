using AppSpaces.App.Models;

namespace AppSpaces.App;

public static class DefaultSettings
{
	public static Settings Create()
	{
		using var workspace = new Win32Workspace();
		workspace.Open();

		var defaultId0 = Guid.NewGuid();
		var defaultId1 = Guid.NewGuid();

		var settings = new Settings
		{
			ActiveAppSpaceId = defaultId0,
			AutomaticallyLaunch = true,
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
			WorkSpaces = new List<WorkSpace>
			{
				CreateDefaultWorkSpace(workspace, defaultId0, defaultId1)
			}
		};

		return settings;
	}

	public static WorkSpace CreateDefaultWorkSpace(IWorkspace workspace, Guid defaultId0, Guid defaultId1)
	{
		var defaultWorkSpace = new WorkSpace
		{
			WorkSpaceBounds = WorkspaceBounds.Create(workspace.DisplayManager.VirtualDisplayBounds),
			Label = $"WorkSpace {WorkspaceBounds.Create(workspace.DisplayManager.VirtualDisplayBounds)}",
			AppSpaces = new List<AppSpace>
			{
				new()
				{
					Id = defaultId0,
					Label = "Working",
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
							IsStreaming = false,
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
							Location = new ScreenLocation(workspace.DisplayManager.PrimaryDisplay.WorkArea.Width / 3 * 2, 0, workspace.DisplayManager.PrimaryDisplay.WorkArea.Width / 3 * 1,
								workspace.DisplayManager.PrimaryDisplay.WorkArea.Height)
						}
					}
				},
				new()
				{
					Id = defaultId1,
					Label = "Sharing",
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
							IsPrimary = false,
							IsStreaming = true,
							Location = new ScreenLocation(0, 0, 1920, 1080)
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
							Location = new ScreenLocation(0, 1080, 1920, Math.Max(0, workspace.DisplayManager.PrimaryDisplay.WorkArea.Height - 1080))
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
							IsPrimary = true,
							IsStreaming = false,
							Location = new ScreenLocation(1920, 0, Math.Max(0, workspace.DisplayManager.PrimaryDisplay.WorkArea.Width - 1920),
								workspace.DisplayManager.PrimaryDisplay.WorkArea.Height)
						}
					}
				}
			}
		};

		return defaultWorkSpace;
	}
}