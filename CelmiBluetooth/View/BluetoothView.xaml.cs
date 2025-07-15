using CelmiBluetooth.ViewModels;

namespace CelmiBluetooth.Views;

public partial class BluetoothView : ContentView
{
    /// <summary>
    /// Construtor padr�o para XAML
    /// </summary>
    public BluetoothView() : this(AppCelmiMaquinas.MauiProgram.Services?.GetService<BluetoothViewModel>()!)
    {
    }

    /// <summary>
    /// Construtor com inje��o de depend�ncia
    /// </summary>
    /// <param name="viewModel">ViewModel para Bluetooth</param>
    public BluetoothView(BluetoothViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Propriedade de conveni�ncia para acessar o ViewModel
    /// </summary>
    public BluetoothViewModel? VM => BindingContext as BluetoothViewModel;

    /// <summary>
    /// Propriedade p�blica para acessar o SelectedNetworkNumber do ViewModel
    /// para resolver problemas de binding em DataTemplates
    /// </summary>
    public int SelectedNetworkNumber => VM?.SelectedNetworkNumber ?? 1;
}