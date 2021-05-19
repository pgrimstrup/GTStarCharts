using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GTStarData
{
    public class Sector
    {
        public Sector()
        {
            Id = Guid.NewGuid();
            Names = new HashSet<SectorName>();
            Labels = new HashSet<SectorLabel>();
            Borders = new HashSet<SectorBorder>();
            Routes = new HashSet<SectorRoute>();
            Subsectors = new HashSet<Subsector>();
        }

        public Guid Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Milieu { get; set; }
        public string Abbreviation { get; set; }
        public string Tags { get; set; }
        public byte[] ThumbnailData { get; set; }
        public string ThumbnailContentType { get; set; }
        public DateTimeOffset? ThumbnailAttempt { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageContentType { get; set; }
        public DateTimeOffset? ImageAttempt { get; set; }
        public int? ImageScale { get; set; }

        public virtual HashSet<SectorName> Names { get; set; }
        public virtual HashSet<Subsector> Subsectors { get; set; }
        public virtual HashSet<SectorBorder> Borders { get; set; }
        public virtual HashSet<SectorRoute> Routes { get; set; }
        public virtual HashSet<SectorLabel> Labels { get; set; }
        public virtual HashSet<DataSource> Sources { get; set; }
        public virtual HashSet<DataProduct> Products { get; set; }

        [NotMapped]
        public string DefaultName
        {
            get { return Names.FirstOrDefault()?.Name; }
        }

        [NotMapped]
        public string UrlName
        {
            get { return Names.FirstOrDefault()?.Name?.Replace(" ", "+"); }
        }

        [NotMapped]
        public string Code
        {
            get { return Abbreviation ?? DefaultName; }
        }
    }


    public class SectorName
    {
        public Guid SectorId { get; set; }
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public string Lang { get; set; }
        public virtual Sector Sector { get; set; }
    }


}
