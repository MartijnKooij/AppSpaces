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
	private readonly Space streamingSpace;
	private readonly Bitmap lastCapturedBitmap;
	private readonly Graphics graphics;
	private Settings? settings;
	private bool isCapturing;
	private bool isClosing;

	public StreamingWindow(Space streamingSpace)
	{
		InitializeComponent();

		this.streamingSpace = streamingSpace;

		Title = Constants.StreamingWindowTitle;
		Width = this.streamingSpace.Location.Width;
		Height = this.streamingSpace.Location.Height;
		Grid.Width = this.streamingSpace.Location.Width;
		Grid.Height = this.streamingSpace.Location.Height;
		StreamingImage.Width = this.streamingSpace.Location.Width;
		StreamingImage.Height = this.streamingSpace.Location.Height;

		lastCapturedBitmap = new Bitmap(this.streamingSpace.Location.Width, this.streamingSpace.Location.Height, PixelFormat.Format24bppRgb);
		graphics = Graphics.FromImage(lastCapturedBitmap);

		var timer = new HiResTimer(33f);
		timer.Elapsed += CaptureScreen;
		timer.Elapsed += RenderScreen;
		timer.Start();

		Closing += (_, _) =>
		{
			isClosing = true;
			settings!.IsStreaming = false;
			timer.Stop();
			lastCapturedBitmap.Dispose();
			graphics.Dispose();
		};

		Loaded += OnLoaded;
	}

	private async void OnLoaded(object sender, RoutedEventArgs e)
	{
		settings = await SettingsService.LoadSettings();
		settings.IsStreaming = true;
	}

	private void CaptureScreen()
	{
		if (isClosing) return;
		if (isCapturing) return;

		isCapturing = true;

		try
		{
			graphics.CopyFromScreen(streamingSpace.Location.X, streamingSpace.Location.Y, 0, 0,
				new Size(streamingSpace.Location.Width, streamingSpace.Location.Height),
				CopyPixelOperation.SourceCopy);
		}
		catch (Exception)
		{
			// No update, fail silently
		}
		finally
		{
			isCapturing = false;
		}
	}

	private void RenderScreen()
	{
		if (isClosing) return;
		
		if (!Dispatcher.CheckAccess())
		{
			Dispatcher.Invoke(RenderScreen);
			return;
		}

		StreamingImage.Source = ToBitmapSource(lastCapturedBitmap);
	}

	private static BitmapSource ToBitmapSource(Bitmap bitmap)
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