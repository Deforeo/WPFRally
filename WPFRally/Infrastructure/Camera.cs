using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using OpenTK;

namespace WPFRally.Infrastructure
{
    public class Camera
    {
        public SKPoint Offset { get; private set; }  // смещение относительно мира
        public float Zoom { get; set; } = 1.0f;

        // Центрирует камеру на цели (обычно машинка)
        public void Follow(SKPoint target, float viewportWidth, float viewportHeight, float worldWidth, float worldHeight)
        {
            float targetX = target.X - viewportWidth / (2 * Zoom);
            float targetY = target.Y - viewportHeight / (2 * Zoom);

            targetX = MathHelper.Clamp(targetX, 0, worldWidth - viewportWidth / Zoom);
            targetY = MathHelper.Clamp(targetY, 0, worldHeight - viewportHeight / Zoom);

            Offset = new SKPoint(targetX, targetY);
        }

        // Преобразование координат мира в экранные
        public SKPoint WorldToScreen(SKPoint worldPoint, float viewportWidth, float viewportHeight)
        {
            float screenX = (worldPoint.X - Offset.X) * Zoom;
            float screenY = (worldPoint.Y - Offset.Y) * Zoom;
            return new SKPoint(screenX, screenY);
        }

        // Прямоугольник мира, который виден в окне
        public SKRect GetVisibleWorldRect(float viewportWidth, float viewportHeight)
        {
            return new SKRect(
                Offset.X,
                Offset.Y,
                Offset.X + viewportWidth / Zoom,
                Offset.Y + viewportHeight / Zoom
            );
        }
    }
}