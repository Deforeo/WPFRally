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
        public string SpritePath { get; set; } // <-- добавьте эту строку

        public Obstacle(float x, float y, float width, float height, string spritePath = null)
        {
            Rect = new SKRect(x, y, x + width, y + height);
            SpritePath = spritePath;
        }

        public bool CollidesWith(SKRect vehicleRect) => Rect.IntersectsWith(vehicleRect);
    }
}
