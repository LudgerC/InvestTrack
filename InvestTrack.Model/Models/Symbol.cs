using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvestTrack.Model.Models
{
    public class Symbol
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;      // bv. XAUUSD
        public string? DisplayName { get; set; }       // bv. Goud
        public string? Category { get; set; }          // bv. Forex, Metals, Index, etc. (optioneel)
        public DateTime CreatedAt { get; set; } = new DateTime(2025, 01, 01);

    }
}
