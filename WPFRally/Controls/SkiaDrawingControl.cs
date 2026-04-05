using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace WPFRally.Controls
{
    public class SkiaDrawingControl : FrameworkElement
    {
        private SKSurface _surface;
        private SKBitmap _bitmap;

        public Action<SKCanvas> DrawAction { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var width = (int)ActualWidth;
            var height = (int)ActualHeight;
            if (width <= 0 || height <= 0) return;

            // Создаём или пересоздаём bitmap при изменении размера
            if (_bitmap == null || _bitmap.Width != width || _bitmap.Height != height)
            {
                _bitmap?.Dispose();
                _bitmap = new SKBitmap(width, height);
                _surface = SKSurface.Create(_bitmap.Info);
            }

            using (var canvas = _surface.Canvas)
            {
                canvas.Clear(SKColors.DarkGreen); // фон мира (трава/асфальт)
                DrawAction?.Invoke(canvas);
            }

            // Рисуем bitmap в WPF DrawingContext
            using (var skiaImage = SKImage.FromBitmap(_bitmap))
            using (var skiaData = skiaImage.Encode(SKEncodedImageFormat.Png, 100))
            {
                var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = skiaData.AsStream();
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                drawingContext.DrawImage(bitmapImage, new Rect(0, 0, width, height));
            }
        }

        public void Invalidate()
        {
            InvalidateVisual();
        }
    }
}
