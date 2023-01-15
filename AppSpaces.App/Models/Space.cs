namespace AppSpaces.App.Models;

public class Space
{
	public ScreenLocation Location { get; set; }
	public bool IsPrimary { get; set; }
	public List<AppSearch> Apps { get; set; }
}