using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AppSpaces.App.Models;
using AppSpaces.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppSpaces.App;

public class TrayIconViewModel : ObservableObject
{
	private Settings settings;
	private WindowService windowService;

	public event EventHandler OnSettings;
	public event EventHandler<ActivateAppSpaceEventArgs> OnActivateAppSpace;
	// ReSharper disable once MemberCanBePrivate.Global
	// ReSharper disable once CollectionNeverQueried.Global
	public ObservableCollection<object> MenuItems { get; set; }

	public void Initialize(Settings s, WindowService w)
	{
		settings = s;
		windowService = w;

		LoadMenu();
	}

	private void LoadMenu()
	{
		MenuItems = new ObservableCollection<object>();
		object menuItem;

		var appSpaces = settings.GetAppSpacesForWorkSpace(windowService.Workspace);
		foreach (var appSpace in appSpaces)
		{
			menuItem = new MenuItem { Header = appSpace.Label, Command = new RelayCommand(() => ActivateAppSpace(appSpace.Id)) };
			MenuItems.Add(menuItem);
		}
	
		MenuItems.Add(new Separator());

		menuItem = new MenuItem { Header = "OnSettings...", Command = new RelayCommand(ShowSettings) };
		MenuItems.Add(menuItem);

		MenuItems.Add(new Separator());

		menuItem = new MenuItem { Header = "Exit", Command = new RelayCommand(ExitApplication) };
		MenuItems.Add(menuItem);
	}

	private void ActivateAppSpace(Guid appSpaceId)
	{
		OnActivateAppSpace?.Invoke(this, new ActivateAppSpaceEventArgs(appSpaceId));
	}

	private void ShowSettings()
	{
		OnSettings?.Invoke(this, EventArgs.Empty);
	}

	private static void ExitApplication()
	{
		Application.Current.Shutdown();
	}
}

public class ActivateAppSpaceEventArgs : EventArgs
{
	public Guid AppSpaceId { get; set; }

	public ActivateAppSpaceEventArgs(Guid appSpaceId)
	{
		AppSpaceId = appSpaceId;
	}
}