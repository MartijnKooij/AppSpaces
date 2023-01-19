using AppSpaces.App.Models;
using AppSpaces.App.Services;
using H.Hooks;

namespace AppSpaces.App;

public sealed partial class App
{
	private readonly WindowService _windowService;
	private Settings _settings = null!;

	private static TaskbarIcon? _trayIcon;
	private static Window? _window;
	private static LowLevelKeyboardHook? _keyboardHooks;

	public App()
	{
		InitializeComponent();

		_windowService = new WindowService();
	}

	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		_settings = await SettingsManager.LoadSettings();
		_windowService.Start(_settings);

		InitializeKeyboardManagement();
		InitializeTrayIcon();
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
		var settingsCommand = (XamlUICommand)Resources["Settings"];
		settingsCommand.ExecuteRequested += ShowSettings;

		var exitApplicationCommand = (XamlUICommand)Resources["ExitApplicationCommand"];
		exitApplicationCommand.ExecuteRequested += ExitApplication;

		_trayIcon = (TaskbarIcon)Resources["TrayIcon"];
		_trayIcon.ForceCreate();
	}

	private static void ShowSettings(object? _, ExecuteRequestedEventArgs args)
	{
		if (_window == null)
		{
			_window = new Window();
			_window.Show();
			return;
		}

		if (_window.Visible)
		{
			_window.Hide();
		}
		else
		{
			_window.Show();
		}
	}

	private void ExitApplication(object? _, ExecuteRequestedEventArgs args)
	{
		_windowService.Stop();
		_keyboardHooks?.Stop();
		_keyboardHooks?.Dispose();
		_trayIcon?.Dispose();
		_window?.Close();
	}
}
