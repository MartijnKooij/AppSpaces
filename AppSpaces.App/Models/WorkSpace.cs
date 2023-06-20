namespace AppSpaces.App.Models;

public class WorkSpace
{
	public string? Label { get; set; }
	public WorkspaceBounds WorkSpaceBounds { get; set; }
	public List<AppSpace> AppSpaces { get; set; } = new();
}