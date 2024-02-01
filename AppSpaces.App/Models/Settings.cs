namespace AppSpaces.App.Models;

public class Settings
{
	public Guid ActiveAppSpaceId { get; set; } = Guid.Empty;
	public bool IsStreaming { get; set; }
	public List<WorkSpace> WorkSpaces { get; set; } = new();
	public List<KeyboardShortcut> KeyboardShortcuts { get; set; } = new();
	public bool AutomaticallyLaunch { get; set; } = true;

	public bool Validate()
	{
		if (!WorkSpaces.Any()) return false;
		foreach (var workSpace in WorkSpaces)
		{
			if (!workSpace.AppSpaces.Any()) return false;
			if (ActiveAppSpaceId == Guid.Empty)
			{
				ActiveAppSpaceId = workSpace.AppSpaces.First().Id;
			}

			foreach (var appSpace in workSpace.AppSpaces)
			{
				if (!appSpace.Spaces.Any()) return false;

				ValidateHasPrimary(appSpace);
				RemoveDuplicates(appSpace);
			}
		}

		return true;
	}

	private static void RemoveDuplicates(AppSpace appSpace)
	{
		var uniqueApps = new HashSet<string>();
		foreach (var space in appSpace.Spaces)
		{
			var newApps = space.Apps.Where(app => uniqueApps.Add(app.SearchQuery.ToLowerInvariant())).ToList();
			space.Apps = newApps;
		}
	}

	public List<AppSpace> GetAppSpacesForWorkSpace(IWorkspace winManWorkspace)
	{
		var workSpace = WorkSpaces.FirstOrDefault(w => w.WorkSpaceBounds.Equals(winManWorkspace.DisplayManager.VirtualDisplayBounds));
		if (workSpace == null)
		{
			var defaultId0 = Guid.NewGuid();
			workSpace = DefaultSettings.CreateDefaultWorkSpace(winManWorkspace, defaultId0, Guid.NewGuid());
			WorkSpaces.Add(workSpace);

			ActiveAppSpaceId = defaultId0;
		}

		return workSpace.AppSpaces;
	}

	private static void ValidateHasPrimary(AppSpace appSpace)
	{
		var hasPrimary = false;
		foreach (var space in appSpace.Spaces.Where(space => space.IsPrimary))
		{
			if (hasPrimary) space.IsPrimary = false;
			hasPrimary = true;
		}

		if (!hasPrimary)
		{
			appSpace.Spaces.First().IsPrimary = true;
		}
	}
}