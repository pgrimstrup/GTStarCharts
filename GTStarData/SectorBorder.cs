using System;
using System.Collections.Generic;
using System.Text;

namespace GTStarData
{
    public class SectorBorder
    {
        public SectorBorder()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid SectorId { get; set; }
        public string Name { get; set; }
        public string Allegiance { get; set; }
        public string LabelPosition { get; set; }
        public bool WrapText { get; set; }
        public string Path { get; set; }

        public virtual Sector Sector { get; set; }
    }
}
