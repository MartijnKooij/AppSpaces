using System.Drawing;

namespace AppSpaces.App.Models;

public class Space
{
	public Rectangle Location { get; set; }
	public bool IsPrimary { get; set; }
	public List<AppSearch> Apps { get; set; }
}