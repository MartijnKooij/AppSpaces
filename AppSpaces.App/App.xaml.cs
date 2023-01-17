using AppSpaces.App.Extensions;
using AppSpaces.App.Models;
using H.Hooks;
using WinMan;
using WinMan.Windows;

namespace AppSpaces.App;

public sealed partial class App
{
	private static TaskbarIcon? _trayIcon;
	private static Window? _window;
	private static Settings? _settings;
	private static IWorkspace? _workspace;
	private static LowLevelKeyboardHook? _keyboardHooks;

	public App()
	{
		InitializeComponent();
	}

	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		_settings = await SettingsManager.LoadSettings();

		InitializeWindowManagement();
		InitializeKeyboardManagement();
		InitializeTrayIcon();
	}

	private static void InitializeKeyboardManagement()
	{
		_keyboardHooks = new LowLevelKeyboardHook
		{
			IsExtendedMode = true
		};
		_keyboardHooks.Up += HandleKeyUp;
		_keyboardHooks.Start();
	}

	private static async void HandleKeyUp(object? sender, KeyboardEventArgs e)
	{
		if (e.Keys.Are(Key.LeftWindows, Key.Control, Key.Alt, Key.PageUp))
		{
			ActivateWindowInSpace(-1);
			return;
		}
		if (e.Keys.Are(Key.LeftWindows, Key.Control, Key.Alt, Key.PageDown))
		{
			ActivateWindowInSpace(1);
			return;
		}
		var registeredShortcut = _settings!.KeyboardShortcuts.SingleOrDefault(shortcut => e.Keys.Are(shortcut.AllKeys));
		if (registeredShortcut == null) return;

		_settings.ActiveAppSpaceId = registeredShortcut.AppSpaceId;
		await SettingsManager.SaveSettings(_settings);

		SnapAllWindowsToRegisteredAppSpace();
	}

	private static void ActivateWindowInSpace(int skip)
	{
		var activeAppSpace = _settings!.AppSpaces.Single(a => a.Id == _settings.ActiveAppSpaceId);
		var activeWindow = _workspace?.FocusedWindow;
		if (activeWindow == null)
		{
			activeWindow = activeAppSpace.Spaces.First().Windows.FirstOrDefault()?.Window;
			if (activeWindow == null) return;
		}

		var spaceOfActiveWindow =
			activeAppSpace.Spaces
				.FirstOrDefault(s => s.Windows
					.Any(w => w.Window.Handle == activeWindow.Handle));
		if (!(spaceOfActiveWindow?.Windows.Any() ?? false))
		{
			return;
		}

		var indexOfActiveWindow = spaceOfActiveWindow.Windows.FindIndex(w => w.Window.Handle == activeWindow.Handle);
		var activeIndex = indexOfActiveWindow + skip;
		if (activeIndex < 0) activeIndex = spaceOfActiveWindow.Windows.Count - 1;
		else if (activeIndex >= spaceOfActiveWindow.Windows.Count) activeIndex = 0;

		try
		{
			spaceOfActiveWindow.Windows[activeIndex].Window.RequestFocus();
		}
		catch (InvalidWindowReferenceException)
		{
			spaceOfActiveWindow.Windows.RemoveAt(activeIndex);
		}
	}

	private static void InitializeWindowManagement()
	{
		_workspace = new Win32Workspace();
		_workspace.WindowManaging += (_, args) => RegisterWindow(args.Source);
		_workspace.WindowAdded += (_, args) => RegisterWindow(args.Source);
		_workspace.Open();
	}

	private static void RegisterWindow(IWindow window)
	{
		SnapToRegisteredAppSpace(window);
		window.PositionChangeEnd += (_, windowPositionChangedEventArgs) =>
			SnapToContainingAppSpace(windowPositionChangedEventArgs.Source);

	}

	private static void SnapAllWindowsToRegisteredAppSpace()
	{
		foreach (var window in _workspace!.GetSnapshot())
		{
			SnapToRegisteredAppSpace(window);
		}
	}

	private static void SnapToRegisteredAppSpace(IWindow window)
	{
		var activeAppSpace = _settings!.AppSpaces.Single(a => a.Id == _settings.ActiveAppSpaceId);
		var matchedWindowSpace = activeAppSpace.Spaces
			.SingleOrDefault(s => s.Apps
				.Any(a => a.IsMatch(window)));
		var windowSpace = matchedWindowSpace ?? activeAppSpace.Spaces.Single(s => s.IsPrimary);

		if (!SnapToSpace(window, windowSpace)) return;

		var matchedAppSearch = matchedWindowSpace?.Apps.Single(a => a.IsMatch(window));
		RegisterWindowInSpace(window, windowSpace, matchedAppSearch);
	}

	private static void SnapToContainingAppSpace(IWindow window)
	{
		var activeAppSpace = _settings!.AppSpaces.Single(a => a.Id == _settings.ActiveAppSpaceId);

		var pointerLocation = new ScreenLocation(_workspace!.CursorLocation.X, _workspace.CursorLocation.Y, 1, 1);
		var windowLocation = new ScreenLocation(window.Position.Left, window.Position.Top, window.Position.Width, window.Position.Height);
		var containingSpace = activeAppSpace.Spaces.SingleOrDefault(space => space.Location.HitTest(pointerLocation) || space.Location.HitTest(windowLocation));
		if (containingSpace == null) return;

		if (!SnapToSpace(window, containingSpace)) return;

		RegisterWindowInSpace(window, containingSpace);
	}

	private static async void RegisterWindowInSpace(IWindow window, Space space, AppSearch? matchedAppSearch = null)
	{
		var activeAppSpace = _settings!.AppSpaces.Single(a => a.Id == _settings.ActiveAppSpaceId);
		var newAppSearch = matchedAppSearch ?? new AppSearch
		{
			SearchType = SearchType.ExecutablePath,
			SearchQuery = window.GetProcessExe()
		};

		// Remove this window from any spaces where it might already be registered in.
		foreach (var otherSpace in activeAppSpace.Spaces)
		{
			otherSpace.Windows.RemoveAll(w => w.Window.Handle == window.Handle);
			otherSpace.Apps.RemoveAll(a =>
				a.SearchType == newAppSearch.SearchType && a.SearchQuery == newAppSearch.SearchQuery);
		}

		// Add it to the current space.
		space.Windows.Add(new WindowInSpace
		{
			Window = window,
			MatchedAppSearch = matchedAppSearch
		});
		space.Apps.Add(newAppSearch);
		await SettingsManager.SaveSettings(_settings);
	}

	private static bool SnapToSpace(IWindow window, Space space)
	{
		if (!window.CanMove) return false;

		window.SetState(WindowState.Restored);
		window.SetPosition(new Rectangle(space.Location.X - window.FrameMargins.Left, space.Location.Y - window.FrameMargins.Top, space.Location.X + space.Location.Width + window.FrameMargins.Right, space.Location.Y + space.Location.Height + window.FrameMargins.Bottom));

		return true;
	}

	private void InitializeTrayIcon()
	{
		var showHideWindowCommand = (XamlUICommand)Resources["ShowHideWindowCommand"];
		showHideWindowCommand.ExecuteRequested += ShowHideWindowCommand_ExecuteRequested;

		var exitApplicationCommand = (XamlUICommand)Resources["ExitApplicationCommand"];
		exitApplicationCommand.ExecuteRequested += ExitApplicationCommand_ExecuteRequested;

		_trayIcon = (TaskbarIcon)Resources["TrayIcon"];
		_trayIcon.ForceCreate();
	}

	private static void ShowHideWindowCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
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

	private static void ExitApplicationCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
	{
		_workspace?.Dispose();
		_keyboardHooks?.Stop();
		_keyboardHooks?.Dispose();
		_trayIcon?.Dispose();
		_window?.Close();
	}
}
