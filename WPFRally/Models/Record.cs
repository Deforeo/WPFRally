using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRally.Models
{
    public class Record
    {
        public int TrackId { get; set; }
        public float TimeSeconds { get; set; }
        public string Initials { get; set; }
        public DateTime Date { get; set; }
    }
}
