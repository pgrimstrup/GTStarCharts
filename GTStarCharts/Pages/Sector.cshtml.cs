using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTStarCharts.TravellerMap;
using GTStarData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GTStarCharts.Pages
{
    public class SectorModel : PageModel
    {
        private readonly ILogger<SectorModel> Logger;
        private readonly GTStarDbContext Data;
        private readonly MapAPI Api;

        public Sector Sector { get; set; }
        public Subsector[] Subsectors { get; set; }

        public SurroundingSubsectors Surrounds { get; set; }

        public SectorModel(ILogger<SectorModel> logger, GTStarDbContext data, MapAPI api)
        {
            Logger = logger;
            Data = data;
            Api = api;
        }

        public IActionResult OnGet(string milieu, string sector)
        {
            try
            {
                this.Sector = Data.FindSector(milieu, sector);
                if (this.Sector == null)
                    return NotFound();

                if (this.Sector.Subsectors.Count == 0)
                    Data.RefreshSectorDetails(Api, this.Sector);

                Subsectors = Sector.Subsectors.OrderBy(s => s.IndexNumber).ToArray();
                Surrounds = Data.FindSurrounds(Sector);
                return Page();
            }
            catch (Exception ex)
            {
                Logger.LogError(1, ex, $"Error while loading Sector Page for {milieu}/{sector}");
                return StatusCode(500);
            }
        }
    }
}
