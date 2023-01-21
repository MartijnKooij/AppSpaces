using ABI.Windows.UI;
using AppSpaces.App.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml.Shapes;
using WinMan;
using WinMan.Windows;
using Rectangle = Microsoft.UI.Xaml.Shapes.Rectangle;

namespace AppSpaces.App;

public sealed partial class MainWindow
{
	private Settings _settings = null!;

	public MainWindow()
	{
		this.Title = "AppSpaces - Settings";
		this.InitializeComponent();

		LoadAppSpaces();
	}

	private async void LoadAppSpaces()
	{
		_settings = await SettingsManager.LoadSettings();

		AppSpaces.ItemsSource = _settings.AppSpaces;
		RenderAppSpace();
	}

	private void RenderAppSpace()
	{
		using var workspace = new Win32Workspace();
		workspace.Open();

		AppSpaceGrid.MaximumRowsOrColumns = Math.Max(
			workspace.DisplayManager.VirtualDisplayBounds.Width,
			workspace.DisplayManager.VirtualDisplayBounds.Height);
		AppSpaceGrid.ItemWidth = 1;
		AppSpaceGrid.ItemHeight = 1;

		var activeAppSpace = _settings.AppSpaces[0];
		var colors = new[] { Colors.AliceBlue, Colors.Azure, Colors.BlueViolet };
		for (var spaceIndex = 0; spaceIndex < activeAppSpace.Spaces.Count; spaceIndex++)
		{
			var space = activeAppSpace.Spaces[spaceIndex];
			var uiElement = new Rectangle
			{
				Fill = new SolidColorBrush(colors[spaceIndex]),
				Width = space.Location.Width,
				Height = space.Location.Height
			};
			uiElement.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, uiElement.Width);
			uiElement.SetValue(VariableSizedWrapGrid.RowSpanProperty, uiElement.Height);

			AppSpaceGrid.Children.Add(uiElement);
		}
	}
}