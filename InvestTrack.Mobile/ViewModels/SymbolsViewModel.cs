using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using InvestTrack.Model.Contracts;
using InvestTrack.Mobile.Services;

namespace InvestTrack.Mobile.ViewModels;

public class SymbolsViewModel : INotifyPropertyChanged
{
    private readonly SymbolsApiClient _api;

    public ObservableCollection<SymbolDto> Symbols { get; } = new();

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public SymbolsViewModel(SymbolsApiClient api) => _api = api;

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            Symbols.Clear();
            var items = await _api.GetSymbolsAsync();
            foreach (var s in items) Symbols.Add(s);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
