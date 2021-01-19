using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TrailerModels
{
    public class AuditTrail
    {
        public int Id { get; set; }
        public EntityObjectState Action { get; set; }
        public string EntityType { get; set; }
        public string OriginalEntity { get; set; }  // JSON
        public string NewEntity { get; set; }   // JSON
        public DateTime AuditDateTime { get; set; }
        public string UserId { get; set; }
    }

    public enum EntityObjectState
    {
        Added = 1,
        Deleted,
        Modified,
        Unchanged
    }
}
