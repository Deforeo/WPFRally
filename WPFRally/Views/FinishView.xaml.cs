using System;
using System.Windows.Controls;
using WPFRally.ViewModels;

namespace WPFRally.Views
{
    public partial class FinishView : UserControl
    {
        public event Action<string> RecordSaved;

        public FinishView()
        {
            InitializeComponent();
        }

        public void ShowRecordDialog()
        {
            var dialog = new RecordNameDialog();
            OverlayContainer.Content = dialog;
            OverlayContainer.Visibility = System.Windows.Visibility.Visible;

            dialog.OnSave += (initials) =>
            {
                OverlayContainer.Visibility = System.Windows.Visibility.Collapsed;
                RecordSaved?.Invoke(initials);
            };
            dialog.OnCancel += () =>
            {
                OverlayContainer.Visibility = System.Windows.Visibility.Collapsed;
                RecordSaved?.Invoke(null); // null означает отмена
            };
        }
    }
}