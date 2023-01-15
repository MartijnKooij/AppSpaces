namespace AppSpaces.App.Models;

public class Settings
{
	public Guid ActiveAppSpaceId { get; set; } = Guid.Empty;
	public List<AppSpace> AppSpaces { get; set; }

	public bool Validate()
	{
		if (!AppSpaces.Any()) return false;
		if (ActiveAppSpaceId == Guid.Empty)
		{
			ActiveAppSpaceId = AppSpaces.First().Id;
		}

		return true;
	}
}