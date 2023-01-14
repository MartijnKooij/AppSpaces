using AppSpaces.App.Models;
using WinMan;
using WinMan.Windows;

namespace AppSpaces.App;

public sealed partial class App
{
	public static TaskbarIcon? TrayIcon { get; private set; }
	public static Window? Window { get; set; }
	public static Settings? Settings { get; set; }

	public static IWorkspace? Workspace { get; private set; }

	private static Guid activeAppSpaceId = Guid.Empty;

	public App()
	{
		InitializeComponent();
	}

	protected override async void OnLaunched(LaunchActivatedEventArgs args)
	{
		Settings = await SettingsManager.LoadSettings();

		//ToDo: Update active
		activeAppSpaceId = Settings.AppSpaces.First().Id;

		InitializeWindowManagement();


		InitializeTrayIcon();
	}

	private static void InitializeWindowManagement()
	{
		Workspace = new Win32Workspace();
		Workspace.WindowManaging += (_, args) => RegisterWindow(args.Source);
		Workspace.WindowAdded += (_, args) => RegisterWindow(args.Source);
		Workspace.Open();
	}

	private static void RegisterWindow(IWindow window)
	{
		var isRegistered = SnapToAppSpaceIfRegistered(window, false);
		if (isRegistered)
		{
			window.PositionChangeEnd += (_, windowPositionChangedEventArgs) =>
				SnapToAppSpaceIfRegistered(windowPositionChangedEventArgs.Source, true);
		}
	}

	private static bool SnapToAppSpaceIfRegistered(IWindow window, bool isMoving)
	{
		// TODO: The active space should probably be managed by settings? Maybe not, depends on how we will switch...
		var activeAppSpace = Settings?.AppSpaces.FirstOrDefault(a => a.Id == App.activeAppSpaceId);
		if (activeAppSpace == null)
		{
			// TODO: Should not happen, should we throw?
			return false;
		}

		var windowSpace = activeAppSpace.Spaces
			.SingleOrDefault(s => s.Apps
				.Any(a => a.IsMatch(window)));

		if (windowSpace == null)
		{
			var primarySpace = activeAppSpace.Spaces.Single(s => s.IsPrimary);
			SnapToSpace(window, primarySpace, isMoving);
			return true;
		}

		SnapToSpace(window, windowSpace, isMoving);
		return true;
	}

	private static void SnapToSpace(IWindow window, Space space, bool isMoving)
	{
		var shouldMove = !isMoving || (space.Location.Left <= window.Position.Left &&
		                               space.Location.Right >= window.Position.Right &&
		                               space.Location.Top <= window.Position.Top &&
		                               space.Location.Bottom >= window.Position.Bottom);

		if (!shouldMove) return;

		window.SetState(WindowState.Restored);
		window.SetPosition(space.Location);
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

	private static void ShowHideWindowCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
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

	private static void ExitApplicationCommand_ExecuteRequested(object? _, ExecuteRequestedEventArgs args)
	{
		Workspace?.Dispose();
		TrayIcon?.Dispose();
		Window?.Close();
	}
}
