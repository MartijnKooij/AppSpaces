namespace AppSpaces.App.Models;

public class Settings
{
	public Guid ActiveAppSpaceId { get; set; } = Guid.Empty;
	public List<AppSpace> AppSpaces { get; set; }
	public List<KeyboardShortcut> KeyboardShortcuts { get; set; }

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

		return true;
	}
}