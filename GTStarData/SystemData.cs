using System;
using System.Collections.Generic;
using System.Text;

namespace GTStarData
{
    public class UWP
    {
        public const int Starport = 0;
        public const int PlanetSize = 1;
        public const int Atmosphere = 2;
        public const int Hydrosphere = 3;
        public const int Population = 4;
        public const int Government = 5;
        public const int LawLevel = 6;
        public const int TechLevel = 7;
    }

    public class SystemData
    {
        public SystemData()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid SectorId { get; set; }
        public Guid SubsectorId { get; set; }
        public string Allegiance { get; set; }
        public string Hex { get; set; }
        public string Name { get; set; }
        public string UWP { get; set; }
        public string Remarks { get; set; }
        public string IX { get; set; }
        public string EX { get; set; }
        public string CX { get; set; }
        public string PBG { get; set; }
        public string Bases { get; set; }
        public string Nobility { get; set; }
        public string Stellar { get; set; }
        public string SS { get; set; }
        public string Zone { get; set; }
        public int? Worlds { get; set; }
        public int? ResourceUnits { get; set; }
        public string Description { get; set; }
        public string GMNotes { get; set; }

        public byte[] JumpImageData { get; set; }
        public string JumpImageContentType { get; set; }
        public int? JumpImageScale { get; set; }
        public DateTimeOffset? JumpImageAttempt { get; set; }

        public virtual Sector Sector { get; set; }
        public virtual Subsector Subsector { get; set; }

        internal int AveragePlanetDiameter()
        {
            switch (UWP.Subcode(GTStarData.UWP.PlanetSize).ToInt())
            {
                case 1: return (800 + 2400) / 2;
                case 2: return (2400 + 4000) / 2;
                case 3: return (4000 + 6000) / 2;
                case 4: return (5600 + 7200) / 2;
                case 5: return (7200 + 8800) / 2;
                case 6: return (8800 + 10400) / 2;
                case 7: return (10400 + 12000) / 2;
                case 8: return (12000 + 13600) / 2;
                case 9: return (13600 + 15200) / 2;
                case 10: return (15200 + 16800) / 2;
                case 11: return (16800 + 18400) / 2;
                case 12: return 20000;

            }
            return 1000;
        }
    }
}
