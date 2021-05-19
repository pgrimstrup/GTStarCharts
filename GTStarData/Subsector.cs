using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GTStarData
{
    public class Subsector
    {
        public Subsector()
        {
            Id = Guid.NewGuid();
            SystemData = new HashSet<SystemData>();
        }

        public Guid Id { get; set; }
        public Guid SectorId { get; set; }
        public int IndexNumber { get; set; } // 0 to 15, equals A to P
        public string Name { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageContentType { get; set; }
        public DateTimeOffset? ImageAttempt { get; set; }
        public int? ImageScale { get; set; }


        [NotMapped]
        public string Code
        {
            get { return new string(new char[] { (Char)(65 + IndexNumber) }); }
        }

        public virtual Sector Sector { get; set; }
        public virtual HashSet<SystemData> SystemData { get; set; }
    }
}
