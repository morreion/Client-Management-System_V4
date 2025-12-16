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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client_Management_System_V4.View
{
    /// <summary>
    /// Interaction logic for Clients.xaml
    /// </summary>
    public partial class Clients : UserControl
    {
        public Clients()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink link && link.DataContext is Client_Management_System_V4.Models.Client client && !string.IsNullOrEmpty(client.Email))
            {
                Utilities.LinkHelper.OpenEmail(client.Email);
            }
        }

        private void OpenEmail_Click(object sender, RoutedEventArgs e)
        {
            // The Button's Tag or DataContext might contain the email, 
            // but here we might just read from the TextBox binding or pass it via Tag.
            if (sender is Button btn && btn.Tag is string email)
            {
                Utilities.LinkHelper.OpenEmail(email);
            }
        }
    }
}
