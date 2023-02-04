using System.Text.Json.Serialization;

namespace AppSpaces.App.Models;

public class Space
{
	public ScreenLocation Location { get; init; } = new(0, 0, 1024, 768);
	public bool IsPrimary { get; set; }
	public bool IsStreaming { get; set; }
	public List<AppSearch> Apps { get; init; } = new();

	[JsonIgnore] public List<WindowInSpace> Windows { get; set; } = new();
}