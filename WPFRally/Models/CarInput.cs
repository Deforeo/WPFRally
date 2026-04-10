using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRally.Models
{
    public struct CarInput
    {
        public float throttle;   // 0..1
        public float brake;      // 0..1
        public float handbrake;  // 0..1
        public float turn;       // -1..1 (left..right)
    }
}
