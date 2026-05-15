using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace WPFRally.Models
{
    public class GameWorld
    {
        public SimpleVehicle Player { get; set; }
        public List<Obstacle> Obstacles { get; set; }
        public List<TriggerZone> TriggerZones { get; set; }
        public List<Checkpoint> Checkpoints { get; set; }
        public int NextCheckpointIndex { get; set; } = 0;

        public float WorldWidth { get; set; }
        public float WorldHeight { get; set; }

        public bool IsRaceActive { get; set; }
        public bool IsFinished { get; set; }
        public float RaceTime { get; set; }
        public event Action<float> OnRaceFinished;

        public event Action OnStartLineCrossed;

        public GameWorld()
        {
            Player = new SimpleVehicle();
            Player.Position = new Vector2(0, 0);
            Player.Angle = 0;
            Player.Velocity = new Vector2(0, 0);

            Obstacles = new List<Obstacle>();

            Checkpoints = new List<Checkpoint>();
            NextCheckpointIndex = 0;

            TriggerZones = new List<TriggerZone>();
        }

        public void Update(float deltaTime, float throttle, float brake, float handbrake, float steer)
        {
            if (IsFinished) return;



            var oldPos = Player.Position;
            var oldVel = Player.Velocity;

            Player.Update(deltaTime, throttle, brake, handbrake, steer);

            // Границы мира
            if (!IsWithinBounds(Player.Position))
            {
                Player.Position = oldPos;
                Player.Velocity = new Vector2(0, 0);
            }

            // Коллизии с препятствиями
            var vehicleRect = Player.GetBounds();
            foreach (var obs in Obstacles)
            {
                
                if (obs.CollidesWith(vehicleRect))
                {
                    // Выталкиваем и корректируем скорость
                    ResolveCollision(obs, vehicleRect, ref Player.Position, ref Player.Velocity);
                    // Пересчитываем границы (могли изменить позицию)
                    vehicleRect = Player.GetBounds();
                    // Повторяем проверку, если всё ещё пересекаемся – ещё раз выталкиваем (но обычно хватает одного раза)
                    if (obs.CollidesWith(vehicleRect))
                    {
                        // fallback – откат позиции (на всякий случай)
                        Player.Position = oldPos;
                        Player.Velocity = Vector2.Zero;
                    }
                    break; // выходим после первого же столкновения (можно обработать все, но с break проще)
                }
            }

            // Триггеры
            foreach (var zone in TriggerZones)
            {
                if (!zone.IsActive) continue;
                if (zone.Type == "Start" && zone.Intersects(vehicleRect))
                {
                    zone.IsActive = false;
/*                    IsRaceActive = true;*/
                    OnStartLineCrossed?.Invoke();
                    RaceTime = 0f;
                }
                if (zone.Type == "Finish" && IsRaceActive && zone.Intersects(vehicleRect) && NextCheckpointIndex >= Checkpoints.Count)
                {
                    zone.IsActive = false;
                    IsFinished = true;
                    IsRaceActive = false;
                    OnRaceFinished?.Invoke(RaceTime);
                    break;
                }
            }

            foreach (var cp in Checkpoints)
            {
                if (!cp.IsPassed && cp.Index == NextCheckpointIndex && cp.Intersects(vehicleRect))
                {
                    cp.IsPassed = true;
                    NextCheckpointIndex++;
                    break; // за один кадр только один чекпоинт
                }
            }

            if (IsRaceActive && !IsFinished)
                RaceTime += deltaTime;
        }

        private void ResolveCollision(Obstacle obs, SKRect vehicleRect, ref Vector2 position, ref Vector2 velocity)
        {
            // Вычисляем перекрытие (пересечение) по X и Y
            float overlapLeft = vehicleRect.Right - obs.Rect.Left;
            float overlapRight = obs.Rect.Right - vehicleRect.Left;
            float overlapTop = vehicleRect.Bottom - obs.Rect.Top;
            float overlapBottom = obs.Rect.Bottom - vehicleRect.Top;

            // Находим минимальное смещение, чтобы вытолкнуть
            float minOverlap = Math.Min(overlapLeft, Math.Min(overlapRight, Math.Min(overlapTop, overlapBottom)));

            if (minOverlap <= 0) return;

            // Смещение по оси X или Y в зависимости от того, где перекрытие минимально
            if (minOverlap == overlapLeft)
                position.X -= minOverlap;
            else if (minOverlap == overlapRight)
                position.X += minOverlap;
            else if (minOverlap == overlapTop)
                position.Y -= minOverlap;
            else if (minOverlap == overlapBottom)
                position.Y += minOverlap;

            // Гасим только нормальную составляющую скорости (в зависимости от того, по какой оси вытолкнули)
            if (minOverlap == overlapLeft || minOverlap == overlapRight)
                velocity.X = 0;
            else if (minOverlap == overlapTop || minOverlap == overlapBottom)
                velocity.Y = 0;

            // Можно также немного уменьшить скорость, но не обнулять полностью
            // For example: velocity = velocity * 0.7f;
        }

        private bool IsWithinBounds(Vector2 pos)
        {
            float halfW = Player.Width / 2;
            float halfH = Player.Height / 2;
            return pos.X - halfW >= 0 && pos.X + halfW <= WorldWidth &&
                   pos.Y - halfH >= 0 && pos.Y + halfH <= WorldHeight;
        }
    }
}