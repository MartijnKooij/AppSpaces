﻿using AppSpaces.App.Extensions;
using AppSpaces.App.Models;
using PInvoke;

namespace AppSpaces.App.Services;

public class WindowService
{
	public readonly IWorkspace Workspace;
	private Settings settings = null!;

	public WindowService()
	{
		Workspace = new Win32Workspace();
		Workspace.WindowManaging += async (_, args) => await RegisterWindow(args.Source);
		Workspace.WindowAdded += async (_, args) => await RegisterWindow(args.Source);
	}

	public void Start(Settings appSettings)
	{
		settings = appSettings;
		Workspace.Open();

		Workspace.DisplayManager.Added += async (_, _) => await SnapAllWindowsToRegisteredAppSpace();
		Workspace.DisplayManager.Removed += async (_, _) => await SnapAllWindowsToRegisteredAppSpace();
		Workspace.DisplayManager.VirtualDisplayBoundsChanged += async (_, _) => await SnapAllWindowsToRegisteredAppSpace();
	}

	public void Stop()
	{
		Workspace.Dispose();
	}

	public async Task SnapAllWindowsToRegisteredAppSpace()
	{
		foreach (var window in Workspace.GetSnapshot())
		{
			await SnapToRegisteredAppSpace(window);
		}
	}
	
	public void ActivateWindowInSpace(bool moveForward)
	{
		var activeAppSpace = GetActiveAppSpace();
		var activeWindow = Workspace.FocusedWindow;
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

	public bool HasStreamingSpace()
	{
		var activeAppSpace = GetActiveAppSpace();
		return activeAppSpace.Spaces.Any(s => s.IsStreaming);
	}

	public Space GetStreamingSpace()
	{
		var activeAppSpace = GetActiveAppSpace();
		return activeAppSpace.Spaces.Single(s => s.IsStreaming);
	}

	private AppSpace GetActiveAppSpace()
	{
		var appSpaces = settings.GetAppSpacesForWorkSpace(Workspace);
		var activeAppSpace = appSpaces.SingleOrDefault(a => a.Id == settings.ActiveAppSpaceId);

		if (activeAppSpace != default) return activeAppSpace;

		activeAppSpace = appSpaces.First();
		settings.ActiveAppSpaceId = activeAppSpace.Id;

		return activeAppSpace;
	}


	private async Task RegisterWindow(IWindow window)
	{
		await SnapToRegisteredAppSpace(window);
		window.PositionChangeEnd += async (_, windowPositionChangedEventArgs) =>
			await SnapToContainingAppSpace(windowPositionChangedEventArgs.Source);
	}

	private async Task SnapToRegisteredAppSpace(IWindow window)
	{
		var activeAppSpace = GetActiveAppSpace();
		var matchedWindowSpace = activeAppSpace.Spaces
			.SingleOrDefault(s => s.Apps
				.Any(a => a.IsMatch(window)));
		var windowSpace = matchedWindowSpace ?? activeAppSpace.Spaces.Single(s => s.IsPrimary);

		if (!SnapToSpace(window, windowSpace))
		{
			if (window.Title == Constants.StreamingWindowTitle)
			{
				SnapStreamingWindowToStreamingSpace(window);
			}

			return;
		}

		// Only register explicit choices, so not when the app is moved to the default primary space
		if (matchedWindowSpace == null) return;

		var matchedAppSearch = matchedWindowSpace.Apps.SingleOrDefault(a => a.IsMatch(window));
		await RegisterWindowInSpace(window, windowSpace, false, matchedAppSearch);
	}

	private void SnapStreamingWindowToStreamingSpace(IWindow streamingWindow)
	{
		SnapToSpace(streamingWindow, GetStreamingSpace(), true);
		streamingWindow.SendToBack();
	}

	private async Task SnapToContainingAppSpace(IWindow window)
	{
		var pointerLocation = new ScreenLocation(Workspace.CursorLocation.X, Workspace.CursorLocation.Y, 1, 1);
		var windowLocation = new ScreenLocation(window.Position.Left, window.Position.Top, window.Position.Width, window.Position.Height);
		var activeAppSpace = GetActiveAppSpace();
		var containingSpace = activeAppSpace.Spaces.FirstOrDefault(space => space.Location.HitTest(pointerLocation) || space.Location.HitTest(windowLocation));
		if (containingSpace == null) return;

		if (!SnapToSpace(window, containingSpace)) return;

		await RegisterWindowInSpace(window, containingSpace, true);
	}

	private async Task RegisterWindowInSpace(IWindow window, Space space, bool shouldSaveSettings = false, AppSearch matchedAppSearch = null)
	{
		var newAppSearch = matchedAppSearch ?? new AppSearch
		{
			SearchType = SearchType.ExecutablePath,
			SearchQuery = window.GetProcessExe()
		};

		// Remove this window from any spaces where it might already be registered in.
		var activeAppSpace = GetActiveAppSpace();
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
		if (space.Apps.All(a => a.SearchQuery != newAppSearch.SearchQuery))
		{
			space.Apps.Add(newAppSearch);
		}

		if (shouldSaveSettings)
		{
			await SettingsService.SaveSettings(settings);
		}
	}

	private static bool SnapToSpace(IWindow window, Space space, bool force = false)
	{
		if (!force && !ShouldMove(window)) return false;

		try
		{
			var left = space.Location.X - window.FrameMargins.Left;
			var top = space.Location.Y - window.FrameMargins.Top;
			// var right = space.Location.X + space.Location.Width + window.FrameMargins.Right;
			// var bottom = space.Location.Y + space.Location.Height + window.FrameMargins.Bottom;
			var width = space.Location.Width + window.FrameMargins.Left + window.FrameMargins.Right;
			var height = space.Location.Height + window.FrameMargins.Top + window.FrameMargins.Bottom;

			// window.SetState(WindowState.Restored);
			// window.SetPosition(new Rectangle(left, top, right, bottom));

			// User32.MoveWindow(window.Handle, left, top, width, height, true);
			User32.SetWindowPos(window.Handle, IntPtr.Zero, left, top, width, height, User32.SetWindowPosFlags.SWP_SHOWWINDOW | User32.SetWindowPosFlags.SWP_NOSENDCHANGING);

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