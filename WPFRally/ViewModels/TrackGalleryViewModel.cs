using System;
using System.Collections.Generic;
using System.Windows.Input;
using WPFRally.Models;
using WPFRally.Services;

namespace WPFRally.ViewModels
{
    public class TrackGalleryViewModel
    {
        private readonly MainWindow _mainWindow;
        public List<Track> Tracks { get; private set; }
        public ICommand BackCommand { get; }

        public TrackGalleryViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            var dataService = new JsonDataService();
            Tracks = dataService.LoadTracks();
            BackCommand = new RelayCommand(o => _mainWindow.ShowMenu());
        }
    }
}
