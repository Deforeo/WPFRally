using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRally.Models
{
    public struct CarOutput
    {
        public float throttle;   // 0..1
        public float clutch;     // 0 (выжато) .. 1 (полностью включено)
        public int gear;         // -1 = задняя, 0 = нейтраль, 1,2,...
        public float brake;      // 0..1
        public float handbrake;  // 0..1
        public float desiredTurnAngle; // в радианах
    }
}
