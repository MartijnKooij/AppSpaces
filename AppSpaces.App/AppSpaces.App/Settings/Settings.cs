using System.Drawing;

namespace AppSpaces.App.Settings;

internal enum SearchType
{
	Title,
	ExecutablePath
}

internal class App
{
	public SearchType SearchType { get; set; }
	public string SearchQuery { get; set; }
}

internal class Space
{
	public Rectangle Location { get; set; }
	public bool IsPrimary { get; set; }
	public List<App> Apps { get; set; } = new List<App>();
}

internal class AppSpace
{
	public List<Space> Spaces { get; set; } = new List<Space>();
}

internal class Settings
{
	public List<AppSpace> AppSpaces { get; set; } = new List<AppSpace>();
}