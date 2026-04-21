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
    /// <summary>
    /// Логика взаимодействия для RecordNameDialog.xaml
    /// </summary>
    public partial class RecordNameDialog : Window
    {
        public string Initials { get; private set; }
        public bool IsSaved { get; private set; }

        public RecordNameDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            InitialsBox.Text = "AAA";
            IsSaved = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string text = InitialsBox.Text.Trim().ToUpper();
            if (text.Length < 3) text = text.PadRight(3, 'A');
            if (text.Length > 3) text = text.Substring(0, 3);
            Initials = text;
            IsSaved = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsSaved = false;
            Close();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            string filtered = "";
            foreach (char c in textBox.Text)
            {
                if (char.IsLetter(c))
                    filtered += char.ToUpper(c);
            }
            if (filtered.Length > 3) filtered = filtered.Substring(0, 3);
            if (textBox.Text != filtered)
            {
                int caret = textBox.CaretIndex;
                textBox.Text = filtered;
                textBox.CaretIndex = Math.Min(caret, filtered.Length);
            }
        }
    }
}
