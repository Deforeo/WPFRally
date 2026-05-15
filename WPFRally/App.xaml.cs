using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WPFRally.Infrastructure;

namespace WPFRally
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SpriteManager.LoadAll();
            CreateAnimatedGradient();
            CreateListBoxGradient();
        }

        private void CreateAnimatedGradient()
        {
            // Создаём градиент с 4 цветами (сверху вниз)
            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)  // диагональное направление
            };

            // Добавляем точки градиента (от светлого к тёмному)
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#453F94"), 0.0)); // Soft Violet (светлый)
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#302B63"), 0.33)); // Royal Purple
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#24243E"), 0.66)); // Abyss Blue
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#0F0C29"), 1.0));  // Classic Indigo (тёмный)

            // === 1. Анимация вращения градиента ===
            var rotateTransform = new RotateTransform(0);
            gradient.Transform = rotateTransform;
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(20),
                RepeatBehavior = RepeatBehavior.Forever
            });

            // === 2. Анимация изменения цвета для каждой точки ===
            // Цвет 1 (#453F94 → #6A5ACD)
            var colorAnim1 = new ColorAnimation
            {
                From = (Color)ColorConverter.ConvertFromString("#453F94"),
                To = (Color)ColorConverter.ConvertFromString("#6A5ACD"), // более яркий фиолет
                Duration = TimeSpan.FromSeconds(8),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            gradient.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, colorAnim1);

            // Цвет 2 (#302B63 → #5B4B8A)
            var colorAnim2 = new ColorAnimation
            {
                From = (Color)ColorConverter.ConvertFromString("#302B63"),
                To = (Color)ColorConverter.ConvertFromString("#5B4B8A"),
                Duration = TimeSpan.FromSeconds(10),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            gradient.GradientStops[1].BeginAnimation(GradientStop.ColorProperty, colorAnim2);

            // Цвет 3 (#24243E → #3E3A6B)
            var colorAnim3 = new ColorAnimation
            {
                From = (Color)ColorConverter.ConvertFromString("#24243E"),
                To = (Color)ColorConverter.ConvertFromString("#3E3A6B"),
                Duration = TimeSpan.FromSeconds(12),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            gradient.GradientStops[2].BeginAnimation(GradientStop.ColorProperty, colorAnim3);

            // Цвет 4 (#0F0C29 → #2A235A)
            var colorAnim4 = new ColorAnimation
            {
                From = (Color)ColorConverter.ConvertFromString("#0F0C29"),
                To = (Color)ColorConverter.ConvertFromString("#2A235A"),
                Duration = TimeSpan.FromSeconds(14),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            gradient.GradientStops[3].BeginAnimation(GradientStop.ColorProperty, colorAnim4);

            // Добавляем градиент в ресурсы приложения как DynamicResource
            Resources.Add("LiveGradient", gradient);
        }

        private void CreateListBoxGradient()
        {
            // Осветлённая копия основного градиента (идеальное сочетание)
            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#7A6BC8"), 0.0)); // светлый акцент
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#584F9B"), 0.33)); // переход
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3E3A6B"), 0.66)); // тёмный
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#2A235A"), 1.0));  // самый тёмный

            // Вращение (анимация)
            var rotateTransform = new RotateTransform(0);
            gradient.Transform = rotateTransform;
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(15),
                RepeatBehavior = RepeatBehavior.Forever
            });

            // Плавное изменение цветов (умеренное)
            var animDur = TimeSpan.FromSeconds(10);
            gradient.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, new ColorAnimation
            {
                From = (Color)ColorConverter.ConvertFromString("#7A6BC8"),
                To = (Color)ColorConverter.ConvertFromString("#8E80D6"),
                Duration = animDur,
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            });
            gradient.GradientStops[1].BeginAnimation(GradientStop.ColorProperty, new ColorAnimation
            {
                From = (Color)ColorConverter.ConvertFromString("#584F9B"),
                To = (Color)ColorConverter.ConvertFromString("#6A60B0"),
                Duration = animDur,
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            });
            gradient.GradientStops[2].BeginAnimation(GradientStop.ColorProperty, new ColorAnimation
            {
                From = (Color)ColorConverter.ConvertFromString("#3E3A6B"),
                To = (Color)ColorConverter.ConvertFromString("#4E4A80"),
                Duration = animDur,
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            });
            gradient.GradientStops[3].BeginAnimation(GradientStop.ColorProperty, new ColorAnimation
            {
                From = (Color)ColorConverter.ConvertFromString("#2A235A"),
                To = (Color)ColorConverter.ConvertFromString("#3A3070"),
                Duration = animDur,
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            });

            Resources.Add("ListBoxGradient", gradient);
        }
    }
}