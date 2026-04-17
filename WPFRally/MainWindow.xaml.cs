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
using System.Text.Json;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using WPFRally.Models;
using WPFRally.Infrastructure;
using WPFRally.ViewModels;
using WPFRally.Views;
using WPFRally.Services;


namespace WPFRally
{
    public partial class MainWindow : Window
    {
        private GameWorld _world;
        private Camera _camera;
        private DateTime _lastUpdate;

        // Управление
        private bool _gasPressed;
        private bool _brakePressed;
        private bool _handbrakePressed;
        private float _steer;

        // ---------- Добавленные поля для навигации ----------
        private Car _selectedCar;
        private Track _selectedTrack;
        private object _currentMenuView;

        public object CurrentMenuView
        {
            get => _currentMenuView;
            set { _currentMenuView = value; }
        }


        public MainWindow()
        {
            InitializeComponent();
            this.Focusable = true;
            DataContext = this; // для привязки CurrentMenuView

            // Инициализация игры (ваш код)
            _world = new GameWorld();
            _camera = new Camera();
            _camera.Zoom = 1.0f;
            this.Loaded += OnLoaded;

            // Показываем главное меню
            ShowMenu();
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
            if (_world == null) return;

            var now = DateTime.Now;
            float deltaTime = (float)(now - _lastUpdate).TotalSeconds;
            if (deltaTime > 0.033f) deltaTime = 0.033f;
            _lastUpdate = now;

            // Получаем управление
            float throttle = _gasPressed ? 1f : 0f;
            float brake = _brakePressed ? 1f : 0f;
            float handbrake = _handbrakePressed ? 1f : 0f;

            _world.Update(deltaTime, throttle, brake, handbrake, _steer);

            // Получаем размеры элемента SKElement
            float viewW = (float)skiaElement.ActualWidth;
            float viewH = (float)skiaElement.ActualHeight;

            // Обновляем камеру
            _camera.Follow(_world.Player.ToSKPoint(), viewW, viewH, _world.WorldWidth, _world.WorldHeight);

            // Запрашиваем перерисовку
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
            SKPoint carPos = _world.Player.ToSKPoint();

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
                case Key.Up: _gasPressed = true; break;
                case Key.Down: _brakePressed = true; break;
                case Key.Left: _steer = -1f; break;
                case Key.Right: _steer = 1f; break;
                case Key.Space: _handbrakePressed = true; break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up: _gasPressed = false; break;
                case Key.Down: _brakePressed = false; break;
                case Key.Left: if (_steer < 0) _steer = 0f; break;
                case Key.Right: if (_steer > 0) _steer = 0f; break;
                case Key.Space: _handbrakePressed = false; break;
            }
        }

        // ---------- НАВИГАЦИОННЫЕ МЕТОДЫ ----------

        /// <summary>
        /// Показывает главное меню.
        /// </summary>
        public void ShowMenu()
        {
            GameGrid.Visibility = Visibility.Collapsed;
            var vm = new MenuViewModel(this);
            var view = new MenuView { DataContext = vm };
            CurrentMenuView = view;
        }

        /// <summary>
        /// Показывает окно выбора автомобиля.
        /// </summary>
        public void ShowCarSelection()
        {
            var vm = new CarSelectionViewModel(this);
            var view = new CarSelectionView { DataContext = vm };
            CurrentMenuView = view;
        }

        /// <summary>
        /// Показывает окно выбора трассы.
        /// </summary>
        /// <param name="selectedCar">Выбранный автомобиль (может быть null, тогда загрузится первый из списка).</param>
        public void ShowTrackSelection(Car selectedCar)
        {
            if (selectedCar == null)
            {
                // Если автомобиль не передан, загружаем первый из списка
                var dataService = new JsonDataService();
                var cars = dataService.LoadCars();
                if (cars.Count > 0)
                    selectedCar = cars[0];
                else
                {
                    MessageBox.Show("Нет доступных автомобилей!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ShowCarSelection();
                    return;
                }
            }
            _selectedCar = selectedCar;
            var vm = new TrackSelectionViewModel(this, selectedCar);
            var view = new TrackSelectionView { DataContext = vm };
            CurrentMenuView = view;
        }

        /// <summary>
        /// Запускает гонку на выбранной трассе.
        /// </summary>
        /// <param name="selectedTrack">Выбранная трасса.</param>
        public void StartRace(Track selectedTrack)
        {
            if (selectedTrack == null)
            {
                MessageBox.Show("Трасса не выбрана!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                ShowMenu();
                return;
            }

            _selectedTrack = selectedTrack;

            // Применяем параметры трассы к игровому миру
            _world.WorldWidth = selectedTrack.WorldWidth;
            _world.WorldHeight = selectedTrack.WorldHeight;
            _world.Player.Position = new Vector2(selectedTrack.StartPosition.X, selectedTrack.StartPosition.Y);
            _world.Player.Angle = 0;
            _world.Player.Velocity = new Vector2(0, 0);
            _world.IsRaceActive = false;
            _world.IsFinished = false;
            _world.RaceTime = 0;

            // Очищаем старые зоны и добавляем новые
            _world.TriggerZones.Clear();
            // Стартовая зона (чтобы начать отсчёт времени)
            _world.TriggerZones.Add(new TriggerZone(
                selectedTrack.StartPosition.X - 30,
                selectedTrack.StartPosition.Y - 30,
                60, 60, "Start"));
            // Финишная зона
            _world.TriggerZones.Add(new TriggerZone(
                selectedTrack.FinishPosition.X - 40,
                selectedTrack.FinishPosition.Y - 40,
                80, 80, "Finish"));

            // Подписываемся на событие финиша (отписываемся сначала, чтобы избежать дублей)
            _world.OnRaceFinished -= OnRaceFinishedHandler;
            _world.OnRaceFinished += OnRaceFinishedHandler;

            // Показываем игровое поле и даём ему фокус для управления
            GameGrid.Visibility = Visibility.Visible;
            this.Focus();
        }

        /// <summary>
        /// Обработчик события финиша гонки.
        /// </summary>
        /// <param name="raceTime">Время гонки в секундах.</param>
        private void OnRaceFinishedHandler(float raceTime)
        {
            // Вызываем показ финишного окна в потоке UI (Dispatcher)
            Dispatcher.Invoke(() => ShowFinish(raceTime));
        }

        /// <summary>
        /// Показывает окно финиша с результатом.
        /// </summary>
        /// <param name="raceTime">Время гонки.</param>
        public void ShowFinish(float raceTime)
        {
            GameGrid.Visibility = Visibility.Collapsed;
            var vm = new FinishViewModel(this, raceTime, _selectedTrack);
            var view = new FinishView { DataContext = vm };
            CurrentMenuView = view;
        }

        /// <summary>
        /// Выход из приложения.
        /// </summary>
        public void ExitGame()
        {
            Application.Current.Shutdown();
        }


    }
}