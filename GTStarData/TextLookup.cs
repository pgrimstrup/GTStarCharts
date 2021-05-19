using System;
using System.Collections.Generic;
using System.Text;

namespace GTStarData
{
    public enum TextType
    {
        Unknown,
        Starport,
        PlanetSize,
        Atmosphere,
        Hydrosphere,
        Population,
        Government,
        LawLevel,
        TechLevel,
        Allegiance,
        Importance,
        TradeCode,
        Ecomomics,
        Culture,
        PBG,
        StarType,
        StartLuminosity,
        TravelZone,
        Bases,
        Climate,
        Orbit,
        Gravity
    }

    public class TextLookup
    {
        public TextLookup()
        {
            Id = Guid.Empty;
        }

        public Guid Id { get; set; }
        public TextType TextType { get; set; }
        public string Code { get; set; }
        public string ShortText { get; set; }
        public string LongText { get; set; }
    }
}
