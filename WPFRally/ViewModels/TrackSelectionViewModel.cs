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
        private readonly Car _selectedCar;
        public List<Track> Tracks { get; private set; }
        public ICommand SelectTrackCommand { get; }
        public ICommand BackCommand { get; }

        public TrackSelectionViewModel(MainWindow mainWindow, Car selectedCar)
        {
            _mainWindow = mainWindow;
            _selectedCar = selectedCar;
            var dataService = new JsonDataService();
            Tracks = dataService.LoadTracks();
            SelectTrackCommand = new RelayCommand(track => _mainWindow.StartRace(track as Track));
            BackCommand = new RelayCommand(o => _mainWindow.ShowCarSelection());
        }
    }
}
