using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using GTStarCharts.TravellerMap;
using GTStarData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GTStarCharts.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> Logger;
        private readonly GTStarDbContext Data;
        private readonly MapAPI Api;

        const int minx = -8;
        const int maxx = 7;
        const int miny = -7;
        const int maxy = 7;

        public Sector[] Sectors { get; set; }

        public IndexModel(ILogger<IndexModel> logger, GTStarDbContext data, MapAPI api)
        {
            Logger = logger;
            Data = data;
            Api = api;
        }

        public IActionResult OnGet()
        {
            try
            {

                Sectors = Data.FindSectors(Globals.Millieu, minx, miny, maxx, maxy);
                if (!Sectors.Any())
                {
                    Data.RefreshSectorNames(Api);
                    Sectors = Data.FindSectors(Globals.Millieu, minx, miny, maxx, maxy);
                }

                return Page();
            }
            catch (Exception ex)
            {
                Logger.LogError(1, ex, $"Error while getting list of Sectors");
                return StatusCode(500);
            }
        }

        public IEnumerable<IEnumerable<Sector>> Rows
        {
            get
            {
                for (int row = miny; row <= maxy; row++)
                {
                    Sector[] result = new Sector[maxx - minx + 1];
                    foreach (var sector in Sectors.Where(s => s.Y == row))
                        result[sector.X - minx] = sector;

                    yield return result;
                }
            }
        }
    }
}
