using System.Windows;
using AppSpaces.App.Wpf.Models;

namespace AppSpaces.App.Wpf;

public partial class StreamingWindow : Window
{
	private Space _streamingSpace;

	public StreamingWindow(Space streamingSpace)
	{
		InitializeComponent();

		_streamingSpace = streamingSpace;

		Title = Constants.StreamingWindowTitle;
	}
}