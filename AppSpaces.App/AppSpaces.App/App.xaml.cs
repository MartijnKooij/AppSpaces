﻿namespace AppSpaces.App;

public sealed partial class App
{
	public static TaskbarIcon? TrayIcon { get; private set; }
	public static Window? Window { get; set; }

	public App()
	{
		InitializeComponent();
	}

	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		InitializeTrayIcon();
	}

	private void InitializeTrayIcon()
	{
		var showHideWindowCommand = (XamlUICommand)Resources["ShowHideWindowCommand"];
		showHideWindowCommand.ExecuteRequested += ShowHideWindowCommand_ExecuteRequested;

		var exitApplicationCommand = (XamlUICommand)Resources["ExitApplicationCommand"];
		exitApplicationCommand.ExecuteRequested += ExitApplicationCommand_ExecuteRequested;

		TrayIcon = (TaskbarIcon)Resources["TrayIcon"];
		TrayIcon.ForceCreate();
	}

	private void ShowHideWindowCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
	{
		if (Window == null)
		{
			Window = new Window();
			Window.Show();
			return;
		}

		if (Window.Visible)
		{
			Window.Hide();
		}
		else
		{
			Window.Show();
		}
	}

	private void ExitApplicationCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
	{
		TrayIcon?.Dispose();
		Window?.Close();
	}
}
