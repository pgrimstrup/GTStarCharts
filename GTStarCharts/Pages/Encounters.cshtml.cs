using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTStarCharts.TravellerMap;
using GTStarData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GTStarCharts.Pages
{
    public class EncountersModel : PageModel
    {
        GTStarDbContext Context;
        MapAPI Maps;
        public SystemData SystemData { get; set; }

        public EncountersModel(GTStarDbContext context, MapAPI maps)
        {
            Context = context;
            Maps = maps;
        }

        public void OnGet(Guid sectorId, string hex)  
        {
            SystemData = Context.FindSystemData(sectorId, hex);
        }

        public void  OnPostStarshipEncounter(Guid id)
        {

        }

        public void OnPostStarportEncounter(Guid id)
        {

        }

        public void OnPostCityEncounter(Guid id)
        {

        }

        public void OnPostRuralEncounter(Guid id)
        {

        }
    }
}
