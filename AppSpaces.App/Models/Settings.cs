namespace AppSpaces.App.Models;

public class Settings
{
	public Guid ActiveAppSpaceId { get; set; } = Guid.Empty;
	public List<AppSpace> AppSpaces { get; set; } = new();
	public List<KeyboardShortcut> KeyboardShortcuts { get; set; } = new();

	public bool Validate()
	{
		if (!AppSpaces.Any()) return false;
		if (ActiveAppSpaceId == Guid.Empty)
		{
			ActiveAppSpaceId = AppSpaces.First().Id;
		}

		foreach (var appSpace in AppSpaces)
		{
			if (!appSpace.Spaces.Any()) return false;

			ValidateHasPrimary(appSpace);
			ValidateHasStreaming(appSpace);
		}

		return true;
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

	private static void ValidateHasStreaming(AppSpace appSpace)
	{
		var hasStreaming = false;
		foreach (var space in appSpace.Spaces.Where(space => space.IsStreaming))
		{
			if (hasStreaming) space.IsStreaming = false;
			hasStreaming = true;
		}

		if (!hasStreaming)
		{
			appSpace.Spaces.First().IsStreaming = true;
		}
	}
}