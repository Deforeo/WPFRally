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
        public string SpritePath { get; set; }
        public bool IsVisible { get; set; } = true; // добавляем

        public Obstacle(float x, float y, float width, float height, string spritePath = null, bool isVisible = true)
        {
            Rect = new SKRect(x+16, y+16, x + width-16, y + height-16);
            SpritePath = spritePath;
            IsVisible = isVisible;
        }

        public bool CollidesWith(SKRect vehicleRect) => Rect.IntersectsWith(vehicleRect);
    }
}
