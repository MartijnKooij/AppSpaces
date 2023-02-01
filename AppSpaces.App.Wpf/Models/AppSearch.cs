using AppSpaces.App.Wpf.Extensions;

namespace AppSpaces.App.Wpf.Models;

public class AppSearch
{
	public SearchType SearchType { get; set; } = SearchType.Title;
	public string SearchQuery { get; set; } = "";

	public bool IsMatch(IWindow window)
	{
		var processExe = window.GetProcessExe();
		switch (SearchType)
		{
			case SearchType.Title:
				var isTitleMatch = window.Title.Contains(SearchQuery, StringComparison.CurrentCultureIgnoreCase);
				if (!isTitleMatch) return false;

				// Switch to exe matching for safer matches as app titles can change.
				SearchType = SearchType.ExecutablePath;
				SearchQuery = processExe;
				return isTitleMatch;
			case SearchType.ExecutablePath:
				return processExe.Equals(SearchQuery, StringComparison.InvariantCultureIgnoreCase);
			default:
				throw new ApplicationException($"Unsupported SearchType {SearchType}");
		}
		
	}
}