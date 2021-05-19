using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GTStarData
{
    public class GTStarDbContext : DbContext
    {
        public const int CodeLength = 50;
        public const int NameLength = 200;

        public GTStarDbContext(DbContextOptions options)
            : base(options)
        {

        }

        public virtual DbSet<Sector> Sectors { get; set; }
        public virtual DbSet<SectorName> SectorNames { get; set; }
        public virtual DbSet<Subsector> Subsectors { get; set; }
        public virtual DbSet<SystemData> SystemData { get; set; }
        public virtual DbSet<SectorBorder> SectorBorders { get; set; }
        public virtual DbSet<SectorLabel> SectorLabels { get; set; }
        public virtual DbSet<SectorRoute> SectorRoutes { get; set; }
        public virtual DbSet<DataSource> DataSources { get; set; }
        public virtual DbSet<DataProduct> DataProducts { get; set; }
        public virtual DbSet<TextLookup> TextLookups { get; set; }

        protected override void OnModelCreating(ModelBuilder model)
        {
            var sectors = model.Entity<Sector>().ToTable("Sectors");
            sectors.HasKey(e => e.Id);
            sectors.Property(e => e.Abbreviation).HasMaxLength(CodeLength);
            sectors.Property(e => e.Milieu).HasMaxLength(CodeLength);
            sectors.Property(e => e.Tags).HasMaxLength(NameLength);
            sectors.Property(e => e.ThumbnailContentType).HasMaxLength(CodeLength);
            sectors.Property(e => e.ImageContentType).HasMaxLength(CodeLength);
            sectors.HasMany(e => e.Names).WithOne(e => e.Sector).HasForeignKey(e => e.SectorId);
            sectors.HasMany(e => e.Subsectors).WithOne(e => e.Sector).HasForeignKey(e => e.SectorId);
            sectors.HasMany(e => e.Borders).WithOne(e => e.Sector).HasForeignKey(e => e.SectorId);
            sectors.HasMany(e => e.Routes).WithOne(e => e.Sector).HasForeignKey(e => e.SectorId);
            sectors.HasMany(e => e.Labels).WithOne(e => e.Sector).HasForeignKey(e => e.SectorId);
            sectors.HasMany(e => e.Sources).WithOne(e => e.Sector).HasForeignKey(e => e.SectorId);
            sectors.HasMany(e => e.Products).WithOne(e => e.Sector).HasForeignKey(e => e.SectorId);

            var sectornames = model.Entity<SectorName>().ToTable("SectorNames");
            sectornames.HasKey(e => new { e.SectorId, e.SortOrder });
            sectornames.Property(e => e.Name).HasMaxLength(NameLength);
            sectornames.Property(e => e.Lang).HasMaxLength(CodeLength);

            var borders = model.Entity<SectorBorder>().ToTable("SectorBorders");
            borders.HasKey(e => e.Id);
            borders.Property(e => e.Allegiance).HasMaxLength(CodeLength);
            borders.Property(e => e.LabelPosition).HasMaxLength(CodeLength);
            borders.Property(e => e.Name).HasMaxLength(NameLength);
            //borders.Property(e => e.Path).IsMaxLength();

            var labels = model.Entity<SectorLabel>().ToTable("SectorLabels");
            labels.HasKey(e => e.Id);
            labels.Property(e => e.Color).HasMaxLength(CodeLength);
            labels.Property(e => e.Hex).HasMaxLength(CodeLength);
            labels.Property(e => e.Title).HasMaxLength(NameLength);

            var routes = model.Entity<SectorRoute>().ToTable("SectorRoutes");
            routes.HasKey(e => e.Id);
            routes.Property(e => e.Allegiance).HasMaxLength(CodeLength);
            routes.Property(e => e.EndHex).HasMaxLength(CodeLength);
            routes.Property(e => e.StartHex).HasMaxLength(CodeLength);

            var subsectors = model.Entity<Subsector>().ToTable("Subsectors");
            subsectors.HasKey(e => e.Id);
            subsectors.Property(e => e.Name).HasMaxLength(NameLength);

            var systems = model.Entity<SystemData>().ToTable("SystemData");
            systems.HasKey(e => e.Id);
            systems.Property(e => e.Hex).HasMaxLength(CodeLength);
            systems.Property(e => e.Name).HasMaxLength(NameLength);
            systems.Property(e => e.UWP).HasMaxLength(CodeLength);
            systems.Property(e => e.Allegiance).HasMaxLength(CodeLength);
            systems.Property(e => e.IX).HasMaxLength(CodeLength);
            systems.Property(e => e.EX).HasMaxLength(CodeLength);
            systems.Property(e => e.CX).HasMaxLength(CodeLength);
            systems.Property(e => e.PBG).HasMaxLength(CodeLength);
            systems.Property(e => e.Stellar).HasMaxLength(CodeLength);
            systems.Property(e => e.SS).HasMaxLength(CodeLength);
            systems.Property(e => e.Nobility).HasMaxLength(CodeLength);
            systems.Property(e => e.Bases).HasMaxLength(CodeLength);
            systems.Property(e => e.Remarks).HasMaxLength(CodeLength);
            systems.Property(e => e.Zone).HasMaxLength(CodeLength);
            systems.Property(e => e.JumpImageContentType).HasMaxLength(CodeLength);
            systems.HasOne(e => e.Sector).WithMany().HasForeignKey(e => e.SectorId);
            systems.HasOne(e => e.Subsector).WithMany(e => e.SystemData).HasForeignKey(e => e.SubsectorId);

            var sources = model.Entity<DataSource>().ToTable("DataSources");
            sources.HasKey(e => e.Id);
            sources.Property(e => e.Milieu).HasMaxLength(CodeLength);
            sources.Property(e => e.Source).HasMaxLength(NameLength);

            var products = model.Entity<DataProduct>().ToTable("DataProducts");
            products.HasKey(e => e.Id);
            products.Property(e => e.Author).HasMaxLength(NameLength);
            products.Property(e => e.Publisher).HasMaxLength(NameLength);
            products.Property(e => e.Title).HasMaxLength(NameLength);
            products.Property(e => e.Url).HasMaxLength(500);

            var text = model.Entity<TextLookup>().ToTable("TextLookups");
            text.HasKey(e => e.Id);
            text.Property(e => e.Code).HasMaxLength(CodeLength);

            base.OnModelCreating(model);
        }

    }
}
