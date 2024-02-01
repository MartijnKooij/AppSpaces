using AppSpaces.App.Models;
using AppSpaces.App.Services;
using System.Windows;
using System.Windows.Controls;
using ComboBoxItem = AppSpaces.App.Models.ComboBoxItem;

namespace AppSpaces.App;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow
{
	private Settings settings = null!;

	public SettingsWindow()
	{
		InitializeComponent();
	}

	private async void OnLoaded(object sender, RoutedEventArgs e)
	{
		settings = await SettingsService.LoadSettings();

		WorkSpacesComboBox.ItemsSource = settings.WorkSpaces.Select(w => new ComboBoxItem
		{
			Key = w.WorkSpaceBounds.ToString(),
			Value = w.Label ?? "Unnamed WorkSpace..."
		});
	}

	private void WorkSpacesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var selectedItem = (ComboBoxItem)WorkSpacesComboBox.SelectedItem;

		if (!string.IsNullOrEmpty(selectedItem?.Value))
		{
			RemoveWorkSpaceButton.IsEnabled = true;
			AddAppSpaceButton.IsEnabled = true;
		}
		else
		{
			RemoveWorkSpaceButton.IsEnabled = false;
			AddAppSpaceButton.IsEnabled = false;
			return;
		}

		var selectedWorkspace = settings.WorkSpaces.Single(w => w.WorkSpaceBounds.ToString() == selectedItem.Key);
		AppSpacesComboBox.ItemsSource = selectedWorkspace.AppSpaces.Select(a => new ComboBoxItem
		{
			Key = a.Id.ToString(),
			Value = a.Label ?? "Unnamed AppSpace"
		});

		WorkSpaceLeftTextBox.Text = selectedWorkspace.WorkSpaceBounds.Left.ToString();
		WorkSpaceTopTextBox.Text = selectedWorkspace.WorkSpaceBounds.Top.ToString();
		WorkSpaceRightTextBox.Text = selectedWorkspace.WorkSpaceBounds.Right.ToString();
		WorkSpaceBottomTextBox.Text = selectedWorkspace.WorkSpaceBounds.Bottom.ToString();
	}

	private void AppSpacesComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var selectedItem = (ComboBoxItem)AppSpacesComboBox.SelectedItem;

		if (!string.IsNullOrEmpty(selectedItem?.Value))
		{
			RemoveAppSpaceButton.IsEnabled = true;
		}
		else
		{
			RemoveAppSpaceButton.IsEnabled = false;
			return;
		}
	}

	private void AddWorkSpaceButton_OnClick(object sender, RoutedEventArgs e)
	{
		throw new NotImplementedException();
	}

	private void RemoveWorkSpaceButton_OnClick(object sender, RoutedEventArgs e)
	{
		throw new NotImplementedException();
	}

	private void AddAppSpaceButton_OnClick(object sender, RoutedEventArgs e)
	{
		throw new NotImplementedException();
	}

	private void RemoveAppSpaceButton_OnClick(object sender, RoutedEventArgs e)
	{
		throw new NotImplementedException();
	}
}