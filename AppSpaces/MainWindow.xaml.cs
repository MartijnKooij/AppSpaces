using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using WinMan;
using WinMan.Windows;

namespace AppSpaces
{
	public partial class MainWindow
	{
		private IWorkspace workspace;

		public MainWindow()
		{
			InitializeComponent();
			Log("Started.");

			workspace = new Win32Workspace();
			workspace.WindowManaging += (s, e) =>
			{
				Log($"Window {e.Source} initially present on the workspace!");
			};
			workspace.WindowAdded += (s, e) =>
			{
				Log($"Window {e.Source} added to the workspace! {e.Source.CanMove}");

				if (e.Source.Title == "Visual Studio Code")
				{
					e.Source.SetState(WinMan.WindowState.Restored);
					e.Source.SetPosition(new Rectangle(100, 100, 600, 600));
					e.Source.PositionChanged += (s, a) =>
					{
						Log($"VS Code moved {a.OldPosition} > {a.NewPosition}");
					};
				}
			};
			workspace.Open();
			Log("Listening...");
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
			workspace.Dispose();
			base.OnClosing(e);
		}
	}
}
