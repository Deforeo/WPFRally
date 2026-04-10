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
        public AdvancedVehicle Player { get; set; }
        public List<Obstacle> Obstacles { get; set; }
        public List<TriggerZone> TriggerZones { get; set; }

        public float WorldWidth { get; set; } = 1500f;
        public float WorldHeight { get; set; } = 1200f;

        public bool IsRaceActive { get; set; } = false;
        public bool IsFinished { get; set; } = false;
        public float RaceTime { get; set; } = 0f;

        public event Action<float> OnRaceFinished;

        public GameWorld()
        {
            Player = new AdvancedVehicle();
            Player.Position = new SKPoint(400, 300);
            Player.Angle = 0;
            Player.LinearVelocity = new Vector2(0, 0);

            Obstacles = new List<Obstacle>();
            Obstacles.Add(new Obstacle(500, 280, 60, 40));
            Obstacles.Add(new Obstacle(200, 150, 80, 30));
            Obstacles.Add(new Obstacle(700, 500, 50, 50));
            Obstacles.Add(new Obstacle(100, 550, 120, 40));

            TriggerZones = new List<TriggerZone>();
            TriggerZones.Add(new TriggerZone(370, 270, 60, 60, "Start"));
            TriggerZones.Add(new TriggerZone(1200, 900, 80, 80, "Finish"));
        }

        public void Update(float deltaTime, CarInput input)
        {
            if (IsFinished) return;

            var oldPos = Player.Position;
            Player.UpdatePhysics(input, deltaTime);

            // Границы мира
            if (!IsWithinBounds(Player.Position))
            {
                Player.Position = oldPos;
                Player.LinearVelocity = new Vector2(0, 0);
            }

            // Коллизии с препятствиями
            var vehicleRect = Player.GetBounds();
            foreach (var obs in Obstacles)
            {
                if (obs.CollidesWith(vehicleRect))
                {
                    Player.Position = oldPos;
                    Player.LinearVelocity = new Vector2(0, 0);
                    break;
                }
            }

            // Проверка триггеров старт/финиш
            foreach (var zone in TriggerZones)
            {
                if (!zone.IsActive) continue;

                if (zone.Type == "Start" && zone.Intersects(Player.GetBounds()))
                {
                    zone.IsActive = false;
                    IsRaceActive = true;
                    RaceTime = 0f;
                    continue;
                }

                if (zone.Type == "Finish" && IsRaceActive && zone.Intersects(Player.GetBounds()))
                {
                    zone.IsActive = false;
                    IsFinished = true;
                    IsRaceActive = false;
                    OnRaceFinished?.Invoke(RaceTime);
                    break;
                }
            }

            // Обновление времени гонки
            if (IsRaceActive && !IsFinished)
            {
                RaceTime += deltaTime;
            }
        }

        private bool IsWithinBounds(SKPoint position)
        {
            float halfW = Player.Width / 2;
            float halfH = Player.Height / 2;
            return position.X - halfW >= 0 && position.X + halfW <= WorldWidth &&
                   position.Y - halfH >= 0 && position.Y + halfH <= WorldHeight;
        }
    }
}
