using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRally.Models
{
    public struct Vector2
    {
        public float X, Y;
        public Vector2(float x, float y) { X = x; Y = y; }
        public float Length() => (float)Math.Sqrt(X * X + Y * Y);
        public float LengthSquared() => X * X + Y * Y;
        public Vector2 Normalized()
        {
            float len = Length();
            if (len < 0.0001f) return new Vector2(0, 0);
            return new Vector2(X / len, Y / len);
        }
        public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;
        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float f) => new Vector2(a.X * f, a.Y * f);
        public static Vector2 operator *(float f, Vector2 a) => new Vector2(a.X * f, a.Y * f);
        public static Vector2 operator /(Vector2 a, float f) => new Vector2(a.X / f, a.Y / f);
    }
}
