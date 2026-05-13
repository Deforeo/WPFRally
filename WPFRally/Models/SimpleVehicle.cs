using System;
using SkiaSharp;

namespace WPFRally.Models
{
    public class SimpleVehicle
    {
        public Vector2 Position;
        public float Angle;
        public Vector2 Velocity;
        private float _speed;

        // Параметры (будут заданы из Car)
        public float MaxSpeed { get; set; }
        public float Acceleration { get; set; }
        public float BrakeForce { get; set; }
        public float Friction { get; set; }
        public float TurnSpeed { get; set; }
        public float HighSpeedTurnReduction { get; set; }
        public float LateralGrip { get; set; }
        public float DriftGripReduction { get; set; }
        
        public float Width { get; set; }
        public float Height { get; set; }

        private float CollisionWidth = 35f;
        private float CollisionHeight = 75f;

        public float Speed => Velocity.Length();

        public SimpleVehicle() { }

        public void ApplyCarParameters(Car car)
        {
            if (car == null) return;
            MaxSpeed = car.MaxSpeed;
            Acceleration = car.Acceleration;
            BrakeForce = car.BrakeForce;
            Friction = car.Friction;
            TurnSpeed = car.TurnSpeed;
            HighSpeedTurnReduction = car.HighSpeedTurnReduction;
            LateralGrip = car.LateralGrip;
            DriftGripReduction = car.DriftGripReduction;
            Width = car.Width;
            Height = car.Height;
        }

        public void Update(float deltaTime, float throttle, float brake, float handbrake, float steer)
        {
            Vector2 force = Vector2.Zero;

            // Газ
            if (throttle > 0)
            {
                Vector2 forward = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
                force += forward * Acceleration * throttle;
            }

            // Торможение
            float totalBrake = (brake > 0 ? BrakeForce * brake : 0) + (handbrake > 0 ? BrakeForce * handbrake * 1.2f : 0);
            if (totalBrake > 0 && Velocity.LengthSquared() > 0.01f)
                force -= Velocity.Normalized() * totalBrake;

            // Сопротивление
            if (Velocity.LengthSquared() > 0.01f)
                force -= Velocity.Normalized() * Friction;

            Velocity += force * deltaTime;
            if (Velocity.Length() > MaxSpeed)
                Velocity = Velocity.Normalized() * MaxSpeed;

            // Поворот
            if (Velocity.Length() > 0.5f)
            {
                float speedFactor = 1f - (Velocity.Length() / MaxSpeed) * HighSpeedTurnReduction;
                float turn = steer * TurnSpeed * speedFactor * deltaTime;
                Angle += turn;
            }

            // Боковое трение (дрифт)
            Vector2 forwardDir = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
            Vector2 rightDir = new Vector2((float)Math.Cos(Angle + Math.PI / 2), (float)Math.Sin(Angle + Math.PI / 2));

            float forwardSpeed = Vector2.Dot(Velocity, forwardDir);
            float lateralSpeed = Vector2.Dot(Velocity, rightDir);

            // Вычисляем интенсивность заноса (0..1)
            float driftIntensity = Math.Min(1f, Math.Abs(lateralSpeed) / MaxSpeed);
            float currentGrip = LateralGrip;
            if (driftIntensity > 0.1f)
                currentGrip *= (1f - DriftGripReduction * Math.Min(0.9f, driftIntensity));

            // Применяем боковое трение
            Vector2 newVelocity = forwardDir * forwardSpeed + rightDir * lateralSpeed * currentGrip;
            Velocity = newVelocity;

            // Обновление позиции
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