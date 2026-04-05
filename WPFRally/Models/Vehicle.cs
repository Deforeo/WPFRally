using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace WPFRally.Models
{
    public class Vehicle
    {
        public SKPoint Position { get; set; }      // координаты на карте (в пикселях)
        public float Angle { get; set; }           // угол в радианах (0 - вправо)
        public float Speed { get; set; }           // текущая скорость (пикселей/сек)

        // Параметры физики
        public float MaxSpeed { get; set; } = 400f;
        public float Acceleration { get; set; } = 300f;   // ускорение (пикс/сек²)
        public float BrakeDeceleration { get; set; } = 500f;
        public float Friction { get; set; } = 150f;       // замедление без газа
        public float TurnSpeed { get; set; } = 2.5f;      // радиан/сек при полном повороте
        public float TurnSpeedFactorAtSpeed { get; set; } = 0.005f; // чем быстрее, тем хуже поворот

        // Размеры для отрисовки и коллизий
        public float Width { get; set; } = 30f;
        public float Height { get; set; } = 18f;

        public void Update(float deltaTime, float throttle, float brake, float steer)
        {
            // Торможение
            if (brake > 0)
            {
                Speed -= BrakeDeceleration * deltaTime;
                if (Speed < 0) Speed = 0;
            }
            else
            {
                // Газ
                if (throttle > 0)
                {
                    Speed += Acceleration * throttle * deltaTime;
                    if (Speed > MaxSpeed) Speed = MaxSpeed;
                }
                else
                {
                    // Трение
                    Speed -= Friction * deltaTime;
                    if (Speed < 0) Speed = 0;
                }
            }

            // Поворот (только если есть скорость)
            if (Math.Abs(Speed) > 0.1f)
            {
                float turn = steer * TurnSpeed * deltaTime;
                // Ухудшение управляемости на высокой скорости
                turn *= (1f - (Speed / MaxSpeed) * TurnSpeedFactorAtSpeed);
                Angle += turn;
            }

            // Движение
            float dx = (float)Math.Cos(Angle) * Speed * deltaTime;
            float dy = (float)Math.Sin(Angle) * Speed * deltaTime;
            Position = new SKPoint(Position.X + dx, Position.Y + dy);
        }

        // Получение прямоугольника машинки (с учётом поворота – для коллизий упростим до AABB)
        public SKRect GetBounds()
        {
            return new SKRect(Position.X - Width / 2, Position.Y - Height / 2,
                              Position.X + Width / 2, Position.Y + Height / 2);
        }
    }
}
