namespace AppSpaces.App.Models;

public class WorkspaceBounds
{
	public int Left { get; set; }
	public int Top { get; set; }
	public int Right { get; set; }
	public int Bottom { get; set; }

	public static WorkspaceBounds Create(Rectangle rectangle)
	{
		return new WorkspaceBounds(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
	}

	public WorkspaceBounds(int left, int top, int right, int bottom)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}

	public bool Equals(Rectangle other)
	{
		return Left == other.Left
		       && Top == other.Top
		       && Right == other.Right
		       && Bottom == other.Bottom;
	}
	
	public override string ToString()
	{
		return $"(L={Left}, T={Top}, R={Right}, B={Bottom})";
	}
}