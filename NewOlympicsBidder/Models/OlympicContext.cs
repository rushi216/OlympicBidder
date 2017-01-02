using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace NewOlympicsBidder.Models
{
    public class OlympicContext : DbContext
    {
        public OlympicContext() : base("OlympicContext")
        {

        }

        public DbSet<Team> Team { get; set; }

        public DbSet<Participant> Participant { get; set; }
    }
}