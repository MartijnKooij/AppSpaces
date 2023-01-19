using AppSpaces.App.Extensions;
using AppSpaces.App.Models;
using WinMan;
using WinMan.Windows;

namespace AppSpaces.App.Services;

public class WindowService
{
	private readonly IWorkspace _workspace;
	private Settings _settings = null!;

	private AppSpace ActiveAppSpace => _settings.AppSpaces.Single(a => a.Id == _settings.ActiveAppSpaceId);

	public WindowService()
	{
		_workspace = new Win32Workspace();
		_workspace.WindowManaging += (_, args) => RegisterWindow(args.Source);
		_workspace.WindowAdded += (_, args) => RegisterWindow(args.Source);
	}

	public void Start(Settings settings)
	{
		_settings = settings;
		_workspace.Open();
	}

	public void Stop()
	{
		_workspace.Dispose();
	}

	public void SnapAllWindowsToRegisteredAppSpace()
	{
		foreach (var window in _workspace.GetSnapshot())
		{
			SnapToRegisteredAppSpace(window);
		}
	}

	public void ActivateWindowInSpace(bool moveForward)
	{
		var activeAppSpace = _settings.AppSpaces.Single(a => a.Id == _settings.ActiveAppSpaceId);
		var activeWindow = _workspace.FocusedWindow;
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
		var activeIndex = indexOfActiveWindow + (moveForward ? 1 : -1);
		if (activeIndex < 0) activeIndex = spaceOfActiveWindow.Windows.Count - 1;
		else if (activeIndex >= spaceOfActiveWindow.Windows.Count) activeIndex = 0;

		try
		{
			spaceOfActiveWindow.Windows[activeIndex].Window.ForceForegroundWindow();
		}
		catch (InvalidWindowReferenceException)
		{
			spaceOfActiveWindow.Windows.RemoveAt(activeIndex);
		}
	}

	private void RegisterWindow(IWindow window)
	{
		SnapToRegisteredAppSpace(window);
		window.PositionChangeEnd += (_, windowPositionChangedEventArgs) =>
			SnapToContainingAppSpace(windowPositionChangedEventArgs.Source);
	}

	private void SnapToRegisteredAppSpace(IWindow window)
	{
		var matchedWindowSpace = ActiveAppSpace.Spaces
			.SingleOrDefault(s => s.Apps
				.Any(a => a.IsMatch(window)));
		var windowSpace = matchedWindowSpace ?? ActiveAppSpace.Spaces.Single(s => s.IsPrimary);

		if (!SnapToSpace(window, windowSpace)) return;

		var matchedAppSearch = matchedWindowSpace?.Apps.Single(a => a.IsMatch(window));
		RegisterWindowInSpace(window, windowSpace, matchedAppSearch);
	}

	private void SnapToContainingAppSpace(IWindow window)
	{
		var pointerLocation = new ScreenLocation(_workspace.CursorLocation.X, _workspace.CursorLocation.Y, 1, 1);
		var windowLocation = new ScreenLocation(window.Position.Left, window.Position.Top, window.Position.Width, window.Position.Height);
		var containingSpace = ActiveAppSpace.Spaces.SingleOrDefault(space => space.Location.HitTest(pointerLocation) || space.Location.HitTest(windowLocation));
		if (containingSpace == null) return;

		if (!SnapToSpace(window, containingSpace)) return;

		RegisterWindowInSpace(window, containingSpace);
	}

	private async void RegisterWindowInSpace(IWindow window, Space space, AppSearch? matchedAppSearch = null)
	{
		var newAppSearch = matchedAppSearch ?? new AppSearch
		{
			SearchType = SearchType.ExecutablePath,
			SearchQuery = window.GetProcessExe()
		};

		// Remove this window from any spaces where it might already be registered in.
		foreach (var otherSpace in ActiveAppSpace.Spaces)
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
		if (!ShouldMove(window)) return false;

		window.SetState(WindowState.Restored);
		window.SetPosition(new Rectangle(space.Location.X - window.FrameMargins.Left, space.Location.Y - window.FrameMargins.Top, space.Location.X + space.Location.Width + window.FrameMargins.Right, space.Location.Y + space.Location.Height + window.FrameMargins.Bottom));

		return true;
	}

	private static bool ShouldMove(IWindow window)
	{
		return window is { CanMove: true, CanResize: true, CanReorder: true, CanMinimize: true, CanMaximize: true };
	}
}