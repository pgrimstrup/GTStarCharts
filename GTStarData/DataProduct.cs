using System;
using System.Collections.Generic;
using System.Text;

namespace GTStarData
{
    public class DataProduct
    {
        public DataProduct()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid SectorId { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string Url { get; set; }

        public virtual Sector Sector { get; set; }
    }
}
