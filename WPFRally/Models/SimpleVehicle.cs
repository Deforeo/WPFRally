using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkiaSharp;

namespace WPFRally.Models
{
    public class SimpleVehicle
    {
        public Vector2 Position;
        public float Angle;           // радианы
        public Vector2 Velocity;

        // Параметры (подобраны для аркадного управления)
        public float MaxSpeed = 350f;
        public float Acceleration = 400f;
        public float BrakeForce = 300f;
        public float Friction = 80f;

        public float TurnSpeed = 3.2f;
        public float HighSpeedTurnReduction = 0.4f;  // чем выше скорость, тем хуже поворот

        // Сцепление с дорогой
        public float Grip = 0.85f;           // базовое сцепление
        public float HandbrakeGrip = 1.1f;   // сцепление при пробеле
        public float DriftGripReduction = 1f; // потеря сцепления при скольжении

        public float Width = 80f;
        public float Height = 120f;
        public float CollisionWidth = 35f;   // меньше, чем Width
        public float CollisionHeight = 70f;

        public float Speed => Velocity.Length();

        public SimpleVehicle()
        {
            Position = new Vector2(0, 0);
            Velocity = new Vector2(0, 0);
            Angle = 0;
        }

        public void Update(float deltaTime, float throttle, float brake, float handbrake, float steer)
        {
            // ----- 1. Ускорение / торможение -----
            Vector2 force = Vector2.Zero;

            // Газ
            if (throttle > 0)
            {
                Vector2 forward = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
                force += forward * Acceleration * throttle;
            }

            // Тормоз (обычный и ручник)
            float totalBrake = (brake > 0 ? BrakeForce * brake : 0) + (handbrake > 0 ? BrakeForce * handbrake * 1.2f : 0);
            if (totalBrake > 0 && Velocity.LengthSquared() > 0.01f)
            {
                force -= Velocity.Normalized() * totalBrake;
            }

            // Сопротивление качению
            if (Velocity.LengthSquared() > 0.01f)
            {
                force -= Velocity.Normalized() * Friction;
            }

            Velocity += force * deltaTime;
            if (Velocity.Length() > MaxSpeed) Velocity = Velocity.Normalized() * MaxSpeed;

            // ----- 2. Поворот (только при движении) -----
            if (Velocity.Length() > 0.5f)
            {
                float speedFactor = 1f - (Velocity.Length() / MaxSpeed) * HighSpeedTurnReduction;
                float turn = steer * TurnSpeed * speedFactor * deltaTime;
                Angle += turn;
            }

            // ----- 3. Боковое трение (убирает скольжение вбок) -----
            Vector2 forwardDir = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
            Vector2 rightDir = new Vector2((float)Math.Cos(Angle + Math.PI / 2), (float)Math.Sin(Angle + Math.PI / 2));

            float forwardSpeed = Vector2.Dot(Velocity, forwardDir);
            float lateralSpeed = Vector2.Dot(Velocity, rightDir);

            float driftIntensity = Math.Min(1f, Math.Abs(lateralSpeed) / MaxSpeed);

            // Базовое сцепление
            float currentGrip = Grip;

            // Если зажат ручник – заменяем сцепление на HandbrakeGrip (но не выше 1 и не ниже 0)
            if (handbrake > 0)
            {
                currentGrip = HandbrakeGrip;
            }
            else if (driftIntensity > 0.1f)
            {
                // Уменьшаем сцепление в зависимости от интенсивности заноса
                // DriftGripReduction может быть больше 1, но мы ограничим итоговое значение снизу
                float reduction = DriftGripReduction * driftIntensity;
                currentGrip *= 1f - Math.Min(0.9f, reduction); // не более 90% потери сцепления
            }

            // Ограничиваем сцепление разумными пределами [0.1 .. 1.0]
            currentGrip = Math.Max(0.1f, Math.Min(1.0f, currentGrip));

            // Применяем боковое трение – убираем боковую скорость пропорционально сцеплению
            Vector2 newVelocity = forwardDir * forwardSpeed + rightDir * lateralSpeed * currentGrip;
            Velocity = newVelocity;

            // ----- 4. Обновление позиции -----
            Position += Velocity * deltaTime;

            // Нормализация угла
            if (Angle > Math.PI * 2) Angle -= (float)(Math.PI * 2);
            if (Angle < 0) Angle += (float)(Math.PI * 2);
        }

        public SKRect GetBounds()
        {
            return new SKRect(Position.X - CollisionWidth / 2, Position.Y - CollisionHeight / 2,
                              Position.X + CollisionWidth / 2, Position.Y + CollisionHeight / 2);
        }

        public SKPoint ToSKPoint() => new SKPoint(Position.X, Position.Y);
    }
}
