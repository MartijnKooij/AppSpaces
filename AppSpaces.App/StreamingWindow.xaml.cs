using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AppSpaces.App.Models;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;

namespace AppSpaces.App;

public partial class StreamingWindow
{
	private readonly Space _streamingSpace;
	private bool _isCapturing;
	private DateTime _lastTime;
	private int _framesRendered;
	private int _fps;

	public StreamingWindow(Space streamingSpace)
	{
		InitializeComponent();

		_streamingSpace = streamingSpace;

		Title = Constants.StreamingWindowTitle;

		var timer = new HiResTimer(100f);
		timer.Elapsed += TimerTick;
		timer.Start();

		Closed += (_, _) => timer.Stop();
	}

	private void TimerTick()
	{
		if (!Dispatcher.CheckAccess())
		{
			Dispatcher.Invoke(TimerTick);
			return;
		}

		if (_isCapturing) return;
		_isCapturing = true;
		_framesRendered++;
		if ((DateTime.Now - _lastTime).TotalMilliseconds >= 1000)
		{
			_fps = _framesRendered;                     
			_framesRendered = 0;            
			_lastTime = DateTime.Now;
		}
		Fps.Text = $"{_fps}FPS";

		using var bmp = new Bitmap(_streamingSpace.Location.Width, _streamingSpace.Location.Height, PixelFormat.Format24bppRgb);
		using var g = Graphics.FromImage(bmp);
		g.CopyFromScreen(_streamingSpace.Location.X, _streamingSpace.Location.Y, 0, 0, new Size(_streamingSpace.Location.Width, _streamingSpace.Location.Height), CopyPixelOperation.SourceCopy);

		StreamingImage.Source = Convert(bmp);
		_isCapturing = false;
	}

	private static BitmapSource Convert(Bitmap bitmap)
	{
		var bitmapData = bitmap.LockBits(
			new Rectangle(0, 0, bitmap.Width, bitmap.Height),
			ImageLockMode.ReadOnly, bitmap.PixelFormat);

		var bitmapSource = BitmapSource.Create(
			bitmapData.Width, bitmapData.Height,
			bitmap.HorizontalResolution, bitmap.VerticalResolution,
			PixelFormats.Bgr24, null,
			bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

		bitmap.UnlockBits(bitmapData);

		return bitmapSource;
	}
}