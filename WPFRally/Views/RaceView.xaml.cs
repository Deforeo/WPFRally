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
        public bool IsPaused { get; private set; }
        private PauseOverlay _pauseOverlay;


        // --- Управление ---
        private bool _gasPressed;
        private bool _brakePressed;
        private bool _handbrakePressed;
        private float _steer;

        private SKTypeface _hudTypeface;
        private SKFont _hudFont;
        private bool _fontLoaded = false;

        private bool _isPaused = false;
        private MainWindow _mainWindow;
        private Car _currentCar;
        private Track _currentTrack;

        // --- Таймер ---
        private DateTime _lastUpdate;
        private SKBitmap _cachedBackground;

        // Событие для уведомления MainWindow о финише
        public event Action<float, Track> RaceFinished;

        public RaceView(MainWindow mainWindow, Car selectedCar, Track selectedTrack)
        {
            _mainWindow = mainWindow;
            _currentCar = selectedCar;
            _currentTrack = selectedTrack;
            InitializeComponent();
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            CompositionTarget.Rendering -= OnRendering;
            _pauseOverlay = new PauseOverlay();
            _pauseOverlay.Resume += () => { Resume(); };
            _pauseOverlay.Restart += () => { Restart(); };
            _pauseOverlay.ExitToMenu += () => { _mainWindow.ShowMenu(); };
            PauseOverlayContainer.Content = _pauseOverlay;
            PauseOverlayContainer.Visibility = Visibility.Visible;
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            CompositionTarget.Rendering += OnRendering;
            PauseOverlayContainer.Visibility = Visibility.Collapsed;
            _pauseOverlay = null;
        }

        public void Restart()
        {
            // Перезапускаем гонку через MainWindow (создаётся новый экземпляр RaceView)
            _mainWindow.StartRace(_currentTrack);
        }

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _lastUpdate = DateTime.Now;
            LoadHudFont();
            CompositionTarget.Rendering += OnRendering;
            this.Focus(); // чтобы клавиши работали сразу
        }

        private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= OnRendering;
            _cachedBackground?.Dispose();
            _cachedBackground = null;
            _hudFont?.Dispose();
            _hudTypeface?.Dispose();
        }
        private void LoadHudFont()
        {
            if (_fontLoaded) return;
            try
            {
                // Формируем надежный абсолютный путь к файлу
                string fontPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Font", "Serpentin DG Bold Italic.otf");

                _hudTypeface = SKTypeface.FromFile(fontPath);

                // Если файл не найден, SKTypeface.FromFile может вернуть null. Делаем проверку:
                if (_hudTypeface == null)
                {
                    _hudTypeface = SKTypeface.CreateDefault(); // Fallback на стандартный шрифт
                }

                _hudFont = new SKFont(_hudTypeface, 28f);
                _fontLoaded = true;
            }
            catch
            {
                // В случае ошибки (например, файл заблокирован), используем стандартный
                _hudTypeface = SKTypeface.CreateDefault();
                _hudFont = new SKFont(_hudTypeface, 28f);
                _fontLoaded = true;
            }
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

            // Камера
            Camera.Follow(World.Player.ToSKPoint(), viewW, viewH, World.WorldWidth, World.WorldHeight);

            SKRect visibleWorld = Camera.GetVisibleWorldRect(viewW, viewH);
            // Фон
            if (SelectedTrack != null && !string.IsNullOrEmpty(SelectedTrack.BackgroundSpritePath))
            {
                var bgSprite = SpriteManager.GetSprite(SelectedTrack.BackgroundSpritePath);
                if (bgSprite != null)
                {
                    // Растягиваем фон на весь мир (от 0,0 до WorldWidth, WorldHeight)
                    SKRect worldRect = new SKRect(0, 0, World.WorldWidth, World.WorldHeight);
                    SKRect screenRect = new SKRect(
                        (worldRect.Left - Camera.Offset.X) * Camera.Zoom,
                        (worldRect.Top - Camera.Offset.Y) * Camera.Zoom,
                        (worldRect.Right - Camera.Offset.X) * Camera.Zoom,
                        (worldRect.Bottom - Camera.Offset.Y) * Camera.Zoom
                    );
                    canvas.DrawBitmap(bgSprite, screenRect);
                }
                else
                    canvas.Clear(SKColors.DarkGreen);
            }
            else
                canvas.Clear(SKColors.DarkGreen);

            // Препятствия
            foreach (var obs in World.Obstacles)
            {
                if (!obs.IsVisible) continue; // не рисуем, но коллизия есть
                if (!obs.Rect.IntersectsWith(visibleWorld)) continue;

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

            // Отрисовка контрольных точек
            foreach (var cp in World.Checkpoints)
            {
                SKRect screenRect = new SKRect(
                    (cp.Rect.Left - Camera.Offset.X) * Camera.Zoom,
                    (cp.Rect.Top - Camera.Offset.Y) * Camera.Zoom,
                    (cp.Rect.Right - Camera.Offset.X) * Camera.Zoom,
                    (cp.Rect.Bottom - Camera.Offset.Y) * Camera.Zoom
                );
                SKColor fillColor = cp.IsPassed ? SKColors.DarkBlue : SKColors.Blue;
                using (var fill = new SKPaint { Color = fillColor.WithAlpha(100), Style = SKPaintStyle.Fill })
                    canvas.DrawRect(screenRect, fill);
                using (var stroke = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Stroke, StrokeWidth = 2 })
                    canvas.DrawRect(screenRect, stroke);
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
            canvas.RotateRadians(World.Player.Angle/* + (float)(Math.PI / 2)*/);

            SKBitmap carSprite = null;
            if (SelectedCar != null && !string.IsNullOrEmpty(SelectedCar.SpritePath))
                carSprite = SpriteManager.GetSprite(SelectedCar.SpritePath);

            if (carSprite != null)
            {
                float targetW = World.Player.Width * Camera.Zoom;
                float targetH = World.Player.Height * Camera.Zoom;
                float spriteW = carSprite.Width;
                float spriteH = carSprite.Height;

                // Вычисляем масштаб, чтобы спрайт вписался в целевой прямоугольник без искажений
                float scale = Math.Min(targetW / spriteW, targetH / spriteH);
                float drawW = spriteW * scale;
                float drawH = spriteH * scale;

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

            // HUD – таймер и чекпоинты с красивым фоном
            if (!_fontLoaded) LoadHudFont();

            string timeText = World.IsRaceActive ? $"Time: {World.RaceTime:F2}s" :
                    (World.IsFinished ? $"Finished! {World.RaceTime:F2}s" : "Not started");
            string cpText = $"Checkpoints: {World.NextCheckpointIndex}/{World.Checkpoints.Count}";

            // УДАЛИТЕ ЭТИ ДВЕ СТРОКИ:
            // SKTypeface typeface = SKTypeface.FromFile("Assets/Font/Most Wazted(RUS BY LYAJKA).otf");
            // SKFont font = new SKFont(typeface, 28f);

            // Используем кэшированный _hudFont:
            float timeWidth = _hudFont.MeasureText(timeText);
            float timeHeight = _hudFont.Size;
            float cpWidth = _hudFont.MeasureText(cpText);
            float cpHeight = _hudFont.Size;

            float padding = 15;
            float cornerRadius = 10;
            float panelX = 20;
            float panelY = 20;

            using (var bgPaint = new SKPaint { Color = SKColors.Black.WithAlpha(180), Style = SKPaintStyle.Fill })
            {
                // Фон для времени
                SKRect timeBgRect = new SKRect(panelX, panelY, panelX + timeWidth + padding * 2, panelY + timeHeight + padding);
                canvas.DrawRoundRect(timeBgRect, cornerRadius, cornerRadius, bgPaint);
                // Фон для чекпоинтов
                float cpTop = panelY + timeHeight + padding + 10;
                SKRect cpBgRect = new SKRect(panelX, cpTop, panelX + cpWidth + padding * 2, cpTop + cpHeight + padding);
                canvas.DrawRoundRect(cpBgRect, cornerRadius, cornerRadius, bgPaint);
            }

            using (var textPaint = new SKPaint { Color = World.IsFinished ? SKColors.Yellow : SKColors.White, IsAntialias = true })
            {
                // Передаем _hudFont вместо font
                canvas.DrawText(timeText, panelX + padding, panelY + _hudFont.Size - 5, SKTextAlign.Left, _hudFont, textPaint);
                float cpY = panelY + timeHeight + padding + 10 + _hudFont.Size - 5;
                canvas.DrawText(cpText, panelX + padding, cpY, SKTextAlign.Left, _hudFont, textPaint);
            }
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
