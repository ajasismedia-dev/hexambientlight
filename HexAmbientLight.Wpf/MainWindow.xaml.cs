using System;
using System.Windows;
using HexAmbientLight.Core.Services;
using HexAmbientLight.Wpf.ViewModels;

namespace HexAmbientLight.Wpf;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        InitializeComponent();
    }
}
