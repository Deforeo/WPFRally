using System;
using System.Collections.Generic;
using System.Windows.Input;
using WPFRally.Models;
using WPFRally.Services;

namespace WPFRally.ViewModels
{
    public class CarSelectionViewModel
    {
        private readonly MainWindow _mainWindow;
        public List<Car> Cars { get; private set; }
        public ICommand SelectCarCommand { get; }
        public ICommand BackCommand { get; }

        public CarSelectionViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            var dataService = new JsonDataService();
            Cars = dataService.LoadCars();
            SelectCarCommand = new RelayCommand(car => _mainWindow.ShowTrackSelection(car as Car));
            BackCommand = new RelayCommand(o => _mainWindow.ShowMenu());
        }
    }
}
