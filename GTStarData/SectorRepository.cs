using GTStarCharts.TravellerMap;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GTStarData
{
    public static class SectorRepository
    {
        public static Sector FindSector(this GTStarDbContext context, Guid id)
        {
            // Find sector b ID
            var q = GetBaseQuery(context);
            var sector = q.SingleOrDefault(s => s.Id == id);
            return sector;
        }

        public static Sector FindSector(this GTStarDbContext context, string millieu, string name)
        {
            // Find the named sector for a given milieu
            var q = GetBaseQuery(context);
            var sector = q.SingleOrDefault(s => s.Milieu == millieu && s.Names.Any(n => n.Name == name));
            return sector;
        }

        public static Sector FindSector(this GTStarDbContext context, string milieu, int mapx, int mapy)
        {
            // Find the sector at the specific map location for a given milieu
            var q = GetBaseQuery(context);
            var sector = q.FirstOrDefault(s => s.Milieu == milieu && s.X == mapx && s.Y == mapy);
            return sector;
        }

        public static Sector[] FindSectors(this GTStarDbContext context, string milieu, int minx, int miny, int maxx, int maxy)
        {
            // Find all sectors within a given grid
            var q = GetBaseQuery(context);
            if (!String.IsNullOrEmpty(milieu))
                q = q.Where(s => s.Milieu == milieu);

            if (minx != 0 || miny != 0 || maxx != 0 || maxy != 0)
                q = q.Where(s => s.X >= minx && s.X <= maxx && s.Y >= miny && s.Y <= maxy);

            return q.OrderBy(s => s.Y).ThenBy(s => s.X).ToArray();
        }

        public static Subsector FindSubsector(this GTStarDbContext context, Guid id)
        {
            // Find subsector by ID
            var q = from ss in context.Subsectors
                        .Include(e => e.SystemData)
                    where ss.Id == id
                    select ss;

            return q.SingleOrDefault();
        }

        public static Subsector FindSubsector(this GTStarDbContext context, Guid sectorId, string name)
        {
            // Find subsector by ID
            var q = from ss in context.Subsectors
                        .Include(e => e.SystemData)
                    where ss.SectorId == sectorId && ss.Name == name
                    select ss;

            return q.SingleOrDefault();
        }

        public static Subsector FindSubsector(this GTStarDbContext context, string milieu, string sector, string subsector)
        {
            // Try to find the subsector based on name within the sector
            var q = from ss in context.Subsectors
                        .Include(e => e.SystemData)
                    where ss.Sector.Milieu == milieu && ss.Sector.Names.Any(n => n.Name == sector) && ss.Name == subsector
                    select ss;

            var result = q.SingleOrDefault();
            if (result == null && subsector.Length == 1)
            {
                // Try to find the subsector based on subsector code
                string subsectorcodes = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                int index = subsectorcodes.IndexOf(subsector.ToUpper()[0]);
                if (index >= 0)
                {
                    q = from ss in context.Subsectors
                                .Include(e => e.SystemData)
                        where ss.Sector.Milieu == milieu && ss.Sector.Names.Any(n => n.Name == sector) && ss.IndexNumber == index
                        select ss;
                    result = q.SingleOrDefault();
                }
            }

            return result;
        }

        public static TextLookup FindTextLookup(this GTStarDbContext context, Guid? lookupId)
        {
            if (lookupId == null)
                return null;

            var q = from sa in context.TextLookups
                    where sa.Id == lookupId
                    select sa;
            return q.FirstOrDefault();
        }

        public static TextLookup FindTextLookup(this GTStarDbContext context, TextType type, string code)
        {
            var q = from tl in context.TextLookups
                    where tl.TextType == type && tl.Code == code
                    select tl;

            var lookup = q.FirstOrDefault();
            if (lookup == null)
            {
                lookup = new TextLookup();
                lookup.TextType = type;
                lookup.Code = code;
            }
            return lookup;
        }

        public static TextLookup FindTradeCodeLookup(this GTStarDbContext context, string code)
        {
            string[] codes = code.Split();
            var q = from tl in context.TextLookups
                    where tl.TextType == TextType.TradeCode && codes.Contains(tl.Code)
                    select tl;

            var result = new TextLookup();
            result.Code = code;
            result.ShortText = String.Join(" ", q.Select(tl => tl.ShortText.TrimEnd('.') + "."));
            return result;
        }

        public static TextLookup FindPopulationLookup(this GTStarDbContext context, SystemData system)
        {
            var lookup = FindTextLookup(context, TextType.Population, system.UWP.Subcode(4));
            var multiplier = system.PBG.Subcode(0);
            if (!String.IsNullOrEmpty(multiplier))
            {
                int pow = Int32.Parse(lookup.Code, System.Globalization.NumberStyles.HexNumber);
                int bas = Int32.Parse(multiplier);
                lookup.ShortText += $" {bas * Math.Pow(10, pow):n0} permanent residents.";
            }


            return lookup;
        }

        public static TextLookup FindBases(this GTStarDbContext context, SystemData system)
        {
            var lookup = new TextLookup();
            lookup.Code = system.Bases;
            if (String.IsNullOrEmpty(system.Bases))
                return lookup;

            var descriptions = new List<string>();
            for (int i = 0; i < system.Bases.Length; i++)
            {
                var basecode = FindTextLookup(context, TextType.Bases, system.Bases.Substring(i, 1));
                if (basecode != null && !String.IsNullOrEmpty(basecode.ShortText))
                    descriptions.Add(basecode.ShortText);
            }
            lookup.ShortText = String.Join(", ", descriptions);

            return lookup;
        }

        public static TextLookup AppendOwnerInformation(this GTStarDbContext context, SystemData system, TextLookup lookup)
        {
            if (String.IsNullOrEmpty(system.Remarks))
                return lookup;

            string[] codes = system.Remarks.Split();
            var ownerCode = codes.FirstOrDefault(c => c.StartsWith("O:"));
            if (ownerCode == null)
                return lookup;

            var owner = FindSystemData(context, system.SectorId, ownerCode.Substring(2));
            if (owner == null)
                return lookup;

            if (lookup.ShortText != null)
                lookup.ShortText = lookup.ShortText.TrimEnd('.', ' ') + ".";
            lookup.ShortText += $" {system.Name} is a territory of {owner.Name} ({owner.Hex})";
            return lookup;
        }

        public static IEnumerable<TextLookup> FindTextLookups(this GTStarDbContext context, TextType type)
        {
            var q = from tl in context.TextLookups
                    where tl.TextType == type
                    orderby tl.Code
                    select tl;

            return q;
        }

        public static SectorLabel[] FindSectorLabels(this GTStarDbContext context, Guid sectorId)
        {
            var q = from sl in context.SectorLabels
                    where sl.SectorId == sectorId
                    select sl;
            return q.ToArray();
        }

        public static SectorBorder[] FindSectorBorders(this GTStarDbContext context, Guid sectorId)
        {
            var q = from sb in context.SectorBorders
                    where sb.SectorId == sectorId
                    select sb;
            return q.ToArray();
        }

        public static SectorRoute[] FindSectorRoutes(this GTStarDbContext context, Guid sectorId)
        {
            var q = from sr in context.SectorRoutes
                    where sr.SectorId == sectorId
                    select sr;
            return q.ToArray();
        }

        public static Subsector[] FindSubsectors(this GTStarDbContext context, Guid sectorId)
        {
            var q = from ss in context.Subsectors
                    where ss.SectorId == sectorId
                    select ss;
            return q.ToArray();
        }

        public static SystemData FindSystemData(this GTStarDbContext context, Guid id)
        {
            var q = from sd in context.SystemData
                        .Include(e => e.Subsector)
                    where sd.Id == id
                    select sd;

            var data = q.FirstOrDefault();
            return data;
        }

        /// <summary>
        /// Returns the SystemData for a specific system
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sectorId">Either the Sector ID or Subsector ID that contains the System</param>
        /// <param name="hex">Either the Sector Hex number or the name of the System to find</param>
        /// <returns></returns>
        public static SystemData FindSystemData(this GTStarDbContext context, Guid sectorId, string hex)
        {
            // Try to find the hex within the sector (hexes are unique to a sector)
            var q = from sd in context.SystemData
                        .Include(e => e.Subsector)
                    where sd.SectorId == sectorId && sd.Hex == hex
                    select sd;

            var data = q.FirstOrDefault();

            if(data == null)
            {
                // If not found, try again assuming theat a subsector id was provided
                q = from sd in context.SystemData
                            .Include(e => e.Subsector)
                        where sd.SubsectorId == sectorId && sd.Hex == hex
                        select sd;
                data = q.FirstOrDefault();

                if(data == null)
                {
                    // If still not found, then try to match on Subsector Id and System Name
                    q = from sd in context.SystemData
                                .Include(e => e.Subsector)
                        where sd.SubsectorId == sectorId && sd.Name == hex
                        select sd;
                    data = q.FirstOrDefault();

                }
            }
            return data;
        }

        public static DataSource[] FindDataSources(this GTStarDbContext context, Guid sectorId)
        {
            var q = from ds in context.DataSources
                    where ds.SectorId == sectorId
                    select ds;
            return q.ToArray();
        }

        public static DataProduct[] FindDataProducts(this GTStarDbContext context, Guid sectorId)
        {
            var q = from dp in context.DataProducts
                    where dp.SectorId == sectorId
                    select dp;
            return q.ToArray();
        }

        public static SurroundingSubsectors FindSurrounds(this GTStarDbContext context, Subsector subsector)
        {
            SurroundingSubsectors surrounds = new SurroundingSubsectors();

            // Note that offset of 2 or -2 is not used since we want a 3x3 grid extracted
            // from a 4x4 mapping system
            surrounds.A = FindSurroundingSubsector(context, subsector, -5);
            surrounds.B = FindSurroundingSubsector(context, subsector, -4);
            surrounds.C = FindSurroundingSubsector(context, subsector, -3);
            surrounds.D = FindSurroundingSubsector(context, subsector, -1);
            surrounds.E = FindSurroundingSubsector(context, subsector, 0);
            surrounds.F = FindSurroundingSubsector(context, subsector, 1);
            surrounds.G = FindSurroundingSubsector(context, subsector, 3);
            surrounds.H = FindSurroundingSubsector(context, subsector, 4);
            surrounds.I = FindSurroundingSubsector(context, subsector, 5);

            return surrounds;
        }

        public static SurroundingSubsectors FindSurrounds(this GTStarDbContext context, Sector sector)
        {
            SurroundingSubsectors surrounds = new SurroundingSubsectors();

            // Simply offset each the mapx and mapy to find the surrounding sector
            surrounds.A = FindSurroundingSector(context, sector, -1, -1);
            surrounds.B = FindSurroundingSector(context, sector, 0, -1);
            surrounds.C = FindSurroundingSector(context, sector, 1, -1);
            surrounds.D = FindSurroundingSector(context, sector, -1, 0);
            surrounds.E = FindSurroundingSector(context, sector, 0, 0);
            surrounds.E.IsCurrent = true;
            surrounds.F = FindSurroundingSector(context, sector, 1, 0);
            surrounds.G = FindSurroundingSector(context, sector, -1, 1);
            surrounds.H = FindSurroundingSector(context, sector, 0, 1);
            surrounds.I = FindSurroundingSector(context, sector, 1, 1);

            return surrounds;
        }

        private static SurroundingSubsector FindSurroundingSector(GTStarDbContext context, Sector sector, int dx, int dy)
        {
            var found = context.FindSector(sector.Milieu, sector.X + dx, sector.Y + dy);
            return new SurroundingSubsector(found, null);
        }

        private static SurroundingSubsector FindSurroundingSubsector(GTStarDbContext context, Subsector subsector, int index)
        {
            int[] top = { -5, -4, -3 };
            int[] left = { -5, -1, 3 };
            int[] bottom = { 3, 4, 5 };
            int[] right = { -3, 1, 5 };

            // Initial target index for the neighboring subsector
            int targetindex = subsector.IndexNumber + index;

            // Determine the sector offset for the neighboring subsector, and adjust the target index
            int mapx = 0;
            int mapy = 0;
            if (left.Contains(index) && subsector.IndexNumber % 4 == 0)
            {
                mapx = -1;
                targetindex += 4;
            }
            if (right.Contains(index) && subsector.IndexNumber % 4 == 3)
            {
                mapx = 1;
                targetindex -= 4;
            }
            if (top.Contains(index) && subsector.IndexNumber < 4)
            {
                mapy = -1;
                targetindex += 16;
            }
            if (bottom.Contains(index) && subsector.IndexNumber > 12)
            {
                mapy = 1;
                targetindex -= 16;
            }

            // Find the sector
            Sector sector = subsector.Sector;
            if (mapx != 0 || mapy != 0)
            {
                sector = FindSector(context, subsector.Sector.Milieu, subsector.Sector.X + mapx, subsector.Sector.Y + mapy);
                if (sector != null && sector.Subsectors.Count == 0)
                {
                    MapAPI api = new MapAPI();
                    context.RefreshSectorDetails(api, sector);
                    sector = FindSector(context, subsector.Sector.Milieu, subsector.Sector.X + mapx, subsector.Sector.Y + mapy);
                }
            }

            if (sector == null)
                return new SurroundingSubsector { SubsectorName = $"{mapx}:{mapy}" };

            // Find the subsector
            Subsector neighbor = sector.Subsectors.FirstOrDefault(s => s.IndexNumber == targetindex);
            if (neighbor == null)
                return new SurroundingSubsector { SectorId = sector.Id, SectorName = sector.DefaultName, SubsectorName = $"{targetindex}" };

            var result = new SurroundingSubsector(sector, neighbor);
            result.IsCurrent = (index == 0);
            return result;
        }


        private static IQueryable<Sector> GetBaseQuery(GTStarDbContext context)
        {
            var q = from sector in context.Sectors
                        .Include(s => s.Names)
                        .Include(s => s.Subsectors)
                    select sector;

            return q;
        }

    }
}
