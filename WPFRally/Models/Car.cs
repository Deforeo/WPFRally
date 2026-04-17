using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRally.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float MaxSpeed { get; set; }
        public float Acceleration { get; set; }
        public float Grip { get; set; }
        public string ColorHex { get; set; }
        public string SpritePath { get; set; } // путь к спрайту
    }
}
