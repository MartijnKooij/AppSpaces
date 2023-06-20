using AppSpaces.App.Extensions;
using AppSpaces.App.Models;

namespace AppSpaces.App.Services;

public class WindowService
{
	private readonly IWorkspace _workspace;
	private Settings _settings = null!;

	public WindowService()
	{
		_workspace = new Win32Workspace();
		_workspace.WindowManaging += async (_, args) => await RegisterWindow(args.Source);
		_workspace.WindowAdded += async (_, args) => await RegisterWindow(args.Source);
	}

	public void Start(Settings settings)
	{
		_settings = settings;
		_workspace.Open();

		_workspace.DisplayManager.Added += async (_, _) => await SnapAllWindowsToRegisteredAppSpace();
		_workspace.DisplayManager.Removed += async (_, _) => await SnapAllWindowsToRegisteredAppSpace();
		_workspace.DisplayManager.VirtualDisplayBoundsChanged += async (_, _) => await SnapAllWindowsToRegisteredAppSpace();
	}

	public void Stop()
	{
		_workspace.Dispose();
	}

	public async Task SnapAllWindowsToRegisteredAppSpace()
	{
		foreach (var window in _workspace.GetSnapshot())
		{
			await SnapToRegisteredAppSpace(window);
		}
	}
	
	public async Task ActivateWindowInSpace(bool moveForward)
	{
		var activeAppSpace = await GetActiveAppSpace();
		var activeWindow = _workspace.FocusedWindow;
		if (activeWindow == null)
		{
			activeWindow = activeAppSpace.Spaces.First().Windows.FirstOrDefault()?.Window;
			if (activeWindow == null) return;
		}

		var spaceOfActiveWindow =
			activeAppSpace.Spaces
				.FirstOrDefault(s => s.Windows
					.Any(w => w.Window?.Handle == activeWindow.Handle));
		if (!(spaceOfActiveWindow?.Windows.Any() ?? false))
		{
			return;
		}

		var indexOfActiveWindow = spaceOfActiveWindow.Windows.FindIndex(w => w.Window?.Handle == activeWindow.Handle);
		var activeIndex = indexOfActiveWindow + (moveForward ? 1 : -1);
		if (activeIndex < 0) activeIndex = spaceOfActiveWindow.Windows.Count - 1;
		else if (activeIndex >= spaceOfActiveWindow.Windows.Count) activeIndex = 0;

		try
		{
			spaceOfActiveWindow.Windows[activeIndex].Window?.ForceForegroundWindow();
		}
		catch (InvalidWindowReferenceException)
		{
			spaceOfActiveWindow.Windows.RemoveAt(activeIndex);
		}
	}

	public async Task<bool> HasStreamingSpace()
	{
		var activeAppSpace = await GetActiveAppSpace();
		return activeAppSpace.Spaces.Any(s => s.IsStreaming);
	}

	public async Task<Space> GetStreamingSpace()
	{
		var activeAppSpace = await GetActiveAppSpace();
		return activeAppSpace.Spaces.Single(s => s.IsStreaming);
	}

	private async Task<AppSpace> GetActiveAppSpace()
	{
		var appSpaces = await _settings.GetAppSpacesForWorkSpace(_workspace);

		return appSpaces.Single(a => a.Id == _settings.ActiveAppSpaceId);
	}


	private async Task RegisterWindow(IWindow window)
	{
		await SnapToRegisteredAppSpace(window);
		window.PositionChangeEnd += async (_, windowPositionChangedEventArgs) =>
			await SnapToContainingAppSpace(windowPositionChangedEventArgs.Source);
	}

	private async Task SnapToRegisteredAppSpace(IWindow window)
	{
		var activeAppSpace = await GetActiveAppSpace();
		var matchedWindowSpace = activeAppSpace.Spaces
			.SingleOrDefault(s => s.Apps
				.Any(a => a.IsMatch(window)));
		var windowSpace = matchedWindowSpace ?? activeAppSpace.Spaces.Single(s => s.IsPrimary);

		if (!SnapToSpace(window, windowSpace))
		{
			if (window.Title == Constants.StreamingWindowTitle)
			{
				await SnapStreamingWindowToStreamingSpace(window);
			}

			return;
		}

		// Only register explicit choices, so don't store when the app is moved to the default primary space
		if (matchedWindowSpace == null) return;

		var matchedAppSearch = matchedWindowSpace.Apps.SingleOrDefault(a => a.IsMatch(window));
		await RegisterWindowInSpace(window, windowSpace, matchedAppSearch);
	}

	private async Task SnapStreamingWindowToStreamingSpace(IWindow streamingWindow)
	{
		SnapToSpace(streamingWindow, await GetStreamingSpace(), true);
		streamingWindow.SendToBack();
	}

	private async Task SnapToContainingAppSpace(IWindow window)
	{
		var pointerLocation = new ScreenLocation(_workspace.CursorLocation.X, _workspace.CursorLocation.Y, 1, 1);
		var windowLocation = new ScreenLocation(window.Position.Left, window.Position.Top, window.Position.Width, window.Position.Height);
		var activeAppSpace = await GetActiveAppSpace();
		var containingSpace = activeAppSpace.Spaces.SingleOrDefault(space => space.Location.HitTest(pointerLocation) || space.Location.HitTest(windowLocation));
		if (containingSpace == null) return;

		if (!SnapToSpace(window, containingSpace)) return;

		await RegisterWindowInSpace(window, containingSpace);
	}

	private async Task RegisterWindowInSpace(IWindow window, Space space, AppSearch? matchedAppSearch = null)
	{
		var newAppSearch = matchedAppSearch ?? new AppSearch
		{
			SearchType = SearchType.ExecutablePath,
			SearchQuery = window.GetProcessExe()
		};

		// Remove this window from any spaces where it might already be registered in.
		var activeAppSpace = await GetActiveAppSpace();
		foreach (var otherSpace in activeAppSpace.Spaces)
		{
			otherSpace.Windows.RemoveAll(w => w.Window?.Handle == window.Handle);
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
		await SettingsService.SaveSettings(_settings);
	}

	private static bool SnapToSpace(IWindow window, Space space, bool force = false)
	{
		if (!force && !ShouldMove(window)) return false;

		try
		{
			window.SetState(WindowState.Restored);
			var left = Math.Max(0, space.Location.X - window.FrameMargins.Left);
			var top = Math.Max(0, space.Location.Y - window.FrameMargins.Top);
			var right = space.Location.X + space.Location.Width + Math.Min(10, window.FrameMargins.Right);
			var bottom = space.Location.Y + space.Location.Height + Math.Min(10, window.FrameMargins.Bottom);
			window.SetPosition(new Rectangle(left, top, right, bottom));

			return true;
		}
		catch (Exception )
		{
			// TODO: Possibly the window was closed, should we remove it from the in memory list of windows?
			return false;
		}
	}

	private static bool ShouldMove(IWindow window)
	{
		return window is { CanMove: true, CanResize: true, CanReorder: true, CanMinimize: true, CanMaximize: true } && window.Title != Constants.StreamingWindowTitle;
	}
}