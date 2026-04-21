using System;
using System.Collections.Generic;
using System.Windows.Input;
using WPFRally.Models;
using WPFRally.Services;

namespace WPFRally.ViewModels
{
    public class RecordDisplay
    {
        public string TrackName { get; set; }
        public float TimeSeconds { get; set; }
        public string TimeText => $"{TimeSeconds:F2}";
        public string Initials { get; set; }
        public string DateText { get; set; }
    }

    public class RecordsViewModel
    {
        private readonly MainWindow _mainWindow;
        public List<RecordDisplay> Records { get; private set; }
        public ICommand BackCommand { get; }

        public RecordsViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            LoadRecords();
            BackCommand = new RelayCommand(o => _mainWindow.ShowMenu());
        }

        private void LoadRecords()
        {
            var dataService = new JsonDataService();
            var records = dataService.LoadRecords();
            var tracks = dataService.LoadTracks();
            var trackDict = new Dictionary<int, string>();
            foreach (var t in tracks) trackDict[t.Id] = t.Name;

            Records = new List<RecordDisplay>();
            foreach (var r in records)
            {
                Records.Add(new RecordDisplay
                {
                    TrackName = trackDict.ContainsKey(r.TrackId) ? trackDict[r.TrackId] : $"Трасса {r.TrackId}",
                    TimeSeconds = r.TimeSeconds,
                    Initials = r.Initials,
                    DateText = r.Date.ToString("dd.MM.yyyy HH:mm")
                });
            }
        }
    }
}
