using System.Diagnostics;
using System.IO;
using PInvoke;

namespace AppSpaces.App.Extensions;

public static class WindowExtensions
{
	public static string GetProcessExe(this IWindow window)
	{
		try
		{
			var threadId = User32.GetWindowThreadProcessId(window.Handle, out var processId);
			if (threadId == 0) return "";

			var process = Process.GetProcessById(processId);
			var moduleFileName = process.MainModule?.FileName;

			return Path.GetFileName(moduleFileName) ?? "";
		}
		catch (Win32Exception)
		{
			return "";
		}
	}

	public static void ForceForegroundWindow(this IWindow window)
	{
		var foreThread = User32.GetWindowThreadProcessId(User32.GetForegroundWindow(), out _);
		var appThread = Kernel32.GetCurrentThreadId();

		if (foreThread != appThread)
		{
			User32.AttachThreadInput(foreThread, appThread, true);
			User32.BringWindowToTop(window.Handle);
			User32.ShowWindow(window.Handle, User32.WindowShowStyle.SW_SHOW);
			User32.AttachThreadInput(foreThread, appThread, false);
		}
		else
		{
			User32.BringWindowToTop(window.Handle);
			User32.ShowWindow(window.Handle, User32.WindowShowStyle.SW_SHOW);
		}
	}
}