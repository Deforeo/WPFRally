using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace WPFRally.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float WorldWidth { get; set; }
        public float WorldHeight { get; set; }
        public SKPoint StartPosition { get; set; }
        public SKPoint FinishPosition { get; set; }
        public List<ObstacleData> Obstacles { get; set; }
        public string BackgroundSpritePath { get; set; } // фон трассы
    }

    public class ObstacleData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string SpritePath { get; set; }
    }
}
