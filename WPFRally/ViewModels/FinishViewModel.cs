using System;
using System.Windows.Input;
using WPFRally.Models;
using WPFRally.Services;

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
            _mainWindow = mainWindow;
            _raceTime = raceTime;
            _track = track;

            var dataService = new JsonDataService();
            var records = dataService.LoadRecords();
            var trackRecords = records.FindAll(r => r.TrackId == track.Id);
            float best = float.MaxValue;
            foreach (var r in trackRecords)
                if (r.TimeSeconds < best) best = r.TimeSeconds;
            BestRecordText = (best != float.MaxValue) ? $"Рекорд: {best:F2} сек" : "Нет рекордов";

            SaveRecordCommand = new RelayCommand(o => SaveRecord());
            BackToMenuCommand = new RelayCommand(o => _mainWindow.ShowMenu());
        }

        private void SaveRecord()
        {
            string initials = "AAA"; // позже добавить диалог
            var dataService = new JsonDataService();
            var records = dataService.LoadRecords();
            records.Add(new Record { TrackId = _track.Id, TimeSeconds = _raceTime, Initials = initials, Date = System.DateTime.Now });
            dataService.SaveRecords(records);
            _mainWindow.ShowMenu();
        }
    }
}