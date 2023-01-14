namespace AppSpaces.App.Models;

public class AppSpace
{
	public Guid Id { get; init; }
	public List<Space> Spaces { get; set; }

	public AppSpace()
	{
		Id = Guid.NewGuid();
	}
}