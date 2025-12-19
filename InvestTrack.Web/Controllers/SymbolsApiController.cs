using InvestTrack.Model.Contracts;
using InvestTrack.Model.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestTrack.Web.Controllers;

[ApiController]
[Route("api/symbols")]
public class SymbolsApiController : ControllerBase
{
    private readonly InvestTrackDbContext _db;

    public SymbolsApiController(InvestTrackDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<SymbolDto>>> Get()
    {
        var symbols = await _db.Symbols
            .OrderBy(s => s.Code)
            .Select(s => new SymbolDto
            {
                Id = s.Id,
                Code = s.Code,
                DisplayName = s.DisplayName,
                Category = s.Category
            })
            .ToListAsync();

        return Ok(symbols);
    }
}
