using System.Windows;
using AppSpaces.App.Wpf.Models;
using AppSpaces.App.Wpf.Services;

namespace AppSpaces.App.Wpf;

public partial class App
{
	private readonly WindowService _windowService;
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

		_trayIcon = FindResource("TrayIcon") as TaskbarIcon;
		_trayIcon?.ForceCreate();
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
	}
}