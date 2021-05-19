using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTStarCharts.Rules;
using GTStarCharts.TravellerMap;
using GTStarData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GTStarCharts.Pages
{
    public class SystemDataModel : PageModel
    {
        readonly ILogger Logger;
        readonly GTStarDbContext DbContext;
        readonly MapAPI Api;

        public Sector Sector { get; set; }
        public Subsector Subsector { get; set; }

        [BindProperty]
        public SystemData Data { get; set; }

        public StellarData Stellar { get; set; }

        public TextLookup Starport { get; set; }
        public TextLookup PlanetSize { get; set; }
        public TextLookup Atmosphere { get; set; }
        public TextLookup Hydrosphere { get; set; }
        public TextLookup Population { get; set; }
        public TextLookup Government { get; set; }
        public TextLookup LawLevel { get; set; }
        public TextLookup TechLevel { get; set; }
        public TextLookup Importance { get; set; }
        public TextLookup Allegiance { get; set; }
        public TextLookup TradeCodes { get; set; }
        public TextLookup Economics { get; set; }
        public TextLookup Culture { get; set; }
        public TextLookup System { get; set; }
        public TextLookup TravelZone { get; set; }
        public TextLookup Bases { get; set; }
        public TextLookup Climate { get; set; }
        public TextLookup Orbit { get; set; }
        public TextLookup Gravity { get; set; }


        public SystemDataModel(ILogger<SystemDataModel> logger, GTStarDbContext context, MapAPI api)
        {
            Logger = logger;
            DbContext = context;
            Api = api;
        }

        public IActionResult OnPost(string milieu, string sector, string subsector, string hex)
        {
            try
            {
                Sector = DbContext.FindSector(milieu, sector);
                if (Sector == null)
                    return NotFound();

                Subsector = DbContext.FindSubsector(Sector.Id, subsector);
                if (Subsector == null)
                    return NotFound();

                var found = DbContext.FindSystemData(Sector.Id, hex);
                if (found == null)
                {
                    found = DbContext.FindSystemData(Subsector.Id, hex);
                    if(found == null)
                        return NotFound();
                }

                found.Description = Data.Description;
                found.GMNotes = Data.GMNotes;
                DbContext.SaveChanges();

                return RedirectToPage();
            }
            catch(Exception ex)
            {
                Logger.LogError(1, ex, $"An error occurred while saving System Data");
                return StatusCode(500);
            }
        }

        public IActionResult OnGet(string milieu, string sector, string subsector, string hex)
        {
            try
            {
                Sector = DbContext.FindSector(milieu, sector);
                if (Sector == null)
                    return NotFound();

                Subsector = DbContext.FindSubsector(Sector.Id, subsector);
                if (Subsector == null)
                    return NotFound();

                Data = DbContext.FindSystemData(Sector.Id, hex);
                if (Data == null)
                    Data = DbContext.FindSystemData(Subsector.Id, hex);

                if (Data == null)
                {
                    var worlddata = Api.GetWorldData(Sector.Milieu, Sector.Code, hex);
                    if (worlddata == null)
                        return NotFound();

                    DbContext.UpdateSystemData(Sector, worlddata);
                    DbContext.SaveChanges();

                    // Reload the system data
                    Data = DbContext.FindSystemData(Sector.Id, hex);
                }

                if (Data == null)
                    return NotFound();

                Starport = DbContext.FindTextLookup(TextType.Starport, Data.UWP.Subcode(UWP.Starport));
                PlanetSize = DbContext.FindTextLookup(TextType.PlanetSize, Data.UWP.Subcode(UWP.PlanetSize));
                Atmosphere = DbContext.FindTextLookup(TextType.Atmosphere, Data.UWP.Subcode(UWP.Atmosphere));
                Hydrosphere = DbContext.FindTextLookup(TextType.Hydrosphere, Data.UWP.Subcode(UWP.Hydrosphere));
                Population = DbContext.FindPopulationLookup(Data);
                Government = DbContext.FindTextLookup(TextType.Government, Data.UWP.Subcode(UWP.Government));
                LawLevel = DbContext.FindTextLookup(TextType.LawLevel, Data.UWP.Subcode(UWP.LawLevel));
                TechLevel = DbContext.FindTextLookup(TextType.TechLevel, Data.UWP.Subcode(UWP.TechLevel));
                Importance = DbContext.FindTextLookup(TextType.Importance, Data.IX.Subcode(0));
                Allegiance = DbContext.FindTextLookup(TextType.Allegiance, Data.Allegiance);
                TradeCodes = DbContext.FindTradeCodeLookup(Data.Remarks);
                Economics = DbContext.FindTextLookup(TextType.Ecomomics, Data.EX);
                Culture = DbContext.FindTextLookup(TextType.Culture, Data.CX);
                System = CreateSystemDetails();
                TravelZone = DbContext.FindTextLookup(TextType.TravelZone, Data.Zone);
                Bases = DbContext.FindBases(Data);
                Climate = DbContext.FindTextLookup(TextType.Climate, "");
                Orbit = DbContext.FindTextLookup(TextType.Orbit, "");
                Gravity = DbContext.FindTextLookup(TextType.Gravity, "");

                Data.CalculateTradeFactors(Economics);
                Stellar = StellarDataFactory.Create(Data.Stellar);

                DbContext.AppendOwnerInformation(Data, TradeCodes);

                return Page();
            }
            catch(Exception ex)
            {
                Logger.LogError(1, ex, $"An error occurred while getting System Data");
                return StatusCode(500);
            }
        }

        private TextLookup CreateSystemDetails()
        {
            int worlds = Data.Worlds.GetValueOrDefault(1);
            Int32.TryParse(Data.PBG.Subcode(2), out int gasgiants);
            Int32.TryParse(Data.PBG.Subcode(1), out int belts);
            int rocky = worlds - gasgiants - belts - 1;

            var result = new TextLookup();
            result.Code = Data.PBG;
            result.ShortText = $"{worlds} worlds, including {Data.Name}, {gasgiants} gas giants, {belts} asteroid belts and {rocky} rocky worlds";
            return result;
        }

    }
}
