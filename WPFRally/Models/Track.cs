using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using SkiaSharp;
using System.Text.Json.Serialization;


namespace WPFRally.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float WorldWidth { get; set; }
        public float WorldHeight { get; set; }
        public SKPoint StartPosition { get; set; }
        public SKPoint FinishPosition { get; set; }
        public List<ObstacleData> Obstacles { get; set; }
        public List<CheckpointData> Checkpoints { get; set; } = new List<CheckpointData>();
        public string BackgroundSpritePath { get; set; } // фон трассы

        [JsonIgnore] // чтобы не сохранялось в JSON
        public BitmapImage Thumbnail
        {
            get
            {
                if (string.IsNullOrEmpty(BackgroundSpritePath)) return null;
                try
                {
                    var uri = new Uri(BackgroundSpritePath, UriKind.Relative);
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

    public class ObstacleData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string SpritePath { get; set; }
        public bool IsVisible { get; set; } = true;
    }
    public class CheckpointData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public int Index { get; set; }
    }


}
