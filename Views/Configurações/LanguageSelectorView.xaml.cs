using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using AppCelmiPecuaria.ViewModel;
using LocalizationResourceManager.Maui;
using Microsoft.Maui.Controls;

namespace AppCelmiPecuaria.Views;

public partial class LanguageSelectorView : ContentView
{
    public LanguageSelectorView()
    {
        InitializeComponent();

        BindingContext = new LanguageSelectorViewModel(MauiProgram.Services.GetService<ILocalizationResourceManager>());

    }
}