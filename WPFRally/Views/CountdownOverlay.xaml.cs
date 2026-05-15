using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WPFRally.Views
{
    public partial class CountdownOverlay : UserControl
    {
        private DispatcherTimer _timer;
        private int _count = 3;
        public event Action CountdownFinished;

        public CountdownOverlay()
        {
            InitializeComponent();
            StartCountdown();
        }

        private void StartCountdown()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTick;
            _timer.Start();
            AnimateText("3");
        }

        private void OnTick(object sender, EventArgs e)
        {
            _count--;
            if (_count > 0)
            {
                AnimateText(_count.ToString());
            }
            else if (_count == 0)
            {
                AnimateText("GO!");
                _timer.Stop();
                // Через 1 секунду после GO! завершаем
                var finishTimer = new DispatcherTimer();
                finishTimer.Interval = TimeSpan.FromSeconds(1);
                finishTimer.Tick += (s, ev) =>
                {
                    finishTimer.Stop();
                    CountdownFinished?.Invoke();
                };
                finishTimer.Start();
            }
        }

        private void AnimateText(string text)
        {
            CountdownText.Text = text;
            var sb = new Storyboard();
            var opacityAnim = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.8));
            var scaleXAnim = new DoubleAnimation(1, 1.5, TimeSpan.FromSeconds(0.8));
            var scaleYAnim = new DoubleAnimation(1, 1.5, TimeSpan.FromSeconds(0.8));
            Storyboard.SetTarget(opacityAnim, CountdownText);
            Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTarget(scaleXAnim, CountdownText);
            Storyboard.SetTargetProperty(scaleXAnim, new PropertyPath("(TextBlock.RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(scaleYAnim, CountdownText);
            Storyboard.SetTargetProperty(scaleYAnim, new PropertyPath("(TextBlock.RenderTransform).(ScaleTransform.ScaleY)"));
            sb.Children.Add(opacityAnim);
            sb.Children.Add(scaleXAnim);
            sb.Children.Add(scaleYAnim);
            sb.Begin();
        }
    }
}