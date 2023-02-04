using System.Windows;
using AppSpaces.App.Models;

namespace AppSpaces.App;

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