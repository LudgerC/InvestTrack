using InvestTrack.Mobile.ViewModels;

namespace InvestTrack.Mobile;

public partial class MainPage : ContentPage
{
    private readonly SymbolsViewModel _vm;

    public MainPage(SymbolsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
