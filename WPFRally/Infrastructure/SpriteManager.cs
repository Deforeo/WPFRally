using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SkiaSharp;


namespace WPFRally.Infrastructure
{
    public static class SpriteManager
    {
        private static Dictionary<string, SKBitmap> _cache = new Dictionary<string, SKBitmap>();

        public static void LoadAll()
        {
            string spritesDir = "Assets/Sprites";
            if (!Directory.Exists(spritesDir))
            {
                Directory.CreateDirectory(spritesDir);
                return;
            }

            var files = Directory.GetFiles(spritesDir, "*.png", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string key = Path.GetFileNameWithoutExtension(file);
                var bitmap = SKBitmap.Decode(file);
                if (bitmap != null)
                    _cache[key] = bitmap;
            }
        }

        public static SKBitmap GetSprite(string pathOrKey)
        {
            if (string.IsNullOrEmpty(pathOrKey)) return null;
            string key = Path.GetFileNameWithoutExtension(pathOrKey);
            if (_cache.ContainsKey(key))
                return _cache[key];
            if (File.Exists(pathOrKey))
                return SKBitmap.Decode(pathOrKey);
            return null;
        }
    }
}
