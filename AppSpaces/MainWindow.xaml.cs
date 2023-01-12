using System;
using System.ComponentModel;
using WinMan;
using WinMan.Windows;

namespace AppSpaces
{
	public partial class MainWindow
	{
		private readonly IWorkspace _workspace;
		private Rectangle _currentSpace = new (100, 100, 600, 600);

		public MainWindow()
		{
			InitializeComponent();
			Log("Started.");

			_workspace = new Win32Workspace();
			_workspace.WindowManaging += (s, e) =>
			{
				Log($"Window {e.Source} initially present on the workspace!");
			};
			_workspace.WindowAdded += (s, e) =>
			{
				Log($"Window {e.Source.Title} added to the workspace!");

				if (e.Source.Title.Contains("Visual Studio Code"))
				{
					MoveToSpace(e.Source);
					e.Source.PositionChangeEnd += (s, a) =>
					{
						Log($"VS Code moved {a.OldPosition} > {a.NewPosition}");
						_currentSpace = new Rectangle(800, 300, 1400, 800);
						MoveToSpace(e.Source);
					};
				}
			};
			_workspace.Open();
			Log("Listening...");
		}

		private void MoveToSpace(IWindow window)
		{
			window.SetState(WinMan.WindowState.Restored);
			window.SetPosition(_currentSpace);
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
			_workspace.Dispose();
			base.OnClosing(e);
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_currentSpace = new Rectangle(300, 300, 1000, 1000);
			foreach (var window in _workspace.GetSnapshot())
			{
				Log($"Checking whether {window.Title} should be moved.");
				if (window.Title.Contains("Visual Studio Code"))
				{
					MoveToSpace(window);
				}
			}
		}
	}
}
