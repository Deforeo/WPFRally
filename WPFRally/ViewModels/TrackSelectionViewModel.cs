using System;
using System.Collections.Generic;
using System.Windows.Input;
using WPFRally.Models;
using WPFRally.Services;

namespace WPFRally.ViewModels
{
    public class TrackSelectionViewModel
    {
        private readonly MainWindow _mainWindow;
        public List<Track> Tracks { get; private set; }
        public ICommand SelectTrackCommand { get; }  // <- именно так
        public ICommand BackCommand { get; }

        public TrackSelectionViewModel(MainWindow mainWindow, Car selectedCar)
        {
            _mainWindow = mainWindow;
            var dataService = new JsonDataService();
            Tracks = dataService.LoadTracks();
            SelectTrackCommand = new RelayCommand(track => _mainWindow.StartRace(track as Track));
            BackCommand = new RelayCommand(o => _mainWindow.ShowCarSelection());
        }
    }
}
