using CelmiBluetooth.ViewModels;

namespace CelmiBluetooth.Views;

public partial class BluetoothView : ContentView
{
    /// <summary>
    /// Construtor padrão para XAML
    /// </summary>
    public BluetoothView() : this(AppCelmiMaquinas.MauiProgram.Services?.GetService<BluetoothViewModel>()!)
    {
    }

    /// <summary>
    /// Construtor com injeção de dependência
    /// </summary>
    /// <param name="viewModel">ViewModel para Bluetooth</param>
    public BluetoothView(BluetoothViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    /// <summary>
    /// Propriedade de conveniência para acessar o ViewModel
    /// </summary>
    public BluetoothViewModel? VM => BindingContext as BluetoothViewModel;

    /// <summary>
    /// Propriedade pública para acessar o SelectedNetworkNumber do ViewModel
    /// para resolver problemas de binding em DataTemplates
    /// </summary>
    public int SelectedNetworkNumber => VM?.SelectedNetworkNumber ?? 1;
}