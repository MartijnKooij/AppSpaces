namespace AppSpaces.App.Wpf.Models;

public class ScreenLocation
{
	public int X { get; }
	public int Y { get; }
	public int Width { get; }
	public int Height { get; }

	public bool HitTest(ScreenLocation location)
	{
		return X <= location.X &&
		       X + Width >= location.X + location.Width &&
		       Y <= location.Y &&
		       Y + Height >= location.Y + location.Height;
	}

	public ScreenLocation(int x, int y, int width, int height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}
}