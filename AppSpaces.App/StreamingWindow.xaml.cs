using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AppSpaces.App.Models;
using Size = System.Drawing.Size;

namespace AppSpaces.App;

public partial class StreamingWindow
{
	private readonly Space _streamingSpace;

	public StreamingWindow(Space streamingSpace)
	{
		InitializeComponent();

		_streamingSpace = streamingSpace;

		Title = Constants.StreamingWindowTitle;

		var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.033) };
		timer.Tick += TimerTick;
		timer.Start();

		Closed += (_, _) => timer.Stop();
	}

	private void TimerTick(object? sender, EventArgs e)
	{
		var bmp = new Bitmap(_streamingSpace.Location.Width, _streamingSpace.Location.Height, PixelFormat.Format32bppArgb);
		var g = Graphics.FromImage(bmp);
		g.CopyFromScreen(_streamingSpace.Location.X, _streamingSpace.Location.Y, 0, 0, new Size(_streamingSpace.Location.Width, _streamingSpace.Location.Height), CopyPixelOperation.SourceCopy);

		StreamingImage.Source = Imaging.CreateBitmapSourceFromHBitmap(
			bmp.GetHbitmap(),
			IntPtr.Zero,
			Int32Rect.Empty,
			BitmapSizeOptions.FromWidthAndHeight(_streamingSpace.Location.Width, _streamingSpace.Location.Height));
	}
}