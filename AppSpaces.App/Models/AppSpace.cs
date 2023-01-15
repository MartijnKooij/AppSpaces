namespace AppSpaces.App.Models;

public class AppSpace
{
	public Guid Id { get; set; }
	public string Label { get; set; }
	public List<Space> Spaces { get; set; }
}