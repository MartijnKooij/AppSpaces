using System.Text.Json.Serialization;
using WinMan;

namespace AppSpaces.App.Models;

public class Space
{
	public ScreenLocation Location { get; set; }
	public bool IsPrimary { get; set; }
	public List<AppSearch> Apps { get; set; }

	[JsonIgnore] public List<WindowInSpace> Windows { get; set; } = new();
}