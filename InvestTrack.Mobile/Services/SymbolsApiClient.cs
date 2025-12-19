using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using InvestTrack.Model.Contracts;

namespace InvestTrack.Mobile.Services;

public class SymbolsApiClient
{
    private readonly HttpClient _http;

    public SymbolsApiClient(HttpClient http) => _http = http;

    public async Task<List<SymbolDto>> GetSymbolsAsync()
        => await _http.GetFromJsonAsync<List<SymbolDto>>("api/symbols")
           ?? new List<SymbolDto>();
}

