using WinMan;

namespace AppSpaces.App.Models;

public class AppSearch
{
	public SearchType SearchType { get; set; } = SearchType.Title;
	public string SearchQuery { get; set; } = "";

	public bool IsMatch(IWindow window)
	{
		//TODO support exe and better wildcard handling
		return window.Title.Contains(SearchQuery, StringComparison.CurrentCultureIgnoreCase);
	}
}