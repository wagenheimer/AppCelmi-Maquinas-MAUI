using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using AppCelmiMaquinas.ViewModel;
using LocalizationResourceManager.Maui;
using Microsoft.Maui.Controls;

namespace AppCelmiMaquinas.Views;

public partial class LanguageSelectorView : ContentView
{
    public LanguageSelectorView()
    {
        InitializeComponent();

        BindingContext = new LanguageSelectorViewModel(MauiProgram.Services.GetService<ILocalizationResourceManager>());

    }
}