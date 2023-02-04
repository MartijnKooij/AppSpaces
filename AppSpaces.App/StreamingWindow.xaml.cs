using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AppSpaces.App.Models;
using PInvoke;

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
		StreamingImage.Source = CaptureRegion(_streamingSpace.Location.X, _streamingSpace.Location.Y, _streamingSpace.Location.Width, _streamingSpace.Location.Height);
	}

	private static BitmapSource CaptureRegion(int x, int y, int width, int height)
	{
		const int sourceCopy = 0xCC0020;
		var sourceDC = User32.SafeDCHandle.Null;
		var targetDC = User32.SafeDCHandle.Null;
		var compatibleBitmapHandle = IntPtr.Zero;
		BitmapSource bitmap;

		try
		{
			// gets the main desktop and all open windows
			sourceDC = User32.GetDC(User32.GetDesktopWindow());
			//sourceDC = User32.GetDC(hWnd);
			targetDC = Gdi32.CreateCompatibleDC(sourceDC);

			// create a bitmap compatible with our target DC
			compatibleBitmapHandle = Gdi32.CreateCompatibleBitmap(sourceDC, width, height);

			// gets the bitmap into the target device context
			Gdi32.SelectObject(targetDC, compatibleBitmapHandle);

			// copy from source to destination
			Gdi32.BitBlt(targetDC, 0, 0, width, height, sourceDC, x, y, sourceCopy);

			// Here's the WPF glue to make it all work. It converts from an
			// hBitmap to a BitmapSource. Love the WPF interop functions
			bitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				compatibleBitmapHandle, IntPtr.Zero, Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());
		}
		catch (Exception ex)
		{
			throw new ApplicationException($"Error capturing region {x},{y},{width},{height}", ex);
		}
		finally
		{
			Gdi32.DeleteObject(compatibleBitmapHandle);

			User32.ReleaseDC(IntPtr.Zero, sourceDC.DangerousGetHandle());
			User32.ReleaseDC(IntPtr.Zero, targetDC.DangerousGetHandle());
		}

		return bitmap;
	}
}