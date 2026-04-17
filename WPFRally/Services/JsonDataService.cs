using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using WPFRally.Models;
using SkiaSharp;

namespace WPFRally.Services
{
    public class JsonDataService : IDataService
    {
        private readonly string _carsPath = "Data/cars.json";
        private readonly string _tracksPath = "Data/tracks.json";
        private readonly string _recordsPath = "Data/records.json";

        public JsonDataService()
        {
            Directory.CreateDirectory("Data");
            EnsureFileExists(_carsPath, CreateDefaultCars);
            EnsureFileExists(_tracksPath, CreateDefaultTracks);
            EnsureFileExists(_recordsPath, () => File.WriteAllText(_recordsPath, "[]"));
        }

        private void EnsureFileExists(string path, Action createDefault)
        {
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                createDefault();
            }
        }

        public List<Car> LoadCars()
        {
            return LoadFromFile<List<Car>>(_carsPath, new List<Car>());
        }

        public void SaveCars(List<Car> cars)
        {
            SaveToFile(_carsPath, cars);
        }

        public List<Track> LoadTracks()
        {
            return LoadFromFile<List<Track>>(_tracksPath, new List<Track>());
        }

        public void SaveTracks(List<Track> tracks)
        {
            SaveToFile(_tracksPath, tracks);
        }

        public List<Record> LoadRecords()
        {
            return LoadFromFile<List<Record>>(_recordsPath, new List<Record>());
        }

        public void SaveRecords(List<Record> records)
        {
            SaveToFile(_recordsPath, records);
        }

        private T LoadFromFile<T>(string path, T defaultValue)
        {
            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return defaultValue;
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return defaultValue;
            }
        }

        private void SaveToFile<T>(string path, T data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(path, json);
        }

        private void CreateDefaultCars()
        {
            var cars = new List<Car>
            {
                new Car { Id = 1, Name = "Rally Beast", MaxSpeed = 650, Acceleration = 700, Grip = 0.85f, ColorHex = "#FF4444", SpritePath = "Assets/Sprites/car_red.png" },
                new Car { Id = 2, Name = "Drift Master", MaxSpeed = 600, Acceleration = 750, Grip = 0.70f, ColorHex = "#44FF44", SpritePath = "Assets/Sprites/car_green.png" },
                new Car { Id = 3, Name = "Grip Pro", MaxSpeed = 700, Acceleration = 650, Grip = 0.95f, ColorHex = "#4444FF", SpritePath = "Assets/Sprites/car_blue.png" }
            };
            SaveCars(cars);
        }

        private void CreateDefaultTracks()
        {
            var tracks = new List<Track>
            {
                new Track
                {
                    Id = 1,
                    Name = "Forest Trail",
                    WorldWidth = 1800,
                    WorldHeight = 1400,
                    StartPosition = new SKPoint(200, 700),
                    FinishPosition = new SKPoint(1500, 700),
                    BackgroundSpritePath = "Assets/Sprites/grass_bg.png",
                    Obstacles = new List<ObstacleData>()
                }
            };
            SaveTracks(tracks);
        }
    }
}
