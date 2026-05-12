using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFRally.Views
{
    public partial class PauseOverlay : UserControl
    {
        public event Action Resume;
        public event Action Restart;
        public event Action ExitToMenu;

        public PauseOverlay()
        {
            InitializeComponent();
        }

        private void Resume_Click(object sender, System.Windows.RoutedEventArgs e) => Resume?.Invoke();
        private void Restart_Click(object sender, System.Windows.RoutedEventArgs e) => Restart?.Invoke();
        private void Exit_Click(object sender, System.Windows.RoutedEventArgs e) => ExitToMenu?.Invoke();
    }
}
