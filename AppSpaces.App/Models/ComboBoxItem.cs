namespace AppSpaces.App.Models;

public class ComboBoxItem
{
	public string Key { get; set; }
	public string Value { get; set; }

	public override string ToString()
	{
		return Key;
	}
}