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

        public MenuViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            PlayCommand = new RelayCommand(o => _mainWindow.ShowCarSelection());
            CarsCommand = new RelayCommand(o => _mainWindow.ShowCarSelection()); // можно отдельное окно, но пока так
            // Исправлено: передаём null, но в ShowTrackSelection нужно предусмотреть этот случай
            TracksCommand = new RelayCommand(o => _mainWindow.ShowTrackSelection(null));
            ExitCommand = new RelayCommand(o => _mainWindow.ExitGame());
        }
    }
}
