using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WinMan;

namespace AppSpaces.App.Extensions;

public static class WindowExtensions
{
	[DllImport("user32.dll", SetLastError=true)]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

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
}