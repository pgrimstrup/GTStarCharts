using System;
using System.Collections.Generic;
using System.Text;

namespace GTStarData
{
    public class SectorLabel
    {
        public SectorLabel()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public Guid SectorId { get; set; }

        public string Hex { get; set; }
        public string Color { get; set; }
        public string Title { get; set; }
        public bool WrapTitle { get; set; }

        public virtual Sector Sector { get; set; }
    }
}
