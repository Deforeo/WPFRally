using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFRally.Models
{
    public class AutomaticTransmissionController
    {
        public CarOutput output;
        private float shiftTimer;
        private const float MinTimeBetweenShifts = 0.2f;

        public AutomaticTransmissionController()
        {
            output = new CarOutput();
            output.gear = 1;
            output.clutch = 1f;
        }

        public void Update(CarInput input, float engineRPM, float speed,
            float upshiftRpm, float downshiftRpm, float idleRpm,
            int maxGear, float deltaTime)
        {
            // Копируем прямой ввод
            output.throttle = input.throttle;
            output.brake = input.brake;
            output.handbrake = input.handbrake;

            // Простейшая логика выбора передачи по оборотам
            if (output.gear >= 1 && shiftTimer <= 0)
            {
                // Повышаем при достижении красной зоны
                if (engineRPM > upshiftRpm && output.gear < maxGear)
                {
                    output.gear++;
                    shiftTimer = MinTimeBetweenShifts;
                }
                // Понижаем при падении оборотов ниже холостых
                else if (engineRPM < downshiftRpm && output.gear > 1)
                {
                    output.gear--;
                    shiftTimer = MinTimeBetweenShifts;
                }
            }

            // Переключение на заднюю при торможении до полной остановки
            if (output.gear == 0 || output.gear >= 1)
            {
                if (input.brake > 0.1f && speed < 0.5f && shiftTimer <= 0)
                {
                    output.gear = -1;
                    shiftTimer = MinTimeBetweenShifts;
                }
            }
            else if (output.gear == -1)
            {
                if (input.throttle > 0.1f && shiftTimer <= 0)
                {
                    output.gear = 1;
                    shiftTimer = MinTimeBetweenShifts;
                }
            }

            // Выжимаем сцепление при ручнике (опционально)
            if (output.handbrake > 0.5f)
                output.clutch = 0f;
            else
                output.clutch = 1f;

            shiftTimer = Math.Max(0, shiftTimer - deltaTime);
        }
    }
}
