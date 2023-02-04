using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppSpaces.App;

public partial class TrayIconViewModel : ObservableObject
{
	public event EventHandler? Settings;

	public event EventHandler<bool>? Streaming;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(StartStreamingCommand))]
	private bool _canExecuteStartStreaming = true;

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(StopStreamingCommand))]
	private bool _canExecuteStopStreaming;

	[RelayCommand]
	private void ShowSettings()
	{
		Settings?.Invoke(this, EventArgs.Empty);
	}

	[RelayCommand(CanExecute = nameof(CanExecuteStartStreaming))]
	private void StartStreaming()
	{
		Streaming?.Invoke(this, true);
		CanExecuteStartStreaming = false;
		CanExecuteStopStreaming = true;
	}

	[RelayCommand(CanExecute = nameof(CanExecuteStopStreaming))]
	private void StopStreaming()
	{
		Streaming?.Invoke(this, false);
		CanExecuteStartStreaming = true;
		CanExecuteStopStreaming = false;
	}

	[RelayCommand]
	private void ExitApplication()
	{
		Application.Current.Shutdown();
	}
}