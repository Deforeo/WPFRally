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
using System.Windows.Navigation;
using System.Windows.Shapes;
using SkiaSharp;
using WPFRally.Models;
using WPFRally.Infrastructure;


namespace WPFRally
{
    public partial class MainWindow : Window
    {
        private GameWorld _world;
        private Camera _camera;
        private DateTime _lastUpdate;
        private CarInput _carInput;

        // Управление
        private bool _gasPressed;
        private bool _brakePressed;
        private float _steer; // -1..+1

        public MainWindow()
        {
            InitializeComponent();
            this.Focusable = true;

            _world = new GameWorld();
            _world.WorldWidth = 3000f;   // можно задать явно
            _world.WorldHeight = 4000f;

            _camera = new Camera();
            _camera.Zoom = 1.0f;

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _lastUpdate = DateTime.Now;
            CompositionTarget.Rendering += OnRendering;
            _world.OnRaceFinished += (time) =>
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Гонка завершена! Время: {time:F2} секунд");
                    // Здесь позже будет переход на финишное меню с рекордами
                });
            };
        }

        private void OnRendering(object sender, EventArgs e)
        {
            // Защита от null (на всякий случай)
            if (_world == null) return;

            var now = DateTime.Now;
            float deltaTime = (float)(now - _lastUpdate).TotalSeconds;
            if (deltaTime > 0.05f) deltaTime = 0.05f;
            _lastUpdate = now;

            float throttle = _gasPressed ? 1f : 0f;
            float brake = _brakePressed ? 1f : 0f;
            _world.Update(deltaTime, _carInput);

            float viewportWidth = (float)skiaElement.ActualWidth;
            float viewportHeight = (float)skiaElement.ActualHeight;
            _camera.Follow(_world.Player.Position, viewportWidth, viewportHeight, _world.WorldWidth, _world.WorldHeight);

            skiaElement.InvalidateVisual();
        }

        private void OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            if (_world == null) return;

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.DarkGreen);

            float viewportWidth = (float)skiaElement.ActualWidth;
            float viewportHeight = (float)skiaElement.ActualHeight;

            // Рисуем препятствия
            foreach (var obs in _world.Obstacles)
            {
                var screenRect = new SKRect(
                    (obs.Rect.Left - _camera.Offset.X) * _camera.Zoom,
                    (obs.Rect.Top - _camera.Offset.Y) * _camera.Zoom,
                    (obs.Rect.Right - _camera.Offset.X) * _camera.Zoom,
                    (obs.Rect.Bottom - _camera.Offset.Y) * _camera.Zoom
                );
                using (var paint = new SKPaint { Color = obs.Color, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(screenRect, paint);
                }
                // Обводка
                using (var paint = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 2 })
                {
                    canvas.DrawRect(screenRect, paint);
                }
            }

            foreach (var zone in _world.TriggerZones)
            {
                var screenRect = new SKRect(
                    (zone.Rect.Left - _camera.Offset.X) * _camera.Zoom,
                    (zone.Rect.Top - _camera.Offset.Y) * _camera.Zoom,
                    (zone.Rect.Right - _camera.Offset.X) * _camera.Zoom,
                    (zone.Rect.Bottom - _camera.Offset.Y) * _camera.Zoom
                );
                SKColor zoneColor = zone.Type == "Start" ? SKColors.Green : SKColors.Red;
                using (var paint = new SKPaint { Color = zoneColor, Style = SKPaintStyle.Stroke, StrokeWidth = 3 })
                {
                    canvas.DrawRect(screenRect, paint);
                }
                // Полупрозрачная заливка
                using (var paint = new SKPaint { Color = zoneColor.WithAlpha(80), Style = SKPaintStyle.Fill })
                {
                    canvas.DrawRect(screenRect, paint);
                }

            }

            string timeText = _world.IsRaceActive ? $"Time: {_world.RaceTime:F2}s" :
                  (_world.IsFinished ? $"Finished! {_world.RaceTime:F2}s" : "Not started");
            using (var font = new SKFont(SKTypeface.FromFamilyName("Arial"), 24f))
            using (var paint = new SKPaint { Color = SKColors.White })
            {
                canvas.DrawText(timeText, 20, 40, SKTextAlign.Left, font, paint);
            }

            var worldRect = new SKRect(0, 0, _world.WorldWidth, _world.WorldHeight);
            var screenWorldRect = new SKRect(
                (worldRect.Left - _camera.Offset.X) * _camera.Zoom,
                (worldRect.Top - _camera.Offset.Y) * _camera.Zoom,
                (worldRect.Right - _camera.Offset.X) * _camera.Zoom,
                (worldRect.Bottom - _camera.Offset.Y) * _camera.Zoom
            );
            using (var paint = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = 3 })
            {
                canvas.DrawRect(screenWorldRect, paint);
            }

            // Рисуем машинку (прямоугольник с поворотом)
            var carPos = _world.Player.Position;
            var screenCarPos = new SKPoint(
                (carPos.X - _camera.Offset.X) * _camera.Zoom,
                (carPos.Y - _camera.Offset.Y) * _camera.Zoom
            );

            canvas.Save();
            canvas.Translate(screenCarPos.X, screenCarPos.Y);
            canvas.RotateRadians(_world.Player.Angle);
            float w = _world.Player.Width * _camera.Zoom;
            float h = _world.Player.Height * _camera.Zoom;
            var rect = new SKRect(-w / 2, -h / 2, w / 2, h / 2);
            using (var paint = new SKPaint { Color = SKColors.Red, Style = SKPaintStyle.Fill })
            {
                canvas.DrawRect(rect, paint);
            }
            using (var paint = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke, StrokeWidth = 2 })
            {
                canvas.DrawRect(rect, paint);
            }
            canvas.Restore();
        }

        // Управление с клавиатуры
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up: _carInput.throttle = 1f; break;
                case Key.Down: _carInput.brake = 1f; break;
                case Key.Left: _carInput.turn = -1f; break;
                case Key.Right: _carInput.turn = 1f; break;
                case Key.Space: _carInput.handbrake = 1f; break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up: _carInput.throttle = 0f; break;
                case Key.Down: _carInput.brake = 0f; break;
                case Key.Left: if (_carInput.turn < 0) _carInput.turn = 0f; break;
                case Key.Right: if (_carInput.turn > 0) _carInput.turn = 0f; break;
                case Key.Space: _carInput.handbrake = 0f; break;
            }
        }
    }
}