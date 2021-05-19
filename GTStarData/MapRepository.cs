using GTStarCharts.TravellerMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTStarData
{
    public static class MapRepository
    {
        public static void RefreshSectorNames(this GTStarDbContext context, MapAPI api)
        {
            var mapsectors = api.GetSectors();

            foreach (var mapsector in mapsectors.OrderBy(s => s.Y).ThenBy(s => s.X))
                context.UpdateSector(mapsector);

            context.SaveChanges();
        }

        public static void RefreshSectorDetails(this GTStarDbContext context, MapAPI api, Sector sector)
        {
            var data = api.GetSectorData(sector.Milieu, sector.DefaultName);
            context.UpdateSectorBorders(sector, data.Borders);
            context.UpdateSectorLabels(sector, data.Labels);
            context.UpdateSectorRoutes(sector, data.Routes);
            context.UpdateSubsectors(sector, data.Subsectors);
            context.UpdateDataSource(sector, data.DataFile);
            context.UpdateDataProducts(sector, data.Products);

            context.SaveChanges();

            context.Entry(sector).Collection(e => e.Subsectors).Load();
        }

        public static Sector UpdateSector(this GTStarDbContext context, MapSector mapsector)
        {
            // Find the sector at the map coordinates within the given milieu
            var sector = context.FindSector(mapsector.Milieu, mapsector.X, mapsector.Y);
            if (sector == null)
            {
                sector = new Sector();
                sector.Abbreviation = mapsector.Abbreviation;
                sector.Milieu = mapsector.Milieu;
                sector.Tags = mapsector.Tags;
                sector.X = mapsector.X;
                sector.Y = mapsector.Y;

                context.Sectors.Add(sector);
            }

            // Rebuild the entire list of names
            sector.Names.Clear();
            for (int index = 0; index < mapsector.Names.Length; index++)
            {
                var name = new SectorName();
                name.SectorId = sector.Id;
                name.SortOrder = index;
                name.Name = mapsector.Names[index].Text.Trim();
                name.Lang = mapsector.Names[index].Lang;

                sector.Names.Add(name);
                context.SectorNames.Add(name);
            }

            return sector;
        }

        public static void UpdateSectorBorders(this GTStarDbContext context, Sector sector, IEnumerable<MapBorder> borders)
        {
            var found = context.FindSectorBorders(sector.Id).ToList();
            foreach (var border in borders)
            {
                var sb = found.FirstOrDefault(f => f.Allegiance == border.Allegiance);
                if (sb == null)
                {
                    sb = new SectorBorder();
                    sb.SectorId = sector.Id;
                    sb.Allegiance = border.Allegiance;
                    context.SectorBorders.Add(sb);
                }

                sb.LabelPosition = border.LabelPosition;
                sb.Path = border.Path;
                sb.WrapText = border.WrapLabel;
                found.Remove(sb);
            }

            context.SectorBorders.RemoveRange(found);
        }

        public static void UpdateSectorLabels(this GTStarDbContext context, Sector sector, IEnumerable<MapLabel> labels)
        {
            var found = context.FindSectorLabels(sector.Id).ToList();
            foreach (var label in labels)
            {
                var sl = found.FirstOrDefault(f => f.Hex == label.Hex);
                if (sl == null)
                {
                    sl = new SectorLabel();
                    sl.SectorId = sector.Id;
                    sl.Hex = label.Hex;
                    context.SectorLabels.Add(sl);
                }

                sl.Title = label.Text;
                sl.WrapTitle = label.Wrap;
                found.Remove(sl);
            }

            context.SectorLabels.RemoveRange(found);
        }

        public static void UpdateSectorRoutes(this GTStarDbContext context, Sector sector, IEnumerable<MapRoute> routes)
        {
            var found = context.FindSectorRoutes(sector.Id).ToList();
            foreach (var route in routes)
            {
                var sr = found.FirstOrDefault(f => f.StartHex == route.Start && f.EndHex == route.End);
                if (sr == null)
                {
                    sr = new SectorRoute();
                    sr.SectorId = sector.Id;
                    sr.StartHex = route.Start;
                    sr.EndHex = route.End;
                    context.SectorRoutes.Add(sr);
                }

                sr.StartOffsetX = route.StartOffsetX;
                sr.StartOffsetY = route.StartOffsetY;
                sr.EndOffsetX = route.EndOffsetX;
                sr.EndOffsetY = route.EndOffsetY;
                found.Remove(sr);
            }

            context.SectorRoutes.RemoveRange(found);
        }

        public static void UpdateSubsectors(this GTStarDbContext context, Sector sector, IEnumerable<MapSubsector> subsectors)
        {
            var found = context.FindSubsectors(sector.Id).ToList();
            foreach (var subsect in subsectors)
            {
                var ss = found.FirstOrDefault(f => f.IndexNumber == subsect.IndexNumber);
                if (ss == null)
                {
                    ss = new Subsector();
                    ss.SectorId = sector.Id;
                    ss.IndexNumber = subsect.IndexNumber;
                    context.Subsectors.Add(ss);
                }

                ss.Name = subsect.Name;
                found.Remove(ss);
            }

            context.Subsectors.RemoveRange(found);
        }

        public static void UpdateSystemData(this GTStarDbContext context, Sector sector, MapSystemData worlddata)
        {
            foreach (var world in worlddata.Worlds)
            {
                var found = context.FindSystemData(sector.Id, world.Hex);
                if (found == null)
                {
                    found = new SystemData();
                    found.SectorId = sector.Id;
                    found.Hex = world.Hex;
                    context.SystemData.Add(found);
                }

                found.Allegiance = world.Allegiance;
                found.CX = world.Cx;
                found.EX = world.Ex;
                found.IX = world.Ix;
                found.PBG = world.PBG;
                found.Name = world.Name;
                found.Remarks = world.Remarks;
                found.UWP = world.UWP;
                found.Bases = world.Bases;
                found.Nobility = world.Nobility;
                found.Stellar = world.Stellar;
                found.SS = world.SS;
                found.Worlds = world.Worlds;
                found.ResourceUnits = world.ResourceUnits;
                found.Zone = world.Zone;

                // Find the Subsector
                int index = world.Subsector;

                var subsector = context.Subsectors.FirstOrDefault(e => e.SectorId == sector.Id && e.IndexNumber == index);
                if (subsector == null)
                {
                    subsector = new Subsector();
                    subsector.SectorId = sector.Id;
                    subsector.Name = world.SubsectorName;
                    subsector.IndexNumber = index;
                    context.Subsectors.Add(subsector);
                }
                found.Subsector = subsector;

            }
        }

        public static void UpdateDataSource(this GTStarDbContext context, Sector sector, MapDataFile source)
        {
            var found = context.FindDataSources(sector.Id).ToList();
            var ds = found.FirstOrDefault(e => e.Source == source.Source);
            if (ds == null)
            {
                ds = new DataSource();
                ds.SectorId = sector.Id;
                ds.Source = source.Source;
                context.DataSources.Add(ds);
            }
            ds.Milieu = source.Milieu;

            found.Remove(ds);

            context.DataSources.RemoveRange(found);
        }

        public static void UpdateDataProducts(this GTStarDbContext context, Sector sector, IEnumerable<MapProduct> products)
        {
            var found = context.FindDataProducts(sector.Id).ToList();
            foreach (var product in products)
            {
                var dp = found.FirstOrDefault(e => e.Title == product.Title);
                if (dp == null)
                {
                    dp = new DataProduct();
                    dp.SectorId = sector.Id;
                    context.DataProducts.Add(dp);
                }

                dp.Author = product.Author;
                dp.Publisher = product.Publisher;
                dp.Title = product.Title;
                dp.Url = product.Url;

                found.Remove(dp);
            }

            context.DataProducts.RemoveRange(found);
        }

    }
}
