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
    public class SubsectorModel : PageModel
    {
        private readonly ILogger Logger;
        private readonly GTStarDbContext Context;
        private readonly MapAPI Api;

        public Sector Sector { get; set; }
        public Subsector Subsector { get; set; }
        public SurroundingSubsectors Surrounds { get; set; }

        public IEnumerable<IEnumerable<SurroundingSubsector>> SubsectorRows
        {
            get
            {
                var subsectors = Sector.Subsectors.OrderBy(s => s.Code);

                yield return subsectors.Take(4).Select(s => new SurroundingSubsector(Sector, s));
                yield return subsectors.Skip(4).Take(4).Select(s => new SurroundingSubsector(Sector, s));
                yield return subsectors.Skip(8).Take(4).Select(s => new SurroundingSubsector(Sector, s));
                yield return subsectors.Skip(12).Take(4).Select(s => new SurroundingSubsector(Sector, s));
            }
        }

        public SubsectorModel(ILogger<SubsectorModel> logger, GTStarDbContext context, MapAPI api)
        {
            Logger = logger;
            Context = context;
            Api = api;
        }

        public IActionResult OnGet(string milieu, string sector, string subsector)
        {
            try
            {
                Subsector = Context.FindSubsector(milieu, sector, subsector);
                if (Subsector == null)
                {
                    return NotFound();
                }

                Sector = Context.FindSector(Subsector.SectorId);
                Surrounds = Context.FindSurrounds(Subsector);
                return Page();
            }
            catch(Exception ex)
            {
                Logger.LogError(1, ex, $"Error occurred while loading Subsector data");
                return StatusCode(500);
            }
        }
    }
}
