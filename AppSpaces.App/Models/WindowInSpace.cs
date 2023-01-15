using WinMan;

namespace AppSpaces.App.Models;

public class WindowInSpace
{
	public IWindow Window { get; set; }
	public AppSearch? MatchedAppSearch { get; set; }
}