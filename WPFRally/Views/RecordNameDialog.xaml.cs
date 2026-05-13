using System;
using System.Windows;
using System.Windows.Controls;

namespace WPFRally.Views
{
    public partial class RecordNameDialog : UserControl
    {
        public event Action<string> OnSave;
        public event Action OnCancel;

        public RecordNameDialog()
        {
            InitializeComponent();
            InitialsBox.Text = "AAA";
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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string text = InitialsBox.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(text)) text = "AAA";
            if (text.Length < 3) text = text.PadRight(3, 'A');
            if (text.Length > 3) text = text.Substring(0, 3);
            OnSave?.Invoke(text);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            OnCancel?.Invoke();
        }
    }
}