using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NewOlympicsBidder.Models
{
    public class Participant
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public long Value { get; set; }

        public Status Status { get; set; }

        public bool IsCurrent { get; set; }

        public int? TeamId { get; set; }

        public string Expertise { get; set; }

        [ForeignKey("TeamId")]
        public virtual Team Team { get; set; }
    }

    public enum Status
    {
        Remaining,
        Skipped,
        Sold
    }
}