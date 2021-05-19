using System;
using System.Collections.Generic;
using System.Text;

namespace GTStarData
{
    public class DataSource
    {
        public DataSource()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid SectorId { get; set; }
        public string Milieu { get; set; }
        public string Source { get; set; }

        public virtual Sector Sector { get; set; }
    }
}
