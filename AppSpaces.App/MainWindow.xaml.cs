namespace AppSpaces.App;

public sealed partial class MainWindow
{
	public MainWindow()
	{
		this.Title = "AppSpaces - Settings";
		this.InitializeComponent();

		LoadAppSpaces();
	}

	private async void LoadAppSpaces()
	{
		var settings = await SettingsManager.LoadSettings();

		AppSpaces.ItemsSource = settings.AppSpaces;
	}
}