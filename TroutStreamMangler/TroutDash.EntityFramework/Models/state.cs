using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using TroutDash.EntityFramework.Models.Mapping;

namespace TroutDash.EntityFramework.Models
{
    public partial class state : GeometryBase
    {
        public state()
        {
            this.counties = new List<county>();
            this.restrictions = new List<restriction>();
            this.streams = new List<stream>();
            this.lakes = new List<lake>();
            this.Pal = new List<publicly_accessible_land>();
            this.PalTypes = new List<publicly_accessible_land_type>();
            this.roads = new List<road>();
            this.road_types = new List<road_type>();
        }

        public virtual ICollection<road> roads { get; set; }
        public virtual ICollection<road_type> road_types { get; set; }
        public int gid { get; set; }
        public string statefp { get; set; }
        public string short_name { get; set; }
        public string Name { get; set; }
        public virtual ICollection<lake> lakes { get; set; } 
        public virtual ICollection<county> counties { get; set; }
        
        public virtual ICollection<restriction> restrictions { get; set; }
        public virtual ICollection<stream> streams { get; set; }
        public virtual ICollection<publicly_accessible_land> Pal { get; set; }
        public virtual ICollection<publicly_accessible_land_type> PalTypes { get; set; }
        
    }
}
