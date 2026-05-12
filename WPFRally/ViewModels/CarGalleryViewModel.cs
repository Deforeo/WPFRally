using System;
using System.Collections.Generic;
using System.Windows.Input;
using WPFRally.Models;
using WPFRally.Services;

namespace WPFRally.ViewModels
{
    public class CarGalleryViewModel
    {
        private readonly MainWindow _mainWindow;
        public List<Car> Cars { get; private set; }
        public ICommand BackCommand { get; }

        public CarGalleryViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            var dataService = new JsonDataService();
            Cars = dataService.LoadCars();
            BackCommand = new RelayCommand(o => _mainWindow.ShowMenu());
        }
    }
}

