using AppCelmiMaquinas.ViewModel;

using CelmiBluetooth.ViewModels;

namespace AppCelmiMaquinas.Views;

public partial class PesagemView : ContentView
{
    /// <summary>
    /// Construtor padr�o para XAML
    /// </summary>
    public PesagemView() : this(AppCelmiMaquinas.MauiProgram.Services?.GetService<PesagemViewModel>()!)
    {
    }

    /// <summary>
    /// Construtor com inje��o de depend�ncia
    /// </summary>
    /// <param name="viewModel">ViewModel para Pesagem</param>
    public PesagemView(PesagemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Propriedade de conveni�ncia para acessar o ViewModel
    /// </summary>
    public PesagemViewModel? VM => BindingContext as PesagemViewModel;
}