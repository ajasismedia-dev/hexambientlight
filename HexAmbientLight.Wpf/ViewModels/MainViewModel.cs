using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace HexAmbientLight.Wpf.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    [ObservableProperty]
    private NavigationItem? _selectedNavigationItem;

    public ObservableCollection<NavigationItem> NavigationItems { get; } = new();

    public MainViewModel(IServiceProvider serviceProvider)
    {
        NavigationItems.Add(new NavigationItem("Dashboard", "\uE80F", serviceProvider.GetRequiredService<DashboardViewModel>()));
        NavigationItems.Add(new NavigationItem("Ambilight", "\uE7F4", serviceProvider.GetRequiredService<AmbilightViewModel>()));
        NavigationItems.Add(new NavigationItem("Game", "\uE909", serviceProvider.GetRequiredService<GameViewModel>()));
        NavigationItems.Add(new NavigationItem("Manual", "\uE790", serviceProvider.GetRequiredService<ManualViewModel>()));
        NavigationItems.Add(new NavigationItem("Device", "\uE770", serviceProvider.GetRequiredService<DeviceViewModel>()));
        NavigationItems.Add(new NavigationItem("Settings", "\uE713", serviceProvider.GetRequiredService<SettingsViewModel>()));
        NavigationItems.Add(new NavigationItem("Diagnostics", "\uE9D9", serviceProvider.GetRequiredService<DiagnosticsViewModel>()));

        SelectedNavigationItem = NavigationItems[0];
    }

    partial void OnSelectedNavigationItemChanged(NavigationItem? value)
    {
        if (value != null)
        {
            CurrentViewModel = value.ViewModel;
            foreach (var navItem in NavigationItems)
            {
                navItem.IsSelected = (navItem == value);
            }
        }
    }
    [RelayCommand]
    private void Navigate(string targetName)
    {
        foreach (var item in NavigationItems)
        {
            if (item.Name.Equals(targetName, StringComparison.OrdinalIgnoreCase) || 
               (item.Name == "CS2 Game Sync" && targetName == "Game") ||
               (item.Name == "Manual Color" && targetName == "Manual") ||
               (item.Name == "WLED Devices" && targetName == "Device"))
            {
                SelectedNavigationItem = item;
                break;
            }
        }
    }
}

public partial class NavigationItem : ObservableObject
{
    public string Name { get; }
    public string Icon { get; }
    public ViewModelBase ViewModel { get; }

    [ObservableProperty]
    private bool _isSelected;

    public NavigationItem(string name, string icon, ViewModelBase viewModel)
    {
        Name = name;
        Icon = icon;
        ViewModel = viewModel;
    }
}
