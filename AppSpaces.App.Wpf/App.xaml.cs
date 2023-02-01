using System.Windows;
using H.NotifyIcon;

namespace AppSpaces.App.Wpf;

public partial class App
{
	private TaskbarIcon? _trayIcon;

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		_trayIcon = FindResource("TrayIcon") as TaskbarIcon;
		_trayIcon?.ForceCreate();
	}

	protected override void OnExit(ExitEventArgs e)
	{
		_trayIcon?.Dispose();
		base.OnExit(e);
	}
}