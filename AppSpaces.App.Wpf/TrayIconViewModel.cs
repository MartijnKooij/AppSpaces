using System.Windows;
using AppSpaces.App.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppSpaces.App.Wpf;

public partial class TrayIconViewModel : ObservableObject
{
	[RelayCommand]
	private void ShowSettings()
	{
		var window = new SettingsWindow();
		window.Show();
	}

	[RelayCommand]
	private async void StartStreaming()
	{
		var settings = await SettingsManager.LoadSettings();
		var windowService = new WindowService();
		windowService.Start(settings);

		var window = new StreamingWindow(windowService.GetStreamingSpace());
		window.Show();
	}

	[RelayCommand]
	private void ExitApplication()
	{
		Application.Current.Shutdown();
	}
}