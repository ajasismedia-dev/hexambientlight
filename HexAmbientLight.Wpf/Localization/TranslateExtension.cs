using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Markup;
using HexAmbientLight.Core.Localization;

namespace HexAmbientLight.Wpf.Localization;

[MarkupExtensionReturnType(typeof(string))]
public class TranslateExtension : MarkupExtension, INotifyPropertyChanged
{
    private string _key = "";

    [ConstructorArgument("key")]
    public string Key 
    { 
        get => _key; 
        set 
        { 
            _key = value; 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); 
        } 
    }

    public string Value => LocalizationManager.Instance.GetString(Key);

    public TranslateExtension() 
    {
        LocalizationManager.Instance.LanguageChanged += (s, e) => 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        };
    }

    public TranslateExtension(string key) : this()
    {
        _key = key;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new System.Windows.Data.Binding("Value") { Source = this };
        return binding.ProvideValue(serviceProvider);
    }
}


