using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFRally.Models;

namespace WPFRally.Services
{
    public interface IDataService
    {
        List<Car> LoadCars();
        void SaveCars(List<Car> cars);
        List<Track> LoadTracks();
        void SaveTracks(List<Track> tracks);
        List<Record> LoadRecords();
        void SaveRecords(List<Record> records);
    }
}
