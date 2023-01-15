using AppSpaces.App.Models;
using WinMan;
using WinMan.Windows;

namespace AppSpaces.App;

public sealed partial class App
{
	private static TaskbarIcon? _trayIcon;
	private static Window? _window;
	private static Settings? _settings;
	private static IWorkspace? _workspace;
	private static Guid _activeAppSpaceId = Guid.Empty;

	public App()
	{
		InitializeComponent();
	}

	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		_settings = await SettingsManager.LoadSettings();

		//ToDo: Update active
		_activeAppSpaceId = _settings.AppSpaces.First().Id;

		InitializeWindowManagement();


		InitializeTrayIcon();
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

	private static void SnapToRegisteredAppSpace(IWindow window)
	{
		if (!GetActiveSpace(out var activeAppSpace)) return;

		var windowSpace = activeAppSpace!.Spaces
			.SingleOrDefault(s => s.Apps
				.Any(a => a.IsMatch(window)));

		if (windowSpace == null)
		{
			return;
		}

		SnapToSpace(window, windowSpace);
	}

	private static void SnapToContainingAppSpace(IWindow window)
	{
		if (!GetActiveSpace(out var activeAppSpace)) return;

		var pointerLocation = new ScreenLocation(_workspace!.CursorLocation.X, _workspace.CursorLocation.Y, 1, 1);
		var windowLocation = new ScreenLocation(window.Position.Left, window.Position.Top, window.Position.Width, window.Position.Height);
		var containingSpace = activeAppSpace!.Spaces.SingleOrDefault(space => space.Location.HitTest(pointerLocation) || space.Location.HitTest(windowLocation));
		if (containingSpace == null) return;

		SnapToSpace(window, containingSpace);
	}

	private static bool GetActiveSpace(out AppSpace? activeAppSpace)
	{
		// TODO: The active space should probably be managed by settings? Maybe not, depends on how we will switch...
		activeAppSpace = _settings?.AppSpaces.FirstOrDefault(a => a.Id == App._activeAppSpaceId);
		if (activeAppSpace != null) return true;

		// TODO: We should enforce that an active app space is always present so we don't have to do these checks in here.
		activeAppSpace = _settings?.AppSpaces.First();
		return activeAppSpace != null;
	}

	private static void SnapToSpace(IWindow window, Space space)
	{
		window.SetState(WindowState.Restored);
		window.SetPosition(new Rectangle(space.Location.X, space.Location.Y, space.Location.X + space.Location.Width, space.Location.Y + space.Location.Height));
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
		_trayIcon?.Dispose();
		_window?.Close();
	}
}
