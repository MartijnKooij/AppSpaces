namespace AppSpaces.App.Models;

public class WindowInSpace
{
	public IWindow? Window { get; init; }
	public AppSearch? MatchedAppSearch { get; set; }
}