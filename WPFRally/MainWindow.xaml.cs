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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPFRally
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private object _currentMenuView;
        public object CurrentMenuView
        {
            get => _currentMenuView;
            set { _currentMenuView = value; OnPropertyChanged(); }
        }

        private GameWorld _world;
        private Camera _camera;
        private DateTime _lastUpdate;
        private RaceView _currentRaceView;

        // Управление
        private bool _gasPressed;
        private bool _brakePressed;
        private bool _handbrakePressed;
        private float _steer;

        // ---------- Добавленные поля для навигации ----------
        private Car _selectedCar;
        private Track _selectedTrack;
        private bool _isPaused = false;

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

        }

        private void OnRendering(object sender, EventArgs e)
        {
            if (_world == null) return;

            // Вычисляем deltaTime
            var now = DateTime.Now;
            float deltaTime = (float)(now - _lastUpdate).TotalSeconds;
            if (deltaTime > 0.033f) deltaTime = 0.033f;
            _lastUpdate = now;

            float throttle = _gasPressed ? 1f : 0f;
            float brake = _brakePressed ? 1f : 0f;
            float handbrake = _handbrakePressed ? 1f : 0f;

            _world.Update(deltaTime, throttle, brake, handbrake, _steer);

            // Обновляем камеру (но можно и в RaceView, но лучше здесь)
            if (CurrentMenuView is RaceView raceView && raceView.Camera != null)
            {
                float viewW = (float)raceView.ActualWidth;
                float viewH = (float)raceView.ActualHeight;
                if (viewW > 0 && viewH > 0)
                    raceView.Camera.Follow(_world.Player.ToSKPoint(), viewW, viewH, _world.WorldWidth, _world.WorldHeight);
            }
        }

        // Управление с клавиатуры
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                TogglePause();
                e.Handled = true;
                return;
            }
            if (e.Key == Key.R && !_isPaused)
            {
                RestartRace();
                e.Handled = true;
                return;
            }
            switch (e.Key)
            {
                case Key.Up: _gasPressed = true; break;
                case Key.Down: _brakePressed = true; break;
                case Key.Left: _steer = -1f; break;
                case Key.Right: _steer = 1f; break;
                case Key.Space: _handbrakePressed = true; break;
            }

            if (e.Key == Key.F11)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                ResizeMode = ResizeMode.CanResize;
                e.Handled = true;
            }
            base.OnKeyDown(e);
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

        private void TogglePause()
        {
            if (_currentRaceView == null) return;
            if (_currentRaceView.IsPaused)
                _currentRaceView.Resume();
            else
                _currentRaceView.Pause();
        }

        private void RestartRace()
        {
            if (_currentRaceView != null)
                _currentRaceView.Restart();
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

        public void ShowCarGallery()
        {
            var vm = new CarGalleryViewModel(this);
            var view = new CarGalleryView(vm);
            CurrentMenuView = view;
        }

        public void ShowTrackGallery()
        {
            var vm = new TrackGalleryViewModel(this);
            var view = new TrackGalleryView(vm);
            CurrentMenuView = view;
        }

        public void ShowRecords()
        {
            var vm = new RecordsViewModel(this);
            var view = new RecordsView(vm);
            CurrentMenuView = view;
        }


        public void SetSelectedCar(Car car)
        {
            _selectedCar = car;
        }

        /// <summary>
        /// Показывает окно выбора трассы.
        /// Если автомобиль не выбран, сначала показываем выбор авто.
        /// </summary>
        public void ShowTrackSelection()
        {
            try
            {

                // 1. Проверка наличия трасс
                var dataService = new JsonDataService();
                var tracks = dataService.LoadTracks();

                if (tracks == null || tracks.Count == 0)
                {
                    System.Windows.MessageBox.Show("Нет доступных трасс!");
                    ShowMenu();
                    return;
                }

                // 2. Проверка выбранного автомобиля
                if (_selectedCar == null)
                {
                    ShowCarSelection();
                    return;
                }


                // 3. Создание ViewModel и View
                var vm = new TrackSelectionViewModel(this, _selectedCar);
                var view = new TrackSelectionView { DataContext = vm };
                CurrentMenuView = view;

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка в ShowTrackSelection: {ex.Message}\n{ex.StackTrace}");
            }
        }
        public void ShowCarSelection()
        {
            try
            {
                var vm = new CarSelectionViewModel(this);
                var view = new CarSelectionView { DataContext = vm };
                CurrentMenuView = view;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в ShowCarSelection: {ex.Message}\n{ex.StackTrace}");
            }
        }
        /// <summary>
        /// Запускает гонку на выбранной трассе.
        /// </summary>
        public void StartRace(Track selectedTrack)
        {
            try
            {
                if (selectedTrack == null) { ShowMenu(); return; }
                if (_selectedCar == null) { ShowCarSelection(); return; }

                _selectedTrack = selectedTrack;

                _world.Player.ApplyCarParameters(_selectedCar);

                // Настройка мира (ваш существующий код)
                _world.WorldWidth = selectedTrack.WorldWidth;
                _world.WorldHeight = selectedTrack.WorldHeight;
                _world.Player.Position = new Vector2(selectedTrack.StartPosition.X - 128, selectedTrack.StartPosition.Y + 64);
                _world.Player.Angle = 0;
                _world.Player.Velocity = new Vector2(0, 0);
                _world.IsRaceActive = false;
                _world.IsFinished = false;
                _world.RaceTime = 0;

                _world.Obstacles.Clear();
                foreach (var obsData in selectedTrack.Obstacles)
                {
                    _world.Obstacles.Add(new Obstacle(
                        obsData.X, obsData.Y, obsData.Width, obsData.Height,
                        obsData.SpritePath, obsData.IsVisible
                    ));
                }

                _world.TriggerZones.Clear();
                _world.TriggerZones.Add(new TriggerZone(selectedTrack.StartPosition.X, selectedTrack.StartPosition.Y, 64, 128, "Start"));
                _world.TriggerZones.Add(new TriggerZone(selectedTrack.FinishPosition.X, selectedTrack.FinishPosition.Y, 128, 64, "Finish"));

                _world.Checkpoints.Clear();
                if (selectedTrack.Checkpoints != null)
                {
                    foreach (var cpData in selectedTrack.Checkpoints)
                        _world.Checkpoints.Add(new Checkpoint(cpData.X, cpData.Y, cpData.Width, cpData.Height, cpData.Index));
                }
                _world.NextCheckpointIndex = 0;

                _world.OnRaceFinished -= OnRaceFinishedHandler;
                _world.OnRaceFinished += OnRaceFinishedHandler;

                // Создаём RaceView и передаём мир и камеру
                var raceView = new RaceView(this, _selectedCar, _selectedTrack);
                
                raceView.World = _world;

                raceView.World.OnStartLineCrossed += () =>
                {
                    raceView.ShowCountdown();
                };
                raceView.Camera = _camera;
                raceView.SelectedCar = _selectedCar;
                raceView.SelectedTrack = selectedTrack;

                // Сохраняем ссылку для рестарта
                _currentRaceView = raceView;

                // Показываем RaceView в ContentControl
                CurrentMenuView = raceView;
                raceView.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в StartRace: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработчик события финиша гонки.
        /// </summary>
        private void OnRaceFinishedHandler(float raceTime)
        {
            Dispatcher.Invoke(() =>
            {
                if (_selectedTrack == null)
                {
                    MessageBox.Show("Ошибка: трасса не выбрана!");
                    ShowMenu();
                    return;
                }
                ShowFinish(raceTime);
            });
        }

        /// <summary>
        /// Показывает окно финиша с результатом.
        /// </summary>
        public void ShowFinish(float raceTime)
        {
            var vm = new FinishViewModel(this, raceTime, _selectedTrack);
            var view = new FinishView();
            view.DataContext = vm;

            // Подписываемся на событие показа диалога
            vm.RequestRecordDialog += () => view.ShowRecordDialog();

            // Подписываемся на событие сохранения рекорда
            view.RecordSaved += (initials) =>
            {
                if (!string.IsNullOrEmpty(initials))
                    vm.SaveRecord(initials);
                ShowMenu();
            };

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