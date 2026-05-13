using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Text.Json;
using WPFRally.Models;
using SkiaSharp;

namespace WPFRally.Services
{
    public class JsonDataService : IDataService
    {
        // Используем абсолютный путь к папке Data внутри папки приложения
        private readonly string _dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        private readonly string _carsPath;
        private readonly string _tracksPath;
        private readonly string _recordsPath;
        private readonly int _defaultWight = 128;
        private readonly int _defaultHight = 128;
        private readonly int _defaultX = 128;
        private readonly int _defaultY = 128;

        public JsonDataService()
        {
            // Создаём папку Data в той же директории, где EXE
            if (!Directory.Exists(_dataDir))
                Directory.CreateDirectory(_dataDir);

            _carsPath = Path.Combine(_dataDir, "cars.json");
            _tracksPath = Path.Combine(_dataDir, "tracks.json");
            _recordsPath = Path.Combine(_dataDir, "records.json");

            // При первом запуске создаём файлы с дефолтными данными, если их нет или они пустые
            EnsureFileExists(_carsPath, CreateDefaultCars);
            EnsureFileExists(_tracksPath, CreateDefaultTracks);
            EnsureFileExists(_recordsPath, () => File.WriteAllText(_recordsPath, "[]"));
        }

        private void EnsureFileExists(string path, Action createAction)
        {
            if (!File.Exists(path) || new FileInfo(path).Length == 0)
            {
                createAction();
                Debug.WriteLine($"Создан файл: {path}");
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
                if (!File.Exists(path))
                    return defaultValue;

                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                    return defaultValue;

                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки {path}: {ex.Message}");
                return defaultValue;
            }
        }

