using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFRally.ViewModels
{
    public class MenuViewModel
    {
        private readonly MainWindow _mainWindow;
        public ICommand PlayCommand { get; }
        public ICommand CarsCommand { get; }
        public ICommand TracksCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand RecordsCommand { get; }
        public ICommand ShowCarsCommand { get; }
        public ICommand ShowTracksCommand { get; }


        public MenuViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            PlayCommand = new RelayCommand(o => _mainWindow.ShowCarSelection());
            CarsCommand = new RelayCommand(o => _mainWindow.ShowCarSelection());
            TracksCommand = new RelayCommand(o => _mainWindow.ShowTrackSelection()); // без параметра
            ExitCommand = new RelayCommand(o => _mainWindow.ExitGame());
            ShowCarsCommand = new RelayCommand(o => _mainWindow.ShowCarGallery());
            ShowTracksCommand = new RelayCommand(o => _mainWindow.ShowTrackGallery());
            // в конструкторе:
            RecordsCommand = new RelayCommand(o => _mainWindow.ShowRecords());
        }
    }
}
