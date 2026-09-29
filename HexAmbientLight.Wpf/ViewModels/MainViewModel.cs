using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using HexAmbientLight.Core.Localization;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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
        NavigationItems.Add(new NavigationItem("Nav.Dashboard", "Dashboard", "\uE80F", serviceProvider.GetRequiredService<DashboardViewModel>()));
        NavigationItems.Add(new NavigationItem("Nav.Ambilight", "Ambilight", "\uE7F4", serviceProvider.GetRequiredService<AmbilightViewModel>()));
        NavigationItems.Add(new NavigationItem("Nav.Game", "Game", "\uE909", serviceProvider.GetRequiredService<GameViewModel>()));
        NavigationItems.Add(new NavigationItem("Nav.LightingStudio", "LightingStudio", "\uE790", serviceProvider.GetRequiredService<LightingStudioViewModel>()));
        NavigationItems.Add(new NavigationItem("Nav.Device", "Device", "\uE770", serviceProvider.GetRequiredService<DeviceViewModel>()));
        NavigationItems.Add(new NavigationItem("Nav.Settings", "Settings", "\uE713", serviceProvider.GetRequiredService<SettingsViewModel>()));
        NavigationItems.Add(new NavigationItem("Nav.Diagnostics", "Diagnostics", "\uE9D9", serviceProvider.GetRequiredService<DiagnosticsViewModel>()));

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
        var item = NavigationItems.FirstOrDefault(i => 
            i.InternalName.Equals(targetName, StringComparison.OrdinalIgnoreCase) || 
            (i.InternalName == "LightingStudio" && targetName == "Manual"));
        if (item != null) SelectedNavigationItem = item;
    }
}

public partial class NavigationItem : ObservableObject
{
    public string InternalName { get; }
    private string _titleKey;
    public string TitleKey
    {
        get => _titleKey;
        set { SetProperty(ref _titleKey, value); OnPropertyChanged(nameof(DisplayName)); }
    }
    public string Icon { get; }
    public ViewModelBase ViewModel { get; }

    public string DisplayName => LocalizationManager.Instance.GetString(TitleKey);

    [ObservableProperty]
    private bool _isSelected;

    public NavigationItem(string titleKey, string internalName, string icon, ViewModelBase viewModel)
    {
        TitleKey = titleKey;
        InternalName = internalName;
        Icon = icon;
        ViewModel = viewModel;

        LocalizationManager.Instance.LanguageChanged += (s, e) => OnPropertyChanged(nameof(DisplayName));
    }
}
