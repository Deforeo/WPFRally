using System;
using System.Windows;
using System.Windows.Input;
using WPFRally.Models;
using WPFRally.Services;
using WPFRally.Views;

namespace WPFRally.ViewModels
{
    public class FinishViewModel
    {
        private readonly MainWindow _mainWindow;
        private readonly float _raceTime;
        private readonly Track _track;

        public string TimeText => $"{_raceTime:F2} sec";
        public string BestRecordText { get; private set; }

        public ICommand SaveRecordCommand { get; }
        public ICommand BackToMenuCommand { get; }

        public FinishViewModel(MainWindow mainWindow, float raceTime, Track track)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            _raceTime = raceTime;
            _track = track ?? throw new ArgumentNullException(nameof(track));

            // Загружаем лучший рекорд для этой трассы
            try
            {
                var dataService = new JsonDataService();
                var records = dataService.LoadRecords();
                var trackRecords = records.FindAll(r => r.TrackId == _track.Id);
                float best = float.MaxValue;
                foreach (var r in trackRecords)
                    if (r.TimeSeconds < best) best = r.TimeSeconds;
                BestRecordText = (best != float.MaxValue) ? $"🏆 Рекорд: {best:F2} сек" : "Нет рекордов";
            }
            catch (Exception ex)
            {
                BestRecordText = "Ошибка загрузки рекордов";
                System.Diagnostics.Debug.WriteLine($"Ошибка в FinishViewModel: {ex.Message}");
            }

            SaveRecordCommand = new RelayCommand(o => SaveRecord());
            BackToMenuCommand = new RelayCommand(o => _mainWindow.ShowMenu());
        }

        private void SaveRecord()
        {
            try
            {
                // 1. Проверка, что все данные есть
                if (_track == null)
                {
                    MessageBox.Show("Ошибка: трасса не определена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _mainWindow?.ShowMenu();
                    return;
                }

                if (_mainWindow == null)
                {
                    MessageBox.Show("Ошибка: главное окно не доступно!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Затемняем главное окно
                var mainWindow = Application.Current.MainWindow;
                mainWindow.Opacity = 0.6;

                

                // 2. Диалог ввода трёх букв (временное решение – позже замените на кастомное окно)
                var dialog = new RecordNameDialog();
                dialog.ShowDialog();
                if (!dialog.IsSaved)
                {
                    _mainWindow.ShowMenu();
                    return;
                }
                string initials = dialog.Initials;
                // Восстанавливаем прозрачность
                mainWindow.Opacity = 1;
                // 3. Сохраняем рекорд
                var dataService = new JsonDataService();
                var records = dataService.LoadRecords();
                records.Add(new Record
                {
                    TrackId = _track.Id,
                    TimeSeconds = _raceTime,
                    Initials = initials.ToUpper(),
                    Date = DateTime.Now
                });
                dataService.SaveRecords(records);

                MessageBox.Show($"Рекорд сохранён! {initials} - {_raceTime:F2} сек", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения рекорда: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Diagnostics.Debug.WriteLine($"SaveRecord error: {ex}");
            }
            finally
            {
                // Возвращаемся в меню (даже если была ошибка)
                _mainWindow?.ShowMenu();
            }
        }
    }
}