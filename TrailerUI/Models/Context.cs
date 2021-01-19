using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using TrailerModels;

namespace TrailerUI.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<AuditTrail> AuditTrails { get; set; }

    }

 

}