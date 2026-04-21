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
            SelectCarCommand = new RelayCommand(car =>
            {
                var selectedCar = car as Car;
                if (selectedCar != null)
                {
                    _mainWindow.SetSelectedCar(selectedCar);  // <- теперь поле получает значение
                    _mainWindow.ShowTrackSelection();
                }
            });
            BackCommand = new RelayCommand(o => _mainWindow.ShowMenu());
        }
    }
}
