using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Automation;

namespace AppSpaces
{
	public partial class MainWindow
	{
		// ReSharper disable InconsistentNaming
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);
		// ReSharper enable InconsistentNaming

		public MainWindow()
		{
			InitializeComponent();
			Log("Started.");

			Automation.AddAutomationEventHandler(
				eventId: WindowPattern.WindowOpenedEvent,
				element: AutomationElement.RootElement,
				scope: TreeScope.Children,
				eventHandler: OnWindowOpened);

			Log("Listening...");
		}

		private void OnWindowOpened(object sender, AutomationEventArgs e)
		{
			var element = sender as AutomationElement;
			if (element == null) return;

			Log($"Window opened for {element.Current.Name} > {element.Current.NativeWindowHandle} > {element.Current.ProcessId}");

			if (element.Current.Name == "Visual Studio Code")
			{
				MoveWindow(new IntPtr(element.Current.NativeWindowHandle), 100, 100, 500, 500, true);

				var lastError = Marshal.GetLastWin32Error();
				Log($"Last error: {lastError}");
			}
		}

		private void Log(string data)
		{
			Dispatcher.Invoke(() =>
			{
				Logger.Text = $"{data}{Environment.NewLine}{Logger.Text}";
			});
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Log("Closing...");
			Automation.RemoveAllEventHandlers();

			base.OnClosing(e);
		}
	}
}
