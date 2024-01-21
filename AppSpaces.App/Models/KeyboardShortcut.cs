namespace AppSpaces.App.Models;

public class KeyboardShortcut
{
	private readonly Key[] modifiers = { Key.LeftWindows, Key.Control, Key.Alt };
	public Key UserKey { get; set; }
	public Guid AppSpaceId { get; set; }
	public Key[] AllKeys => modifiers.Concat(new[] { UserKey }).ToArray();

}