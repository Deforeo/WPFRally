using System;
using System.Windows.Input;
using System.Windows;
using WPFRally.Models;
using WPFRally.Services;

namespace WPFRally.ViewModels
{
    public class FinishViewModel
    {
        private readonly MainWindow _mainWindow;
        private readonly float _raceTime;
        private readonly Track _track;

        public string TimeText => $"{_raceTime:F2} сек";
        public string BestRecordText { get; private set; }

        public ICommand SaveRecordCommand { get; }
        public ICommand RestartCommand { get; }
        public ICommand BackToMenuCommand { get; }

        public event Action RequestRecordDialog; // событие для View

        public FinishViewModel(MainWindow mainWindow, float raceTime, Track track)
        {
            _mainWindow = mainWindow;
            _raceTime = raceTime;
            _track = track;

            // Загружаем рекорды
            var dataService = new JsonDataService();
            var records = dataService.LoadRecords();
            var trackRecords = records.FindAll(r => r.TrackId == _track.Id);
            float best = float.MaxValue;
            foreach (var r in trackRecords)
                if (r.TimeSeconds < best) best = r.TimeSeconds;
            BestRecordText = (best != float.MaxValue) ? $"🏆 Рекорд: {best:F2} сек" : "Нет рекордов";

            SaveRecordCommand = new RelayCommand(o => RequestRecordDialog?.Invoke());
            RestartCommand = new RelayCommand(o => _mainWindow.StartRace(_track));
            BackToMenuCommand = new RelayCommand(o => _mainWindow.ShowMenu());
        }

        public void SaveRecord(string initials)
        {
            if (string.IsNullOrEmpty(initials)) return;

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
        }
    }
}