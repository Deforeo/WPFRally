using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace WPFRally.Models
{
    public class TriggerZone
    {
        public SKRect Rect { get; set; }
        public string Type { get; set; } // "Start" или "Finish"
        public bool IsActive { get; set; } = true; // можно деактивировать после срабатывания

        public TriggerZone(float x, float y, float width, float height, string type)
        {
            Rect = new SKRect(x, y, x + width, y + height);
            Type = type;
        }

        public bool Contains(SKPoint point)
        {
            return Rect.Contains(point.X, point.Y);
        }

        public bool Intersects(SKRect vehicleRect)
        {
            return Rect.IntersectsWith(vehicleRect);
        }
    }
}
