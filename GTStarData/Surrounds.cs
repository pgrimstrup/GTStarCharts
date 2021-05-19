using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTStarData
{
    public class SurroundingSubsectors
    {
        public SurroundingSubsector A { get; set; }
        public SurroundingSubsector B { get; set; }
        public SurroundingSubsector C { get; set; }

        public SurroundingSubsector D { get; set; }
        public SurroundingSubsector E { get; set; }
        public SurroundingSubsector F { get; set; }

        public SurroundingSubsector G { get; set; }
        public SurroundingSubsector H { get; set; }
        public SurroundingSubsector I { get; set; }

        public IEnumerable<SurroundingSubsector[]> Rows
        {
            get
            {
                yield return new SurroundingSubsector[] { A, B, C } ;
                yield return new SurroundingSubsector[] { D, E, F } ;
                yield return new SurroundingSubsector[] { G, H, I } ;
            }
        }
    }

    public class SurroundingSubsector
    {
        public string Milieu { get; set; }
        public string Code { get; set; }
        public Guid SectorId { get; set; }
        public string SectorName { get; set; }
        public Guid SubsectorId { get; set; }
        public string SubsectorName { get; set; }
        public bool IsCurrent { get; set; }

        public SurroundingSubsector()
        {

        }

        public SurroundingSubsector(Sector sector, Subsector subsector)
        {
            if (sector != null)
            {
                Milieu = sector.Milieu;
                SectorId = sector.Id;
                SectorName = sector.DefaultName;
            }

            if (subsector != null)
            {
                SubsectorId = subsector.Id;
                SubsectorName = subsector.Name;
                Code = subsector.Code;
            }
        }
    }
}
