using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using H.NotifyIcon;

namespace AppSpaces.App.Wpf;

public partial class TrayIconViewModel : ObservableObject
{
	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(ShowWindowCommand))]
	public bool canExecuteShowWindow = true;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(HideWindowCommand))]
	public bool canExecuteHideWindow;

	/// <summary>
	/// Shows a window, if none is already open.
	/// </summary>
	[RelayCommand(CanExecute = nameof(CanExecuteShowWindow))]
	public void ShowWindow()
	{
		Application.Current.MainWindow ??= new SettingsWindow();
		Application.Current.MainWindow.Show(disableEfficiencyMode: true);
		CanExecuteShowWindow = false;
		CanExecuteHideWindow = true;
	}

	/// <summary>
	/// Hides the main window. This command is only enabled if a window is open.
	/// </summary>
	[RelayCommand(CanExecute = nameof(CanExecuteHideWindow))]
	public void HideWindow()
	{
		Application.Current.MainWindow.Hide(enableEfficiencyMode: true);
		CanExecuteShowWindow = true;
		CanExecuteHideWindow = false;
	}

	/// <summary>
	/// Shuts down the application.
	/// </summary>
	[RelayCommand]
	public void ExitApplication()
	{
		Application.Current.Shutdown();
	}
}