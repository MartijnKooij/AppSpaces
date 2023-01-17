using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WinMan;

namespace AppSpaces.App.Extensions;

public static class WindowExtensions
{
	[DllImport("user32.dll", SetLastError=true)]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

	// When you don't want the ProcessId, use this overload and pass 
	// IntPtr.Zero for the second parameter
	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, 
		IntPtr processId);

	[DllImport("kernel32.dll")]
	private static extern uint GetCurrentThreadId();

	/// The GetForegroundWindow function returns a handle to the 
	/// foreground window.
	[DllImport("user32.dll")] 
	private static extern IntPtr GetForegroundWindow(); 

	[DllImport("user32.dll")]
	private static extern bool AttachThreadInput(uint idAttach, 
		uint idAttachTo, bool fAttach); 

	[DllImport("user32.dll", SetLastError = true)] 
	private static extern bool BringWindowToTop(IntPtr hWnd); 


	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

	public const uint SW_SHOW = 5;

	public static string GetProcessExe(this IWindow window)
	{
		try
		{
			var threadId = GetWindowThreadProcessId(window.Handle, out var processId);
			if (threadId == 0) return "";

			var process = Process.GetProcessById((int)processId);
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
		var foreThread = GetWindowThreadProcessId(GetForegroundWindow(), 
			IntPtr.Zero);
		var appThread = GetCurrentThreadId();

		if (foreThread != appThread)
		{
			AttachThreadInput(foreThread, appThread, true);
			BringWindowToTop(window.Handle);
			ShowWindow(window.Handle, SW_SHOW);
			AttachThreadInput(foreThread, appThread, false);
		}
		else
		{
			BringWindowToTop(window.Handle);
			ShowWindow(window.Handle, SW_SHOW);
		}
	}
}