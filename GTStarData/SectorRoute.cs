using System;
using System.Collections.Generic;
using System.Text;

namespace GTStarData
{
    public class SectorRoute
    {
        public SectorRoute()
        {

        }

        public Guid Id { get; set; }
        public Guid SectorId { get; set; }

        public string StartHex { get; set; }
        public string EndHex { get; set; }
        public string Allegiance { get; set; }

        public int StartOffsetX { get; set; }
        public int StartOffsetY { get; set; }
        public int EndOffsetX { get; set; }
        public int EndOffsetY { get; set; }

        public virtual Sector Sector { get; set; }
    }
}
