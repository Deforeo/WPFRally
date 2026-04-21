using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using WPFRally.Models;
using WPFRally.Infrastructure;
using SkiaSharp.Views.Desktop;


namespace WPFRally.Views
{
    public partial class RaceView : UserControl
    {
        // Ссылки на игровой мир и камеру (передаются из MainWindow)
        public GameWorld World { get; set; }
        public Camera Camera { get; set; }
        public Car SelectedCar { get; set; }
        public Track SelectedTrack { get; set; }

        // --- Управление ---
        private bool _gasPressed;
        private bool _brakePressed;
        private bool _handbrakePressed;
        private float _steer;

        // --- Таймер ---
        private DateTime _lastUpdate;

        // Событие для уведомления MainWindow о финише
        public event Action<float, Track> RaceFinished;

        public RaceView()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _lastUpdate = DateTime.Now;
            CompositionTarget.Rendering += OnRendering;
            this.Focus(); // чтобы клавиши работали сразу
        }

        private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        // Игровой цикл
        private void OnRendering(object sender, EventArgs e)
        {
            if (World == null) return;

            var now = DateTime.Now;
            float deltaTime = (float)(now - _lastUpdate).TotalSeconds;
            if (deltaTime > 0.033f) deltaTime = 0.033f;
            _lastUpdate = now;

            float throttle = _gasPressed ? 1f : 0f;
            float brake = _brakePressed ? 1f : 0f;
            float handbrake = _handbrakePressed ? 1f : 0f;

            // Обновляем физику мира
            World.Update(deltaTime, throttle, brake, handbrake, _steer);

            // Если финиш и событие не вызвано – оповещаем
            if (World.IsFinished && RaceFinished != null)
            {
                RaceFinished.Invoke(World.RaceTime, SelectedTrack);
                // Отключаем событие, чтобы не вызывать повторно
                RaceFinished = null;
            }

            // Перерисовываем
            skiaElement.InvalidateVisual();
        }

        // Отрисовка (ваш код, но с небольшими улучшениями)
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (World == null || Camera == null) return;

            var canvas = e.Surface.Canvas;
            float viewW = e.Info.Width;
            float viewH = e.Info.Height;

            // Фон
            if (SelectedTrack != null && !string.IsNullOrEmpty(SelectedTrack.BackgroundSpritePath))
            {
                var bgSprite = SpriteManager.GetSprite(SelectedTrack.BackgroundSpritePath);
                if (bgSprite != null)
                    canvas.DrawBitmap(bgSprite, new SKRect(0, 0, viewW, viewH));
                else
                    canvas.Clear(SKColors.DarkGreen);
            }
            else
                canvas.Clear(SKColors.DarkGreen);

            // Камера
            Camera.Follow(World.Player.ToSKPoint(), viewW, viewH, World.WorldWidth, World.WorldHeight);

            // Препятствия
            foreach (var obs in World.Obstacles)
            {
                SKPoint screenPos = new SKPoint(
                    (obs.Rect.Left - Camera.Offset.X) * Camera.Zoom,
                    (obs.Rect.Top - Camera.Offset.Y) * Camera.Zoom);
                float w = obs.Rect.Width * Camera.Zoom;
                float h = obs.Rect.Height * Camera.Zoom;
                SKRect screenRect = new SKRect(screenPos.X, screenPos.Y, screenPos.X + w, screenPos.Y + h);

                var sprite = SpriteManager.GetSprite(obs.SpritePath);
                if (sprite != null)
                    canvas.DrawBitmap(sprite, screenRect);
                else
                {
                    using (var paint = new SKPaint { Color = obs.Color, Style = SKPaintStyle.Fill })
                        canvas.DrawRect(screenRect, paint);
                }
            }

            // Зоны старт/финиш
            foreach (var zone in World.TriggerZones)
            {
                SKRect screenRect = new SKRect(
                    (zone.Rect.Left - Camera.Offset.X) * Camera.Zoom,
                    (zone.Rect.Top - Camera.Offset.Y) * Camera.Zoom,
                    (zone.Rect.Right - Camera.Offset.X) * Camera.Zoom,
                    (zone.Rect.Bottom - Camera.Offset.Y) * Camera.Zoom);
                SKColor zoneColor = zone.Type == "Start" ? SKColors.Green : SKColors.Red;
                using (var fill = new SKPaint { Color = zoneColor.WithAlpha(80), Style = SKPaintStyle.Fill })
                    canvas.DrawRect(screenRect, fill);
                using (var stroke = new SKPaint { Color = zoneColor, Style = SKPaintStyle.Stroke, StrokeWidth = 3 })
                    canvas.DrawRect(screenRect, stroke);
            }

            // Машина
            var carPos = World.Player.ToSKPoint();
            SKPoint screenCar = new SKPoint(
                (carPos.X - Camera.Offset.X) * Camera.Zoom,
                (carPos.Y - Camera.Offset.Y) * Camera.Zoom
            );
            canvas.Save();
            canvas.Translate(screenCar.X, screenCar.Y);

            // КОРРЕКЦИЯ: поворачиваем спрайт так, чтобы его "нос" совпадал с направлением движения
            // Так как спрайт нарисован вверх (Y), а угол 0 - вправо (X), вычитаем 90°
            canvas.RotateRadians(World.Player.Angle + (float)(Math.PI / 2));

            SKBitmap carSprite = null;
            if (SelectedCar != null && !string.IsNullOrEmpty(SelectedCar.SpritePath))
                carSprite = SpriteManager.GetSprite(SelectedCar.SpritePath);

            if (carSprite != null)
            {
                // Целевой размер в экранных пикселях
                float targetW = World.Player.Width * Camera.Zoom;
                float targetH = World.Player.Height * Camera.Zoom;
                float scale = Math.Min(targetW / carSprite.Width, targetH / carSprite.Height);
                float drawW = carSprite.Width * scale;
                float drawH = carSprite.Height * scale;
                SKRect destRect = new SKRect(-drawW / 2, -drawH / 2, drawW / 2, drawH / 2);
                canvas.DrawBitmap(carSprite, destRect);
            }
            else
            {
                // fallback – цветной прямоугольник
                float w = World.Player.Width * Camera.Zoom;
                float h = World.Player.Height * Camera.Zoom;
                SKRect rect = new SKRect(-w / 2, -h / 2, w / 2, h / 2);
                using (var paint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Fill })
                    canvas.DrawRect(rect, paint);
            }
            canvas.Restore();

            // HUD – таймер
            string timeText = World.IsRaceActive ? $"Time: {World.RaceTime:F2}s" :
                              (World.IsFinished ? $"Finished! {World.RaceTime:F2}s" : "Not started");
            using (var font = new SKFont(SKTypeface.FromFamilyName("Arial"), 24f))
            using (var paint = new SKPaint { Color = SKColors.White })
                canvas.DrawText(timeText, 20, 40, SKTextAlign.Left, font, paint);
        }

        // --- Управление с клавиатуры ---
        /*private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up: _gasPressed = true; break;
                case Key.Down: _brakePressed = true; break;
                case Key.Left: _steer = -1f; break;
                case Key.Right: _steer = 1f; break;
                case Key.Space: _handbrakePressed = true; break;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up: _gasPressed = false; break;
                case Key.Down: _brakePressed = false; break;
                case Key.Left: if (_steer < 0) _steer = 0; break;
                case Key.Right: if (_steer > 0) _steer = 0; break;
                case Key.Space: _handbrakePressed = false; break;
            }
        }*/
    }
}
