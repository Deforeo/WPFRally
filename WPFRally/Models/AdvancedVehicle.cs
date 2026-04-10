using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace WPFRally.Models
{
    public class AdvancedVehicle
    {
        // ----- Параметры двигателя -----
        public AnimationCurve torqueCurve;         // крутящий момент (Нм) в зависимости от RPM
        public float engineInertia = 0.2f;         // момент инерции двигателя
        public float idleRpm = 800f;
        public float redlineRpm = 7000f;
        public float upshiftRpm = 6500f;
        public float downshiftRpm = 2500f;
        public float engineBrakeTorque = 50f;      // Нм при отпущенном газе

        // ----- Трансмиссия -----
        public float[] forwardGearRatios = { 3.5f, 2.0f, 1.4f, 1.0f, 0.8f };
        public float reverseGearRatio = -3.0f;
        public float finalDriveRatio = 4.0f;

        // ----- Колёса и рулевое -----
        public float maxSteerAngle = 30f;           // градусы
        public float steerSpeed = 5f;               // скорость поворота колёс (рад/сек)
        public float driveWheelRadius = 0.35f;      // метры
        public float wheelMass = 15f;               // кг
        public float lsdViscous = 400f;             // коэффициент блокировки диффа

        // ----- Сцепление и сопротивление -----
        public float frictionCoeff = 2.0f;
        public float lateralGrip = 2.5f;
        public float rollingResistance = 12f;

        // ----- Физические свойства (масса, момент инерции) -----
        public float mass = 1000f;                  // кг
        public float inertia = 800f;                // кг·м² (момент инерции автомобиля)

        // ----- Состояние -----
        public SKPoint Position { get; set; }
        public float Angle { get; set; }            // радианы
        public Vector2 LinearVelocity { get; set; } // м/с
        public float AngularVelocity { get; set; }  // рад/с

        // ----- Состояние двигателя и трансмиссии -----
        private float engineAngularVelocity;        // рад/с
        private float currentSteerAngle;            // радианы
        private float steerVelocity;

        private AutomaticTransmissionController controller;
        private CarOutput output;

        // Производные массивы передаточных чисел
        private float[] gearRatios;                 // полные передаточные числа (включая final drive)
        private int maxGear;

        // Свойства для доступа
        public float EngineRPM => engineAngularVelocity * 60f / (2f * (float)Math.PI);
        public float Speed => LinearVelocity.Length();
        public int CurrentGear => output.gear;
        public float Clutch => output.clutch;
        public float SteerAngleDeg => currentSteerAngle * (180f / (float)Math.PI);

        // Размеры для отрисовки и коллизий
        public float Width { get; set; } = 30f;
        public float Height { get; set; } = 18f;

        public AdvancedVehicle()
        {
            // Инициализация кривой момента (по умолчанию – линейная)
            torqueCurve = new AnimationCurve();
            torqueCurve.AddKey(0f, 200f);
            torqueCurve.AddKey(redlineRpm, 150f);

            // Построение полных передаточных чисел
            gearRatios = new float[forwardGearRatios.Length + 1];
            for (int i = 0; i < forwardGearRatios.Length; i++)
                gearRatios[i + 1] = forwardGearRatios[i] * finalDriveRatio;
            maxGear = gearRatios.Length - 1;

            controller = new AutomaticTransmissionController();
            output = controller.output;

            // Начальное состояние
            engineAngularVelocity = idleRpm * 2f * (float)Math.PI / 60f;
            LinearVelocity = new Vector2(0, 0);
            AngularVelocity = 0;
            Angle = 0;
            Position = new SKPoint(0, 0);
        }

        public void UpdatePhysics(CarInput input, float deltaTime)
        {
            // 1. Обновляем контроллер КПП
            controller.Update(input, EngineRPM, Speed,
                upshiftRpm, downshiftRpm, idleRpm,
                maxGear, deltaTime);
            output = controller.output;

            // 2. Обновляем угол поворота колёс
            float targetSteer = output.desiredTurnAngle * (float)Math.PI / 180f;
            currentSteerAngle = SmoothDamp(currentSteerAngle, targetSteer,
                ref steerVelocity, 1f / steerSpeed, float.PositiveInfinity, deltaTime);

            // 3. Связь оборотов двигателя и колёс (если сцепление включено)
            float gearRatio = GetGearRatio(output.gear);
            float wheelSpeed = Speed / driveWheelRadius; // рад/с

            if (gearRatio != 0 && output.clutch > 0.5f)
            {
                engineAngularVelocity = wheelSpeed * gearRatio;
            }
            engineAngularVelocity = Math.Max(engineAngularVelocity, idleRpm * 2f * (float)Math.PI / 60f);

            // 4. Крутящий момент двигателя
            float engineTorque = CalculateEngineTorque(engineAngularVelocity, output.throttle);

            // 5. Продольная сила от двигателя и тормозов
            float driveForce = 0f;
            if (gearRatio != 0 && output.clutch > 0.5f)
            {
                float wheelTorque = engineTorque * gearRatio;
                driveForce = wheelTorque / driveWheelRadius;
            }

            float brakeForce = (output.brake + output.handbrake * 0.8f) * 8000f; // Н
            float resistance = -Math.Sign(wheelSpeed) * rollingResistance * wheelSpeed;

            float totalLongitudinalForce = driveForce + resistance;
            if (brakeForce > 0)
                totalLongitudinalForce = -Math.Sign(wheelSpeed) * brakeForce;

            // 6. Направления
            Vector2 forwardDir = new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
            Vector2 rightDir = new Vector2(-(float)Math.Sin(Angle), (float)Math.Cos(Angle));

            Vector2 tractionForceVec = forwardDir * totalLongitudinalForce;

            // 7. Боковое трение (скольжение)
            Vector2 localVelocity = WorldToLocalVelocity(LinearVelocity, Angle);
            float lateralSlip = localVelocity.Y;
            Vector2 lateralFriction = -rightDir * lateralSlip * lateralGrip * mass;

            // 8. Результирующая сила и изменение линейной скорости
            Vector2 totalForce = tractionForceVec + lateralFriction;
            Vector2 acceleration = totalForce / mass;
            LinearVelocity += acceleration * deltaTime;

            // 9. Поворот автомобиля (угловая скорость)
            float turnRadius = Math.Max(1f, Math.Abs(currentSteerAngle) > 0.01f ? (float)Math.Tan(currentSteerAngle) : 1000f);
            float angularVelocityFromSteer = (Speed / turnRadius) * Math.Sign(currentSteerAngle);
            // Смешиваем с текущей угловой скоростью (можно просто задать)
            AngularVelocity = angularVelocityFromSteer;
            // Дополнительно затухание поворота при малой скорости
            if (Speed < 0.5f) AngularVelocity *= 0.95f;

            Angle += AngularVelocity * deltaTime;

            // 10. Обновление позиции
            Position = new SKPoint(
                Position.X + LinearVelocity.X * deltaTime,
                Position.Y + LinearVelocity.Y * deltaTime
            );

            // 11. Обновление оборотов двигателя при выжатом сцеплении или нейтрали
            if (gearRatio == 0 || output.clutch < 0.5f)
            {
                float engineAccel = engineTorque / engineInertia;
                engineAngularVelocity += engineAccel * deltaTime;
                engineAngularVelocity = Math.Max(engineAngularVelocity, idleRpm * 2f * (float)Math.PI / 60f);
            }
        }

        private float GetGearRatio(int gear)
        {
            if (gear > 0 && gear <= maxGear)
                return gearRatios[gear];
            if (gear == -1)
                return reverseGearRatio * finalDriveRatio;
            return 0;
        }

        private float CalculateEngineTorque(float angVel, float throttle)
        {
            float rpm = angVel * 60f / (2f * (float)Math.PI);
            float baseTorque = torqueCurve.Evaluate(rpm);
            if (throttle > 0.01f)
                return baseTorque * throttle;
            else
            {
                float brakeFactor = (rpm - idleRpm) / (redlineRpm - idleRpm);
                brakeFactor = Math.Max(0f, Math.Min(1f, brakeFactor));
                return -engineBrakeTorque * brakeFactor;
            }
        }

        private Vector2 WorldToLocalVelocity(Vector2 worldVel, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            float localX = worldVel.X * cos + worldVel.Y * sin;
            float localY = -worldVel.X * sin + worldVel.Y * cos;
            return new Vector2(localX, localY);
        }

        private float SmoothDamp(float current, float target, ref float velocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            // Простая реализация SmoothDamp (без оптимизаций)
            float omega = 2f / smoothTime;
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            float change = current - target;
            float maxChange = maxSpeed * smoothTime;
            change = Math.Max(-maxChange, Math.Min(maxChange, change));
            target = current - change;
            float temp = (velocity + omega * change) * deltaTime;
            velocity = (velocity - omega * temp) * exp;
            float result = target + (change + temp) * exp;
            if (target - current > 0f == result > target)
            {
                result = target;
                velocity = (result - target) / deltaTime;
            }
            return result;
        }

        // Вспомогательный метод для получения границ
        public SKRect GetBounds()
        {
            return new SKRect(Position.X - Width / 2, Position.Y - Height / 2,
                              Position.X + Width / 2, Position.Y + Height / 2);
        }
    }

    // Простой класс кривой (аналог AnimationCurve из Unity)
    public class AnimationCurve
    {
        private List<Keyframe> keys = new List<Keyframe>();

        public void AddKey(float time, float value)
        {
            keys.Add(new Keyframe(time, value));
            keys.Sort((a, b) => a.time.CompareTo(b.time));
        }

        public float Evaluate(float time)
        {
            if (keys.Count == 0) return 0;
            if (time <= keys[0].time) return keys[0].value;
            if (time >= keys[keys.Count - 1].time) return keys[keys.Count - 1].value;

            for (int i = 0; i < keys.Count - 1; i++)
            {
                if (time >= keys[i].time && time <= keys[i + 1].time)
                {
                    float t = (time - keys[i].time) / (keys[i + 1].time - keys[i].time);
                    return keys[i].value + (keys[i + 1].value - keys[i].value) * t;
                }
            }
            return 0;
        }

        private struct Keyframe
        {
            public float time, value;
            public Keyframe(float t, float v) { time = t; value = v; }
        }
    }

    // Простая структура Vector2
    public struct Vector2
    {
        public float X, Y;

        public Vector2(float x, float y) { X = x; Y = y; }

        public float Length() => (float)Math.Sqrt(X * X + Y * Y);

        // Арифметические операторы
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float f) => new Vector2(a.X * f, a.Y * f);
        public static Vector2 operator *(float f, Vector2 a) => new Vector2(a.X * f, a.Y * f);
        public static Vector2 operator /(Vector2 a, float f) => new Vector2(a.X / f, a.Y / f);

        // Унарный минус (исправляет ошибку CS0023)
        public static Vector2 operator -(Vector2 v) => new Vector2(-v.X, -v.Y);
    }
}
