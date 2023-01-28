using System.Drawing;
using System.Drawing.Imaging;
using AppSpaces.App.Models;
using Microsoft.UI.Xaml.Media.Imaging;

namespace AppSpaces.App;

public sealed partial class StreamingWindow
{
	private Space _streamingSpace;

	public StreamingWindow(Space streamingSpace)
	{
		_streamingSpace = streamingSpace;
		InitializeComponent();

		Title = Constants.StreamingWindowTitle;
		ExtendsContentIntoTitleBar = true;

		var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.033) };
		timer.Tick += TimerTick;
		timer.Start();

		this.Closed += (_, _) => timer.Stop();
	}

	private void TimerTick(object? sender, object e)
	{
		var bmp = new Bitmap(_streamingSpace.Location.Width, _streamingSpace.Location.Height, PixelFormat.Format32bppArgb);
		var g = Graphics.FromImage(bmp);
		g.CopyFromScreen(_streamingSpace.Location.X, _streamingSpace.Location.Y, 0, 0, new Size(_streamingSpace.Location.Width, _streamingSpace.Location.Height), CopyPixelOperation.SourceCopy);

		var bitmapImage = new BitmapImage();
		using var stream = new MemoryStream();

		bmp.Save(stream, ImageFormat.Jpeg);
		stream.Position = 0;
		bitmapImage.SetSource(stream.AsRandomAccessStream());

		StreamImage.Source = bitmapImage;
	}
}