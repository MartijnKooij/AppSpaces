using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppSpaces.App;

public partial class TrayIconViewModel : ObservableObject
{
	public event EventHandler? Settings;

	[RelayCommand]
	private void ShowSettings()
	{
		Settings?.Invoke(this, EventArgs.Empty);
	}

	[RelayCommand]
	private void ExitApplication()
	{
		Application.Current.Shutdown();
	}
}