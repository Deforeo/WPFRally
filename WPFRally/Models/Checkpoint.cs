using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace WPFRally.Models
{
    public class Checkpoint
    {
        public SKRect Rect { get; set; }
        public int Index { get; set; }
        public bool IsPassed { get; set; }

        public Checkpoint(float x, float y, float width, float height, int index)
        {
            Rect = new SKRect(x, y, x + width, y + height);
            Index = index;
            IsPassed = false;
        }

        public bool Intersects(SKRect vehicleRect) => Rect.IntersectsWith(vehicleRect);
    }
}
