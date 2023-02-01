namespace AppSpaces.App.Wpf.Models;

public class KeyboardShortcut
{
	private readonly Key[] _modifiers = { Key.LeftWindows, Key.Control, Key.Alt };
	public Key UserKey { get; set; }
	public Guid AppSpaceId { get; set; }
	public Key[] AllKeys => _modifiers.Concat(new[] { UserKey }).ToArray();

}