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

        public float WorldWidth { get; set; } = 1800f;
        public float WorldHeight { get; set; } = 1400f;

        public bool IsRaceActive { get; set; }
        public bool IsFinished { get; set; }
        public float RaceTime { get; set; }
        public event Action<float> OnRaceFinished;

        public GameWorld()
        {
            Player = new SimpleVehicle();
            Player.Position = new Vector2(400, 300);
            Player.Angle = 0;
            Player.Velocity = new Vector2(0, 0);

            Obstacles = new List<Obstacle>();
            Obstacles.Add(new Obstacle(500, 280, 60, 40));
            Obstacles.Add(new Obstacle(200, 150, 80, 30));
            Obstacles.Add(new Obstacle(700, 500, 50, 50));
            Obstacles.Add(new Obstacle(100, 550, 120, 40));

            TriggerZones = new List<TriggerZone>();
            TriggerZones.Add(new TriggerZone(370, 270, 60, 60, "Start"));
            TriggerZones.Add(new TriggerZone(1200, 900, 80, 80, "Finish"));
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
            bool collided = false;
            foreach (var obs in Obstacles)
            {
                if (obs.CollidesWith(vehicleRect))
                {
                    collided = true;
                    break;
                }
            }
            if (collided)
            {
                Player.Position = oldPos;
                Player.Velocity = new Vector2(0, 0);
            }

            // Триггеры
            foreach (var zone in TriggerZones)
            {
                if (!zone.IsActive) continue;
                if (zone.Type == "Start" && zone.Intersects(vehicleRect))
                {
                    zone.IsActive = false;
                    IsRaceActive = true;
                    RaceTime = 0f;
                }
                if (zone.Type == "Finish" && IsRaceActive && zone.Intersects(vehicleRect))
                {
                    zone.IsActive = false;
                    IsFinished = true;
                    IsRaceActive = false;
                    OnRaceFinished?.Invoke(RaceTime);
                }
            }

            if (IsRaceActive && !IsFinished)
                RaceTime += deltaTime;
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