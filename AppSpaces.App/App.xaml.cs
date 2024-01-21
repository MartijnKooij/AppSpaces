using System.IO;
using System.Windows;
using AppSpaces.App.Models;
using AppSpaces.App.Services;
using Serilog;
using static System.Diagnostics.Process;
using Registry = Microsoft.Win32.Registry;

namespace AppSpaces.App;

public partial class App
{
	private readonly WindowService windowService;
	private StreamingWindow streamingWindow;
	private Settings settings = null!;

	private static TaskbarIcon _trayIcon;
	private static LowLevelKeyboardHook _keyboardHooks;

	public App()
	{
		windowService = new WindowService();

		var logFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AppSpaces", "Logs", "AppSpaces.log");

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
			.CreateLogger();

		Log.Information("Starting AppSpaces...");
	}

	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		settings = await SettingsService.LoadSettings();
		windowService.Start(settings);

		InitializeKeyboardManagement();
		InitializeTrayIcon();
		UpdateStreaming();

		RegisterStartOnBoot();
	}

	protected override void OnExit(ExitEventArgs e)
	{
		windowService.Stop();
		_keyboardHooks?.Stop();
		_keyboardHooks?.Dispose();
		_trayIcon?.Dispose();

		base.OnExit(e);
	}

	private void InitializeKeyboardManagement()
	{
		_keyboardHooks = new LowLevelKeyboardHook
		{
			IsExtendedMode = true
		};
		_keyboardHooks.Up += HandleKeyUp;
		_keyboardHooks.Start();
	}

	private async void HandleKeyUp(object sender, KeyboardEventArgs e)
	{
		if (e.Keys.Are(Key.LeftWindows, Key.Control, Key.Alt, Key.PageUp))
		{
			windowService.ActivateWindowInSpace(false);
			return;
		}
		if (e.Keys.Are(Key.LeftWindows, Key.Control, Key.Alt, Key.PageDown))
		{
			windowService.ActivateWindowInSpace(true);
			return;
		}
		var registeredShortcut = settings.KeyboardShortcuts.SingleOrDefault(shortcut => e.Keys.Are(shortcut.AllKeys));
		if (registeredShortcut == null) return;

		settings.ActiveAppSpaceId = registeredShortcut.AppSpaceId;
		await SettingsService.SaveSettings(settings);

		// Open/Close streaming before re-arranging windows...
		UpdateStreaming();
		await windowService.SnapAllWindowsToRegisteredAppSpace();
	}

	private void UpdateStreaming()
	{
		if (!Dispatcher.CheckAccess())
		{
			Dispatcher.Invoke(UpdateStreaming);
			return;
		}

		if (!windowService.HasStreamingSpace())
		{
			streamingWindow?.Close();
		}
		else
		{
			if (streamingWindow is not { IsVisible: true })
			{
				streamingWindow = new StreamingWindow(windowService.GetStreamingSpace());
			}

			streamingWindow.Show();
		}
	}

	private void RegisterStartOnBoot()
	{
		var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

		if (!settings.AutomaticallyLaunch)
		{
			key?.DeleteValue("AppSpaces");
			return;
		}

		var exePath = GetCurrentProcess().MainModule?.FileName;
		if (!string.IsNullOrEmpty(exePath) && exePath.EndsWith("exe"))
		{
			key?.SetValue("AppSpaces", exePath);
		}
	}

	private void InitializeTrayIcon()
	{
		_trayIcon = (TaskbarIcon)Resources["TrayIcon"];
		_trayIcon.ForceCreate();

		var trayIconContext = (TrayIconViewModel)_trayIcon.DataContext;
		trayIconContext.Initialize(settings, windowService);
		
		trayIconContext.OnSettings += OnShowOnSettings;
		trayIconContext.OnActivateAppSpace += OnActivateAppSpace;
	}

	private async void OnActivateAppSpace(object sender, ActivateAppSpaceEventArgs e)
	{
		settings.ActiveAppSpaceId = e.AppSpaceId;
		await SettingsService.SaveSettings(settings);

		// Open/Close streaming before re-arranging windows...
		UpdateStreaming();
		await windowService.SnapAllWindowsToRegisteredAppSpace();
	}

	private static void OnShowOnSettings(object sender, EventArgs eventArgs)
	{
		var window = new SettingsWindow();
		window.Show();
	}
}