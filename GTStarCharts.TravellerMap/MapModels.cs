using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GTStarCharts.TravellerMap
{
    public class MapResponse
    {
        public MapSector[] Sectors { get; set; }
    }

    public class MapImageData
    {
        public int Scale { get; set; }
        public byte[] ImageData { get; set; }
        public string ContentType { get; set; }
    }

    public class MapSector
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Milieu { get; set; }
        public string Tags { get; set; }
        public string Abbreviation { get; set; }

        public MapSectorName[] Names { get; set; }

    }

    public class MapSectorName
    {
        public string Text { get; set; }
        public string Lang { get; set; }
    }

    public class MapSectorDataResponse
    {
        public bool Selected { get; set; }
        public string Tags { get; set; }
        public string Abbreviation { get; set; }
        public MapSectorName[] Names { get; set; }
        public string Credits { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public MapProduct[] Products { get; set; }
        public MapDataFile DataFile { get; set; }
        public MapSubsector[] Subsectors { get; set; }
        public MapAllegiance[] Allegiances { get; set; }
        public MapLabel[] Labels { get; set; }
        public MapBorder[] Borders { get; set; }
        public MapRoute[] Routes { get; set; }
    }

    public class MapProduct
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }

        [JsonProperty("Ref")]
        public string Url { get; set; }
    }

    public class MapDataFile
    {
        public string Source { get; set; }
        public string Milieu { get; set; }
    }

    public class MapSubsector
    {
        public string Name { get; set; }
        public string Index { get; set; }
        public int IndexNumber { get; set; }
    }

    public class MapAllegiance
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Base { get; set; }
    }

    public class MapLabel
    {
        public string Hex { get; set; }
        public string Color { get; set; }
        public bool Wrap { get; set; }
        public string Text { get; set; }
    }

    public class MapBorder
    {
        public bool WrapLabel { get; set; }
        public string Color { get; set; }
        public string Allegiance { get; set; }
        public string LabelPosition { get; set; }
        public string Path { get; set; }
    }

    public class MapRoute
    {
        public string Start { get; set; }
        public string End { get; set; }
        public int StartOffsetX { get; set; }
        public int StartOffsetY { get; set; }
        public int EndOffsetX { get; set; }
        public int EndOffsetY { get; set; }

        public string Allegiance { get; set; }
    }

    public class MapSystemData
    {
        public MapWorldData[] Worlds { get; set; }
    }

    public class MapWorldData
    {
        public string Name { get; set; }
        public string Hex { get; set; }
        public string UWP { get; set; }
        public string PBG { get; set; }
        public string Zone { get; set; }
        public string Bases { get; set; }
        public string Allegiance { get; set; }
        public string Stellar { get; set; }
        public string SS { get; set; }
        public string Ix { get; set; }
        public int CalculatedImportance { get; set; }
        public string Ex { get; set; }
        public string Cx { get; set; }
        public string Nobility { get; set; }
        public int Worlds { get; set; }
        public int ResourceUnits { get; set; }
        public int Subsector { get; set; }
        public int Quadrant { get; set; }
        public int WorldX { get; set; }
        public int WorldY { get; set; }
        public string Remarks { get; set; }
        public string LegacyBaseCode { get; set; }
        public string Sector { get; set; }
        public string SubsectorName { get; set; }
        public string SectorAbbreviation { get; set; }
        public string AllegianceName { get; set; }
    }
}
