using System;
using System.Windows.Media.Imaging;
using System.Text.Json.Serialization;

namespace WPFRally.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float MaxSpeed { get; set; }
        public float Acceleration { get; set; }
        public float Grip { get; set; }
        public string ColorHex { get; set; }
        public string SpritePath { get; set; } // путь к спрайту

        [JsonIgnore] // чтобы не сохранялось в JSON
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
