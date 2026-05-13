using System;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace WPFRally.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SpritePath { get; set; }
        public string ColorHex { get; set; }

        // --- Физические параметры ---
        public float MaxSpeed { get; set; } = 650f;
        public float Acceleration { get; set; } = 700f;
        public float BrakeForce { get; set; } = 900f;
        public float Friction { get; set; } = 80f;
        public float TurnSpeed { get; set; } = 3.2f;
        public float HighSpeedTurnReduction { get; set; } = 0.4f;
        public float LateralGrip { get; set; } = 1.1f;
        public float DriftGripReduction { get; set; } = 0.3f;

        // Размеры коллизии и визуала
        public float Width { get; set; } = 45f;
        public float Height { get; set; } = 90f;

        [JsonIgnore]
        public BitmapImage Thumbnail
        {
            get
            {
                if (string.IsNullOrEmpty(SpritePath)) return null;
                try
                {
                    var uri = new Uri(SpritePath, UriKind.Relative);
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = uri;
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();
                    return img;
                }
                catch { return null; }
            }
        }
    }
}