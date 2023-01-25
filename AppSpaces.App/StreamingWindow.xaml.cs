namespace AppSpaces.App;

public sealed partial class StreamingWindow
{
	public StreamingWindow()
	{
		InitializeComponent();

		Title = Constants.StreamingWindowTitle;
		ExtendsContentIntoTitleBar = true;
	}
}