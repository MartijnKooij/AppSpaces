namespace AppSpaces.App.Models;

public class AppSpace
{
	public Guid Id { get; init; }
	public string? Label { get; set; }
	public List<Space> Spaces { get; init; } = new();
}