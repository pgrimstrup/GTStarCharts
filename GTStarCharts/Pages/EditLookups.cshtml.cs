using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTStarData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GTStarCharts.Pages
{
    public class EditLookupsModel : PageModel
    {
        readonly GTStarData.GTStarDbContext _context;

        [BindProperty]
        public TextType LookupType { get; set; }

        [BindProperty]
        public string NewCode { get; set; }

        [BindProperty]
        public string NewShortDescription { get; set; }

        [BindProperty]
        public string NewLongDescription { get; set; }

        public Dictionary<TextType, string> LookupTypes { get; set; }
        public List<TextLookup> Values { get; set; }

        public EditLookupsModel(GTStarDbContext context)
        {
            _context = context;

            LookupTypes = new Dictionary<TextType, string>();
            LookupTypes.Add(TextType.Unknown, "-- All --");
            LookupTypes.Add(TextType.Starport, "Starport");
            LookupTypes.Add(TextType.PlanetSize, "Planet Size");
            LookupTypes.Add(TextType.Atmosphere, "Atmosphere");
            LookupTypes.Add(TextType.Hydrosphere, "Hydrosphere");
            LookupTypes.Add(TextType.Population, "Population");
            LookupTypes.Add(TextType.Government, "Government");
            LookupTypes.Add(TextType.LawLevel, "Law Level");
            LookupTypes.Add(TextType.TechLevel, "Tech Level");
            LookupTypes.Add(TextType.Allegiance, "Allegiance");
            LookupTypes.Add(TextType.Importance, "Importance");
            LookupTypes.Add(TextType.TradeCode, "Trade Code");
            LookupTypes.Add(TextType.Ecomomics, "Economics");
            LookupTypes.Add(TextType.Culture, "Culture");
            LookupTypes.Add(TextType.TravelZone, "Travel Zone");
            LookupTypes.Add(TextType.Bases, "Base");
            LookupTypes.Add(TextType.Climate, "Climate");

        }

        public void OnGet()
        {
            if (TempData.ContainsKey("LookupType"))
                LookupType = (TextType)TempData["LookupType"];

            if (LookupType != TextType.Unknown)
                Values = _context.FindTextLookups(LookupType).ToList();
        

        }

        public IActionResult OnPostTextType()
        {
            TempData["LookupType"] = LookupType;
            return RedirectToPage();
        }

        public IActionResult OnPostAddValue()
        {
            if (LookupType != TextType.Unknown)
            {
                var lookup = _context.FindTextLookup(LookupType, NewCode);
                lookup.ShortText = NewShortDescription;
                lookup.LongText = NewLongDescription;
                if (lookup.Id == Guid.Empty)
                {
                    lookup.Id = Guid.NewGuid();
                    _context.TextLookups.Add(lookup);
                }
                _context.SaveChanges();
            }

            TempData["LookupType"] = LookupType;
            return RedirectToPage();
        }
    }
}
