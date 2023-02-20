using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AppSpaces.App.Models;
using AppSpaces.App.Services;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

namespace AppSpaces.App;

public partial class StreamingWindow
{
	private readonly Space _streamingSpace;
	private readonly Bitmap _lastCapturedBitmap;
	private readonly Graphics _graphics;
	private Settings? _settings;
	private bool _isCapturing;
	private bool _isClosing;

	public StreamingWindow(Space streamingSpace)
	{
		InitializeComponent();

		_streamingSpace = streamingSpace;

		Title = Constants.StreamingWindowTitle;

		_lastCapturedBitmap = new Bitmap(_streamingSpace.Location.Width, _streamingSpace.Location.Height, PixelFormat.Format24bppRgb);
		_graphics = Graphics.FromImage(_lastCapturedBitmap);

		var timer = new HiResTimer(33f);
		timer.Elapsed += CaptureScreen;
		timer.Elapsed += RenderScreen;
		timer.Start();

		Closing += (_, _) =>
		{
			_isClosing = true;
			_settings!.IsStreaming = false;
			timer.Stop();
			_lastCapturedBitmap.Dispose();
			_graphics.Dispose();
		};

		Loaded += OnLoaded;
	}

	private async void OnLoaded(object sender, RoutedEventArgs e)
	{
		_settings = await SettingsService.LoadSettings();
		_settings.IsStreaming = true;
	}

	private void RenderScreen()
	{
		if (_isClosing) return;
		
		if (!Dispatcher.CheckAccess())
		{
			Dispatcher.Invoke(RenderScreen);
			return;
		}

		StreamingImage.Source = Convert(_lastCapturedBitmap);
	}

	private void CaptureScreen()
	{
		if (_isClosing) return;
		if (_isCapturing) return;

		_isCapturing = true;

		try
		{
			_graphics.CopyFromScreen(_streamingSpace.Location.X, _streamingSpace.Location.Y, 0, 0,
				new Size(_streamingSpace.Location.Width, _streamingSpace.Location.Height),
				CopyPixelOperation.SourceCopy);
		}
		catch (Exception)
		{
			// No update, fail silently
		}
		finally
		{
			_isCapturing = false;
		}
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