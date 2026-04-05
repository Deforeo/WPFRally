using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace WPFRally.Models
{
    public class Obstacle
    {
        public SKRect Rect { get; set; }
        public SKColor Color { get; set; } = SKColors.Gray;

        public Obstacle(float x, float y, float width, float height)
        {
            Rect = new SKRect(x, y, x + width, y + height);
        }

        public bool CollidesWith(SKRect vehicleRect)
        {
            return Rect.IntersectsWith(vehicleRect);
        }
    }
}