        private void SaveToFile<T>(string path, T data)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(path, json);
                Debug.WriteLine($"Сохранено в {path}: {json.Length} байт");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка сохранения {path}: {ex.Message}");
                throw; // Пробросить, чтобы вызывающий код знал о проблеме
            }
        }

        private void CreateDefaultCars()
        {
            var cars = new List<Car>
            {
                new Car
        {
            Id = 1,
            Name = "Rally Beast",
            SpritePath = "Assets/Sprites/RedStrip.png",
            ColorHex = "#FF4444",
            MaxSpeed = 600f,
            Acceleration = 400f,
            BrakeForce = 900f,
            Friction = 80f,
            TurnSpeed = 3.0f,
            HighSpeedTurnReduction = 0.45f,
            LateralGrip = 0.85f,
            DriftGripReduction = 0.35f,
            Width = 80f,
            Height = 120f
        },
        new Car
        {
            Id = 2,
            Name = "Drift Master",
            SpritePath = "Assets/Sprites/GreenStrip.png",
            ColorHex = "#44FF44",
            MaxSpeed = 550f,
            Acceleration = 450f,
            BrakeForce = 850f,
            Friction = 70f,
            TurnSpeed = 3.5f,
            HighSpeedTurnReduction = 0.35f,
            LateralGrip = 0.70f,
            DriftGripReduction = 0.55f,
            Width = 80f,
            Height = 120f
        },
        new Car
        {
            Id = 3,
            Name = "Grip Pro",
            SpritePath = "Assets/Sprites/BlueStrip.png",
            ColorHex = "#4444FF",
            MaxSpeed = 650f,
            Acceleration = 500f,
            BrakeForce = 950f,
            Friction = 90f,
            TurnSpeed = 2.8f,
            HighSpeedTurnReduction = 0.50f,
            LateralGrip = 0.95f,
            DriftGripReduction = 0.20f,
            Width = 80f,
            Height = 120f
        }
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
                    Name = "Forest nail",
                    WorldWidth = 5120,
                    WorldHeight = 3840,
                    StartPosition = new SKPoint(_defaultX*2+32, _defaultY*11),
                    FinishPosition = new SKPoint(_defaultX*9, _defaultY*19+32),
                    BackgroundSpritePath = "Assets/Sprites/amp-4.png",
                    Obstacles = new List<ObstacleData>
                    {
                        // Прямоугольники
                        new ObstacleData{X = _defaultX, Y = _defaultY * 10, Width = _defaultWight *6, Height = _defaultHight,  SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX, Y = _defaultY * 12, Width = _defaultWight *6, Height = _defaultHight,  SpritePath = null, IsVisible = false },
                      
                        new ObstacleData{X = _defaultX * 12, Y = _defaultY * 4, Width = _defaultWight * 8, Height = _defaultHight, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 14, Y = _defaultY * 9, Width = _defaultWight * 6, Height = _defaultHight, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 14, Y = _defaultY * 14, Width = _defaultWight * 6, Height = _defaultHight, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 28, Y = _defaultY * 3, Width = _defaultWight * 8, Height = _defaultHight, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 28, Y = _defaultY * 5, Width = _defaultWight * 6, Height = _defaultHight, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 32, Y = _defaultY * 20, Width = _defaultWight * 4, Height = _defaultHight, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 3, Y = _defaultY * 25, Width = _defaultWight * 30, Height = _defaultHight, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 3, Y = _defaultY * 27, Width = _defaultWight * 30, Height = _defaultHight, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 3, Y = _defaultY * 15, Width = _defaultWight * 14, Height = _defaultHight, SpritePath = null, IsVisible = false },

                        // Шатры
                        new ObstacleData{X = _defaultX * 0, Y = _defaultY * 17, Width = _defaultWight * 2, Height = _defaultHight * 9, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 33, Y = _defaultY * 8, Width = _defaultWight * 2, Height = _defaultHight * 3, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 34, Y = _defaultY * 12, Width = _defaultWight * 2, Height = _defaultHight * 3, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 33, Y = _defaultY * 16, Width = _defaultWight * 2, Height = _defaultHight * 3, SpritePath = null, IsVisible = false },

                        // Кубы
                        new ObstacleData{X = _defaultX * 2, Y = _defaultY * 8, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 3, Y = _defaultY * 6, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 5, Y = _defaultY * 13, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 8, Y = _defaultY * 13, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 12, Y = _defaultY * 1, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 15, Y = _defaultY * 1, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 18, Y = _defaultY * 1, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 14, Y = _defaultY * 6, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 17, Y = _defaultY * 5, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 14, Y = _defaultY * 11, Width = _defaultWight * 6, Height = _defaultHight*2 , SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 28, Y = _defaultY * 1, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 34, Y = _defaultY * 5, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 28, Y = _defaultY * 10, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 28, Y = _defaultY * 14, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 27, Y = _defaultY * 17, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 28, Y = _defaultY * 22, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 23, Y = _defaultY * 15, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 23, Y = _defaultY * 21, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 12, Y = _defaultY * 19, Width = _defaultWight * 6, Height = _defaultHight *2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 5, Y = _defaultY * 19, Width = _defaultWight * 2, Height = _defaultHight * 4, SpritePath = null, IsVisible = false },
                         new ObstacleData{X = _defaultX * 0, Y = _defaultY * 11, Width = _defaultWight , Height = _defaultHight, SpritePath = null, IsVisible = false },

                        // Фонари
                        new ObstacleData{X = _defaultX * 7, Y = _defaultY * 8, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 8, Y = _defaultY * 7, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 9, Y = _defaultY * 6, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 10, Y = _defaultY * 5, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 8, Y = _defaultY * 11, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 9, Y = _defaultY * 10, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 10, Y = _defaultY * 9, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 11, Y = _defaultY * 8, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 12, Y = _defaultY * 7, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 21, Y = _defaultY * 11, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 22, Y = _defaultY * 10, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 23, Y = _defaultY * 9, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 24, Y = _defaultY * 8, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 25, Y = _defaultY * 7, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 26, Y = _defaultY * 6, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 23, Y = _defaultY * 13, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 24, Y = _defaultY * 12, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 25, Y = _defaultY * 11, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 26, Y = _defaultY * 10, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 27, Y = _defaultY * 9, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 28, Y = _defaultY * 8, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 33, Y = _defaultY * 23, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 34, Y = _defaultY * 22, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 35, Y = _defaultY * 21, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 34, Y = _defaultY * 26, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 35, Y = _defaultY * 25, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 36, Y = _defaultY * 24, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 19, Y = _defaultY * 15, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 20, Y = _defaultY * 16, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 21, Y = _defaultY * 17, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 22, Y = _defaultY * 18, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 23, Y = _defaultY * 19, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 17, Y = _defaultY * 17, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 18, Y = _defaultY * 18, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 19, Y = _defaultY * 19, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 20, Y = _defaultY * 20, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 19, Y = _defaultY * 21, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 8, Y = _defaultY * 18, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false },
                        new ObstacleData{X = _defaultX * 10, Y = _defaultY * 18, Width = _defaultWight, Height = _defaultHight * 2, SpritePath = null, IsVisible = false }
                    } ,
                    Checkpoints = new List<CheckpointData>
                    {
                        new CheckpointData { X = 384, Y = 1408, Width = 40, Height = 128, Index = 0 },
                        new CheckpointData { X = 2048, Y = 384, Width = 40, Height = 128, Index = 1 },
                        new CheckpointData { X = 2176, Y = 1280, Width = 40, Height = 128, Index = 2 },
                        new CheckpointData { X = 2688, Y = 1664, Width = 40, Height = 128, Index = 3 },
                        new CheckpointData { X = 3456, Y = 640, Width = 40, Height = 128, Index = 4 },
                        new CheckpointData { X = 4608, Y = 768, Width = 40, Height = 128, Index = 5 },
                        new CheckpointData { X = 4096, Y = 1408, Width = 40, Height = 128, Index = 6 },
                        new CheckpointData { X = 4608, Y = 1920, Width = 40, Height = 128, Index = 7 },
                        new CheckpointData { X = 4096, Y = 2432, Width = 40, Height = 128, Index = 8 },
                        new CheckpointData { X = 4608, Y = 2816, Width = 40, Height = 128, Index = 9 },
                        new CheckpointData { X = 4096, Y = 3328, Width = 40, Height = 128, Index = 10 },
                        new CheckpointData { X = 384, Y = 3328, Width = 40, Height = 128, Index = 11 },
                        new CheckpointData { X = 2176, Y = 2048, Width = 40, Height = 128, Index = 12 },
                        new CheckpointData { X = 1920, Y = 2944, Width = 40, Height = 128, Index = 13 },
                        new CheckpointData { X = 1152, Y = 2560, Width = 40, Height = 128, Index = 14 }
                    }
                },
                new Track
                {
                    Id =2,
                    Name = "test",
                    WorldWidth = 1000,
                    WorldHeight = 1000,
                    StartPosition = new SKPoint(_defaultX*2, _defaultY*2),
                    FinishPosition = new SKPoint(_defaultX*4, _defaultY*4),
                    BackgroundSpritePath = null,
                    Obstacles = new List<ObstacleData>
                    {
                         new ObstacleData{X = _defaultX * 2, Y = _defaultY * 8, Width = _defaultWight * 2, Height = _defaultHight * 2, SpritePath = null, IsVisible = true },
                    },
                     Checkpoints = new List<CheckpointData>
                    {
                        new CheckpointData { X = 384, Y = 364, Width = 40, Height = 128, Index = 0 },
                    }
                },
            };
            SaveTracks(tracks);
        }
    }
}
