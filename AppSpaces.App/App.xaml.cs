using System.Windows;
using AppSpaces.App.Models;
using AppSpaces.App.Services;

namespace AppSpaces.App;

public partial class App
{
	private readonly WindowService _windowService;
	private StreamingWindow? _streamingWindow;
	private Settings _settings = null!;

	private static TaskbarIcon? _trayIcon;
	private static LowLevelKeyboardHook? _keyboardHooks;

	public App()
	{
		_windowService = new WindowService();
	}

	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		_settings = await SettingsManager.LoadSettings();
		_windowService.Start(_settings);

		InitializeKeyboardManagement();
		InitializeTrayIcon();
	}

	protected override void OnExit(ExitEventArgs e)
	{
		_windowService.Stop();
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

	private async void HandleKeyUp(object? sender, KeyboardEventArgs e)
	{
		if (e.Keys.Are(Key.LeftWindows, Key.Control, Key.Alt, Key.PageUp))
		{
			_windowService.ActivateWindowInSpace(false);
			return;
		}
		if (e.Keys.Are(Key.LeftWindows, Key.Control, Key.Alt, Key.PageDown))
		{
			_windowService.ActivateWindowInSpace(true);
			return;
		}
		var registeredShortcut = _settings.KeyboardShortcuts.SingleOrDefault(shortcut => e.Keys.Are(shortcut.AllKeys));
		if (registeredShortcut == null) return;

		_settings.ActiveAppSpaceId = registeredShortcut.AppSpaceId;
		await SettingsManager.SaveSettings(_settings);

		_windowService.SnapAllWindowsToRegisteredAppSpace();
	}

	private void InitializeTrayIcon()
	{
		_trayIcon = (TaskbarIcon)Resources["TrayIcon"];
		_trayIcon.ForceCreate();

		var trayIconContext = (TrayIconViewModel)_trayIcon.DataContext;
		trayIconContext.Settings += OnShowSettings;
		trayIconContext.Streaming += OnStreaming;

	}

	private static void OnShowSettings()
	{
		var window = new SettingsWindow();
		window.Show();
	}

	private void OnStreaming(object? sender, bool shouldStart)
	{
		if (shouldStart)
		{
			_streamingWindow = new StreamingWindow(_windowService.GetStreamingSpace());
			_streamingWindow.Show();
		}
		else
		{
			_streamingWindow?.Close();
		}

	}
}