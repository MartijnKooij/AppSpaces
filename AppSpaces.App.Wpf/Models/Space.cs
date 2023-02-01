using System.Text.Json.Serialization;

namespace AppSpaces.App.Wpf.Models;

public class Space
{
	public ScreenLocation Location { get; set; }
	public bool IsPrimary { get; set; }
	public bool IsStreaming { get; set; }
	public List<AppSearch> Apps { get; set; }

	[JsonIgnore] public List<WindowInSpace> Windows { get; set; } = new();
}