using System.Windows.Controls;

namespace Client_Management_System_V4.View
{
    /// <summary>
    /// Interaction logic for Supplements.xaml
    /// </summary>
    public partial class Supplements : UserControl
    {
        public Supplements()
        {
            InitializeComponent();
        }

        private void OpenEmail_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string email)
            {
                Utilities.LinkHelper.OpenEmail(email);
            }
        }

        private void OpenWebsite_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string url)
            {
                Utilities.LinkHelper.OpenLink(url);
            }
        }
    }
}
